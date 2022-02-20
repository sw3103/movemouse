using ellabi.ViewModels;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace ellabi.Converters
{
    public class MouseStateIsSleepingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return (value != null) && Enum.TryParse(value.ToString(), true, out MouseWindowViewModel.MouseState state) && state.Equals(MouseWindowViewModel.MouseState.Sleeping);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}