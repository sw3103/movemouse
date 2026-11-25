using ellabi.Actions;
using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ellabi.Converters
{
    public class CursorDirectionToStealthModeBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MoveMouseCursorAction.CursorDirection direction)
            {
                return direction == MoveMouseCursorAction.CursorDirection.None;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (bool.TryParse(value?.ToString(), out var b))
            {
                return b ? MoveMouseCursorAction.CursorDirection.None : MoveMouseCursorAction.CursorDirection.Square;
            }
            return MoveMouseCursorAction.CursorDirection.Square;
        }
    }
}
