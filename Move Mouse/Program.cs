using System;
using System.Diagnostics;
using System.Windows.Forms;
using Ellanet.Forms;

namespace Ellanet
{
    internal class Program
    {
        [STAThread]
        private static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.ThreadException += Application_ThreadException;

            try
            {
                if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length < 2)
                {
                    Application.EnableVisualStyles();
                    Application.DoEvents();
                    Application.Run(new SystemTrayIcon());
                }
            }
            catch (Exception ex)
            {
                CloseMoveMouseWithException(ex.Message);
            }
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

        //todo Show famous mice when double-clicking icon
        //todo Balloon notifications when blackout starts and stops
        //todo Balloon notifications when scheduling starts and stops
    }
}
