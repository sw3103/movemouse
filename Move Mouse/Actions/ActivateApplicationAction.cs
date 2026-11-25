using ellabi.Platform;
using ellabi.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace ellabi.Actions
{
    public class ActivateApplicationAction : ActionBase
    {
        private SearchMode _mode;
        private string _application;

        public enum SearchMode
        {
            Process,
            Window
        }

        public IEnumerable<SearchMode> SearchModeValues => Enum.GetValues(typeof(SearchMode)).Cast<SearchMode>();

        public override bool IsValid => !string.IsNullOrWhiteSpace(Application);

        public SearchMode Mode
        {
            get => _mode;
            set
            {
                _mode = value;
                Application = null;
                OnPropertyChanged();
                RefreshApplications();
            }
        }

        public string Application
        {
            get => _application;
            set
            {
                _application = value;
                OnPropertyChanged();
                ExecuteCommand?.RaiseCanExecuteChanged();
            }
        }

        [XmlIgnore]
        public RelayCommand RefreshApplicationsCommand { get; set; }

        [XmlIgnore]
        public List<string> AvailableApplications
        {
            get
            {
                var availableApplications = new List<string>();

                try
                {
                    if (!string.IsNullOrWhiteSpace(Application))
                    {
                        availableApplications.Add(Application);
                    }

                    foreach (var window in LinuxWindowService.GetOpenWindows())
                    {
                        string candidate = Mode == SearchMode.Window ? window.Title : window.ProcessName;
                        if (!string.IsNullOrWhiteSpace(candidate) && !availableApplications.Contains(candidate))
                        {
                            availableApplications.Add(candidate);
                        }
                    }
                }
                catch (Exception ex)
                {
                    StaticCode.Logger?.Here().Error(ex.Message);
                }

                return availableApplications.OrderBy(a => a).ToList();
            }
        }

        public ActivateApplicationAction()
        {
            RefreshApplicationsCommand = new RelayCommand(param => RefreshApplications());
        }

        public override bool CanExecute() => IsValid;

        public override void Execute()
        {
            try
            {
                IntervalExecutionCount++;
                StaticCode.Logger?.Here().Information(ToString());
                LinuxWindowService.ActivateWindow(Application, Mode == SearchMode.Window);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void RefreshApplications()
        {
            try
            {
                OnPropertyChanged(nameof(AvailableApplications));
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public override string ToString()
        {
            return $"{GetType().Name} | Name = {Name} | Mode = {Mode} | Application = {Application} | Trigger = {Trigger} | Repeat = {Repeat} | RepeatMode = {RepeatMode} | IntervalThrottle = {IntervalThrottle} | IntervalExecutionCount = {IntervalExecutionCount} | InterruptsIdleTime = {InterruptsIdleTime}";
        }
    }
}
