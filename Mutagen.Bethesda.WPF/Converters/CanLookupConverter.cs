using Noggog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Mutagen.Bethesda.WPF
{
    public class CanLookupConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 3) return Binding.DoNothing;
            if (values[0] is not FormKey formKey) return Binding.DoNothing;
            if (values[1] is not ILinkCache linkCache) return Binding.DoNothing;
            bool compareTo = true;
            if (parameter is bool p)
            {
                compareTo = p;
            }
            else if (parameter is string str && str.Equals("FALSE", StringComparison.OrdinalIgnoreCase))
            {
                compareTo = false;
            }
            if (values[2] is Type type)
            {
                if (linkCache.TryResolveIdentifier(formKey, type, out var _))
                {
                    return compareTo;
                }
            }
            else if (values[2] is IEnumerable<Type> types)
            {
                if (linkCache.TryResolveIdentifier(formKey, types, out var _))
                {
                    return compareTo;
                }
            }
            else
            {
                return Binding.DoNothing;
            }
            return !compareTo;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
