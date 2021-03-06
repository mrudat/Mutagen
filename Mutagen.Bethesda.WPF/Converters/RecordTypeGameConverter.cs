using Loqui;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Mutagen.Bethesda.WPF
{
    public class RecordTypeGameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Type type) return Binding.DoNothing;
            if (LoquiRegistration.TryGetRegister(type, out var registration))
            {
                return registration.ProtocolKey.Namespace;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
