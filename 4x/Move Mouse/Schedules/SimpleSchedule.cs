using System;
using System.Text;
using System.Xml.Serialization;

namespace ellabi.Schedules
{
    public class SimpleSchedule : ScheduleBase
    {
        private bool _monday;
        private bool _tuesday;
        private bool _wednesday;
        private bool _thursday;
        private bool _friday;
        private bool _saturday;
        private bool _sunday;
        private TimeSpan _time;
        private int _delay;
        public override bool IsValid => (Monday || Tuesday || Wednesday || Thursday || Friday || Saturday || Sunday) && (Time < TimeSpan.FromHours(24));

        public override string CronExpression
        {
            get
            {
                var time = Delay == 0 ? Time : Time.Add(TimeSpan.FromSeconds(new Random().Next(0, Delay)));
                if (time.TotalDays >= 1) time = new TimeSpan(23, 59, 59);
                return String.Format("{0} {1} {2} ? * {3}", time.Seconds, time.Minutes, time.Hours, BuildDayOfWeekPart());
            }
        }

        public bool Monday
        {
            get => _monday;
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
            get => _tuesday;
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
            get => _wednesday;
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
            get => _thursday;
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
            get => _friday;
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
            get => _saturday;
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
            get => _sunday;
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
            get => _time;
            set
            {
                _time = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValid));
            }
        }

        [XmlElement(DataType = "duration", ElementName = "Time")]
        public string TimeString
        {
            get => _time.ToString();
            set => _time = String.IsNullOrEmpty(value) ? TimeSpan.Zero : TimeSpan.Parse(value);
        }

        public int Delay
        {
            get => _delay;
            set
            {
                _delay = value;
                OnPropertyChanged();
            }
        }

        public SimpleSchedule()
        {
            Time = DateTime.Now.TimeOfDay.Add(TimeSpan.FromSeconds(-1));
            Monday = true;
            Tuesday = true;
            Wednesday = true;
            Thursday = true;
            Friday = true;
            Saturday = true;
            Sunday = true;
        }

        private string BuildDayOfWeekPart()
        {
            if (Monday && Tuesday && Wednesday && Thursday && Friday && Saturday && Sunday)
            {
                return "*";
            }

            var sb = new StringBuilder();
            if (Monday) sb.Append("MON,");
            if (Tuesday) sb.Append("TUE,");
            if (Wednesday) sb.Append("WED,");
            if (Thursday) sb.Append("THU,");
            if (Friday) sb.Append("FRI,");
            if (Saturday) sb.Append("SAT,");
            if (Sunday) sb.Append("SUN");
            return sb.ToString().Trim(',');
        }
    }
}