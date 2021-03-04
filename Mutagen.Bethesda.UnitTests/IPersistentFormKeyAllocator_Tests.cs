using Mutagen.Bethesda.Core.Persistance;
using Mutagen.Bethesda.Oblivion;
using Noggog.Utility;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using Xunit;

namespace Mutagen.Bethesda.UnitTests
{
    public abstract class IPersistentFormKeyAllocator_Tests<TFormKeyAllocator> : IFormKeyAllocator_Tests<TFormKeyAllocator>
        where TFormKeyAllocator : IPersistentFormKeyAllocator
    {
        protected Lazy<TempFolder> tempFolder = new(() => TempFolder.FactoryByPath(path: Utility.TempFolderPath));

        public void CanSaveAndLoad()
        {
            var mod = new OblivionMod(Utility.PluginModKey);
            FormKey formKey;
            { 
                var allocator = CreateFormKeyAllocator(mod);

                formKey = allocator.GetNextFormKey(Utility.Edid1);

                allocator.Save();
                DisposeFormKeyAllocator(allocator);
            }

            {
                var allocator = CreateFormKeyAllocator(mod);

                var loadedFormKey = allocator.GetNextFormKey(Utility.Edid1);
                Assert.Equal(formKey, loadedFormKey);

                DisposeFormKeyAllocator(allocator);
            }
        }

        [Fact]
        public void OutOfOrderAllocationReturnsSameIdentifiers()
        {
            uint formID1, formID2;
            {
                var mod = new OblivionMod(Utility.PluginModKey);
                var allocator = CreateFormKeyAllocator(mod);
                var formKey1 = allocator.GetNextFormKey(Utility.Edid1);
                formID1 = formKey1.ID;

                allocator.GetNextFormKey();

                var formKey2 = allocator.GetNextFormKey(Utility.Edid2);
                formID2 = formKey2.ID;

                allocator.Save();
                DisposeFormKeyAllocator(allocator);
            }

            {
                var mod = new OblivionMod(Utility.PluginModKey);
                var allocator = CreateFormKeyAllocator(mod);

                var formKey2 = allocator.GetNextFormKey(Utility.Edid2);
                Assert.Equal(formID2, formKey2.ID);

                allocator.GetNextFormKey();

                var formKey1 = allocator.GetNextFormKey(Utility.Edid1);
                Assert.Equal(formID1, formKey1.ID);

                DisposeFormKeyAllocator(allocator);
            }
        }

        [Fact]
        public void ParallelAllocation()
        {
            var input = Enumerable.Range(1, 100).Select(i => (i, i.ToString())).ToList();
            var output1 = new ConcurrentDictionary<int, uint>();

            var mod = new OblivionMod(Utility.PluginModKey);
            {
                var allocator = CreateFormKeyAllocator(mod);

                void apply((int i, string s) x)
                {
                    // "Randomly" allocate some non-unique FormIDs.
                    if (x.i % 3 == 0)
                        allocator.GetNextFormKey();
                    else
                    {
                        var key = allocator.GetNextFormKey(x.s);
                        output1.TryAdd(x.i, key.ID);
                    }
                }

                input.AsParallel().ForAll(apply);

                allocator.Save();

                DisposeFormKeyAllocator(allocator);
            }

            {
                var allocator = CreateFormKeyAllocator(mod);

                void check((int i, string s) x)
                {
                    if (x.i % 3 != 0)
                    {
                        var key = allocator.GetNextFormKey(x.s);
                        if (!output1.TryGetValue(x.i, out var expectedID))
                            throw new Exception("?");
                        Assert.Equal(expectedID, key.ID);
                    }
                }

                input.AsParallel().ForAll(check);

                DisposeFormKeyAllocator(allocator);
            }
        }
    }
}
