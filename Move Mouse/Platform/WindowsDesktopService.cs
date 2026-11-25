using System;
using System.Diagnostics;
using System.IO;

namespace ellabi.Platform
{
    internal static class WindowsDesktopService
    {
        public static void OpenTarget(string target)
        {
            if (string.IsNullOrWhiteSpace(target)) return;

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = target,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public static void ShowNotification(string title, string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            StaticCode.Logger?.Here().Information($"{title}: {message}");
        }

        public static void FollowLog(string logPath)
        {
            if (string.IsNullOrWhiteSpace(logPath) || !File.Exists(logPath)) return;

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoExit -Command Get-Content -Path '{logPath.Replace("'", "''")}' -Wait -Tail 500",
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public static void RunScript(string scriptPath, bool waitForExit, bool hidden)
        {
            if (string.IsNullOrWhiteSpace(scriptPath) || !File.Exists(scriptPath)) return;

            string trimmed = scriptPath.Trim();
            ProcessStartInfo psi;

            if (trimmed.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
            {
                psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    UseShellExecute = false,
                    CreateNoWindow = hidden
                };
                psi.ArgumentList.Add("-ExecutionPolicy");
                psi.ArgumentList.Add("Bypass");
                psi.ArgumentList.Add("-File");
                psi.ArgumentList.Add(trimmed);
            }
            else if (trimmed.EndsWith(".bat", StringComparison.OrdinalIgnoreCase) ||
                     trimmed.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase))
            {
                psi = new ProcessStartInfo
                {
                    FileName = trimmed,
                    UseShellExecute = false,
                    CreateNoWindow = hidden
                };
            }
            else
            {
                psi = new ProcessStartInfo
                {
                    FileName = trimmed,
                    UseShellExecute = true,
                    CreateNoWindow = hidden
                };
            }

            using var proc = Process.Start(psi);
            if (waitForExit && proc != null)
            {
                proc.WaitForExit();
            }
        }
    }
}