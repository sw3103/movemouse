using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Microsoft.VisualBasic;

namespace Ellanet
{
    public partial class MouseForm : Form
    {
        private const int traceSeconds = 5;
        private const string moveMouseXmlName = "Move Mouse.xml";
        private const string homeAddress = "http://movemouse.codeplex.com/";
        private const string contactAddress = "http://www.codeplex.com/site/users/view/sw3103/";
        private const string helpAddress = "http://movemouse.codeplex.com/documentation/";
        private const string paypalAddress = "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=QZTWHD9CRW5XN";
        private const string versionXmlUrl = "https://movemouse.svn.codeplex.com/svn/Version.xml";

        private Thread moveMouseThread;
        private TimeSpan waitUntilAutoMoveDetect = new TimeSpan(0, 0, 5);
        private DateTime mmStartTime;
        private Point startingMousePoint = new Point();
        private System.Windows.Forms.Timer resumeTimer = new System.Windows.Forms.Timer();
        private DateTime traceTimeComplete = DateTime.MinValue;
        private Thread traceMouseThread;
        private string moveMouseXmlDirectory = Environment.ExpandEnvironmentVariables(@"%APPDATA%\Ellanet\Move Mouse");
        private bool _suppressAutoStart = false;
        private BlackoutStatusChangeEventArgs.BlackoutStatus blackoutStatus = BlackoutStatusChangeEventArgs.BlackoutStatus.Ended;
        private TimeSpan waitBetweenUpdateChecks = new TimeSpan(7, 0, 0, 0);
        private DateTime lastUpdateCheck = DateTime.MinValue;

        private delegate void UpdateCountdownProgressBarDelegate(ref ProgressBar pb, int delay, int elapsed);
        private delegate void ButtonPerformClickDelegate(ref Button b);
        private delegate Point GetControlScreenLocationDelegate(ref Control c);
        private delegate object GetComboBoxSelectedItemDelegate(ref ComboBox cb);
        private delegate int GetComboBoxSelectedIndexDelegate(ref ComboBox cb);
        private delegate void SetNumericUpDownValueDelegate(ref NumericUpDown nud, int value);
        private delegate void SetButtonTextDelegate(ref Button b, string text);
        private delegate void SetButtonTagDelegate(ref Button b, object o);
        private delegate object GetButtonTagDelegate(ref Button b);
        private delegate string GetButtonTextDelegate(ref Button b);
        private delegate bool GetCheckBoxCheckedDelegate(ref CheckBox cb);
        private delegate void AddComboBoxItemDelegate(ref ComboBox cb, string item);

        public delegate void BlackoutStatusChangeHandler(object sender, BlackoutStatusChangeEventArgs e);
        public delegate void NewVersionAvailableHandler(object sender, NewVersionAvailableEventArgs e);

        public event BlackoutStatusChangeHandler BlackoutStatusChange;
        public event NewVersionAvailableHandler NewVersionAvailable;

        [Flags]
        enum MouseEventFlags : int
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010
        }

        [Flags]
        enum Win32Consts
        {
            INPUT_MOUSE = 0,
            INPUT_KEYBOARD = 1,
            INPUT_HARDWARE = 2,
        }

        [Flags]
        enum ShowWindowCommands : int
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
            ShowNA = 8,
            Restore = 9,
            ShowDefault = 10,
            ForceMinimize = 11
        }

        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            public static readonly int SizeOf = Marshal.SizeOf(typeof(LASTINPUTINFO));
            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public int dwTime;
        }

        struct INPUT
        {
            public int type;
            public MOUSEINPUT mi;
        }

        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public int time;
            public int dwExtraInfo;
        }

        [DllImport("user32.dll")]
        static extern void mouse_event(
            uint dwFlags,
            uint dx,
            uint dy,
            uint dwData,
            int dwExtraInfo);

        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(
            ref LASTINPUTINFO plii);

        [DllImport("user32.dll")]
        static extern bool SystemParametersInfo(
            int uAction,
            int uParam,
            ref int lpvParam,
            int flags);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(
            uint nInputs,
            ref INPUT pInputs,
            int cbSize);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(
            string lpClassName,
            string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(
            IntPtr hWnd,
            ShowWindowCommands nCmdShow);

        public MouseForm(bool suppressAutoStart)
        {
            InitializeComponent();
            _suppressAutoStart = suppressAutoStart;
            int screenSaverTimeout = GetScreenSaverTimeout();

            if (screenSaverTimeout > 0)
            {
                resumeNumericUpDown.Value = (screenSaverTimeout / 2);
            }

            keystrokeCheckBox.CheckedChanged += new EventHandler(keystrokeCheckBox_CheckedChanged);
            appActivateCheckBox.CheckedChanged += new EventHandler(appActivateCheckBox_CheckedChanged);
            //minimiseOnPauseCheckBox.CheckedChanged += new EventHandler(minimiseOnPauseCheckBox_CheckedChanged);
            //minimiseOnStartCheckBox.CheckedChanged += new EventHandler(minimiseOnStartCheckBox_CheckedChanged);
            staticPositionCheckBox.CheckedChanged += new EventHandler(startPositionCheckBox_CheckedChanged);
            resumeCheckBox.CheckedChanged += new EventHandler(resumeCheckBox_CheckedChanged);
            launchAtLogonCheckBox.CheckedChanged += new EventHandler(launchAtLogonCheckBox_CheckedChanged);
            blackoutCheckBox.CheckedChanged += new EventHandler(blackoutCheckBox_CheckedChanged);
            PopulateBlackoutStartEndComboBoxes();
            ReadSettings();
            this.Icon = global::Ellanet.Properties.Resources.Mouse_Icon;
            this.Text = String.Format("Move Mouse ({0}.{1}.{2}) - {3}", Assembly.GetExecutingAssembly().GetName().Version.Major, Assembly.GetExecutingAssembly().GetName().Version.Minor, Assembly.GetExecutingAssembly().GetName().Version.Build, homeAddress);
            this.FormClosing += new FormClosingEventHandler(MouseForm_FormClosing);
            this.Load += new EventHandler(MouseForm_Load);
            this.Resize += new EventHandler(MouseForm_Resize);
            actionButton.Click += new EventHandler(actionButton_Click);
            moveMouseCheckBox.CheckedChanged += new EventHandler(moveMouseCheckBox_CheckedChanged);
            clickMouseCheckBox.CheckedChanged += new EventHandler(clickMouseCheckBox_CheckedChanged);
            autoPauseCheckBox.CheckedChanged += new EventHandler(autoPauseCheckBox_CheckedChanged);
            resumeTimer.Interval = 1000;
            resumeTimer.Tick += new EventHandler(resumeTimer_Tick);
            traceButton.Click += new EventHandler(traceButton_Click);
            mousePictureBox.MouseEnter += new EventHandler(mousePictureBox_MouseEnter);
            mousePictureBox.MouseLeave += new EventHandler(mousePictureBox_MouseLeave);
            mousePictureBox.MouseClick += new MouseEventHandler(mousePictureBox_MouseClick);
            helpPictureBox.MouseEnter += new EventHandler(helpPictureBox_MouseEnter);
            helpPictureBox.MouseLeave += new EventHandler(helpPictureBox_MouseLeave);
            helpPictureBox.MouseClick += new MouseEventHandler(helpPictureBox_MouseClick);
            contactPictureBox.MouseEnter += new EventHandler(contactPictureBox_MouseEnter);
            contactPictureBox.MouseLeave += new EventHandler(contactPictureBox_MouseLeave);
            contactPictureBox.MouseClick += new MouseEventHandler(contactPictureBox_MouseClick);
            paypalPictureBox.MouseEnter += new EventHandler(paypalPictureBox_MouseEnter);
            paypalPictureBox.MouseLeave += new EventHandler(paypalPictureBox_MouseLeave);
            paypalPictureBox.MouseClick += new MouseEventHandler(paypalPictureBox_MouseClick);
            boStartComboBox.SelectedIndexChanged += new EventHandler(boStartComboBox_SelectedIndexChanged);
            boEndComboBox.SelectedIndexChanged += new EventHandler(boEndComboBox_SelectedIndexChanged);
            SetButtonTag(ref traceButton, GetButtonText(ref traceButton));
        }

        void paypalPictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                Process.Start(paypalAddress);
            }
            catch
            {
            }
        }

        void paypalPictureBox_MouseLeave(object sender, EventArgs e)
        {
            if (this.Cursor != Cursors.WaitCursor)
            {
                this.Cursor = Cursors.Default;
            }
        }

        void paypalPictureBox_MouseEnter(object sender, EventArgs e)
        {
            if (this.Cursor != Cursors.WaitCursor)
            {
                this.Cursor = Cursors.Hand;
            }
        }

        void contactPictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                Process.Start(contactAddress);
            }
            catch
            {
            }
        }

        void contactPictureBox_MouseLeave(object sender, EventArgs e)
        {
            if (this.Cursor != Cursors.WaitCursor)
            {
                this.Cursor = Cursors.Default;
            }
        }

        void contactPictureBox_MouseEnter(object sender, EventArgs e)
        {
            if (this.Cursor != Cursors.WaitCursor)
            {
                this.Cursor = Cursors.Hand;
            }
        }

        protected void OnBlackoutStatusChange(object sender, BlackoutStatusChangeEventArgs e)
        {
            if (BlackoutStatusChange != null)
            {
                BlackoutStatusChange(this, e);
            }
        }

        protected void OnNewVersionAvailable(object sender, NewVersionAvailableEventArgs e)
        {
            if (NewVersionAvailable != null)
            {
                NewVersionAvailable(this, e);
            }
        }

        void boEndComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (boEndComboBox.SelectedIndex < boStartComboBox.SelectedIndex)
            {
                boStartComboBox.SelectedIndex = boEndComboBox.SelectedIndex;
            }
        }

        void boStartComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (boEndComboBox.SelectedIndex < boStartComboBox.SelectedIndex)
            {
                boEndComboBox.SelectedIndex = boStartComboBox.SelectedIndex;
            }
        }

        void PopulateBlackoutStartEndComboBoxes()
        {
            int minute = 0;
            int hour = 0;
            bool breakLoop = false;

            while (!breakLoop)
            {
                string time = String.Format("{0}:{1}", hour.ToString().PadLeft(2, Convert.ToChar("0")), minute.ToString().PadLeft(2, Convert.ToChar("0")));
                boStartComboBox.Items.Add(time);
                boEndComboBox.Items.Add(time);
                minute += 5;

                if (minute >= 60)
                {
                    minute = 0;
                    hour++;
                }

                if (hour >= 24)
                {
                    breakLoop = true;
                }
            }

            boStartComboBox.Items.RemoveAt(boStartComboBox.Items.Count - 1);
            boEndComboBox.Items.RemoveAt(0);
        }

        void blackoutCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            insideOutsideComboBox.Enabled = blackoutCheckBox.Checked;
            boStartComboBox.Enabled = blackoutCheckBox.Checked;
            boEndComboBox.Enabled = blackoutCheckBox.Checked;

            if (blackoutCheckBox.Checked && insideOutsideComboBox.SelectedIndex.Equals(-1))
            {
                insideOutsideComboBox.SelectedIndex = 0;
                boStartComboBox.Text = "09:00";
                boEndComboBox.Text = "17:00";
            }
        }

        void helpPictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                Process.Start(helpAddress);
            }
            catch
            {
            }
        }

        void helpPictureBox_MouseLeave(object sender, EventArgs e)
        {
            if (this.Cursor != Cursors.WaitCursor)
            {
                this.Cursor = Cursors.Default;
            }
        }

        void helpPictureBox_MouseEnter(object sender, EventArgs e)
        {
            if (this.Cursor != Cursors.WaitCursor)
            {
                this.Cursor = Cursors.Hand;
            }
        }

        void mousePictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                Process.Start(homeAddress);
            }
            catch
            {
            }
        }

        void mousePictureBox_MouseLeave(object sender, EventArgs e)
        {
            if (this.Cursor != Cursors.WaitCursor)
            {
                this.Cursor = Cursors.Default;
            }
        }

        void mousePictureBox_MouseEnter(object sender, EventArgs e)
        {
            if (this.Cursor != Cursors.WaitCursor)
            {
                this.Cursor = Cursors.Hand;
            }
        }

        void appActivateCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            processComboBox.Enabled = appActivateCheckBox.Checked;
        }

        void ListRunningProcesses(object stareInfo)
        {
            try
            {
                foreach (Process p in Process.GetProcesses())
                {
                    if (!String.IsNullOrEmpty(p.MainWindowTitle) && !processComboBox.Items.Contains(p.MainWindowTitle))
                    {
                        AddComboBoxItem(ref processComboBox, p.MainWindowTitle);
                    }
                }
            }
            catch
            {
            }
        }

        //void minimiseOnStartCheckBox_CheckedChanged(object sender, EventArgs e)
        //{
        //    minimiseToSystemTrayCheckBox.Enabled = (minimiseOnStartCheckBox.Checked | minimiseOnPauseCheckBox.Checked);
        //}

        //void minimiseOnPauseCheckBox_CheckedChanged(object sender, EventArgs e)
        //{
        //    minimiseToSystemTrayCheckBox.Enabled = (minimiseOnStartCheckBox.Checked | minimiseOnPauseCheckBox.Checked);
        //}

        void MouseForm_Resize(object sender, EventArgs e)
        {
            if ((this.WindowState == FormWindowState.Minimized) && minimiseToSystemTrayCheckBox.Checked)
            {
                this.ShowInTaskbar = false;
            }
        }

        void launchAtLogonCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (launchAtLogonCheckBox.Checked)
            {
                Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true).SetValue("Move Mouse", Application.ExecutablePath);
            }
            else
            {
                Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true).DeleteValue("Move Mouse");
            }
        }

        void MouseForm_Load(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(CheckForUpdate);
            ThreadPool.QueueUserWorkItem(ListRunningProcesses);

            if (startOnLaunchCheckBox.Checked && !_suppressAutoStart)
            {
                actionButton.PerformClick();
            }
        }

        void CheckForUpdate(object stateInfo)
        {
            try
            {
                if (lastUpdateCheck.Add(waitBetweenUpdateChecks) < DateTime.Now)
                {
                    lastUpdateCheck = DateTime.Now;
                    XmlDocument versionXmlDoc = new XmlDocument();
                    versionXmlDoc.Load(versionXmlUrl);
                    Version availableVersion = new Version(Convert.ToInt32(versionXmlDoc.SelectSingleNode("version/major").InnerText), Convert.ToInt32(versionXmlDoc.SelectSingleNode("version/minor").InnerText), Convert.ToInt32(versionXmlDoc.SelectSingleNode("version/build").InnerText));

                    if (availableVersion > Assembly.GetExecutingAssembly().GetName().Version)
                    {
                        DateTime released = Convert.ToDateTime(versionXmlDoc.SelectSingleNode("version/released_date").InnerText);
                        DateTime advertised = Convert.ToDateTime(versionXmlDoc.SelectSingleNode("version/advertised_date").InnerText);
                        List<string> features = new List<string>();
                        List<string> fixes = new List<string>();

                        if (versionXmlDoc.SelectSingleNode("version/features").ChildNodes.Count > 0)
                        {
                            foreach (XmlNode featureNode in versionXmlDoc.SelectSingleNode("version/features").ChildNodes)
                            {
                                features.Add(featureNode.InnerText);
                            }
                        }

                        if (versionXmlDoc.SelectSingleNode("version/fixes").ChildNodes.Count > 0)
                        {
                            foreach (XmlNode fixNode in versionXmlDoc.SelectSingleNode("version/fixes").ChildNodes)
                            {
                                fixes.Add(fixNode.InnerText);
                            }
                        }

                        if (advertised < DateTime.Now)
                        {
                            OnNewVersionAvailable(this, new NewVersionAvailableEventArgs(availableVersion, released, advertised, features.ToArray(), fixes.ToArray()));
                        }
                    }
                }
            }
            catch
            {
            }
        }

        void traceButton_Click(object sender, EventArgs e)
        {
            traceTimeComplete = DateTime.Now.AddSeconds(traceSeconds);

            if ((traceMouseThread == null) || (traceMouseThread.ThreadState != System.Threading.ThreadState.Running))
            {
                traceMouseThread = new Thread(new ThreadStart(TraceMouse));
                traceMouseThread.Start();
            }
        }

        void TraceMouse()
        {
            do
            {
                SetNumericUpDownValue(ref xNumericUpDown, Cursor.Position.X);
                SetNumericUpDownValue(ref yNumericUpDown, Cursor.Position.Y);
                SetButtonText(ref traceButton, String.Format("{0} ({1})", Convert.ToString(GetButtonTag(ref traceButton)), traceTimeComplete.Subtract(DateTime.Now).TotalSeconds.ToString("0.0")));
                Thread.Sleep(100);
            }
            while (traceTimeComplete > DateTime.Now);

            SetButtonText(ref traceButton, Convert.ToString(GetButtonTag(ref traceButton)));
        }

        void startPositionCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            xNumericUpDown.Enabled = staticPositionCheckBox.Checked;
            yNumericUpDown.Enabled = staticPositionCheckBox.Checked;
            traceButton.Enabled = staticPositionCheckBox.Checked;
        }

        void keystrokeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!moveMouseCheckBox.Checked && !clickMouseCheckBox.Checked && !keystrokeCheckBox.Checked)
            {
                keystrokeCheckBox.Checked = true;
            }

            keystrokeComboBox.Enabled = keystrokeCheckBox.Checked;
        }

        void resumeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            resumeNumericUpDown.Enabled = resumeCheckBox.Checked;
        }

        void resumeTimer_Tick(object sender, EventArgs e)
        {
            //Debug.WriteLine(GetLastInputTime().ToString());

            if (GetCheckBoxChecked(ref resumeCheckBox) && (GetLastInputTime() > resumeNumericUpDown.Value))
            {
                ButtonPerformClick(ref actionButton);
            }
        }

        void autoPauseCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            startingMousePoint = Cursor.Position;
        }

        void clickMouseCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!moveMouseCheckBox.Checked && !clickMouseCheckBox.Checked && !keystrokeCheckBox.Checked)
            {
                clickMouseCheckBox.Checked = true;
            }

            staticPositionCheckBox.Enabled = (clickMouseCheckBox.Checked | moveMouseCheckBox.Checked);
            xNumericUpDown.Enabled = (staticPositionCheckBox.Enabled & staticPositionCheckBox.Checked);
            yNumericUpDown.Enabled = (staticPositionCheckBox.Enabled & staticPositionCheckBox.Checked);
            traceButton.Enabled = (staticPositionCheckBox.Enabled & staticPositionCheckBox.Checked);
        }

        void moveMouseCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if ((!moveMouseCheckBox.Checked) && (!clickMouseCheckBox.Checked) && (!keystrokeCheckBox.Checked))
            {
                moveMouseCheckBox.Checked = true;
            }

            stealthCheckBox.Enabled = moveMouseCheckBox.Checked;
            staticPositionCheckBox.Enabled = (clickMouseCheckBox.Checked | moveMouseCheckBox.Checked);
            xNumericUpDown.Enabled = (staticPositionCheckBox.Enabled & staticPositionCheckBox.Checked);
            yNumericUpDown.Enabled = (staticPositionCheckBox.Enabled & staticPositionCheckBox.Checked);
            traceButton.Enabled = (staticPositionCheckBox.Enabled & staticPositionCheckBox.Checked);
        }

        void MouseForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (moveMouseThread != null)
            {
                moveMouseThread.Abort();
            }

            resumeTimer.Stop();
            SaveSettings();
        }

        void actionButton_Click(object sender, EventArgs e)
        {
            switch (actionButton.Text)
            {
                case "Pause":

                    if (moveMouseThread != null)
                    {
                        moveMouseThread.Abort();
                    }

                    resumeTimer.Start();
                    actionButton.Text = "Start";
                    this.Opacity = 1.0;
                    //this.TopMost = false;

                    if (minimiseOnPauseCheckBox.Checked)
                    {
                        this.WindowState = FormWindowState.Minimized;
                    }

                    break;
                default:

                    //if ((!moveMouseCheckBox.Checked) && (!clickMouseCheckBox.Checked))
                    //{
                    //    MessageBox.Show("Please select at least on mouse action (move or click).", "No Mouse Actions Enabled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //}
                    //else
                    //{
                    resumeTimer.Stop();
                    moveMouseThread = new Thread(new ThreadStart(MoveMouseThread));
                    moveMouseThread.Start();
                    actionButton.Text = "Pause";
                    this.Opacity = .75;
                    //this.TopMost = true;
                    mmStartTime = DateTime.Now;

                    if (minimiseOnStartCheckBox.Checked)
                    {
                        this.WindowState = FormWindowState.Minimized;
                    }
                    else
                    {
                        this.WindowState = FormWindowState.Normal;
                    }
                    //}

                    SaveSettings();
                    break;
            }
        }

        void MoveMouseThread()
        {
            int secondsElapsed = 0;
            blackoutStatus = BlackoutStatusChangeEventArgs.BlackoutStatus.Ended;

            if (staticPositionCheckBox.Checked)
            {
                Cursor.Position = new Point(Convert.ToInt32(xNumericUpDown.Value), Convert.ToInt32(yNumericUpDown.Value));
            }

            if (appActivateCheckBox.Checked && (GetComboBoxSelectedIndex(ref processComboBox) > -1))
            {
                try
                {
                    IntPtr handle = FindWindow(null, GetComboBoxSelectedItem(ref processComboBox).ToString());
                    Process p = GetProessByMainWindowTitle(GetComboBoxSelectedItem(ref processComboBox).ToString());

                    if (handle != IntPtr.Zero)
                    {
                        if (p == null)
                        {
                            ShowWindow(handle, ShowWindowCommands.Restore);
                        }

                        Interaction.AppActivate(GetComboBoxSelectedItem(ref processComboBox).ToString());
                    }
                }
                catch
                {
                }
            }

            do
            {
                Thread.Sleep(1000);

                if (!BlackoutInEffect())
                {
                    UpdateCountdownProgressBar(ref countdownProgressBar, Convert.ToInt32(delayNumericUpDown.Value), secondsElapsed);
                    secondsElapsed += 1;

                    if (blackoutStatus == BlackoutStatusChangeEventArgs.BlackoutStatus.Started)
                    {
                        blackoutStatus = BlackoutStatusChangeEventArgs.BlackoutStatus.Ended;
                        string startTime = null;
                        string endTime = null;
                        GetNextBlackoutStatusChangeTime(out startTime, out endTime);
                        OnBlackoutStatusChange(this, new BlackoutStatusChangeEventArgs(blackoutStatus, startTime, endTime));
                    }

                    if (autoPauseCheckBox.Checked && (mmStartTime.Add(waitUntilAutoMoveDetect) < DateTime.Now) && (startingMousePoint != Cursor.Position))
                    {
                        ButtonPerformClick(ref actionButton);
                    }
                    else
                    {
                        startingMousePoint = Cursor.Position;
                    }

                    if (secondsElapsed > Convert.ToInt32(delayNumericUpDown.Value))
                    {
                        if (clickMouseCheckBox.Checked)
                        {
                            mouse_event((int)MouseEventFlags.LEFTDOWN, 0, 0, 0, 0);
                            mouse_event((int)MouseEventFlags.LEFTUP, 0, 0, 0, 0);
                        }

                        if (moveMouseCheckBox.Checked)
                        {
                            if (!stealthCheckBox.Checked)
                            {
                                const int mouseMoveLoopSleep = 1;
                                const int mouseSpeed = 1;
                                const int moveSquareSize = 10;
                                Point cursorStartPosition = Cursor.Position;

                                for (int i = 0; i < moveSquareSize; i += mouseSpeed)
                                {
                                    MoveMouse(new Point(1, 0));
                                    Thread.Sleep(mouseMoveLoopSleep);
                                }

                                for (int i = 0; i < moveSquareSize; i += mouseSpeed)
                                {
                                    MoveMouse(new Point(0, 1));
                                    Thread.Sleep(mouseMoveLoopSleep);
                                }

                                for (int i = 0; i < moveSquareSize; i += mouseSpeed)
                                {
                                    MoveMouse(new Point(-1, 0));
                                    Thread.Sleep(mouseMoveLoopSleep);
                                }

                                for (int i = 0; i < moveSquareSize; i += mouseSpeed)
                                {
                                    MoveMouse(new Point(0, -1));
                                    Thread.Sleep(mouseMoveLoopSleep);
                                }

                                Cursor.Position = cursorStartPosition;
                            }
                            else
                            {
                                MoveMouse(new Point(0, 0));
                            }
                        }

                        if (keystrokeCheckBox.Checked && (GetComboBoxSelectedIndex(ref keystrokeComboBox) > -1))
                        {
                            SendKeys.SendWait(GetComboBoxSelectedItem(ref keystrokeComboBox).ToString());
                        }

                        secondsElapsed = 0;
                    }
                }
                else
                {
                    if (blackoutStatus == BlackoutStatusChangeEventArgs.BlackoutStatus.Ended)
                    {
                        blackoutStatus = BlackoutStatusChangeEventArgs.BlackoutStatus.Started;
                        string startTime = null;
                        string endTime = null;
                        GetNextBlackoutStatusChangeTime(out startTime, out endTime);
                        OnBlackoutStatusChange(this, new BlackoutStatusChangeEventArgs(blackoutStatus, startTime, endTime));
                    }
                }
            } while (true);
        }

        bool BlackoutInEffect()
        {
            try
            {
                if (GetCheckBoxChecked(ref blackoutCheckBox))
                {
                    switch (GetComboBoxSelectedItem(ref insideOutsideComboBox).ToString())
                    {
                        case "inside":

                            if ((DateTime.Now > Convert.ToDateTime(String.Format("{0} {1}", DateTime.Now.ToString("yyyy-MMM-dd"), GetComboBoxSelectedItem(ref boStartComboBox).ToString()))) && (DateTime.Now < Convert.ToDateTime(String.Format("{0} {1}", DateTime.Now.ToString("yyyy-MMM-dd"), GetComboBoxSelectedItem(ref boEndComboBox).ToString()))))
                            {
                                return true;
                            }

                            break;
                        case "outside":

                            if ((DateTime.Now < Convert.ToDateTime(String.Format("{0} {1}", DateTime.Now.ToString("yyyy-MMM-dd"), GetComboBoxSelectedItem(ref boStartComboBox).ToString()))) || (DateTime.Now > Convert.ToDateTime(String.Format("{0} {1}", DateTime.Now.ToString("yyyy-MMM-dd"), GetComboBoxSelectedItem(ref boEndComboBox).ToString()))))
                            {
                                return true;
                            }

                            break;
                    }
                }
            }
            catch
            {
            }

            return false;
        }

        void GetNextBlackoutStatusChangeTime(out string startTime, out string endTime)
        {
            startTime = "??:??";
            endTime = "??:??";

            try
            {
                switch (GetComboBoxSelectedItem(ref insideOutsideComboBox).ToString())
                {
                    case "inside":
                        startTime = GetComboBoxSelectedItem(ref boEndComboBox).ToString();
                        endTime = GetComboBoxSelectedItem(ref boStartComboBox).ToString();
                        break;
                    case "outside":
                        startTime = GetComboBoxSelectedItem(ref boStartComboBox).ToString();
                        endTime = GetComboBoxSelectedItem(ref boEndComboBox).ToString();
                        break;
                }
            }
            catch
            {
            }
        }

        void UpdateCountdownProgressBar(ref ProgressBar pb, int delay, int elapsed)
        {
            if (InvokeRequired)
            {
                Invoke(new UpdateCountdownProgressBarDelegate(UpdateCountdownProgressBar), new object[] { pb, delay, elapsed });
            }
            else
            {
                pb.Minimum = 0;
                pb.Maximum = delay;

                if (elapsed < delay)
                {
                    pb.Value = delay - elapsed;
                }
                else
                {
                    pb.Value = 0;
                }
            }
        }

        void ButtonPerformClick(ref Button b)
        {
            if (InvokeRequired)
            {
                Invoke(new ButtonPerformClickDelegate(ButtonPerformClick), new object[] { b });
            }
            else
            {
                b.PerformClick();
            }
        }

        Point GetControlScreenLocation(ref Control c)
        {
            if (InvokeRequired)
            {
                return (Point)Invoke(new GetControlScreenLocationDelegate(GetControlScreenLocation), new object[] { c });
            }
            else
            {
                return this.PointToScreen(c.Location);
            }
        }

        object GetComboBoxSelectedItem(ref ComboBox cb)
        {
            if (InvokeRequired)
            {
                return (object)Invoke(new GetComboBoxSelectedItemDelegate(GetComboBoxSelectedItem), new object[] { cb });
            }
            else
            {
                return cb.SelectedItem;
            }
        }

        int GetComboBoxSelectedIndex(ref ComboBox cb)
        {
            if (InvokeRequired)
            {
                return (int)Invoke(new GetComboBoxSelectedIndexDelegate(GetComboBoxSelectedIndex), new object[] { cb });
            }
            else
            {
                return cb.SelectedIndex;
            }
        }

        void SetNumericUpDownValue(ref NumericUpDown nud, int value)
        {
            if (InvokeRequired)
            {
                Invoke(new SetNumericUpDownValueDelegate(SetNumericUpDownValue), new object[] { nud, value });
            }
            else
            {
                nud.Value = value;
            }
        }

        void SetButtonText(ref Button b, string text)
        {
            if (InvokeRequired)
            {
                Invoke(new SetButtonTextDelegate(SetButtonText), new object[] { b, text });
            }
            else
            {
                b.Text = text;
            }
        }

        void SetButtonTag(ref Button b, object o)
        {
            if (InvokeRequired)
            {
                Invoke(new SetButtonTagDelegate(SetButtonTag), new object[] { b, o });
            }
            else
            {
                b.Tag = o;
            }
        }

        object GetButtonTag(ref Button b)
        {
            if (InvokeRequired)
            {
                return Invoke(new GetButtonTagDelegate(GetButtonTag), new object[] { b });
            }
            else
            {
                return b.Tag;
            }
        }

        string GetButtonText(ref Button b)
        {
            if (InvokeRequired)
            {
                return Convert.ToString(Invoke(new GetButtonTextDelegate(GetButtonText), new object[] { b }));
            }
            else
            {
                return b.Text;
            }
        }

        bool GetCheckBoxChecked(ref CheckBox cb)
        {
            if (InvokeRequired)
            {
                return Convert.ToBoolean(Invoke(new GetCheckBoxCheckedDelegate(GetCheckBoxChecked), new object[] { cb }));
            }
            else
            {
                return cb.Checked;
            }
        }

        void AddComboBoxItem(ref ComboBox cb, string item)
        {
            if (InvokeRequired)
            {
                Invoke(new AddComboBoxItemDelegate(AddComboBoxItem), new object[] { cb, item });
            }
            else
            {
                cb.Items.Add(item);
            }
        }

        int GetLastInputTime()
        {
            int idleTime = 0;
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = Marshal.SizeOf(lastInputInfo);
            lastInputInfo.dwTime = 0;
            int envTicks = Environment.TickCount;

            if (GetLastInputInfo(ref lastInputInfo))
            {
                int lastInputTick = lastInputInfo.dwTime;
                idleTime = envTicks - lastInputTick;
            }

            return ((idleTime > 0) ? (idleTime / 1000) : 0);
        }

        int GetScreenSaverTimeout()
        {
            const int SPI_GETSCREENSAVERTIMEOUT = 14;
            int value = 0;
            SystemParametersInfo(SPI_GETSCREENSAVERTIMEOUT, 0, ref value, 0);
            return value;
        }

        void MoveMouse(Point point)
        {
            MOUSEINPUT mi = new MOUSEINPUT();
            mi.dx = point.X;
            mi.dy = point.Y;
            mi.mouseData = 0;
            mi.time = 0;
            mi.dwFlags = Convert.ToInt32(MouseEventFlags.MOVE);
            mi.dwExtraInfo = 0;
            INPUT input = new INPUT();
            input.mi = mi;
            input.type = Convert.ToInt32(Win32Consts.INPUT_MOUSE);
            SendInput(1, ref input, 28);
        }

        void ReadSettings()
        {
            try
            {
                if (File.Exists(Path.Combine(moveMouseXmlDirectory, moveMouseXmlName)))
                {
                    XmlDocument settingsXmlDoc = new XmlDocument();
                    settingsXmlDoc.Load(Path.Combine(moveMouseXmlDirectory, moveMouseXmlName));
                    delayNumericUpDown.Value = Convert.ToDecimal(settingsXmlDoc.SelectSingleNode("settings/second_delay").InnerText);
                    moveMouseCheckBox.Checked = Convert.ToBoolean(settingsXmlDoc.SelectSingleNode("settings/move_mouse_pointer").InnerText);
                    stealthCheckBox.Checked = Convert.ToBoolean(settingsXmlDoc.SelectSingleNode("settings/stealth_mode").InnerText);
                    staticPositionCheckBox.Checked = Convert.ToBoolean(settingsXmlDoc.SelectSingleNode("settings/enable_static_position").InnerText);
                    xNumericUpDown.Value = Convert.ToDecimal(settingsXmlDoc.SelectSingleNode("settings/x_static_position").InnerText);
                    yNumericUpDown.Value = Convert.ToDecimal(settingsXmlDoc.SelectSingleNode("settings/y_static_position").InnerText);
                    clickMouseCheckBox.Checked = Convert.ToBoolean(settingsXmlDoc.SelectSingleNode("settings/click_left_mouse_button").InnerText);
                    keystrokeCheckBox.Checked = Convert.ToBoolean(settingsXmlDoc.SelectSingleNode("settings/send_keystroke").InnerText);
                    keystrokeComboBox.Text = settingsXmlDoc.SelectSingleNode("settings/keystroke").InnerText;
                    autoPauseCheckBox.Checked = Convert.ToBoolean(settingsXmlDoc.SelectSingleNode("settings/pause_when_mouse_moved").InnerText);
                    resumeCheckBox.Checked = Convert.ToBoolean(settingsXmlDoc.SelectSingleNode("settings/automatically_resume").InnerText);
                    resumeNumericUpDown.Value = Convert.ToDecimal(settingsXmlDoc.SelectSingleNode("settings/resume_seconds").InnerText);
                    startOnLaunchCheckBox.Checked = Convert.ToBoolean(settingsXmlDoc.SelectSingleNode("settings/automatically_start_on_launch").InnerText);
                    launchAtLogonCheckBox.Checked = Convert.ToBoolean(settingsXmlDoc.SelectSingleNode("settings/automatically_launch_on_logon").InnerText);
                    minimiseOnPauseCheckBox.Checked = Convert.ToBoolean(settingsXmlDoc.SelectSingleNode("settings/minimise_on_pause").InnerText);
                    minimiseOnStartCheckBox.Checked = Convert.ToBoolean(settingsXmlDoc.SelectSingleNode("settings/minimise_on_start").InnerText);
                    minimiseToSystemTrayCheckBox.Checked = Convert.ToBoolean(settingsXmlDoc.SelectSingleNode("settings/minimise_to_system_tray").InnerText);
                    appActivateCheckBox.Checked = Convert.ToBoolean(settingsXmlDoc.SelectSingleNode("settings/activate_application").InnerText);

                    if (!String.IsNullOrEmpty(settingsXmlDoc.SelectSingleNode("settings/activate_application_title").InnerText))
                    {
                        processComboBox.Items.Add(settingsXmlDoc.SelectSingleNode("settings/activate_application_title").InnerText);
                        processComboBox.Text = settingsXmlDoc.SelectSingleNode("settings/activate_application_title").InnerText;
                    }

                    blackoutCheckBox.Checked = Convert.ToBoolean(settingsXmlDoc.SelectSingleNode("settings/blackout_schedule_enabled").InnerText);
                    insideOutsideComboBox.Text = settingsXmlDoc.SelectSingleNode("settings/blackout_schedule_scope").InnerText;
                    boStartComboBox.Text = settingsXmlDoc.SelectSingleNode("settings/blackout_schedule_start").InnerText;
                    boEndComboBox.Text = settingsXmlDoc.SelectSingleNode("settings/blackout_schedule_end").InnerText;
                    lastUpdateCheck = Convert.ToDateTime(settingsXmlDoc.SelectSingleNode("settings/last_update_check").InnerText);
                }
            }
            catch
            {
            }
        }

        void SaveSettings()
        {
            try
            {
                if (!Directory.Exists(moveMouseXmlDirectory))
                {
                    Directory.CreateDirectory(moveMouseXmlDirectory);
                }

                XmlDocument settingsXmlDoc = new XmlDocument();
                settingsXmlDoc.LoadXml("<settings><second_delay /><move_mouse_pointer /><stealth_mode /><enable_static_position /><x_static_position /><y_static_position /><click_left_mouse_button /><send_keystroke /><keystroke /><pause_when_mouse_moved /><automatically_resume /><resume_seconds /><automatically_start_on_launch /><automatically_launch_on_logon /><minimise_on_pause /><minimise_on_start /><minimise_to_system_tray /><activate_application /><activate_application_title /><blackout_schedule_enabled /><blackout_schedule_scope /><blackout_schedule_start /><blackout_schedule_end /><last_update_check /></settings>");
                settingsXmlDoc.SelectSingleNode("settings/second_delay").InnerText = Convert.ToDecimal(delayNumericUpDown.Value).ToString();
                settingsXmlDoc.SelectSingleNode("settings/move_mouse_pointer").InnerText = moveMouseCheckBox.Checked.ToString();
                settingsXmlDoc.SelectSingleNode("settings/stealth_mode").InnerText = stealthCheckBox.Checked.ToString();
                settingsXmlDoc.SelectSingleNode("settings/enable_static_position").InnerText = staticPositionCheckBox.Checked.ToString();
                settingsXmlDoc.SelectSingleNode("settings/x_static_position").InnerText = Convert.ToDecimal(xNumericUpDown.Value).ToString();
                settingsXmlDoc.SelectSingleNode("settings/y_static_position").InnerText = Convert.ToDecimal(yNumericUpDown.Value).ToString();
                settingsXmlDoc.SelectSingleNode("settings/click_left_mouse_button").InnerText = clickMouseCheckBox.Checked.ToString();
                settingsXmlDoc.SelectSingleNode("settings/send_keystroke").InnerText = keystrokeCheckBox.Checked.ToString();
                settingsXmlDoc.SelectSingleNode("settings/keystroke").InnerText = keystrokeComboBox.Text;
                settingsXmlDoc.SelectSingleNode("settings/pause_when_mouse_moved").InnerText = autoPauseCheckBox.Checked.ToString();
                settingsXmlDoc.SelectSingleNode("settings/automatically_resume").InnerText = resumeCheckBox.Checked.ToString();
                settingsXmlDoc.SelectSingleNode("settings/resume_seconds").InnerText = Convert.ToDecimal(resumeNumericUpDown.Value).ToString();
                settingsXmlDoc.SelectSingleNode("settings/automatically_start_on_launch").InnerText = startOnLaunchCheckBox.Checked.ToString();
                settingsXmlDoc.SelectSingleNode("settings/automatically_launch_on_logon").InnerText = launchAtLogonCheckBox.Checked.ToString();
                settingsXmlDoc.SelectSingleNode("settings/minimise_on_pause").InnerText = minimiseOnPauseCheckBox.Checked.ToString();
                settingsXmlDoc.SelectSingleNode("settings/minimise_on_start").InnerText = minimiseOnStartCheckBox.Checked.ToString();
                settingsXmlDoc.SelectSingleNode("settings/minimise_to_system_tray").InnerText = minimiseToSystemTrayCheckBox.Checked.ToString();
                settingsXmlDoc.SelectSingleNode("settings/activate_application").InnerText = appActivateCheckBox.Checked.ToString();
                settingsXmlDoc.SelectSingleNode("settings/activate_application_title").InnerText = processComboBox.Text;
                settingsXmlDoc.SelectSingleNode("settings/blackout_schedule_enabled").InnerText = blackoutCheckBox.Checked.ToString();
                settingsXmlDoc.SelectSingleNode("settings/blackout_schedule_scope").InnerText = insideOutsideComboBox.Text;
                settingsXmlDoc.SelectSingleNode("settings/blackout_schedule_start").InnerText = boStartComboBox.Text;
                settingsXmlDoc.SelectSingleNode("settings/blackout_schedule_end").InnerText = boEndComboBox.Text;
                settingsXmlDoc.SelectSingleNode("settings/last_update_check").InnerText = lastUpdateCheck.ToString("yyyy-MMM-dd HH:mm:ss");
                settingsXmlDoc.Save(Path.Combine(moveMouseXmlDirectory, moveMouseXmlName));
            }
            catch
            {
            }
        }

        Process GetProessByMainWindowTitle(string title)
        {
            foreach (Process p in Process.GetProcesses())
            {
                if (p.MainWindowTitle.Equals(title, StringComparison.CurrentCultureIgnoreCase))
                {
                    return p;
                }
            }

            return null;
        }
    }
}
