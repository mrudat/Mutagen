using Noggog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Mutagen.Bethesda.Core.Persistance
{
    public abstract class BaseFormKeyAllocator : IFormKeyAllocator
    {
        protected readonly HashSet<string> AllocatedEditorIDs = new();

        /// <summary>
        /// Attached Mod that will be used as reference when allocating new keys
        /// </summary>
        public readonly IMod Mod;

        /// <summary>
        /// Constructs a new FormKeyAllocator that looks to a given Mod for the next key
        /// </summary>
        public BaseFormKeyAllocator(IMod mod)
        {
            Mod = mod;
        }

        /// <summary>
        /// Returns a FormKey with the next listed ID in the Mod's header.
        /// No checks will be done that this is truly a unique key; It is assumed the header is in a correct state.
        ///
        /// The Mod's header will be incremented to mark the allocated key as "used".
        /// </summary>
        /// <returns>The next FormKey from the Mod</returns>
        public virtual FormKey GetNextFormKey()
        {
            lock (Mod)
            {
                var candidateFormID = Mod.NextFormID;
                if (candidateFormID > 0xFFFFFF)
                    throw new OverflowException();

                this.Mod.NextFormID++;

                return new FormKey(this.Mod.ModKey, candidateFormID);
            }
        }

        public FormKey GetNextFormKey(string? editorID)
        {
            if (editorID == null) return GetNextFormKey();

            lock (AllocatedEditorIDs)
                if (!AllocatedEditorIDs.Add(editorID))
                    throw new ConstraintException($"Attempted to allocate a duplicate unique FormKey for {editorID}");

            return _GetNextFormKey(editorID);
        }

        protected virtual FormKey _GetNextFormKey(string editorID)
        {
            return GetNextFormKey();
        }
    }
}
