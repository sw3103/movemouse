using System;
using System.Diagnostics;
using System.Drawing;
using System.Management;
using System.Windows.Forms;

namespace Ellanet
{
    public partial class SystemTrayIcon : Form
    {
        private const int BalloonTipTimeout = 30000;
        private const string DownloadsUrl = "http://movemouse.codeplex.com/releases/";
        private readonly NotifyIcon _sysTrayIcon;
        private readonly ContextMenu _sysTrayMenu;
        private MouseForm _moveMouse;
        private bool _directUserToDownloadsOnBalloonClick;

        public SystemTrayIcon()
        {
            InitializeComponent();
            _sysTrayMenu = new ContextMenu();
            _sysTrayMenu.MenuItems.Add("Open", OpenMoveMouse);
            _sysTrayMenu.MenuItems.Add("-");
            _sysTrayMenu.MenuItems.Add("Close", CloseMoveMouse);
            _sysTrayIcon = new NotifyIcon();
            _sysTrayIcon.DoubleClick += sysTrayIcon_DoubleClick;
            _sysTrayIcon.Text = "Move Mouse";
            _sysTrayIcon.Icon = new Icon(Properties.Resources.Mouse_Icon, new Size(16, 16));
            _sysTrayIcon.ContextMenu = _sysTrayMenu;
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
        }

        private void sysTrayIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            try
            {
                if (_directUserToDownloadsOnBalloonClick)
                {
                    Process.Start(DownloadsUrl);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void sysTrayIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowMoveMouse(true);
        }

        protected override void OnLoad(EventArgs e)
        {
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

            _sysTrayIcon.Dispose();
            Close();
        }

        private void ShowMoveMouse(bool suppressAutoStart)
        {
            if ((_moveMouse == null) || (_moveMouse.IsDisposed))
            {
                _moveMouse = new MouseForm(suppressAutoStart);
                _moveMouse.BlackoutStatusChange += moveMouse_BlackoutStatusChange;
                _moveMouse.NewVersionAvailable += moveMouse_NewVersionAvailable;
                _moveMouse.FormClosing += _moveMouse_FormClosing;
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

        private void _moveMouse_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_moveMouse.MinimiseToSystemTrayWarningShown)
            {
                _sysTrayIcon.ShowBalloonTip(BalloonTipTimeout, "Move Mouse", "Closed to System Tray.", ToolTipIcon.Info);
            }
        }

        private void moveMouse_NewVersionAvailable(object sender, NewVersionAvailableEventArgs e)
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

        private void moveMouse_BlackoutStatusChange(object sender, BlackoutStatusChangeEventArgs e)
        {
            try
            {
                switch (e.Status)
                {
                    case BlackoutStatusChangeEventArgs.BlackoutStatus.Started:
                        _sysTrayIcon.ShowBalloonTip(BalloonTipTimeout, "Blackout Schedule Started", String.Format("Move Mouse has now entered into a blackout schedule, and will suspend all operations until {0}.", e.StartTime), ToolTipIcon.Info);
                        break;
                    case BlackoutStatusChangeEventArgs.BlackoutStatus.Ended:
                        _sysTrayIcon.ShowBalloonTip(BalloonTipTimeout, "Blackout Schedule Ended", String.Format("Move Mouse has now left the blackout schedule, and will resume all operations until {0}.", e.EndTime), ToolTipIcon.Info);
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
    }
}
