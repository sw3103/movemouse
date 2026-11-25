using Avalonia.Threading;
using ellabi.Actions;
using ellabi.Classes;
using ellabi.Platform;
using ellabi.Schedules;
using ellabi.Utilities;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml.Serialization;

namespace ellabi.ViewModels
{
    public class SettingsWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Settings _settings;
        private ActionBase _selectedAction;
        private RelayCommand _removeSelectedActionCommand;
        private RelayCommand _copySelectedActionCommand;
        private RelayCommand _moveUpSelectedActionCommand;
        private RelayCommand _moveDownSelectedActionCommand;
        private RelayCommand _removeSelectedScheduleCommand;
        private RelayCommand _addBlackoutCommand;
        private RelayCommand _removeSelectedBlackoutCommand;
        private StartupTaskState _launchAtStartup;
        private readonly System.Timers.Timer _updateSystemIdleTimeTimer;
        private readonly object _settingsLock = new object();
        private DateTime _lastSettingsPropertyChanged;
        private readonly TimeSpan _lastSettingsPropertyChangedInterval = TimeSpan.FromMilliseconds(1000);
        private Guid _lastThreadId;

        public Settings Settings => _settings ??= ReadSettings();

        public ObservableCollection<int> VirtualKeys => new ObservableCollection<int>(StaticCode.VirtualKeys.Value.Keys);

        public ActionBase SelectedAction
        {
            get => _selectedAction;
            set
            {
                _selectedAction = value;
                OnPropertyChanged();
                RaiseActionCommands();
            }
        }

        public RelayCommand RemoveSelectedActionCommand
        {
            get => _removeSelectedActionCommand;
            set
            {
                _removeSelectedActionCommand = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand CopySelectedActionCommand
        {
            get => _copySelectedActionCommand;
            set
            {
                _copySelectedActionCommand = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand MoveUpSelectedActionCommand
        {
            get => _moveUpSelectedActionCommand;
            set
            {
                _moveUpSelectedActionCommand = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand MoveDownSelectedActionCommand
        {
            get => _moveDownSelectedActionCommand;
            set
            {
                _moveDownSelectedActionCommand = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand RemoveSelectedScheduleCommand
        {
            get => _removeSelectedScheduleCommand;
            set
            {
                _removeSelectedScheduleCommand = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand AddBlackoutCommand
        {
            get => _addBlackoutCommand;
            set
            {
                _addBlackoutCommand = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand RemoveSelectedBlackoutCommand
        {
            get => _removeSelectedBlackoutCommand;
            set
            {
                _removeSelectedBlackoutCommand = value;
                OnPropertyChanged();
            }
        }

        public StartupTaskState LaunchAtStartup
        {
            get => _launchAtStartup;
            set => ToggleStartupTask();
        }

        public string Version
        {
            get
            {
                var v = Assembly.GetExecutingAssembly().GetName().Version;
                return v != null ? $"{v.Major}.{v.Minor}.{v.Build}" : "4.21.0";
            }
        }

        public string HomePage => StaticCode.HomePageUrl;

        public string MailAddress => StaticCode.MailAddress;

        public string TwitterUrl => StaticCode.TwitterUrl;

        public string GitHubUrl => StaticCode.GitHubUrl;

        public string PayPalUrl => StaticCode.PayPalUrl;

        public string Copyright
        {
            get
            {
                var attr = (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(
                    Assembly.GetExecutingAssembly(), typeof(AssemblyCopyrightAttribute), false);
                return attr?.Copyright ?? "Copyright © Steve Williams 2017";
            }
        }

        public TimeSpan SystemIdleTime => StaticCode.GetLastInputTime();

        public IEnumerable<LogEventLevel> LogEventLevels => Enum.GetValues(typeof(LogEventLevel)).Cast<LogEventLevel>();

        public SettingsWindowViewModel()
        {
            if (Settings.EnableLogging)
            {
                StaticCode.EnableLog(Settings.LogLevel);
            }

            StaticCode.Logger?.Here().Information($"WorkingDirectory = {StaticCode.WorkingDirectory}");
            _selectedAction = Settings.Actions.FirstOrDefault();
            _removeSelectedActionCommand = new RelayCommand(param => RemoveSelectedAction(), param => CanRemoveSelectedAction());
            _copySelectedActionCommand = new RelayCommand(param => CopySelectedAction(), param => CanCopySelectedAction());
            _moveUpSelectedActionCommand = new RelayCommand(param => MoveUpSelectedAction(), param => CanMoveUpSelectedAction());
            _moveDownSelectedActionCommand = new RelayCommand(param => MoveDownSelectedAction(), param => CanMoveDownSelectedAction());
            _removeSelectedScheduleCommand = new RelayCommand(RemoveSelectedSchedule);
            _addBlackoutCommand = new RelayCommand(param => AddBlackout());
            _removeSelectedBlackoutCommand = new RelayCommand(RemoveSelectedBlackout);
            Settings.PropertyChanged += Settings_PropertyChanged;
            RefreshStartupTask();
            _updateSystemIdleTimeTimer = new System.Timers.Timer(250);
            _updateSystemIdleTimeTimer.Elapsed += UpdateSystemIdleTimeTimer_Elapsed;
        }

        private void UpdateSystemIdleTimeTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            OnPropertyChanged(nameof(SystemIdleTime));
        }

        private void RaiseActionCommands()
        {
            _removeSelectedActionCommand?.RaiseCanExecuteChanged();
            _copySelectedActionCommand?.RaiseCanExecuteChanged();
            _moveUpSelectedActionCommand?.RaiseCanExecuteChanged();
            _moveDownSelectedActionCommand?.RaiseCanExecuteChanged();
        }

        public void AddKeystroke(int key)
        {
            StaticCode.Logger?.Here().Debug(key.ToString());

            try
            {
                if (SelectedAction is KeystrokeAction keystrokeAction)
                {
                    keystrokeAction.AddKeystroke(key);
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public void StartSystemIdleTimer() => _updateSystemIdleTimeTimer.Start();

        public void StopSystemIdleTimer() => _updateSystemIdleTimeTimer.Stop();

        public void RefreshStartupTask()
        {
            StaticCode.Logger?.Here().Debug(string.Empty);

            try
            {
                _launchAtStartup = LinuxStartupService.GetState();
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
                _launchAtStartup = StartupTaskState.Disabled;
            }

            OnPropertyChanged(nameof(LaunchAtStartup));
        }

        private void ToggleStartupTask()
        {
            StaticCode.Logger?.Here().Debug(string.Empty);

            try
            {
                LinuxStartupService.SetEnabled(LaunchAtStartup != StartupTaskState.Enabled);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }

            RefreshStartupTask();
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                StaticCode.Logger?.Here().Debug(e.PropertyName);
                _lastSettingsPropertyChanged = DateTime.Now;
                SaveSettings(Settings);

                switch (e.PropertyName)
                {
                    case nameof(Settings.Actions):
                        RaiseActionCommands();
                        break;
                    case nameof(Settings.EnableLogging):
                        if (Settings.EnableLogging)
                        {
                            StaticCode.EnableLog(Settings.LogLevel);
                        }
                        else
                        {
                            StaticCode.DisableLog();
                        }
                        break;
                    case nameof(Settings.LogLevel):
                        if (Settings.EnableLogging)
                        {
                            StaticCode.EnableLog(Settings.LogLevel);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void SaveSettings(Settings settings)
        {
            _lastThreadId = Guid.NewGuid();
            ThreadPool.QueueUserWorkItem(SaveSettingsThread, settings);
        }

        private void SaveSettingsThread(object settings)
        {
            var threadId = _lastThreadId;

            lock (_settingsLock)
            {
                while (threadId.Equals(_lastThreadId) && (_lastSettingsPropertyChanged.Add(_lastSettingsPropertyChangedInterval) > DateTime.Now))
                {
                    Thread.Sleep(100);
                }

                if (threadId.Equals(_lastThreadId))
                {
                    StaticCode.Logger?.Here().Debug(StaticCode.SettingsXmlPath);
                    StreamWriter sw = null;

                    try
                    {
                        Directory.CreateDirectory(StaticCode.WorkingDirectory);
                        sw = new StreamWriter(StaticCode.SettingsXmlPath, false);
                        var xs = XmlSerializer.FromTypes(new[] { typeof(Settings) })[0];
                        xs.Serialize(sw, settings);
                        sw.Flush();
                    }
                    catch (Exception ex)
                    {
                        StaticCode.Logger?.Here().Error(ex.Message);
                    }
                    finally
                    {
                        sw?.Close();
                        sw?.Dispose();
                    }

                    Thread.Sleep(1000);
                }
            }
        }

        private Settings ReadSettings()
        {
            StaticCode.Logger?.Here().Debug(StaticCode.SettingsXmlPath);

            lock (_settingsLock)
            {
                StreamReader sr = null;

                try
                {
                    if (File.Exists(StaticCode.SettingsXmlPath))
                    {
                        var xs = XmlSerializer.FromTypes(new[] { typeof(Settings) })[0];
                        sr = new StreamReader(StaticCode.SettingsXmlPath);
                        StaticCode.Logger?.Here().Debug(sr.ReadToEnd());
                        sr.BaseStream.Seek(0, SeekOrigin.Begin);
                        return (Settings)xs.Deserialize(sr);
                    }
                }
                catch (Exception ex)
                {
                    StaticCode.Logger?.Here().Error(ex.Message);
                }
                finally
                {
                    sr?.Close();
                }
            }

            return new Settings();
        }

        public void AddAction(Type actionType)
        {
            StaticCode.Logger?.Here().Debug(actionType.ToString());

            try
            {
                var action = (ActionBase)Activator.CreateInstance(actionType);
                InsertAction(action);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void InsertAction(ActionBase action)
        {
            StaticCode.Logger?.Here().Debug(action.ToString());

            try
            {
                var actions = Settings.Actions == null ? new List<ActionBase>() : new List<ActionBase>(Settings.Actions);

                if (SelectedAction != null)
                {
                    actions.Insert(actions.FindIndex(t => t.Id.Equals(SelectedAction.Id)) + 1, action);
                }
                else
                {
                    actions.Add(action);
                }

                Settings.Actions = actions.ToArray();
                SelectedAction = action;
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public void RemoveSelectedAction()
        {
            try
            {
                if (!CanRemoveSelectedAction()) return;

                StaticCode.Logger?.Here().Debug(SelectedAction.ToString());
                var previousActionIndex = Settings.Actions.ToList().FindIndex(t => t.Id.Equals(SelectedAction.Id)) - 1;
                Settings.Actions = new List<ActionBase>(Settings.Actions.Except(new[] { SelectedAction })).ToArray();

                SelectedAction = previousActionIndex > -1
                    ? Settings.Actions[previousActionIndex]
                    : Settings.Actions.FirstOrDefault();
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private bool CanRemoveSelectedAction() => SelectedAction != null && Settings.Actions.Length > 1;

        public void CopySelectedAction()
        {
            try
            {
                if (!CanCopySelectedAction()) return;

                StaticCode.Logger?.Here().Debug(SelectedAction.ToString());
                var copy = DeepCopy(SelectedAction);
                if (copy != null)
                {
                    copy.Id = Guid.NewGuid();
                    InsertAction(copy);
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private bool CanCopySelectedAction() => SelectedAction != null;

        private TAction DeepCopy<TAction>(TAction action)
        {
            try
            {
                using var ms = new MemoryStream();
                var serializer = new XmlSerializer(action.GetType());
                serializer.Serialize(ms, action);
                ms.Position = 0;
                return (TAction)serializer.Deserialize(ms);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }

            return default;
        }

        private void MoveUpSelectedAction()
        {
            try
            {
                if (!CanMoveUpSelectedAction()) return;

                StaticCode.Logger?.Here().Debug(SelectedAction.ToString());
                var actions = new List<ActionBase>(Settings.Actions);
                var action = SelectedAction;
                int index = actions.FindIndex(t => t.Id.Equals(SelectedAction.Id));
                actions.RemoveAt(index);
                actions.Insert(index - 1, action);
                Settings.Actions = actions.ToArray();
                SelectedAction = action;
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private bool CanMoveUpSelectedAction()
        {
            try
            {
                return SelectedAction != null && Settings.Actions != null && Settings.Actions.Any() &&
                       !Settings.Actions.ToList().FindIndex(action => action.Id.Equals(SelectedAction.Id)).Equals(0);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }

            return false;
        }

        private void MoveDownSelectedAction()
        {
            try
            {
                if (!CanMoveDownSelectedAction()) return;

                StaticCode.Logger?.Here().Debug(SelectedAction.ToString());
                var actions = new List<ActionBase>(Settings.Actions);
                var action = SelectedAction;
                int index = actions.FindIndex(t => t.Id.Equals(SelectedAction.Id));
                actions.RemoveAt(index);
                actions.Insert(index + 1, action);
                Settings.Actions = actions.ToArray();
                SelectedAction = action;
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private bool CanMoveDownSelectedAction()
        {
            try
            {
                return SelectedAction != null && Settings.Actions != null && Settings.Actions.Any() &&
                       !Settings.Actions.ToList().FindIndex(action => action.Id.Equals(SelectedAction.Id)).Equals(Settings.Actions.Length - 1);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }

            return false;
        }

        public void AddSchedule(Type scheduleType)
        {
            StaticCode.Logger?.Here().Debug(scheduleType.ToString());

            try
            {
                var schedule = (ScheduleBase)Activator.CreateInstance(scheduleType);
                var schedules = Settings.Schedules == null ? new List<ScheduleBase>() : new List<ScheduleBase>(Settings.Schedules);
                schedules.Add(schedule);
                Settings.Schedules = schedules.ToArray();
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public void RemoveSelectedSchedule(object id)
        {
            StaticCode.Logger?.Here().Debug(id?.ToString() ?? string.Empty);

            try
            {
                if (id != null && Settings.Schedules != null)
                {
                    var scheduleToRemove = Settings.Schedules.FirstOrDefault(schedule => schedule.Id.Equals((Guid)id));

                    if (scheduleToRemove != null)
                    {
                        Settings.Schedules = new List<ScheduleBase>(Settings.Schedules.Except(new[] { scheduleToRemove })).ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public void AddBlackout()
        {
            StaticCode.Logger?.Here().Debug(string.Empty);

            try
            {
                var blackouts = Settings.Blackouts == null ? new List<Blackout>() : new List<Blackout>(Settings.Blackouts);
                blackouts.Add(new Blackout());
                Settings.Blackouts = blackouts.ToArray();
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public void RemoveSelectedBlackout(object id)
        {
            StaticCode.Logger?.Here().Debug(id?.ToString() ?? string.Empty);

            try
            {
                if (id != null && Settings.Blackouts != null)
                {
                    var blackoutToRemove = Settings.Blackouts.FirstOrDefault(blackout => blackout.Id.Equals((Guid)id));

                    if (blackoutToRemove != null)
                    {
                        Settings.Blackouts = new List<Blackout>(Settings.Blackouts.Except(new[] { blackoutToRemove })).ToArray();
                    }
                }
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
            if (_updateSystemIdleTimeTimer != null)
            {
                _updateSystemIdleTimeTimer.Stop();
                _updateSystemIdleTimeTimer.Elapsed -= UpdateSystemIdleTimeTimer_Elapsed;
                _updateSystemIdleTimeTimer.Dispose();
            }
        }
    }
}
