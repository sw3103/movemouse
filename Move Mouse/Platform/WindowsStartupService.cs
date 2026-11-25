using Microsoft.Win32;
using System;
using System.IO;

namespace ellabi.Platform
{
    internal static class WindowsStartupService
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

        public static StartupTaskState GetState()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false);
                if (key == null)
                {
                    return StartupTaskState.Disabled;
                }

                string value = key.GetValue(StaticCode.RunRegistryValueName) as string;
                return string.IsNullOrWhiteSpace(value) ? StartupTaskState.Disabled : StartupTaskState.Enabled;
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
                using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true)
                               ?? Registry.CurrentUser.CreateSubKey(RunKeyPath);

                if (enabled)
                {
                    string exePath = Environment.ProcessPath;
                    if (string.IsNullOrWhiteSpace(exePath))
                    {
                        exePath = Path.Combine(AppContext.BaseDirectory, "MoveMouse.exe");
                    }

                    key.SetValue(StaticCode.RunRegistryValueName, $"\"{exePath}\"");
                }
                else
                {
                    key.DeleteValue(StaticCode.RunRegistryValueName, false);
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }
    }
}