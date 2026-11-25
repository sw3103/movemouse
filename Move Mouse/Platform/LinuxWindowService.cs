using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace ellabi.Platform
{
    internal static class LinuxWindowService
    {
        public sealed class WindowEntry
        {
            public nuint Handle { get; init; }
            public string Title { get; init; }
            public string ProcessName { get; init; }
        }

        private static bool HasDisplay => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY"));

        public static List<WindowEntry> GetOpenWindows()
        {
            var windows = new List<WindowEntry>();

            if (HasDisplay)
            {
                try
                {
                    nint dpy = X11Native.XOpenDisplay(null);
                    if (dpy != nint.Zero)
                    {
                        try
                        {
                            nuint root = X11Native.XDefaultRootWindow(dpy);
                            nuint clientListAtom = X11Native.XInternAtom(dpy, "_NET_CLIENT_LIST", X11Native.False);
                            nuint netWmNameAtom = X11Native.XInternAtom(dpy, "_NET_WM_NAME", X11Native.False);
                            nuint utf8Atom = X11Native.XInternAtom(dpy, "UTF8_STRING", X11Native.False);
                            nuint netWmPidAtom = X11Native.XInternAtom(dpy, "_NET_WM_PID", X11Native.False);

                            if (X11Native.XGetWindowProperty(dpy, root, clientListAtom, 0, 4096, X11Native.False,
                                    X11Native.XaWindow, out _, out int format, out nuint count, out _, out nint prop) == 0 && prop != nint.Zero)
                            {
                                try
                                {
                                    int winCount = (int)count;
                                    for (int i = 0; i < winCount; i++)
                                    {
                                        nuint win = format == 32
                                            ? (nuint)Marshal.ReadIntPtr(prop, i * nint.Size)
                                            : (nuint)Marshal.ReadInt64(prop, i * 8);

                                        string title = GetX11WindowTitle(dpy, win, netWmNameAtom, utf8Atom);
                                        int pid = GetX11WindowPid(dpy, win, netWmPidAtom);
                                        string procName = string.Empty;

                                        if (pid > 0)
                                        {
                                            try
                                            {
                                                procName = Process.GetProcessById(pid).ProcessName;
                                            }
                                            catch
                                            {
                                            }
                                        }

                                        if (!string.IsNullOrWhiteSpace(title) || !string.IsNullOrWhiteSpace(procName))
                                        {
                                            windows.Add(new WindowEntry
                                            {
                                                Handle = win,
                                                Title = title,
                                                ProcessName = procName
                                            });
                                        }
                                    }
                                }
                                finally
                                {
                                    X11Native.XFree(prop);
                                }
                            }
                        }
                        finally
                        {
                            X11Native.XCloseDisplay(dpy);
                        }
                    }
                }
                catch (Exception ex)
                {
                    StaticCode.Logger?.Here().Error(ex.Message);
                }
            }

            // Fallback / supplement: running processes (especially useful on pure Wayland)
            if (windows.Count == 0)
            {
                try
                {
                    foreach (var p in Process.GetProcesses())
                    {
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(p.ProcessName))
                            {
                                windows.Add(new WindowEntry
                                {
                                    Handle = 0,
                                    Title = p.MainWindowTitle,
                                    ProcessName = p.ProcessName
                                });
                            }
                        }
                        catch
                        {
                        }
                    }
                }
                catch
                {
                }
            }

            return windows;
        }

        public static bool ActivateWindow(string pattern, bool byWindowTitle)
        {
            if (string.IsNullOrWhiteSpace(pattern)) return false;

            var windows = GetOpenWindows();
            WindowEntry match = FindMatch(windows, pattern, byWindowTitle);

            if (match != null && match.Handle != 0 && HasDisplay)
            {
                return ActivateX11Window(match.Handle);
            }

            // Wayland / CLI fallbacks (kdotool on KDE Plasma, wmctrl, xdotool)
            string cleanPattern = pattern.Trim('*');
            if (byWindowTitle)
            {
                if (TryRunCli("kdotool", "search", "--name", cleanPattern, "windowactivate")) return true;
                if (TryRunCli("wmctrl", "-a", cleanPattern)) return true;
                if (TryRunCli("xdotool", "search", "--name", cleanPattern, "windowactivate")) return true;
            }
            else
            {
                if (TryRunCli("kdotool", "search", "--class", cleanPattern, "windowactivate")) return true;
                if (TryRunCli("wmctrl", "-x", "-a", cleanPattern)) return true;
                if (TryRunCli("xdotool", "search", "--class", cleanPattern, "windowactivate")) return true;
            }

            return false;
        }

        private static WindowEntry FindMatch(List<WindowEntry> windows, string pattern, bool byWindowTitle)
        {
            if (byWindowTitle)
            {
                if (pattern.StartsWith("*") && pattern.EndsWith("*"))
                {
                    string needle = pattern.Trim('*');
                    return windows.FirstOrDefault(w => !string.IsNullOrEmpty(w.Title) && w.Title.Contains(needle, StringComparison.CurrentCultureIgnoreCase));
                }
                if (pattern.StartsWith("*"))
                {
                    string needle = pattern.Trim('*');
                    return windows.FirstOrDefault(w => !string.IsNullOrEmpty(w.Title) && w.Title.EndsWith(needle, StringComparison.CurrentCultureIgnoreCase));
                }
                if (pattern.EndsWith("*"))
                {
                    string needle = pattern.Trim('*');
                    return windows.FirstOrDefault(w => !string.IsNullOrEmpty(w.Title) && w.Title.StartsWith(needle, StringComparison.CurrentCultureIgnoreCase));
                }
                return windows.FirstOrDefault(w => string.Equals(w.Title, pattern, StringComparison.CurrentCultureIgnoreCase));
            }

            return windows.FirstOrDefault(w => string.Equals(w.ProcessName, pattern, StringComparison.CurrentCultureIgnoreCase));
        }

        private static bool ActivateX11Window(nuint window)
        {
            try
            {
                nint dpy = X11Native.XOpenDisplay(null);
                if (dpy == nint.Zero) return false;
                try
                {
                    nuint root = X11Native.XDefaultRootWindow(dpy);
                    nuint activeAtom = X11Native.XInternAtom(dpy, "_NET_ACTIVE_WINDOW", X11Native.False);

                    X11Native.XMapRaised(dpy, window);

                    var ev = new X11Native.XEvent();
                    ev.xclient = new X11Native.XClientMessageEvent
                    {
                        type = X11Native.ClientMessage,
                        send_event = X11Native.True,
                        display = dpy,
                        window = window,
                        message_type = activeAtom,
                        format = 32,
                        data0 = 1, // source indication: application
                        data1 = 0,
                        data2 = 0,
                        data3 = 0,
                        data4 = 0
                    };

                    X11Native.XSendEvent(dpy, root, X11Native.False,
                        X11Native.SubstructureRedirectMask | X11Native.SubstructureNotifyMask, ref ev);
                    X11Native.XFlush(dpy);
                    return true;
                }
                finally
                {
                    X11Native.XCloseDisplay(dpy);
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
                return false;
            }
        }

        private static string GetX11WindowTitle(nint dpy, nuint win, nuint netWmNameAtom, nuint utf8Atom)
        {
            if (X11Native.XGetWindowProperty(dpy, win, netWmNameAtom, 0, 1024, X11Native.False,
                    utf8Atom, out _, out _, out nuint count, out _, out nint prop) == 0 && prop != nint.Zero)
            {
                try
                {
                    if ((int)count > 0)
                    {
                        return Marshal.PtrToStringUTF8(prop, (int)count);
                    }
                }
                finally
                {
                    X11Native.XFree(prop);
                }
            }

            if (X11Native.XFetchName(dpy, win, out nint namePtr) != 0 && namePtr != nint.Zero)
            {
                try
                {
                    return Marshal.PtrToStringAnsi(namePtr);
                }
                finally
                {
                    X11Native.XFree(namePtr);
                }
            }

            return null;
        }

        private static int GetX11WindowPid(nint dpy, nuint win, nuint netWmPidAtom)
        {
            if (X11Native.XGetWindowProperty(dpy, win, netWmPidAtom, 0, 1, X11Native.False,
                    X11Native.XaCardinal, out _, out _, out nuint count, out _, out nint prop) == 0 && prop != nint.Zero)
            {
                try
                {
                    if ((int)count > 0)
                    {
                        return Marshal.ReadInt32(prop);
                    }
                }
                finally
                {
                    X11Native.XFree(prop);
                }
            }

            return 0;
        }

        private static bool TryRunCli(string fileName, params string[] args)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                foreach (var a in args) psi.ArgumentList.Add(a);
                using var p = Process.Start(psi);
                p?.WaitForExit(1000);
                return p != null && p.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
