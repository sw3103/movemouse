using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Threading.Tasks;

namespace ellabi.Views
{
    public partial class MessageDialog : Window
    {
        public bool Result { get; private set; }

        public MessageDialog()
        {
            InitializeComponent();
        }

        public static async Task<bool> ShowYesNoAsync(Window owner, string title, string message)
        {
            var dlg = new MessageDialog();
            dlg.Title = title;
            dlg.TitleText.Text = title;
            dlg.MessageText.Text = message;
            if (owner != null)
            {
                await dlg.ShowDialog(owner);
            }
            else
            {
                dlg.Show();
            }
            return dlg.Result;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            Close();
        }
    }
}
