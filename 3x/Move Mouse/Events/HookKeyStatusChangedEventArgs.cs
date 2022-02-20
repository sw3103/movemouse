using System;

namespace Ellanet.Events
{
    public class HookKeyStatusChangedEventArgs : EventArgs
    {
        public bool Enabled { get; internal set; }
        public System.Windows.Forms.Keys Key { get; internal set; }

        public HookKeyStatusChangedEventArgs(bool enabled, System.Windows.Forms.Keys key)
        {
            Enabled = enabled;
            Key = key;
        }
    }
}
