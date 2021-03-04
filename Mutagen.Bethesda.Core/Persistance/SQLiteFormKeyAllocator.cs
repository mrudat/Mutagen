using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Mutagen.Bethesda.Core.Persistance
{
    record SQLiteFormKeyAllocatorFormIDRecord(string EditorID, uint FormID, uint PatcherID);

    record SQLiteFormKeyAllocatorPatcherRecord(uint PatcherID, string PatcherName);

    internal class SQLiteFormKeyAllocatorDbContext : DbContext
    {
        private readonly string ConnectionString;

        internal readonly DbSet<SQLiteFormKeyAllocatorFormIDRecord> FormIDs;

        internal readonly DbSet<SQLiteFormKeyAllocatorPatcherRecord> Patchers;

        internal SQLiteFormKeyAllocatorDbContext(string dbPath)
        {
            ConnectionString = $"Data Source={dbPath}";

            this.Database.EnsureCreated();

            FormIDs = this.Set<SQLiteFormKeyAllocatorFormIDRecord>();
            Patchers = this.Set<SQLiteFormKeyAllocatorPatcherRecord>();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var formIdRecord = modelBuilder.Entity<SQLiteFormKeyAllocatorFormIDRecord>();
            var patcherRecord = modelBuilder.Entity<SQLiteFormKeyAllocatorPatcherRecord>();

            formIdRecord.ToTable("FormIDs");
            formIdRecord.HasKey(r => r.FormID);
            formIdRecord.HasAlternateKey(r => r.EditorID);

            patcherRecord.ToTable("Patchers");
            patcherRecord.HasKey(r => r.PatcherID);
            patcherRecord.HasAlternateKey(r => r.PatcherName);

            formIdRecord
                .HasOne<SQLiteFormKeyAllocatorPatcherRecord>()
                .WithMany()
                .HasForeignKey(r => r.PatcherID);
        }

    }

    public class SQLiteFormKeyAllocator : BaseSharedFormKeyAllocator, IDisposable, ISharedFormKeyAllocator
    {
        private readonly uint PatcherID;

        private readonly SQLiteFormKeyAllocatorDbContext Connection;

        private bool disposedValue;

        private readonly bool manyPatchers = true;

        public SQLiteFormKeyAllocator(IMod mod, string stateFolder) : this(mod, stateFolder, DefaultPatcherName)
        {
            manyPatchers = false;
        }

        public SQLiteFormKeyAllocator(IMod mod, string stateFolder, string patcherName) : base(mod, stateFolder)
        {
            Connection = new SQLiteFormKeyAllocatorDbContext(Path.Combine(stateFolder, $"{Mod.ModKey.FileName}.sqlite"));
            PatcherID = GetOrAddPatcherID(patcherName);
        }

        public override FormKey GetNextFormKey()
        {
            lock (Connection)
            {
                lock (Mod)
                {
                    var candidateFormID = Mod.NextFormID;
                    if (candidateFormID > 0xFFFFFF)
                        throw new OverflowException();

                    // TODO maybe track ranges of allocated formIDs to make this go faster?

                    // should be $"select 1 from FormIDs where FormID = {candidateFormID}"
                    while (Connection.FormIDs.Any(r => r.FormID == candidateFormID)) {
                        candidateFormID++;
                        if (candidateFormID > 0xFFFFFF)
                            throw new OverflowException();
                    }

                    Mod.NextFormID = candidateFormID + 1;

                    return new FormKey(Mod.ModKey, candidateFormID);
                }
            }
        }

        protected override FormKey _GetNextFormKey(string editorID)
        {
            lock (Connection)
            {
                // should be $"select EditorID, FormID, PatcherID from FormIDs where EditorID = {editorID}"
                var rec = Connection.FormIDs.AsNoTracking().FirstOrDefault(r => r.EditorID == editorID);

                if (rec is not null)
                {
                    if (manyPatchers)
                        if (rec.PatcherID != PatcherID)
                            throw new ConstraintException($"Attempted to allocate a unique FormKey for {editorID} when it was previously allocated by {GetPatcherName(rec.PatcherID)}");
                    return Mod.ModKey.MakeFormKey(rec.FormID);
                }

                var formKey = GetNextFormKey();

                Connection.FormIDs.Add(new(editorID, formKey.ID, PatcherID));
         
                return formKey;
            }
        }

        private uint GetOrAddPatcherID(string patcherName)
        {
            lock (Connection)
            {
                // TODO figure out how to get EF to generate this statement.
                Connection.Database.ExecuteSqlInterpolated($"insert or ignore into Patchers(PatcherName) values ({patcherName})");

                // should be $"select PatcherID from Patchers where PatcherName = {patcherName}"
                return Connection.Patchers.Where(r => r.PatcherName == patcherName).Select(r => r.PatcherID).Single();
            }
        }

        private uint? GetPatcherID(string patcherName)
        {
            lock (Connection)
            {
                // should be $"select PatcherID from Patchers where PatcherName = {patcherName}"
                return Connection.Patchers.Where(r => r.PatcherName == patcherName).Select(r => r.PatcherID).FirstOrDefault();
            }
        }

        private string GetPatcherName(uint patcherID)
        {
            lock (Connection)
            {
                // should be $"select PatcherName from Patchers where PatcherID = {patcherID}"
                return Connection.Patchers.Where(r => r.PatcherID == patcherID).Select(r => r.PatcherName).Single();
            }
        }

        public void ClearPatcher() => ClearPatcher(PatcherID);

        public void ClearPatcher(string patcherName)
        {
            uint? patcherID = GetPatcherID(patcherName);
            if (patcherID is null) return;
            ClearPatcher((uint)patcherID);
        }

        private void ClearPatcher(uint patcherID)
        {
            lock (Connection)
            {
                // should be $"delete from FormIDs where PatcherID = {patcherID}", but probably isn't.
                Connection.FormIDs.RemoveRange(Connection.FormIDs.Where(r => r.PatcherID == patcherID));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue) return;
            if (disposing)
                Connection?.Dispose();
            AllocatedEditorIDs?.Clear();
            disposedValue = true;
        }

        ~SQLiteFormKeyAllocator()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public override void Save()
        {
            lock (Connection)
            {
                Connection.SaveChanges();
            }
        }

        public override void Export(ISharedFormKeyAllocator newAllocator)
        {
            lock (Connection)
            {
                var data = (
                    from rec in Connection.FormIDs
                    join p in Connection.Patchers
                      on rec.PatcherID equals p.PatcherID
                    select new { rec.EditorID, p.PatcherName, FormKey = Mod.ModKey.MakeFormKey(rec.FormID) }
                )
                .ToList() // TODO make this less bad.
                .Select(r => (r.EditorID, r.PatcherName, r.FormKey))
                ;

                newAllocator.Import(data);
            }
        }

        public void Export(SQLiteFormKeyAllocator newAllocator)
        {
            lock (Connection)
            {
                var data = 
                    from rec in Connection.FormIDs
                    join p in Connection.Patchers
                      on rec.PatcherID equals p.PatcherID
                    group new KeyValuePair<string,uint>(rec.EditorID,rec.FormID) by p.PatcherName
                ;

                newAllocator.Import(data);
            }
        }

        private void Import(IQueryable<IGrouping<string, KeyValuePair<string, uint>>> oldData)
        {
            lock (Connection)
            {
                foreach (var patcherData in oldData)
                {
                    var patcherID = GetOrAddPatcherID(patcherData.Key);
                    foreach (var (editorID, formID) in patcherData)
                    {
                        Connection.FormIDs.Add(new(editorID, patcherID, formID));
                    }
                }
            }
        }

        public override void Import(IEnumerable<(string, string, FormKey)> oldData)
        {
            lock (Connection)
            {
                foreach (var (editorID, patcherName, formKey) in oldData)
                {
                    if (formKey.ModKey != Mod.ModKey)
                        throw new ArgumentException($"Attempted to import formKey from foreign mod {formKey.ModKey}");
                    var patcherID = GetOrAddPatcherID(patcherName);
                    Connection.FormIDs.Add(new(editorID, patcherID, formKey.ID));
                }
            }
        }
         
        public override void Export(IPersistentFormKeyAllocator newAllocator)
        {
            lock (Connection)
            {
                var data =
                from rec in Connection.FormIDs
                select new KeyValuePair<string, FormKey>(rec.EditorID, Mod.ModKey.MakeFormKey(rec.FormID));
                
                newAllocator.Import(data);
            }
        }

        public override void Import(string patcherName, IEnumerable<KeyValuePair<string, FormKey>> oldData)
        {
            lock (Connection)
            {
                var patcherID = GetOrAddPatcherID(patcherName);
                foreach (var (editorID, formKey) in oldData)
                {
                    if (formKey.ModKey != Mod.ModKey)
                        throw new ArgumentException($"Attempted to import formKey from foreign mod {formKey.ModKey}");
                    Connection.FormIDs.Add(new(editorID, patcherID, formKey.ID));
                }
            }
        }
    }
}
