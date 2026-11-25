using System.Collections.Generic;

namespace ellabi.Platform
{
    internal static class WindowsWindowService
    {
        public static List<LinuxWindowService.WindowEntry> GetOpenWindows() => new List<LinuxWindowService.WindowEntry>();

        public static bool ActivateWindow(string pattern, bool byWindowTitle) => false;
    }
}