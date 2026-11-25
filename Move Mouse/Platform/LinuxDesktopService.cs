using System;
using System.Diagnostics;
using System.IO;

namespace ellabi.Platform
{
    internal static class LinuxDesktopService
    {
        public static void OpenTarget(string target)
        {
            if (string.IsNullOrWhiteSpace(target)) return;

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "xdg-open",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                psi.ArgumentList.Add(target);
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

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "notify-send",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                psi.ArgumentList.Add("-a");
                psi.ArgumentList.Add("Move Mouse");
                psi.ArgumentList.Add(string.IsNullOrWhiteSpace(title) ? "Move Mouse" : title);
                psi.ArgumentList.Add(message);
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public static void FollowLog(string logPath)
        {
            if (string.IsNullOrWhiteSpace(logPath)) return;

            string escaped = logPath.Replace("'", "'\\''");
            string tailCmd = $"tail -n 500 -f '{escaped}'";

            (string bin, string[] args)[] terminals =
            {
                ("konsole", new[] { "-e", "bash", "-c", tailCmd }),
                ("gnome-terminal", new[] { "--", "bash", "-c", tailCmd }),
                ("xfce4-terminal", new[] { "-e", $"bash -c \"{tailCmd}\"" }),
                ("alacritty", new[] { "-e", "bash", "-c", tailCmd }),
                ("kitty", new[] { "bash", "-c", tailCmd }),
                ("xterm", new[] { "-T", "Move Mouse Log", "-e", "bash", "-c", tailCmd }),
                ("x-terminal-emulator", new[] { "-e", "bash", "-c", tailCmd })
            };

            foreach (var (bin, args) in terminals)
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = bin,
                        UseShellExecute = false
                    };
                    foreach (var a in args) psi.ArgumentList.Add(a);
                    var p = Process.Start(psi);
                    if (p != null) return;
                }
                catch
                {
                }
            }

            // Fallback: open log file with default viewer
            OpenTarget(logPath);
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
                    FileName = "pwsh",
                    UseShellExecute = false,
                    CreateNoWindow = hidden
                };
                psi.ArgumentList.Add("-ExecutionPolicy");
                psi.ArgumentList.Add("Bypass");
                psi.ArgumentList.Add("-File");
                psi.ArgumentList.Add(trimmed);
            }
            else if (trimmed.EndsWith(".py", StringComparison.OrdinalIgnoreCase))
            {
                psi = new ProcessStartInfo
                {
                    FileName = "python3",
                    UseShellExecute = false,
                    CreateNoWindow = hidden
                };
                psi.ArgumentList.Add(trimmed);
            }
            else
            {
                psi = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    UseShellExecute = false,
                    CreateNoWindow = hidden
                };
                psi.ArgumentList.Add(trimmed);
            }

            using var proc = Process.Start(psi);
            if (waitForExit && proc != null)
            {
                proc.WaitForExit();
            }
        }
    }
}
