using System;

namespace Ellanet.Events
{
    public class PowerLineStatusChangedEventArgs : EventArgs
    {
        public PowerLineStatus Status { get; internal set; }

        // ReSharper disable UnusedMember.Global
        public enum PowerLineStatus
        {
            Online,
            Offline,
            Unknown
        }
        // ReSharper restore UnusedMember.Global

        public PowerLineStatusChangedEventArgs(PowerLineStatus status)
        {
            Status = status;
        }
    }
}