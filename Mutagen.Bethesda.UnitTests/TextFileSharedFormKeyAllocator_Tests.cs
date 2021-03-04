using Mutagen.Bethesda.Core.Persistance;
using Mutagen.Bethesda.Oblivion;
using Noggog.Utility;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.IO;
using System.Linq;
using Xunit;

namespace Mutagen.Bethesda.UnitTests
{
    public class TextFileSharedFormKeyAllocator_Tests : ISharedFormKeyAllocator_Tests<TextFileSharedFormKeyAllocator>
    {
        protected override TextFileSharedFormKeyAllocator CreateFormKeyAllocator(IMod mod) => new(mod, tempFolder.Value.Dir.Path);

        protected override TextFileSharedFormKeyAllocator CreateFormKeyAllocator(IMod mod, string patcherName) => new(mod, tempFolder.Value.Dir.Path, patcherName);


        protected override void DisposeFormKeyAllocator(TextFileSharedFormKeyAllocator allocator)
        {
            if (tempFolder.IsValueCreated)
                tempFolder.Value.Dispose();
        }

        [Fact]
        public void StaticExport()
        {
            using var folder = tempFolder.Value;
            var mod = new OblivionMod(Utility.PluginModKey);
            var file = Path.Combine(folder.Dir.Path, $"{Utility.PluginModKey.FileName}.txt");

            var allocator = CreateFormKeyAllocator(mod);
            allocator.Import(new (string, FormKey)[]
                {
                    (Utility.Edid1, Utility.Form1),
                    (Utility.Edid2, Utility.Form2),
                });
            allocator.Save();
            var lines = File.ReadAllLines(file);
            Assert.Equal(
                new string[]
                {
                    Utility.Edid1,
                    Utility.Form1.ToString(),
                    Utility.Edid2,
                    Utility.Form2.ToString(),
                },
                lines);
        }

        [Fact]
        public void TypicalImport()
        {
            using var folder = tempFolder.Value;
            var mod = new OblivionMod(Utility.PluginModKey);
            var file = Path.Combine(folder.Dir.Path, $"{Utility.PluginModKey.FileName}.txt");

            File.WriteAllLines(
                file,
                new string[]
                {
                    Utility.Edid1,
                    Utility.Form1.ToString(),
                    Utility.Edid2,
                    Utility.Form2.ToString(),
                });
            var allocator = CreateFormKeyAllocator(mod);
            var formID = allocator.GetNextFormKey(Utility.Edid1);
            Assert.Equal(Utility.PluginModKey, formID.ModKey);
            Assert.Equal(formID, Utility.Form1);
            formID = allocator.GetNextFormKey(Utility.Edid2);
            Assert.Equal(formID, Utility.Form2);
        }

        [Fact]
        public void FailedImportTruncatedFile()
        {
            using var folder = tempFolder.Value;
            var mod = new OblivionMod(Utility.PluginModKey);
            var file = Path.Combine(folder.Dir.Path, $"{Utility.PluginModKey.FileName}.txt");

            File.WriteAllLines(
                file,
                new string[]
                {
                    Utility.Edid1,
                    Utility.Form1.ToString(),
                    Utility.Edid2,
                    //Utility.Form2.ToString(),
                });
            Assert.Throws<ArgumentException>(() => CreateFormKeyAllocator(mod));
        }

        [Fact]
        public void FailedImportDuplicateFormKey()
        {
            using var folder = tempFolder.Value;
            var mod = new OblivionMod(Utility.PluginModKey);
            var file = Path.Combine(folder.Dir.Path, $"{Utility.PluginModKey.FileName}.txt");
            File.WriteAllLines(
                file,
                new string[]
                {
                    Utility.Edid1,
                    Utility.Form1.ToString(),
                    Utility.Edid2,
                    Utility.Form1.ToString(),
                });
            Assert.Throws<ArgumentException>(() => CreateFormKeyAllocator(mod));
        }

        [Fact]
        public void FailedImportDuplicateEditorID()
        {
            using var folder = tempFolder.Value;
            var mod = new OblivionMod(Utility.PluginModKey);
            var file = Path.Combine(folder.Dir.Path, $"{Utility.PluginModKey.FileName}.txt");
            File.WriteAllLines(
                file,
                new string[]
                {
                    Utility.Edid1,
                    Utility.Form1.ToString(),
                    Utility.Edid1,
                    Utility.Form2.ToString(),
                });
            Assert.Throws<ArgumentException>(() => CreateFormKeyAllocator(mod));
        }

        [Fact]
        public void TypicalReimport()
        {
            using var folder = tempFolder.Value;
            var mod = new OblivionMod(Utility.PluginModKey);
            var file = Path.Combine(folder.Dir.Path, $"{Utility.PluginModKey.FileName}.txt");

            {
                var allocator = CreateFormKeyAllocator(mod);
                var list = new (string, FormKey)[]
                {
                    (Utility.Edid1, Utility.Form1),
                    (Utility.Edid2, Utility.Form2),
                };
                allocator.Import(list);
                allocator.Save();
            }

            {
                var allocator = CreateFormKeyAllocator(mod);
                var formID = allocator.GetNextFormKey();
                Assert.Equal(Utility.PluginModKey, formID.ModKey);
                formID = allocator.GetNextFormKey(Utility.Edid1);
                Assert.Equal(formID, Utility.Form1);
                formID = allocator.GetNextFormKey(Utility.Edid2);
                Assert.Equal(formID, Utility.Form2);
            }

        }
    }
}
