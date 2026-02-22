using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace ellabi.Converters
{
    public class StringToFileExistsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return !String.IsNullOrWhiteSpace(value?.ToString()) && File.Exists(value.ToString());
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