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

        public override bool CanExecute()
        {
            return IsValid;
        }

        public override void Execute()
        {
            try
            {
                StaticCode.Logger?.Here().Information(ToString());
                NativeMethods.SetCursorPos(X, Y);
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
                CursorTrackingEnabled = true;
                _cursorTrackingTimer = new Timer { Interval = 100 };
                _cursorTrackingTimer.Elapsed += _cursorTrackingTimer_Elapsed;
                _cursorTrackingTimer.Start();
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void _cursorTrackingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                CursorTrackingTimeRemaining = _cursorTrackingTime.Subtract(StaticCode.GetLastInputTime());

                if (CursorTrackingTimeRemaining.TotalMilliseconds <= 0)
                {
                    _cursorTrackingTimer.Stop();
                    CursorTrackingEnabled = false;
                }
                else
                {
                    var w32Mouse = new NativeMethods.Win32Point();
                    NativeMethods.GetCursorPos(ref w32Mouse);
                    X = w32Mouse.X;
                    Y = w32Mouse.Y;
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public override string ToString()
        {
            return $"Name = {Name} | X = {X} | Y = {Y} | Trigger = {Trigger} | Repeat = {Repeat}";
        }
    }
}