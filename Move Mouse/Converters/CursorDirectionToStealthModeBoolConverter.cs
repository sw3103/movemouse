using ellabi.Actions;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace ellabi.Converters
{
    public class CursorDirectionToStealthModeBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if ((value != null) && value.GetType() == typeof(MoveMouseCursorAction.CursorDirection))
                {
                    var direction = (MoveMouseCursorAction.CursorDirection)value;
                    return direction.Equals(MoveMouseCursorAction.CursorDirection.None);
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
                    return b ? MoveMouseCursorAction.CursorDirection.None : MoveMouseCursorAction.CursorDirection.Square;
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }

            return MoveMouseCursorAction.CursorDirection.Square;
        }
    }
}