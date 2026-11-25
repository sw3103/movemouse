using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ellabi.Converters
{
    public class VirtualKeyToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int vk && StaticCode.VirtualKeys.Value.TryGetValue(vk, out var info))
            {
                return info.Key;
            }
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
