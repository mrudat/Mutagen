using System.Collections.Generic;

namespace Mutagen.Bethesda.Core.Persistance
{
    /// <summary>
    /// An interface for something that can allocate new FormKeys when requested that can persist allocated FormKeys to disk.
    /// </summary>
    public interface IPersistentFormKeyAllocator : IFormKeyAllocator
    {
        /// <summary>
        /// Saves the state.
        /// </summary>
        public void Save();

        public void Export(IPersistentFormKeyAllocator newAllocator);

        public void Import(IEnumerable<KeyValuePair<string, FormKey>> oldData);

        public void Import(IMod oldMod);
    }
}
