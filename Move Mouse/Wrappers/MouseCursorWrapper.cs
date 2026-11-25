using ellabi.Platform;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace ellabi.Wrappers
{
    public class MouseCursorWrapper
    {
        private readonly HashSet<Point> _previousLocations = new HashSet<Point>();

        public bool BreakOnUserActivity { get; set; }

        public bool UserActivityDetected { get; private set; }

        public void MoveFromCurrentLocation(Point point)
        {
            try
            {
                if (BreakOnUserActivity && _previousLocations.Any() && !_previousLocations.Contains(GetCursorPosition()))
                {
                    UserActivityDetected = true;
                }
                else
                {
                    LinuxInputService.MoveRelative(point.X, point.Y);
                    _previousLocations.Add(GetCursorPosition());
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public void MoveNorth(int distance, int speed) => MoveSteps(0, -1, distance, speed);
        public void MoveNorthEast(int distance, int speed) => MoveSteps(1, -1, distance, speed);
        public void MoveEast(int distance, int speed) => MoveSteps(1, 0, distance, speed);
        public void MoveSouthEast(int distance, int speed) => MoveSteps(1, 1, distance, speed);
        public void MoveSouth(int distance, int speed) => MoveSteps(0, 1, distance, speed);
        public void MoveSouthWest(int distance, int speed) => MoveSteps(-1, 1, distance, speed);
        public void MoveWest(int distance, int speed) => MoveSteps(-1, 0, distance, speed);
        public void MoveNorthWest(int distance, int speed) => MoveSteps(-1, -1, distance, speed);

        private void MoveSteps(int dx, int dy, int distance, int speed)
        {
            for (int i = 0; i < distance; i++)
            {
                MoveFromCurrentLocation(new Point(dx, dy));

                if (UserActivityDetected)
                {
                    break;
                }

                if (speed > 0)
                {
                    Thread.Sleep(speed);
                }
            }
        }

        public Point GetCursorPosition() => LinuxInputService.GetCursorPosition();
    }
}
