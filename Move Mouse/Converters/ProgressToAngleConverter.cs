using Avalonia.Controls;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ellabi.Converters
{
    public class ProgressToAngleConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values != null && values.Count >= 2 &&
                    values[0] is double progress &&
                    values[1] is ProgressBar bar)
                {
                    double range = bar.Maximum - bar.Minimum;
                    if (range > 0)
                    {
                        return 359.999 * (progress / range);
                    }
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }

            return 0.0;
        }
    }
}
