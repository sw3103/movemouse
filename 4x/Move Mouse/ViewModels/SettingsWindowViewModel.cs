using ellabi.Actions;
using ellabi.Annotations;
using ellabi.Classes;
using ellabi.Schedules;
using ellabi.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Windows.ApplicationModel;

namespace ellabi.ViewModels
{
    internal class SettingsWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Settings _settings;
        private ActionBase _selectedAction;
        private RelayCommand _removeSelectedActionCommand;
        private RelayCommand _moveUpSelectedActionCommand;
        private RelayCommand _moveDownSelectedActionCommand;
        private RelayCommand _removeSelectedScheduleCommand;
        private RelayCommand _addBlackoutCommand;
        private RelayCommand _removeSelectedBlackoutCommand;
        private StartupTaskState _launchAtStartup;

        public Settings Settings => _settings ?? (_settings = ReadSettings());

        public ActionBase SelectedAction
        {
            get => (_selectedAction == null) && (Settings.Actions != null) && Settings.Actions.Any() ? Settings.Actions.First() : _selectedAction;
            set
            {
                _selectedAction = value;
                OnPropertyChanged();
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

        public string Version => String.Format("{0}.{1}.{2}", Assembly.GetExecutingAssembly().GetName().Version.Major, Assembly.GetExecutingAssembly().GetName().Version.Minor, Assembly.GetExecutingAssembly().GetName().Version.Build);

        public string HomePage => StaticCode.HomePageUrl;

        public string MailAddress => StaticCode.MailAddress;

        public string TwitterUrl => StaticCode.TwitterUrl;

        public string GitHubUrl => StaticCode.GitHubUrl;

        public string PayPalUrl => StaticCode.PayPalUrl;

        public string Copyright => ((AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyCopyrightAttribute), false)).Copyright;

        public SettingsWindowViewModel()
        {
            if (Settings.EnableLogging)
            {
                StaticCode.EnableLog();
            }

            StaticCode.Logger?.Here().Information($"WorkingDirectory = {StaticCode.WorkingDirectory}");
            ReadSettings();
            _removeSelectedActionCommand = new RelayCommand(param => RemoveSelectedAction(), param => CanRemoveSelectedAction());
            _moveUpSelectedActionCommand = new RelayCommand(param => MoveUpSelectedAction(), param => CanMoveUpSelectedAction());
            _moveDownSelectedActionCommand = new RelayCommand(param => MoveDownSelectedAction(), param => CanMoveDownSelectedAction());
            _removeSelectedScheduleCommand = new RelayCommand(RemoveSelectedSchedule);
            _addBlackoutCommand = new RelayCommand(param => AddBlackout());
            _removeSelectedBlackoutCommand = new RelayCommand(RemoveSelectedBlackout);
            Settings.PropertyChanged += Settings_PropertyChanged;
            RefreshStartupTask();
        }

        public async void RefreshStartupTask()
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                if (StaticCode.DownloadSource == StaticCode.MoveMouseSource.MicrosoftStore)
                {
                    var startupTask = await StartupTask.GetAsync("MoveMouseStartupTask");
                    _launchAtStartup = startupTask.State;
                }
                else
                {
                    var runKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                    _launchAtStartup = (runKey?.GetValue(StaticCode.RunRegistryValueName) != null) ? StartupTaskState.Enabled : StartupTaskState.Disabled;
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
                _launchAtStartup = StartupTaskState.Disabled;
            }

            OnPropertyChanged(nameof(LaunchAtStartup));
        }

        private async void ToggleStartupTask()
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                if (StaticCode.DownloadSource == StaticCode.MoveMouseSource.MicrosoftStore)
                {
                    var startupTask = await StartupTask.GetAsync("MoveMouseStartupTask");

                    if (startupTask.State.Equals(StartupTaskState.Enabled))
                    {
                        startupTask.Disable();
                    }
                    else
                    {
                        await startupTask.RequestEnableAsync();
                    }
                }
                else
                {
                    var runKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);

                    if (LaunchAtStartup == StartupTaskState.Enabled)
                    {
                        runKey?.DeleteValue(StaticCode.RunRegistryValueName);
                    }
                    else
                    {
                        runKey?.SetValue(StaticCode.RunRegistryValueName, $"\"{Assembly.GetExecutingAssembly().Location}\"");
                    }
                }
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
                SaveSettings(Settings);

                switch (e.PropertyName)
                {
                    //case "LaunchAtLogon":
                    //    {
                    //        if (Settings.LaunchAtLogon)
                    //        {
                    //            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                    //            key?.SetValue("Move Mouse", $"\"{Assembly.GetExecutingAssembly().Location}\" /WorkingDirectory:\"{StaticCode.WorkingDirectory}\"");
                    //        }
                    //        else
                    //        {
                    //            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                    //            key?.DeleteValue("Move Mouse");
                    //        }

                    //        break;
                    //    }
                    case "EnableLogging":
                        {
                            if (Settings.EnableLogging)
                            {
                                StaticCode.EnableLog();
                            }
                            else
                            {
                                StaticCode.DisableLog();
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

        private void SaveSettings(Settings settings)
        {
            StaticCode.Logger?.Here().Debug(StaticCode.SettingsXmlPath);

            try
            {
                var xs = new XmlSerializer(typeof(Settings));
                var sw = new StreamWriter(StaticCode.SettingsXmlPath, false);
                xs.Serialize(sw, settings);
                sw.Close();
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private Settings ReadSettings()
        {
            StaticCode.Logger?.Here().Debug(StaticCode.SettingsXmlPath);
            StreamReader sr = null;

            try
            {
                if (File.Exists(StaticCode.SettingsXmlPath))
                {
                    var xs = new XmlSerializer(typeof(Settings));
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

            return new Settings();
        }

        public void AddAction(Type actionType)
        {
            StaticCode.Logger?.Here().Debug(actionType.ToString());

            try
            {
                var action = (ActionBase)Activator.CreateInstance(actionType);
                var actions = (Settings.Actions == null) ? new List<ActionBase>() : new List<ActionBase>(Settings.Actions);

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
                StaticCode.Logger?.Here().Debug(SelectedAction.ToString());
                Settings.Actions = new List<ActionBase>(Settings.Actions.Except(new[] { SelectedAction })).ToArray();
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private bool CanRemoveSelectedAction()
        {
            return (SelectedAction != null) && (Settings.Actions.Length > 1);
        }

        private void MoveUpSelectedAction()
        {
            try
            {
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
                return (SelectedAction != null) && (Settings.Actions != null) && (Settings.Actions.Any()) && !Settings.Actions.ToList().FindIndex(action => action.Id.Equals(SelectedAction.Id)).Equals(0);
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
                return (SelectedAction != null) && (Settings.Actions != null) && Settings.Actions.Any() && !Settings.Actions.ToList().FindIndex(action => action.Id.Equals(SelectedAction.Id)).Equals(Settings.Actions.Length - 1);
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
                var schedules = (Settings.Schedules == null) ? new List<ScheduleBase>() : new List<ScheduleBase>(Settings.Schedules);
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
            StaticCode.Logger?.Here().Debug(id.ToString());

            try
            {
                if (id != null)
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
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                var blackouts = (Settings.Blackouts == null) ? new List<Blackout>() : new List<Blackout>(Settings.Blackouts);
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
            StaticCode.Logger?.Here().Debug(id.ToString());

            try
            {
                if (id != null)
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

        //public void LogActivity(string details)
        //{
        //    _activityLog?.LogActivity(details);
        //    OnPropertyChanged("Activities");
        //}

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