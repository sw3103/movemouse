using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace ellabi.Schedules
{
    public abstract class ScheduleBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ScheduleAction _action;
        private bool _isEnabled;

        public enum ScheduleAction
        {
            Start,
            Stop
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

        public abstract bool IsValid { get; }

        [XmlIgnore]
        public abstract string CronExpression { get; }

        public Guid Id { get; set; }

        public IEnumerable<ScheduleAction> ScheduleActionValues => Enum.GetValues(typeof(ScheduleAction)).Cast<ScheduleAction>();

        public ScheduleAction Action
        {
            get => _action;
            set
            {
                _action = value;
                OnPropertyChanged();
            }
        }

        protected ScheduleBase()
        {
            Id = Guid.NewGuid();
            _isEnabled = true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}