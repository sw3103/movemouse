using System;
using System.IO;
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
                        Directory.CreateDirectory(StaticCode.WorkingDirectory);
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls12;
                        StaticCode.CreateLog();
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