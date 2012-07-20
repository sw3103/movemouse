using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace Ellanet
{
    public partial class SystemTrayIcon : Form
    {
        private const int balloonTipTimeout = 30000;
        private const string downloadsUrl = "http://movemouse.codeplex.com/releases/";

        private NotifyIcon sysTrayIcon;
        private ContextMenu sysTrayMenu;
        private MouseForm moveMouse;
        private bool directUserToDownloadsOnBalloonClick = false;

        public SystemTrayIcon()
        {
            InitializeComponent();
            sysTrayMenu = new ContextMenu();
            sysTrayMenu.MenuItems.Add("Open", OpenMoveMouse);
            sysTrayMenu.MenuItems.Add("-");
            sysTrayMenu.MenuItems.Add("Close", CloseMoveMouse);
            sysTrayIcon = new NotifyIcon();
            sysTrayIcon.DoubleClick += new EventHandler(sysTrayIcon_DoubleClick);
            sysTrayIcon.Text = "Move Mouse";
            sysTrayIcon.Icon = new Icon(global::Ellanet.Properties.Resources.Mouse_Icon, new Size(16, 16));
            sysTrayIcon.ContextMenu = sysTrayMenu;
            sysTrayIcon.Visible = true;
            sysTrayIcon.BalloonTipClicked += new EventHandler(sysTrayIcon_BalloonTipClicked);
            sysTrayIcon.BalloonTipClosed += new EventHandler(sysTrayIcon_BalloonTipClosed);
        }

        void sysTrayIcon_BalloonTipClosed(object sender, EventArgs e)
        {
            directUserToDownloadsOnBalloonClick = false;
        }

        void sysTrayIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            try
            {
                Process.Start(downloadsUrl);
            }
            catch
            {
            }
        }

        void sysTrayIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowMoveMouse(true);
        }

        protected override void OnLoad(EventArgs e)
        {
            ShowMoveMouse(false);
            this.Visible = false;
            this.ShowInTaskbar = false;
            base.OnLoad(e);
        }

        private void OpenMoveMouse(object sender, EventArgs e)
        {
            ShowMoveMouse(true);
        }

        private void CloseMoveMouse(object sender, EventArgs e)
        {
            if ((moveMouse != null) || (!moveMouse.IsDisposed))
            {
                moveMouse.Close();
            }

            sysTrayIcon.Dispose();
            this.Close();
        }

        private void ShowMoveMouse(bool suppressAutoStart)
        {
            if ((moveMouse == null) || (moveMouse.IsDisposed))
            {
                moveMouse = new MouseForm(suppressAutoStart);
                moveMouse.BlackoutStatusChange += new MouseForm.BlackoutStatusChangeHandler(moveMouse_BlackoutStatusChange);
                moveMouse.NewVersionAvailable += new MouseForm.NewVersionAvailableHandler(moveMouse_NewVersionAvailable);
                moveMouse.Show();
            }
            else
            {
                moveMouse.ShowInTaskbar = true;
                moveMouse.WindowState = FormWindowState.Normal;
                moveMouse.Activate();
                moveMouse.BringToFront();
            }
        }

        void moveMouse_NewVersionAvailable(object sender, NewVersionAvailableEventArgs e)
        {
            try
            {
                directUserToDownloadsOnBalloonClick = true;
                string balloonText = String.Format("Move Mouse {0} was released on {1}.\r\n", e.Version.ToString(), e.Released.ToString("dd-MMM-yyyy"));

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
                sysTrayIcon.ShowBalloonTip(balloonTipTimeout, "New Version Available", balloonText, ToolTipIcon.Info);
            }
            catch
            {
            }
        }

        void moveMouse_BlackoutStatusChange(object sender, BlackoutStatusChangeEventArgs e)
        {
            try
            {
                switch (e.Status)
                {
                    case BlackoutStatusChangeEventArgs.BlackoutStatus.Started:
                        sysTrayIcon.ShowBalloonTip(balloonTipTimeout, "Blackout Schedule Started", String.Format("Move Mouse has now entered into a blackout schedule, and will suspend all operations until {0}.", e.StartTime), ToolTipIcon.Info);
                        break;
                    case BlackoutStatusChangeEventArgs.BlackoutStatus.Ended:
                        sysTrayIcon.ShowBalloonTip(balloonTipTimeout, "Blackout Schedule Ended", String.Format("Move Mouse has now left the blackout schedule, and will resume all operations until {0}.", e.EndTime), ToolTipIcon.Info);
                        break;
                }
            }
            catch
            {
            }
        }
    }
}
