using Ellanet.Classes;
using Ellanet.Events;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Management;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using ComponentFactory.Krypton.Toolkit;

#pragma warning disable 414

namespace Ellanet.Forms
{
    public partial class MouseForm : KryptonForm
    {
        private const int TraceSeconds = 5;

        private const string MoveMouseXmlName = "Move Mouse.xml";

        //private const string HomeAddress = "http://movemouse.codeplex.com/";
        private const string ContactAddress = "mailto:contact@movemouse.co.uk?subject=Move%20Mouse%20Feedback";
        //private const string HelpAddress = "http://movemouse.codeplex.com/documentation/";
        //private const string ScriptsHelpAddress = "https://movemouse.codeplex.com/wikipage?title=Custom%20Scripts";
        private const string PayPalAddress = "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=QZTWHD9CRW5XN";
        private const string UpdateXmlUrl = "https://raw.githubusercontent.com/sw3103/movemouse/master/Update_3x.xml";
        private const string MiceResourceUrlPrefix = "https://raw.githubusercontent.com/sw3103/movemouse/master/Mice/";
        private const string TwitterAddress = "https://twitter.com/movemouse";
        private const string MiceXmlName = "Mice.xml";
        private const string StartScriptName = "Move Mouse - Start";
        private const string IntervalScriptName = "Move Mouse - Interval";
        private const string PauseScriptName = "Move Mouse - Pause";
        private const int MaxScriptPathLength = 40;

        private readonly TimeSpan _waitBetweenUpdateChecks = new TimeSpan(7, 0, 0, 0);
        private readonly TimeSpan _waitUntilAutoMoveDetect = new TimeSpan(0, 0, 2);
        private readonly System.Windows.Forms.Timer _mouseTimer = new System.Windows.Forms.Timer();
        private readonly System.Windows.Forms.Timer _resumeTimer = new System.Windows.Forms.Timer();
        private readonly System.Windows.Forms.Timer _autoStartTimer = new System.Windows.Forms.Timer();
        private readonly System.Windows.Forms.Timer _autoPauseTimer = new System.Windows.Forms.Timer();
        private readonly string _moveMouseTempDirectory = Environment.ExpandEnvironmentVariables(@"%Temp%\Ellanet\Move Mouse");
        private readonly bool _suppressAutoStart;
        private readonly string _homeAddress;
        private DateTime _mmStartTime;
        private Point _startingMousePoint;
        private DateTime _traceTimeComplete = DateTime.MinValue;
        private Thread _traceMouseThread;
        private BlackoutStatusChangedEventArgs.BlackoutStatus _blackoutStatus = BlackoutStatusChangedEventArgs.BlackoutStatus.Inactive;
        private DateTime _lastUpdateCheck = DateTime.MinValue;
        private string _scriptEditor = Path.Combine(Environment.ExpandEnvironmentVariables("%WINDIR%"), @"System32\notepad.exe");
        private List<ScriptingLanguage> _scriptingLanguages;
        private int _mouseTimerTicks;
        private List<KeyValuePair<TimeSpan, TimeSpan>> _blackoutSchedules;
        private List<TimeSpan> _startSchedules;
        private List<TimeSpan> _pauseSchedules;
        private List<CelebrityMouse> _celebrityMice;
        private bool _easterEggActive;
        private PowerLineStatusChangedEventArgs.PowerLineStatus _powerLineStatus = PowerLineStatusChangedEventArgs.PowerLineStatus.Online;

        private delegate void UpdateCountdownProgressBarDelegate(ref ProgressBar pb, int delay, int elapsed);

        private delegate void ButtonPerformClickDelegate(ref Button b);

        private delegate void KryptonButtonPerformClickDelegate(ref KryptonButton kb);

        private delegate object GetComboBoxSelectedItemDelegate(ref ComboBox cb);

        private delegate int GetComboBoxSelectedIndexDelegate(ref ComboBox cb);

        private delegate object GetComboBoxTagDelegate(ref ComboBox cb);

        private delegate void SetNumericUpDownValueDelegate(ref NumericUpDown nud, int value);

        private delegate void SetButtonTextDelegate(ref Button b, string text);

        private delegate void SetButtonTagDelegate(ref Button b, object o);

        private delegate object GetButtonTagDelegate(ref Button b);

        private delegate string GetButtonTextDelegate(ref Button b);

        private delegate bool GetCheckBoxCheckedDelegate(ref KryptonCheckBox cb);

        private delegate void AddComboBoxItemDelegate(ref ComboBox cb, string item, bool selected);

        private delegate void ClearComboBoxItemsDelegate(ref ComboBox cb);

        private delegate void ShowCelebrityMouseDelegate(CelebrityMouse cb);

        private delegate bool IsWindowMinimisedDelegate(IntPtr handle);

        private delegate void ZeroParameterDelegate();

        public delegate void BlackoutStatusChangedHandler(object sender, BlackoutStatusChangedEventArgs e);

        public delegate void NewVersionAvailableHandler(object sender, NewVersionAvailableEventArgs e);

        public delegate void ScheduleArrivedHandler(object sender, ScheduleArrivedEventArgs e);

        public delegate void PowerLineStatusChangedHandler(object sender, PowerLineStatusChangedEventArgs e);

        public delegate void PowerShellexecutionPolicyWarningHandler(object sender);

        public delegate void HookKeyStatusChangedHandler(object sender, HookKeyStatusChangedEventArgs e);

        public delegate void MoveMouseStartedHandler();

        public delegate void MoveMousePausedHandler();

        public delegate void MoveMouseStoppedHandler();

        public event BlackoutStatusChangedHandler BlackoutStatusChanged;
        public event NewVersionAvailableHandler NewVersionAvailable;
        public event ScheduleArrivedHandler ScheduleArrived;
        public event PowerLineStatusChangedHandler PowerLineStatusChanged;
        public event PowerShellexecutionPolicyWarningHandler PowerShellexecutionPolicyWarning;
        public event HookKeyStatusChangedHandler HookKeyStatusChanged;
        public event MoveMouseStartedHandler MoveMouseStarted;
        public event MoveMousePausedHandler MoveMousePaused;
        public event MoveMouseStoppedHandler MoveMouseStopped;

        public bool MinimiseToSystemTrayWarningShown { get; private set; }

        // ReSharper disable UnusedMember.Global
        // ReSharper disable UnusedMember.Local
        // ReSharper disable InconsistentNaming
        // ReSharper disable NotAccessedField.Local

        private enum Script
        {
            Start,
            Interval,
            Pause
        }

        private enum PowerShellExecutionPolicy
        {
            Restricted,
            AllSigned,
            RemoteSigned,
            Unrestricted
        }

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

        // ReSharper restore UnusedMember.Global
        // ReSharper restore UnusedMember.Local

        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            private static readonly int SizeOf = Marshal.SizeOf(typeof(LASTINPUTINFO));
            [MarshalAs(UnmanagedType.U4)] public int cbSize;
            [MarshalAs(UnmanagedType.U4)] public int dwTime;
        }

        private struct INPUT
        {
            public int type;
            public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public MouseEventFlags dwFlags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        // ReSharper disable MemberCanBePrivate.Local

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        private struct WINDOWPLACEMENT
        {
            public int length;
            public readonly int flags;
            public readonly ShowWindowCommands showCmd;
            public readonly Point ptMinPosition;
            public readonly Point ptMaxPosition;
            public readonly Rectangle rcNormalPosition;
        }

        // ReSharper restore MemberCanBePrivate.Local
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

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowPlacement(
            IntPtr hWnd,
            ref WINDOWPLACEMENT lpwndpl);

        public MouseForm(bool suppressAutoStart)
        {
            InitializeComponent();
            _suppressAutoStart = suppressAutoStart;
            int screenSaverTimeout = GetScreenSaverTimeout();

            if (screenSaverTimeout > 0)
            {
                if ((decimal) (screenSaverTimeout / 2.0) > resumeNumericUpDown.Maximum)
                {
                    resumeNumericUpDown.Value = resumeNumericUpDown.Maximum;
                }
                else
                {
                    resumeNumericUpDown.Value = (decimal) (screenSaverTimeout / 2.0);
                }
            }

            scriptEditorLabel.Text = _scriptEditor;
            keystrokeCheckBox.CheckedChanged += keystrokeCheckBox_CheckedChanged;
            appActivateCheckBox.CheckedChanged += appActivateCheckBox_CheckedChanged;
            staticPositionCheckBox.CheckedChanged += startPositionCheckBox_CheckedChanged;
            resumeCheckBox.CheckedChanged += resumeCheckBox_CheckedChanged;
            launchAtLogonCheckBox.CheckedChanged += launchAtLogonCheckBox_CheckedChanged;
            executeStartScriptCheckBox.CheckedChanged += executeStartScriptCheckBox_CheckedChanged;
            executeIntervalScriptCheckBox.CheckedChanged += executeIntervalScriptCheckBox_CheckedChanged;
            executePauseScriptCheckBox.CheckedChanged += executePauseScriptCheckBox_CheckedChanged;
            hotkeyCheckBox.CheckedChanged += hotkeyCheckBox_CheckedChanged;
            hotkeyComboBox.SelectedIndexChanged += hotkeyComboBox_SelectedIndexChanged;
            scriptEditorLabel.TextChanged += scriptEditorLabel_TextChanged;
            ListScriptingLanguages();
            ReadSettings();
            _homeAddress = IsWindows10() ? "http://www.movemouse.co.uk/" : "https://github.com/sw3103/movemouse/";
            Icon = Properties.Resources.Mouse_Icon;
            Text = String.Format("Move Mouse ({0}.{1}.{2}) - {3}", Assembly.GetExecutingAssembly().GetName().Version.Major, Assembly.GetExecutingAssembly().GetName().Version.Minor, Assembly.GetExecutingAssembly().GetName().Version.Build, _homeAddress);
            FormClosing += MouseForm_FormClosing;
            Load += MouseForm_Load;
            Resize += MouseForm_Resize;
            actionButton.Click += actionButton_Click;
            moveMouseCheckBox.CheckedChanged += moveMouseCheckBox_CheckedChanged;
            clickMouseCheckBox.CheckedChanged += clickMouseCheckBox_CheckedChanged;
            autoPauseCheckBox.CheckedChanged += autoPauseCheckBox_CheckedChanged;
            _mouseTimer.Interval = 1000;
            _mouseTimer.Tick += _mouseTimer_Tick;
            _resumeTimer.Interval = 1000;
            _resumeTimer.Tick += _resumeTimer_Tick;
            _autoStartTimer.Interval = 1000;
            _autoStartTimer.Tick += _autoStartTimer_Tick;
            _autoPauseTimer.Interval = 1000;
            _autoPauseTimer.Tick += _autoPauseTimer_Tick;
            traceButton.Click += traceButton_Click;
            helpPictureBox.MouseEnter += helpPictureBox_MouseEnter;
            helpPictureBox.MouseLeave += helpPictureBox_MouseLeave;
            //helpPictureBox.MouseClick += helpPictureBox_MouseClick;
            contactPictureBox.MouseEnter += contactPictureBox_MouseEnter;
            contactPictureBox.MouseLeave += contactPictureBox_MouseLeave;
            contactPictureBox.MouseClick += contactPictureBox_MouseClick;
            paypalPictureBox.MouseEnter += paypalPictureBox_MouseEnter;
            paypalPictureBox.MouseLeave += paypalPictureBox_MouseLeave;
            paypalPictureBox.MouseClick += paypalPictureBox_MouseClick;
            twitterPictureBox.MouseEnter += twitterPictureBox_MouseEnter;
            twitterPictureBox.MouseLeave += twitterPictureBox_MouseLeave;
            twitterPictureBox.MouseClick += twitterPictureBox_MouseClick;
            homePictureBox.MouseEnter += homePictureBox_MouseEnter;
            homePictureBox.MouseLeave += homePictureBox_MouseLeave;
            homePictureBox.MouseClick += homePictureBox_MouseClick;
            refreshButton.Click += refreshButton_Click;
            scriptsHelpPictureBox.MouseEnter += scriptsHelpPictureBox_MouseEnter;
            scriptsHelpPictureBox.MouseLeave += scriptsHelpPictureBox_MouseLeave;
            //scriptsHelpPictureBox.MouseClick += scriptsHelpPictureBox_MouseClick;
            addScheduleButton.Click += addScheduleButton_Click;
            editScheduleButton.Click += editScheduleButton_Click;
            removeScheduleButton.Click += removeScheduleButton_Click;
            addBlackoutButton.Click += addBlackoutButton_Click;
            editBlackoutButton.Click += editBlackoutButton_Click;
            removeBlackoutButton.Click += removeBlackoutButton_Click;
            changeScriptEditorButton.Click += changeScriptEditorButton_Click;
            editStartScriptButton.Click += editStartScriptButton_Click;
            editIntervalScriptButton.Click += editIntervalScriptButton_Click;
            editPauseScriptButton.Click += editPauseScriptButton_Click;
            importStartScriptButton.Click += ImportStartScriptButton_Click;
            importIntervalScriptButton.Click += ImportIntervalScriptButton_Click;
            importPauseScriptButton.Click += ImportPauseScriptButton_Click;
            scheduleListView.SelectedIndexChanged += scheduleListView_SelectedIndexChanged;
            scheduleListView.DoubleClick += scheduleListView_DoubleClick;
            blackoutListView.SelectedIndexChanged += blackoutListView_SelectedIndexChanged;
            blackoutListView.DoubleClick += blackoutListView_DoubleClick;
            mousePictureBox.DoubleClick += mousePictureBox_DoubleClick;
            SetButtonTag(ref traceButton, GetButtonText(ref traceButton));
            ToolTip paypalToolTip = new ToolTip();
            paypalToolTip.SetToolTip(paypalPictureBox, "Donate to the Move Mouse project using PayPal");
            ToolTip twitterToolTip = new ToolTip();
            twitterToolTip.SetToolTip(twitterPictureBox, "Follow Move Mouse on Twitter");
            ToolTip homeToolTip = new ToolTip();
            homeToolTip.SetToolTip(homePictureBox, "Move Mouse project home on CodePlex (home...mousehole...get it!?!?)");
        }

        private void ImportPauseScriptButton_Click(object sender, EventArgs e)
        {
            ImportScript(Script.Pause);
        }

        private void ImportIntervalScriptButton_Click(object sender, EventArgs e)
        {
            ImportScript(Script.Interval);
        }

        private void ImportStartScriptButton_Click(object sender, EventArgs e)
        {
            ImportScript(Script.Start);
        }

        private void ImportScript(Script script)
        {
            var sl = GetScriptingLanguage(GetComboBoxSelectedItem(ref scriptLanguageComboBox).ToString());

            if (sl != null)
            {
                var ofd = new OpenFileDialog
                {
                    CheckFileExists = true,
                    Multiselect = false,
                    DefaultExt = sl.FileExtension,
                    Filter = String.Format("{1} Script (*.{0})|*.{0}", sl.FileExtension, sl.Name)
                };
                var dr = ofd.ShowDialog();

                if (dr == DialogResult.OK)
                {
                    string scriptPath = GetScriptPath(script, sl.FileExtension);

                    if (!String.IsNullOrEmpty(scriptPath) && !String.IsNullOrEmpty(ofd.FileName))
                    {
                        File.Copy(ofd.FileName, scriptPath, true);
                    }
                }
            }
        }

        private void homePictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                Process.Start(_homeAddress);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void homePictureBox_MouseLeave(object sender, EventArgs e)
        {
            if (Cursor != Cursors.WaitCursor)
            {
                Cursor = Cursors.Default;
            }
        }

        private void homePictureBox_MouseEnter(object sender, EventArgs e)
        {
            if (Cursor != Cursors.WaitCursor)
            {
                Cursor = Cursors.Hand;
            }
        }

        private void twitterPictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                Process.Start(TwitterAddress);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void twitterPictureBox_MouseLeave(object sender, EventArgs e)
        {
            if (Cursor != Cursors.WaitCursor)
            {
                Cursor = Cursors.Default;
            }
        }

        private void twitterPictureBox_MouseEnter(object sender, EventArgs e)
        {
            if (Cursor != Cursors.WaitCursor)
            {
                Cursor = Cursors.Hand;
            }
        }

        private void hotkeyComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateHookKeyStatus();
        }

        private void hotkeyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            hotkeyComboBox.Enabled = hotkeyCheckBox.Checked;
            UpdateHookKeyStatus();
        }

        private void mousePictureBox_DoubleClick(object sender, EventArgs e)
        {
            ShowCelebrityMouse(true);
        }

        private void _autoPauseTimer_Tick(object sender, EventArgs e)
        {
            //Debug.WriteLine("_autoPauseTimer_Tick");
            var timeNow = new TimeSpan(DateTime.Now.TimeOfDay.Hours, DateTime.Now.TimeOfDay.Minutes, DateTime.Now.TimeOfDay.Seconds);

            if ((_pauseSchedules != null) && (_pauseSchedules.Contains(timeNow)))
            {
                ButtonPerformClick(ref actionButton);
                OnScheduleArrived(this, new ScheduleArrivedEventArgs(ScheduleArrivedEventArgs.ScheduleAction.Pause, timeNow));
            }
        }

        private void _autoStartTimer_Tick(object sender, EventArgs e)
        {
            //Debug.WriteLine("_autoStartTimer_Tick");
            var timeNow = new TimeSpan(DateTime.Now.TimeOfDay.Hours, DateTime.Now.TimeOfDay.Minutes, DateTime.Now.TimeOfDay.Seconds);

            if ((_startSchedules != null) && (_startSchedules.Contains(timeNow)))
            {
                ButtonPerformClick(ref actionButton);
                OnScheduleArrived(this, new ScheduleArrivedEventArgs(ScheduleArrivedEventArgs.ScheduleAction.Start, timeNow));
            }
        }

        private void executePauseScriptCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            DetermineScriptsTabControlState();
        }

        private void executeIntervalScriptCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            DetermineScriptsTabControlState();
        }

        private void executeStartScriptCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            DetermineScriptsTabControlState();
        }

        private void DetermineScriptsTabControlState()
        {
            editStartScriptButton.Enabled = executeStartScriptCheckBox.Checked;
            editIntervalScriptButton.Enabled = executeIntervalScriptCheckBox.Checked;
            editPauseScriptButton.Enabled = executePauseScriptCheckBox.Checked;
            importStartScriptButton.Enabled = executeStartScriptCheckBox.Checked;
            importIntervalScriptButton.Enabled = executeIntervalScriptCheckBox.Checked;
            importPauseScriptButton.Enabled = executePauseScriptCheckBox.Checked;
            showScriptExecutionCheckBox.Enabled = (executeStartScriptCheckBox.Checked || executeIntervalScriptCheckBox.Checked || executePauseScriptCheckBox.Checked);
            scriptLanguageComboBox.Enabled = (executeStartScriptCheckBox.Checked || executeIntervalScriptCheckBox.Checked || executePauseScriptCheckBox.Checked);
            changeScriptEditorButton.Enabled = (executeStartScriptCheckBox.Checked || executeIntervalScriptCheckBox.Checked || executePauseScriptCheckBox.Checked);
        }

        private void editPauseScriptButton_Click(object sender, EventArgs e)
        {
            EditScript(Script.Pause);
        }

        private void editIntervalScriptButton_Click(object sender, EventArgs e)
        {
            EditScript(Script.Interval);
        }

        private void editStartScriptButton_Click(object sender, EventArgs e)
        {
            EditScript(Script.Start);
        }

        private void changeScriptEditorButton_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                CheckFileExists = true,
                DefaultExt = "exe",
                Filter = "Application File (*.exe)|*.exe",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Multiselect = false,
                Title = "Script Editor Path"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _scriptEditor = ofd.FileName;
                scriptEditorLabel.Text = _scriptEditor;
            }
        }

        private void scriptEditorLabel_TextChanged(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(scriptEditorLabel.Text) && !scriptEditorLabel.Text.StartsWith("..."))
            {
                if (scriptEditorLabel.Text.Length > MaxScriptPathLength)
                {
                    scriptEditorLabel.Text = String.Format("...{0}", scriptEditorLabel.Text.Substring(scriptEditorLabel.Text.Length - MaxScriptPathLength));
                }
            }
        }

        private void ListScriptingLanguages()
        {
            _scriptingLanguages = new List<ScriptingLanguage>
            {
                new ScriptingLanguage
                {
                    Name = "PowerShell",
                    FileExtension = "ps1",
                    ScriptEngine = @"%SystemRoot%\system32\WindowsPowerShell\v1.0\powershell.exe",
                    ScriptPrefixArguments = "-File"
                },
                new ScriptingLanguage
                {
                    Name = "Batch",
                    FileExtension = "bat",
                    ScriptEngine = "%ComSpec%",
                    ScriptPrefixArguments = "/C"
                },
                new ScriptingLanguage
                {
                    Name = "VBScript",
                    FileExtension = "vbs",
                    ScriptEngine = @"%WinDir%\System32\cscript.exe",
                    ScriptPrefixArguments = String.Empty
                },
                new ScriptingLanguage
                {
                    Name = "JScript",
                    FileExtension = "js",
                    ScriptEngine = @"%WinDir%\System32\cscript.exe",
                    ScriptPrefixArguments = String.Empty
                },
                //new ScriptingLanguage
                //{
                //    Name = "Python",
                //    FileExtension = "py",
                //    ScriptEngine = "python.exe",
                //    ScriptPrefixArguments = String.Empty
                //}
            };

            scriptLanguageComboBox.Items.Clear();

            foreach (var sl in _scriptingLanguages)
            {
                scriptLanguageComboBox.Items.Add(sl.Name);
            }

            scriptLanguageComboBox.SelectedIndex = 0;
        }

        private void removeBlackoutButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in blackoutListView.SelectedItems)
            {
                blackoutListView.Items.Remove(lvi);
            }

            UpdateBlackoutSchedulesList();
        }

        private void editBlackoutButton_Click(object sender, EventArgs e)
        {
            EditSelectedBlackout();
        }

        private void addBlackoutButton_Click(object sender, EventArgs e)
        {
            var abf = new AddBlackoutForm();
            Opacity = .75;

            if (abf.ShowDialog() == DialogResult.OK)
            {
                AddBlackoutToListView(abf.Start, abf.End, -1, true);
            }

            Opacity = 1;
        }

        private void blackoutListView_DoubleClick(object sender, EventArgs e)
        {
            EditSelectedBlackout();
        }

        private void blackoutListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            editBlackoutButton.Enabled = blackoutListView.SelectedItems.Count.Equals(1);
            removeBlackoutButton.Enabled = blackoutListView.SelectedItems.Count > 0;
        }

        private void AddBlackoutToListView(TimeSpan start, TimeSpan end, int index, bool select)
        {
            ListViewItem lvi;

            if ((index > -1) && ((blackoutListView.Items.Count - 1) >= index))
            {
                lvi = blackoutListView.Items[index];
                lvi.SubItems.Clear();
            }
            else
            {
                lvi = new ListViewItem();
                blackoutListView.Items.Add(lvi);
            }

            lvi.Text = start.ToString();
            lvi.SubItems.Add(end.ToString());
            blackoutListView.SelectedItems.Clear();
            lvi.Selected = select;
            blackoutListView.Select();
            blackoutListView.Sort();
            UpdateBlackoutSchedulesList();
        }

        private void EditSelectedBlackout()
        {
            if (blackoutListView.SelectedItems.Count > 0)
            {
                TimeSpan startTs;
                TimeSpan endTs;
                TimeSpan.TryParse(blackoutListView.SelectedItems[0].SubItems[0].Text, out startTs);
                TimeSpan.TryParse(blackoutListView.SelectedItems[0].SubItems[1].Text, out endTs);
                var abf = new AddBlackoutForm(startTs, endTs);
                Opacity = .75;

                if (abf.ShowDialog() == DialogResult.OK)
                {
                    AddBlackoutToListView(abf.Start, abf.End, blackoutListView.SelectedIndices[0], true);
                }

                blackoutListView.Select();
                Opacity = 1;
            }
        }

        private void UpdateBlackoutSchedulesList()
        {
            _blackoutSchedules = new List<KeyValuePair<TimeSpan, TimeSpan>>();

            if (blackoutListView.Items.Count > 0)
            {
                foreach (ListViewItem lvi in blackoutListView.Items)
                {
                    TimeSpan startTs;
                    TimeSpan endTs;

                    if (TimeSpan.TryParse(lvi.SubItems[0].Text, out startTs) && TimeSpan.TryParse(lvi.SubItems[1].Text, out endTs))
                    {
                        _blackoutSchedules.Add(new KeyValuePair<TimeSpan, TimeSpan>(startTs, endTs));
                    }
                }
            }
        }

        private bool IsBlackoutActive(TimeSpan time)
        {
            if ((_blackoutSchedules != null) && (_blackoutSchedules.Count > 0))
            {
                foreach (var blackout in _blackoutSchedules)
                {
                    if (blackout.Key < blackout.Value)
                    {
                        if ((time >= blackout.Key) && (time < blackout.Value))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if ((time < blackout.Value) || (time >= blackout.Key))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private void GetNextBlackoutStatusChangeTime(out TimeSpan startTime, out TimeSpan endTime)
        {
            startTime = new TimeSpan();
            endTime = new TimeSpan();

            if (_blackoutSchedules.Count > 0)
            {
                startTime = _blackoutSchedules[0].Key;
                endTime = _blackoutSchedules[0].Value;

                foreach (var blackout in _blackoutSchedules)
                {
                    if ((blackout.Key > DateTime.Now.TimeOfDay) || (blackout.Value > DateTime.Now.TimeOfDay))
                    {
                        startTime = blackout.Key;
                        endTime = blackout.Value;
                        return;
                    }
                }
            }
        }

        private void scheduleListView_DoubleClick(object sender, EventArgs e)
        {
            EditSelectedSchedule();
        }

        private void scheduleListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            editScheduleButton.Enabled = scheduleListView.SelectedItems.Count.Equals(1);
            removeScheduleButton.Enabled = scheduleListView.SelectedItems.Count > 0;
        }

        private void removeScheduleButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in scheduleListView.SelectedItems)
            {
                scheduleListView.Items.Remove(lvi);
            }

            UpdateAutoScheduleLists();
        }

        private void editScheduleButton_Click(object sender, EventArgs e)
        {
            EditSelectedSchedule();
        }

        private void addScheduleButton_Click(object sender, EventArgs e)
        {
            var asf = new AddScheduleForm();
            Opacity = .75;

            if (asf.ShowDialog() == DialogResult.OK)
            {
                AddScheduleToListView(asf.Time, asf.Action, -1, true);
            }

            Opacity = 1;
        }

        private void AddScheduleToListView(TimeSpan time, string action, int index, bool select)
        {
            ListViewItem lvi;

            if ((index > -1) && ((scheduleListView.Items.Count - 1) >= index))
            {
                lvi = scheduleListView.Items[index];
                lvi.SubItems.Clear();
            }
            else
            {
                lvi = new ListViewItem();
                scheduleListView.Items.Add(lvi);
            }

            lvi.Text = time.ToString();
            lvi.SubItems.Add(action);
            scheduleListView.SelectedItems.Clear();
            lvi.Selected = select;
            scheduleListView.Select();
            scheduleListView.Sort();
            UpdateAutoScheduleLists();
        }

        private void EditSelectedSchedule()
        {
            if (scheduleListView.SelectedItems.Count > 0)
            {
                TimeSpan ts;
                TimeSpan.TryParse(scheduleListView.SelectedItems[0].SubItems[0].Text, out ts);
                var asf = new AddScheduleForm(ts, scheduleListView.SelectedItems[0].SubItems[1].Text);
                Opacity = .75;

                if (asf.ShowDialog() == DialogResult.OK)
                {
                    AddScheduleToListView(asf.Time, asf.Action, scheduleListView.SelectedIndices[0], true);
                }

                scheduleListView.Select();
                Opacity = 1;
            }
        }

        private void UpdateAutoScheduleLists()
        {
            _startSchedules = new List<TimeSpan>();
            _pauseSchedules = new List<TimeSpan>();

            if (scheduleListView.Items.Count > 0)
            {
                foreach (ListViewItem lvi in scheduleListView.Items)
                {
                    TimeSpan ts;

                    if (TimeSpan.TryParse(lvi.SubItems[0].Text, out ts))
                    {
                        switch ((ScheduleArrivedEventArgs.ScheduleAction) Enum.Parse(typeof(ScheduleArrivedEventArgs.ScheduleAction), lvi.SubItems[1].Text, true))
                        {
                            case ScheduleArrivedEventArgs.ScheduleAction.Start:
                                _startSchedules.Add(ts);
                                break;
                            case ScheduleArrivedEventArgs.ScheduleAction.Pause:
                                _pauseSchedules.Add(ts);
                                break;
                        }
                    }
                }
            }
        }

        //private void scriptsHelpPictureBox_MouseClick(object sender, MouseEventArgs e)
        //{
        //    try
        //    {
        //        Process.Start(ScriptsHelpAddress);
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex.Message);
        //    }
        //}

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

        private void EditScript(Script script)
        {
            var sl = GetScriptingLanguage(GetComboBoxSelectedItem(ref scriptLanguageComboBox).ToString());

            if (sl != null)
            {
                string scriptPath = GetScriptPath(script, sl.FileExtension);

                if (!String.IsNullOrEmpty(scriptPath) && !File.Exists(scriptPath))
                {
                    CreateEmptyScript(scriptPath);
                }

                var p = new Process
                {
                    StartInfo =
                    {
                        FileName = _scriptEditor,
                        Arguments = String.Format("\"{0}\"", scriptPath)
                    }
                };
                p.Exited += p_Exited;
                p.Start();
            }
        }

        private string GetScriptPath(Script script, string extension)
        {
            switch (script)
            {
                case Script.Start:
                    return Path.Combine(StaticCode.WorkingDirectory, String.Format("{0}.{1}", StartScriptName, extension));
                case Script.Interval:
                    return Path.Combine(StaticCode.WorkingDirectory, String.Format("{0}.{1}", IntervalScriptName, extension));
                case Script.Pause:
                    return Path.Combine(StaticCode.WorkingDirectory, String.Format("{0}.{1}", PauseScriptName, extension));
            }

            return null;
        }

        private void p_Exited(object sender, EventArgs e)
        {
            BringToFront();
        }

        private void CreateEmptyScript(string path)
        {
            var sw = new StreamWriter(path, false);
            //sw.WriteLine("' Move Mouse Custom Script");
            //sw.WriteLine("' See {0} for some useful scripting examples.", ScriptsHelpAddress);
            sw.Close();
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(ListOpenWindows);
        }

        public sealed override string Text
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

        private void UpdateHookKeyStatus()
        {
            var key = Keys.None;
            bool enabled = GetCheckBoxChecked(ref hotkeyCheckBox);
            object hookKey = GetComboBoxSelectedItem(ref hotkeyComboBox);

            if (!String.IsNullOrEmpty(hookKey?.ToString()))
            {
                key = (Keys) Enum.Parse(typeof(Keys), hookKey.ToString(), true);
            }

            var eventArgs = new HookKeyStatusChangedEventArgs(enabled, key);
            OnHookKeyStatusChanged(this, eventArgs);
        }

        //private void helpPictureBox_MouseClick(object sender, MouseEventArgs e)
        //{
        //    try
        //    {
        //        Process.Start(HelpAddress);
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex.Message);
        //    }
        //}

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

        private void appActivateCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            processComboBox.Enabled = appActivateCheckBox.Checked;
            refreshButton.Enabled = appActivateCheckBox.Checked;
        }

        private void ListOpenWindows(object stateInfo)
        {
            try
            {
                ClearComboBoxItems(ref processComboBox);
                var tag = GetComboBoxTag(ref processComboBox);

                foreach (var p in Process.GetProcesses())
                {
                    try
                    {
                        if (!String.IsNullOrEmpty(p.MainWindowTitle) && !processComboBox.Items.Contains(p.MainWindowTitle))
                        {
                            //Debug.WriteLine(p.MainWindowTitle);
                            AddComboBoxItem(ref processComboBox, p.MainWindowTitle, ((tag != null) && tag.ToString().Equals(p.MainWindowTitle)));
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void MouseForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                ShowInTaskbar = !minimiseToSystemTrayCheckBox.Checked;
            }
            else
            {
                Refresh();
            }
        }

        private void launchAtLogonCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (launchAtLogonCheckBox.Checked)
            {
                var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                key?.SetValue("Move Mouse", Application.ExecutablePath);
            }
            else
            {
                var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                key?.DeleteValue("Move Mouse");
            }
        }

        private void MouseForm_Load(object sender, EventArgs e)
        {
            if (!Directory.Exists(StaticCode.WorkingDirectory))
            {
                Directory.CreateDirectory(StaticCode.WorkingDirectory);
            }

            if (!Directory.Exists(_moveMouseTempDirectory))
            {
                Directory.CreateDirectory(_moveMouseTempDirectory);
            }

            ThreadPool.QueueUserWorkItem(CheckForUpdate);
            ThreadPool.QueueUserWorkItem(ListOpenWindows);
            ThreadPool.QueueUserWorkItem(UpdateCelebrityMiceList);
            UpdateHookKeyStatus();

            if (startOnLaunchCheckBox.Checked && !_suppressAutoStart)
            {
                actionButton.PerformClick();
            }

            #region Loop for testing blackout schedules

            //for (int i = 0; i < Convert.ToInt32(new TimeSpan(24, 0, 0).TotalSeconds); i++)
            //{
            //    var ts = new TimeSpan(0, 0, i);
            //    Debug.WriteLine(String.Format("IsBlackoutActive({0} = {1}", ts, IsBlackoutActive(ts)));
            //}

            #endregion
        }

        public void StartStopToggle()
        {
            actionButton.PerformClick();
        }

        private void UpdateCelebrityMiceList(object stateInfo)
        {
            try
            {
                _celebrityMice = new List<CelebrityMouse>();
                var miceXmlDoc = new XmlDocument();
                miceXmlDoc.Load(String.Format("{0}{1}", MiceResourceUrlPrefix, MiceXmlName));
                var mouseNodes = miceXmlDoc.SelectNodes("mice/mouse");

                if ((mouseNodes != null) && (mouseNodes.Count > 0))
                {
                    foreach (XmlNode mouseNode in mouseNodes)
                    {
                        var nameNode = mouseNode.SelectSingleNode("name");
                        var imageNameNode = mouseNode.SelectSingleNode("image_name");
                        var toolTipNode = mouseNode.SelectSingleNode("tool_tip");

                        if ((nameNode != null) && (imageNameNode != null) && (toolTipNode != null))
                        {
                            _celebrityMice.Add(new CelebrityMouse {Name = nameNode.InnerText, ImageName = imageNameNode.InnerText, ToolTip = toolTipNode.InnerText});
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
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
                SetButtonText(ref traceButton, String.Format("{0:0.0}", _traceTimeComplete.Subtract(DateTime.Now).TotalSeconds));
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

        private void resumeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            resumeNumericUpDown.Enabled = resumeCheckBox.Checked;
        }

        private void _resumeTimer_Tick(object sender, EventArgs e)
        {
            //Debug.WriteLine("_resumeTimer_Tick");
            //Debug.WriteLine(String.Format("GetLastInputTime() = {0}", GetLastInputTime()));
            //todo Something is happening after 4 seconds

            if (GetCheckBoxChecked(ref resumeCheckBox) && (GetLastInputTime() >= resumeNumericUpDown.Value))
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
            //if (!AtLeastOneActionIsEnabled())
            //{
            //    clickMouseCheckBox.Checked = true;
            //}

            DetermineActionsTabControlState();
        }

        private void moveMouseCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            //if (!AtLeastOneActionIsEnabled())
            //{
            //    moveMouseCheckBox.Checked = true;
            //}

            DetermineActionsTabControlState();
        }

        private void keystrokeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            //if (!AtLeastOneActionIsEnabled())
            //{
            //    keystrokeCheckBox.Checked = true;
            //}

            DetermineActionsTabControlState();
        }

        //private bool AtLeastOneActionIsEnabled()
        //{
        //    return (moveMouseCheckBox.Checked || clickMouseCheckBox.Checked || keystrokeCheckBox.Checked);
        //}

        private void DetermineActionsTabControlState()
        {
            stealthCheckBox.Enabled = moveMouseCheckBox.Checked;
            staticPositionCheckBox.Enabled = (clickMouseCheckBox.Checked | moveMouseCheckBox.Checked);
            xNumericUpDown.Enabled = (staticPositionCheckBox.Enabled & staticPositionCheckBox.Checked);
            yNumericUpDown.Enabled = (staticPositionCheckBox.Enabled & staticPositionCheckBox.Checked);
            traceButton.Enabled = (staticPositionCheckBox.Enabled & staticPositionCheckBox.Checked);
            keystrokeComboBox.Enabled = keystrokeCheckBox.Checked;

            if (keystrokeComboBox.SelectedIndex.Equals(-1))
            {
                keystrokeComboBox.SelectedIndex = 0;
            }
        }

        private void MouseForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            OnMoveMouseStopped();
            _mouseTimer.Stop();
            _resumeTimer.Stop();
            _autoStartTimer.Stop();
            _autoPauseTimer.Stop();
            SaveSettings();
        }

        private void LaunchScript(Script script)
        {
            var sl = GetScriptingLanguage(GetComboBoxSelectedItem(ref scriptLanguageComboBox).ToString());

            if (sl != null)
            {
                string scriptPath = null;

                switch (script)
                {
                    case Script.Start:

                        if (GetCheckBoxChecked(ref executeStartScriptCheckBox))
                        {
                            scriptPath = Path.Combine(StaticCode.WorkingDirectory, String.Format("{0}.{1}", StartScriptName, sl.FileExtension));
                        }

                        break;
                    case Script.Interval:

                        if (GetCheckBoxChecked(ref executeIntervalScriptCheckBox))
                        {
                            scriptPath = Path.Combine(StaticCode.WorkingDirectory, String.Format("{0}.{1}", IntervalScriptName, sl.FileExtension));
                        }

                        break;
                    case Script.Pause:

                        if (GetCheckBoxChecked(ref executePauseScriptCheckBox))
                        {
                            scriptPath = Path.Combine(StaticCode.WorkingDirectory, String.Format("{0}.{1}", PauseScriptName, sl.FileExtension));
                        }

                        break;
                }

                if (!String.IsNullOrEmpty(scriptPath) && File.Exists(scriptPath))
                {
                    var p = new Process
                    {
                        StartInfo =
                        {
                            FileName = Environment.ExpandEnvironmentVariables(sl.ScriptEngine),
                            Arguments = String.IsNullOrEmpty(sl.ScriptPrefixArguments) ? String.Format("\"{0}\"", scriptPath) : String.Format("{0} \"{1}\"", sl.ScriptPrefixArguments, scriptPath),
                            WindowStyle = GetCheckBoxChecked(ref showScriptExecutionCheckBox) ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden
                        }
                    };
                    p.Start();
                }
            }
        }

        private ScriptingLanguage GetScriptingLanguage(string name)
        {
            if (!String.IsNullOrEmpty(name))
            {
                foreach (var sl in _scriptingLanguages)
                {
                    if (sl.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return sl;
                    }
                }
            }

            return null;
        }

        private void UpdateCountdownProgressBar(ref ProgressBar pb, int delay, int elapsed)
        {
            if (InvokeRequired)
            {
                Invoke(new UpdateCountdownProgressBarDelegate(UpdateCountdownProgressBar), pb, delay, elapsed);
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
                Invoke(new ButtonPerformClickDelegate(ButtonPerformClick), b);
            }
            else
            {
                b.PerformClick();
            }
        }

        private void KryptonButtonPerformClick(ref KryptonButton kb)
        {
            if (InvokeRequired)
            {
                Invoke(new KryptonButtonPerformClickDelegate(KryptonButtonPerformClick), kb);
            }
            else
            {
                kb.PerformClick();
            }
        }

        private object GetComboBoxSelectedItem(ref ComboBox cb)
        {
            if (InvokeRequired)
            {
                return Invoke(new GetComboBoxSelectedItemDelegate(GetComboBoxSelectedItem), cb);
            }

            return cb.SelectedItem;
        }

        private int GetComboBoxSelectedIndex(ref ComboBox cb)
        {
            if (InvokeRequired)
            {
                return (int) Invoke(new GetComboBoxSelectedIndexDelegate(GetComboBoxSelectedIndex), cb);
            }

            return cb.SelectedIndex;
        }

        private void SetNumericUpDownValue(ref NumericUpDown nud, int value)
        {
            if (InvokeRequired)
            {
                Invoke(new SetNumericUpDownValueDelegate(SetNumericUpDownValue), nud, value);
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
                Invoke(new SetButtonTextDelegate(SetButtonText), b, text);
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
                Invoke(new SetButtonTagDelegate(SetButtonTag), b, o);
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
                return Invoke(new GetButtonTagDelegate(GetButtonTag), b);
            }

            return b.Tag;
        }

        private string GetButtonText(ref Button b)
        {
            if (InvokeRequired)
            {
                return Convert.ToString(Invoke(new GetButtonTextDelegate(GetButtonText), b));
            }

            return b.Text;
        }

        private bool GetCheckBoxChecked(ref KryptonCheckBox cb)
        {
            if (InvokeRequired)
            {
                return Convert.ToBoolean(Invoke(new GetCheckBoxCheckedDelegate(GetCheckBoxChecked), cb));
            }

            return cb.Checked;
        }

        private void AddComboBoxItem(ref ComboBox cb, string item, bool selected)
        {
            if (InvokeRequired)
            {
                Invoke(new AddComboBoxItemDelegate(AddComboBoxItem), cb, item, selected);
            }
            else
            {
                int index = cb.Items.Add(item);

                if (selected)
                {
                    cb.SelectedIndex = index;
                }
            }
        }

        private object GetComboBoxTag(ref ComboBox cb)
        {
            if (InvokeRequired)
            {
                return Invoke(new GetComboBoxTagDelegate(GetComboBoxTag), cb);
            }

            return cb.Tag;
        }

        private void ClearComboBoxItems(ref ComboBox cb)
        {
            if (InvokeRequired)
            {
                Invoke(new ClearComboBoxItemsDelegate(ClearComboBoxItems), cb);
            }
            else
            {
                cb.Items.Clear();
            }
        }

        private bool IsWindowMinimised(IntPtr handle)
        {
            if (InvokeRequired)
            {
                return Convert.ToBoolean(Invoke(new IsWindowMinimisedDelegate(IsWindowMinimised), handle));
            }

            var placement = GetPlacement(handle);
            return (placement.showCmd == ShowWindowCommands.ShowMinimized);
        }

        private void ShowCelebrityMouse(CelebrityMouse cb)
        {
            if (InvokeRequired)
            {
                Invoke(new ShowCelebrityMouseDelegate(ShowCelebrityMouse), cb);
            }
            else
            {
                if (cb != null)
                {
                    string imageLocalPath = Path.Combine(_moveMouseTempDirectory, cb.ImageName);

                    if (File.Exists(imageLocalPath))
                    {
                        mousePictureBox.Image = new Bitmap(imageLocalPath);
                        mouseTabPage.Text = cb.Name;
                    }
                }
            }
        }

        private WINDOWPLACEMENT GetPlacement(IntPtr hwnd)
        {
            var placement = new WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(placement);
            GetWindowPlacement(hwnd, ref placement);
            return placement;
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

            return (idleTime > 0) ? (idleTime / 1000) : 0;
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

        private void MoveMousePointer(Point point)
        {
            var mi = new MOUSEINPUT
            {
                dx = point.X,
                dy = point.Y,
                mouseData = 0,
                time = 0,
                dwFlags = MouseEventFlags.MOVE,
                dwExtraInfo = UIntPtr.Zero
            };
            var input = new INPUT
            {
                mi = mi,
                type = Convert.ToInt32(Win32Consts.INPUT_MOUSE)
            };
            SendInput(1, ref input, Marshal.SizeOf(input));
        }

        private void WriteSingleNodeInnerText(ref XmlDocument xmlDoc, string nodePath, string text)
        {
            var node = xmlDoc?.SelectSingleNode(nodePath);

            if (node != null)
            {
                node.InnerText = text;
            }
        }

        private string ReadSingleNodeInnerTextAsString(ref XmlDocument xmlDoc, string nodePath)
        {
            var node = xmlDoc?.SelectSingleNode(nodePath);

            if (node != null)
            {
                return node.InnerText;
            }

            return String.Empty;
        }

        private bool ReadSingleNodeInnerTextAsBoolean(ref XmlDocument xmlDoc, string nodePath)
        {
            var node = xmlDoc?.SelectSingleNode(nodePath);

            if (node != null)
            {
                bool b;

                if (Boolean.TryParse(node.InnerText, out b))
                {
                    return b;
                }
            }

            return false;
        }

        private decimal ReadSingleNodeInnerTextAsDecimal(ref XmlDocument xmlDoc, string nodePath)
        {
            var node = xmlDoc?.SelectSingleNode(nodePath);

            if (node != null)
            {
                decimal d;

                if (Decimal.TryParse(node.InnerText, out d))
                {
                    return d;
                }
            }

            return 0;
        }

        private DateTime ReadSingleNodeInnerTextAsDateTime(ref XmlDocument xmlDoc, string nodePath)
        {
            var node = xmlDoc?.SelectSingleNode(nodePath);

            if (node != null)
            {
                DateTime dt;

                if (DateTime.TryParse(node.InnerText, out dt))
                {
                    return dt;
                }
            }

            return new DateTime();
        }

        //private Int32 ReadSingleNodeInnerTextAsInt32(ref XmlDocument xmlDoc, string nodePath)
        //{
        //    var node = xmlDoc?.SelectSingleNode(nodePath);

        //    if (node != null)
        //    {
        //        Int32 i;

        //        if (Int32.TryParse(node.InnerText, out i))
        //        {
        //            return i;
        //        }
        //    }

        //    return 0;
        //}

        private string ReadSingleNodeInnerTextAsString(XmlNode parentNode, string nodePath)
        {
            var node = parentNode?.SelectSingleNode(nodePath);

            if (node != null)
            {
                return node.InnerText;
            }

            return String.Empty;
        }

        //private bool ReadSingleNodeInnerTextAsBoolean(XmlNode parentNode, string nodePath)
        //{
        //    var node = parentNode?.SelectSingleNode(nodePath);

        //    if (node != null)
        //    {
        //        bool b;

        //        if (Boolean.TryParse(node.InnerText, out b))
        //        {
        //            return b;
        //        }
        //    }

        //    return false;
        //}

        //private decimal ReadSingleNodeInnerTextAsDecimal(XmlNode parentNode, string nodePath)
        //{
        //    var node = parentNode?.SelectSingleNode(nodePath);

        //    if (node != null)
        //    {
        //        decimal d;

        //        if (Decimal.TryParse(node.InnerText, out d))
        //        {
        //            return d;
        //        }
        //    }

        //    return 0;
        //}

        private DateTime ReadSingleNodeInnerTextAsDateTime(XmlNode parentNode, string nodePath)
        {
            var node = parentNode?.SelectSingleNode(nodePath);

            if (node != null)
            {
                DateTime dt;

                if (DateTime.TryParse(node.InnerText, out dt))
                {
                    return dt;
                }
            }

            return new DateTime();
        }

        private Int32 ReadSingleNodeInnerTextAsInt32(XmlNode parentNode, string nodePath)
        {
            var node = parentNode?.SelectSingleNode(nodePath);

            if (node != null)
            {
                Int32 i;

                if (Int32.TryParse(node.InnerText, out i))
                {
                    return i;
                }
            }

            return 0;
        }

        private void ReadSettings()
        {
            if (InvokeRequired)
            {
                Invoke(new ZeroParameterDelegate(ReadSettings));
            }
            else
            {
                try
                {
                    if (File.Exists(Path.Combine(StaticCode.WorkingDirectory, MoveMouseXmlName)))
                    {
                        var settingsXmlDoc = new XmlDocument();
                        settingsXmlDoc.Load(Path.Combine(StaticCode.WorkingDirectory, MoveMouseXmlName));
                        delayNumericUpDown.Value = ReadSingleNodeInnerTextAsDecimal(ref settingsXmlDoc, "settings/second_delay");
                        moveMouseCheckBox.Checked = ReadSingleNodeInnerTextAsBoolean(ref settingsXmlDoc, "settings/move_mouse_pointer");
                        stealthCheckBox.Checked = ReadSingleNodeInnerTextAsBoolean(ref settingsXmlDoc, "settings/stealth_mode");
                        staticPositionCheckBox.Checked = ReadSingleNodeInnerTextAsBoolean(ref settingsXmlDoc, "settings/enable_static_position");
                        xNumericUpDown.Value = ReadSingleNodeInnerTextAsDecimal(ref settingsXmlDoc, "settings/x_static_position");
                        yNumericUpDown.Value = ReadSingleNodeInnerTextAsDecimal(ref settingsXmlDoc, "settings/y_static_position");
                        clickMouseCheckBox.Checked = ReadSingleNodeInnerTextAsBoolean(ref settingsXmlDoc, "settings/click_left_mouse_button");
                        keystrokeCheckBox.Checked = ReadSingleNodeInnerTextAsBoolean(ref settingsXmlDoc, "settings/send_keystroke");
                        keystrokeComboBox.SelectedItem = ReadSingleNodeInnerTextAsString(ref settingsXmlDoc, "settings/keystroke");
                        autoPauseCheckBox.Checked = ReadSingleNodeInnerTextAsBoolean(ref settingsXmlDoc, "settings/pause_when_mouse_moved");
                        resumeCheckBox.Checked = ReadSingleNodeInnerTextAsBoolean(ref settingsXmlDoc, "settings/automatically_resume");
                        resumeNumericUpDown.Value = ReadSingleNodeInnerTextAsDecimal(ref settingsXmlDoc, "settings/resume_seconds");
                        disableOnBatteryCheckBox.Checked = ReadSingleNodeInnerTextAsBoolean(ref settingsXmlDoc, "settings/disable_on_battery");
                        hotkeyCheckBox.Checked = ReadSingleNodeInnerTextAsBoolean(ref settingsXmlDoc, "settings/enable_hotkey");
                        hotkeyComboBox.SelectedItem = ReadSingleNodeInnerTextAsString(ref settingsXmlDoc, "settings/hotkey");
                        startOnLaunchCheckBox.Checked = ReadSingleNodeInnerTextAsBoolean(ref settingsXmlDoc, "settings/automatically_start_on_launch");
                        launchAtLogonCheckBox.Checked = ReadSingleNodeInnerTextAsBoolean(ref settingsXmlDoc, "settings/automatically_launch_on_logon");
                        minimiseOnPauseCheckBox.Checked = ReadSingleNodeInnerTextAsBoolean(ref settingsXmlDoc, "settings/minimise_on_pause");
                        minimiseOnStartCheckBox.Checked = ReadSingleNodeInnerTextAsBoolean(ref settingsXmlDoc, "settings/minimise_on_start");
                        minimiseToSystemTrayCheckBox.Checked = ReadSingleNodeInnerTextAsBoolean(ref settingsXmlDoc, "settings/minimise_to_system_tray");
                        appActivateCheckBox.Checked = ReadSingleNodeInnerTextAsBoolean(ref settingsXmlDoc, "settings/activate_application");

                        if (!String.IsNullOrEmpty(ReadSingleNodeInnerTextAsString(ref settingsXmlDoc, "settings/activate_application_title")))
                        {
                            processComboBox.Tag = ReadSingleNodeInnerTextAsString(ref settingsXmlDoc, "settings/activate_application_title");
                        }

                        _lastUpdateCheck = ReadSingleNodeInnerTextAsDateTime(ref settingsXmlDoc, "settings/last_update_check");
                        MinimiseToSystemTrayWarningShown = ReadSingleNodeInnerTextAsBoolean(ref settingsXmlDoc, "settings/system_tray_warning_shown");
                        executeStartScriptCheckBox.Checked = ReadSingleNodeInnerTextAsBoolean(ref settingsXmlDoc, "settings/execute_start_script");
                        executeIntervalScriptCheckBox.Checked = ReadSingleNodeInnerTextAsBoolean(ref settingsXmlDoc, "settings/execute_interval_script");
                        executePauseScriptCheckBox.Checked = ReadSingleNodeInnerTextAsBoolean(ref settingsXmlDoc, "settings/execute_pause_script");
                        showScriptExecutionCheckBox.Checked = ReadSingleNodeInnerTextAsBoolean(ref settingsXmlDoc, "settings/show_script_execution");
                        scriptLanguageComboBox.SelectedItem = ReadSingleNodeInnerTextAsString(ref settingsXmlDoc, "settings/script_language");
                        string scriptEditor = ReadSingleNodeInnerTextAsString(ref settingsXmlDoc, "settings/script_editor");
                        scriptEditorLabel.Text = String.IsNullOrEmpty(scriptEditor) ? _scriptEditor : scriptEditor;
                        var scheduleNodes = settingsXmlDoc.SelectNodes("settings/schedules/schedule");
                        var blackoutNodes = settingsXmlDoc.SelectNodes("settings/blackouts/blackout");
                        scheduleListView.Items.Clear();
                        blackoutListView.Items.Clear();

                        if ((scheduleNodes != null) && (scheduleNodes.Count > 0))
                        {
                            foreach (XmlNode scheduleNode in scheduleNodes)
                            {
                                TimeSpan ts;
                                var timeNode = scheduleNode.SelectSingleNode("time");
                                var actionNode = scheduleNode.SelectSingleNode("action");

                                if ((timeNode != null) && (actionNode != null) && TimeSpan.TryParse(timeNode.InnerText, out ts))
                                {
                                    AddScheduleToListView(ts, actionNode.InnerText, -1, false);
                                }
                            }
                        }

                        if ((blackoutNodes != null) && (blackoutNodes.Count > 0))
                        {
                            foreach (XmlNode blackoutNode in blackoutNodes)
                            {
                                TimeSpan startTs;
                                TimeSpan endTs;
                                var startNode = blackoutNode.SelectSingleNode("start");
                                var endNode = blackoutNode.SelectSingleNode("end");

                                if ((startNode != null) && (endNode != null) && TimeSpan.TryParse(startNode.InnerText, out startTs) && TimeSpan.TryParse(endNode.InnerText, out endTs))
                                {
                                    AddBlackoutToListView(startTs, endTs, -1, false);
                                }
                            }
                        }

                        ReadLegacySettings(ref settingsXmlDoc);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        private void ReadLegacySettings(ref XmlDocument xmlDoc)
        {
            try
            {
                if (ReadSingleNodeInnerTextAsBoolean(ref xmlDoc, "settings/blackout_schedule_enabled"))
                {
                    string start = null;
                    string end = null;
                    TimeSpan startTs;
                    TimeSpan endTs;

                    switch (ReadSingleNodeInnerTextAsString(ref xmlDoc, "settings/blackout_schedule_scope"))
                    {
                        case "outside":
                            start = ReadSingleNodeInnerTextAsString(ref xmlDoc, "settings/blackout_schedule_end");
                            end = ReadSingleNodeInnerTextAsString(ref xmlDoc, "settings/blackout_schedule_start");
                            break;
                        case "inside":
                            start = ReadSingleNodeInnerTextAsString(ref xmlDoc, "settings/blackout_schedule_start");
                            end = ReadSingleNodeInnerTextAsString(ref xmlDoc, "settings/blackout_schedule_end");
                            break;
                    }

                    if (!String.IsNullOrEmpty(start) && !String.IsNullOrEmpty(end) && TimeSpan.TryParse(start, out startTs) && TimeSpan.TryParse(end, out endTs))
                    {
                        AddBlackoutToListView(startTs, endTs, -1, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            try
            {
                if (ReadSingleNodeInnerTextAsBoolean(ref xmlDoc, "settings/enable_custom_scripts"))
                {
                    scriptLanguageComboBox.SelectedItem = "VBScript";
                    executeStartScriptCheckBox.Checked = File.Exists(Path.Combine(StaticCode.WorkingDirectory, String.Format("{0}.vbs", StartScriptName)));
                    executeIntervalScriptCheckBox.Checked = File.Exists(Path.Combine(StaticCode.WorkingDirectory, String.Format("{0}.vbs", IntervalScriptName)));
                    executePauseScriptCheckBox.Checked = File.Exists(Path.Combine(StaticCode.WorkingDirectory, String.Format("{0}.vbs", PauseScriptName)));
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
                settingsXmlDoc.LoadXml("<settings><second_delay /><move_mouse_pointer /><stealth_mode /><enable_static_position /><x_static_position /><y_static_position /><click_left_mouse_button /><send_keystroke /><keystroke /><pause_when_mouse_moved /><automatically_resume /><resume_seconds /><disable_on_battery /><enable_hotkey /><hotkey /><automatically_start_on_launch /><automatically_launch_on_logon /><minimise_on_pause /><minimise_on_start /><minimise_to_system_tray /><activate_application /><activate_application_title /><last_update_check /><system_tray_warning_shown /><execute_start_script /><execute_interval_script /><execute_pause_script /><show_script_execution /><script_language /><script_editor /><schedules /><blackouts /></settings>");
                WriteSingleNodeInnerText(ref settingsXmlDoc, "settings/second_delay", Convert.ToDecimal(delayNumericUpDown.Value).ToString(CultureInfo.InvariantCulture));
                WriteSingleNodeInnerText(ref settingsXmlDoc, "settings/move_mouse_pointer", moveMouseCheckBox.Checked.ToString());
                WriteSingleNodeInnerText(ref settingsXmlDoc, "settings/stealth_mode", stealthCheckBox.Checked.ToString());
                WriteSingleNodeInnerText(ref settingsXmlDoc, "settings/enable_static_position", staticPositionCheckBox.Checked.ToString());
                WriteSingleNodeInnerText(ref settingsXmlDoc, "settings/x_static_position", Convert.ToDecimal(xNumericUpDown.Value).ToString(CultureInfo.InvariantCulture));
                WriteSingleNodeInnerText(ref settingsXmlDoc, "settings/y_static_position", Convert.ToDecimal(yNumericUpDown.Value).ToString(CultureInfo.InvariantCulture));
                WriteSingleNodeInnerText(ref settingsXmlDoc, "settings/click_left_mouse_button", clickMouseCheckBox.Checked.ToString());
                WriteSingleNodeInnerText(ref settingsXmlDoc, "settings/send_keystroke", keystrokeCheckBox.Checked.ToString());
                WriteSingleNodeInnerText(ref settingsXmlDoc, "settings/keystroke", keystrokeComboBox.SelectedItem?.ToString() ?? String.Empty);
                WriteSingleNodeInnerText(ref settingsXmlDoc, "settings/pause_when_mouse_moved", autoPauseCheckBox.Checked.ToString());
                WriteSingleNodeInnerText(ref settingsXmlDoc, "settings/automatically_resume", resumeCheckBox.Checked.ToString());
                WriteSingleNodeInnerText(ref settingsXmlDoc, "settings/resume_seconds", Convert.ToDecimal(resumeNumericUpDown.Value).ToString(CultureInfo.InvariantCulture));
                WriteSingleNodeInnerText(ref settingsXmlDoc, "settings/disable_on_battery", disableOnBatteryCheckBox.Checked.ToString());
                WriteSingleNodeInnerText(ref settingsXmlDoc, "settings/enable_hotkey", hotkeyCheckBox.Checked.ToString());
                WriteSingleNodeInnerText(ref settingsXmlDoc, "settings/hotkey", hotkeyComboBox.SelectedItem?.ToString() ?? String.Empty);
                WriteSingleNodeInnerText(ref settingsXmlDoc, "settings/automatically_start_on_launch", startOnLaunchCheckBox.Checked.ToString());
                WriteSingleNodeInnerText(ref settingsXmlDoc, "settings/automatically_launch_on_logon", launchAtLogonCheckBox.Checked.ToString());
                WriteSingleNodeInnerText(ref settingsXmlDoc, "settings/minimise_on_pause", minimiseOnPauseCheckBox.Checked.ToString());
                WriteSingleNodeInnerText(ref settingsXmlDoc, "settings/minimise_on_start", minimiseOnStartCheckBox.Checked.ToString());
                WriteSingleNodeInnerText(ref settingsXmlDoc, "settings/minimise_to_system_tray", minimiseToSystemTrayCheckBox.Checked.ToString());
                WriteSingleNodeInnerText(ref settingsXmlDoc, "settings/activate_application", appActivateCheckBox.Checked.ToString());
                WriteSingleNodeInnerText(ref settingsXmlDoc, "settings/activate_application_title", processComboBox.SelectedItem?.ToString() ?? String.Empty);
                WriteSingleNodeInnerText(ref settingsXmlDoc, "settings/last_update_check", _lastUpdateCheck.ToString("yyyy-MMM-dd HH:mm:ss"));
                WriteSingleNodeInnerText(ref settingsXmlDoc, "settings/system_tray_warning_shown", "True");
                WriteSingleNodeInnerText(ref settingsXmlDoc, "settings/execute_start_script", executeStartScriptCheckBox.Checked.ToString());
                WriteSingleNodeInnerText(ref settingsXmlDoc, "settings/execute_interval_script", executeIntervalScriptCheckBox.Checked.ToString());
                WriteSingleNodeInnerText(ref settingsXmlDoc, "settings/execute_pause_script", executePauseScriptCheckBox.Checked.ToString());
                WriteSingleNodeInnerText(ref settingsXmlDoc, "settings/show_script_execution", showScriptExecutionCheckBox.Checked.ToString());
                WriteSingleNodeInnerText(ref settingsXmlDoc, "settings/script_language", scriptLanguageComboBox.Text);
                WriteSingleNodeInnerText(ref settingsXmlDoc, "settings/script_editor", _scriptEditor);
                var schedulesNode = settingsXmlDoc.SelectSingleNode("settings/schedules");
                var blackoutsNode = settingsXmlDoc.SelectSingleNode("settings/blackouts");

                if ((schedulesNode != null) && (scheduleListView.Items.Count > 0))
                {
                    foreach (ListViewItem lvi in scheduleListView.Items)
                    {
                        var scheduleNode = settingsXmlDoc.CreateElement("schedule");
                        var timeNode = settingsXmlDoc.CreateElement("time");
                        var actionNode = settingsXmlDoc.CreateElement("action");
                        timeNode.InnerText = lvi.SubItems[0].Text;
                        actionNode.InnerText = lvi.SubItems[1].Text;
                        scheduleNode.AppendChild(timeNode);
                        scheduleNode.AppendChild(actionNode);
                        schedulesNode.AppendChild(scheduleNode);
                    }
                }

                if ((blackoutsNode != null) && (blackoutListView.Items.Count > 0))
                {
                    foreach (ListViewItem lvi in blackoutListView.Items)
                    {
                        var blackoutNode = settingsXmlDoc.CreateElement("blackout");
                        var startNode = settingsXmlDoc.CreateElement("start");
                        var endNode = settingsXmlDoc.CreateElement("end");
                        startNode.InnerText = lvi.SubItems[0].Text;
                        endNode.InnerText = lvi.SubItems[1].Text;
                        blackoutNode.AppendChild(startNode);
                        blackoutNode.AppendChild(endNode);
                        blackoutsNode.AppendChild(blackoutNode);
                    }
                }

                settingsXmlDoc.Save(Path.Combine(StaticCode.WorkingDirectory, MoveMouseXmlName));
                processComboBox.Tag = processComboBox.Text;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void CheckForUpdate(object stateInfo)
        {
            try
            {
                if (_lastUpdateCheck.Add(_waitBetweenUpdateChecks) < DateTime.Now)
                {
                    _lastUpdateCheck = DateTime.Now;
                    var versionXmlDoc = new XmlDocument();
                    versionXmlDoc.Load(UpdateXmlUrl);
                    var versionNodes = versionXmlDoc.SelectNodes("versions/version");

                    if ((versionNodes != null) && (versionNodes.Count > 0))
                    {
                        foreach (XmlNode versionNode in versionNodes)
                        {
                            var wmiFilter = ReadSingleNodeInnerTextAsString(versionNode, "wmi_filter");

                            if (String.IsNullOrEmpty(wmiFilter) || MatchWmiFilter(wmiFilter))
                            {
                                var availableVersion = new Version(ReadSingleNodeInnerTextAsInt32(versionNode, "major"), ReadSingleNodeInnerTextAsInt32(versionNode, "minor"), ReadSingleNodeInnerTextAsInt32(versionNode, "build"));

                                if (availableVersion > Assembly.GetExecutingAssembly().GetName().Version)
                                {
                                    var released = ReadSingleNodeInnerTextAsDateTime(versionNode, "released_date");
                                    var advertised = ReadSingleNodeInnerTextAsDateTime(versionNode, "advertised_date");
                                    var downloadUrl = ReadSingleNodeInnerTextAsString(versionNode, "download_url");
                                    var features = new List<string>();
                                    var fixes = new List<string>();
                                    var featureNodes = versionNode.SelectNodes("features/feature");
                                    var fixNodes = versionNode.SelectNodes("fixes/fix");

                                    if ((featureNodes != null) && (featureNodes.Count > 0))
                                    {
                                        foreach (XmlNode featureNode in featureNodes)
                                        {
                                            features.Add(featureNode.InnerText);
                                        }
                                    }

                                    if ((fixNodes != null) && (fixNodes.Count > 0))
                                    {
                                        foreach (XmlNode fixNode in fixNodes)
                                        {
                                            fixes.Add(fixNode.InnerText);
                                        }
                                    }

                                    if (advertised < DateTime.Now)
                                    {
                                        OnNewVersionAvailable(this, new NewVersionAvailableEventArgs(availableVersion, released, advertised, features.ToArray(), fixes.ToArray(), downloadUrl));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void actionButton_Click(object sender, EventArgs e)
        {
            switch (actionButton.Text)
            {
                case "Pause":
                    OnMoveMouseStopped();
                    _mouseTimer.Stop();
                    _autoPauseTimer.Stop();
                    _resumeTimer.Start();
                    _autoStartTimer.Start();
                    _easterEggActive = false;
                    ResetMousePicture();
                    actionButton.Text = "Start";
                    countdownProgressBar.Value = 0;
                    optionsTabControl.Enabled = true;
                    Opacity = 1.0;

                    if (minimiseOnPauseCheckBox.Checked)
                    {
                        WindowState = FormWindowState.Minimized;
                    }

                    ReadSettings();

                    if (!IsBlackoutActive(DateTime.Now.TimeOfDay))
                    {
                        LaunchScript(Script.Pause);
                    }

                    break;
                default:
                    _mouseTimerTicks = 0;
                    _mmStartTime = DateTime.Now;
                    _blackoutStatus = BlackoutStatusChangedEventArgs.BlackoutStatus.Inactive;
                    _powerLineStatus = PowerLineStatusChangedEventArgs.PowerLineStatus.Online;
                    _easterEggActive = new Random().Next(1, 100).Equals(31);
                    ResetMousePicture();
                    RaisePowerShellExecutionPolicyWarning();

                    if (!IsBlackoutActive(DateTime.Now.TimeOfDay))
                    {
                        LaunchScript(Script.Start);
                        MoveMouseToStaticPosition();
                        ActivateApplication();
                        WindowState = minimiseOnStartCheckBox.Checked ? FormWindowState.Minimized : FormWindowState.Normal;
                    }

                    _resumeTimer.Stop();
                    _autoStartTimer.Stop();
                    OnMoveMouseStarted();
                    _mouseTimer.Start();
                    _autoPauseTimer.Start();
                    actionButton.Text = "Pause";
                    optionsTabControl.SelectedTab = mouseTabPage;
                    optionsTabControl.Enabled = false;
                    Opacity = .75;
                    SaveSettings();
                    break;
            }
        }

        private void RaisePowerShellExecutionPolicyWarning()
        {
            var sl = GetScriptingLanguage(GetComboBoxSelectedItem(ref scriptLanguageComboBox).ToString());

            if ((sl != null) && sl.Name.Equals("PowerShell", StringComparison.CurrentCultureIgnoreCase) && (GetCheckBoxChecked(ref executeStartScriptCheckBox) || GetCheckBoxChecked(ref executeIntervalScriptCheckBox) || GetCheckBoxChecked(ref executePauseScriptCheckBox)) && GetCurrentPsExecutionPolicy().Equals(PowerShellExecutionPolicy.Restricted))
            {
                OnPowerShellexecutionPolicyWarning(this);
            }
        }

        private void _mouseTimer_Tick(object sender, EventArgs e)
        {
            //Debug.WriteLine("_mouseTimer_Tick");

            if (!IsBlackoutActive(DateTime.Now.TimeOfDay) && (!GetCheckBoxChecked(ref disableOnBatteryCheckBox) || !IsRunningOnBattery()))
            {
                UpdateCountdownProgressBar(ref countdownProgressBar, Convert.ToInt32(delayNumericUpDown.Value), _mouseTimerTicks);
                _mouseTimerTicks++;
                AutoPauseIfMouseHasMoved();

                if (_blackoutStatus == BlackoutStatusChangedEventArgs.BlackoutStatus.Active)
                {
                    _blackoutStatus = BlackoutStatusChangedEventArgs.BlackoutStatus.Inactive;
                    TimeSpan startTime;
                    TimeSpan endTime;
                    GetNextBlackoutStatusChangeTime(out startTime, out endTime);
                    OnBlackoutStatusChanged(this, new BlackoutStatusChangedEventArgs(_blackoutStatus, startTime, endTime));
                    OnMoveMouseStarted();
                }

                if (_powerLineStatus == PowerLineStatusChangedEventArgs.PowerLineStatus.Offline)
                {
                    _powerLineStatus = PowerLineStatusChangedEventArgs.PowerLineStatus.Online;
                    OnPowerLineStatusChanged(this, new PowerLineStatusChangedEventArgs(_powerLineStatus));
                    OnMoveMouseStarted();
                }

                if (_mouseTimerTicks > Convert.ToInt32(delayNumericUpDown.Value))
                {
                    ReadSettings();
                    LaunchScript(Script.Interval);
                    ShowCelebrityMouse(_easterEggActive);
                    SendKeystroke();
                    ClickMouse();
                    MoveMouse();
                    _mouseTimerTicks = 0;
                }
            }
            else
            {
                if (IsBlackoutActive(DateTime.Now.TimeOfDay) && (_blackoutStatus == BlackoutStatusChangedEventArgs.BlackoutStatus.Inactive))
                {
                    _blackoutStatus = BlackoutStatusChangedEventArgs.BlackoutStatus.Active;
                    TimeSpan startTime;
                    TimeSpan endTime;
                    GetNextBlackoutStatusChangeTime(out startTime, out endTime);
                    OnBlackoutStatusChanged(this, new BlackoutStatusChangedEventArgs(_blackoutStatus, startTime, endTime));
                    OnMoveMousePaused();
                }

                if (GetCheckBoxChecked(ref disableOnBatteryCheckBox) && IsRunningOnBattery() && (_powerLineStatus == PowerLineStatusChangedEventArgs.PowerLineStatus.Online))
                {
                    _powerLineStatus = PowerLineStatusChangedEventArgs.PowerLineStatus.Offline;
                    OnPowerLineStatusChanged(this, new PowerLineStatusChangedEventArgs(_powerLineStatus));
                    OnMoveMousePaused();
                }
            }
        }

        private void ClickMouse()
        {
            if (GetCheckBoxChecked(ref clickMouseCheckBox))
            {
                mouse_event((int) MouseEventFlags.LEFTDOWN, 0, 0, 0, 0);
                mouse_event((int) MouseEventFlags.LEFTUP, 0, 0, 0, 0);
            }
        }

        private void MoveMouse()
        {
            if (GetCheckBoxChecked(ref moveMouseCheckBox))
            {
                if (!GetCheckBoxChecked(ref stealthCheckBox))
                {
                    const int mouseMoveLoopSleep = 1;
                    const int mouseSpeed = 1;
                    const int moveSquareSize = 10;
                    var cursorStartPosition = Cursor.Position;

                    for (int i = 0; i < moveSquareSize; i += mouseSpeed)
                    {
                        MoveMousePointer(new Point(1, 0));
                        Thread.Sleep(mouseMoveLoopSleep);
                    }

                    for (int i = 0; i < moveSquareSize; i += mouseSpeed)
                    {
                        MoveMousePointer(new Point(0, 1));
                        Thread.Sleep(mouseMoveLoopSleep);
                    }

                    for (int i = 0; i < moveSquareSize; i += mouseSpeed)
                    {
                        MoveMousePointer(new Point(-1, 0));
                        Thread.Sleep(mouseMoveLoopSleep);
                    }

                    for (int i = 0; i < moveSquareSize; i += mouseSpeed)
                    {
                        MoveMousePointer(new Point(0, -1));
                        Thread.Sleep(mouseMoveLoopSleep);
                    }

                    Cursor.Position = cursorStartPosition;
                }
                else
                {
                    MoveMousePointer(new Point(0, 0));
                }
            }
        }

        private void MoveMouseToStaticPosition()
        {
            if (GetCheckBoxChecked(ref staticPositionCheckBox))
            {
                Cursor.Position = new Point(Convert.ToInt32(xNumericUpDown.Value), Convert.ToInt32(yNumericUpDown.Value));
            }
        }

        private void AutoPauseIfMouseHasMoved()
        {
            if (GetCheckBoxChecked(ref autoPauseCheckBox) && (_mmStartTime.Add(_waitUntilAutoMoveDetect) < DateTime.Now) && (_startingMousePoint != Cursor.Position))
            {
                ButtonPerformClick(ref actionButton);
            }
            else
            {
                _startingMousePoint = Cursor.Position;
            }
        }

        private void SendKeystroke()
        {
            if (GetCheckBoxChecked(ref keystrokeCheckBox) && (GetComboBoxSelectedIndex(ref keystrokeComboBox) > -1))
            {
                SendKeys.SendWait(GetComboBoxSelectedItem(ref keystrokeComboBox).ToString());
            }
        }

        private void ActivateApplication()
        {
            if (GetCheckBoxChecked(ref appActivateCheckBox) && (GetComboBoxSelectedIndex(ref processComboBox) > -1))
            {
                try
                {
                    IntPtr handle = FindWindow(null, GetComboBoxSelectedItem(ref processComboBox).ToString());

                    if (handle != IntPtr.Zero)
                    {
                        if (IsWindowMinimised(handle))
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
        }

        private void ShowCelebrityMouse(bool ignoreEasterEggState)
        {
            try
            {
                if ((ignoreEasterEggState || _easterEggActive) && (_celebrityMice != null) && (_celebrityMice.Count > 0) && Directory.Exists(_moveMouseTempDirectory))
                {
                    ThreadPool.QueueUserWorkItem(ShowCelebrityMouseThread);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void ShowCelebrityMouseThread(object stateInfo)
        {
            try
            {
                var mouse = _celebrityMice[new Random().Next(0, (_celebrityMice.Count - 1))];
                string imageLocalPath = Path.Combine(_moveMouseTempDirectory, mouse.ImageName);

                if (!File.Exists(imageLocalPath))
                {
                    string imageUrl = String.Format("{0}{1}", MiceResourceUrlPrefix, mouse.ImageName);
                    //Debug.WriteLine(imageUrl);
                    var wc = new WebClient();
                    wc.DownloadFile(imageUrl, imageLocalPath);
                }

                ShowCelebrityMouse(mouse);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void ResetMousePicture()
        {
            if (InvokeRequired)
            {
                Invoke(new ZeroParameterDelegate(ResetMousePicture), new object[] { });
            }
            else
            {
                mousePictureBox.Image = _easterEggActive ? Properties.Resources.EasterEgg_Image : Properties.Resources.Mouse_Image;
                mouseTabPage.Text = _easterEggActive ? "Easter Egg" : "Mouse";
            }
        }

        private bool IsRunningOnBattery()
        {
            return SystemInformation.PowerStatus.PowerLineStatus.Equals(PowerLineStatus.Offline);
        }

        private bool IsWindows10()
        {
            return MatchWmiFilter("SELECT * FROM Win32_OperatingSystem WHERE Version LIKE '10.%'");
        }

        private bool MatchWmiFilter(string wql)
        {
            try
            {
                var mos = new ManagementObjectSearcher(@"\\.\root\CIMv2", wql);
                var moc = mos.Get();
                return (moc.Count > 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return false;
        }

        private PowerShellExecutionPolicy GetCurrentPsExecutionPolicy()
        {
            var psKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\PowerShell\1\ShellIds\Microsoft.PowerShell");
            var executionPolicy = psKey?.GetValue("ExecutionPolicy");

            if (executionPolicy != null)
            {
                return (PowerShellExecutionPolicy) Enum.Parse(typeof(PowerShellExecutionPolicy), executionPolicy.ToString(), true);
            }

            return PowerShellExecutionPolicy.Restricted;
        }

        protected void OnBlackoutStatusChanged(object sender, BlackoutStatusChangedEventArgs e)
        {
            BlackoutStatusChanged?.Invoke(sender, e);
        }

        protected void OnNewVersionAvailable(object sender, NewVersionAvailableEventArgs e)
        {
            NewVersionAvailable?.Invoke(sender, e);
        }

        protected void OnScheduleArrived(object sender, ScheduleArrivedEventArgs e)
        {
            ScheduleArrived?.Invoke(sender, e);
        }

        protected void OnPowerLineStatusChanged(object sender, PowerLineStatusChangedEventArgs e)
        {
            PowerLineStatusChanged?.Invoke(sender, e);
        }

        protected void OnPowerShellexecutionPolicyWarning(object sender)
        {
            PowerShellexecutionPolicyWarning?.Invoke(sender);
        }

        protected void OnHookKeyStatusChanged(object sender, HookKeyStatusChangedEventArgs e)
        {
            HookKeyStatusChanged?.Invoke(sender, e);
        }

        protected void OnMoveMouseStarted()
        {
            MoveMouseStarted?.Invoke();
        }

        protected void OnMoveMousePaused()
        {
            MoveMousePaused?.Invoke();
        }

        protected void OnMoveMouseStopped()
        {
            MoveMouseStopped?.Invoke();
        }
    }
}