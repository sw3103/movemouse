using ellabi.Platform;
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
            _holdInterval = 0.1;
            InterruptsIdleTime = true;
        }

        public override bool CanExecute() => IsValid;

        public override void Execute()
        {
            try
            {
                IntervalExecutionCount++;
                StaticCode.Logger?.Here().Information(ToString());

                var btnType = _button switch
                {
                    MouseButton.Left => LinuxInputService.MouseButtonType.Left,
                    MouseButton.Middle => LinuxInputService.MouseButtonType.Middle,
                    MouseButton.Right => LinuxInputService.MouseButtonType.Right,
                    _ => LinuxInputService.MouseButtonType.Left
                };

                LinuxInputService.MouseButton(btnType, true);

                if (Hold && HoldInterval > 0)
                {
                    Thread.Sleep(Convert.ToInt32(1000 * HoldInterval));
                }

                LinuxInputService.MouseButton(btnType, false);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public override string ToString()
        {
            return $"{GetType().Name} | Name = {Name} | Button = {Button} | Hold = {Hold} | HoldInterval = {HoldInterval} | Trigger = {Trigger} | Repeat = {Repeat} | RepeatMode = {RepeatMode} | IntervalThrottle = {IntervalThrottle} | IntervalExecutionCount = {IntervalExecutionCount} | InterruptsIdleTime = {InterruptsIdleTime}";
        }
    }
}
