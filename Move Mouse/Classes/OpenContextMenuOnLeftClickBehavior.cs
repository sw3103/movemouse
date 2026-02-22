using System.Windows;

namespace ellabi.Classes
{
    // https://stackoverflow.com/questions/59584206/wpf-contextmenu-loses-datacontext-if-it-is-displayed-using-a-left-click-event
    public class OpenContextMenuOnLeftClickBehavior : DependencyObject
    {
        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnabledProperty, value);
        }

        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled", typeof(bool), typeof(OpenContextMenuOnLeftClickBehavior), new FrameworkPropertyMetadata(false, IsEnabledProperty_Changed));

        private static void IsEnabledProperty_Changed(DependencyObject dpobj, DependencyPropertyChangedEventArgs args)
        {
            var f = dpobj as FrameworkElement;

            if (f != null)
            {
                bool newValue = (bool)args.NewValue;

                if (newValue)
                    f.PreviewMouseLeftButtonUp += Target_PreviewMouseLeftButtonUpEvent;
                else
                    f.PreviewMouseLeftButtonUp -= Target_PreviewMouseLeftButtonUpEvent;
            }
        }

        protected static void Target_PreviewMouseLeftButtonUpEvent(object sender, RoutedEventArgs e)
        {
            var f = sender as FrameworkElement;

            if (f?.ContextMenu != null)
            {
                f.ContextMenu.PlacementTarget = f;
                f.ContextMenu.IsOpen = true;
            }

            e.Handled = true;
        }
    }
}
