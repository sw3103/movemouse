using System;
using System.Diagnostics;
using System.Linq;

namespace ellabi.Platform
{
    internal static class WindowsSessionLockService
    {
        public static bool IsSessionLocked()
        {
            try
            {
                return Process.GetProcessesByName("LogonUI").Any();
            }
            catch
            {
                return false;
            }
        }
    }
}