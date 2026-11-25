using System;
using System.Diagnostics;
using System.Drawing;

namespace ellabi.Platform
{
    internal static class LinuxInputService
    {
        private static readonly UInputDevice _uinput = new UInputDevice();
        private static readonly object _x11Lock = new object();
        private static Point _virtualCursor = new Point(0, 0);

        public enum MouseButtonType
        {
            Left,
            Middle,
            Right
        }

        public enum ScrollDirectionType
        {
            Up,
            Down,
            Left,
            Right
        }

        private static bool HasDisplay => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY"));
        private static bool IsWayland => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY"));

        public static Point GetCursorPosition()
        {
            if (OperatingSystem.IsWindows())
            {
                return WindowsInputService.GetCursorPosition();
            }

            if (HasDisplay)
            {
                try
                {
                    lock (_x11Lock)
                    {
                        nint dpy = X11Native.XOpenDisplay(null);
                        if (dpy != nint.Zero)
                        {
                            try
                            {
                                nuint root = X11Native.XDefaultRootWindow(dpy);
                                if (X11Native.XQueryPointer(dpy, root, out _, out _, out int rx, out int ry, out _, out _, out _) != 0)
                                {
                                    _virtualCursor = new Point(rx, ry);
                                    return _virtualCursor;
                                }
                            }
                            finally
                            {
                                X11Native.XCloseDisplay(dpy);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    StaticCode.Logger?.Here().Error(ex.Message);
                }
            }

            return _virtualCursor;
        }

        public static void MoveRelative(int dx, int dy)
        {
            if (OperatingSystem.IsWindows())
            {
                WindowsInputService.MoveRelative(dx, dy);
                return;
            }

            try
            {
                _virtualCursor = new Point(_virtualCursor.X + dx, _virtualCursor.Y + dy);

                // Stealth mode (0,0): nudge screensaver/idle without moving pointer visibly
                if (dx == 0 && dy == 0)
                {
                    SimulateStealthActivity();
                    return;
                }

                // Prefer XTest on pure X11; prefer uinput on Wayland (since XTest only affects XWayland)
                if (!IsWayland && HasDisplay && TryX11MoveRelative(dx, dy))
                {
                    return;
                }

                if (_uinput.MoveRelative(dx, dy))
                {
                    return;
                }

                if (HasDisplay && TryX11MoveRelative(dx, dy))
                {
                    return;
                }

                TryRunCli("ydotool", "mousemove", "--", dx.ToString(), dy.ToString());
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public static void SetCursorPosition(int x, int y)
        {
            if (OperatingSystem.IsWindows())
            {
                WindowsInputService.SetCursorPosition(x, y);
                return;
            }

            try
            {
                _virtualCursor = new Point(x, y);

                if (HasDisplay)
                {
                    lock (_x11Lock)
                    {
                        nint dpy = X11Native.XOpenDisplay(null);
                        if (dpy != nint.Zero)
                        {
                            try
                            {
                                X11Native.XTestFakeMotionEvent(dpy, -1, x, y, X11Native.CurrentTime);
                                X11Native.XFlush(dpy);
                                return;
                            }
                            finally
                            {
                                X11Native.XCloseDisplay(dpy);
                            }
                        }
                    }
                }

                if (TryRunCli("ydotool", "mousemove", "--absolute", x.ToString(), y.ToString()))
                {
                    return;
                }

                TryRunCli("xdotool", "mousemove", x.ToString(), y.ToString());
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public static void MouseButton(MouseButtonType button, bool isDown)
        {
            if (OperatingSystem.IsWindows())
            {
                var winBtn = button switch
                {
                    MouseButtonType.Left => WindowsInputService.MouseButtonType.Left,
                    MouseButtonType.Middle => WindowsInputService.MouseButtonType.Middle,
                    MouseButtonType.Right => WindowsInputService.MouseButtonType.Right,
                    _ => WindowsInputService.MouseButtonType.Left
                };
                WindowsInputService.MouseButton(winBtn, isDown);
                return;
            }

            try
            {
                ushort evdevBtn = button switch
                {
                    MouseButtonType.Left => UInputDevice.BTN_LEFT,
                    MouseButtonType.Middle => UInputDevice.BTN_MIDDLE,
                    MouseButtonType.Right => UInputDevice.BTN_RIGHT,
                    _ => UInputDevice.BTN_LEFT
                };

                if (!IsWayland && HasDisplay && TryX11MouseButton(button, isDown))
                {
                    return;
                }

                if (_uinput.MouseButton(evdevBtn, isDown))
                {
                    return;
                }

                if (HasDisplay && TryX11MouseButton(button, isDown))
                {
                    return;
                }

                uint xBtn = button switch
                {
                    MouseButtonType.Left => 1,
                    MouseButtonType.Middle => 2,
                    MouseButtonType.Right => 3,
                    _ => 1
                };
                TryRunCli("xdotool", isDown ? "mousedown" : "mouseup", xBtn.ToString());
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public static void Scroll(ScrollDirectionType direction, uint distance)
        {
            if (OperatingSystem.IsWindows())
            {
                var winDir = direction switch
                {
                    ScrollDirectionType.Up => WindowsInputService.ScrollDirectionType.Up,
                    ScrollDirectionType.Down => WindowsInputService.ScrollDirectionType.Down,
                    ScrollDirectionType.Left => WindowsInputService.ScrollDirectionType.Left,
                    ScrollDirectionType.Right => WindowsInputService.ScrollDirectionType.Right,
                    _ => WindowsInputService.ScrollDirectionType.Down
                };
                WindowsInputService.Scroll(winDir, distance);
                return;
            }

            try
            {
                int clicks = Math.Max(1, (int)Math.Round(distance / 120.0));

                if (IsWayland || !HasDisplay)
                {
                    bool horizontal = direction == ScrollDirectionType.Left || direction == ScrollDirectionType.Right;
                    int delta = direction switch
                    {
                        ScrollDirectionType.Up => clicks,
                        ScrollDirectionType.Down => -clicks,
                        ScrollDirectionType.Right => clicks,
                        ScrollDirectionType.Left => -clicks,
                        _ => -clicks
                    };

                    if (_uinput.Scroll(horizontal, delta))
                    {
                        return;
                    }
                }

                if (HasDisplay)
                {
                    uint xBtn = direction switch
                    {
                        ScrollDirectionType.Up => 4,
                        ScrollDirectionType.Down => 5,
                        ScrollDirectionType.Left => 6,
                        ScrollDirectionType.Right => 7,
                        _ => 5
                    };

                    lock (_x11Lock)
                    {
                        nint dpy = X11Native.XOpenDisplay(null);
                        if (dpy != nint.Zero)
                        {
                            try
                            {
                                for (int i = 0; i < clicks; i++)
                                {
                                    X11Native.XTestFakeButtonEvent(dpy, xBtn, X11Native.True, X11Native.CurrentTime);
                                    X11Native.XTestFakeButtonEvent(dpy, xBtn, X11Native.False, X11Native.CurrentTime);
                                }
                                X11Native.XFlush(dpy);
                                return;
                            }
                            finally
                            {
                                X11Native.XCloseDisplay(dpy);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public static void SendKey(int virtualKeyCode, bool isDown)
        {
            if (OperatingSystem.IsWindows())
            {
                WindowsInputService.SendKey(virtualKeyCode, isDown);
                return;
            }

            try
            {
                if (!IsWayland && HasDisplay && TryX11Key(virtualKeyCode, isDown))
                {
                    return;
                }

                if (LinuxKeyMap.TryGetEvdevCode(virtualKeyCode, out ushort evdev) && _uinput.SendKey(evdev, isDown))
                {
                    return;
                }

                if (HasDisplay && TryX11Key(virtualKeyCode, isDown))
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private static void SimulateStealthActivity()
        {
            if (HasDisplay)
            {
                try
                {
                    lock (_x11Lock)
                    {
                        nint dpy = X11Native.XOpenDisplay(null);
                        if (dpy != nint.Zero)
                        {
                            try
                            {
                                X11Native.XTestFakeRelativeMotionEvent(dpy, 0, 0, X11Native.CurrentTime);
                                X11Native.XResetScreenSaver(dpy);
                                X11Native.XFlush(dpy);
                            }
                            finally
                            {
                                X11Native.XCloseDisplay(dpy);
                            }
                        }
                    }
                }
                catch
                {
                }
            }

            if (IsWayland)
            {
                // Poke Freedesktop ScreenSaver DBus + 1px round-trip via uinput if available
                TryRunCli("gdbus", "call", "--session", "--dest", "org.freedesktop.ScreenSaver",
                    "--object-path", "/ScreenSaver", "--method", "org.freedesktop.ScreenSaver.SimulateUserActivity");
            }
        }

        private static bool TryX11MoveRelative(int dx, int dy)
        {
            lock (_x11Lock)
            {
                nint dpy = X11Native.XOpenDisplay(null);
                if (dpy == nint.Zero) return false;
                try
                {
                    X11Native.XTestFakeRelativeMotionEvent(dpy, dx, dy, X11Native.CurrentTime);
                    X11Native.XFlush(dpy);
                    return true;
                }
                finally
                {
                    X11Native.XCloseDisplay(dpy);
                }
            }
        }

        private static bool TryX11MouseButton(MouseButtonType button, bool isDown)
        {
            uint xBtn = button switch
            {
                MouseButtonType.Left => 1,
                MouseButtonType.Middle => 2,
                MouseButtonType.Right => 3,
                _ => 1
            };

            lock (_x11Lock)
            {
                nint dpy = X11Native.XOpenDisplay(null);
                if (dpy == nint.Zero) return false;
                try
                {
                    X11Native.XTestFakeButtonEvent(dpy, xBtn, isDown ? X11Native.True : X11Native.False, X11Native.CurrentTime);
                    X11Native.XFlush(dpy);
                    return true;
                }
                finally
                {
                    X11Native.XCloseDisplay(dpy);
                }
            }
        }

        private static bool TryX11Key(int vk, bool isDown)
        {
            if (!LinuxKeyMap.TryGetX11KeySym(vk, out nuint keysym)) return false;

            lock (_x11Lock)
            {
                nint dpy = X11Native.XOpenDisplay(null);
                if (dpy == nint.Zero) return false;
                try
                {
                    byte keycode = X11Native.XKeysymToKeycode(dpy, keysym);
                    if (keycode == 0) return false;
                    X11Native.XTestFakeKeyEvent(dpy, keycode, isDown ? X11Native.True : X11Native.False, X11Native.CurrentTime);
                    X11Native.XFlush(dpy);
                    return true;
                }
                finally
                {
                    X11Native.XCloseDisplay(dpy);
                }
            }
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
                foreach (var arg in args) psi.ArgumentList.Add(arg);
                using var p = Process.Start(psi);
                p?.WaitForExit(1000);
                return p != null && p.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        public static void Dispose()
        {
            if (OperatingSystem.IsWindows())
            {
                WindowsInputService.Dispose();
                return;
            }

            _uinput.Dispose();
        }
    }
}
