using System;

namespace ellabi.Platform
{
    internal static class WindowsIdleService
    {
        public static TimeSpan GetLastInputTime()
        {
            try
            {
                var info = new WindowsNative.LastInputInfo { Size = (uint)System.Runtime.InteropServices.Marshal.SizeOf<WindowsNative.LastInputInfo>() };
                if (!WindowsNative.GetLastInputInfo(ref info))
                {
                    return TimeSpan.Zero;
                }

                uint tickCount = unchecked((uint)Environment.TickCount);
                uint idleMs = tickCount - info.Time;
                return TimeSpan.FromMilliseconds(idleMs);
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }
    }
}