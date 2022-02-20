using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Ellanet.Forms;
using Microsoft.VisualBasic;

namespace Ellanet
{
    internal class Program
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
                bool createdNew;

                using (new Mutex(true, "d45b30b9-9e65-4d33-a2bc-d6ba6a7500bd", out createdNew))
                {
                    if (createdNew)
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
                    else
                    {
                        foreach (var process in Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName))
                        {
                            if ((process.Id != Process.GetCurrentProcess().Id) && !String.IsNullOrEmpty(process.MainWindowTitle))
                            {
                                Interaction.AppActivate(process.MainWindowTitle);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CloseMoveMouseWithException(ex.Message);
            }
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
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
