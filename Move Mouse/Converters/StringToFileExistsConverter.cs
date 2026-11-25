using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;

namespace ellabi.Converters
{
    public class StringToFileExistsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return !string.IsNullOrWhiteSpace(value?.ToString()) && File.Exists(value.ToString());
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
