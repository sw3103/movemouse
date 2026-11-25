using System;
using System.IO;

namespace ellabi.Platform
{
    internal static class LinuxPowerService
    {
        private const string PowerSupplyPath = "/sys/class/power_supply";

        public static bool IsRunningOnBattery()
        {
            if (OperatingSystem.IsWindows())
            {
                return WindowsPowerService.IsRunningOnBattery();
            }

            try
            {
                if (!Directory.Exists(PowerSupplyPath)) return false;

                bool hasBattery = false;
                bool anyMainsOnline = false;
                bool anyBatteryDischarging = false;

                foreach (var dir in Directory.GetDirectories(PowerSupplyPath))
                {
                    string typeFile = Path.Combine(dir, "type");
                    if (!File.Exists(typeFile)) continue;

                    string type = File.ReadAllText(typeFile).Trim();

                    if (string.Equals(type, "Mains", StringComparison.OrdinalIgnoreCase))
                    {
                        string onlineFile = Path.Combine(dir, "online");
                        if (File.Exists(onlineFile) && File.ReadAllText(onlineFile).Trim() == "1")
                        {
                            anyMainsOnline = true;
                        }
                    }
                    else if (string.Equals(type, "Battery", StringComparison.OrdinalIgnoreCase))
                    {
                        hasBattery = true;
                        string statusFile = Path.Combine(dir, "status");
                        if (File.Exists(statusFile))
                        {
                            string status = File.ReadAllText(statusFile).Trim();
                            if (string.Equals(status, "Discharging", StringComparison.OrdinalIgnoreCase))
                            {
                                anyBatteryDischarging = true;
                            }
                        }
                    }
                }

                if (anyMainsOnline) return false;
                return hasBattery && anyBatteryDischarging;
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
                return false;
            }
        }
    }
}
