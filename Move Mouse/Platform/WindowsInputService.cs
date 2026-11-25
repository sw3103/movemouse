using System;
using System.Drawing;

namespace ellabi.Platform
{
    internal static class WindowsInputService
    {
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

        public static Point GetCursorPosition()
        {
            if (WindowsNative.GetCursorPos(out var pt))
            {
                return new Point(pt.X, pt.Y);
            }

            return Point.Empty;
        }

        public static void MoveRelative(int dx, int dy)
        {
            if (dx == 0 && dy == 0)
            {
                // Same as stealth nudge on Linux: a no-op move still refreshes last-input tracking on Windows.
                return;
            }

            SendMouse(WindowsNative.MouseeventfMove, dx, dy, 0);
        }

        public static void SetCursorPosition(int x, int y)
        {
            WindowsNative.SetCursorPos(x, y);
        }

        public static void MouseButton(MouseButtonType button, bool isDown)
        {
            uint flag = button switch
            {
                MouseButtonType.Left => isDown ? WindowsNative.MouseeventfLeftdown : WindowsNative.MouseeventfLeftup,
                MouseButtonType.Middle => isDown ? WindowsNative.MouseeventfMiddledown : WindowsNative.MouseeventfMiddleup,
                MouseButtonType.Right => isDown ? WindowsNative.MouseeventfRightdown : WindowsNative.MouseeventfRightup,
                _ => isDown ? WindowsNative.MouseeventfLeftdown : WindowsNative.MouseeventfLeftup
            };
            SendMouse(flag, 0, 0, 0);
        }

        public static void Scroll(ScrollDirectionType direction, uint distance)
        {
            int notches = Math.Max(1, (int)Math.Round(distance / 120.0));
            int delta = notches * 120;
            uint flag = direction switch
            {
                ScrollDirectionType.Left => WindowsNative.MouseeventfHwheel,
                ScrollDirectionType.Right => WindowsNative.MouseeventfHwheel,
                _ => WindowsNative.MouseeventfWheel
            };

            if (direction == ScrollDirectionType.Down || direction == ScrollDirectionType.Left)
            {
                delta = -delta;
            }

            for (int i = 0; i < notches; i++)
            {
                SendMouse(flag, 0, 0, unchecked((uint)delta));
            }
        }

        public static void SendKey(int virtualKeyCode, bool isDown)
        {
            uint flags = isDown ? 0u : WindowsNative.KeyeventfKeyup;
            if (IsExtendedKey(virtualKeyCode))
            {
                flags |= WindowsNative.KeyeventfExtendedkey;
            }

            var input = new WindowsNative.INPUT
            {
                Type = WindowsNative.InputKeyboard,
                U = new WindowsNative.InputUnion
                {
                    Keyboard = new WindowsNative.KeyboardInput
                    {
                        VirtualKey = (ushort)(virtualKeyCode & 0xFFFF),
                        ScanCode = 0,
                        Flags = flags,
                        Time = 0,
                        ExtraInfo = 0
                    }
                }
            };

            WindowsNative.SendInput(1, new[] { input }, System.Runtime.InteropServices.Marshal.SizeOf<WindowsNative.INPUT>());
        }

        public static void Dispose()
        {
        }

        private static void SendMouse(uint flags, int dx, int dy, uint mouseData)
        {
            var input = new WindowsNative.INPUT
            {
                Type = WindowsNative.InputMouse,
                U = new WindowsNative.InputUnion
                {
                    Mouse = new WindowsNative.MouseInput
                    {
                        Dx = dx,
                        Dy = dy,
                        MouseData = mouseData,
                        Flags = flags,
                        Time = 0,
                        ExtraInfo = 0
                    }
                }
            };

            WindowsNative.SendInput(1, new[] { input }, System.Runtime.InteropServices.Marshal.SizeOf<WindowsNative.INPUT>());
        }

        private static bool IsExtendedKey(int vk)
        {
            return vk is 0xA3 or 0xA5 or 0x21 or 0x22 or 0x23 or 0x24 or 0x25 or 0x26 or 0x27 or 0x28 or 0x2D or 0x2E or 0x5B or 0x5C;
        }
    }
}