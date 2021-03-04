using Mutagen.Bethesda.Core.Persistance;
using System.Collections.Generic;

namespace Mutagen.Bethesda.Persistance
{

    /// <summary>
    /// A simple FormKey allocator that simply leverages a Mod's NextObjectID tracker to allocate.
    /// No safety checks or syncronization is provided.
    ///
    /// This class is thread safe.
    /// </summary>
    public class SimpleFormKeyAllocator : BaseFormKeyAllocator, IFormKeyAllocator
    {
        private readonly Dictionary<string, FormKey> _cache = new();

        public SimpleFormKeyAllocator(IMod mod) : base(mod) { }

        public bool TryRegister(string edid, FormKey formKey)
        {
            lock (_cache)
            {
                return _cache.TryAdd(edid, formKey);
            }
        }

        public void Register(string edid, FormKey formKey)
        {
            lock (_cache)
            {
                _cache.Add(edid, formKey);
            }
        }

        protected override FormKey _GetNextFormKey(string editorID)
        {
            lock (_cache)
            {
                if (_cache.TryGetValue(editorID, out var id)) return id;
            }
            return GetNextFormKey();
        }
    }
}
