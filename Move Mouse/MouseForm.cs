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

namespace Ellanet
{
    public partial class MouseForm : Form
    {
        Thread moveMouseThread;
        TimeSpan waitUntilAutoMoveDetect = new TimeSpan(0, 0, 5);
        DateTime startTime;
        Point startingMousePoint = new Point();
        System.Windows.Forms.Timer resumeTimer = new System.Windows.Forms.Timer();

        delegate void UpdateCountdownProgressBarDelegate(ref ProgressBar pb, int delay, int elapsed);
        delegate void ButtonPerformClickDelegate(ref Button b);
        delegate Point GetControlScreenLocationDelegate(ref Control c);
        delegate object ComboBoxGetSelectedItemDelegate(ref ComboBox cb);
        delegate int ComboBoxGetSelectedIndexDelegate(ref ComboBox cb);

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
            this.Icon = global::Ellanet.Properties.Resources.Mouse_Icon;
            this.Text = String.Format("Move Mouse ({0}.{1}.{2}) - http://movemouse.codeplex.com/", Assembly.GetExecutingAssembly().GetName().Version.Major, Assembly.GetExecutingAssembly().GetName().Version.Minor, Assembly.GetExecutingAssembly().GetName().Version.Build);
            actionButton.Click += new EventHandler(actionButton_Click);
            this.FormClosing += new FormClosingEventHandler(MouseForm_FormClosing);
            moveMouseCheckBox.CheckedChanged += new EventHandler(moveMouseCheckBox_CheckedChanged);
            clickMouseCheckBox.CheckedChanged += new EventHandler(clickMouseCheckBox_CheckedChanged);
            autoPauseCheckBox.CheckedChanged += new EventHandler(autoPauseCheckBox_CheckedChanged);
            resumeTimer.Interval = 1000;
            resumeTimer.Tick += new EventHandler(resumeTimer_Tick);
            resumeCheckBox.CheckedChanged += new EventHandler(resumeCheckBox_CheckedChanged);
            keystrokeCheckBox.CheckedChanged += new EventHandler(keystrokeCheckBox_CheckedChanged);
            int screenSaverTimeout = GetScreenSaverTimeout();

            if (screenSaverTimeout > 0)
            {
                resumeNumericUpDown.Value = (screenSaverTimeout / 2);
            }
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
            if (resumeCheckBox.Checked)
            {
                resumeNumericUpDown.Enabled = true;
            }
            else
            {
                resumeNumericUpDown.Enabled = false;
            }
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
        }

        void moveMouseCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if ((!moveMouseCheckBox.Checked) && (!clickMouseCheckBox.Checked) && (!keystrokeCheckBox.Checked))
            {
                moveMouseCheckBox.Checked = true;
            }

            stealthCheckBox.Enabled = moveMouseCheckBox.Checked;
        }

        void MouseForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (moveMouseThread != null)
            {
                moveMouseThread.Abort();
            }

            resumeTimer.Stop();
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
                    this.WindowState = FormWindowState.Minimized;
                    this.TopMost = false;
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
                    this.WindowState = FormWindowState.Normal;
                    this.TopMost = true;
                    startTime = DateTime.Now;
                    //}

                    break;
            }
        }

        void MoveMouseThread()
        {
            int secondsElapsed = 0;

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
    }
}
