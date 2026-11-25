using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ellabi.Platform
{
    internal static class LinuxAudioService
    {
        public static async Task<double?> GetDefaultVolumeAsync()
        {
            // 1. Try WirePlumber (wpctl)
            string wpctlOut = await RunCommandAsync("wpctl", "get-volume", "@DEFAULT_AUDIO_SINK@");
            if (!string.IsNullOrWhiteSpace(wpctlOut))
            {
                // Output: "Volume: 0.45" or "Volume: 0.45 [MUTED]"
                var match = Regex.Match(wpctlOut, @"Volume:\s*([0-9]+(?:\.[0-9]+)?)");
                if (match.Success && double.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double vol))
                {
                    return Math.Clamp(vol * 100.0, 0.0, 100.0);
                }
            }

            // 2. Try PulseAudio / PipeWire-Pulse (pactl)
            string pactlOut = await RunCommandAsync("pactl", "get-sink-volume", "@DEFAULT_SINK@");
            if (!string.IsNullOrWhiteSpace(pactlOut))
            {
                var match = Regex.Match(pactlOut, @"(\d+)%");
                if (match.Success && double.TryParse(match.Groups[1].Value, out double vol))
                {
                    return Math.Clamp(vol, 0.0, 100.0);
                }
            }

            return null;
        }

        public static async Task SetDefaultVolumeAsync(double volumePercent)
        {
            double clamped = Math.Clamp(volumePercent, 0.0, 100.0);
            string wpVal = (clamped / 100.0).ToString("0.00", CultureInfo.InvariantCulture);

            if (await RunCommandExitCodeAsync("wpctl", "set-volume", "@DEFAULT_AUDIO_SINK@", wpVal) == 0)
            {
                return;
            }

            int pct = (int)Math.Round(clamped);
            await RunCommandExitCodeAsync("pactl", "set-sink-volume", "@DEFAULT_SINK@", $"{pct}%");
        }

        private static async Task<string> RunCommandAsync(string fileName, params string[] args)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                foreach (var arg in args) psi.ArgumentList.Add(arg);

                using var proc = Process.Start(psi);
                if (proc == null) return null;
                string output = await proc.StandardOutput.ReadToEndAsync();
                await proc.WaitForExitAsync();
                return proc.ExitCode == 0 ? output : null;
            }
            catch
            {
                return null;
            }
        }

        private static async Task<int> RunCommandExitCodeAsync(string fileName, params string[] args)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                foreach (var arg in args) psi.ArgumentList.Add(arg);

                using var proc = Process.Start(psi);
                if (proc == null) return -1;
                await proc.WaitForExitAsync();
                return proc.ExitCode;
            }
            catch
            {
                return -1;
            }
        }
    }
}
