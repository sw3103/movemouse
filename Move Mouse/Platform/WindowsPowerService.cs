using System;

namespace ellabi.Platform
{
    internal static class WindowsPowerService
    {
        public static bool IsRunningOnBattery()
        {
            try
            {
                if (!WindowsNative.GetSystemPowerStatus(out var status))
                {
                    return false;
                }

                return status.AcLineStatus == WindowsNative.AcLineOffline;
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
                return false;
            }
        }
    }
}