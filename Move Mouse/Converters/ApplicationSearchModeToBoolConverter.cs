using ellabi.Actions;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace ellabi.Converters
{
    public class ApplicationSearchModeToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if ((value != null) && value.GetType() == typeof(ActivateApplicationAction.SearchMode))
                {
                    var mode = (ActivateApplicationAction.SearchMode) value;
                    return mode.Equals(ActivateApplicationAction.SearchMode.Window);
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (Boolean.TryParse(value?.ToString(), out var b))
                {
                    return b ? ActivateApplicationAction.SearchMode.Window : ActivateApplicationAction.SearchMode.Process;
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }

            return ActivateApplicationAction.SearchMode.Process;
        }
    }
}