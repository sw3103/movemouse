using ellabi.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace ellabi.Classes
{
    public class Blackout : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _monday;
        private bool _tuesday;
        private bool _wednesday;
        private bool _thursday;
        private bool _friday;
        private bool _saturday;
        private bool _sunday;
        private TimeSpan _time;
        private TimeSpan _duration;

        public bool IsValid => (Monday || Tuesday || Wednesday || Thursday || Friday || Saturday || Sunday);

        public Guid Id { get; set; }

        public bool Monday
        {
            get { return _monday; }
            set
            {
                _monday = value;

                if (!IsValid)
                {
                    _monday = true;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValid));
            }
        }

        public bool Tuesday
        {
            get { return _tuesday; }
            set
            {
                _tuesday = value;

                if (!IsValid)
                {
                    _tuesday = true;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValid));
            }
        }

        public bool Wednesday
        {
            get { return _wednesday; }
            set
            {
                _wednesday = value;

                if (!IsValid)
                {
                    _wednesday = true;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValid));
            }
        }

        public bool Thursday
        {
            get { return _thursday; }
            set
            {
                _thursday = value;

                if (!IsValid)
                {
                    _thursday = true;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValid));
            }
        }

        public bool Friday
        {
            get { return _friday; }
            set
            {
                _friday = value;

                if (!IsValid)
                {
                    _friday = true;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValid));
            }
        }

        public bool Saturday
        {
            get { return _saturday; }
            set
            {
                _saturday = value;

                if (!IsValid)
                {
                    _saturday = true;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValid));
            }
        }

        public bool Sunday
        {
            get { return _sunday; }
            set
            {
                _sunday = value;

                if (!IsValid)
                {
                    _sunday = true;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValid));
            }
        }

        [XmlIgnore]
        public TimeSpan Time
        {
            get { return _time; }
            set
            {
                _time = value;
                OnPropertyChanged();
            }
        }

        [XmlElement(DataType = "duration", ElementName = "Time")]
        public string TimeString
        {
            get { return _time.ToString(); }
            set
            {
                try
                {
                    _time = String.IsNullOrEmpty(value) ? TimeSpan.Zero : TimeSpan.Parse(value);
                }
                catch (Exception ex)
                {
                    StaticCode.Logger?.Here().Error(ex.Message);
                }
            }
        }

        [XmlIgnore]
        public TimeSpan Duration
        {
            get { return _duration; }
            set
            {
                _duration = value;
                OnPropertyChanged();
            }
        }

        [XmlElement(DataType = "duration", ElementName = "Duration")]
        public string DurationString
        {
            get { return _duration.ToString(); }
            set
            {
                try
                {
                    _duration = String.IsNullOrEmpty(value) ? TimeSpan.Zero : TimeSpan.Parse(value);
                }
                catch (Exception ex)
                {
                    StaticCode.Logger?.Here().Error(ex.Message);
                }
            }
        }

        [XmlIgnore]
        public DayOfWeek[] EnabledDays
        {
            get
            {
                var days = new List<DayOfWeek>();

                try
                {
                    //todo Is there a more elegant way to do this?
                    if (Monday) days.Add(DayOfWeek.Monday);
                    if (Tuesday) days.Add(DayOfWeek.Tuesday);
                    if (Wednesday) days.Add(DayOfWeek.Wednesday);
                    if (Thursday) days.Add(DayOfWeek.Thursday);
                    if (Friday) days.Add(DayOfWeek.Friday);
                    if (Saturday) days.Add(DayOfWeek.Saturday);
                    if (Sunday) days.Add(DayOfWeek.Sunday);
                }
                catch (Exception ex)
                {
                    StaticCode.Logger?.Here().Error(ex.Message);
                }

                return days.ToArray();
            }
        }

        public Blackout()
        {
            try
            {
                Id = Guid.NewGuid();
                Time = DateTime.Now.TimeOfDay.Add(TimeSpan.FromSeconds(-1));
                Duration = TimeSpan.FromHours(1);
                Monday = true;
                Tuesday = true;
                Wednesday = true;
                Thursday = true;
                Friday = true;
                Saturday = true;
                Sunday = true;
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

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