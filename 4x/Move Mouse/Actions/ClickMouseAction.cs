using ellabi.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ellabi.Actions
{
    public class ClickMouseAction : ActionBase
    {
        private MouseButton _button;
        private bool _hold;
        private double _holdInterval;

        public enum MouseButton
        {
            Left,
            Middle,
            Right
        }

        public IEnumerable<MouseButton> MouseButtonValues => Enum.GetValues(typeof(MouseButton)).Cast<MouseButton>();

        public override bool IsValid => true;

        public MouseButton Button
        {
            get => _button;
            set
            {
                _button = value;
                OnPropertyChanged();
            }
        }

        public bool Hold
        {
            get => _hold;
            set
            {
                _hold = value;
                OnPropertyChanged();
            }
        }

        public double HoldInterval
        {
            get => _holdInterval;
            set
            {
                _holdInterval = value;
                OnPropertyChanged();
            }
        }

        public ClickMouseAction()
        {
            _button = MouseButton.Left;
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

                switch (_button)
                {
                    case MouseButton.Left:
                        NativeMethods.mouse_event((int)NativeMethods.MouseEventFlags.LEFTDOWN, 0, 0, 0, 0);
                        break;
                    case MouseButton.Middle:
                        NativeMethods.mouse_event((int)NativeMethods.MouseEventFlags.MIDDLEDOWN, 0, 0, 0, 0);
                        break;
                    case MouseButton.Right:
                        NativeMethods.mouse_event((int)NativeMethods.MouseEventFlags.RIGHTDOWN, 0, 0, 0, 0);
                        break;
                        //case MouseButton.WheelUp:
                        //    NativeMethods.mouse_event((int)NativeMethods.MouseEventFlags.WHEEL, 0, 0, Distance, 0);
                        //    break;
                }

                if (Hold)
                {
                    Thread.Sleep(Convert.ToInt32(1000 * HoldInterval));
                }

                switch (_button)
                {
                    case MouseButton.Left:
                        NativeMethods.mouse_event((int)NativeMethods.MouseEventFlags.LEFTUP, 0, 0, 0, 0);
                        break;
                    case MouseButton.Middle:
                        NativeMethods.mouse_event((int)NativeMethods.MouseEventFlags.MIDDLEUP, 0, 0, 0, 0);
                        break;
                    case MouseButton.Right:
                        NativeMethods.mouse_event((int)NativeMethods.MouseEventFlags.RIGHTUP, 0, 0, 0, 0);
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
            return $"{this.GetType().Name} | Name = {Name} | Button = {Button} | Hold = {Hold} | HoldInterval = {HoldInterval} | Trigger = {Trigger} | Repeat = {Repeat} | RepeatMode = {RepeatMode} | IntervalThrottle = {IntervalThrottle} | IntervalExecutionCount = {IntervalExecutionCount} | InterruptsIdleTime = {InterruptsIdleTime}";
        }
    }
}