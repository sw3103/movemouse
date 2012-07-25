using System;
using System.Collections.Generic;
using System.Text;

namespace Ellanet
{
    public class NewVersionAvailableEventArgs : EventArgs
    {
        public Version Version
        {
            get;
            internal set;
        }

        public DateTime Released
        {
            get;
            internal set;
        }

        public DateTime Advertised
        {
            get;
            internal set;
        }

        public string[] Features
        {
            get;
            internal set;
        }

        public string[] Fixes
        {
            get;
            internal set;
        }

        public NewVersionAvailableEventArgs(Version version, DateTime released, DateTime advertised, string[] features, string[] fixes)
        {
            this.Version = version;
            this.Released = released;
            this.Advertised = advertised;
            this.Features = features;
            this.Fixes = fixes;
        }
    }
}
