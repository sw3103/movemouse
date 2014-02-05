using System;

namespace Ellanet.Events
{
    public class ScheduleArrivedEventArgs : EventArgs
    {
        public ScheduleAction Action { get; private set; }
        public TimeSpan Time { get; private set; }

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
