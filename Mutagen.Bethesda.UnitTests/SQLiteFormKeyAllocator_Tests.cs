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
    public class SQLiteFormKeyAllocator_Tests : ISharedFormKeyAllocator_Tests<SQLiteFormKeyAllocator>
    {
        protected override SQLiteFormKeyAllocator CreateFormKeyAllocator(IMod mod)
        {
            return new SQLiteFormKeyAllocator(mod, tempFolder.Value.Dir.Path);
        }

        protected override SQLiteFormKeyAllocator CreateFormKeyAllocator(IMod mod, string patcherName)
        {
            return new SQLiteFormKeyAllocator(mod, tempFolder.Value.Dir.Path, patcherName);
        }

        protected override void DisposeFormKeyAllocator(SQLiteFormKeyAllocator allocator)
        {
            allocator.Dispose();
        }
    }
}
