using Avalonia;
using Avalonia.Controls;

namespace ellabi.Controls
{
    public partial class ActionFooterControl : UserControl
    {
        public static readonly StyledProperty<bool> ShowTestButtonProperty =
            AvaloniaProperty.Register<ActionFooterControl, bool>(nameof(ShowTestButton), true);

        public bool ShowTestButton
        {
            get => GetValue(ShowTestButtonProperty);
            set => SetValue(ShowTestButtonProperty, value);
        }

        static ActionFooterControl()
        {
            ShowTestButtonProperty.Changed.AddClassHandler<ActionFooterControl>((c, e) =>
            {
                if (c.TestRow != null && e.NewValue is bool show)
                {
                    c.TestRow.IsVisible = show;
                }
            });
        }

        public ActionFooterControl()
        {
            InitializeComponent();
        }
    }
}
