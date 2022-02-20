using System;

namespace ellabi.Schedules
{
    public class AdvancedSchedule : ScheduleBase
    {
        private string _schedule;

        public override bool IsValid => !String.IsNullOrWhiteSpace(CronExpression) && Quartz.CronExpression.IsValidExpression(CronExpression);

        public override string CronExpression => Schedule;

        public string Schedule
        {
            get => _schedule;
            set
            {
                _schedule = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValid));
            }
        }
    }
}