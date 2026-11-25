using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ellabi.Converters
{
    public class BoolToIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b && b ? 0 : 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int i && i == 0;
        }
    }
}
