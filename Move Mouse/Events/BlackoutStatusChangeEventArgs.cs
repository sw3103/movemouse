using System;

namespace Ellanet.Events
{
    public class BlackoutStatusChangeEventArgs : EventArgs
    {
        public BlackoutStatus Status { get; internal set; }
        public string StartTime { get; internal set; }
        public string EndTime { get; internal set; }

        public enum BlackoutStatus
        {
            Active,
            Inactive
        }

        public BlackoutStatusChangeEventArgs(BlackoutStatus status, string startTime, string endTime)
        {
            Status = status;
            StartTime = startTime;
            EndTime = endTime;
        }
    }
}
