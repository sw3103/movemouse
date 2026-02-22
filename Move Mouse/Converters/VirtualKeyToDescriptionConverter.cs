using System;
using System.Globalization;
using System.Windows.Data;

namespace ellabi.Converters
{
    public class VirtualKeyToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return StaticCode.VirtualKeys.Value.ContainsKey((int)value) ? StaticCode.VirtualKeys.Value[(int)value].Value : "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
