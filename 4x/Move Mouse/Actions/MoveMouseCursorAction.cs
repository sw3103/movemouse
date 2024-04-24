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
        private int _upperDistance;
        private bool _random;
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
                if (value > UpperDistance)
                {
                    UpperDistance = value;
                }

                _distance = value;
                OnPropertyChanged();
            }
        }

        public int UpperDistance
        {
            get => _upperDistance;
            set
            {
                if (value < Distance)
                {
                    Distance = value;
                }

                _upperDistance = value < 1 ? 1 : value;
                OnPropertyChanged();
            }
        }

        public bool Random
        {
            get => _random;
            set
            {
                _random = value;
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
            _upperDistance = 20;
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
                int distance = Random ? new Random().Next(Convert.ToInt32(Distance), Convert.ToInt32(UpperDistance)) : Distance;

                switch (Direction)
                {
                    case CursorDirection.Square:
                        MouseCursorWrapper.MoveEast(distance, _delay);
                        MouseCursorWrapper.MoveSouth(distance, _delay);
                        MouseCursorWrapper.MoveWest(distance, _delay);
                        MouseCursorWrapper.MoveNorth(distance, _delay);
                        break;
                    case CursorDirection.North:
                        MouseCursorWrapper.MoveNorth(distance, _delay);
                        break;
                    case CursorDirection.East:
                        MouseCursorWrapper.MoveEast(distance, _delay);
                        break;
                    case CursorDirection.South:
                        MouseCursorWrapper.MoveSouth(distance, _delay);
                        break;
                    case CursorDirection.West:
                        MouseCursorWrapper.MoveWest(distance, _delay);
                        break;
                    case CursorDirection.NorthEast:
                        MouseCursorWrapper.MoveNorthEast(distance, _delay);
                        break;
                    case CursorDirection.SouthEast:
                        MouseCursorWrapper.MoveSouthEast(distance, _delay);
                        break;
                    case CursorDirection.SouthWest:
                        MouseCursorWrapper.MoveSouthWest(distance, _delay);
                        break;
                    case CursorDirection.NorthWest:
                        MouseCursorWrapper.MoveNorthWest(distance, _delay);
                        break;
                    case CursorDirection.UpAndDown:
                        MouseCursorWrapper.MoveNorth(distance, _delay);
                        MouseCursorWrapper.MoveSouth(distance, _delay);
                        break;
                    case CursorDirection.DownAndUp:
                        MouseCursorWrapper.MoveSouth(distance, _delay);
                        MouseCursorWrapper.MoveNorth(distance, _delay);
                        break;
                    case CursorDirection.LeftAndRight:
                        MouseCursorWrapper.MoveWest(distance, _delay);
                        MouseCursorWrapper.MoveEast(distance, _delay);
                        break;
                    case CursorDirection.RightAndLeft:
                        MouseCursorWrapper.MoveEast(distance, _delay);
                        MouseCursorWrapper.MoveWest(distance, _delay);
                        break;
                    case CursorDirection.Random:
                        int distanceRemaining = distance;
                        int lastDirection = 0;

                        while (distanceRemaining > 0)
                        {
                            const int maxDistance = 150;
                            int randomDistance = new Random(distanceRemaining).Next(1, (distanceRemaining < maxDistance) ? distanceRemaining : maxDistance);
                            distanceRemaining = distanceRemaining - randomDistance;
                            int direction;

                            do
                            {
                                direction = new Random().Next(1, 9);
                            } while (direction.Equals(lastDirection));

                            lastDirection = direction;

                            switch (direction)
                            {
                                case 1:
                                    MouseCursorWrapper.MoveNorth(randomDistance, _delay);
                                    break;
                                case 2:
                                    MouseCursorWrapper.MoveEast(randomDistance, _delay);
                                    break;
                                case 3:
                                    MouseCursorWrapper.MoveSouth(randomDistance, _delay);
                                    break;
                                case 4:
                                    MouseCursorWrapper.MoveWest(randomDistance, _delay);
                                    break;
                                case 5:
                                    MouseCursorWrapper.MoveNorthEast(randomDistance, _delay);
                                    break;
                                case 6:
                                    MouseCursorWrapper.MoveSouthEast(randomDistance, _delay);
                                    break;
                                case 7:
                                    MouseCursorWrapper.MoveSouthWest(randomDistance, _delay);
                                    break;
                                case 8:
                                    MouseCursorWrapper.MoveNorthWest(randomDistance, _delay);
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