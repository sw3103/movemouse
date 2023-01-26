using ellabi.Schedules;
using ellabi.Wrappers;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Storage;

namespace ellabi
{
    public static class StaticCode
    {
        public delegate void ScheduleArrivedHandler(ScheduleBase.ScheduleAction action);
        //public delegate void ThemeUpdatedHandler(Theme theme);
        public delegate void UpdateAvailablityChangedHandler(bool updateAvailable);

        public static event ScheduleArrivedHandler ScheduleArrived;
        //public static event ThemeUpdatedHandler ThemeUpdated;
        public static event UpdateAvailablityChangedHandler UpdateAvailablityChanged;

        public const string PayPalUrl = "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=QZTWHD9CRW5XN";
        public const string HomePageUrl = "http://www.movemouse.co.uk";
        public const string HelpPageUrl = "https://github.com/sw3103/movemouse/wiki";
        public const string TwitterUrl = "https://twitter.com/movemouse";
        public const string GitHubUrl = "https://github.com/sw3103/movemouse";
        public const string CronHelpUrl = "http://www.quartz-scheduler.org/documentation/quartz-2.3.0/tutorials/crontrigger.html";
        public const string TroubleshootingUrl = "https://github.com/sw3103/movemouse/wiki/Troubleshooting";
        //public const string CronHelpUrl = "https://crontab.guru/examples.html";
        //public const string ThemesXmlUrl = "https://raw.githubusercontent.com/sw3103/movemouse/master/Themes/Themes.xml";
        //public const string ThemesXmlUrl = "C:\\Users\\steve\\source\\repos\\movemouse\\Themes\\Themes_Test.xml";
        //public const string ThemesXmlUrl = "https://raw.githubusercontent.com/sw3103/movemouse/master/Themes/Themes_Test.xml";
        public const string UpdateXmlUrl = "https://raw.githubusercontent.com/sw3103/movemouse/master/Update_4x.xml";
        public const string MailAddress = "contact@movemouse.co.uk";
        public const string RunRegistryValueName = "Move Mouse";

        public static string WorkingDirectory = Path.Combine(Environment.ExpandEnvironmentVariables("%AppData%"), @"Ellanet\Move Mouse");
        public static string TempDirectory = Path.Combine(Environment.ExpandEnvironmentVariables("%Temp%"), @"Ellanet\Move Mouse");
        public static MoveMouseSource DownloadSource = MoveMouseSource.GitHub;

        public static string UpdateUrl;
        public static string ContactMailToAddress = $"mailto:{MailAddress}?subject=Move%20Mouse%20Feedback";
        public static ILogger Logger;

        private static string _logPath;
        private static LoggingLevelSwitch _loggingLevelSwitch = new LoggingLevelSwitch();
     
        public enum MoveMouseSource
        {
            MicrosoftStore,
            GitHub
        }

        public static string SettingsXmlPath => Path.Combine(WorkingDirectory, "Settings.xml");

        public static string LogPath => _logPath;

        public static void CreateLog()
        {
            try
            {
                _loggingLevelSwitch.MinimumLevel = (LogEventLevel)1 + (int)LogEventLevel.Fatal;
                _logPath = Path.Combine(StaticCode.DownloadSource == MoveMouseSource.MicrosoftStore ? ApplicationData.Current.LocalFolder.Path : TempDirectory, "Move Mouse.log");

                if (File.Exists(_logPath))
                {
                    try
                    {
                        File.Delete(_logPath);
                    }
                    catch (Exception)
                    {
                    }
                }

                var logConfiguration = new LoggerConfiguration()
                                        .MinimumLevel.ControlledBy(_loggingLevelSwitch)
                                        .WriteTo.File(_logPath,
                                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff}\t[{Level:u3}]\t{MemberName}\t{Message}{NewLine}{Exception}");
                Logger = logConfiguration.CreateLogger();
            }
            catch (Exception ex)
            {
                Logger?.Here().Error(ex.Message);
            }
        }

        public static void EnableLog(LogEventLevel minimumLevel)
        {
            try
            {
                _loggingLevelSwitch.MinimumLevel = minimumLevel;
                Logger?.Here().Debug(LogPath);
            }
            catch (Exception ex)
            {
                Logger?.Here().Error(ex.Message);
            }
        }

        public static void DisableLog()
        {
            Logger?.Here().Debug(String.Empty);

            try
            {
                _loggingLevelSwitch.MinimumLevel = (LogEventLevel)1 + (int)LogEventLevel.Fatal;
            }
            catch (Exception ex)
            {
                Logger?.Here().Error(ex.Message);
            }
        }

        public static TimeSpan GetLastInputTime()
        {
            int idleTime = 0;

            try
            {
                var lastInputInfo = new NativeMethods.LASTINPUTINFO();
                lastInputInfo.cbSize = Marshal.SizeOf(lastInputInfo);
                lastInputInfo.dwTime = 0;

                if (NativeMethods.GetLastInputInfo(ref lastInputInfo))
                {
                    int lastInputTick = lastInputInfo.dwTime;
                    idleTime = Environment.TickCount - lastInputTick;
                }
            }
            catch (Exception ex)
            {
                Logger?.Here().Error(ex.Message);
            }

            return TimeSpan.FromMilliseconds(idleTime);
        }

        public static void OnScheduleArrived(ScheduleBase.ScheduleAction action)
        {
            Logger?.Here().Debug(action.ToString());
            ScheduleArrived?.Invoke(action);
        }

        //public static void OnThemeUpdated(Theme theme)
        //{
        //    ThemeUpdated?.Invoke(theme);
        //}

        public static void OnUpdateAvailablityChanged(bool updateAvailable)
        {
            UpdateAvailablityChanged?.Invoke(updateAvailable);
        }
    }
}

public static class LoggerExtensions
{
    public static ILogger Here(this ILogger logger,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        return logger
            .ForContext("MemberName", memberName)
            .ForContext("FilePath", sourceFilePath)
            .ForContext("LineNumber", sourceLineNumber);
    }
}