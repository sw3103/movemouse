using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;

namespace ellabi.Controls
{
    public partial class TimeSpanUpDown : UserControl
    {
        public static readonly StyledProperty<TimeSpan> ValueProperty =
            AvaloniaProperty.Register<TimeSpanUpDown, TimeSpan>(
                nameof(Value),
                TimeSpan.Zero,
                defaultBindingMode: BindingMode.TwoWay);

        public static readonly StyledProperty<TimeSpan> MinimumProperty =
            AvaloniaProperty.Register<TimeSpanUpDown, TimeSpan>(nameof(Minimum), TimeSpan.Zero);

        public static readonly StyledProperty<TimeSpan> MaximumProperty =
            AvaloniaProperty.Register<TimeSpanUpDown, TimeSpan>(nameof(Maximum), new TimeSpan(23, 59, 59));

        public TimeSpan Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public TimeSpan Minimum
        {
            get => GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public TimeSpan Maximum
        {
            get => GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        static TimeSpanUpDown()
        {
            ValueProperty.Changed.AddClassHandler<TimeSpanUpDown>((ctrl, _) => ctrl.UpdateText());
        }

        public TimeSpanUpDown()
        {
            InitializeComponent();
            UpdateText();
        }

        private void UpdateText()
        {
            if (TimeTextBox != null)
            {
                var v = Value;
                TimeTextBox.Text = $"{v.Hours:D2}:{v.Minutes:D2}:{v.Seconds:D2}";
            }
        }

        private TimeSpan GetStep()
        {
            int caret = TimeTextBox?.CaretIndex ?? 4;
            if (caret <= 2) return TimeSpan.FromHours(1);
            if (caret <= 5) return TimeSpan.FromMinutes(1);
            return TimeSpan.FromSeconds(1);
        }

        private void Step(int sign)
        {
            var next = Value + TimeSpan.FromTicks(GetStep().Ticks * sign);
            if (next < Minimum) next = Minimum;
            if (next > Maximum) next = Maximum;
            Value = new TimeSpan(next.Hours, next.Minutes, next.Seconds);
            UpdateText();
        }

        private void UpButton_Click(object sender, RoutedEventArgs e) => Step(1);

        private void DownButton_Click(object sender, RoutedEventArgs e) => Step(-1);

        private void TimeTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                Step(1);
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                Step(-1);
                e.Handled = true;
            }
        }
    }
}
