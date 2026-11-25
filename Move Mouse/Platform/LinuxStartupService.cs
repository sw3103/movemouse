using System;
using System.IO;

namespace ellabi.Platform
{
    public enum StartupTaskState
    {
        Disabled = 0,
        DisabledByUser = 1,
        Enabled = 2
    }

    internal static class LinuxStartupService
    {
        private static string AutostartDirectory
        {
            get
            {
                string configHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
                if (string.IsNullOrWhiteSpace(configHome))
                {
                    configHome = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
                }
                return Path.Combine(configHome, "autostart");
            }
        }

        private static string DesktopFilePath => Path.Combine(AutostartDirectory, "movemouse.desktop");

        public static StartupTaskState GetState()
        {
            try
            {
                if (!File.Exists(DesktopFilePath))
                {
                    return StartupTaskState.Disabled;
                }

                string content = File.ReadAllText(DesktopFilePath);
                if (content.Contains("Hidden=true", StringComparison.OrdinalIgnoreCase) ||
                    content.Contains("X-GNOME-Autostart-enabled=false", StringComparison.OrdinalIgnoreCase))
                {
                    return StartupTaskState.Disabled;
                }

                return StartupTaskState.Enabled;
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
                return StartupTaskState.Disabled;
            }
        }

        public static void SetEnabled(bool enabled)
        {
            try
            {
                if (enabled)
                {
                    Directory.CreateDirectory(AutostartDirectory);
                    string exePath = Environment.ProcessPath ?? AppContext.BaseDirectory;
                    string desktopEntry =
                        "[Desktop Entry]\n" +
                        "Type=Application\n" +
                        "Name=Move Mouse\n" +
                        "Comment=Keep your session awake and automate mouse/keyboard actions\n" +
                        $"Exec=\"{exePath}\"\n" +
                        "Terminal=false\n" +
                        "X-GNOME-Autostart-enabled=true\n";
                    File.WriteAllText(DesktopFilePath, desktopEntry);
                }
                else if (File.Exists(DesktopFilePath))
                {
                    File.Delete(DesktopFilePath);
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }
    }
}
