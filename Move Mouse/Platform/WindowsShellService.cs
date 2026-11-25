using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ellabi.Platform
{
    internal static class WindowsShellService
    {
        private const int SwRestore = 9;
        private const int SwShow = 5;
        private delegate bool EnumWindowsProc(nint hwnd, nint lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, nint lParam);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(nint hwnd, out uint lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(nint hwnd, System.Text.StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(nint hwnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(nint hwnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(nint hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(nint hWnd);

        [DllImport("user32.dll")]
        private static extern bool BringWindowToTop(nint hWnd);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
            public int Width => Right - Left;
            public int Height => Bottom - Top;
            public long Area => (long)Width * Height;
        }

        /// <summary>
        /// Avalonia on Win32 may create a tiny caption HWND (~160x28) alongside the real client surface.
        /// Resizes and foregrounds the largest visible top-level window belonging to this process.
        /// </summary>
        public static void RestoreMainWindow(Window window, int logicalSize = 600, bool ensureVisible = true)
        {
            if (!OperatingSystem.IsWindows() || window == null)
            {
                return;
            }

            try
            {
                window.Width = logicalSize;
                window.Height = logicalSize;
                window.MinWidth = logicalSize;
                window.MinHeight = logicalSize;

                if (!ensureVisible)
                {
                    return;
                }

                if (window.WindowState == WindowState.Minimized)
                {
                    window.WindowState = WindowState.Normal;
                }

                if (!window.IsVisible)
                {
                    window.Show();
                }

                nint target = ResolveBestMainWindowHandle(window);
                if (target != nint.Zero)
                {
                    int pixels = ScaleToDevicePixels(window, logicalSize);
                    WindowsNative.SetWindowSize(target, pixels, pixels);

                    ShowWindow(target, SwRestore);
                    ShowWindow(target, SwShow);
                    BringWindowToTop(target);
                    SetForegroundWindow(target);
                }

                if (window.Bounds.Width < logicalSize - 50 || window.Bounds.Height < logicalSize - 50)
                {
                    window.Hide();
                    window.Show();
                    window.WindowState = WindowState.Normal;
                }

                ApplyBorderlessChrome(window);
                window.Activate();
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        internal static void ApplyBorderlessChrome(Window window)
        {
            window.SystemDecorations = SystemDecorations.None;
            window.ExtendClientAreaToDecorationsHint = false;
            window.TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent };
            window.Background = Brushes.Transparent;
        }

        private static int ScaleToDevicePixels(Window window, int logical)
        {
            double scale = window.RenderScaling;
            if (scale <= 0) scale = 1.0;
            return Math.Max(logical, (int)Math.Round(logical * scale));
        }

        private static nint ResolveBestMainWindowHandle(Window window)
        {
            nint platform = window.TryGetPlatformHandle()?.Handle ?? nint.Zero;
            uint pid = (uint)Environment.ProcessId;

            var candidates = new List<(nint Hwnd, long Area, string Title)>();
            EnumWindows((hwnd, _) =>
            {
                GetWindowThreadProcessId(hwnd, out uint windowPid);
                if (windowPid != pid || !IsWindowVisible(hwnd))
                {
                    return true;
                }

                GetWindowRect(hwnd, out RECT r);
                if (r.Width <= 0 || r.Height <= 0)
                {
                    return true;
                }

                var sb = new System.Text.StringBuilder(256);
                GetWindowText(hwnd, sb, sb.Capacity);
                candidates.Add((hwnd, r.Area, sb.ToString()));
                return true;
            }, nint.Zero);

            if (candidates.Count == 0)
            {
                return platform;
            }

            // Prefer the largest surface — the titled HWND is often ~160x28 while the client is much larger.
            candidates.Sort((a, b) => b.Area.CompareTo(a.Area));

            return candidates[0].Hwnd;
        }
    }
}