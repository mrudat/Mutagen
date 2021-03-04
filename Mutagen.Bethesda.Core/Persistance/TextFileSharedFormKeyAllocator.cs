using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Mutagen.Bethesda.Core.Persistance
{
    /// <summary>
    /// A FormKey allocator that utilizes a folder of text files to persist and sync.
    /// 
    /// This class is made thread safe by locking internally on the Mod object.
    /// </summary>
    public class TextFileSharedFormKeyAllocator : BaseSharedFormKeyAllocator, ISharedFormKeyAllocator
    {
        private readonly Dictionary<string, (string patcherName, FormKey formKey)> _cache = new();

        private readonly HashSet<uint> FormIDSet = new();

        public TextFileSharedFormKeyAllocator(IMod mod, string stateFolder) : this(mod, stateFolder, DefaultPatcherName) { }

        public TextFileSharedFormKeyAllocator(IMod mod, string stateFolder, string patcherName) : base(mod, stateFolder)
        {
            Load();
        }

        public override FormKey GetNextFormKey()
        {
            lock (Mod)
            {
                var candidateFormID = Mod.NextFormID;
                if (candidateFormID > 0xFFFFFF)
                    throw new OverflowException();

                while (FormIDSet.Contains(candidateFormID))
                {
                    candidateFormID++;
                    if (candidateFormID > 0xFFFFFF)
                        throw new OverflowException();
                }

                Mod.NextFormID = candidateFormID + 1;

                return new FormKey(Mod.ModKey, candidateFormID);
            }
        }

        protected override FormKey _GetNextFormKey(string editorID)
        {
            lock (_cache)
            {
                if (_cache.TryGetValue(editorID, out var rec))
                {
                    if (rec.patcherName != this.patcherName)
                        throw new ConstraintException($"Attempted to allocate a unique FormKey for {editorID} when it was previously allocated by {rec.patcherName}");
                    return rec.formKey;
                }

                var formKey = GetNextFormKey();

                _cache.Add(editorID, (patcherName, formKey));

                return formKey;
            }
        }

        public override void Save()
        {
            var stateDirectory = Path.Combine(stateFolder, $"{Mod.ModKey.FileName}.d");

            var data = this._cache
                .Where(p => p.Value.patcherName == patcherName)
                .Select(p => (p.Key, p.Value.formKey));

            var stateFile = Path.Combine(stateDirectory, $"{patcherName}.txt");
            var tempFile = Path.Combine(stateDirectory, $"{patcherName}.tmp");
            try
            {
                using var streamWriter = new StreamWriter(tempFile);
                foreach (var (Key, Value) in data)
                {
                    streamWriter.WriteLine(Key);
                    streamWriter.WriteLine(Value);
                }
                File.Move(tempFile, stateFile);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        private void Load()
        {
            var stateDirectory = Path.Combine(stateFolder, $"{Mod.ModKey.FileName}.d");
            if (!Directory.Exists(stateDirectory))
                return;

            foreach (var file in Directory.EnumerateFiles(stateDirectory, "*.txt"))
                LoadFile(file, Path.GetFileName(file)[0..^4]);
        }

        private void LoadFile(string filePath, string patcherName)
        {
            using var streamReader = new StreamReader(filePath);
            while (true)
            {
                var edidStr = streamReader.ReadLine();
                var formKeyStr = streamReader.ReadLine();
                if (edidStr == null) break;
                if (formKeyStr == null)
                {
                    throw new ArgumentException("Unexpected odd number of lines.");
                }
                var formKey = FormKey.Factory(formKeyStr);
                if (!FormIDSet.Add(formKey.ID))
                {
                    throw new ArgumentException("Duplicate formKey loaded from {filePath}.");
                }
                _cache.Add(edidStr, new(patcherName, formKey));
            }
        }

        public override void Export(IPersistentFormKeyAllocator newAllocator)
        {
            var data =
                from kpv in _cache
                select new KeyValuePair<string,FormKey>(kpv.Key, kpv.Value.formKey);
            newAllocator.Import(data);
        }

        public override void Import(string patcherName, IEnumerable<KeyValuePair<string, FormKey>> oldData)
        {
            foreach (var (editorID, formKey) in oldData)
            {
                if (formKey.ModKey != Mod.ModKey)
                    throw new ArgumentException($"Attempted to import formKey from foreign mod {formKey.ModKey}");
                _cache.Add(editorID, (patcherName, formKey));
                if (!FormIDSet.Add(formKey.ID))
                    throw new ArgumentException("Attempted to import duplicate formKey");
            }
        }

        public override void Export(ISharedFormKeyAllocator newAllocator)
        {
            var data =
                from kpv in _cache
                select (kpv.Key, kpv.Value.patcherName, kpv.Value.formKey);
            newAllocator.Import(data);
        }

        internal void Import((string, FormKey)[] oldData)
        {
            foreach (var (editorID, formKey) in oldData)
            {
                if (formKey.ModKey != Mod.ModKey)
                    throw new ArgumentException($"Attempted to import formKey from foreign mod {formKey.ModKey}");
                _cache.Add(editorID, (DefaultPatcherName, formKey));
                if (!FormIDSet.Add(formKey.ID))
                    throw new ArgumentException("Attempted to import duplicate formKey");
            }
        }

        public override void Import(IEnumerable<(string, string, FormKey)> oldData)
        {
            foreach (var (editorID, patcherName, formKey) in oldData)
            {
                if (formKey.ModKey != Mod.ModKey)
                    throw new ArgumentException($"Attempted to import formKey from foreign mod {formKey.ModKey}");
                _cache.Add(editorID, (patcherName, formKey));
                if(!FormIDSet.Add(formKey.ID))
                    throw new ArgumentException("Attempted to import duplicate formKey");
            }
        }


    }
}
