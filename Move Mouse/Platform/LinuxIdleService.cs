using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ellabi.Platform
{
    internal static class LinuxIdleService
    {
        private static readonly object _lock = new object();
        private static DateTime _lastQuery = DateTime.MinValue;
        private static TimeSpan _cachedIdle = TimeSpan.Zero;
        private static readonly TimeSpan CacheWindow = TimeSpan.FromMilliseconds(150);

        public static TimeSpan GetLastInputTime()
        {
            lock (_lock)
            {
                if (DateTime.UtcNow - _lastQuery < CacheWindow)
                {
                    return _cachedIdle;
                }

                TimeSpan? result = null;

                // 1. On X11, XScreenSaverQueryInfo is direct, fast, and millisecond-accurate
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY")))
                {
                    result = TryGetX11IdleTime();
                }

                // 2. On Wayland (GNOME Mutter IdleMonitor -> ms, or Freedesktop ScreenSaver -> s)
                if (!result.HasValue || (IsWayland && result.Value == TimeSpan.Zero))
                {
                    result = TryGetMutterIdleTime() ?? TryGetFreedesktopIdleTime() ?? result;
                }

                _cachedIdle = result ?? TimeSpan.Zero;
                _lastQuery = DateTime.UtcNow;
                return _cachedIdle;
            }
        }

        private static bool IsWayland => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY"));

        private static TimeSpan? TryGetX11IdleTime()
        {
            try
            {
                nint dpy = X11Native.XOpenDisplay(null);
                if (dpy == nint.Zero) return null;
                try
                {
                    nint infoPtr = X11Native.XScreenSaverAllocInfo();
                    if (infoPtr == nint.Zero) return null;
                    try
                    {
                        nuint root = X11Native.XDefaultRootWindow(dpy);
                        if (X11Native.XScreenSaverQueryInfo(dpy, root, infoPtr) != 0)
                        {
                            var info = Marshal.PtrToStructure<X11Native.XScreenSaverInfo>(infoPtr);
                            return TimeSpan.FromMilliseconds((double)info.idle);
                        }
                    }
                    finally
                    {
                        X11Native.XFree(infoPtr);
                    }
                }
                finally
                {
                    X11Native.XCloseDisplay(dpy);
                }
            }
            catch
            {
            }

            return null;
        }

        private static TimeSpan? TryGetMutterIdleTime()
        {
            try
            {
                string output = RunGdbus(
                    "org.gnome.Mutter.IdleMonitor",
                    "/org/gnome/Mutter/IdleMonitor/Core",
                    "org.gnome.Mutter.IdleMonitor.GetIdletime");

                if (!string.IsNullOrWhiteSpace(output))
                {
                    // Format: "(uint64 12345,)"
                    var parts = output.Split(new[] { ' ', ',', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var p in parts)
                    {
                        if (ulong.TryParse(p, out ulong ms))
                        {
                            return TimeSpan.FromMilliseconds(ms);
                        }
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        private static TimeSpan? TryGetFreedesktopIdleTime()
        {
            try
            {
                string output = RunGdbus(
                    "org.freedesktop.ScreenSaver",
                    "/ScreenSaver",
                    "org.freedesktop.ScreenSaver.GetSessionIdleTime");

                if (!string.IsNullOrWhiteSpace(output))
                {
                    // Format: "(uint32 12,)"
                    var parts = output.Split(new[] { ' ', ',', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var p in parts)
                    {
                        if (uint.TryParse(p, out uint seconds))
                        {
                            return TimeSpan.FromSeconds(seconds);
                        }
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        private static string RunGdbus(string dest, string path, string method)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "gdbus",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                psi.ArgumentList.Add("call");
                psi.ArgumentList.Add("--session");
                psi.ArgumentList.Add("--dest");
                psi.ArgumentList.Add(dest);
                psi.ArgumentList.Add("--object-path");
                psi.ArgumentList.Add(path);
                psi.ArgumentList.Add("--method");
                psi.ArgumentList.Add(method);

                using var proc = Process.Start(psi);
                if (proc == null) return null;
                string stdout = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit(500);
                return proc.ExitCode == 0 ? stdout : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
