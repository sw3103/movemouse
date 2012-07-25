using System;
using System.Collections.Generic;
using System.Text;

namespace Ellanet
{
    public class BlackoutStatusChangeEventArgs : EventArgs
    {
        public BlackoutStatus Status
        {
            get;
            internal set;
        }

        public string StartTime
        {
            get;
            internal set;
        }

        public string EndTime
        {
            get;
            internal set;
        }

        public enum BlackoutStatus
        {
            Started,
            Ended
        }

        public BlackoutStatusChangeEventArgs(BlackoutStatus status, string startTime, string endTime)
        {
            this.Status = status;
            this.StartTime = startTime;
            this.EndTime = endTime;
        }
    }
}
