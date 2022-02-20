using System;

namespace Ellanet.Events
{
    public class BlackoutStatusChangedEventArgs : EventArgs
    {
        public BlackoutStatus Status { get; internal set; }
        public TimeSpan StartTime { get; internal set; }
        public TimeSpan EndTime { get; internal set; }

        public enum BlackoutStatus
        {
            Active,
            Inactive
        }

        public BlackoutStatusChangedEventArgs(BlackoutStatus status, TimeSpan startTime, TimeSpan endTime)
        {
            Status = status;
            StartTime = startTime;
            EndTime = endTime;
        }
    }
}
