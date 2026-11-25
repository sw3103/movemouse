using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace ellabi.Platform
{
    /// <summary>
    /// Emits kernel-level virtual mouse and keyboard events via /dev/uinput.
    /// Works on both Wayland and X11 when /dev/uinput is writable.
    /// </summary>
    internal sealed class UInputDevice : IDisposable
    {
        private const int O_WRONLY = 1;
        private const int O_NONBLOCK = 2048;

        private const uint UI_SET_EVBIT = 0x40045564;
        private const uint UI_SET_KEYBIT = 0x40045565;
        private const uint UI_SET_RELBIT = 0x40045566;
        private const uint UI_DEV_CREATE = 0x5501;
        private const uint UI_DEV_DESTROY = 0x5502;

        private const ushort EV_SYN = 0x00;
        private const ushort EV_KEY = 0x01;
        private const ushort EV_REL = 0x02;

        private const ushort SYN_REPORT = 0x00;

        private const ushort REL_X = 0x00;
        private const ushort REL_Y = 0x01;
        private const ushort REL_HWHEEL = 0x06;
        private const ushort REL_WHEEL = 0x08;

        public const ushort BTN_LEFT = 0x110;
        public const ushort BTN_RIGHT = 0x111;
        public const ushort BTN_MIDDLE = 0x112;

        private int _fd = -1;
        private readonly object _lock = new object();
        private bool _initAttempted;

        [StructLayout(LayoutKind.Sequential)]
        private struct InputEvent
        {
            public long tv_sec;
            public long tv_usec;
            public ushort type;
            public ushort code;
            public int value;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct InputId
        {
            public ushort bustype;
            public ushort vendor;
            public ushort product;
            public ushort version;
        }

        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct UInputUserDev
        {
            public fixed byte name[80];
            public InputId id;
            public uint ff_effects_max;
            public fixed int absmax[64];
            public fixed int absmin[64];
            public fixed int absfuzz[64];
            public fixed int absflat[64];
        }

        [DllImport("libc", SetLastError = true)]
        private static extern int open(string pathname, int flags);

        [DllImport("libc", SetLastError = true)]
        private static extern int close(int fd);

        [DllImport("libc", SetLastError = true)]
        private static extern int ioctl(int fd, nuint request, nint arg);

        [DllImport("libc", SetLastError = true)]
        private static extern int ioctl(int fd, nuint request);

        [DllImport("libc", SetLastError = true)]
        private static extern nint write(int fd, ref InputEvent ev, nuint count);

        [DllImport("libc", SetLastError = true)]
        private static extern nint write(int fd, ref UInputUserDev dev, nuint count);

        public bool EnsureInitialized()
        {
            lock (_lock)
            {
                if (_fd >= 0) return true;
                if (_initAttempted) return false;
                _initAttempted = true;

                try
                {
                    int fd = open("/dev/uinput", O_WRONLY | O_NONBLOCK);
                    if (fd < 0) return false;

                    ioctl(fd, UI_SET_EVBIT, EV_SYN);
                    ioctl(fd, UI_SET_EVBIT, EV_KEY);
                    ioctl(fd, UI_SET_EVBIT, EV_REL);

                    ioctl(fd, UI_SET_RELBIT, REL_X);
                    ioctl(fd, UI_SET_RELBIT, REL_Y);
                    ioctl(fd, UI_SET_RELBIT, REL_WHEEL);
                    ioctl(fd, UI_SET_RELBIT, REL_HWHEEL);

                    ioctl(fd, UI_SET_KEYBIT, BTN_LEFT);
                    ioctl(fd, UI_SET_KEYBIT, BTN_RIGHT);
                    ioctl(fd, UI_SET_KEYBIT, BTN_MIDDLE);

                    for (int code = 1; code < 248; code++)
                    {
                        ioctl(fd, UI_SET_KEYBIT, code);
                    }

                    var dev = new UInputUserDev
                    {
                        id = new InputId
                        {
                            bustype = 0x03, // BUS_USB
                            vendor = 0x1234,
                            product = 0x5678,
                            version = 1
                        }
                    };

                    unsafe
                    {
                        byte[] nameBytes = Encoding.ASCII.GetBytes("Move Mouse Virtual Input");
                        for (int i = 0; i < nameBytes.Length && i < 79; i++)
                        {
                            dev.name[i] = nameBytes[i];
                        }
                    }

                    if (write(fd, ref dev, (nuint)Marshal.SizeOf<UInputUserDev>()) < 0)
                    {
                        close(fd);
                        return false;
                    }

                    if (ioctl(fd, UI_DEV_CREATE) < 0)
                    {
                        close(fd);
                        return false;
                    }

                    _fd = fd;
                    Thread.Sleep(60);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool MoveRelative(int dx, int dy)
        {
            if (!EnsureInitialized()) return false;
            lock (_lock)
            {
                if (dx != 0) Emit(EV_REL, REL_X, dx);
                if (dy != 0) Emit(EV_REL, REL_Y, dy);
                Emit(EV_SYN, SYN_REPORT, 0);
                return true;
            }
        }

        public bool MouseButton(ushort btnCode, bool isDown)
        {
            if (!EnsureInitialized()) return false;
            lock (_lock)
            {
                Emit(EV_KEY, btnCode, isDown ? 1 : 0);
                Emit(EV_SYN, SYN_REPORT, 0);
                return true;
            }
        }

        public bool Scroll(bool horizontal, int steps)
        {
            if (!EnsureInitialized()) return false;
            lock (_lock)
            {
                Emit(EV_REL, horizontal ? REL_HWHEEL : REL_WHEEL, steps);
                Emit(EV_SYN, SYN_REPORT, 0);
                return true;
            }
        }

        public bool SendKey(ushort evdevCode, bool isDown)
        {
            if (!EnsureInitialized()) return false;
            lock (_lock)
            {
                Emit(EV_KEY, evdevCode, isDown ? 1 : 0);
                Emit(EV_SYN, SYN_REPORT, 0);
                return true;
            }
        }

        private void Emit(ushort type, ushort code, int value)
        {
            var ev = new InputEvent
            {
                type = type,
                code = code,
                value = value
            };
            write(_fd, ref ev, (nuint)Marshal.SizeOf<InputEvent>());
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_fd >= 0)
                {
                    try
                    {
                        ioctl(_fd, UI_DEV_DESTROY);
                        close(_fd);
                    }
                    catch
                    {
                    }
                    _fd = -1;
                }
            }
        }
    }
}
