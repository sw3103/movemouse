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

namespace Ellanet
{
    public partial class MouseForm : Form
    {
        const int traceSeconds = 5;
        const string moveMouseXmlName = "Move Mouse.xml";

        Thread moveMouseThread;
        TimeSpan waitUntilAutoMoveDetect = new TimeSpan(0, 0, 5);
        DateTime startTime;
        Point startingMousePoint = new Point();
        System.Windows.Forms.Timer resumeTimer = new System.Windows.Forms.Timer();
        DateTime traceTimeComplete = DateTime.MinValue;
        Thread traceMouseThread;
        string moveMouseXmlDirectory = Environment.ExpandEnvironmentVariables(@"%APPDATA%\Ellanet\Move Mouse");

        delegate void UpdateCountdownProgressBarDelegate(ref ProgressBar pb, int delay, int elapsed);
        delegate void ButtonPerformClickDelegate(ref Button b);
        delegate Point GetControlScreenLocationDelegate(ref Control c);
        delegate object ComboBoxGetSelectedItemDelegate(ref ComboBox cb);
        delegate int ComboBoxGetSelectedIndexDelegate(ref ComboBox cb);
        delegate void SetNumericUpDownValueDelegate(ref NumericUpDown nud, int value);
        delegate void SetButtonTextDelegate(ref Button b, string text);
        delegate void SetButtonTagDelegate(ref Button b, object o);
        delegate object GetButtonTagDelegate(ref Button b);
        delegate string GetButtonTextDelegate(ref Button b);

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

        public MouseForm()
        {
            InitializeComponent();
            int screenSaverTimeout = GetScreenSaverTimeout();

            if (screenSaverTimeout > 0)
            {
                resumeNumericUpDown.Value = (screenSaverTimeout / 2);
            }

            keystrokeCheckBox.CheckedChanged += new EventHandler(keystrokeCheckBox_CheckedChanged);
            staticPositionCheckBox.CheckedChanged += new EventHandler(startPositionCheckBox_CheckedChanged);
            resumeCheckBox.CheckedChanged += new EventHandler(resumeCheckBox_CheckedChanged);
            launchAtLogonCheckBox.CheckedChanged += new EventHandler(launchAtLogonCheckBox_CheckedChanged);
            ReadSettings();
            this.Icon = global::Ellanet.Properties.Resources.Mouse_Icon;
            this.Text = String.Format("Move Mouse ({0}.{1}.{2}) - http://movemouse.codeplex.com/", Assembly.GetExecutingAssembly().GetName().Version.Major, Assembly.GetExecutingAssembly().GetName().Version.Minor, Assembly.GetExecutingAssembly().GetName().Version.Build);
            this.FormClosing += new FormClosingEventHandler(MouseForm_FormClosing);
            this.Load += new EventHandler(MouseForm_Load);
            actionButton.Click += new EventHandler(actionButton_Click);
            moveMouseCheckBox.CheckedChanged += new EventHandler(moveMouseCheckBox_CheckedChanged);
            clickMouseCheckBox.CheckedChanged += new EventHandler(clickMouseCheckBox_CheckedChanged);
            autoPauseCheckBox.CheckedChanged += new EventHandler(autoPauseCheckBox_CheckedChanged);
            resumeTimer.Interval = 1000;
            resumeTimer.Tick += new EventHandler(resumeTimer_Tick);
            traceButton.Click += new EventHandler(traceButton_Click);
            SetButtonTag(ref traceButton, GetButtonText(ref traceButton));
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
            if (startOnLaunchCheckBox.Checked)
            {
                actionButton.PerformClick();
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
            if ((!moveMouseCheckBox.Checked) && (!clickMouseCheckBox.Checked) && (!keystrokeCheckBox.Checked))
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

            if (GetLastInputTime() > resumeNumericUpDown.Value)
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
            if ((!moveMouseCheckBox.Checked) && (!clickMouseCheckBox.Checked) && (!keystrokeCheckBox.Checked))
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
                    this.TopMost = false;

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
                    this.TopMost = true;
                    startTime = DateTime.Now;

                    if (minimiseOnStartCheckBox.Checked)
                    {
                        this.WindowState = FormWindowState.Minimized;
                    }
                    else
                    {
                        this.WindowState = FormWindowState.Normal;
                    }
                    //}

                    break;
            }
        }

        void MoveMouseThread()
        {
            int secondsElapsed = 0;

            if (staticPositionCheckBox.Checked)
            {
                Cursor.Position = new Point(Convert.ToInt32(xNumericUpDown.Value), Convert.ToInt32(yNumericUpDown.Value));
            }

            do
            {
                UpdateCountdownProgressBar(ref countdownProgressBar, Convert.ToInt32(delayNumericUpDown.Value), secondsElapsed);
                secondsElapsed += 1;
                Thread.Sleep(1000);

                if ((autoPauseCheckBox.Checked) && (startTime.Add(waitUntilAutoMoveDetect) < DateTime.Now) && (startingMousePoint != Cursor.Position))
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

                    if ((keystrokeCheckBox.Checked) && (ComboBoxGetSelectedIndex(ref keystrokeComboBox) > -1))
                    {
                        SendKeys.SendWait(ComboBoxGetSelectedItem(ref keystrokeComboBox).ToString());
                    }

                    secondsElapsed = 0;
                }
            } while (true);
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

        object ComboBoxGetSelectedItem(ref ComboBox cb)
        {
            if (InvokeRequired)
            {
                return (object)Invoke(new ComboBoxGetSelectedItemDelegate(ComboBoxGetSelectedItem), new object[] { cb });
            }
            else
            {
                return cb.SelectedItem;
            }
        }

        int ComboBoxGetSelectedIndex(ref ComboBox cb)
        {
            if (InvokeRequired)
            {
                return (int)Invoke(new ComboBoxGetSelectedIndexDelegate(ComboBoxGetSelectedIndex), new object[] { cb });
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
                settingsXmlDoc.LoadXml("<settings><second_delay /><move_mouse_pointer /><stealth_mode /><enable_static_position /><x_static_position /><y_static_position /><click_left_mouse_button /><send_keystroke /><keystroke /><pause_when_mouse_moved /><automatically_resume /><resume_seconds /><automatically_start_on_launch /><automatically_launch_on_logon /><minimise_on_pause /><minimise_on_start /></settings>");
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
                settingsXmlDoc.Save(Path.Combine(moveMouseXmlDirectory, moveMouseXmlName));
            }
            catch
            {
            }
        }
    }
}
