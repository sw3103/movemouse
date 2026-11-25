using ellabi.Platform;
using ellabi.Utilities;
using ellabi.Wrappers;
using System;
using System.Timers;
using System.Xml.Serialization;

namespace ellabi.Actions
{
    public class PositionMouseCursorAction : ActionBase
    {
        private int _x;
        private int _y;
        private TimeSpan _cursorTrackingTimeRemaining;
        private readonly TimeSpan _cursorTrackingTime = TimeSpan.FromSeconds(3);
        private Timer _cursorTrackingTimer;
        private bool _cursorTrackingEnabled;

        public override bool IsValid => true;

        public int X
        {
            get => _x;
            set
            {
                _x = value;
                OnPropertyChanged();
            }
        }

        public int Y
        {
            get => _y;
            set
            {
                _y = value;
                OnPropertyChanged();
            }
        }

        [XmlIgnore]
        public RelayCommand StartCursorTrackingCommand { get; set; }

        [XmlIgnore]
        public TimeSpan CursorTrackingTimeRemaining
        {
            get => _cursorTrackingTimeRemaining;
            set
            {
                _cursorTrackingTimeRemaining = value;
                OnPropertyChanged();
            }
        }

        [XmlIgnore]
        public bool CursorTrackingEnabled
        {
            get => _cursorTrackingEnabled;
            set
            {
                _cursorTrackingEnabled = value;
                OnPropertyChanged();
            }
        }

        public PositionMouseCursorAction()
        {
            StartCursorTrackingCommand = new RelayCommand(param => StartCursorTracking());
        }

        public override bool CanExecute() => IsValid;

        public override void Execute()
        {
            try
            {
                IntervalExecutionCount++;
                StaticCode.Logger?.Here().Information(ToString());
                LinuxInputService.SetCursorPosition(X, Y);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public void StartCursorTracking()
        {
            try
            {
                _cursorTrackingTimer?.Stop();
                _cursorTrackingTimer?.Dispose();
                CursorTrackingEnabled = true;
                CursorTrackingTimeRemaining = _cursorTrackingTime;
                var startWait = DateTime.UtcNow;
                _cursorTrackingTimer = new Timer { Interval = 100 };
                _cursorTrackingTimer.Elapsed += (s, e) =>
                {
                    try
                    {
                        var lastInput = StaticCode.GetLastInputTime();
                        var elapsed = lastInput > TimeSpan.Zero ? lastInput : (DateTime.UtcNow - startWait);
                        CursorTrackingTimeRemaining = _cursorTrackingTime.Subtract(elapsed);

                        if (CursorTrackingTimeRemaining.TotalMilliseconds <= 0)
                        {
                            _cursorTrackingTimer?.Stop();
                            CursorTrackingEnabled = false;
                        }
                        else
                        {
                            var position = new MouseCursorWrapper().GetCursorPosition();
                            X = position.X;
                            Y = position.Y;
                        }
                    }
                    catch (Exception ex)
                    {
                        StaticCode.Logger?.Here().Error(ex.Message);
                    }
                };
                _cursorTrackingTimer.Start();
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public override string ToString()
        {
            return $"{GetType().Name} | Name = {Name} | X = {X} | Y = {Y} | Trigger = {Trigger} | Repeat = {Repeat} | RepeatMode = {RepeatMode} | IntervalThrottle = {IntervalThrottle} | IntervalExecutionCount = {IntervalExecutionCount} | InterruptsIdleTime = {InterruptsIdleTime}";
        }
    }
}
