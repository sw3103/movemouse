using ellabi.Wrappers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ellabi.Actions
{
    public class MoveMouseCursorAction : ActionBase
    {
        private int _distance;
        private CursorDirection _direction;
        private CursorSpeed _speed;
        private int _delay;

        public enum CursorDirection
        {
            Square,
            None,
            Random,
            North,
            NorthEast,
            East,
            SouthEast,
            South,
            SouthWest,
            West,
            NorthWest,
            UpAndDown,
            DownAndUp,
            LeftAndRight,
            RightAndLeft
        }

        public enum CursorSpeed
        {
            Slow,
            Normal,
            Fast,
            Custom
        }

        public IEnumerable<CursorDirection> CursorDirectionValues => Enum.GetValues(typeof(CursorDirection)).Cast<CursorDirection>();

        public IEnumerable<CursorSpeed> CursorSpeedValues => Enum.GetValues(typeof(CursorSpeed)).Cast<CursorSpeed>();

        public int Distance
        {
            get => _distance;
            set
            {
                _distance = value;
                OnPropertyChanged();
            }
        }

        public CursorDirection Direction
        {
            get => _direction;
            set
            {
                _direction = value;
                OnPropertyChanged();
            }
        }

        public override bool IsValid => Direction.Equals(CursorDirection.None) || (Distance > 0);

        public CursorSpeed Speed
        {
            get => _speed;
            set
            {
                _speed = value;

                switch (_speed)
                {
                    case CursorSpeed.Fast:
                        {
                            _delay = 0;
                            break;
                        }
                    case CursorSpeed.Normal:
                        {
                            _delay = 5;
                            break;
                        }
                    case CursorSpeed.Slow:
                        {
                            _delay = 10;
                            break;
                        }
                }

                OnPropertyChanged();
            }
        }

        public int Delay
        {
            get => _delay;
            set
            {
                if (_speed == CursorSpeed.Custom)
                {
                    _delay = value;
                    OnPropertyChanged();
                }
            }
        }

        public MoveMouseCursorAction()
        {
            _distance = 10;
            _direction = CursorDirection.Square;
            Speed = CursorSpeed.Normal;
            InterruptsIdleTime = true;
        }

        public override bool CanExecute()
        {
            return IsValid;
        }

        public override void Execute()
        {
            try
            {
                IntervalExecutionCount++;
                StaticCode.Logger?.Here().Information(ToString());

                switch (Direction)
                {
                    case CursorDirection.Square:
                        MouseCursorWrapper.MoveEast(Distance, _delay);
                        MouseCursorWrapper.MoveSouth(Distance, _delay);
                        MouseCursorWrapper.MoveWest(Distance, _delay);
                        MouseCursorWrapper.MoveNorth(Distance, _delay);
                        break;
                    case CursorDirection.North:
                        MouseCursorWrapper.MoveNorth(Distance, _delay);
                        break;
                    case CursorDirection.East:
                        MouseCursorWrapper.MoveEast(Distance, _delay);
                        break;
                    case CursorDirection.South:
                        MouseCursorWrapper.MoveSouth(Distance, _delay);
                        break;
                    case CursorDirection.West:
                        MouseCursorWrapper.MoveWest(Distance, _delay);
                        break;
                    case CursorDirection.NorthEast:
                        MouseCursorWrapper.MoveNorthEast(Distance, _delay);
                        break;
                    case CursorDirection.SouthEast:
                        MouseCursorWrapper.MoveSouthEast(Distance, _delay);
                        break;
                    case CursorDirection.SouthWest:
                        MouseCursorWrapper.MoveSouthWest(Distance, _delay);
                        break;
                    case CursorDirection.NorthWest:
                        MouseCursorWrapper.MoveNorthWest(Distance, _delay);
                        break;
                    case CursorDirection.UpAndDown:
                        MouseCursorWrapper.MoveNorth(Distance, _delay);
                        MouseCursorWrapper.MoveSouth(Distance, _delay);
                        break;
                    case CursorDirection.DownAndUp:
                        MouseCursorWrapper.MoveSouth(Distance, _delay);
                        MouseCursorWrapper.MoveNorth(Distance, _delay);
                        break;
                    case CursorDirection.LeftAndRight:
                        MouseCursorWrapper.MoveWest(Distance, _delay);
                        MouseCursorWrapper.MoveEast(Distance, _delay);
                        break;
                    case CursorDirection.RightAndLeft:
                        MouseCursorWrapper.MoveEast(Distance, _delay);
                        MouseCursorWrapper.MoveWest(Distance, _delay);
                        break;
                    case CursorDirection.Random:
                        int distanceRemaining = Distance;
                        int lastDirection = 0;

                        while (distanceRemaining > 0)
                        {
                            const int maxDistance = 150;
                            int distance = new Random(distanceRemaining).Next(1, (distanceRemaining < maxDistance) ? distanceRemaining : maxDistance);
                            distanceRemaining = distanceRemaining - distance;
                            int direction;

                            do
                            {
                                direction = new Random().Next(1, 9);
                            } while (direction.Equals(lastDirection));

                            lastDirection = direction;

                            switch (direction)
                            {
                                case 1:
                                    MouseCursorWrapper.MoveNorth(distance, _delay);
                                    break;
                                case 2:
                                    MouseCursorWrapper.MoveEast(distance, _delay);
                                    break;
                                case 3:
                                    MouseCursorWrapper.MoveSouth(distance, _delay);
                                    break;
                                case 4:
                                    MouseCursorWrapper.MoveWest(distance, _delay);
                                    break;
                                case 5:
                                    MouseCursorWrapper.MoveNorthEast(distance, _delay);
                                    break;
                                case 6:
                                    MouseCursorWrapper.MoveSouthEast(distance, _delay);
                                    break;
                                case 7:
                                    MouseCursorWrapper.MoveSouthWest(distance, _delay);
                                    break;
                                case 8:
                                    MouseCursorWrapper.MoveNorthWest(distance, _delay);
                                    break;
                            }
                        }

                        break;
                    case CursorDirection.None:
                        MouseCursorWrapper.MoveFromCurrentLocation(new Point());
                        break;
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public override string ToString()
        {
            return $"{this.GetType().Name} | Name = {Name} | Distance = {Distance} | Direction = {Direction} | Speed = {Speed} | Delay = {Delay} | Trigger = {Trigger} | Repeat = {Repeat} | RepeatMode = {RepeatMode} | IntervalThrottle = {IntervalThrottle} | IntervalExecutionCount = {IntervalExecutionCount} | InterruptsIdleTime = {InterruptsIdleTime}";
        }
    }
}