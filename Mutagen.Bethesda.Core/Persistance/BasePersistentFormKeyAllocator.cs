using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Mutagen.Bethesda.Core.Persistance
{
    public abstract class BasePersistentFormKeyAllocator : BaseFormKeyAllocator, IPersistentFormKeyAllocator
    {
        protected readonly string stateFolder;

        public BasePersistentFormKeyAllocator(IMod mod, string stateFolder) : base(mod)
        {
            this.stateFolder = stateFolder;
        }

        public void Import(IMod oldMod)
        {
            Import(Export(oldMod));
        }

        protected static IEnumerable<KeyValuePair<string, FormKey>> Export(IMod oldMod)
        {
            return
                from majorRecord in oldMod.EnumerateMajorRecords()
                let editorID = majorRecord.EditorID
                where editorID is not null
                let formKey = majorRecord.FormKey
                where formKey.ModKey == oldMod.ModKey
                select new KeyValuePair<string, FormKey>(editorID, formKey);
        }

        public abstract void Save();

        public abstract void Export(IPersistentFormKeyAllocator newAllocator);

        public abstract void Import(IEnumerable<KeyValuePair<string, FormKey>> oldData);
    }
}
