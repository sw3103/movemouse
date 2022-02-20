using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

namespace ellabi.Wrappers
{
    public static class MouseCursorWrapper
    {
        public static void MoveFromCurrentLocation(Point point)
        {
            try
            {
                var mi = new NativeMethods.MOUSEINPUT
                {
                    dx = point.X,
                    dy = point.Y,
                    mouseData = 0,
                    time = 0,
                    dwFlags = NativeMethods.MouseEventFlags.MOVE,
                    dwExtraInfo = UIntPtr.Zero
                };
                var input = new NativeMethods.INPUT
                {
                    mi = mi,
                    type = Convert.ToInt32(NativeMethods.Win32Consts.INPUT_MOUSE)
                };
                NativeMethods.SendInput(1, ref input, Marshal.SizeOf(input));
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public static void MoveNorth(int distance, int speed)
        {
            for (int i = 0; i < distance; i++)
            {
                MoveFromCurrentLocation(new Point(0, -1));
                Thread.Sleep(speed);
            }
        }

        public static void MoveNorthEast(int distance, int speed)
        {
            for (int i = 0; i < distance; i++)
            {
                MoveFromCurrentLocation(new Point(1, -1));
                Thread.Sleep(speed);
            }
        }

        public static void MoveEast(int distance, int speed)
        {
            for (int i = 0; i < distance; i++)
            {
                MoveFromCurrentLocation(new Point(1, 0));
                Thread.Sleep(speed);
            }
        }

        public static void MoveSouthEast(int distance, int speed)
        {
            for (int i = 0; i < distance; i++)
            {
                MoveFromCurrentLocation(new Point(1, 1));
                Thread.Sleep(speed);
            }
        }

        public static void MoveSouth(int distance, int speed)
        {
            for (int i = 0; i < distance; i++)
            {
                MoveFromCurrentLocation(new Point(0, 1));
                Thread.Sleep(speed);
            }
        }

        public static void MoveSouthWest(int distance, int speed)
        {
            for (int i = 0; i < distance; i++)
            {
                MoveFromCurrentLocation(new Point(-1, 1));
                Thread.Sleep(speed);
            }
        }

        public static void MoveWest(int distance, int speed)
        {
            for (int i = 0; i < distance; i++)
            {
                MoveFromCurrentLocation(new Point(-1, 0));
                Thread.Sleep(speed);
            }
        }

        public static void MoveNorthWest(int distance, int speed)
        {
            for (int i = 0; i < distance; i++)
            {
                MoveFromCurrentLocation(new Point(-1, -1));
                Thread.Sleep(speed);
            }
        }
    }
}