using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ellabi.Views;
using System;
using System.IO;
using System.Linq;

namespace ellabi
{
    public partial class App : Application
    {
        private FileStream _instanceLockStream;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                try
                {
                    StaticCode.CreateLog();

                    if (!TryAcquireSingleInstanceLock())
                    {
                        desktop.Shutdown();
                        return;
                    }

                    var args = desktop.Args;
                    if (StaticCode.DownloadSource == StaticCode.MoveMouseSource.GitHub && args != null)
                    {
                        var workingDirectoryArg = args.FirstOrDefault(a =>
                            a.StartsWith("/WorkingDirectory:", StringComparison.CurrentCultureIgnoreCase) ||
                            a.StartsWith("--working-directory=", StringComparison.CurrentCultureIgnoreCase));

                        if (workingDirectoryArg != null)
                        {
                            int sepIdx = workingDirectoryArg.IndexOf(':') >= 0
                                ? workingDirectoryArg.IndexOf(':')
                                : workingDirectoryArg.IndexOf('=');

                            if (sepIdx >= 0)
                            {
                                string alternateWorkingDirectory = workingDirectoryArg.Substring(sepIdx + 1);
                                if (Directory.Exists(alternateWorkingDirectory))
                                {
                                    StaticCode.WorkingDirectory = alternateWorkingDirectory;
                                }
                            }
                        }
                    }

                    Directory.CreateDirectory(StaticCode.WorkingDirectory);

                    var mainWindow = new MouseWindow();
                    desktop.MainWindow = mainWindow;
                    mainWindow.Show();

                    desktop.Exit += (_, _) => ReleaseSingleInstanceLock();
                }
                catch (Exception ex)
                {
                    StaticCode.Logger?.Here().Error(ex.Message);
                }
            }

            base.OnFrameworkInitializationCompleted();
        }

        private bool TryAcquireSingleInstanceLock()
        {
            try
            {
                Directory.CreateDirectory(StaticCode.TempDirectory);
                string lockPath = Path.Combine(StaticCode.TempDirectory, "movemouse.lock");
                _instanceLockStream = new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
                return true;
            }
        }

        private void ReleaseSingleInstanceLock()
        {
            try
            {
                _instanceLockStream?.Dispose();
                _instanceLockStream = null;
            }
            catch
            {
            }
        }
    }
}
