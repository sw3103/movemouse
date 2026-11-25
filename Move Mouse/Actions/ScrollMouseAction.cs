using ellabi.Platform;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ellabi.Actions
{
    public class ScrollMouseAction : ActionBase
    {
        private uint _distance;
        private uint _upperDistance;
        private bool _random;
        private WheelDirection _direction;

        public enum WheelDirection
        {
            Up,
            Down,
            Left,
            Right,
            Random
        }

        public IEnumerable<WheelDirection> WheelDirectionValues => Enum.GetValues(typeof(WheelDirection)).Cast<WheelDirection>();

        public override bool IsValid => Distance > 0;

        public uint Distance
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

        public uint UpperDistance
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

        public WheelDirection Direction
        {
            get => _direction;
            set
            {
                _direction = value;
                OnPropertyChanged();
            }
        }

        public ScrollMouseAction()
        {
            _distance = 100;
            _upperDistance = 200;
            _direction = WheelDirection.Down;
            InterruptsIdleTime = true;
        }

        public override bool CanExecute() => IsValid;

        public override void Execute()
        {
            try
            {
                IntervalExecutionCount++;
                StaticCode.Logger?.Here().Information(ToString());

                var direction = (Direction == WheelDirection.Random)
                    ? WheelDirectionValues.Where(wd => wd != WheelDirection.Random).OrderBy(_ => Guid.NewGuid()).FirstOrDefault()
                    : Direction;

                uint distance = Random
                    ? (uint)System.Random.Shared.Next(Convert.ToInt32(Distance), Convert.ToInt32(UpperDistance) + 1)
                    : Distance;

                var scrollDir = direction switch
                {
                    WheelDirection.Up => LinuxInputService.ScrollDirectionType.Up,
                    WheelDirection.Down => LinuxInputService.ScrollDirectionType.Down,
                    WheelDirection.Left => LinuxInputService.ScrollDirectionType.Left,
                    WheelDirection.Right => LinuxInputService.ScrollDirectionType.Right,
                    _ => LinuxInputService.ScrollDirectionType.Down
                };

                LinuxInputService.Scroll(scrollDir, distance);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public override string ToString()
        {
            return $"{GetType().Name} | Name = {Name} | Distance = {Distance} | Direction = {Direction} | Trigger = {Trigger} | Repeat = {Repeat} | RepeatMode = {RepeatMode} | IntervalThrottle = {IntervalThrottle} | IntervalExecutionCount = {IntervalExecutionCount} | InterruptsIdleTime = {InterruptsIdleTime}";
        }
    }
}
