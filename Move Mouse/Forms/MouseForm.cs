using Ellanet.Classes;
using Ellanet.Events;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace Ellanet.Forms
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
        private readonly string _moveMouseWorkingDirectory = Environment.ExpandEnvironmentVariables(@"%APPDATA%\Ellanet\Move Mouse");
        private readonly bool _suppressAutoStart;
        private DateTime _mmStartTime;
        private Point _startingMousePoint;
        private DateTime _traceTimeComplete = DateTime.MinValue;
        private Thread _traceMouseThread;
        private BlackoutStatusChangeEventArgs.BlackoutStatus _blackoutStatus = BlackoutStatusChangeEventArgs.BlackoutStatus.Inactive;
        private DateTime _lastUpdateCheck = DateTime.MinValue;
        private string _scriptEditor = Path.Combine(Environment.ExpandEnvironmentVariables("%WINDIR%"), @"System32\notepad.exe");
        private List<ScriptingLanguage> _scriptingLanguages;
        private int _mouseTimerTicks;
        private List<KeyValuePair<TimeSpan, TimeSpan>> _blackoutSchedules;

        private delegate void UpdateCountdownProgressBarDelegate(ref ProgressBar pb, int delay, int elapsed);

        private delegate void ButtonPerformClickDelegate(ref Button b);

        private delegate object GetComboBoxSelectedItemDelegate(ref ComboBox cb);

        private delegate int GetComboBoxSelectedIndexDelegate(ref ComboBox cb);

        private delegate object GetComboBoxTagDelegate(ref ComboBox cb);

        private delegate void SetNumericUpDownValueDelegate(ref NumericUpDown nud, int value);

        private delegate void SetButtonTextDelegate(ref Button b, string text);

        private delegate void SetButtonTagDelegate(ref Button b, object o);

        private delegate object GetButtonTagDelegate(ref Button b);

        private delegate string GetButtonTextDelegate(ref Button b);

        private delegate bool GetCheckBoxCheckedDelegate(ref CheckBox cb);

        private delegate void AddComboBoxItemDelegate(ref ComboBox cb, string item, bool selected);

        public delegate void BlackoutStatusChangeHandler(object sender, BlackoutStatusChangeEventArgs e);

        public delegate void NewVersionAvailableHandler(object sender, NewVersionAvailableEventArgs e);

        public delegate void ClearComboBoxItemsDelegate(ref ComboBox cb);

        private delegate bool IsWindowMinimisedDelegate(IntPtr handle);

        public event BlackoutStatusChangeHandler BlackoutStatusChange;
        public event NewVersionAvailableHandler NewVersionAvailable;

        public bool MinimiseToSystemTrayWarningShown { get; private set; }

        private enum Script
        {
            Start,
            Interval,
            Pause
        }

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
            executeStartScriptCheckBox.CheckedChanged += executeStartScriptCheckBox_CheckedChanged;
            executeIntervalScriptCheckBox.CheckedChanged += executeIntervalScriptCheckBox_CheckedChanged;
            executePauseScriptCheckBox.CheckedChanged += executePauseScriptCheckBox_CheckedChanged;
            scriptEditorLabel.TextChanged += scriptEditorLabel_TextChanged;
            ListScriptingLanguages();
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
            //todo Reduce these to test load
            _mouseTimer.Interval = 1000;
            _mouseTimer.Tick += _mouseTimer_Tick;
            _resumeTimer.Interval = 1000;
            _resumeTimer.Tick += _resumeTimer_Tick;
            _autoStartTimer.Interval = 1000;
            _autoStartTimer.Tick += _autoStartTimer_Tick;
            _autoPauseTimer.Interval = 1000;
            _autoPauseTimer.Tick += _autoPauseTimer_Tick;
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
            refreshButton.Click += refreshButton_Click;
            scriptsHelpPictureBox.MouseEnter += scriptsHelpPictureBox_MouseEnter;
            scriptsHelpPictureBox.MouseLeave += scriptsHelpPictureBox_MouseLeave;
            scriptsHelpPictureBox.MouseClick += scriptsHelpPictureBox_MouseClick;
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
            scheduleListView.SelectedIndexChanged += scheduleListView_SelectedIndexChanged;
            scheduleListView.DoubleClick += scheduleListView_DoubleClick;
            blackoutListView.SelectedIndexChanged += blackoutListView_SelectedIndexChanged;
            blackoutListView.DoubleClick += blackoutListView_DoubleClick;
            SetButtonTag(ref traceButton, GetButtonText(ref traceButton));
        }

        private void _autoPauseTimer_Tick(object sender, EventArgs e)
        {
            //todo Needs to respect blackout
            Debug.WriteLine("_autoPauseTimer_Tick");
        }

        private void _autoStartTimer_Tick(object sender, EventArgs e)
        {
            //todo Needs to respect blackout
            Debug.WriteLine("_autoStartTimer_Tick");
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

                    if (TimeSpan.TryParse(lvi.Text, out startTs) && TimeSpan.TryParse(lvi.SubItems[1].Text, out endTs))
                    {
                        _blackoutSchedules.Add(new KeyValuePair<TimeSpan, TimeSpan>(startTs, endTs));
                    }
                }
            }
        }

        private bool IsBlackoutActive(TimeSpan time)
        {
            if (_blackoutSchedules.Count > 0)
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

            //todo Reinstate, and where else do these need to go?
            //try
            //{
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
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine(ex.Message);
            //}
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

        private void EditScript(Script script)
        {
            var sl = GetScriptingLanguage(GetComboBoxSelectedItem(ref scriptLanguageComboBox).ToString());

            if (sl != null)
            {
                string scriptPath = null;

                switch (script)
                {
                    case Script.Start:
                        scriptPath = Path.Combine(_moveMouseWorkingDirectory, String.Format("{0}.{1}", StartScriptName, sl.FileExtension));
                        break;
                    case Script.Interval:
                        scriptPath = Path.Combine(_moveMouseWorkingDirectory, String.Format("{0}.{1}", IntervalScriptName, sl.FileExtension));
                        break;
                    case Script.Pause:
                        scriptPath = Path.Combine(_moveMouseWorkingDirectory, String.Format("{0}.{1}", PauseScriptName, sl.FileExtension));
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
                        FileName = _scriptEditor,
                        Arguments = String.Format("\"{0}\"", scriptPath)
                    }
                };
                p.Exited += p_Exited;
                p.Start();
            }
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
            else
            {
                _autoStartTimer.Start();
            }

            #region Loop for testing blackout schedules

            //for (int i = 0; i < Convert.ToInt32(new TimeSpan(24, 0, 0).TotalSeconds); i++)
            //{
            //    var ts = new TimeSpan(0, 0, i);
            //    Debug.WriteLine(String.Format("IsBlackoutActive({0} = {1}", ts, IsBlackoutActive(ts)));
            //}

            #endregion
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
                SetButtonText(ref traceButton, String.Format("{0}", _traceTimeComplete.Subtract(DateTime.Now).TotalSeconds.ToString("0.0")));
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
            Debug.WriteLine("_resumeTimer_Tick");
            Debug.WriteLine(String.Format("GetLastInputTime() = {0}", GetLastInputTime()));
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
            if (!AtLeastOneActionIsEnabled())
            {
                clickMouseCheckBox.Checked = true;
            }

            DetermineActionsTabControlState();
        }

        private void moveMouseCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!AtLeastOneActionIsEnabled())
            {
                moveMouseCheckBox.Checked = true;
            }

            DetermineActionsTabControlState();
        }

        private void keystrokeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!AtLeastOneActionIsEnabled())
            {
                keystrokeCheckBox.Checked = true;
            }

            DetermineActionsTabControlState();
        }

        private bool AtLeastOneActionIsEnabled()
        {
            return (moveMouseCheckBox.Checked || clickMouseCheckBox.Checked || keystrokeCheckBox.Checked);
        }

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
                            scriptPath = Path.Combine(_moveMouseWorkingDirectory, String.Format("{0}.{1}", StartScriptName, sl.FileExtension));
                        }

                        break;
                    case Script.Interval:

                        if (GetCheckBoxChecked(ref executeIntervalScriptCheckBox))
                        {
                            scriptPath = Path.Combine(_moveMouseWorkingDirectory, String.Format("{0}.{1}", IntervalScriptName, sl.FileExtension));
                        }

                        break;
                    case Script.Pause:

                        if (GetCheckBoxChecked(ref executePauseScriptCheckBox))
                        {
                            scriptPath = Path.Combine(_moveMouseWorkingDirectory, String.Format("{0}.{1}", PauseScriptName, sl.FileExtension));
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

        private void AddComboBoxItem(ref ComboBox cb, string item, bool selected)
        {
            if (InvokeRequired)
            {
                Invoke(new AddComboBoxItemDelegate(AddComboBoxItem), new object[] {cb, item, selected});
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
                return Invoke(new GetComboBoxTagDelegate(GetComboBoxTag), new object[] {cb});
            }

            return cb.Tag;
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

        private bool IsWindowMinimised(IntPtr handle)
        {
            if (InvokeRequired)
            {
                return Convert.ToBoolean(Invoke(new IsWindowMinimisedDelegate(IsWindowMinimised), new object[] {handle}));
            }

            var placement = GetPlacement(handle);
            return placement.showCmd == ShowWindowCommands.ShowMinimized;
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

            return (idleTime > 0) ? (idleTime/1000) : 0;
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
                        processComboBox.Tag = settingsXmlDoc.SelectSingleNode("settings/activate_application_title").InnerText;
                    }

                    _lastUpdateCheck = Convert.ToDateTime(settingsXmlDoc.SelectSingleNode("settings/last_update_check").InnerText);
                    MinimiseToSystemTrayWarningShown = Convert.ToBoolean(settingsXmlDoc.SelectSingleNode("settings/system_tray_warning_shown").InnerText);
                    executeStartScriptCheckBox.Checked = Convert.ToBoolean(settingsXmlDoc.SelectSingleNode("settings/execute_start_script").InnerText);
                    executeIntervalScriptCheckBox.Checked = Convert.ToBoolean(settingsXmlDoc.SelectSingleNode("settings/execute_interval_script").InnerText);
                    executePauseScriptCheckBox.Checked = Convert.ToBoolean(settingsXmlDoc.SelectSingleNode("settings/execute_pause_script").InnerText);
                    showScriptExecutionCheckBox.Checked = Convert.ToBoolean(settingsXmlDoc.SelectSingleNode("settings/show_script_execution").InnerText);
                    scriptLanguageComboBox.SelectedItem = settingsXmlDoc.SelectSingleNode("settings/script_language").InnerText;
                    _scriptEditor = settingsXmlDoc.SelectSingleNode("settings/script_editor").InnerText;
                    scriptEditorLabel.Text = _scriptEditor;
                    scheduleListView.Items.Clear();
                    blackoutListView.Items.Clear();

                    if (settingsXmlDoc.SelectNodes("settings/schedules/schedule").Count > 0)
                    {
                        foreach (XmlNode scheduleNode in settingsXmlDoc.SelectNodes("settings/schedules/schedule"))
                        {
                            TimeSpan ts;

                            if (TimeSpan.TryParse(scheduleNode.SelectSingleNode("time").InnerText, out ts))
                            {
                                AddScheduleToListView(ts, scheduleNode.SelectSingleNode("action").InnerText, -1, false);
                            }
                        }
                    }

                    if (settingsXmlDoc.SelectNodes("settings/blackouts/blackout").Count > 0)
                    {
                        foreach (XmlNode blackoutNode in settingsXmlDoc.SelectNodes("settings/blackouts/blackout"))
                        {
                            TimeSpan startTs;
                            TimeSpan endTs;

                            if (TimeSpan.TryParse(blackoutNode.SelectSingleNode("start").InnerText, out startTs) && TimeSpan.TryParse(blackoutNode.SelectSingleNode("end").InnerText, out endTs))
                            {
                                AddBlackoutToListView(startTs, endTs, -1, false);
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

        private void SaveSettings()
        {
            try
            {
                var settingsXmlDoc = new XmlDocument();
                settingsXmlDoc.LoadXml("<settings><second_delay /><move_mouse_pointer /><stealth_mode /><enable_static_position /><x_static_position /><y_static_position /><click_left_mouse_button /><send_keystroke /><keystroke /><pause_when_mouse_moved /><automatically_resume /><resume_seconds /><automatically_start_on_launch /><automatically_launch_on_logon /><minimise_on_pause /><minimise_on_start /><minimise_to_system_tray /><activate_application /><activate_application_title /><last_update_check /><system_tray_warning_shown /><execute_start_script /><execute_interval_script /><execute_pause_script /><show_script_execution /><script_language /><script_editor /><schedules /><blackouts /></settings>");
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
                settingsXmlDoc.SelectSingleNode("settings/last_update_check").InnerText = _lastUpdateCheck.ToString("yyyy-MMM-dd HH:mm:ss");
                settingsXmlDoc.SelectSingleNode("settings/system_tray_warning_shown").InnerText = "True";
                settingsXmlDoc.SelectSingleNode("settings/execute_start_script").InnerText = GetCheckBoxChecked(ref executeStartScriptCheckBox).ToString();
                settingsXmlDoc.SelectSingleNode("settings/execute_interval_script").InnerText = GetCheckBoxChecked(ref executeIntervalScriptCheckBox).ToString();
                settingsXmlDoc.SelectSingleNode("settings/execute_pause_script").InnerText = GetCheckBoxChecked(ref executePauseScriptCheckBox).ToString();
                settingsXmlDoc.SelectSingleNode("settings/show_script_execution").InnerText = GetCheckBoxChecked(ref showScriptExecutionCheckBox).ToString();
                settingsXmlDoc.SelectSingleNode("settings/script_language").InnerText = GetComboBoxSelectedItem(ref scriptLanguageComboBox).ToString();
                settingsXmlDoc.SelectSingleNode("settings/script_editor").InnerText = _scriptEditor;

                if (scheduleListView.Items.Count > 0)
                {
                    foreach (ListViewItem lvi in scheduleListView.Items)
                    {
                        var scheduleNode = settingsXmlDoc.CreateElement("schedule");
                        scheduleNode.InnerXml = "<time /><action />";
                        scheduleNode.SelectSingleNode("time").InnerText = lvi.SubItems[0].Text;
                        scheduleNode.SelectSingleNode("action").InnerText = lvi.SubItems[1].Text;
                        settingsXmlDoc.SelectSingleNode("settings/schedules").AppendChild(scheduleNode);
                    }
                }

                if (blackoutListView.Items.Count > 0)
                {
                    foreach (ListViewItem lvi in blackoutListView.Items)
                    {
                        var blackoutNode = settingsXmlDoc.CreateElement("blackout");
                        blackoutNode.InnerXml = "<start /><end />";
                        blackoutNode.SelectSingleNode("start").InnerText = lvi.SubItems[0].Text;
                        blackoutNode.SelectSingleNode("end").InnerText = lvi.SubItems[1].Text;
                        settingsXmlDoc.SelectSingleNode("settings/blackouts").AppendChild(blackoutNode);
                    }
                }

                settingsXmlDoc.Save(Path.Combine(_moveMouseWorkingDirectory, MoveMouseXmlName));
                processComboBox.Tag = processComboBox.Text;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        // ReSharper restore PossibleNullReferenceException

        private void actionButton_Click(object sender, EventArgs e)
        {
            switch (actionButton.Text)
            {
                case "Pause":
                    _mouseTimer.Stop();
                    _autoPauseTimer.Stop();
                    _resumeTimer.Start();
                    _autoStartTimer.Start();
                    actionButton.Text = "Start";
                    countdownProgressBar.Value = 0;
                    optionsTabControl.Enabled = true;
                    Opacity = 1.0;

                    if (minimiseOnPauseCheckBox.Checked)
                    {
                        WindowState = FormWindowState.Minimized;
                    }

                    ReadSettings();
                    LaunchScript(Script.Pause);
                    break;
                default:
                    LaunchScript(Script.Start);
                    _mouseTimerTicks = 0;
                    _blackoutStatus = BlackoutStatusChangeEventArgs.BlackoutStatus.Inactive;
                    MoveMouseToStaticPosition();
                    ActivateApplication();
                    _resumeTimer.Stop();
                    _autoStartTimer.Stop();
_mouseTimer.Start();
                    _autoPauseTimer.Start();
                    actionButton.Text = "Pause";
                    optionsTabControl.SelectedTab = mouseTabPage;
                    optionsTabControl.Enabled = false;
                    Opacity = .75;
                    _mmStartTime = DateTime.Now;
                    WindowState = minimiseOnStartCheckBox.Checked ? FormWindowState.Minimized : FormWindowState.Normal;
                    SaveSettings();
                    break;
            }
        }

        private void _mouseTimer_Tick(object sender, EventArgs e)
        {
            Debug.WriteLine("_mouseTimer_Tick");

            if (!IsBlackoutActive(DateTime.Now.TimeOfDay))
            {
                UpdateCountdownProgressBar(ref countdownProgressBar, Convert.ToInt32(delayNumericUpDown.Value), _mouseTimerTicks);
                _mouseTimerTicks++;
                StopIfMouseHasMoved();

                if (_blackoutStatus == BlackoutStatusChangeEventArgs.BlackoutStatus.Active)
                {
                    _blackoutStatus = BlackoutStatusChangeEventArgs.BlackoutStatus.Inactive;
                    TimeSpan startTime;
                    TimeSpan endTime;
                    GetNextBlackoutStatusChangeTime(out startTime, out endTime);
                    OnBlackoutStatusChange(this, new BlackoutStatusChangeEventArgs(_blackoutStatus, startTime.ToString(), endTime.ToString()));
                }

                if (_mouseTimerTicks > Convert.ToInt32(delayNumericUpDown.Value))
                {
                    LaunchScript(Script.Interval);
                    SendKeystroke();
                    ClickMouse();
                    MoveMouse();
                    _mouseTimerTicks = 0;
                }
            }
            else
            {
                if (_blackoutStatus == BlackoutStatusChangeEventArgs.BlackoutStatus.Inactive)
                {
                    _blackoutStatus = BlackoutStatusChangeEventArgs.BlackoutStatus.Active;
                    TimeSpan startTime;
                    TimeSpan endTime;
                    GetNextBlackoutStatusChangeTime(out startTime, out endTime);
                    OnBlackoutStatusChange(this, new BlackoutStatusChangeEventArgs(_blackoutStatus, startTime.ToString(), endTime.ToString()));
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

        private void StopIfMouseHasMoved()
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
    }
}
