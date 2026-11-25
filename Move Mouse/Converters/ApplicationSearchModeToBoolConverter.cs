using ellabi.Actions;
using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ellabi.Converters
{
    public class ApplicationSearchModeToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ActivateApplicationAction.SearchMode mode)
            {
                return mode == ActivateApplicationAction.SearchMode.Window;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (bool.TryParse(value?.ToString(), out var b))
            {
                return b ? ActivateApplicationAction.SearchMode.Window : ActivateApplicationAction.SearchMode.Process;
            }
            return ActivateApplicationAction.SearchMode.Process;
        }
    }
}
