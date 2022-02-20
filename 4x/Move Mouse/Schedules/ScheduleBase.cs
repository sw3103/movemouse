using ellabi.Annotations;
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

        public enum ScheduleAction
        {
            Start,
            Stop
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
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}