using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mutagen.Bethesda
{
    /// <summary>
    /// An interface for classes that contain FormKeys and can enumerate them.
    /// </summary>
    public interface IFormLinkContainer : IFormLinkContainerGetter
    {
        /// <summary>
        /// Swaps out all links to point to new FormKeys
        /// </summary>
        void RemapLinks(IReadOnlyDictionary<FormKey, FormKey> mapping);
    }

    /// <summary>
    /// An interface for classes that contain FormKeys and can enumerate them.
    /// </summary>
    public interface IFormLinkContainerGetter
    {
        /// <summary>
        /// Enumerable of all contained FormKeys
        /// </summary>
        IEnumerable<FormLinkInformation> ContainedFormLinks { get; }
    }

    // ToDo
    // Refactor to use record concepts

    public class FormLinkInformation : IEquatable<FormLinkInformation>
    {
        public FormKey FormKey { get; }
        public Type Type { get; }

        public FormLinkInformation(FormKey formKey, Type type)
        {
            FormKey = formKey;
            Type = type;
        }

        public override bool Equals(object? obj)
        {
            return (obj is FormLinkInformation rhs) && Equals(rhs);
        }

        public bool Equals(FormLinkInformation? other)
        {
            if (other == null) return false;
            if (this.FormKey != other.FormKey) return false;
            if (this.Type != other.Type) return false;
            return true;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(FormKey);
            hash.Add(Type);
            return hash.ToHashCode();
        }

        public override string ToString()
        {
            return $"({Type}) => {FormKey}";
        }

        public static FormLinkInformation Factory<TMajorGetter>(IFormLink<TMajorGetter> link)
            where TMajorGetter : IMajorRecordCommonGetter
        {
            return new FormLinkInformation(link.FormKey, typeof(TMajorGetter));
        }

        public static FormLinkInformation Factory<TMajorGetter>(IFormLinkNullable<TMajorGetter> link)
            where TMajorGetter : IMajorRecordCommonGetter
        {
            return new FormLinkInformation(link.FormKey, typeof(TMajorGetter));
        }

        public static FormLinkInformation Factory(IMajorRecordCommonGetter majorRec)
        {
            return new FormLinkInformation(majorRec.FormKey, majorRec.Registration.GetterType);
        }

        public static FormLinkInformation Factory(FormLinkInformation rhs) => rhs;
    }
}
