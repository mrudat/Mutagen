using System.Collections.Generic;

namespace Mutagen.Bethesda.Core.Persistance
{

    /// <summary>
    /// An interface for something that can allocate new FormKeys when requested shared between multiple programs
    /// </summary>
    public interface ISharedFormKeyAllocator : IPersistentFormKeyAllocator
    {
        public void Export(ISharedFormKeyAllocator newAllocator);

        public void Import(IEnumerable<(string, string, FormKey)> oldData);
        public void Import(string patcherName, IEnumerable<KeyValuePair<string, FormKey>> oldData);
    }
}
