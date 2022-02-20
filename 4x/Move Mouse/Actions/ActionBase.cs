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
    public abstract class ActionBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _name;
        private bool _isEnabled;
        private bool _repeat;
        private EventTrigger _trigger;

        public enum EventTrigger
        {
            Start,
            Interval,
            Stop
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

        protected ActionBase()
        {
            Id = Guid.NewGuid();
            _isEnabled = true;
            _repeat = true;
            _trigger = EventTrigger.Interval;
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
    }
}