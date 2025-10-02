using ellabi.Actions;
using ellabi.Annotations;
using ellabi.Schedules;
using Serilog.Events;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Serialization;

namespace ellabi.Classes
{
    public class Settings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _lowerInterval = 30;
        private int _upperInterval = 60;
        private bool _randomInterval = false;
        private bool _autoPause = false;
        private bool _autoResume = false;
        private int _autoResumeSeconds = 30;
        private bool? _topmostWhenRunning;
        private bool? _hideFromTaskbar;
        private bool? _hideMainWindow;
        private bool? _hideSystemTrayIcon;
        private bool? _hideFromAltTab;
        private bool? _overrideWindowTitle;
        private string _windowTitle;
        private bool? _overrideIcon;
        private string _iconPath;
        private bool? _launchAtLogon;
        private bool? _startAtLaunch;
        private bool? _moveMouseHasBeenClicked;
        private bool? _actionsHaveBeenClicked;
        private ActionBase[] _actions;
        private ScheduleBase[] _schedules;
        private Blackout[] _blackouts;
        private int? _runningVolume;
        private bool? _adjustRunningVolume;
        private bool? _minimiseOnStop;
        private bool? _preventScreenBurn;
        private bool? _activeWhenLocked;
        private bool? _showMoveMouseStatus;
        private bool? _enableLogging;
        //private string _logPath;
        private bool? _disableButtonAnimation;
        //private bool? _hookKeyEnabled;
        //private Key _hookKey;
        private bool? _pauseOnBattery;
        private LogEventLevel? _logLevel;
        private bool? _showSystemTrayNotifications;
        //private bool? _reactivatePreviousWindow;
        private bool? _showTaskbarStatus;

        public int LowerInterval
        {
            get => _lowerInterval;
            set
            {
                if (value > UpperInterval)
                {
                    UpperInterval = value;
                }

                _lowerInterval = value < 0 ? 0 : value;
                OnPropertyChanged();
            }
        }

        public int UpperInterval
        {
            get => _upperInterval;
            set
            {
                if (value < LowerInterval)
                {
                    LowerInterval = value;
                }

                _upperInterval = value < 1 ? 1 : value;
                OnPropertyChanged();
            }
        }

        public bool RandomInterval
        {
            get => _randomInterval;
            set
            {
                _randomInterval = value;
                OnPropertyChanged();
            }
        }

        public bool AutoPause
        {
            get => _autoPause;
            set
            {
                _autoPause = value;
                OnPropertyChanged();
            }
        }

        public bool AutoResume
        {
            get => _autoResume;
            set
            {
                _autoResume = value;
                OnPropertyChanged();
            }
        }

        public int AutoResumeSeconds
        {
            get => _autoResumeSeconds;
            set
            {
                _autoResumeSeconds = value;
                OnPropertyChanged();
            }
        }

        public bool EnableLogging
        {
            get
            {
                if (_enableLogging == null) _enableLogging = false;
                return _enableLogging.Value;
            }
            set
            {
                _enableLogging = value;
                OnPropertyChanged();
            }
        }

        //public string LogPath
        //{
        //    get => _logPath;
        //    set
        //    {
        //        _logPath = value;
        //        OnPropertyChanged();
        //    }
        //}

        public bool TopmostWhenRunning
        {
            get
            {
                if (_topmostWhenRunning == null) _topmostWhenRunning = false;
                return _topmostWhenRunning.Value;
            }
            set
            {
                _topmostWhenRunning = value;
                OnPropertyChanged();
            }
        }

        public bool HideFromTaskbar
        {
            get
            {
                if (_hideFromTaskbar == null) _hideFromTaskbar = false;
                return _hideFromTaskbar.Value;
            }
            set
            {
                _hideFromTaskbar = value;

                if (!HideFromTaskbar)
                {
                    HideFromAltTab = false;
                }

                OnPropertyChanged();
            }
        }

        public bool HideFromAltTab
        {
            get
            {
                if (_hideFromAltTab == null) _hideFromAltTab = false;
                return _hideFromAltTab.Value;
            }
            set
            {
                _hideFromAltTab = value;

                if (HideFromAltTab)
                {
                    HideFromTaskbar = true;
                }

                OnPropertyChanged();
            }
        }

        public bool HideMainWindow
        {
            get
            {
                if (_hideMainWindow == null) _hideMainWindow = false;
                return _hideMainWindow.Value;
            }
            set
            {
                _hideMainWindow = value;
                OnPropertyChanged();
            }
        }

        public bool HideSystemTrayIcon
        {
            get
            {
                if (_hideSystemTrayIcon == null) _hideSystemTrayIcon = false;
                return _hideSystemTrayIcon.Value;
            }
            set
            {
                _hideSystemTrayIcon = value;
                OnPropertyChanged();
            }
        }

        public bool LaunchAtLogon
        {
            get
            {
                if (_launchAtLogon == null) _launchAtLogon = false;
                return _launchAtLogon.Value;
            }
            set
            {
                _launchAtLogon = value;
                OnPropertyChanged();
            }
        }

        public bool StartAtLaunch
        {
            get
            {
                if (_startAtLaunch == null) _startAtLaunch = false;
                return _startAtLaunch.Value;
            }
            set
            {
                _startAtLaunch = value;
                OnPropertyChanged();
            }
        }

        public bool MoveMouseHasBeenClicked
        {
            get
            {
                if (_moveMouseHasBeenClicked == null) _moveMouseHasBeenClicked = false;
                return _moveMouseHasBeenClicked.Value;
            }
            set
            {
                _moveMouseHasBeenClicked = value;
                OnPropertyChanged();
            }
        }

        public bool ActionsHaveBeenClicked
        {
            get
            {
                if (_actionsHaveBeenClicked == null) _actionsHaveBeenClicked = false;
                return _actionsHaveBeenClicked.Value;
            }
            set
            {
                _actionsHaveBeenClicked = value;
                OnPropertyChanged();
            }
        }

        public bool OverrideWindowTitle
        {
            get
            {
                if (_overrideWindowTitle == null) _overrideWindowTitle = false;
                return _overrideWindowTitle.Value;
            }
            set
            {
                _overrideWindowTitle = value;
                OnPropertyChanged();
            }
        }

        public string WindowTitle
        {
            get => _windowTitle;
            set
            {
                _windowTitle = value;
                OnPropertyChanged();
            }
        }

        public bool OverrideIcon
        {
            get
            {
                if (_overrideIcon == null) _overrideIcon = false;
                return _overrideIcon.Value;
            }
            set
            {
                _overrideIcon = value;
                OnPropertyChanged();
            }
        }

        public string IconPath
        {
            get => _iconPath;
            set
            {
                _iconPath = value;
                OnPropertyChanged();
            }
        }

        //public bool HookKeyEnabled
        //{
        //    get
        //    {
        //        if (_hookKeyEnabled == null) _hookKeyEnabled = false;
        //        return _hookKeyEnabled.Value;
        //    }
        //    set
        //    {
        //        _hookKeyEnabled = value;
        //        OnPropertyChanged();
        //    }
        //}

        //public Key HookKey
        //{
        //    get { return _hookKey; }
        //    set
        //    {
        //        _hookKey = value;
        //        OnPropertyChanged();
        //    }
        //}

        public int RunningVolume
        {
            get
            {
                if (_runningVolume == null) _runningVolume = 30;
                return _runningVolume.Value;
            }
            set
            {
                _runningVolume = value;
                OnPropertyChanged();
            }
        }

        public bool AdjustRunningVolume
        {
            get
            {
                if (_adjustRunningVolume == null) _adjustRunningVolume = false;
                return _adjustRunningVolume.Value;
            }
            set
            {
                _adjustRunningVolume = value;
                OnPropertyChanged();
            }
        }

        public bool MinimiseOnStop
        {
            get
            {
                if (_minimiseOnStop == null) _minimiseOnStop = false;
                return _minimiseOnStop.Value;
            }
            set
            {
                _minimiseOnStop = value;
                OnPropertyChanged();
            }
        }

        public bool PreventScreenBurn
        {
            get
            {
                if (_preventScreenBurn == null) _preventScreenBurn = false;
                return _preventScreenBurn.Value;
            }
            set
            {
                _preventScreenBurn = value;
                OnPropertyChanged();
            }
        }

        public bool ActiveWhenLocked
        {
            get
            {
                if (_activeWhenLocked == null) _activeWhenLocked = false;
                return _activeWhenLocked.Value;
            }
            set
            {
                _activeWhenLocked = value;
                OnPropertyChanged();
            }
        }

        public bool ShowMoveMouseStatus
        {
            get
            {
                if (_showMoveMouseStatus == null) _showMoveMouseStatus = false;
                return _showMoveMouseStatus.Value;
            }
            set
            {
                _showMoveMouseStatus = value;
                OnPropertyChanged();
            }
        }

        public bool DisableButtonAnimation
        {
            get
            {
                if (_disableButtonAnimation == null) _disableButtonAnimation = false;
                return _disableButtonAnimation.Value;
            }
            set
            {
                _disableButtonAnimation = value;
                OnPropertyChanged();
            }
        }

        public bool PauseOnBattery
        {
            get
            {
                if (_pauseOnBattery == null) _pauseOnBattery = false;
                return _pauseOnBattery.Value;
            }
            set
            {
                _pauseOnBattery = value;
                OnPropertyChanged();
            }
        }

        public LogEventLevel LogLevel
        {
            get
            {
                if (_logLevel == null) _logLevel = LogEventLevel.Verbose;
                return _logLevel.Value;
            }
            set
            {
                _logLevel = value;
                OnPropertyChanged();
            }
        }

        public bool ShowSystemTrayNotifications
        {
            get
            {
                if (_showSystemTrayNotifications == null) _showSystemTrayNotifications = false;
                return _showSystemTrayNotifications.Value;
            }
            set
            {
                _showSystemTrayNotifications = value;
                OnPropertyChanged();
            }
        }

        //public bool ReactivatePreviousWindow
        //{
        //    get
        //    {
        //        if (_reactivatePreviousWindow == null) _reactivatePreviousWindow = false;
        //        return _reactivatePreviousWindow.Value;
        //    }
        //    set
        //    {
        //        _reactivatePreviousWindow = value;
        //        OnPropertyChanged();
        //    }
        //}

        public bool ShowTaskbarStatus
        {
            get
            {
                if (_showTaskbarStatus == null) _showTaskbarStatus = true;
                return _showTaskbarStatus.Value;
            }
            set
            {
                _showTaskbarStatus = value;
                OnPropertyChanged();
            }
        }

        [XmlArrayItem(Type = typeof(ActionBase)),
         XmlArrayItem(Type = typeof(MoveMouseCursorAction)),
         XmlArrayItem(Type = typeof(ClickMouseAction)),
         XmlArrayItem(Type = typeof(ScriptAction)),
         XmlArrayItem(Type = typeof(PositionMouseCursorAction)),
         XmlArrayItem(Type = typeof(ActivateApplicationAction)),
         XmlArrayItem(Type = typeof(CommandAction)),
         XmlArrayItem(Type = typeof(ScrollMouseAction)),
         XmlArrayItem(Type = typeof(SleepAction))]
        public ActionBase[] Actions
        {
            get
            {
                if ((_actions == null) || (!_actions.Any()))
                {
                    var action = new MoveMouseCursorAction();
                    action.PropertyChanged += Action_PropertyChanged;
                    _actions = new ActionBase[] { action };
                    OnPropertyChanged();
                }

                return _actions;
            }
            set
            {
                if ((_actions != null) && _actions.Any())
                {
                    foreach (var action in _actions)
                    {
                        action.PropertyChanged -= Action_PropertyChanged;
                    }
                }

                _actions = value;

                if ((_actions != null) && _actions.Any())
                {
                    foreach (var action in _actions)
                    {
                        action.PropertyChanged += Action_PropertyChanged;
                    }
                }

                OnPropertyChanged();
            }
        }

        [XmlArrayItem(Type = typeof(ScheduleBase)),
         XmlArrayItem(Type = typeof(SimpleSchedule)),
         XmlArrayItem(Type = typeof(AdvancedSchedule))]
        public ScheduleBase[] Schedules
        {
            get => _schedules;
            set
            {
                if ((_schedules != null) && _schedules.Any())
                {
                    foreach (var schedule in _schedules)
                    {
                        schedule.PropertyChanged -= Schedule_PropertyChanged;
                    }
                }

                _schedules = value;

                if ((_schedules != null) && _schedules.Any())
                {
                    foreach (var schedule in _schedules)
                    {
                        schedule.PropertyChanged += Schedule_PropertyChanged;
                    }
                }

                OnPropertyChanged();
            }
        }

        public Blackout[] Blackouts
        {
            get => _blackouts;
            set
            {
                if ((_blackouts != null) && _blackouts.Any())
                {
                    foreach (var blackout in _blackouts)
                    {
                        blackout.PropertyChanged -= Blackout_PropertyChanged;
                    }
                }

                _blackouts = value;

                if ((_blackouts != null) && _blackouts.Any())
                {
                    foreach (var blackout in _blackouts)
                    {
                        blackout.PropertyChanged += Blackout_PropertyChanged;
                    }
                }

                OnPropertyChanged();
            }
        }

        private void Action_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(e.PropertyName);
            OnPropertyChanged(nameof(Actions));
        }

        private void Schedule_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(e.PropertyName);
            if (e?.PropertyName != "IsValid") OnPropertyChanged(nameof(Schedules));
        }

        private void Blackout_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(e.PropertyName);
            if (e?.PropertyName != "IsValid") OnPropertyChanged(nameof(Blackouts));
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            try
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }
    }
}