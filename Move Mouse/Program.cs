using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Ellanet.Forms;

namespace Ellanet
{
    class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            if (!Debugger.IsAttached)
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                Application.ThreadException += Application_ThreadException;
            }

            try
            {
                if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length < 2)
                {
                    if ((args != null) && (args.Length > 0))
                    {
                        foreach (string arg in args)
                        {
                            if (arg.StartsWith("/WorkingDirectory:", StringComparison.CurrentCultureIgnoreCase))
                            {
                                string alternateWd = arg.Substring(18);

                                if (!String.IsNullOrEmpty(alternateWd) && Directory.Exists(alternateWd))
                                {
                                    StaticCode.WorkingDirectory = alternateWd;
                                }
                            }
                            else if (arg.StartsWith("/EnableToolWindowStyle", StringComparison.CurrentCultureIgnoreCase))
                            {
                                StaticCode.EnableToolWindowStyle = true;
                            }
                        }
                    }

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
    }
}
