using System;
using System.Globalization;
using System.Windows.Data;
using Windows.ApplicationModel;

namespace ellabi.Converters
{
    public class StartupTaskStateToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if ((value != null) && value.GetType() == typeof(StartupTaskState))
                {
                    var state = (StartupTaskState) value;
                    return state.Equals(StartupTaskState.Enabled);
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
                    return b ? StartupTaskState.Enabled : StartupTaskState.Disabled;
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }

            return StartupTaskState.Disabled;
        }
    }
}