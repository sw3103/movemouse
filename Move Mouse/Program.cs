using System;
using System.Windows.Forms;

namespace Ellanet
{
    internal class Program
    {
        private static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.DoEvents();
                Application.Run(new SystemTrayIcon());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Generic Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            //todo Simple and advanced mode
        }
    }
}
