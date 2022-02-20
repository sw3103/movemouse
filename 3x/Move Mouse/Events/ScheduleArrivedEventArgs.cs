using System;

namespace Ellanet.Events
{
    public class ScheduleArrivedEventArgs : EventArgs
    {
        public ScheduleAction Action { get; internal set; }
        public TimeSpan Time { get; internal set; }

        public enum ScheduleAction
        {
            Start,
            Pause
        }

        public ScheduleArrivedEventArgs(ScheduleAction action, TimeSpan time)
        {
            Action = action;
            Time = time;
        }
    }
}
