using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mutagen.Bethesda
{
    public class RecordCollisionException : Exception
    {
        public readonly FormKey? FormKey;
        public readonly Type? GroupType;

        public RecordCollisionException(FormKey formKey, Type type)
        {
            GroupType = type;
            FormKey = formKey;
        }

        public override string ToString()
        {
            return $"{nameof(RecordCollisionException)}: Two records with the same FormKey {FormKey} existed in the same Group<{GroupType?.Name}>";
        }
    }
}
