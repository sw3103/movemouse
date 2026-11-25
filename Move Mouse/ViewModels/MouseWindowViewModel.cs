using Avalonia.Threading;
using ellabi.Actions;
using ellabi.Jobs;
using ellabi.Platform;
using ellabi.Schedules;
using ellabi.Utilities;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ellabi.ViewModels
{
    public class MouseWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        public enum NotificationIcon
        {
            None,
            Info,
            Warning,
            Error
        }

        public delegate void MouseStateChangedHandler(object sender, MouseState state);
        public delegate void AltTabVisibilityChangedHandler(object sender, bool visible);
        public delegate void RequestActivateHandler(object sender);
        public delegate void RequestMinimiseHandler(object sender);
        public delegate void RequestNotificationHandler(object sender, string title, string message, NotificationIcon symbol);
        public delegate void RequestConcealMainWindowHandler(object sender);

        public event PropertyChangedEventHandler PropertyChanged;
        public event MouseStateChangedHandler MouseStateChanged;
        public event AltTabVisibilityChangedHandler AltTabVisibilityChanged;
        public event RequestActivateHandler RequestActivate;
        public event RequestMinimiseHandler RequestMinimise;
        public event RequestNotificationHandler RequestNotification;
        public event RequestConcealMainWindowHandler RequestConcealMainWindow;

        private MouseState _currentState;
        private DateTime _executionTime;
        private Timer _actionTimer;
        private SettingsWindowViewModel _settingsVm;
        private System.Timers.Timer _autoPauseTimer;
        private System.Timers.Timer _autoResumeTimer;
        private System.Timers.Timer _blackoutTimer;
        private System.Timers.Timer _environmentMonitorTimer;
        private bool _firstPass = true;
        private readonly TimeSpan _minTimeBetweenStopStartToggle = TimeSpan.FromMilliseconds(500);
        private DateTime _lastStopStartToggleTime = DateTime.MinValue;
        private StdSchedulerFactory _schedulerFactory;
        private IScheduler _scheduler;
        private bool _workstationLocked;
        private bool _lastOnBattery;
        private bool _forceSystrayIconVisibility;
        private double? _initialVolume;
        private Guid _activeExecutionId;
        private readonly object _actionsLock = new object();
        private readonly object _pauseLock = new object();
        private readonly SemaphoreSlim _scheduleJobsSemaphore = new SemaphoreSlim(1, 1);
        private bool _updateAvailable;

        public enum MouseState
        {
            Idle,
            Running,
            Paused,
            Executing,
            Sleeping,
            OnBattery,
            Locked
        }

        public SettingsWindowViewModel SettingsVm => _settingsVm ??= new SettingsWindowViewModel();

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
            StartStopToggleCommand = new RelayCommand(param => StartStopToggle(), param => CanStartStopToggle());
            ShowMoveMouseCommand = new RelayCommand(param => OnRequestActivate(this));
            CurrentState = MouseState.Idle;
            SettingsVm.Settings.PropertyChanged += Settings_PropertyChanged;
            StaticCode.ScheduleArrived += StaticCode_ScheduleArrived;
            if (StaticCode.DownloadSource == StaticCode.MoveMouseSource.GitHub)
            {
                StaticCode.UpdateAvailablityChanged += StaticCode_UpdateAvailablityChanged;
            }
            StaticCode.RefreshSchedules += StaticCode_RefreshSchedules;

            _lastOnBattery = RunningOnBattery();
            _workstationLocked = LinuxSessionLockService.IsSessionLocked();
            _environmentMonitorTimer = new System.Timers.Timer(2500);
            _environmentMonitorTimer.Elapsed += EnvironmentMonitorTimer_Elapsed;
            _environmentMonitorTimer.Start();

            ThreadPool.QueueUserWorkItem(ForceSystrayIconVisibilityAtLaunch);
            ScheduleJobs();
        }

        private void EnvironmentMonitorTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                bool locked = LinuxSessionLockService.IsSessionLocked();
                if (locked != _workstationLocked)
                {
                    _workstationLocked = locked;
                    StaticCode.Logger?.Here().Debug(locked ? "SessionLock" : "SessionUnlock");

                    if (!locked && (CurrentState.Equals(MouseState.Locked) || CurrentState.Equals(MouseState.Running)) && !SettingsVm.Settings.ActiveWhenLocked)
                    {
                        ShowNotification("Automatically resuming now workstation has been unlocked.");
                    }
                }

                bool onBattery = RunningOnBattery();
                if (onBattery != _lastOnBattery)
                {
                    _lastOnBattery = onBattery;
                    if (SettingsVm.Settings.PauseOnBattery)
                    {
                        if (CurrentState.Equals(MouseState.Running) && onBattery)
                        {
                            Stop(MouseState.OnBattery);
                            ShowNotification("Pausing now running on battery.");
                        }
                        else if (CurrentState.Equals(MouseState.OnBattery) && !onBattery)
                        {
                            Start();
                            ShowNotification("Resuming now running on mains power.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void StaticCode_RefreshSchedules()
        {
            Debug.WriteLine("StaticCode_RefreshSchedules()");
            ScheduleJobs();
        }

        private void ForceSystrayIconVisibilityAtLaunch(object stateInfo)
        {
            StaticCode.Logger?.Here().Debug(string.Empty);

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

        private void StaticCode_ScheduleArrived(ScheduleBase.ScheduleAction action)
        {
            try
            {
                StaticCode.Logger?.Here().Debug(action.ToString());

                switch (action)
                {
                    case ScheduleBase.ScheduleAction.Start:
                        Start();
                        ShowNotification("Scheduled start.");
                        break;
                    case ScheduleBase.ScheduleAction.Stop:
                        Stop(MouseState.Idle);
                        ShowNotification("Scheduled stop.");
                        break;
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private async void ScheduleJobs()
        {
            if (!await _scheduleJobsSemaphore.WaitAsync(0))
            {
                StaticCode.Logger?.Here().Warning("ScheduleJobs already running.");
                return;
            }

            try
            {
                CleanupJobs();

                _schedulerFactory = new StdSchedulerFactory();
                _scheduler = await _schedulerFactory.GetScheduler();

                if (SettingsVm.Settings.Schedules != null && SettingsVm.Settings.Schedules.Any(schedule => schedule.IsValid && schedule.IsEnabled))
                {
                    foreach (var schedule in SettingsVm.Settings.Schedules.Where(schedule => schedule.IsValid && schedule.IsEnabled))
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

                if (StaticCode.DownloadSource == StaticCode.MoveMouseSource.GitHub)
                {
                    var resetUpdateStatusJob = JobBuilder.Create<ResetUpdateStatusJob>()
                        .WithIdentity("ResetUpdateStatusJob")
                        .Build();
                    await _scheduler.ScheduleJob(resetUpdateStatusJob, new CronTriggerImpl("ResetUpdateStatusJob", null, "0 1 0 ? * *"));
                    await _scheduler.TriggerJob(new JobKey("ResetUpdateStatusJob"));

                    var checkForUpdateTime = TimeSpan.FromHours(10).Add(TimeSpan.FromSeconds(Random.Shared.Next(0, 21600)));
                    var checkForUpdateJob = JobBuilder.Create<CheckForUpdateJob>()
                        .WithIdentity("CheckForUpdateJob")
                        .Build();
                    await _scheduler.ScheduleJob(checkForUpdateJob, new CronTriggerImpl("CheckForUpdateJob", null, $"{checkForUpdateTime.Seconds} {checkForUpdateTime.Minutes} {checkForUpdateTime.Hours} ? * *"));
                    await _scheduler.TriggerJob(new JobKey("CheckForUpdateJob"));
                }

                var refreshSchedulesJob = JobBuilder.Create<RefreshSchedulesJob>()
                    .WithIdentity("RefreshSchedulesJob")
                    .Build();
                var refreshSchedulesTime = DateTime.Now.AddSeconds(-1).TimeOfDay;
                await _scheduler.ScheduleJob(refreshSchedulesJob, new CronTriggerImpl("RefreshSchedulesJob", null, $"{refreshSchedulesTime.Seconds} {refreshSchedulesTime.Minutes} {refreshSchedulesTime.Hours} ? * *"));
                await _scheduler.Start();
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
            finally
            {
                _scheduleJobsSemaphore.Release();
            }
        }

        private void CleanupJobs()
        {
            StaticCode.Logger?.Here().Debug(string.Empty);

            try
            {
                if (_scheduler != null && !_scheduler.IsShutdown)
                {
                    Task.WaitAll(new[] { _scheduler.Clear() });
                    Task.WaitAll(new[] { _scheduler.Shutdown(false) });
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
                    case nameof(Classes.Settings.AutoPause):
                        if (!SettingsVm.Settings.AutoPause) StopAutoPauseTimer();
                        break;
                    case nameof(Classes.Settings.AutoResume):
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
                    case nameof(Classes.Settings.Schedules):
                        ScheduleJobs();
                        break;
                    case nameof(Classes.Settings.HideFromAltTab):
                        OnAltTabVisibilityChanged(this, !SettingsVm.Settings.HideFromAltTab);
                        break;
                    case nameof(Classes.Settings.PauseOnBattery):
                        if (!SettingsVm.Settings.PauseOnBattery && CurrentState.Equals(MouseState.OnBattery))
                        {
                            Start();
                            ShowNotification("Resuming now running on mains power.");
                        }
                        else if (SettingsVm.Settings.PauseOnBattery && CurrentState.Equals(MouseState.Running) && RunningOnBattery())
                        {
                            Stop(MouseState.OnBattery);
                            ShowNotification("Pausing now running on battery.");
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public void StartStopToggle(bool concealMainWindowAfter = false)
        {
            StaticCode.Logger?.Here().Information(string.Empty);

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

                    if (concealMainWindowAfter)
                    {
                        OnRequestConcealMainWindow(this);
                    }
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private bool CanStartStopToggle() => true;

        public void Start()
        {
            StaticCode.Logger?.Here().Information(string.Empty);

            try
            {
                if (!SettingsVm.Settings.MoveMouseHasBeenClicked) SettingsVm.Settings.MoveMouseHasBeenClicked = true;

                if (CurrentState != MouseState.Running)
                {
                    if (SettingsVm.Settings.PauseOnBattery && RunningOnBattery())
                    {
                        CurrentState = MouseState.OnBattery;
                    }
                    else if (!BlackoutIsActive())
                    {
                        StopAutoResumeTimer();
                        _activeExecutionId = Guid.NewGuid();
                        _lastStopStartToggleTime = DateTime.Now;
                        if (_firstPass) PerformActions(ActionBase.EventTrigger.Start);

                        double interval = SettingsVm.Settings.RandomInterval
                            ? Random.Shared.Next(SettingsVm.Settings.LowerInterval * 1000, SettingsVm.Settings.UpperInterval * 1000)
                            : (SettingsVm.Settings.LowerInterval * 1000);
                        interval = interval > 0 ? interval : 1;

                        ExecutionTime = DateTime.Now.AddMilliseconds(interval);
                        CurrentState = MouseState.Running;
                        _actionTimer?.Dispose();
                        _actionTimer = new Timer(param => PerformActions(ActionBase.EventTrigger.Interval), null, TimeSpan.FromMilliseconds(interval), Timeout.InfiniteTimeSpan);
                        StopBlackoutTimer();
                        AdjustVolume();
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
                    if (!_initialVolume.HasValue)
                    {
                        _initialVolume = await LinuxAudioService.GetDefaultVolumeAsync();
                    }

                    StaticCode.Logger?.Here().Information(SettingsVm.Settings.RunningVolume.ToString());
                    await LinuxAudioService.SetDefaultVolumeAsync(SettingsVm.Settings.RunningVolume);
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
                    StaticCode.Logger?.Here().Information(_initialVolume.Value.ToString());
                    await LinuxAudioService.SetDefaultVolumeAsync(_initialVolume.Value);
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
                lock (_actionsLock)
                {
                    var executionId = _activeExecutionId;
                    var lastInputTime = StaticCode.GetLastInputTime();
                    var actionAborted = false;
                    IEnumerable<ActionBase> actions = null;

                    if (_firstPass)
                    {
                        for (int i = 0; i < SettingsVm.Settings.Actions.Length; i++)
                        {
                            SettingsVm.Settings.Actions[i].IntervalExecutionCount = 0;
                        }
                    }

                    switch (trigger)
                    {
                        case ActionBase.EventTrigger.Start:
                            if (_firstPass && SettingsVm.Settings.Actions.Any(action => action.IsValid && action.IsEnabled && action.Trigger.Equals(trigger)))
                            {
                                actions = SettingsVm.Settings.Actions.Where(action => action.IsValid && action.IsEnabled && action.Trigger.Equals(trigger));

                                foreach (var action in actions)
                                {
                                    if (!actionAborted && _activeExecutionId.Equals(executionId))
                                    {
                                        action.Execute();
                                        actionAborted = action.Aborted;
                                    }
                                }
                            }
                            break;

                        case ActionBase.EventTrigger.Interval:
                            if (SettingsVm.Settings.ActiveWhenLocked || !_workstationLocked)
                            {
                                if (!BlackoutIsActive())
                                {
                                    CurrentState = MouseState.Executing;
                                    StopAutoPauseTimer();

                                    if (SettingsVm.Settings.Actions.Any(action => action.IsValid && action.IsEnabled && action.Trigger.Equals(trigger)))
                                    {
                                        actions = _firstPass
                                            ? SettingsVm.Settings.Actions.Where(action => action.IsValid && action.IsEnabled && action.Trigger.Equals(trigger))
                                            : SettingsVm.Settings.Actions.Where(action => action.IsValid && action.IsEnabled && action.Trigger.Equals(trigger) && action.Repeat &&
                                                ((action.RepeatMode == ActionBase.IntervalRepeatMode.Forever) ||
                                                 ((action.RepeatMode == ActionBase.IntervalRepeatMode.Throttle) && (action.IntervalExecutionCount < action.IntervalThrottle))));

                                        foreach (var action in actions)
                                        {
                                            if (!actionAborted && _activeExecutionId.Equals(executionId))
                                            {
                                                action.Execute();
                                                actionAborted = action.Aborted;
                                            }
                                        }
                                    }

                                    _firstPass = false;
                                }
                                else
                                {
                                    CurrentState = MouseState.Sleeping;
                                }
                            }
                            else
                            {
                                CurrentState = MouseState.Locked;
                            }

                            if (actionAborted)
                            {
                                Stop((SettingsVm.Settings.AutoPause && SettingsVm.Settings.AutoResume) ? MouseState.Paused : MouseState.Idle);
                            }
                            else
                            {
                                if (_activeExecutionId.Equals(executionId) &&
                                    (CurrentState.Equals(MouseState.Locked) ||
                                     CurrentState.Equals(MouseState.Sleeping) ||
                                     (!_firstPass && SettingsVm.Settings.Actions.Any(action => action.IsValid && action.IsEnabled && action.Trigger.Equals(trigger) && action.Repeat &&
                                         ((action.RepeatMode == ActionBase.IntervalRepeatMode.Forever) ||
                                          ((action.RepeatMode == ActionBase.IntervalRepeatMode.Throttle) && (action.IntervalExecutionCount < action.IntervalThrottle)))))))
                                {
                                    Start();
                                }
                                else
                                {
                                    Stop(MouseState.Idle);
                                    ShowNotification("Automatically stopping as there are no actions that are configured to repeat forever at each interval.");
                                }
                            }
                            break;

                        case ActionBase.EventTrigger.Stop:
                            if (SettingsVm.Settings.Actions.Any(action => action.IsValid && action.IsEnabled && action.Trigger.Equals(trigger)))
                            {
                                actions = SettingsVm.Settings.Actions.Where(action => action.IsValid && action.IsEnabled && action.Trigger.Equals(trigger));

                                foreach (var action in actions)
                                {
                                    if (!actionAborted && _activeExecutionId.Equals(executionId))
                                    {
                                        action.Execute();
                                        actionAborted = action.Aborted;
                                    }
                                }
                            }
                            break;
                    }

                    if (actions != null && actions.Any(action => action.InterruptsIdleTime) && lastInputTime <= StaticCode.GetLastInputTime())
                    {
                        StaticCode.Logger?.Here().Warning($"Your system idle time has not been reset which may result in session timeout due to lack of user activity. Please refer to the Troubleshooting section in the Wiki for further assistance ({StaticCode.TroubleshootingUrl}).");
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
            StaticCode.Logger?.Here().Debug(string.Empty);

            try
            {
                StopAutoPauseTimer();

                if (SettingsVm.Settings.AutoPause)
                {
                    if (_autoPauseTimer == null)
                    {
                        _autoPauseTimer = new System.Timers.Timer { Interval = 250 };
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
            StaticCode.Logger?.Here().Debug(string.Empty);

            lock (_pauseLock)
            {
                try
                {
                    _autoPauseTimer?.Stop();
                }
                catch (Exception ex)
                {
                    StaticCode.Logger?.Here().Error(ex.Message);
                }
            }
        }

        private void _autoPauseTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                StaticCode.Logger?.Here().Debug(StaticCode.GetLastInputTime().ToString());

                lock (_pauseLock)
                {
                    if (CurrentState.Equals(MouseState.Running) && StaticCode.GetLastInputTime() < TimeSpan.FromMilliseconds(_autoPauseTimer.Interval))
                    {
                        Stop((SettingsVm.Settings.AutoPause && SettingsVm.Settings.AutoResume) ? MouseState.Paused : MouseState.Idle);
                    }
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void StartAutoResumeTimer()
        {
            StaticCode.Logger?.Here().Debug(string.Empty);

            try
            {
                StopAutoResumeTimer();

                if (_autoResumeTimer == null)
                {
                    _autoResumeTimer = new System.Timers.Timer { Interval = 250 };
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
            StaticCode.Logger?.Here().Debug(string.Empty);

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
                    ShowNotification($"Automatically resuming after {SettingsVm.Settings.AutoResumeSeconds} seconds of inactivity.");
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void StartBlackoutTimer()
        {
            StaticCode.Logger?.Here().Debug(string.Empty);

            try
            {
                StopBlackoutTimer();
                ShowNotification("Going to sleep whilst blackout in effect.");

                if (_blackoutTimer == null)
                {
                    _blackoutTimer = new System.Timers.Timer { Interval = 1000 };
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
            StaticCode.Logger?.Here().Debug(string.Empty);

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
            StaticCode.Logger?.Here().Debug(string.Empty);

            try
            {
                if (!BlackoutIsActive())
                {
                    Start();
                    ShowNotification("Resuming now blackout has expired.");
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private bool BlackoutIsActive()
        {
            bool blackoutIsActive;

            try
            {
                blackoutIsActive = SettingsVm.Settings.Blackouts != null && SettingsVm.Settings.Blackouts.Any(blackout => blackout.IsActive);
            }
            catch (Exception ex)
            {
                blackoutIsActive = false;
                StaticCode.Logger?.Here().Error(ex.Message);
            }

            StaticCode.Logger?.Here().Debug(blackoutIsActive.ToString());
            return blackoutIsActive;
        }

        private bool RunningOnBattery()
        {
            bool runningOnBattery = LinuxPowerService.IsRunningOnBattery();
            StaticCode.Logger?.Here().Debug(runningOnBattery.ToString());
            return runningOnBattery;
        }

        public void ShowNotification(string message)
        {
            ShowNotification(null, message, NotificationIcon.None);
        }

        public void ShowNotification(string title, string message, NotificationIcon symbol)
        {
            if (!SettingsVm.Settings.HideSystemTrayIcon && SettingsVm.Settings.ShowSystemTrayNotifications && !_workstationLocked)
            {
                OnRequestNotification(this, title, message, symbol);
            }
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

        protected void OnRequestActivate(object sender)
        {
            StaticCode.Logger?.Here().Debug(string.Empty);

            try
            {
                RequestActivate?.Invoke(sender);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        protected void OnRequestConcealMainWindow(object sender)
        {
            StaticCode.Logger?.Here().Debug(string.Empty);

            try
            {
                RequestConcealMainWindow?.Invoke(sender);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        protected void OnRequestNotification(object sender, string title, string message, NotificationIcon symbol)
        {
            StaticCode.Logger?.Here().Debug($"{title}\t{message}\t{symbol}");

            try
            {
                RequestNotification?.Invoke(sender, title, message, symbol);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        protected void OnRequestMinimise(object sender)
        {
            StaticCode.Logger?.Here().Debug(string.Empty);

            try
            {
                RequestMinimise?.Invoke(sender);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            try
            {
                if (Avalonia.Application.Current != null && !Dispatcher.UIThread.CheckAccess())
                {
                    Dispatcher.UIThread.Post(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
                }
                else
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }
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
                _actionTimer?.Dispose();
                _autoPauseTimer?.Dispose();
                _autoResumeTimer?.Dispose();
                _blackoutTimer?.Dispose();
                _environmentMonitorTimer?.Dispose();
                RestoreVolume();
                CleanupJobs();
                StaticCode.ScheduleArrived -= StaticCode_ScheduleArrived;
                StaticCode.UpdateAvailablityChanged -= StaticCode_UpdateAvailablityChanged;
                StaticCode.RefreshSchedules -= StaticCode_RefreshSchedules;
                SettingsVm.Dispose();
                LinuxInputService.Dispose();
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }
    }
}
