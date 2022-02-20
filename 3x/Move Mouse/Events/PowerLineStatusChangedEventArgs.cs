using System;

namespace Ellanet.Events
{
    public class PowerLineStatusChangedEventArgs: EventArgs
    {
        public PowerLineStatus Status { get; internal set; }

        public enum PowerLineStatus
        {
            Online,
            Offline,
            Unknown
        }

        public PowerLineStatusChangedEventArgs(PowerLineStatus status)
        {
            Status = status;
        }
    }
}
