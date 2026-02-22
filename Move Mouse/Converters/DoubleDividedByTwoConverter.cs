using System;
using System.Diagnostics;
using System.Windows.Data;

namespace ellabi.Converters
{
    public class DoubleDividedByTwoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if ((value != null) && Double.TryParse(value.ToString(), out var d))
                {
                    return d / 2;
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }

            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}