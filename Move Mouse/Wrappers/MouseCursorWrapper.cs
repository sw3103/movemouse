using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using static ellabi.Wrappers.NativeMethods;

namespace ellabi.Wrappers
{
    public class MouseCursorWrapper
    {
        private HashSet<Point> _previousLocations = new HashSet<Point>();

        public bool BreakOnUserActivity { get; set; }

        public bool UserActivityDetected { get; private set; }

        public void MoveFromCurrentLocation(Point point)
        {
            try
            {
                if (BreakOnUserActivity && (_previousLocations.Any() && !_previousLocations.Contains(GetCursorPosition())))
                {
                    UserActivityDetected = true;
                }
                else
                {
                    NativeMethods.INPUT[] inputs = new NativeMethods.INPUT[]
                    {
                        new NativeMethods.INPUT
                        {
                            type = (int)InputType.Mouse,
                            u = new InputUnion
                            {
                                mi = new NativeMethods.MOUSEINPUT
                                {
                                    dx = point.X,
                                    dy = point.Y,
                                    mouseData = 0,
                                    time = 0,
                                    dwFlags = NativeMethods.MouseEventFlags.MOVE,
                                    dwExtraInfo = IntPtr.Zero
                                }
                            }
                        }
                    };
                    NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(NativeMethods.INPUT)));
                    _previousLocations.Add(GetCursorPosition());
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public void MoveNorth(int distance, int speed)
        {
            for (int i = 0; i < distance; i++)
            {
                MoveFromCurrentLocation(new Point(0, -1));

                if (UserActivityDetected)
                {
                    break;
                }
                else
                {
                    Thread.Sleep(speed);
                }
            }
        }

        public void MoveNorthEast(int distance, int speed)
        {
            for (int i = 0; i < distance; i++)
            {
                MoveFromCurrentLocation(new Point(1, -1));

                if (UserActivityDetected)
                {
                    break;
                }
                else
                {
                    Thread.Sleep(speed);
                }
            }
        }

        public void MoveEast(int distance, int speed)
        {
            for (int i = 0; i < distance; i++)
            {
                MoveFromCurrentLocation(new Point(1, 0));

                if (UserActivityDetected)
                {
                    break;
                }
                else
                {
                    Thread.Sleep(speed);
                }
            }
        }

        public void MoveSouthEast(int distance, int speed)
        {
            for (int i = 0; i < distance; i++)
            {
                MoveFromCurrentLocation(new Point(1, 1));

                if (UserActivityDetected)
                {
                    break;
                }
                else
                {
                    Thread.Sleep(speed);
                }
            }
        }

        public void MoveSouth(int distance, int speed)
        {
            for (int i = 0; i < distance; i++)
            {
                MoveFromCurrentLocation(new Point(0, 1));

                if (UserActivityDetected)
                {
                    break;
                }
                else
                {
                    Thread.Sleep(speed);
                }
            }
        }

        public void MoveSouthWest(int distance, int speed)
        {
            for (int i = 0; i < distance; i++)
            {
                MoveFromCurrentLocation(new Point(-1, 1));

                if (UserActivityDetected)
                {
                    break;
                }
                else
                {
                    Thread.Sleep(speed);
                }
            }
        }

        public void MoveWest(int distance, int speed)
        {
            for (int i = 0; i < distance; i++)
            {
                MoveFromCurrentLocation(new Point(-1, 0));

                if (UserActivityDetected)
                {
                    break;
                }
                else
                {
                    Thread.Sleep(speed);
                }
            }
        }

        public void MoveNorthWest(int distance, int speed)
        {
            for (int i = 0; i < distance; i++)
            {
                MoveFromCurrentLocation(new Point(-1, -1));

                if (UserActivityDetected)
                {
                    break;
                }
                else
                {
                    Thread.Sleep(speed);
                }
            }
        }

        public Point GetCursorPosition()
        {
            var pt = new NativeMethods.Win32Point();

            if (NativeMethods.GetCursorPos(ref pt))
            {
                return new Point(pt.X, pt.Y);
            }

            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }
    }
}