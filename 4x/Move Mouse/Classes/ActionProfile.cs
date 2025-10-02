using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ellabi.Actions;
using ellabi.Annotations;

namespace ellabi.Classes
{
    [Serializable]
    public class ActionProfile : INotifyPropertyChanged
    {
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        private string _name;
        private int _lowerInterval = 30;
        private int _upperInterval = 60;
        private bool _randomInterval = false;
        private bool _autoPause = false;
        private bool _autoResume = false;
        private int _autoResumeSeconds = 30;
        private bool _adjustRunningVolume = false;
        private int _runningVolume = 30;
        private bool _activeWhenLocked = false;
        private bool _pauseOnBattery = false;
        private bool _enableLogging = false;

        public Guid Id { get; set; } = Guid.NewGuid();
        
        public string Name 
        { 
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public List<ActionBase> Actions { get; set; } = new List<ActionBase>();

        // Profile-specific timing settings
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

        // Profile-specific behavior settings
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

        public bool AdjustRunningVolume
        {
            get => _adjustRunningVolume;
            set
            {
                _adjustRunningVolume = value;
                OnPropertyChanged();
            }
        }

        public int RunningVolume
        {
            get => _runningVolume;
            set
            {
                _runningVolume = value;
                OnPropertyChanged();
            }
        }

        public bool ActiveWhenLocked
        {
            get => _activeWhenLocked;
            set
            {
                _activeWhenLocked = value;
                OnPropertyChanged();
            }
        }

        public bool PauseOnBattery
        {
            get => _pauseOnBattery;
            set
            {
                _pauseOnBattery = value;
                OnPropertyChanged();
            }
        }

        public bool EnableLogging
        {
            get => _enableLogging;
            set
            {
                _enableLogging = value;
                OnPropertyChanged();
            }
        }

        public ActionProfile() { }

        public ActionProfile(string name)
        {
            Name = name;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}