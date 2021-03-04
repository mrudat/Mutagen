using Noggog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Data;

namespace Mutagen.Bethesda.Core.Persistance
{
    /// <summary>
    /// A FormKey allocator that utilizes a folder of text files to persist and sync.
    /// 
    /// This class is made thread safe by locking internally on the Mod object.
    /// </summary>
    public class TextFileFormKeyAllocator : BasePersistentFormKeyAllocator, IPersistentFormKeyAllocator
    {
        private readonly Dictionary<string, FormKey> _cache = new();

        public TextFileFormKeyAllocator(IMod mod, string stateFolder) : base(mod, stateFolder) {
            Load();
        }

        protected override FormKey _GetNextFormKey(string editorID)
        {
            lock (Mod)
            {
                if (_cache.TryGetValue(editorID, out var id))
                    return id;

                var formKey = GetNextFormKey();

                _cache.Add(editorID, formKey);

                return formKey;
            }
        }

        private void Load()
        {
            var stateFilePath = Path.Combine(stateFolder, $"{Mod.ModKey.FileName}.txt");

            if (!File.Exists(stateFilePath)) return;

            var streamReader = new StreamReader(stateFilePath);

            var idLine = streamReader.ReadLine();
            if (!uint.TryParse(idLine, out var nextID))
            {
                throw new ArgumentException($"Unconvertable next ID line: {idLine}");
            }
            Mod.NextFormID = nextID;
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
                _cache.Add(edidStr, formKey);
            }
        }

        public override void Save()
        {
            var stateFile = Path.Combine(stateFolder, $"{Mod.ModKey.FileName}.txt");
            var tempFile = Path.Combine(stateFolder, $"{Mod.ModKey.FileName}.tmp");
            try {
                {
                    using var streamWriter = new StreamWriter(tempFile);
                    streamWriter.WriteLine(Mod.NextFormID.ToString());
                    foreach (var pair in _cache)
                    {
                        streamWriter.WriteLine(pair.Key);
                        streamWriter.WriteLine(pair.Value);
                    }
                }
                File.Move(tempFile, stateFile, true);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        public override void Export(IPersistentFormKeyAllocator newAllocator)
        {
            newAllocator.Import(_cache);
        }

        public override void Import(IEnumerable<KeyValuePair<string, FormKey>> oldData)
        {
            foreach (var (editorID, formKey) in oldData)
            {
                if (formKey.ModKey != Mod.ModKey)
                    throw new ArgumentException($"Attempted to import formKey from foreign mod {formKey.ModKey}");
                _cache.Add(editorID, formKey);
            }
        }
    }
}
