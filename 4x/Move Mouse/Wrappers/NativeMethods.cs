using System;
using System.Drawing;
using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace ellabi.Wrappers
{
    internal class NativeMethods
    {
        //public const int WH_KEYBOARD_LL = 13;

        //public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [Flags]
        public enum MouseEventFlags
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010,
            WHEEL = 0x00000800,
            HWHEEL = 0x00001000,
            XDOWN = 0x00000080,
            XUP = 0x00000100
        }

        [Flags]
        public enum Win32Consts
        {
            INPUT_MOUSE = 0,
            INPUT_KEYBOARD = 1,
            INPUT_HARDWARE = 2,
        }

        [Flags]
        public enum ShowWindowCommands
        {
            Hide = 0,
            Normal = 1,
            ShowMinimized = 2,
            Maximize = 3,
            ShowMaximized = 3,
            ShowNoActivate = 4,
            Show = 5,
            Minimize = 6,
            ShowMinNoActive = 7,
            ShowNa = 8,
            Restore = 9,
            ShowDefault = 10,
            ForceMinimize = 11
        }

        [Flags]
        public enum ExtendedWindowStyles
        {
            // ...
            WS_EX_TOOLWINDOW = 0x00000080,
            // ...
        }

        public enum GetWindowLongFields
        {
            GWL_HWNDPARENT = (-8),
            GWL_ID = (-12),
            GWL_STYLE = (-16),
            GWL_EXSTYLE = (-20)
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LASTINPUTINFO
        {
            private static readonly int SizeOf = Marshal.SizeOf(typeof(LASTINPUTINFO));
            [MarshalAs(UnmanagedType.U4)] public int cbSize;
            [MarshalAs(UnmanagedType.U4)] public int dwTime;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public int type;
            public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public MouseEventFlags dwFlags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Win32Point
        {
            public int X;
            public int Y;
        };

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACEMENT
        {
            public int length;
            public readonly int flags;
            public readonly ShowWindowCommands showCmd;
            public readonly Point ptMinPosition;
            public readonly Point ptMaxPosition;
            public readonly Rectangle rcNormalPosition;
        }

        [DllImport("user32.dll")]
        public static extern uint SendInput(
            uint nInputs,
            ref INPUT pInputs,
            int cbSize);

        [DllImport("User32.dll")]
        public static extern bool SetCursorPos(
            int X,
            int Y);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(
            ref Win32Point pt);

        [DllImport("user32.dll")]
        public static extern bool GetLastInputInfo(
            ref LASTINPUTINFO plii);

        [DllImport("user32.dll")]
        public static extern void mouse_event(
            uint dwFlags,
            uint dx,
            uint dy,
            uint dwData,
            int dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(
            string lpClassName,
            string lpWindowName);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(
            IntPtr hWnd,
            ShowWindowCommands nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowPlacement(
            IntPtr hWnd,
            ref WINDOWPLACEMENT lpwndpl);

        //[DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        //private static extern IntPtr IntSetWindowLongPtr(
        //    IntPtr hWnd,
        //    int nIndex,
        //    IntPtr dwNewLong);

        //[DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        //private static extern Int32 IntSetWindowLong(
        //    IntPtr hWnd,
        //    int nIndex,
        //    Int32 dwNewLong);

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(
            int dwErrorCode);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong(
            IntPtr hWnd,
            int nIndex);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLong(
            IntPtr hWnd,
            int nIndex,
            int dwNewLong);

        //[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        //public static extern IntPtr SetWindowsHookEx(
        //    int idHook,
        //    LowLevelKeyboardProc lpfn,
        //    IntPtr hMod,
        //    uint dwThreadId);

        //[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        //public static extern bool UnhookWindowsHookEx(
        //    IntPtr hhk);

        //[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        //public static extern IntPtr GetModuleHandle(
        //    string lpModuleName);
    }
}