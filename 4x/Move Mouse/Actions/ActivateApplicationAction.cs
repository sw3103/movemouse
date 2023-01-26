using ellabi.Utilities;
using ellabi.Wrappers;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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

        public override bool IsValid => !String.IsNullOrWhiteSpace(Application);

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
                    if (!String.IsNullOrWhiteSpace(Application))
                    {
                        availableApplications.Add(Application);
                    }

                    foreach (var process in Process.GetProcesses().Where(p => p.Responding))
                    {
                        try
                        {
                            if (!String.IsNullOrEmpty(process.MainWindowTitle))
                            {
                                var application = Mode.Equals(SearchMode.Window) ? process.MainWindowTitle : process.ProcessName;

                                if (!availableApplications.Contains(application))
                                {
                                    availableApplications.Add(application);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            StaticCode.Logger?.Here().Error(ex.Message);
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

        public override bool CanExecute()
        {
            return IsValid;
        }

        public override void Execute()
        {
            try
            {
                StaticCode.Logger?.Here().Information(ToString());
                string windowName = null;

                if (Mode.Equals(SearchMode.Window) && (Application.StartsWith("*") || Application.EndsWith("*")))
                {
                    if (Application.StartsWith("*") && Application.EndsWith("*"))
                    {
                        windowName = Process.GetProcesses().FirstOrDefault(p => !String.IsNullOrEmpty(p.MainWindowTitle) && (p.MainWindowTitle.IndexOf(Application.Trim(Convert.ToChar("*")), StringComparison.CurrentCultureIgnoreCase) >= 0))?.MainWindowTitle;
                    }
                    else if (Application.StartsWith("*"))
                    {
                        windowName = Process.GetProcesses().FirstOrDefault(p => !String.IsNullOrEmpty(p.MainWindowTitle) && p.MainWindowTitle.EndsWith(Application.Trim(Convert.ToChar("*")), StringComparison.CurrentCultureIgnoreCase))?.MainWindowTitle;
                    }
                    else if (Application.EndsWith("*"))
                    {
                        windowName = Process.GetProcesses().FirstOrDefault(p => !String.IsNullOrEmpty(p.MainWindowTitle) && p.MainWindowTitle.StartsWith(Application.Trim(Convert.ToChar("*")), StringComparison.CurrentCultureIgnoreCase))?.MainWindowTitle;
                    }
                }
                else
                {
                    windowName = Mode.Equals(SearchMode.Window) ? Application : Process.GetProcessesByName(Application).FirstOrDefault(p => !String.IsNullOrEmpty(p.MainWindowTitle))?.MainWindowTitle;
                }

                //Debug.WriteLine(windowName);

                if (!String.IsNullOrEmpty(windowName))
                {
                    IntPtr handle = NativeMethods.FindWindow(null, windowName);

                    if (handle != IntPtr.Zero)
                    {
                        if (IsWindowMinimised(handle))
                        {
                            NativeMethods.ShowWindow(handle, NativeMethods.ShowWindowCommands.Restore);
                        }

                        Interaction.AppActivate(windowName);
                    }
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private bool IsWindowMinimised(IntPtr handle)
        {
            try
            {
                var placement = new NativeMethods.WINDOWPLACEMENT();
                placement.length = Marshal.SizeOf(placement);
                NativeMethods.GetWindowPlacement(handle, ref placement);
                return placement.showCmd == NativeMethods.ShowWindowCommands.ShowMinimized;
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }

            return false;
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
            return $"{this.GetType().Name} | Name = {Name} | Mode = {Mode} | Application = {Application} | Trigger = {Trigger} | Repeat = {Repeat} | RepeatMode = {RepeatMode} | IntervalThrottle = {IntervalThrottle} | IntervalExecutionCount = {IntervalExecutionCount}";
        }
    }
}