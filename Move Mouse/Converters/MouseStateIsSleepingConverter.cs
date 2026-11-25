using ellabi.ViewModels;
using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ellabi.Converters
{
    public class MouseStateIsSleepingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MouseWindowViewModel.MouseState state)
            {
                return state == MouseWindowViewModel.MouseState.Sleeping;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
