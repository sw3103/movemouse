using AudioSwitcher.AudioApi.CoreAudio;
using ellabi.Actions;
using ellabi.Annotations;
using ellabi.Jobs;
using ellabi.Schedules;
using ellabi.Utilities;
using Microsoft.Win32;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ellabi.ViewModels
{
    internal class MouseWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        public delegate void MouseStateChangedHandler(object sender, MouseState state);

        public delegate void AltTabVisibilityChangedHandler(object sender, bool visible);

        public delegate void RequestActivateHandler(object sender);

        public delegate void RequestMinimiseHandler(object sender);

        //public delegate void HookKeyEnabledChangedHandler(object sender, bool enabled, Key key);

        public event PropertyChangedEventHandler PropertyChanged;
        public event MouseStateChangedHandler MouseStateChanged;
        public event AltTabVisibilityChangedHandler AltTabVisibilityChanged;
        public event RequestActivateHandler RequestActivate;
        public event RequestMinimiseHandler RequestMinimise;
        //public event HookKeyEnabledChangedHandler HookKeyEnabledChanged;

        private MouseState _currentState;
        private DateTime _executionTime;
        private Timer _actionTimer;
        private SettingsWindowViewModel _settingsVm;
        private System.Timers.Timer _autoPauseTimer;
        private System.Timers.Timer _autoResumeTimer;
        private bool _firstPass = true;
        private readonly TimeSpan _minTimeBetweenStopStartToggle = TimeSpan.FromMilliseconds(500);
        private DateTime _lastStopStartToggleTime = DateTime.MinValue;
        private StdSchedulerFactory _schedulerFactory;
        private IScheduler _scheduler;
        //private DateTime _lastSchedulesUpdateTime = new DateTime(1980, 3, 31);
        //private readonly TimeSpan _schedulesUpdateDelay = TimeSpan.FromSeconds(1);
        //private BitmapImage _themeImage;
        private System.Timers.Timer _blackoutTimer;
        private bool _workstationLocked;
        private bool _forceSystrayIconVisibility;
        private double? _initialVolume;
        private CoreAudioDevice _defaultPlaybackDevice;
        private Guid _activeExecutionId;
        private object _lock = new object();
        private bool _updateAvailable;
        //private DateTime _startTime;

        public enum MouseState
        {
            Idle,
            Running,
            Paused,
            Executing,
            Sleeping,
            OnBattery
        }

        public SettingsWindowViewModel SettingsVm => _settingsVm ?? (_settingsVm = new SettingsWindowViewModel());

        public MouseState CurrentState
        {
            get => _currentState;
            set
            {
                _currentState = value;
                StaticCode.Logger?.Here().Information(_currentState.ToString());
                OnPropertyChanged();
                OnMouseStateChanged(this, _currentState);
            }
        }

        public RelayCommand StartStopToggleCommand { get; }

        public RelayCommand ShowMoveMouseCommand { get; }

        public DateTime ExecutionTime
        {
            get => _executionTime;
            set
            {
                _executionTime = value;
                StaticCode.Logger?.Here().Information(_executionTime.ToString("HH:mm:ss"));
                OnPropertyChanged();
            }
        }

        //public BitmapImage ThemeImage
        //{
        //    get => _themeImage;
        //    set
        //    {
        //        _themeImage = value;
        //        OnPropertyChanged();
        //    }
        //}

        public bool UpdateAvailable
        {
            get => _updateAvailable;
            set
            {
                _updateAvailable = value;
                OnPropertyChanged();
            }
        }

        public bool ForceSystrayIconVisibility
        {
            get => _forceSystrayIconVisibility;
            set
            {
                _forceSystrayIconVisibility = value;
                StaticCode.Logger?.Here().Information(_forceSystrayIconVisibility.ToString());
                OnPropertyChanged();
            }
        }

        public MouseWindowViewModel()
        {
            //StaticCode.ActivityLog.LogActivity("Move Mouse launched");
            StartStopToggleCommand = new RelayCommand(param => StartStopToggle(), param => CanStartStopToggle());
            ShowMoveMouseCommand = new RelayCommand(param => OnRequestActivate(this));
            CurrentState = MouseState.Idle;
            SettingsVm.Settings.PropertyChanged += Settings_PropertyChanged;
            StaticCode.ScheduleArrived += StaticCode_ScheduleArrived;
            //StaticCode.ThemeUpdated += StaticCode_ThemeUpdated;
            if (StaticCode.DownloadSource == StaticCode.MoveMouseSource.GitHub) StaticCode.UpdateAvailablityChanged += StaticCode_UpdateAvailablityChanged;
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
            ThreadPool.QueueUserWorkItem(ForceSystrayIconVisibilityAtLaunch);
            ScheduleJobs();
            //Debug.WriteLine(SystemInformation.PowerStatus.BatteryChargeStatus);
        }

        private void ForceSystrayIconVisibilityAtLaunch(object stateInfo)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                ForceSystrayIconVisibility = true;
                Thread.Sleep(TimeSpan.FromSeconds(10));
                ForceSystrayIconVisibility = false;
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void StaticCode_UpdateAvailablityChanged(bool updateAvailable)
        {
            UpdateAvailable = updateAvailable;
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            try
            {
                StaticCode.Logger?.Here().Debug(e.Reason.ToString());

                switch (e.Reason)
                {
                    case SessionSwitchReason.SessionUnlock:
                        _workstationLocked = false;
                        break;
                    case SessionSwitchReason.SessionLock:
                        _workstationLocked = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        //private void StaticCode_ThemeUpdated(Theme theme)
        //{
        //    try
        //    {
        //        System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
        //        {
        //            ThemeImage = (theme != null) ? new BitmapImage(new Uri(theme.ImagePath, UriKind.Absolute)) : null;
        //        }));
        //    }
        //    catch (Exception ex)
        //    {
        //        StaticCode.Logger?.Here().Error(ex.Message);
        //    }
        //}

        private void StaticCode_ScheduleArrived(ScheduleBase.ScheduleAction action)
        {
            try
            {
                StaticCode.Logger?.Here().Debug(action.ToString());

                switch (action)
                {
                    case ScheduleBase.ScheduleAction.Start:
                        Start();
                        break;
                    case ScheduleBase.ScheduleAction.Stop:
                        Stop(MouseState.Idle);
                        break;
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void ScheduleJobs()
        {
            ThreadPool.QueueUserWorkItem(ScheduleJobsThread);
        }

        private async void ScheduleJobsThread(object stateInfo)
        {
            using (Mutex myMutex = new Mutex(false, "ScheduleJobsThread", out var owned))
            {
                if (owned)
                {
                    try
                    {
                        //_lastSchedulesUpdateTime = DateTime.Now;

                        //if (_lastSchedulesUpdateTime.Add(_schedulesUpdateDelay) > DateTime.Now)
                        //{
                        CleanupJobs();

                        //while (_lastSchedulesUpdateTime.Add(_schedulesUpdateDelay) > DateTime.Now)
                        //{
                        //    Thread.Sleep(100);
                        //}

                        _schedulerFactory = new StdSchedulerFactory();
                        _scheduler = await _schedulerFactory.GetScheduler();

                        if ((SettingsVm.Settings.Schedules != null) && SettingsVm.Settings.Schedules.Any(schedule => schedule.IsValid))
                        {
                            foreach (var schedule in SettingsVm.Settings.Schedules.Where(schedule => schedule.IsValid))
                            {
                                try
                                {
                                    var job = JobBuilder.Create<ScheduleArrivedJob>()
                                        .WithIdentity(schedule.Id.ToString())
                                        .UsingJobData("action", schedule.Action.ToString())
                                        .Build();
                                    var trigger = new CronTriggerImpl(schedule.Id.ToString(), null, schedule.CronExpression);
                                    StaticCode.Logger?.Here().Information($"{schedule.Action} schedule created ({schedule.CronExpression})");
                                    await _scheduler.ScheduleJob(job, trigger);
                                }
                                catch (Exception ex)
                                {
                                    StaticCode.Logger?.Here().Error(ex.Message);
                                }
                            }
                        }

                        //var updateThemeJob = JobBuilder.Create<UpdateThemeJob>()
                        //    .WithIdentity("UpdateThemeJob")
                        //    .Build();
                        //var themeUpdateTime = TimeSpan.FromSeconds(new Random().Next(0, 3600));
                        //_scheduler.ScheduleJob(updateThemeJob, new CronTriggerImpl("UpdateThemeJob", null, $"{themeUpdateTime.Seconds} {themeUpdateTime.Minutes} {themeUpdateTime.Hours} ? * *"));
                        ////For testing aggressive theme updates
                        ////_scheduler.ScheduleJob(updateThemeJob, new CronTriggerImpl("UpdateThemeJob", null, $"0 0/1 * ? * *"));
                        //_scheduler.TriggerJob(new JobKey("UpdateThemeJob"));

                        if (StaticCode.DownloadSource == StaticCode.MoveMouseSource.GitHub)
                        {
                            var resetUpdateStatus = JobBuilder.Create<ResetUpdateStatusJob>()
                                .WithIdentity("ResetUpdateStatusJob")
                                .Build();
                            await _scheduler.ScheduleJob(resetUpdateStatus, new CronTriggerImpl("ResetUpdateStatusJob", null, "0 0 0 ? * *"));
                            await _scheduler.TriggerJob(new JobKey("ResetUpdateStatusJob"));
                            var checkForUpdateTime = TimeSpan.FromHours(10).Add(TimeSpan.FromSeconds(new Random().Next(0, 21600)));
                            var checkForUpdateJob = JobBuilder.Create<CheckForUpdateJob>()
                                .WithIdentity("CheckForUpdateJob")
                                .Build();
                            await _scheduler.ScheduleJob(checkForUpdateJob, new CronTriggerImpl("CheckForUpdateJob", null, $"{checkForUpdateTime.Seconds} {checkForUpdateTime.Minutes} {checkForUpdateTime.Hours} ? * *"));
                            await _scheduler.TriggerJob(new JobKey("CheckForUpdateJob"));
                        }

                        await _scheduler.Start();
                        //}
                    }
                    catch (Exception ex)
                    {
                        StaticCode.Logger?.Here().Error(ex.Message);
                    }
                }
                else
                {
                    StaticCode.Logger?.Here().Warning("Mutex already owned.");
                }
            }
        }

        private void CleanupJobs()
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                if ((_scheduler != null) && !_scheduler.IsShutdown)
                {
                    _scheduler?.Clear();
                    _scheduler?.Shutdown();
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(e.PropertyName);

            try
            {
                switch (e.PropertyName)
                {
                    case "AutoPause":
                        {
                            if (!SettingsVm.Settings.AutoPause) StopAutoPauseTimer();
                            break;
                        }
                    case "AutoResume":
                        {
                            if (!SettingsVm.Settings.AutoResume)
                            {
                                if (CurrentState.Equals(MouseState.Paused))
                                {
                                    Stop(MouseState.Idle);
                                }
                                else
                                {
                                    StopAutoResumeTimer();
                                }
                            }
                            break;
                        }
                    case "Schedules":
                        {
                            ScheduleJobs();
                            break;
                        }
                    case "HideFromAltTab":
                        {
                            OnAltTabVisibilityChanged(this, !SettingsVm.Settings.HideFromAltTab);
                            break;
                        }
                        //case "HookKeyEnabled":
                        //{
                        //        //OnHookKeyEnabledChanged(this, SettingsVm.Settings.HookKeyEnabled, SettingsVm.Settings.HookKey);
                        //        OnHookKeyEnabledChanged(this, SettingsVm.Settings.HookKeyEnabled, Key.S);
                        //        break;
                        //}
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public void StartStopToggle()
        {
            StaticCode.Logger?.Here().Information(String.Empty);

            try
            {
                if (DateTime.Now.Subtract(_lastStopStartToggleTime) > _minTimeBetweenStopStartToggle)
                {
                    if (CurrentState.Equals(MouseState.Idle))
                    {
                        Start();
                    }
                    else
                    {
                        Stop(MouseState.Idle);
                    }
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private bool CanStartStopToggle()
        {
            return true;
            //return !CurrentState.Equals(MouseState.Executing);
        }

        public void Start()
        {
            StaticCode.Logger?.Here().Information(String.Empty);

            try
            {
                if (!SettingsVm.Settings.MoveMouseHasBeenClicked) SettingsVm.Settings.MoveMouseHasBeenClicked = true;

                if (CurrentState != MouseState.Running)
                {
                    if (!BlackoutIsActive())
                    {
                        StopAutoResumeTimer();
                        _activeExecutionId = Guid.NewGuid();
                        _lastStopStartToggleTime = DateTime.Now;
                        PerformActions(ActionBase.EventTrigger.Start);
                        double interval = SettingsVm.Settings.RandomInterval ? new Random().Next((SettingsVm.Settings.LowerInterval * 1000), (SettingsVm.Settings.UpperInterval * 1000)) : (SettingsVm.Settings.LowerInterval * 1000);
                        interval = interval > 0 ? interval : 1;
                        ExecutionTime = DateTime.Now.AddMilliseconds(interval);
                        CurrentState = MouseState.Running;
                        _actionTimer = new Timer(param => { PerformActions(ActionBase.EventTrigger.Interval); }, null, TimeSpan.FromMilliseconds(interval), Timeout.InfiniteTimeSpan);
                        StopBlackoutTimer();
                        AdjustVolume();
                        if (_settingsVm.Settings.TopmostWhenRunning) OnRequestActivate(this);
                        StartAutoPauseTimer();
                    }
                    else
                    {
                        CurrentState = MouseState.Sleeping;
                        StartBlackoutTimer();
                    }
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public void Stop(MouseState state)
        {
            StaticCode.Logger?.Here().Information(state.ToString());

            try
            {
                if (CurrentState != state)
                {
                    var previousState = CurrentState;
                    _lastStopStartToggleTime = DateTime.Now;
                    _activeExecutionId = Guid.NewGuid();
                    StopAutoPauseTimer();
                    StopAutoResumeTimer();
                    StopBlackoutTimer();
                    RestoreVolume();
                    _actionTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                    _firstPass = true;
                    CurrentState = state;

                    if (previousState == MouseState.Running)
                    {
                        PerformActions(ActionBase.EventTrigger.Stop);
                        if (_settingsVm.Settings.MinimiseOnStop) OnRequestMinimise(this);
                    }

                    if (CurrentState.Equals(MouseState.Paused))
                    {
                        StartAutoResumeTimer();
                    }
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private async void AdjustVolume()
        {
            try
            {
                if (SettingsVm.Settings.AdjustRunningVolume)
                {
                    if (_defaultPlaybackDevice == null)
                    {
                        _defaultPlaybackDevice = new CoreAudioController().DefaultPlaybackDevice;
                    }

                    if (!_initialVolume.HasValue)
                    {
                        _initialVolume = _defaultPlaybackDevice.Volume;
                    }

                    StaticCode.Logger?.Here().Information(SettingsVm.Settings.RunningVolume.ToString());
                    //_defaultPlaybackDevice.Volume = SettingsVm.Settings.RunningVolume;
                    await _defaultPlaybackDevice.SetVolumeAsync(SettingsVm.Settings.RunningVolume);
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private async void RestoreVolume()
        {
            try
            {
                if (SettingsVm.Settings.AdjustRunningVolume && _initialVolume.HasValue)
                {
                    if (_defaultPlaybackDevice == null)
                    {
                        _defaultPlaybackDevice = new CoreAudioController().DefaultPlaybackDevice;
                    }

                    StaticCode.Logger?.Here().Information(_initialVolume.Value.ToString());
                    //_defaultPlaybackDevice.Volume = _initialVolume.Value;
                    await _defaultPlaybackDevice.SetVolumeAsync(_initialVolume.Value);
                    _initialVolume = null;
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void PerformActions(ActionBase.EventTrigger trigger)
        {
            StaticCode.Logger?.Here().Information(trigger.ToString());
            StaticCode.Logger?.Here().Information($"_firstPass = {_firstPass}");

            try
            {
                lock (_lock)
                {
                    var executionId = _activeExecutionId;

                    switch (trigger)
                    {
                        case ActionBase.EventTrigger.Start:
                            if (_firstPass)
                            {
                                if (SettingsVm.Settings.Actions.Any(action => action.IsValid && action.IsEnabled && action.Trigger.Equals(trigger)))
                                {
                                    var actions = SettingsVm.Settings.Actions.Where(action => action.IsValid && action.IsEnabled && action.Trigger.Equals(trigger));

                                    foreach (var action in actions)
                                    {
                                        if (_activeExecutionId.Equals(executionId))
                                        {
                                            action.Execute();
                                        }
                                    }
                                }
                            }

                            break;
                        case ActionBase.EventTrigger.Interval:
                            if (((SettingsVm.Settings.ActiveWhenLocked.HasValue && SettingsVm.Settings.ActiveWhenLocked.Value) || !_workstationLocked) && !BlackoutIsActive())
                            {
                                CurrentState = MouseState.Executing;
                                StopAutoPauseTimer();

                                if (SettingsVm.Settings.Actions.Any(action => action.IsValid && action.IsEnabled && action.Trigger.Equals(trigger)))
                                {
                                    var actions = _firstPass ? SettingsVm.Settings.Actions.Where(action => action.IsValid && action.IsEnabled && action.Trigger.Equals(trigger)) : SettingsVm.Settings.Actions.Where(action => action.IsValid && action.IsEnabled && action.Trigger.Equals(trigger) && action.Repeat);

                                    foreach (var action in actions)
                                    {
                                        if (_activeExecutionId.Equals(executionId))
                                        {
                                            action.Execute();
                                        }
                                    }
                                }

                                _firstPass = false;
                            }
                            else
                            {
                                CurrentState = MouseState.Sleeping;
                            }

                            if (_activeExecutionId.Equals(executionId) && (CurrentState.Equals(MouseState.Sleeping) || (!_firstPass && SettingsVm.Settings.Actions.Any(action => action.IsValid && action.IsEnabled && action.Trigger.Equals(trigger) && action.Repeat))))
                            {
                                Start();
                            }
                            else
                            {
                                Stop(MouseState.Idle);
                            }

                            break;
                        case ActionBase.EventTrigger.Stop:
                            if (SettingsVm.Settings.Actions.Any(action => action.IsValid && action.IsEnabled && action.Trigger.Equals(trigger)))
                            {
                                var actions = SettingsVm.Settings.Actions.Where(action => action.IsValid && action.IsEnabled && action.Trigger.Equals(trigger));

                                foreach (var action in actions)
                                {
                                    if (_activeExecutionId.Equals(executionId))
                                    {
                                        action.Execute();
                                    }
                                }
                            }

                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void StartAutoPauseTimer()
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                StopAutoPauseTimer();

                if (SettingsVm.Settings.AutoPause)
                {
                    if (_autoPauseTimer == null)
                    {
                        _autoPauseTimer = new System.Timers.Timer
                        {
                            Interval = 250
                        };
                        _autoPauseTimer.Elapsed += _autoPauseTimer_Elapsed;
                    }

                    _autoPauseTimer.Start();
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void StopAutoPauseTimer()
        {
            StaticCode.Logger?.Here().Debug(String.Empty);
            _autoPauseTimer?.Stop();
        }

        private void _autoPauseTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                StaticCode.Logger?.Here().Debug(StaticCode.GetLastInputTime().ToString());

                if (StaticCode.GetLastInputTime() < TimeSpan.FromMilliseconds(_autoPauseTimer.Interval))
                {
                    Stop((SettingsVm.Settings.AutoPause && SettingsVm.Settings.AutoResume) ? MouseState.Paused : MouseState.Idle);
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void StartAutoResumeTimer()
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                StopAutoResumeTimer();

                if (_autoResumeTimer == null)
                {
                    _autoResumeTimer = new System.Timers.Timer
                    {
                        Interval = 250
                    };
                    _autoResumeTimer.Elapsed += _autoResumeTimer_Elapsed;
                }

                _autoResumeTimer.Start();
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void StopAutoResumeTimer()
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                _autoResumeTimer?.Stop();
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void _autoResumeTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                StaticCode.Logger?.Here().Debug(StaticCode.GetLastInputTime().ToString());

                if (StaticCode.GetLastInputTime().TotalSeconds > SettingsVm.Settings.AutoResumeSeconds)
                {
                    Start();
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void StartBlackoutTimer()
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                StopBlackoutTimer();

                if (_blackoutTimer == null)
                {
                    _blackoutTimer = new System.Timers.Timer
                    {
                        Interval = 1000
                    };
                    _blackoutTimer.Elapsed += _blackoutTimer_Elapsed;
                }

                _blackoutTimer.Start();
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void StopBlackoutTimer()
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                _blackoutTimer?.Stop();
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void _blackoutTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                if (!BlackoutIsActive())
                {
                    Start();
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private bool BlackoutIsActive()
        {
            bool _blackoutIsActive;

            try
            {
                _blackoutIsActive = (SettingsVm.Settings.Blackouts != null) && SettingsVm.Settings.Blackouts.Any(blackout => (blackout.EnabledDays.Any(day => day.Equals(DateTime.Now.AddDays(-1).DayOfWeek)) && (new DateTime(DateTime.Now.AddDays(-1).Year, DateTime.Now.AddDays(-1).Month, DateTime.Now.AddDays(-1).Day, blackout.Time.Hours, blackout.Time.Minutes, blackout.Time.Seconds).Add(blackout.Duration) > DateTime.Now)) || (blackout.EnabledDays.Any(day => day.Equals(DateTime.Now.DayOfWeek)) && (blackout.Time < DateTime.Now.TimeOfDay) && (blackout.Time.Add(blackout.Duration) > DateTime.Now.TimeOfDay)));
            }
            catch (Exception ex)
            {
                _blackoutIsActive = false;
                StaticCode.Logger?.Here().Error(ex.Message);
            }

            StaticCode.Logger?.Here().Debug(_blackoutIsActive.ToString());
            return _blackoutIsActive;
        }

        protected void OnMouseStateChanged(object sender, MouseState state)
        {
            StaticCode.Logger?.Here().Debug(state.ToString());

            try
            {
                MouseStateChanged?.Invoke(sender, state);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        protected void OnAltTabVisibilityChanged(object sender, bool visible)
        {
            StaticCode.Logger?.Here().Debug(visible.ToString());

            try
            {
                AltTabVisibilityChanged?.Invoke(sender, visible);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        //protected void OnHookKeyEnabledChanged(object sender, bool enabled, Key key)
        //{
        //    HookKeyEnabledChanged?.Invoke(sender, enabled, key);
        //}

        protected void OnRequestActivate(object sender)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                RequestActivate?.Invoke(sender);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        protected void OnRequestMinimise(object sender)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                RequestMinimise?.Invoke(sender);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
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

        public void Dispose()
        {
            try
            {
                RestoreVolume();
                CleanupJobs();
                StaticCode.ScheduleArrived -= StaticCode_ScheduleArrived;
                //StaticCode.ThemeUpdated -= StaticCode_ThemeUpdated;
                StaticCode.UpdateAvailablityChanged -= StaticCode_UpdateAvailablityChanged;
                SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }
    }
}