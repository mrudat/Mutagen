using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mutagen.Bethesda
{
    public static class FormLinkMixIn
    {
        /// <summary>
        /// Mix in to facilitate converting to FormLinks from interfaces where implicit operators aren't
        /// available.  This particular extension function shouldn't need an explicitly defined generic
        /// when calling it.  It only works with non-abstract class types, though.
        /// </summary>
        public static FormLink<TGetter> AsLink<TGetter>(this IMapsToGetter<TGetter> rec)
            where TGetter : class, IMajorRecordCommonGetter
        {
            return new FormLink<TGetter>(rec.FormKey);
        }

        /// <summary>
        /// Mix in to facilitate converting to FormLinks from interfaces where implicit operators aren't
        /// available.  This particular extension function needs an explicitly defined generic
        /// when calling it, as it doesn't know what link type it should convert to automatically.
        /// </summary>
        public static FormLink<TGetter> AsLink<TGetter>(this IMajorRecordCommonGetter rec)
            where TGetter : class, IMajorRecordCommonGetter
        {
            return new FormLink<TGetter>(rec.FormKey);
        }

        public static bool Contains<TGetter>(this IReadOnlyCollection<IFormLink<TGetter>> coll, TGetter record)
            where TGetter : class, IMajorRecordCommonGetter
        {
            return coll.Contains(new FormLink<TGetter>(record.FormKey));
        }

        public static bool Contains<TGetter>(this IReadOnlyCollection<FormLink<TGetter>> coll, TGetter record)
            where TGetter : class, IMajorRecordCommonGetter
        {
            return coll.Contains(new FormLink<TGetter>(record.FormKey));
        }

        public static bool Contains<TGetter>(this IReadOnlyCollection<FormLinkNullable<TGetter>> coll, TGetter record)
            where TGetter : class, IMajorRecordCommonGetter
        {
            return coll.Contains(new FormLinkNullable<TGetter>(record.FormKey));
        }
    }
}
