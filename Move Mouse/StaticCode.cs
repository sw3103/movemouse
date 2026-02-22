using ellabi.Schedules;
using ellabi.Wrappers;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public delegate void RefreshSchedulesHandler();

        public static event ScheduleArrivedHandler ScheduleArrived;
        //public static event ThemeUpdatedHandler ThemeUpdated;
        public static event UpdateAvailablityChangedHandler UpdateAvailablityChanged;
        public static event RefreshSchedulesHandler RefreshSchedules;

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
        public static Lazy<Dictionary<int, KeyValuePair<string, string>>> VirtualKeys = new Lazy<Dictionary<int, KeyValuePair<string, string>>>(GetVirtualKeys);
       
        private static string _logPath;
        private static LoggingLevelSwitch _loggingLevelSwitch = new LoggingLevelSwitch();
     
        public enum MoveMouseSource
        {
            MicrosoftStore,
            GitHub
        }

        private static Dictionary<int, KeyValuePair<string, string>> GetVirtualKeys()
        {
            Debug.WriteLine("Loading Virtual Keys...");
            return new Dictionary<int, KeyValuePair<string, string>>
            {
                //[0x1] = new KeyValuePair<string, string>("LBUTTON", "Left mouse button"),
                //[0x2] = new KeyValuePair<string, string>("RBUTTON", "Right mouse button"),
                [0x3] = new KeyValuePair<string, string>("CANCEL", "Control-break processing"),
                [0x4] = new KeyValuePair<string, string>("MBUTTON", "Middle mouse button"),
                [0x5] = new KeyValuePair<string, string>("XBUTTON1", "X1 mouse button"),
                [0x6] = new KeyValuePair<string, string>("XBUTTON2", "X2 mouse button"),
                [0x8] = new KeyValuePair<string, string>("BACK", "Backspace key"),
                [0x9] = new KeyValuePair<string, string>("TAB", "Tab key"),
                [0xC] = new KeyValuePair<string, string>("CLEAR", "Clear key"),
                [0xD] = new KeyValuePair<string, string>("RETURN", "Enter key"),
                [0x10] = new KeyValuePair<string, string>("SHIFT", "Shift key"),
                [0x11] = new KeyValuePair<string, string>("CONTROL", "Ctrl key"),
                [0x12] = new KeyValuePair<string, string>("MENU", "Alt key"),
                [0x13] = new KeyValuePair<string, string>("PAUSE", "Pause key"),
                [0x14] = new KeyValuePair<string, string>("CAPITAL", "Caps lock key"),
                [0x15] = new KeyValuePair<string, string>("KANA", "IME Kana mode"),
                [0x15] = new KeyValuePair<string, string>("HANGUL", "IME Hangul mode"),
                [0x16] = new KeyValuePair<string, string>("IME_ON", "IME On"),
                [0x17] = new KeyValuePair<string, string>("JUNJA", "IME Junja mode"),
                [0x18] = new KeyValuePair<string, string>("FINAL", "IME final mode"),
                [0x19] = new KeyValuePair<string, string>("HANJA", "IME Hanja mode"),
                [0x19] = new KeyValuePair<string, string>("KANJI", "IME Kanji mode"),
                [0x1A] = new KeyValuePair<string, string>("IME_OFF", "IME Off"),
                [0x1B] = new KeyValuePair<string, string>("ESCAPE", "Esc key"),
                [0x1C] = new KeyValuePair<string, string>("CONVERT", "IME convert"),
                [0x1D] = new KeyValuePair<string, string>("NONCONVERT", "IME nonconvert"),
                [0x1E] = new KeyValuePair<string, string>("ACCEPT", "IME accept"),
                [0x1F] = new KeyValuePair<string, string>("MODECHANGE", "IME mode change request"),
                [0x20] = new KeyValuePair<string, string>("SPACE", "Spacebar key"),
                [0x21] = new KeyValuePair<string, string>("PRIOR", "Page up key"),
                [0x22] = new KeyValuePair<string, string>("NEXT", "Page down key"),
                [0x23] = new KeyValuePair<string, string>("END", "End key"),
                [0x24] = new KeyValuePair<string, string>("HOME", "Home key"),
                [0x25] = new KeyValuePair<string, string>("LEFT", "Left arrow key"),
                [0x26] = new KeyValuePair<string, string>("UP", "Up arrow key"),
                [0x27] = new KeyValuePair<string, string>("RIGHT", "Right arrow key"),
                [0x28] = new KeyValuePair<string, string>("DOWN", "Down arrow key"),
                [0x29] = new KeyValuePair<string, string>("SELECT", "Select key"),
                [0x2A] = new KeyValuePair<string, string>("PRINT", "Print key"),
                [0x2B] = new KeyValuePair<string, string>("EXECUTE", "Execute key"),
                [0x2C] = new KeyValuePair<string, string>("SNAPSHOT", "Print screen key"),
                [0x2D] = new KeyValuePair<string, string>("INSERT", "Insert key"),
                [0x2E] = new KeyValuePair<string, string>("DELETE", "Delete key"),
                [0x2F] = new KeyValuePair<string, string>("HELP", "Help key"),
                [0x30] = new KeyValuePair<string, string>("0", "0 key"),
                [0x31] = new KeyValuePair<string, string>("1", "1 key"),
                [0x32] = new KeyValuePair<string, string>("2", "2 key"),
                [0x33] = new KeyValuePair<string, string>("3", "3 key"),
                [0x34] = new KeyValuePair<string, string>("4", "4 key"),
                [0x35] = new KeyValuePair<string, string>("5", "5 key"),
                [0x36] = new KeyValuePair<string, string>("6", "6 key"),
                [0x37] = new KeyValuePair<string, string>("7", "7 key"),
                [0x38] = new KeyValuePair<string, string>("8", "8 key"),
                [0x39] = new KeyValuePair<string, string>("9", "9 key"),
                [0x41] = new KeyValuePair<string, string>("A", "A key"),
                [0x42] = new KeyValuePair<string, string>("B", "B key"),
                [0x43] = new KeyValuePair<string, string>("C", "C key"),
                [0x44] = new KeyValuePair<string, string>("D", "D key"),
                [0x45] = new KeyValuePair<string, string>("E", "E key"),
                [0x46] = new KeyValuePair<string, string>("F", "F key"),
                [0x47] = new KeyValuePair<string, string>("G", "G key"),
                [0x48] = new KeyValuePair<string, string>("H", "H key"),
                [0x49] = new KeyValuePair<string, string>("I", "I key"),
                [0x4A] = new KeyValuePair<string, string>("J", "J key"),
                [0x4B] = new KeyValuePair<string, string>("K", "K key"),
                [0x4C] = new KeyValuePair<string, string>("L", "L key"),
                [0x4D] = new KeyValuePair<string, string>("M", "M key"),
                [0x4E] = new KeyValuePair<string, string>("N", "N key"),
                [0x4F] = new KeyValuePair<string, string>("O", "O key"),
                [0x50] = new KeyValuePair<string, string>("P", "P key"),
                [0x51] = new KeyValuePair<string, string>("Q", "Q key"),
                [0x52] = new KeyValuePair<string, string>("R", "R key"),
                [0x53] = new KeyValuePair<string, string>("S", "S key"),
                [0x54] = new KeyValuePair<string, string>("T", "T key"),
                [0x55] = new KeyValuePair<string, string>("U", "U key"),
                [0x56] = new KeyValuePair<string, string>("V", "V key"),
                [0x57] = new KeyValuePair<string, string>("W", "W key"),
                [0x58] = new KeyValuePair<string, string>("X", "X key"),
                [0x59] = new KeyValuePair<string, string>("Y", "Y key"),
                [0x5A] = new KeyValuePair<string, string>("Z", "Z key"),
                [0x5B] = new KeyValuePair<string, string>("LWIN", "Left Windows logo key"),
                [0x5C] = new KeyValuePair<string, string>("RWIN", "Right Windows logo key"),
                [0x5D] = new KeyValuePair<string, string>("APPS", "Application key"),
                [0x5F] = new KeyValuePair<string, string>("SLEEP", "Computer Sleep key"),
                [0x60] = new KeyValuePair<string, string>("NUMPAD0", "Numeric keypad 0 key"),
                [0x61] = new KeyValuePair<string, string>("NUMPAD1", "Numeric keypad 1 key"),
                [0x62] = new KeyValuePair<string, string>("NUMPAD2", "Numeric keypad 2 key"),
                [0x63] = new KeyValuePair<string, string>("NUMPAD3", "Numeric keypad 3 key"),
                [0x64] = new KeyValuePair<string, string>("NUMPAD4", "Numeric keypad 4 key"),
                [0x65] = new KeyValuePair<string, string>("NUMPAD5", "Numeric keypad 5 key"),
                [0x66] = new KeyValuePair<string, string>("NUMPAD6", "Numeric keypad 6 key"),
                [0x67] = new KeyValuePair<string, string>("NUMPAD7", "Numeric keypad 7 key"),
                [0x68] = new KeyValuePair<string, string>("NUMPAD8", "Numeric keypad 8 key"),
                [0x69] = new KeyValuePair<string, string>("NUMPAD9", "Numeric keypad 9 key"),
                [0x6A] = new KeyValuePair<string, string>("MULTIPLY", "Multiply key"),
                [0x6B] = new KeyValuePair<string, string>("ADD", "Add key"),
                [0x6C] = new KeyValuePair<string, string>("SEPARATOR", "Separator key"),
                [0x6D] = new KeyValuePair<string, string>("SUBTRACT", "Subtract key"),
                [0x6E] = new KeyValuePair<string, string>("DECIMAL", "Decimal key"),
                [0x6F] = new KeyValuePair<string, string>("DIVIDE", "Divide key"),
                [0x70] = new KeyValuePair<string, string>("F1", "F1 key"),
                [0x71] = new KeyValuePair<string, string>("F2", "F2 key"),
                [0x72] = new KeyValuePair<string, string>("F3", "F3 key"),
                [0x73] = new KeyValuePair<string, string>("F4", "F4 key"),
                [0x74] = new KeyValuePair<string, string>("F5", "F5 key"),
                [0x75] = new KeyValuePair<string, string>("F6", "F6 key"),
                [0x76] = new KeyValuePair<string, string>("F7", "F7 key"),
                [0x77] = new KeyValuePair<string, string>("F8", "F8 key"),
                [0x78] = new KeyValuePair<string, string>("F9", "F9 key"),
                [0x79] = new KeyValuePair<string, string>("F10", "F10 key"),
                [0x7A] = new KeyValuePair<string, string>("F11", "F11 key"),
                [0x7B] = new KeyValuePair<string, string>("F12", "F12 key"),
                [0x7C] = new KeyValuePair<string, string>("F13", "F13 key"),
                [0x7D] = new KeyValuePair<string, string>("F14", "F14 key"),
                [0x7E] = new KeyValuePair<string, string>("F15", "F15 key"),
                [0x7F] = new KeyValuePair<string, string>("F16", "F16 key"),
                [0x80] = new KeyValuePair<string, string>("F17", "F17 key"),
                [0x81] = new KeyValuePair<string, string>("F18", "F18 key"),
                [0x82] = new KeyValuePair<string, string>("F19", "F19 key"),
                [0x83] = new KeyValuePair<string, string>("F20", "F20 key"),
                [0x84] = new KeyValuePair<string, string>("F21", "F21 key"),
                [0x85] = new KeyValuePair<string, string>("F22", "F22 key"),
                [0x86] = new KeyValuePair<string, string>("F23", "F23 key"),
                [0x87] = new KeyValuePair<string, string>("F24", "F24 key"),
                [0x90] = new KeyValuePair<string, string>("NUMLOCK", "Num lock key"),
                [0x91] = new KeyValuePair<string, string>("SCROLL", "Scroll lock key"),
                [0xA0] = new KeyValuePair<string, string>("LSHIFT", "Left Shift key"),
                [0xA1] = new KeyValuePair<string, string>("RSHIFT", "Right Shift key"),
                [0xA2] = new KeyValuePair<string, string>("LCONTROL", "Left Ctrl key"),
                [0xA3] = new KeyValuePair<string, string>("RCONTROL", "Right Ctrl key"),
                [0xA4] = new KeyValuePair<string, string>("LMENU", "Left Alt key"),
                [0xA5] = new KeyValuePair<string, string>("RMENU", "Right Alt key"),
                [0xA6] = new KeyValuePair<string, string>("BROWSER_BACK", "Browser Back key"),
                [0xA7] = new KeyValuePair<string, string>("BROWSER_FORWARD", "Browser Forward key"),
                [0xA8] = new KeyValuePair<string, string>("BROWSER_REFRESH", "Browser Refresh key"),
                [0xA9] = new KeyValuePair<string, string>("BROWSER_STOP", "Browser Stop key"),
                [0xAA] = new KeyValuePair<string, string>("BROWSER_SEARCH", "Browser Search key"),
                [0xAB] = new KeyValuePair<string, string>("BROWSER_FAVORITES", "Browser Favorites key"),
                [0xAC] = new KeyValuePair<string, string>("BROWSER_HOME", "Browser Start and Home key"),
                [0xAD] = new KeyValuePair<string, string>("VOLUME_MUTE", "Volume Mute key"),
                [0xAE] = new KeyValuePair<string, string>("VOLUME_DOWN", "Volume Down key"),
                [0xAF] = new KeyValuePair<string, string>("VOLUME_UP", "Volume Up key"),
                [0xB0] = new KeyValuePair<string, string>("MEDIA_NEXT_TRACK", "Next Track key"),
                [0xB1] = new KeyValuePair<string, string>("MEDIA_PREV_TRACK", "Previous Track key"),
                [0xB2] = new KeyValuePair<string, string>("MEDIA_STOP", "Stop Media key"),
                [0xB3] = new KeyValuePair<string, string>("MEDIA_PLAY_PAUSE", "Play/Pause Media key"),
                [0xB4] = new KeyValuePair<string, string>("LAUNCH_MAIL", "Start Mail key"),
                [0xB5] = new KeyValuePair<string, string>("LAUNCH_MEDIA_SELECT", "Select Media key"),
                [0xB6] = new KeyValuePair<string, string>("LAUNCH_APP1", "Start Application 1 key"),
                [0xB7] = new KeyValuePair<string, string>("LAUNCH_APP2", "Start Application 2 key"),
                [0xE5] = new KeyValuePair<string, string>("PROCESSKEY", "IME PROCESS key"),
                [0xF6] = new KeyValuePair<string, string>("ATTN", "Attn key"),
                [0xF7] = new KeyValuePair<string, string>("CRSEL", "CrSel key"),
                [0xF8] = new KeyValuePair<string, string>("EXSEL", "ExSel key"),
                [0xF9] = new KeyValuePair<string, string>("EREOF", "Erase EOF key"),
                [0xFA] = new KeyValuePair<string, string>("PLAY", "Play key"),
                [0xFB] = new KeyValuePair<string, string>("ZOOM", "Zoom key"),
                [0xFC] = new KeyValuePair<string, string>("NONAME", "Reserved"),
                [0xFD] = new KeyValuePair<string, string>("PA1", "PA1 key"),
                [0xFE] = new KeyValuePair<string, string>("OEM_CLEAR", "Clear key")
            };            
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

            //Debug.WriteLine(idleTime);
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

        public static void OnRefreshSchedules()
        {
            RefreshSchedules?.Invoke();
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