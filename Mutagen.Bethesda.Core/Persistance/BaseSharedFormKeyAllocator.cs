using System.Collections.Generic;

namespace Mutagen.Bethesda.Core.Persistance
{
    public abstract class BaseSharedFormKeyAllocator : BasePersistentFormKeyAllocator, ISharedFormKeyAllocator
    {
        public static readonly string DefaultPatcherName = "default";

        protected readonly string patcherName;

        public BaseSharedFormKeyAllocator(IMod mod, string stateFolder) : this(mod, stateFolder, DefaultPatcherName) { }

        public BaseSharedFormKeyAllocator(IMod mod, string stateFolder, string patcherName) : base(mod, stateFolder) {
            this.patcherName = patcherName;
        }

        public abstract void Export(ISharedFormKeyAllocator newAllocator);

        public abstract void Import(IEnumerable<(string, string, FormKey)> oldData);

        public override void Import(IEnumerable<KeyValuePair<string, FormKey>> oldData)
        {
            Import(DefaultPatcherName, oldData);
        }

        public abstract void Import(string patcherName, IEnumerable<KeyValuePair<string, FormKey>> oldData);
    }
}
