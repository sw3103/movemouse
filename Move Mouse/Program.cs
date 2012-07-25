using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Ellanet
{
    class Program
    {
        static void Main(string[] args)
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
