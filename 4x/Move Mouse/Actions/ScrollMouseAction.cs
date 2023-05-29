using ellabi.Wrappers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ellabi.Actions
{
    public class ScrollMouseAction : ActionBase
    {
        private uint _distance;
        private WheelDirection _direction;

        public enum WheelDirection
        {
            Up,
            Down,
            Left,
            Right
        }

        public IEnumerable<WheelDirection> WheelDirectionValues => Enum.GetValues(typeof(WheelDirection)).Cast<WheelDirection>();

        public override bool IsValid => Distance > 0;

        public uint Distance
        {
            get => _distance;
            set
            {
                _distance = value;
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
            _direction = WheelDirection.Down;
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
                StaticCode.Logger?.Here().Information(ToString());

                switch (_direction)
                {
                    case WheelDirection.Up:
                        {
                            NativeMethods.mouse_event((int)NativeMethods.MouseEventFlags.WHEEL, 0, 0, Distance, 0);
                            break;
                        }
                    case WheelDirection.Down:
                        {
                            NativeMethods.mouse_event((int)NativeMethods.MouseEventFlags.WHEEL, 0, 0, (uint)(Distance * -1), 0);
                            break;
                        }
                    case WheelDirection.Left:
                        {
                            NativeMethods.mouse_event((int)NativeMethods.MouseEventFlags.HWHEEL, 0, 0, Distance, 0);
                            break;
                        }
                    case WheelDirection.Right:
                        {
                            NativeMethods.mouse_event((int)NativeMethods.MouseEventFlags.HWHEEL, 0, 0, (uint)(Distance * -1), 0);
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public override string ToString()
        {
            return $"{this.GetType().Name} | Name = {Name} | Distance = {Distance} | Direction = {Direction} | Trigger = {Trigger} | Repeat = {Repeat} | RepeatMode = {RepeatMode} | IntervalThrottle = {IntervalThrottle} | IntervalExecutionCount = {IntervalExecutionCount} | InterruptsIdleTime = {InterruptsIdleTime}";
        }
    }
}