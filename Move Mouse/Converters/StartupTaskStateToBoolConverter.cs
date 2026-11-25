using ellabi.Platform;
using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ellabi.Converters
{
    public class StartupTaskStateToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StartupTaskState state)
            {
                return state == StartupTaskState.Enabled;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (bool.TryParse(value?.ToString(), out var b))
            {
                return b ? StartupTaskState.Enabled : StartupTaskState.Disabled;
            }
            return StartupTaskState.Disabled;
        }
    }
}
