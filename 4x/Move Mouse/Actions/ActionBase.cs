using ellabi.Annotations;
using ellabi.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace ellabi.Actions
{
    [Serializable]
    [XmlInclude(typeof(ActivateApplicationAction))]
    [XmlInclude(typeof(ClickMouseAction))]
    [XmlInclude(typeof(CommandAction))]
    [XmlInclude(typeof(MoveMouseCursorAction))]
    [XmlInclude(typeof(PositionMouseCursorAction))]
    [XmlInclude(typeof(ScriptAction))]
    [XmlInclude(typeof(ScrollMouseAction))]
    [XmlInclude(typeof(SleepAction))]
    public abstract class ActionBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _name;
        private bool _isEnabled;
        private bool _repeat;
        private EventTrigger _trigger;
        private IntervalRepeatMode _repeatMode;
        private int _intervalThrottle;
        private int _intervalExecutionCount;
        private bool _interruptsIdleTime;

        public enum EventTrigger
        {
            Start,
            Interval,
            Stop
        }

        public enum IntervalRepeatMode
        {
            Forever,
            Throttle
        }

        public abstract bool IsValid { get; }

        public Guid Id { get; set; }

        public string Name
        {
            get => _name;
            set
            {
                _name = String.IsNullOrWhiteSpace(value) ? null : value;
                OnPropertyChanged();
            }
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool Repeat
        {
            get => _repeat;
            set
            {
                _repeat = value;
                OnPropertyChanged();
            }
        }

        [XmlIgnore]
        public RelayCommand ExecuteCommand { get; set; }

        public EventTrigger Trigger
        {
            get => _trigger;
            set
            {
                _trigger = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<EventTrigger> EventTriggerValues => Enum.GetValues(typeof(EventTrigger)).Cast<EventTrigger>();

        public IntervalRepeatMode RepeatMode
        {
            get => _repeatMode;
            set
            {
                _repeatMode = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<IntervalRepeatMode> IntervalRepeatModeValues => Enum.GetValues(typeof(IntervalRepeatMode)).Cast<IntervalRepeatMode>();

        public int IntervalThrottle
        {
            get => _intervalThrottle;
            set
            {
                _intervalThrottle = value;
                OnPropertyChanged();
            }
        }

        [XmlIgnore]
        public int IntervalExecutionCount
        {
            get => _intervalExecutionCount;
            set
            {
                _intervalExecutionCount = value;
                OnPropertyChanged();
            }
        }

        [XmlIgnore]
        public bool InterruptsIdleTime
        {
            get => _interruptsIdleTime;
            set
            {
                _interruptsIdleTime = value;
                OnPropertyChanged();
            }
        }

        protected ActionBase()
        {
            Id = Guid.NewGuid();
            _isEnabled = true;
            _repeat = true;
            _trigger = EventTrigger.Interval;
            _repeatMode = IntervalRepeatMode.Forever;
            _intervalThrottle = 1;
            ExecuteCommand = new RelayCommand(param => Execute(), param => CanExecute());
        }

        public abstract bool CanExecute();

        public abstract void Execute();

        public abstract override string ToString();

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

        /// <summary>
        /// Creates a deep copy of this action with a new ID
        /// </summary>
        /// <returns>A new action instance with the same properties but different ID</returns>
        public virtual ActionBase Clone()
        {
            var cloned = (ActionBase)MemberwiseClone();
            cloned.Id = Guid.NewGuid();
            return cloned;
        }
    }
}