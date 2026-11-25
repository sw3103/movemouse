using System;
using System.Diagnostics;

namespace ellabi.Platform
{
    internal static class LinuxSessionLockService
    {
        public static bool IsSessionLocked()
        {
            if (OperatingSystem.IsWindows())
            {
                return WindowsSessionLockService.IsSessionLocked();
            }

            try
            {
                // 1. Check loginctl LockedHint for current session
                string sessionId = Environment.GetEnvironmentVariable("XDG_SESSION_ID");
                if (!string.IsNullOrWhiteSpace(sessionId))
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "loginctl",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };
                    psi.ArgumentList.Add("show-session");
                    psi.ArgumentList.Add(sessionId);
                    psi.ArgumentList.Add("-p");
                    psi.ArgumentList.Add("LockedHint");
                    psi.ArgumentList.Add("--value");

                    using var proc = Process.Start(psi);
                    if (proc != null)
                    {
                        string output = proc.StandardOutput.ReadToEnd().Trim();
                        proc.WaitForExit(500);
                        if (proc.ExitCode == 0 && string.Equals(output, "yes", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }

                // 2. Fallback to org.freedesktop.ScreenSaver.GetActive
                var gdbus = new ProcessStartInfo
                {
                    FileName = "gdbus",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                gdbus.ArgumentList.Add("call");
                gdbus.ArgumentList.Add("--session");
                gdbus.ArgumentList.Add("--dest");
                gdbus.ArgumentList.Add("org.freedesktop.ScreenSaver");
                gdbus.ArgumentList.Add("--object-path");
                gdbus.ArgumentList.Add("/ScreenSaver");
                gdbus.ArgumentList.Add("--method");
                gdbus.ArgumentList.Add("org.freedesktop.ScreenSaver.GetActive");

                using var gProc = Process.Start(gdbus);
                if (gProc != null)
                {
                    string output = gProc.StandardOutput.ReadToEnd().Trim();
                    gProc.WaitForExit(500);
                    if (gProc.ExitCode == 0 && output.Contains("true", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            catch
            {
            }

            return false;
        }
    }
}
