using System;

namespace Ellanet.Events
{
    public class NewVersionAvailableEventArgs : EventArgs
    {
        public Version Version { get; internal set; }
        public DateTime Released { get; internal set; }
        public DateTime Advertised { get; internal set; }
        public string[] Features { get; internal set; }
        public string[] Fixes { get; internal set; }
        public string DownloadUrl { get; internal set; }

        public NewVersionAvailableEventArgs(Version version, DateTime released, DateTime advertised, string[] features, string[] fixes, string downloadUrl)
        {
            Version = version;
            Released = released;
            Advertised = advertised;
            Features = features;
            Fixes = fixes;
            DownloadUrl = downloadUrl;
        }
    }
}