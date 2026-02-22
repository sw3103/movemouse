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
        private bool _abortIfUserActivityDetected;

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

        public bool AbortIfUserActivityDetected
        {
            get => _abortIfUserActivityDetected;
            set
            {
                _abortIfUserActivityDetected = value;
                OnPropertyChanged();
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

        public override bool CanExecute() => IsValid;

        public override void Execute()
        {
            try
            {
                IntervalExecutionCount++;
                StaticCode.Logger?.Here().Information(ToString());
                int distance = Random ? new Random().Next(Convert.ToInt32(Distance), Convert.ToInt32(UpperDistance)) : Distance;
                var mcw = new MouseCursorWrapper() { BreakOnUserActivity = AbortIfUserActivityDetected };

                switch (Direction)
                {
                    case CursorDirection.Square:
                        mcw.MoveEast(distance, _delay);
                        if (!mcw.UserActivityDetected) mcw.MoveSouth(distance, _delay);
                        if (!mcw.UserActivityDetected) mcw.MoveWest(distance, _delay);
                        if (!mcw.UserActivityDetected) mcw.MoveNorth(distance, _delay);
                        break;
                    case CursorDirection.North:
                        mcw.MoveNorth(distance, _delay);
                        break;
                    case CursorDirection.East:
                        mcw.MoveEast(distance, _delay);
                        break;
                    case CursorDirection.South:
                        mcw.MoveSouth(distance, _delay);
                        break;
                    case CursorDirection.West:
                        mcw.MoveWest(distance, _delay);
                        break;
                    case CursorDirection.NorthEast:
                        mcw.MoveNorthEast(distance, _delay);
                        break;
                    case CursorDirection.SouthEast:
                        mcw.MoveSouthEast(distance, _delay);
                        break;
                    case CursorDirection.SouthWest:
                        mcw.MoveSouthWest(distance, _delay);
                        break;
                    case CursorDirection.NorthWest:
                        mcw.MoveNorthWest(distance, _delay);
                        break;
                    case CursorDirection.UpAndDown:
                        mcw.MoveNorth(distance, _delay);
                        if (!mcw.UserActivityDetected) mcw.MoveSouth(distance, _delay);
                        break;
                    case CursorDirection.DownAndUp:
                        mcw.MoveSouth(distance, _delay);
                        if (!mcw.UserActivityDetected) mcw.MoveNorth(distance, _delay);
                        break;
                    case CursorDirection.LeftAndRight:
                        mcw.MoveWest(distance, _delay);
                        if (!mcw.UserActivityDetected) mcw.MoveEast(distance, _delay);
                        break;
                    case CursorDirection.RightAndLeft:
                        mcw.MoveEast(distance, _delay);
                        if (!mcw.UserActivityDetected) mcw.MoveWest(distance, _delay);
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
                                    if (!mcw.UserActivityDetected) mcw.MoveNorth(randomDistance, _delay);
                                    break;
                                case 2:
                                    if (!mcw.UserActivityDetected) mcw.MoveEast(randomDistance, _delay);
                                    break;
                                case 3:
                                    if (!mcw.UserActivityDetected) mcw.MoveSouth(randomDistance, _delay);
                                    break;
                                case 4:
                                    if (!mcw.UserActivityDetected) mcw.MoveWest(randomDistance, _delay);
                                    break;
                                case 5:
                                    if (!mcw.UserActivityDetected) mcw.MoveNorthEast(randomDistance, _delay);
                                    break;
                                case 6:
                                    if (!mcw.UserActivityDetected) mcw.MoveSouthEast(randomDistance, _delay);
                                    break;
                                case 7:
                                    if (!mcw.UserActivityDetected) mcw.MoveSouthWest(randomDistance, _delay);
                                    break;
                                case 8:
                                    if (!mcw.UserActivityDetected) mcw.MoveNorthWest(randomDistance, _delay);
                                    break;
                            }
                        }

                        break;
                    case CursorDirection.None:
                        mcw.MoveFromCurrentLocation(new Point());
                        break;
                }

                Aborted = mcw.UserActivityDetected;
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