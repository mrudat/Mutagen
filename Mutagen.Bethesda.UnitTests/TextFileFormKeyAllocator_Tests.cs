using Mutagen.Bethesda.Core.Persistance;
using Mutagen.Bethesda.Oblivion;
using Noggog.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Mutagen.Bethesda.UnitTests
{
    public class TextFileFormKeyAllocator_Tests : IPersistentFormKeyAllocator_Tests<TextFileFormKeyAllocator>
    {
        protected override TextFileFormKeyAllocator CreateFormKeyAllocator(IMod mod) => new(mod, tempFolder.Value.Dir.Path);

        protected override void DisposeFormKeyAllocator(TextFileFormKeyAllocator allocator) {
            if (tempFolder.IsValueCreated)
                tempFolder.Value.Dispose();
        }

        [Fact]
        public void StaticExport()
        {
            using var folder = tempFolder.Value;
            var mod = new OblivionMod(Utility.PluginModKey);

            ((IMod)mod).NextFormID = 123;

            var allocator = CreateFormKeyAllocator(mod);
            var formKey1 = allocator.GetNextFormKey(Utility.Edid1);
            var formKey2 = allocator.GetNextFormKey(Utility.Edid2);
            allocator.Save();

            var lines = File.ReadAllLines(Path.Combine(tempFolder.Value.Dir.Path, $"{mod.ModKey.FileName}.txt"));
            Assert.Equal(
                new string[]
                {
                    ((IMod)mod).NextFormID.ToString(),
                    Utility.Edid1,
                    formKey1.ToString(),
                    Utility.Edid2,
                    formKey2.ToString(),
                },
                lines);
        }

        [Fact]
        public void TypicalImport()
        {
            using var folder = tempFolder.Value;
            var mod = new OblivionMod(Utility.PluginModKey);
            var file = Path.Combine(folder.Dir.Path, $"{mod.ModKey.FileName}.txt");
            uint nextID = 123;
            File.WriteAllLines(
                file,
                new string[]
                {
                    nextID.ToString(),
                    Utility.Edid1,
                    Utility.Form1.ToString(),
                    Utility.Edid2,
                    Utility.Form2.ToString(),
                });
            var allocator = CreateFormKeyAllocator(mod);
            var formID = allocator.GetNextFormKey();
            Assert.Equal(nextID, formID.ID);
            Assert.Equal(Utility.PluginModKey, formID.ModKey);
            formID = allocator.GetNextFormKey(Utility.Edid1);
            Assert.Equal(formID, Utility.Form1);
            formID = allocator.GetNextFormKey(Utility.Edid2);
            Assert.Equal(formID, Utility.Form2);
        }

        [Fact]
        public void TypicalReimport()
        {
            using var folder = tempFolder.Value;
            var file = Path.Combine(folder.Dir.Path, $"{Utility.PluginModKey.FileName}.txt");
            uint nextID = 123;
            {
                var mod = new OblivionMod(Utility.PluginModKey);
                var allocator = CreateFormKeyAllocator(mod);
                var list = new KeyValuePair<string, FormKey>[]
                {
                    new KeyValuePair<string, FormKey>(Utility.Edid1, Utility.Form1),
                    new KeyValuePair<string, FormKey>(Utility.Edid2, Utility.Form2),
                };
                allocator.Import(list);
                allocator.Save();
            }

            {
                var mod = new OblivionMod(Utility.PluginModKey);
                var allocator = CreateFormKeyAllocator(mod);
                var formID = allocator.GetNextFormKey();
                Assert.Equal(nextID, formID.ID);
                Assert.Equal(Utility.PluginModKey, formID.ModKey);
                formID = allocator.GetNextFormKey(Utility.Edid1);
                Assert.Equal(formID, Utility.Form1);
                formID = allocator.GetNextFormKey(Utility.Edid2);
                Assert.Equal(formID, Utility.Form2);
            }
        }
    }
}
