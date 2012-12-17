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
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                Application.ThreadException += Application_ThreadException;
                Application.EnableVisualStyles();
                Application.DoEvents();
                Application.Run(new SystemTrayIcon());
            }
            catch (Exception ex)
            {
                CloseMoveMouseWithException(ex.Message);
            }

            //todo Simple and advanced mode
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            CloseMoveMouseWithException(e.Exception.Message);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            CloseMoveMouseWithException(e.ExceptionObject.ToString());
        }

        private static void CloseMoveMouseWithException(string exception)
        {
            MessageBox.Show(exception, "Move Mouse - Generic Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
    }
}
