using Ellanet.Events;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Management;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Ellanet.Forms
{
    public partial class SystemTrayIcon : Form
    {
        private const int BalloonTipTimeout = 30000;
        private const string DownloadsUrl = "http://movemouse.codeplex.com/releases/";

        // ReSharper disable InconsistentNaming

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        // ReSharper restore InconsistentNaming

        private readonly NotifyIcon _sysTrayIcon;
        private MouseForm _moveMouse;
        private bool _directUserToDownloadsOnBalloonClick;
        private bool _directUserToPowerShellExecutionPolicyFormOnBalloonClick;
        private IntPtr _hookId = IntPtr.Zero;
        private Keys _hookKey;
        private LowLevelKeyboardProc _hookProc;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(
            int idHook,
            LowLevelKeyboardProc lpfn,
            IntPtr hMod,
            uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(
            IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(
            IntPtr hhk,
            int nCode,
            IntPtr wParam,
            IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(
            string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public SystemTrayIcon()
        {
            InitializeComponent();
            var sysTrayMenu = new ContextMenu();
            sysTrayMenu.MenuItems.Add("Open", OpenMoveMouse);
            sysTrayMenu.MenuItems.Add("-");
            sysTrayMenu.MenuItems.Add("Close", CloseMoveMouse);
            _sysTrayIcon = new NotifyIcon();
            _sysTrayIcon.DoubleClick += _sysTrayIcon_DoubleClick;
            _sysTrayIcon.Text = "Move Mouse";
            _sysTrayIcon.Icon = new Icon(Properties.Resources.Mouse_Icon, new Size(16, 16));
            _sysTrayIcon.ContextMenu = sysTrayMenu;
            _sysTrayIcon.Visible = true;
            _sysTrayIcon.BalloonTipClicked += sysTrayIcon_BalloonTipClicked;
            _sysTrayIcon.BalloonTipClosed += sysTrayIcon_BalloonTipClosed;

            try
            {
                if (Is64BitWindows8Point1() && (GetCurrentDpi() > 120))
                {
                    var layersRegKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers");

                    if (layersRegKey != null)
                    {
                        layersRegKey.SetValue(Application.ExecutablePath, "HIGHDPIAWARE");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void sysTrayIcon_BalloonTipClosed(object sender, EventArgs e)
        {
            _directUserToDownloadsOnBalloonClick = false;
            _directUserToPowerShellExecutionPolicyFormOnBalloonClick = false;
        }

        private void sysTrayIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            try
            {
                if (_directUserToDownloadsOnBalloonClick)
                {
                    Process.Start(DownloadsUrl);
                }

                if (_directUserToPowerShellExecutionPolicyFormOnBalloonClick)
                {
                    var psForm = new PowerShellExecutionPolicyForm();
                    psForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void _sysTrayIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowMoveMouse(true);
        }

        protected override void OnLoad(EventArgs e)
        {
            _hookProc = HookCallback;
            ShowMoveMouse(false);
            Visible = false;
            ShowInTaskbar = false;
            base.OnLoad(e);
        }

        private void OpenMoveMouse(object sender, EventArgs e)
        {
            ShowMoveMouse(true);
        }

        private void CloseMoveMouse(object sender, EventArgs e)
        {
            if ((_moveMouse != null) && (!_moveMouse.IsDisposed))
            {
                _moveMouse.Close();
            }

            if (_hookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookId);
            }

            _sysTrayIcon.Dispose();
            Close();
        }

        private void ShowMoveMouse(bool suppressAutoStart)
        {
            if ((_moveMouse == null) || (_moveMouse.IsDisposed))
            {
                _moveMouse = new MouseForm(suppressAutoStart);
                _moveMouse.BlackoutStatusChanged += _moveMouse_BlackoutStatusChanged;
                _moveMouse.NewVersionAvailable += _moveMouse_NewVersionAvailable;
                _moveMouse.ScheduleArrived += _moveMouse_ScheduleArrived;
                _moveMouse.FormClosing += _moveMouse_FormClosing;
                _moveMouse.PowerLineStatusChanged += _moveMouse_PowerLineStatusChanged;
                _moveMouse.PowerShellexecutionPolicyWarning += _moveMouse_PowerShellexecutionPolicyWarning;
                _moveMouse.HookKeyStatusChanged += _moveMouse_HookKeyStatusChanged;
                _moveMouse.FormBorderStyle = StaticCode.EnableToolWindowStyle ? FormBorderStyle.FixedToolWindow : FormBorderStyle.FixedSingle;
                _moveMouse.Show();
            }
            else
            {
                _moveMouse.ShowInTaskbar = true;
                _moveMouse.WindowState = FormWindowState.Normal;
                _moveMouse.Activate();
                _moveMouse.BringToFront();
            }
        }

        private void _moveMouse_HookKeyStatusChanged(object sender, HookKeyStatusChangedEventArgs e)
        {
            if (_hookId != IntPtr.Zero)
            {
                Debug.WriteLine(String.Format("Unhooking {0}...", _hookKey));
                UnhookWindowsHookEx(_hookId);
            }

            if (e.Enabled && !e.Key.Equals(Keys.None))
            {
                _hookKey = e.Key;
                Debug.WriteLine(String.Format("Hooking {0}...", _hookKey));
                _hookId = SetHook(_hookProc);
            }
        }

        private void _moveMouse_PowerShellexecutionPolicyWarning(object sender)
        {
            _directUserToPowerShellExecutionPolicyFormOnBalloonClick = true;
            _sysTrayIcon.ShowBalloonTip(BalloonTipTimeout, "PowerShell Execution Policy", String.Format("Move Mouse has detected that your PowerShell execution policy will not allow you to run scripts.\r\n\r\nPlease click here to resolve this."), ToolTipIcon.Warning);
        }

        private void _moveMouse_PowerLineStatusChanged(object sender, PowerLineStatusChangedEventArgs e)
        {
            try
            {
                switch (e.Status)
                {
                    case PowerLineStatusChangedEventArgs.PowerLineStatus.Offline:
                        _sysTrayIcon.ShowBalloonTip(BalloonTipTimeout, "Battery Mode Enabled", String.Format("Move Mouse has detected that you are now running on battery, and will suspend all operations until you reconnect to mains power."), ToolTipIcon.Info);
                        break;
                    case PowerLineStatusChangedEventArgs.PowerLineStatus.Online:
                        _sysTrayIcon.ShowBalloonTip(BalloonTipTimeout, "Battery Mode Disabled", String.Format("Move Mouse will resume all operations now that you are reconnected to mains power."), ToolTipIcon.Info);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void _moveMouse_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_moveMouse.MinimiseToSystemTrayWarningShown)
            {
                _sysTrayIcon.ShowBalloonTip(BalloonTipTimeout, "Move Mouse", "Closed to System Tray.", ToolTipIcon.Info);
            }
        }

        private void _moveMouse_NewVersionAvailable(object sender, NewVersionAvailableEventArgs e)
        {
            try
            {
                _directUserToDownloadsOnBalloonClick = true;
                string balloonText = String.Format("Move Mouse {0} was released on {1}.\r\n", e.Version, e.Released.ToString("dd-MMM-yyyy"));

                if ((e.Features != null) && (e.Features.Length > 0))
                {
                    balloonText += String.Format("\r\nNew Features\r\n");

                    foreach (string feature in e.Features)
                    {
                        balloonText += String.Format("  - {0}\r\n", feature);
                    }
                }

                if ((e.Fixes != null) && (e.Fixes.Length > 0))
                {
                    balloonText += String.Format("\r\nFixes\r\n");

                    foreach (string fix in e.Fixes)
                    {
                        balloonText += String.Format("  - {0}\r\n", fix);
                    }
                }

                balloonText += String.Format("\r\nPlease click here to visit the downloads page.");
                _sysTrayIcon.ShowBalloonTip(BalloonTipTimeout, "New Version Available", balloonText, ToolTipIcon.Info);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void _moveMouse_BlackoutStatusChanged(object sender, BlackoutStatusChangedEventArgs e)
        {
            try
            {
                switch (e.Status)
                {
                    case BlackoutStatusChangedEventArgs.BlackoutStatus.Active:
                        _sysTrayIcon.ShowBalloonTip(BalloonTipTimeout, "Blackout Schedule Started", String.Format("Move Mouse has now entered into a blackout schedule, and will suspend all operations until {0}.", e.EndTime), ToolTipIcon.Info);
                        break;
                    case BlackoutStatusChangedEventArgs.BlackoutStatus.Inactive:
                        _sysTrayIcon.ShowBalloonTip(BalloonTipTimeout, "Blackout Schedule Ended", String.Format("Move Mouse has now left the blackout schedule, and will resume all operations until {0}.", e.StartTime), ToolTipIcon.Info);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void _moveMouse_ScheduleArrived(object sender, ScheduleArrivedEventArgs e)
        {
            try
            {
                switch (e.Action)
                {
                    case ScheduleArrivedEventArgs.ScheduleAction.Start:
                        _sysTrayIcon.ShowBalloonTip(BalloonTipTimeout, "Scheduled Start", String.Format("Move Mouse automatically started ({0}).", e.Time), ToolTipIcon.Info);
                        break;
                    case ScheduleArrivedEventArgs.ScheduleAction.Pause:
                        _sysTrayIcon.ShowBalloonTip(BalloonTipTimeout, "Scheduled Pause", String.Format("Move Mouse automatically paused ({0}).", e.Time), ToolTipIcon.Info);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private int GetCurrentDpi()
        {
            try
            {
                using (Graphics g = CreateGraphics())
                {
                    return (int) g.DpiX;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return 96;
        }

        private bool Is64BitWindows8Point1()
        {
            try
            {
                var mos = new ManagementObjectSearcher(@"\\.\root\CIMv2", "SELECT * FROM Win32_OperatingSystem WHERE Version > '6.3' AND OSArchitecture = '64-bit'");
                var moc = mos.Get();
                return (moc.Count > 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return false;
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if ((nCode >= 0) && (wParam == (IntPtr) WM_KEYDOWN))
                {
                    int vkCode = Marshal.ReadInt32(lParam);

                    if ((_hookKey == (Keys) vkCode) && ((Keys.Control | Keys.Alt) == ModifierKeys))
                    {
                        Debug.WriteLine((Keys) vkCode);
                        ShowMoveMouse(true);
                        _moveMouse.StartStopToggle();
                    }
                }

                return CallNextHookEx(_hookId, nCode, wParam, lParam);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return IntPtr.Zero;
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            try
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return IntPtr.Zero;
        }
    }
}
