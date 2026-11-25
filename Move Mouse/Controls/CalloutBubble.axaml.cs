using Avalonia;
using Avalonia.Controls;

namespace ellabi.Controls
{
    public partial class CalloutBubble : UserControl
    {
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<CalloutBubble, string>(nameof(Text), string.Empty);

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        static CalloutBubble()
        {
            TextProperty.Changed.AddClassHandler<CalloutBubble>((c, e) =>
            {
                if (c.BubbleText != null)
                {
                    c.BubbleText.Text = e.NewValue as string ?? string.Empty;
                }
            });
        }

        public CalloutBubble()
        {
            InitializeComponent();
        }
    }
}
