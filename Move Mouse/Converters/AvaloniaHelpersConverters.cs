using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using ellabi.Actions;
using ellabi.Platform;
using ellabi.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ellabi.Converters
{
    public class EnumEqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return false;
            return string.Equals(value.ToString(), parameter.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b && parameter != null && targetType.IsEnum)
            {
                return Enum.Parse(targetType, parameter.ToString());
            }
            return Avalonia.Data.BindingOperations.DoNothing;
        }
    }

    public class EnumNotEqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return true;
            return !string.Equals(value.ToString(), parameter.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class MouseStateBrushConverter : IValueConverter
    {
        private static readonly IBrush IdleBrush = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop(Color.Parse("#9BB9E3"), 0.0),
                new GradientStop(Color.Parse("#437CAF"), 1.0)
            }
        };

        private static readonly IBrush RunningBrush = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop(Colors.LawnGreen, 0.0),
                new GradientStop(Colors.Green, 1.0)
            }
        };

        private static readonly IBrush ExecutingBrush = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop(Colors.OrangeRed, 0.0),
                new GradientStop(Colors.DarkRed, 1.0)
            }
        };

        private static readonly IBrush PausedBrush = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop(Color.Parse("#FFE93D"), 0.0),
                new GradientStop(Color.Parse("#FFC200"), 1.0)
            }
        };

        private static readonly IBrush SleepingBrush = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop(Colors.DarkSlateBlue, 0.0),
                new GradientStop(Colors.Purple, 1.0)
            }
        };

        private static readonly IBrush OnBatteryBrush = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop(Colors.Orange, 0.0),
                new GradientStop(Color.Parse("#C99926"), 1.0)
            }
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MouseWindowViewModel.MouseState state)
            {
                return state switch
                {
                    MouseWindowViewModel.MouseState.Running => RunningBrush,
                    MouseWindowViewModel.MouseState.Executing => ExecutingBrush,
                    MouseWindowViewModel.MouseState.Paused => PausedBrush,
                    MouseWindowViewModel.MouseState.Sleeping => SleepingBrush,
                    MouseWindowViewModel.MouseState.OnBattery => OnBatteryBrush,
                    _ => IdleBrush
                };
            }
            return IdleBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class ActionDisplayNameConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            string name = values != null && values.Count > 0 ? values[0] as string : null;
            if (!string.IsNullOrWhiteSpace(name)) return name;
            return parameter?.ToString() ?? "Action";
        }
    }

    public class ActionForegroundConverter : IValueConverter
    {
        private static readonly IBrush EnabledBrush = Brushes.White;
        private static readonly IBrush DisabledBrush = Brushes.Gray;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b && !b ? DisabledBrush : EnabledBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class BoolAndConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Count == 0) return false;
            foreach (var v in values)
            {
                if (v is not bool b || !b) return false;
            }
            return true;
        }
    }

    public class BoolAndNotSecondConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Count < 2) return false;
            return values[0] is bool a && a && values[1] is bool b && !b;
        }
    }

    public class NotBoolAndNotBoolConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Count < 2) return false;
            return values[0] is bool a && !a && values[1] is bool b && !b;
        }
    }

    public class IntervalPluralUnitConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Count >= 2 &&
                values[0] is int lower &&
                values[1] is bool random)
            {
                return (lower == 1 && !random) ? "second" : "seconds";
            }
            return "seconds";
        }
    }

    public class SingularPluralSuffixConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int count = value is int i ? i : 0;
            return count == 1 ? "second of inactivity" : "seconds of inactivity";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class AutoPauseLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b && b
                ? "Automatically pause when user activity detected"
                : "Automatically stop when user activity detected";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class DistanceLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b && b ? "Lower" : "Distance";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class SleepDurationLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b && b ? "Lower" : "Duration";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class MoveSpeedCustomEnabledConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Count >= 3 &&
                values[0] is MoveMouseCursorAction.CursorSpeed speed &&
                values[1] is bool isEnabled &&
                values[2] is MoveMouseCursorAction.CursorDirection direction)
            {
                return isEnabled && speed == MoveMouseCursorAction.CursorSpeed.Custom && direction != MoveMouseCursorAction.CursorDirection.None;
            }
            return false;
        }
    }

    public class MoveDirectionNotNoneEnabledConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Count >= 2 &&
                values[0] is bool isEnabled &&
                values[1] is MoveMouseCursorAction.CursorDirection direction)
            {
                return isEnabled && direction != MoveMouseCursorAction.CursorDirection.None;
            }
            return false;
        }
    }

    public class RepeatThrottleEnabledConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Count >= 3 &&
                values[0] is bool repeat &&
                values[1] is ActionBase.IntervalRepeatMode mode &&
                values[2] is bool isEnabled)
            {
                return isEnabled && repeat && mode == ActionBase.IntervalRepeatMode.Throttle;
            }
            return false;
        }
    }

    public class AdvancedScheduleInvalidBorderConverter : IMultiValueConverter
    {
        private static readonly IBrush InvalidBrush = Brushes.Red;
        private static readonly IBrush ValidBrush = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop(Color.Parse("#9BB9E3"), 0.0),
                new GradientStop(Color.Parse("#437CAF"), 1.0)
            }
        };

        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Count >= 2 &&
                values[0] is bool isValid &&
                values[1] is bool isEnabled &&
                !isValid && isEnabled)
            {
                return InvalidBrush;
            }
            return ValidBrush;
        }
    }

    public class TrackButtonTextConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Count >= 2 &&
                values[0] is bool tracking && tracking &&
                values[1] is TimeSpan remaining)
            {
                var clamped = remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
                return $"{clamped.Seconds}.{clamped.Milliseconds / 100}";
            }
            return "Track";
        }
    }

    public class DecimalToNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return 0m;
            return System.Convert.ToDecimal(value, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal d = value is decimal dec ? dec : 0m;
            if (targetType == typeof(int) || targetType == typeof(int?)) return (int)Math.Round(d);
            if (targetType == typeof(uint) || targetType == typeof(uint?)) return (uint)Math.Max(0, Math.Round(d));
            if (targetType == typeof(double) || targetType == typeof(double?)) return (double)d;
            return d;
        }
    }
}
