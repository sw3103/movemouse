using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
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
        private const int TraceSeconds = 5;
        private const string MoveMouseXmlName = "Move Mouse.xml";
        private const string HomeAddress = "http://movemouse.codeplex.com/";
        private const string ContactAddress = "http://www.codeplex.com/site/users/view/sw3103/";
        private const string HelpAddress = "http://movemouse.codeplex.com/documentation/";
        private const string ScriptsHelpAddress = "https://movemouse.codeplex.com/wikipage?title=Custom%20Scripts";
        private const string PayPalAddress = "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=QZTWHD9CRW5XN";
        private const string VersionXmlUrl = "https://movemouse.svn.codeplex.com/svn/Version.xml";
        private const string StartScriptName = "Move Mouse - Start.vbs";
        private const string IntervalScriptName = "Move Mouse - Interval.vbs";
        private const string PauseScriptName = "Move Mouse - Pause.vbs";

        private readonly TimeSpan _waitBetweenUpdateChecks = new TimeSpan(7, 0, 0, 0);
        private readonly TimeSpan _waitUntilAutoMoveDetect = new TimeSpan(0, 0, 5);
        private readonly System.Windows.Forms.Timer _resumeTimer = new System.Windows.Forms.Timer();
        private readonly string _moveMouseWorkingDirectory = Environment.ExpandEnvironmentVariables(@"%APPDATA%\Ellanet\Move Mouse");
        private readonly bool _suppressAutoStart;
        private Thread _moveMouseThread;
        private DateTime _mmStartTime;
        private Point _startingMousePoint;
        private DateTime _traceTimeComplete = DateTime.MinValue;
        private Thread _traceMouseThread;
        private BlackoutStatusChangeEventArgs.BlackoutStatus _blackoutStatus = BlackoutStatusChangeEventArgs.BlackoutStatus.Ended;
        private DateTime _lastUpdateCheck = DateTime.MinValue;
        private string _scriptEditor = Path.Combine(Environment.ExpandEnvironmentVariables("%WINDIR%"), @"System32\notepad.exe");

        private delegate void UpdateCountdownProgressBarDelegate(ref ProgressBar pb, int delay, int elapsed);

        private delegate void ButtonPerformClickDelegate(ref Button b);

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

        public delegate void ClearComboBoxItemsDelegate(ref ComboBox cb);

        public event BlackoutStatusChangeHandler BlackoutStatusChange;
        public event NewVersionAvailableHandler NewVersionAvailable;

        public bool MinimiseToSystemTrayWarningShown { get; private set; }

        // ReSharper disable UnusedMember.Local
        // ReSharper disable InconsistentNaming

        [Flags]
        private enum MouseEventFlags
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
        private enum Win32Consts
        {
            INPUT_MOUSE = 0,
            INPUT_KEYBOARD = 1,
            INPUT_HARDWARE = 2,
        }

        [Flags]
        private enum ShowWindowCommands
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

        // ReSharper restore UnusedMember.Local
        // ReSharper disable NotAccessedField.Local

        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            private static readonly int SizeOf = Marshal.SizeOf(typeof (LASTINPUTINFO));
            [MarshalAs(UnmanagedType.U4)] public int cbSize;
            [MarshalAs(UnmanagedType.U4)] public int dwTime;
        }

        private struct INPUT
        {
            public int type;
            public MOUSEINPUT mi;
        }

        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public int time;
            public int dwExtraInfo;
        }

        // ReSharper restore NotAccessedField.Local
        // ReSharper restore InconsistentNaming

        [DllImport("user32.dll")]
        private static extern void mouse_event(
            uint dwFlags,
            uint dx,
            uint dy,
            uint dwData,
            int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(
            ref LASTINPUTINFO plii);

        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(
            int uAction,
            int uParam,
            ref int lpvParam,
            int flags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(
            uint nInputs,
            ref INPUT pInputs,
            int cbSize);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(
            string lpClassName,
            string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(
            IntPtr hWnd,
            ShowWindowCommands nCmdShow);

        public MouseForm(bool suppressAutoStart)
        {
            InitializeComponent();
            _suppressAutoStart = suppressAutoStart;
            int screenSaverTimeout = GetScreenSaverTimeout();

            if (screenSaverTimeout > 0)
            {
                if ((decimal) (screenSaverTimeout/2.0) > resumeNumericUpDown.Maximum)
                {
                    resumeNumericUpDown.Value = resumeNumericUpDown.Maximum;
                }
                else
                {
                    resumeNumericUpDown.Value = (decimal) (screenSaverTimeout/2.0);
                }
            }

            keystrokeCheckBox.CheckedChanged += keystrokeCheckBox_CheckedChanged;
            appActivateCheckBox.CheckedChanged += appActivateCheckBox_CheckedChanged;
            staticPositionCheckBox.CheckedChanged += startPositionCheckBox_CheckedChanged;
            resumeCheckBox.CheckedChanged += resumeCheckBox_CheckedChanged;
            launchAtLogonCheckBox.CheckedChanged += launchAtLogonCheckBox_CheckedChanged;
            blackoutCheckBox.CheckedChanged += blackoutCheckBox_CheckedChanged;
            customScriptsCheckBox.CheckedChanged += customScriptsCheckBox_CheckedChanged;
            PopulateBlackoutStartEndComboBoxes();
            ReadSettings();
            Icon = Properties.Resources.Mouse_Icon;
            Text = String.Format("Move Mouse ({0}.{1}.{2}) - {3}", Assembly.GetExecutingAssembly().GetName().Version.Major, Assembly.GetExecutingAssembly().GetName().Version.Minor, Assembly.GetExecutingAssembly().GetName().Version.Build, HomeAddress);
            FormClosing += MouseForm_FormClosing;
            Load += MouseForm_Load;
            Resize += MouseForm_Resize;
            actionButton.Click += actionButton_Click;
            moveMouseCheckBox.CheckedChanged += moveMouseCheckBox_CheckedChanged;
            clickMouseCheckBox.CheckedChanged += clickMouseCheckBox_CheckedChanged;
            autoPauseCheckBox.CheckedChanged += autoPauseCheckBox_CheckedChanged;
            _resumeTimer.Interval = 1000;
            _resumeTimer.Tick += resumeTimer_Tick;
            traceButton.Click += traceButton_Click;
            mousePictureBox.MouseEnter += mousePictureBox_MouseEnter;
            mousePictureBox.MouseLeave += mousePictureBox_MouseLeave;
            mousePictureBox.MouseClick += mousePictureBox_MouseClick;
            helpPictureBox.MouseEnter += helpPictureBox_MouseEnter;
            helpPictureBox.MouseLeave += helpPictureBox_MouseLeave;
            helpPictureBox.MouseClick += helpPictureBox_MouseClick;
            contactPictureBox.MouseEnter += contactPictureBox_MouseEnter;
            contactPictureBox.MouseLeave += contactPictureBox_MouseLeave;
            contactPictureBox.MouseClick += contactPictureBox_MouseClick;
            paypalPictureBox.MouseEnter += paypalPictureBox_MouseEnter;
            paypalPictureBox.MouseLeave += paypalPictureBox_MouseLeave;
            paypalPictureBox.MouseClick += paypalPictureBox_MouseClick;
            boStartComboBox.SelectedIndexChanged += boStartComboBox_SelectedIndexChanged;
            boEndComboBox.SelectedIndexChanged += boEndComboBox_SelectedIndexChanged;
            refreshButton.Click += refreshButton_Click;
            editScriptButton.Click += editScriptButton_Click;
            scriptsHelpPictureBox.MouseEnter += scriptsHelpPictureBox_MouseEnter;
            scriptsHelpPictureBox.MouseLeave += scriptsHelpPictureBox_MouseLeave;
            scriptsHelpPictureBox.MouseClick += scriptsHelpPictureBox_MouseClick;
            SetButtonTag(ref traceButton, GetButtonText(ref traceButton));
        }

        private void customScriptsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            editScriptButton.Enabled = customScriptsCheckBox.Checked;
        }

        private void scriptsHelpPictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                Process.Start(ScriptsHelpAddress);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void scriptsHelpPictureBox_MouseLeave(object sender, EventArgs e)
        {
            if (Cursor != Cursors.WaitCursor)
            {
                Cursor = Cursors.Default;
            }
        }

        private void scriptsHelpPictureBox_MouseEnter(object sender, EventArgs e)
        {
            if (Cursor != Cursors.WaitCursor)
            {
                Cursor = Cursors.Hand;
            }
        }

        private void editScriptButton_Click(object sender, EventArgs e)
        {
            var se = new ScriptEditor(_scriptEditor);

            if (se.ShowDialog() == DialogResult.OK)
            {
                _scriptEditor = se.ScriptEditorPath;
                SaveSettings();
                string scriptPath = null;

                switch (se.ScriptToEdit)
                {
                    case "Start":
                        scriptPath = Path.Combine(_moveMouseWorkingDirectory, StartScriptName);
                        break;
                    case "Interval":
                        scriptPath = Path.Combine(_moveMouseWorkingDirectory, IntervalScriptName);
                        break;
                    case "Pause":
                        scriptPath = Path.Combine(_moveMouseWorkingDirectory, PauseScriptName);
                        break;
                }

                if (!String.IsNullOrEmpty(scriptPath) && !File.Exists(scriptPath))
                {
                    CreateEmptyScript(scriptPath);
                }

                var p = new Process
                    {
                        StartInfo =
                            {
                                FileName = se.ScriptEditorPath,
                                Arguments = String.Format("\"{0}\"", scriptPath)
                            }
                    };
                p.Start();
            }
        }

        private void CreateEmptyScript(string path)
        {
            var sw = new StreamWriter(path, false);
            sw.WriteLine("' Move Mouse Custom Script");
            sw.WriteLine("' See {0} for some useful scripting examples.", ScriptsHelpAddress);
            sw.Close();
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(ListOpenWindows);
        }

        public override sealed string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        private void paypalPictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                Process.Start(PayPalAddress);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void paypalPictureBox_MouseLeave(object sender, EventArgs e)
        {
            if (Cursor != Cursors.WaitCursor)
            {
                Cursor = Cursors.Default;
            }
        }

        private void paypalPictureBox_MouseEnter(object sender, EventArgs e)
        {
            if (Cursor != Cursors.WaitCursor)
            {
                Cursor = Cursors.Hand;
            }
        }

        private void contactPictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                Process.Start(ContactAddress);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void contactPictureBox_MouseLeave(object sender, EventArgs e)
        {
            if (Cursor != Cursors.WaitCursor)
            {
                Cursor = Cursors.Default;
            }
        }

        private void contactPictureBox_MouseEnter(object sender, EventArgs e)
        {
            if (Cursor != Cursors.WaitCursor)
            {
                Cursor = Cursors.Hand;
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

        private void boEndComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (boEndComboBox.SelectedIndex < boStartComboBox.SelectedIndex)
            {
                boStartComboBox.SelectedIndex = boEndComboBox.SelectedIndex;
            }
        }

        private void boStartComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (boEndComboBox.SelectedIndex < boStartComboBox.SelectedIndex)
            {
                boEndComboBox.SelectedIndex = boStartComboBox.SelectedIndex;
            }
        }

        private void PopulateBlackoutStartEndComboBoxes()
        {
            int minute = 0;
            int hour = 0;
            bool breakLoop = false;

            while (!breakLoop)
            {
                string time = String.Format("{0}:{1}", hour.ToString(CultureInfo.InvariantCulture).PadLeft(2, Convert.ToChar("0")), minute.ToString(CultureInfo.InvariantCulture).PadLeft(2, Convert.ToChar("0")));
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

        private void blackoutCheckBox_CheckedChanged(object sender, EventArgs e)
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

        private void helpPictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                Process.Start(HelpAddress);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void helpPictureBox_MouseLeave(object sender, EventArgs e)
        {
            if (Cursor != Cursors.WaitCursor)
            {
                Cursor = Cursors.Default;
            }
        }

        private void helpPictureBox_MouseEnter(object sender, EventArgs e)
        {
            if (Cursor != Cursors.WaitCursor)
            {
                Cursor = Cursors.Hand;
            }
        }

        private void mousePictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                Process.Start(HomeAddress);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void mousePictureBox_MouseLeave(object sender, EventArgs e)
        {
            if (Cursor != Cursors.WaitCursor)
            {
                Cursor = Cursors.Default;
            }
        }

        private void mousePictureBox_MouseEnter(object sender, EventArgs e)
        {
            if (Cursor != Cursors.WaitCursor)
            {
                Cursor = Cursors.Hand;
            }
        }

        private void appActivateCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            processComboBox.Enabled = appActivateCheckBox.Checked;
            refreshButton.Enabled = appActivateCheckBox.Checked;
        }

        private void ListOpenWindows(object stareInfo)
        {
            try
            {
                ClearComboBoxItems(ref processComboBox);

                foreach (Process p in Process.GetProcesses())
                {
                    if (!String.IsNullOrEmpty(p.MainWindowTitle) && !processComboBox.Items.Contains(p.MainWindowTitle))
                    {
                        AddComboBoxItem(ref processComboBox, p.MainWindowTitle);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
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

        private void MouseForm_Resize(object sender, EventArgs e)
        {
            if ((WindowState == FormWindowState.Minimized) && minimiseToSystemTrayCheckBox.Checked)
            {
                ShowInTaskbar = false;
            }
        }

        private void launchAtLogonCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (launchAtLogonCheckBox.Checked)
            {
                var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);

                if (key != null)
                {
                    key.SetValue("Move Mouse", Application.ExecutablePath);
                }
            }
            else
            {
                var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);

                if (key != null)
                {
                    key.DeleteValue("Move Mouse");
                }
            }
        }

        private void MouseForm_Load(object sender, EventArgs e)
        {
            if (!Directory.Exists(_moveMouseWorkingDirectory))
            {
                Directory.CreateDirectory(_moveMouseWorkingDirectory);
            }

            ThreadPool.QueueUserWorkItem(CheckForUpdate);
            ThreadPool.QueueUserWorkItem(ListOpenWindows);

            if (startOnLaunchCheckBox.Checked && !_suppressAutoStart)
            {
                actionButton.PerformClick();
            }
        }

        private void CheckForUpdate(object stateInfo)
        {
            // ReSharper disable PossibleNullReferenceException

            try
            {
                if (_lastUpdateCheck.Add(_waitBetweenUpdateChecks) < DateTime.Now)
                {
                    _lastUpdateCheck = DateTime.Now;
                    var versionXmlDoc = new XmlDocument();
                    versionXmlDoc.Load(VersionXmlUrl);
                    var availableVersion = new Version(Convert.ToInt32(versionXmlDoc.SelectSingleNode("version/major").InnerText), Convert.ToInt32(versionXmlDoc.SelectSingleNode("version/minor").InnerText), Convert.ToInt32(versionXmlDoc.SelectSingleNode("version/build").InnerText));

                    if (availableVersion > Assembly.GetExecutingAssembly().GetName().Version)
                    {
                        DateTime released = Convert.ToDateTime(versionXmlDoc.SelectSingleNode("version/released_date").InnerText);
                        DateTime advertised = Convert.ToDateTime(versionXmlDoc.SelectSingleNode("version/advertised_date").InnerText);
                        var features = new List<string>();
                        var fixes = new List<string>();

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
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            // ReSharper restore PossibleNullReferenceException
        }

        private void traceButton_Click(object sender, EventArgs e)
        {
            _traceTimeComplete = DateTime.Now.AddSeconds(TraceSeconds);

            if ((_traceMouseThread == null) || (_traceMouseThread.ThreadState != System.Threading.ThreadState.Running))
            {
                _traceMouseThread = new Thread(TraceMouse);
                _traceMouseThread.Start();
            }
        }

        private void TraceMouse()
        {
            do
            {
                SetNumericUpDownValue(ref xNumericUpDown, Cursor.Position.X);
                SetNumericUpDownValue(ref yNumericUpDown, Cursor.Position.Y);
                SetButtonText(ref traceButton, String.Format("{0} ({1})", Convert.ToString(GetButtonTag(ref traceButton)), _traceTimeComplete.Subtract(DateTime.Now).TotalSeconds.ToString("0.0")));
                Thread.Sleep(100);
            } while (_traceTimeComplete > DateTime.Now);

            SetButtonText(ref traceButton, Convert.ToString(GetButtonTag(ref traceButton)));
        }

        private void startPositionCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            xNumericUpDown.Enabled = staticPositionCheckBox.Checked;
            yNumericUpDown.Enabled = staticPositionCheckBox.Checked;
            traceButton.Enabled = staticPositionCheckBox.Checked;
        }

        private void keystrokeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!moveMouseCheckBox.Checked && !clickMouseCheckBox.Checked && !keystrokeCheckBox.Checked)
            {
                keystrokeCheckBox.Checked = true;
            }

            keystrokeComboBox.Enabled = keystrokeCheckBox.Checked;
        }

        private void resumeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            resumeNumericUpDown.Enabled = resumeCheckBox.Checked;
        }

        private void resumeTimer_Tick(object sender, EventArgs e)
        {
            //Debug.WriteLine(GetLastInputTime().ToString());

            if (GetCheckBoxChecked(ref resumeCheckBox) && (GetLastInputTime() > resumeNumericUpDown.Value))
            {
                ButtonPerformClick(ref actionButton);
            }
        }

        private void autoPauseCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _startingMousePoint = Cursor.Position;
        }

        private void clickMouseCheckBox_CheckedChanged(object sender, EventArgs e)
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

        private void moveMouseCheckBox_CheckedChanged(object sender, EventArgs e)
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

        private void MouseForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_moveMouseThread != null)
            {
                _moveMouseThread.Abort();
            }

            _resumeTimer.Stop();
            SaveSettings();
        }

        private void actionButton_Click(object sender, EventArgs e)
        {
            switch (actionButton.Text)
            {
                case "Pause":

                    if (_moveMouseThread != null)
                    {
                        _moveMouseThread.Abort();
                    }

                    _resumeTimer.Start();
                    actionButton.Text = "Start";
                    Opacity = 1.0;
                    //this.TopMost = false;

                    if (minimiseOnPauseCheckBox.Checked)
                    {
                        WindowState = FormWindowState.Minimized;
                    }

                    LaunchScript(PauseScriptName);
                    break;
                default:
                    LaunchScript(StartScriptName);
                    _resumeTimer.Stop();
                    _moveMouseThread = new Thread(MoveMouseThread);
                    _moveMouseThread.Start();
                    actionButton.Text = "Pause";
                    Opacity = .75;
                    _mmStartTime = DateTime.Now;
                    WindowState = minimiseOnStartCheckBox.Checked ? FormWindowState.Minimized : FormWindowState.Normal;
                    SaveSettings();
                    break;
            }
        }

        private void LaunchScript(string name)
        {
            if (GetCheckBoxChecked(ref customScriptsCheckBox))
            {
                string scriptPath = Path.Combine(_moveMouseWorkingDirectory, name);

                if (File.Exists(scriptPath))
                {
                    var p = new Process
                        {
                            StartInfo =
                                {
                                    FileName = Path.Combine(Environment.ExpandEnvironmentVariables("%WINDIR%"), @"System32\wscript.exe"),
                                    Arguments = String.Format("\"{0}\"", scriptPath),
                                    WindowStyle = ProcessWindowStyle.Normal
                                }
                        };
                    p.Start();
                }
            }
        }

        private void MoveMouseThread()
        {
            int secondsElapsed = 0;
            _blackoutStatus = BlackoutStatusChangeEventArgs.BlackoutStatus.Ended;

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
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            do
            {
                Thread.Sleep(1000);

                if (!BlackoutInEffect())
                {
                    UpdateCountdownProgressBar(ref countdownProgressBar, Convert.ToInt32(delayNumericUpDown.Value), secondsElapsed);
                    secondsElapsed += 1;

                    if (_blackoutStatus == BlackoutStatusChangeEventArgs.BlackoutStatus.Started)
                    {
                        _blackoutStatus = BlackoutStatusChangeEventArgs.BlackoutStatus.Ended;
                        string startTime;
                        string endTime;
                        GetNextBlackoutStatusChangeTime(out startTime, out endTime);
                        OnBlackoutStatusChange(this, new BlackoutStatusChangeEventArgs(_blackoutStatus, startTime, endTime));
                    }

                    if (autoPauseCheckBox.Checked && (_mmStartTime.Add(_waitUntilAutoMoveDetect) < DateTime.Now) && (_startingMousePoint != Cursor.Position))
                    {
                        ButtonPerformClick(ref actionButton);
                    }
                    else
                    {
                        _startingMousePoint = Cursor.Position;
                    }

                    if (secondsElapsed > Convert.ToInt32(delayNumericUpDown.Value))
                    {
                        LaunchScript(IntervalScriptName);

                        if (clickMouseCheckBox.Checked)
                        {
                            mouse_event((int) MouseEventFlags.LEFTDOWN, 0, 0, 0, 0);
                            mouse_event((int) MouseEventFlags.LEFTUP, 0, 0, 0, 0);
                        }

                        if (moveMouseCheckBox.Checked)
                        {
                            if (!stealthCheckBox.Checked)
                            {
                                const int mouseMoveLoopSleep = 1;
                                const int mouseSpeed = 1;
                                const int moveSquareSize = 10;
                                var cursorStartPosition = Cursor.Position;

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
                    if (_blackoutStatus == BlackoutStatusChangeEventArgs.BlackoutStatus.Ended)
                    {
                        _blackoutStatus = BlackoutStatusChangeEventArgs.BlackoutStatus.Started;
                        string startTime;
                        string endTime;
                        GetNextBlackoutStatusChangeTime(out startTime, out endTime);
                        OnBlackoutStatusChange(this, new BlackoutStatusChangeEventArgs(_blackoutStatus, startTime, endTime));
                    }
                }
            } while (DateTime.Now < DateTime.MaxValue);
        }

        private bool BlackoutInEffect()
        {
            try
            {
                if (GetCheckBoxChecked(ref blackoutCheckBox))
                {
                    switch (GetComboBoxSelectedItem(ref insideOutsideComboBox).ToString())
                    {
                        case "inside":

                            if ((DateTime.Now > Convert.ToDateTime(String.Format("{0} {1}", DateTime.Now.ToString("yyyy-MMM-dd"), GetComboBoxSelectedItem(ref boStartComboBox)))) && (DateTime.Now < Convert.ToDateTime(String.Format("{0} {1}", DateTime.Now.ToString("yyyy-MMM-dd"), GetComboBoxSelectedItem(ref boEndComboBox)))))
                            {
                                return true;
                            }

                            break;
                        case "outside":

                            if ((DateTime.Now < Convert.ToDateTime(String.Format("{0} {1}", DateTime.Now.ToString("yyyy-MMM-dd"), GetComboBoxSelectedItem(ref boStartComboBox)))) || (DateTime.Now > Convert.ToDateTime(String.Format("{0} {1}", DateTime.Now.ToString("yyyy-MMM-dd"), GetComboBoxSelectedItem(ref boEndComboBox)))))
                            {
                                return true;
                            }

                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return false;
        }

        private void GetNextBlackoutStatusChangeTime(out string startTime, out string endTime)
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
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void UpdateCountdownProgressBar(ref ProgressBar pb, int delay, int elapsed)
        {
            if (InvokeRequired)
            {
                Invoke(new UpdateCountdownProgressBarDelegate(UpdateCountdownProgressBar), new object[] {pb, delay, elapsed});
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

        private void ButtonPerformClick(ref Button b)
        {
            if (InvokeRequired)
            {
                Invoke(new ButtonPerformClickDelegate(ButtonPerformClick), new object[] {b});
            }
            else
            {
                b.PerformClick();
            }
        }

        //private Point GetControlScreenLocation(ref Control c)
        //{
        //    if (InvokeRequired)
        //    {
        //        return (Point) Invoke(new GetControlScreenLocationDelegate(GetControlScreenLocation), new object[] {c});
        //    }

        //    return this.PointToScreen(c.Location);
        //}

        private object GetComboBoxSelectedItem(ref ComboBox cb)
        {
            if (InvokeRequired)
            {
                return Invoke(new GetComboBoxSelectedItemDelegate(GetComboBoxSelectedItem), new object[] {cb});
            }

            return cb.SelectedItem;
        }

        private int GetComboBoxSelectedIndex(ref ComboBox cb)
        {
            if (InvokeRequired)
            {
                return (int) Invoke(new GetComboBoxSelectedIndexDelegate(GetComboBoxSelectedIndex), new object[] {cb});
            }

            return cb.SelectedIndex;
        }

        private void SetNumericUpDownValue(ref NumericUpDown nud, int value)
        {
            if (InvokeRequired)
            {
                Invoke(new SetNumericUpDownValueDelegate(SetNumericUpDownValue), new object[] {nud, value});
            }
            else
            {
                nud.Value = value;
            }
        }

        private void SetButtonText(ref Button b, string text)
        {
            if (InvokeRequired)
            {
                Invoke(new SetButtonTextDelegate(SetButtonText), new object[] {b, text});
            }
            else
            {
                b.Text = text;
            }
        }

        private void SetButtonTag(ref Button b, object o)
        {
            if (InvokeRequired)
            {
                Invoke(new SetButtonTagDelegate(SetButtonTag), new[] {b, o});
            }
            else
            {
                b.Tag = o;
            }
        }

        private object GetButtonTag(ref Button b)
        {
            if (InvokeRequired)
            {
                return Invoke(new GetButtonTagDelegate(GetButtonTag), new object[] {b});
            }

            return b.Tag;
        }

        private string GetButtonText(ref Button b)
        {
            if (InvokeRequired)
            {
                return Convert.ToString(Invoke(new GetButtonTextDelegate(GetButtonText), new object[] {b}));
            }

            return b.Text;
        }

        private bool GetCheckBoxChecked(ref CheckBox cb)
        {
            if (InvokeRequired)
            {
                return Convert.ToBoolean(Invoke(new GetCheckBoxCheckedDelegate(GetCheckBoxChecked), new object[] {cb}));
            }

            return cb.Checked;
        }

        private void AddComboBoxItem(ref ComboBox cb, string item)
        {
            if (InvokeRequired)
            {
                Invoke(new AddComboBoxItemDelegate(AddComboBoxItem), new object[] {cb, item});
            }
            else
            {
                cb.Items.Add(item);
            }
        }

        private void ClearComboBoxItems(ref ComboBox cb)
        {
            if (InvokeRequired)
            {
                Invoke(new ClearComboBoxItemsDelegate(ClearComboBoxItems), new object[] {cb});
            }
            else
            {
                cb.Items.Clear();
            }
        }

        private int GetLastInputTime()
        {
            int idleTime = 0;
            var lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = Marshal.SizeOf(lastInputInfo);
            lastInputInfo.dwTime = 0;
            int envTicks = Environment.TickCount;

            if (GetLastInputInfo(ref lastInputInfo))
            {
                int lastInputTick = lastInputInfo.dwTime;
                idleTime = envTicks - lastInputTick;
            }

            return ((idleTime > 0) ? (idleTime/1000) : 0);
        }

        private int GetScreenSaverTimeout()
        {
            // ReSharper disable InconsistentNaming
            const int SPI_GETSCREENSAVERTIMEOUT = 14;
            // ReSharper restore InconsistentNaming
            int value = 0;
            SystemParametersInfo(SPI_GETSCREENSAVERTIMEOUT, 0, ref value, 0);
            return value;
        }

        private void MoveMouse(Point point)
        {
            var mi = new MOUSEINPUT
                {
                    dx = point.X,
                    dy = point.Y,
                    mouseData = 0,
                    time = 0,
                    dwFlags = Convert.ToInt32(MouseEventFlags.MOVE),
                    dwExtraInfo = 0
                };
            var input = new INPUT
                {
                    mi = mi,
                    type = Convert.ToInt32(Win32Consts.INPUT_MOUSE)
                };
            SendInput(1, ref input, 28);
        }

        // ReSharper disable PossibleNullReferenceException

        private void ReadSettings()
        {
            try
            {
                if (File.Exists(Path.Combine(_moveMouseWorkingDirectory, MoveMouseXmlName)))
                {
                    var settingsXmlDoc = new XmlDocument();
                    settingsXmlDoc.Load(Path.Combine(_moveMouseWorkingDirectory, MoveMouseXmlName));
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
                    _lastUpdateCheck = Convert.ToDateTime(settingsXmlDoc.SelectSingleNode("settings/last_update_check").InnerText);
                    MinimiseToSystemTrayWarningShown = Convert.ToBoolean(settingsXmlDoc.SelectSingleNode("settings/system_tray_warning_shown").InnerText);
                    customScriptsCheckBox.Checked = Convert.ToBoolean(settingsXmlDoc.SelectSingleNode("settings/enable_custom_scripts").InnerText);
                    _scriptEditor = settingsXmlDoc.SelectSingleNode("settings/script_editor").InnerText;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void SaveSettings()
        {
            try
            {
                var settingsXmlDoc = new XmlDocument();
                settingsXmlDoc.LoadXml("<settings><second_delay /><move_mouse_pointer /><stealth_mode /><enable_static_position /><x_static_position /><y_static_position /><click_left_mouse_button /><send_keystroke /><keystroke /><pause_when_mouse_moved /><automatically_resume /><resume_seconds /><automatically_start_on_launch /><automatically_launch_on_logon /><minimise_on_pause /><minimise_on_start /><minimise_to_system_tray /><activate_application /><activate_application_title /><blackout_schedule_enabled /><blackout_schedule_scope /><blackout_schedule_start /><blackout_schedule_end /><last_update_check /><system_tray_warning_shown /><enable_custom_scripts /><script_editor /></settings>");
                settingsXmlDoc.SelectSingleNode("settings/second_delay").InnerText = Convert.ToDecimal(delayNumericUpDown.Value).ToString(CultureInfo.InvariantCulture);
                settingsXmlDoc.SelectSingleNode("settings/move_mouse_pointer").InnerText = moveMouseCheckBox.Checked.ToString();
                settingsXmlDoc.SelectSingleNode("settings/stealth_mode").InnerText = stealthCheckBox.Checked.ToString();
                settingsXmlDoc.SelectSingleNode("settings/enable_static_position").InnerText = staticPositionCheckBox.Checked.ToString();
                settingsXmlDoc.SelectSingleNode("settings/x_static_position").InnerText = Convert.ToDecimal(xNumericUpDown.Value).ToString(CultureInfo.InvariantCulture);
                settingsXmlDoc.SelectSingleNode("settings/y_static_position").InnerText = Convert.ToDecimal(yNumericUpDown.Value).ToString(CultureInfo.InvariantCulture);
                settingsXmlDoc.SelectSingleNode("settings/click_left_mouse_button").InnerText = clickMouseCheckBox.Checked.ToString();
                settingsXmlDoc.SelectSingleNode("settings/send_keystroke").InnerText = keystrokeCheckBox.Checked.ToString();
                settingsXmlDoc.SelectSingleNode("settings/keystroke").InnerText = keystrokeComboBox.Text;
                settingsXmlDoc.SelectSingleNode("settings/pause_when_mouse_moved").InnerText = autoPauseCheckBox.Checked.ToString();
                settingsXmlDoc.SelectSingleNode("settings/automatically_resume").InnerText = resumeCheckBox.Checked.ToString();
                settingsXmlDoc.SelectSingleNode("settings/resume_seconds").InnerText = Convert.ToDecimal(resumeNumericUpDown.Value).ToString(CultureInfo.InvariantCulture);
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
                settingsXmlDoc.SelectSingleNode("settings/last_update_check").InnerText = _lastUpdateCheck.ToString("yyyy-MMM-dd HH:mm:ss");
                settingsXmlDoc.SelectSingleNode("settings/system_tray_warning_shown").InnerText = "True";
                settingsXmlDoc.SelectSingleNode("settings/enable_custom_scripts").InnerText = customScriptsCheckBox.Checked.ToString();
                settingsXmlDoc.SelectSingleNode("settings/script_editor").InnerText = _scriptEditor;
                settingsXmlDoc.Save(Path.Combine(_moveMouseWorkingDirectory, MoveMouseXmlName));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        // ReSharper restore PossibleNullReferenceException

        private Process GetProessByMainWindowTitle(string title)
        {
            foreach (var p in Process.GetProcesses())
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
