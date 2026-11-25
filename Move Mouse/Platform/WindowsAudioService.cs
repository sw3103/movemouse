using System;
using System.Threading.Tasks;

namespace ellabi.Platform
{
    internal static class WindowsAudioService
    {
        public static Task<double?> GetDefaultVolumeAsync() => Task.FromResult<double?>(null);

        public static Task SetDefaultVolumeAsync(double volumePercent) => Task.CompletedTask;
    }
}