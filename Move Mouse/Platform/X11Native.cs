using System;
using System.Runtime.InteropServices;

namespace ellabi.Platform
{
    internal static class X11Native
    {
        private const string LibX11 = "libX11.so.6";
        private const string LibXtst = "libXtst.so.6";
        private const string LibXss = "libXss.so.1";

        public const int CurrentTime = 0;
        public const int False = 0;
        public const int True = 1;
        public const nuint AnyPropertyType = 0;
        public const nuint XaWindow = 33;
        public const nuint XaCardinal = 6;
        public const nuint XaString = 31;
        public const int ClientMessage = 33;
        public const nint SubstructureRedirectMask = 1 << 20;
        public const nint SubstructureNotifyMask = 1 << 19;

        [StructLayout(LayoutKind.Sequential)]
        public struct XScreenSaverInfo
        {
            public nuint window;
            public int state;
            public int kind;
            public nuint til_or_since;
            public nuint idle;
            public nuint eventMask;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XClientMessageEvent
        {
            public int type;
            public nuint serial;
            public int send_event;
            public nint display;
            public nuint window;
            public nuint message_type;
            public int format;
            public nint data0;
            public nint data1;
            public nint data2;
            public nint data3;
            public nint data4;
        }

        [StructLayout(LayoutKind.Explicit, Size = 192)]
        public struct XEvent
        {
            [FieldOffset(0)] public int type;
            [FieldOffset(0)] public XClientMessageEvent xclient;
        }

        [DllImport(LibX11)]
        public static extern nint XOpenDisplay(string display);

        [DllImport(LibX11)]
        public static extern int XCloseDisplay(nint display);

        [DllImport(LibX11)]
        public static extern int XFlush(nint display);

        [DllImport(LibX11)]
        public static extern nuint XDefaultRootWindow(nint display);

        [DllImport(LibX11)]
        public static extern int XQueryPointer(
            nint display,
            nuint w,
            out nuint root_return,
            out nuint child_return,
            out int root_x_return,
            out int root_y_return,
            out int win_x_return,
            out int win_y_return,
            out uint mask_return);

        [DllImport(LibX11)]
        public static extern int XWarpPointer(
            nint display,
            nuint src_w,
            nuint dest_w,
            int src_x,
            int src_y,
            uint src_width,
            uint src_height,
            int dest_x,
            int dest_y);

        [DllImport(LibX11)]
        public static extern int XResetScreenSaver(nint display);

        [DllImport(LibX11)]
        public static extern byte XKeysymToKeycode(nint display, nuint keysym);

        [DllImport(LibX11)]
        public static extern nuint XInternAtom(nint display, string atom_name, int only_if_exists);

        [DllImport(LibX11)]
        public static extern int XGetWindowProperty(
            nint display,
            nuint w,
            nuint property,
            nint long_offset,
            nint long_length,
            int delete,
            nuint req_type,
            out nuint actual_type_return,
            out int actual_format_return,
            out nuint nitems_return,
            out nuint bytes_after_return,
            out nint prop_return);

        [DllImport(LibX11)]
        public static extern int XFree(nint data);

        [DllImport(LibX11)]
        public static extern int XFetchName(nint display, nuint w, out nint window_name_return);

        [DllImport(LibX11)]
        public static extern int XSendEvent(nint display, nuint w, int propagate, nint event_mask, ref XEvent event_send);

        [DllImport(LibX11)]
        public static extern int XMapRaised(nint display, nuint w);

        [DllImport(LibXtst)]
        public static extern int XTestFakeMotionEvent(nint display, int screen_number, int x, int y, nuint delay);

        [DllImport(LibXtst)]
        public static extern int XTestFakeRelativeMotionEvent(nint display, int x, int y, nuint delay);

        [DllImport(LibXtst)]
        public static extern int XTestFakeButtonEvent(nint display, uint button, int is_press, nuint delay);

        [DllImport(LibXtst)]
        public static extern int XTestFakeKeyEvent(nint display, uint keycode, int is_press, nuint delay);

        [DllImport(LibXss)]
        public static extern nint XScreenSaverAllocInfo();

        [DllImport(LibXss)]
        public static extern int XScreenSaverQueryInfo(nint display, nuint drawable, nint saver_info);
    }
}
