using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;

namespace ellabi
{
    public partial class App
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                using (new Mutex(true, "f45b30b9-9e65-4d33-a2bc-d6ba6a7500bd", out var createdNew))
                {
                    if (createdNew)
                    {
                        StaticCode.CreateLog();

                        if ((StaticCode.DownloadSource == StaticCode.MoveMouseSource.GitHub) && (e.Args != null) && e.Args.Any(a => a.StartsWith("/WorkingDirectory:", StringComparison.CurrentCultureIgnoreCase)))
                        {
                            var workingDirectoryArg = e.Args.First(a => a.StartsWith("/WorkingDirectory:", StringComparison.CurrentCultureIgnoreCase));
                            var alternateWorkingDirectory = workingDirectoryArg.Substring(workingDirectoryArg.IndexOf(':') + 1);

                            if (Directory.Exists(alternateWorkingDirectory))
                            {
                                StaticCode.WorkingDirectory = workingDirectoryArg.Substring(workingDirectoryArg.IndexOf(':') + 1);
                            }
                        }

                        Directory.CreateDirectory(StaticCode.WorkingDirectory);
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls12;
                        StartupUri = new Uri("Views/MouseWindow.xaml", UriKind.Relative);
                    }
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            StaticCode.Logger?.Here().Error(e.Exception.Message);
        }
    }
}