using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Ellanet
{
    public partial class SystemTrayIcon : Form
    {
        private NotifyIcon sysTrayIcon;
        private ContextMenu sysTrayMenu;
        private MouseForm moveMouse;

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
    }
}
