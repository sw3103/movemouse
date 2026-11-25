using System;
using System.Runtime.InteropServices;

namespace ellabi.Platform
{
    internal static class WindowsNative
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct LastInputInfo
        {
            public uint Size;
            public uint Time;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct INPUT
        {
            public uint Type;
            public InputUnion U;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct InputUnion
        {
            [FieldOffset(0)] public MouseInput Mouse;
            [FieldOffset(0)] public KeyboardInput Keyboard;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MouseInput
        {
            public int Dx;
            public int Dy;
            public uint MouseData;
            public uint Flags;
            public uint Time;
            public nuint ExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct KeyboardInput
        {
            public ushort VirtualKey;
            public ushort ScanCode;
            public uint Flags;
            public uint Time;
            public nuint ExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SystemPowerStatus
        {
            public byte AcLineStatus;
            public byte BatteryFlag;
            public byte BatteryLifePercent;
            public byte SystemStatusFlag;
            public uint BatteryLifeTime;
            public uint BatteryFullLifeTime;
        }

        internal const uint InputMouse = 0;
        internal const uint InputKeyboard = 1;
        internal const uint MouseeventfMove = 0x0001;
        internal const uint MouseeventfLeftdown = 0x0002;
        internal const uint MouseeventfLeftup = 0x0004;
        internal const uint MouseeventfRightdown = 0x0008;
        internal const uint MouseeventfRightup = 0x0010;
        internal const uint MouseeventfMiddledown = 0x0020;
        internal const uint MouseeventfMiddleup = 0x0040;
        internal const uint MouseeventfWheel = 0x0800;
        internal const uint MouseeventfHwheel = 0x01000;
        internal const uint MouseeventfAbsolute = 0x8000;
        internal const uint KeyeventfKeyup = 0x0002;
        internal const uint KeyeventfExtendedkey = 0x0001;
        internal const byte AcLineOffline = 0;
        internal const byte AcLineOnline = 1;

        [DllImport("user32.dll")]
        internal static extern bool GetLastInputInfo(ref LastInputInfo plii);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool GetCursorPos(out POINT pt);

        [DllImport("user32.dll")]
        internal static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("kernel32.dll")]
        internal static extern bool GetSystemPowerStatus(out SystemPowerStatus lpSystemPowerStatus);

        [DllImport("user32.dll")]
        internal static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        internal const uint SwpNomove = 0x0002;
        internal const uint SwpNozorder = 0x0004;
        internal const uint SwpNoactivate = 0x0010;
        internal const uint SwpShowwindow = 0x0040;

        internal const int SmRemoteSession = 0x1000;

        internal static void SetWindowPos(nint hWnd, int width, int height)
        {
            SetWindowPos(hWnd, nint.Zero, 0, 0, width, height, SwpNomove | SwpNozorder | SwpNoactivate);
        }

        internal static void SetWindowSize(nint hWnd, int width, int height, bool show = true)
        {
            uint flags = SwpNomove | SwpNozorder;
            if (show)
            {
                flags |= SwpShowwindow;
            }

            SetWindowPos(hWnd, nint.Zero, 0, 0, width, height, flags);
        }
    }
}