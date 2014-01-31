namespace Ellanet
{
    partial class MouseForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mousePictureBox = new System.Windows.Forms.PictureBox();
            this.actionButton = new System.Windows.Forms.Button();
            this.countdownProgressBar = new System.Windows.Forms.ProgressBar();
            this.delayNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.moveMouseCheckBox = new System.Windows.Forms.CheckBox();
            this.clickMouseCheckBox = new System.Windows.Forms.CheckBox();
            this.autoPauseCheckBox = new System.Windows.Forms.CheckBox();
            this.resumeCheckBox = new System.Windows.Forms.CheckBox();
            this.resumeNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.stealthCheckBox = new System.Windows.Forms.CheckBox();
            this.keystrokeCheckBox = new System.Windows.Forms.CheckBox();
            this.keystrokeComboBox = new System.Windows.Forms.ComboBox();
            this.startOnLaunchCheckBox = new System.Windows.Forms.CheckBox();
            this.launchAtLogonCheckBox = new System.Windows.Forms.CheckBox();
            this.staticPositionCheckBox = new System.Windows.Forms.CheckBox();
            this.xNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.yNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.traceButton = new System.Windows.Forms.Button();
            this.minimiseOnPauseCheckBox = new System.Windows.Forms.CheckBox();
            this.minimiseOnStartCheckBox = new System.Windows.Forms.CheckBox();
            this.minimiseToSystemTrayCheckBox = new System.Windows.Forms.CheckBox();
            this.processComboBox = new System.Windows.Forms.ComboBox();
            this.appActivateCheckBox = new System.Windows.Forms.CheckBox();
            this.helpPictureBox = new System.Windows.Forms.PictureBox();
            this.insideOutsideComboBox = new System.Windows.Forms.ComboBox();
            this.blackoutCheckBox = new System.Windows.Forms.CheckBox();
            this.boStartComboBox = new System.Windows.Forms.ComboBox();
            this.boEndComboBox = new System.Windows.Forms.ComboBox();
            this.contactPictureBox = new System.Windows.Forms.PictureBox();
            this.paypalPictureBox = new System.Windows.Forms.PictureBox();
            this.refreshButton = new System.Windows.Forms.Button();
            this.customScriptsCheckBox = new System.Windows.Forms.CheckBox();
            this.scriptsHelpPictureBox = new System.Windows.Forms.PictureBox();
            this.editScriptButton = new System.Windows.Forms.Button();
            this.optionsTabControl = new System.Windows.Forms.TabControl();
            this.mouseTabPage = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.actionsTabPage = new System.Windows.Forms.TabPage();
            this.blackoutsTabPage = new System.Windows.Forms.TabPage();
            this.schedulesTabPage = new System.Windows.Forms.TabPage();
            this.scriptsTabPage = new System.Windows.Forms.TabPage();
            this.behaviourTabPage = new System.Windows.Forms.TabPage();
            ((System.ComponentModel.ISupportInitialize)(this.mousePictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.delayNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.resumeNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.yNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.helpPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.contactPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.paypalPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.scriptsHelpPictureBox)).BeginInit();
            this.optionsTabControl.SuspendLayout();
            this.mouseTabPage.SuspendLayout();
            this.actionsTabPage.SuspendLayout();
            this.blackoutsTabPage.SuspendLayout();
            this.scriptsTabPage.SuspendLayout();
            this.behaviourTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // mousePictureBox
            // 
            this.mousePictureBox.Image = global::Ellanet.Properties.Resources.Mouse_Image;
            this.mousePictureBox.Location = new System.Drawing.Point(30, 83);
            this.mousePictureBox.Name = "mousePictureBox";
            this.mousePictureBox.Size = new System.Drawing.Size(133, 129);
            this.mousePictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.mousePictureBox.TabIndex = 0;
            this.mousePictureBox.TabStop = false;
            // 
            // actionButton
            // 
            this.actionButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.actionButton.Location = new System.Drawing.Point(212, 297);
            this.actionButton.Name = "actionButton";
            this.actionButton.Size = new System.Drawing.Size(75, 23);
            this.actionButton.TabIndex = 0;
            this.actionButton.Text = "Start";
            this.actionButton.UseVisualStyleBackColor = true;
            // 
            // countdownProgressBar
            // 
            this.countdownProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.countdownProgressBar.Location = new System.Drawing.Point(12, 337);
            this.countdownProgressBar.Name = "countdownProgressBar";
            this.countdownProgressBar.Size = new System.Drawing.Size(475, 17);
            this.countdownProgressBar.TabIndex = 2;
            // 
            // delayNumericUpDown
            // 
            this.delayNumericUpDown.Location = new System.Drawing.Point(20, 18);
            this.delayNumericUpDown.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.delayNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.delayNumericUpDown.Name = "delayNumericUpDown";
            this.delayNumericUpDown.Size = new System.Drawing.Size(53, 23);
            this.delayNumericUpDown.TabIndex = 1;
            this.delayNumericUpDown.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(76, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 16);
            this.label1.TabIndex = 4;
            this.label1.Text = "second interval";
            // 
            // moveMouseCheckBox
            // 
            this.moveMouseCheckBox.AutoSize = true;
            this.moveMouseCheckBox.Checked = true;
            this.moveMouseCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.moveMouseCheckBox.Location = new System.Drawing.Point(20, 50);
            this.moveMouseCheckBox.Name = "moveMouseCheckBox";
            this.moveMouseCheckBox.Size = new System.Drawing.Size(143, 20);
            this.moveMouseCheckBox.TabIndex = 2;
            this.moveMouseCheckBox.Text = "Move mouse pointer";
            this.moveMouseCheckBox.UseVisualStyleBackColor = true;
            // 
            // clickMouseCheckBox
            // 
            this.clickMouseCheckBox.AutoSize = true;
            this.clickMouseCheckBox.Location = new System.Drawing.Point(20, 140);
            this.clickMouseCheckBox.Name = "clickMouseCheckBox";
            this.clickMouseCheckBox.Size = new System.Drawing.Size(157, 20);
            this.clickMouseCheckBox.TabIndex = 11;
            this.clickMouseCheckBox.Text = "Click left mouse button";
            this.clickMouseCheckBox.UseVisualStyleBackColor = true;
            // 
            // autoPauseCheckBox
            // 
            this.autoPauseCheckBox.AutoSize = true;
            this.autoPauseCheckBox.Location = new System.Drawing.Point(20, 20);
            this.autoPauseCheckBox.Name = "autoPauseCheckBox";
            this.autoPauseCheckBox.Size = new System.Drawing.Size(224, 20);
            this.autoPauseCheckBox.TabIndex = 20;
            this.autoPauseCheckBox.Text = "Pause when mouse pointer moved";
            this.autoPauseCheckBox.UseVisualStyleBackColor = true;
            // 
            // resumeCheckBox
            // 
            this.resumeCheckBox.AutoSize = true;
            this.resumeCheckBox.Location = new System.Drawing.Point(20, 50);
            this.resumeCheckBox.Name = "resumeCheckBox";
            this.resumeCheckBox.Size = new System.Drawing.Size(359, 20);
            this.resumeCheckBox.TabIndex = 21;
            this.resumeCheckBox.Text = "Automatically resume after                seconds of inactivity";
            this.resumeCheckBox.UseVisualStyleBackColor = true;
            // 
            // resumeNumericUpDown
            // 
            this.resumeNumericUpDown.Enabled = false;
            this.resumeNumericUpDown.Location = new System.Drawing.Point(199, 48);
            this.resumeNumericUpDown.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.resumeNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.resumeNumericUpDown.Name = "resumeNumericUpDown";
            this.resumeNumericUpDown.Size = new System.Drawing.Size(53, 23);
            this.resumeNumericUpDown.TabIndex = 22;
            this.resumeNumericUpDown.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
            // 
            // stealthCheckBox
            // 
            this.stealthCheckBox.AutoSize = true;
            this.stealthCheckBox.Location = new System.Drawing.Point(20, 80);
            this.stealthCheckBox.Name = "stealthCheckBox";
            this.stealthCheckBox.Size = new System.Drawing.Size(282, 20);
            this.stealthCheckBox.TabIndex = 3;
            this.stealthCheckBox.Text = "Stealth mode (pointer movement not visible)";
            this.stealthCheckBox.UseVisualStyleBackColor = true;
            // 
            // keystrokeCheckBox
            // 
            this.keystrokeCheckBox.AutoSize = true;
            this.keystrokeCheckBox.Location = new System.Drawing.Point(20, 170);
            this.keystrokeCheckBox.Name = "keystrokeCheckBox";
            this.keystrokeCheckBox.Size = new System.Drawing.Size(114, 20);
            this.keystrokeCheckBox.TabIndex = 14;
            this.keystrokeCheckBox.Text = "Send keystroke";
            this.keystrokeCheckBox.UseVisualStyleBackColor = true;
            // 
            // keystrokeComboBox
            // 
            this.keystrokeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.keystrokeComboBox.Enabled = false;
            this.keystrokeComboBox.FormattingEnabled = true;
            this.keystrokeComboBox.Items.AddRange(new object[] {
            "{BACKSPACE}",
            "{BREAK}",
            "{CAPSLOCK}",
            "{DELETE}",
            "{DOWN}",
            "{END}",
            "{ENTER}",
            "{ESC}",
            "{HELP}",
            "{HOME}",
            "{INSERT}",
            "{LEFT}",
            "{NUMLOCK}",
            "{PGDN}",
            "{PGUP}",
            "{PRTSC}",
            "{RIGHT}",
            "{SCROLLLOCK}",
            "{TAB}",
            "{UP}",
            "{F1}",
            "{F2}",
            "{F3}",
            "{F4}",
            "{F5}",
            "{F6}",
            "{F7}",
            "{F8}",
            "{F9}",
            "{F10}",
            "{F11}",
            "{F12}",
            "{F13}",
            "{F14}",
            "{F15}",
            "{F16}",
            "{ADD}",
            "{SUBTRACT}",
            "{MULTIPLY}",
            "{DIVIDE}"});
            this.keystrokeComboBox.Location = new System.Drawing.Point(161, 168);
            this.keystrokeComboBox.Name = "keystrokeComboBox";
            this.keystrokeComboBox.Size = new System.Drawing.Size(199, 24);
            this.keystrokeComboBox.TabIndex = 15;
            // 
            // startOnLaunchCheckBox
            // 
            this.startOnLaunchCheckBox.AutoSize = true;
            this.startOnLaunchCheckBox.Location = new System.Drawing.Point(20, 80);
            this.startOnLaunchCheckBox.Name = "startOnLaunchCheckBox";
            this.startOnLaunchCheckBox.Size = new System.Drawing.Size(267, 20);
            this.startOnLaunchCheckBox.TabIndex = 23;
            this.startOnLaunchCheckBox.Text = "Automatically start Move Mouse on launch";
            this.startOnLaunchCheckBox.UseVisualStyleBackColor = true;
            // 
            // launchAtLogonCheckBox
            // 
            this.launchAtLogonCheckBox.AutoSize = true;
            this.launchAtLogonCheckBox.Location = new System.Drawing.Point(20, 110);
            this.launchAtLogonCheckBox.Name = "launchAtLogonCheckBox";
            this.launchAtLogonCheckBox.Size = new System.Drawing.Size(325, 20);
            this.launchAtLogonCheckBox.TabIndex = 24;
            this.launchAtLogonCheckBox.Text = "Automatically launch Move Mouse at Windows logon";
            this.launchAtLogonCheckBox.UseVisualStyleBackColor = true;
            // 
            // staticPositionCheckBox
            // 
            this.staticPositionCheckBox.AutoSize = true;
            this.staticPositionCheckBox.Location = new System.Drawing.Point(20, 110);
            this.staticPositionCheckBox.Name = "staticPositionCheckBox";
            this.staticPositionCheckBox.Size = new System.Drawing.Size(289, 20);
            this.staticPositionCheckBox.TabIndex = 7;
            this.staticPositionCheckBox.Text = "Static mouse pointer position   x                  y";
            this.staticPositionCheckBox.UseVisualStyleBackColor = true;
            // 
            // xNumericUpDown
            // 
            this.xNumericUpDown.Enabled = false;
            this.xNumericUpDown.Location = new System.Drawing.Point(228, 109);
            this.xNumericUpDown.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.xNumericUpDown.Name = "xNumericUpDown";
            this.xNumericUpDown.Size = new System.Drawing.Size(53, 23);
            this.xNumericUpDown.TabIndex = 8;
            // 
            // yNumericUpDown
            // 
            this.yNumericUpDown.Enabled = false;
            this.yNumericUpDown.Location = new System.Drawing.Point(307, 109);
            this.yNumericUpDown.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.yNumericUpDown.Name = "yNumericUpDown";
            this.yNumericUpDown.Size = new System.Drawing.Size(53, 23);
            this.yNumericUpDown.TabIndex = 9;
            // 
            // traceButton
            // 
            this.traceButton.Enabled = false;
            this.traceButton.Location = new System.Drawing.Point(375, 168);
            this.traceButton.Name = "traceButton";
            this.traceButton.Size = new System.Drawing.Size(75, 24);
            this.traceButton.TabIndex = 10;
            this.traceButton.Text = "Track";
            this.traceButton.UseVisualStyleBackColor = true;
            // 
            // minimiseOnPauseCheckBox
            // 
            this.minimiseOnPauseCheckBox.AutoSize = true;
            this.minimiseOnPauseCheckBox.Location = new System.Drawing.Point(20, 140);
            this.minimiseOnPauseCheckBox.Name = "minimiseOnPauseCheckBox";
            this.minimiseOnPauseCheckBox.Size = new System.Drawing.Size(133, 20);
            this.minimiseOnPauseCheckBox.TabIndex = 25;
            this.minimiseOnPauseCheckBox.Text = "Minimise on pause";
            this.minimiseOnPauseCheckBox.UseVisualStyleBackColor = true;
            // 
            // minimiseOnStartCheckBox
            // 
            this.minimiseOnStartCheckBox.AutoSize = true;
            this.minimiseOnStartCheckBox.Location = new System.Drawing.Point(20, 170);
            this.minimiseOnStartCheckBox.Name = "minimiseOnStartCheckBox";
            this.minimiseOnStartCheckBox.Size = new System.Drawing.Size(125, 20);
            this.minimiseOnStartCheckBox.TabIndex = 26;
            this.minimiseOnStartCheckBox.Text = "Minimise on start";
            this.minimiseOnStartCheckBox.UseVisualStyleBackColor = true;
            // 
            // minimiseToSystemTrayCheckBox
            // 
            this.minimiseToSystemTrayCheckBox.AutoSize = true;
            this.minimiseToSystemTrayCheckBox.Location = new System.Drawing.Point(20, 200);
            this.minimiseToSystemTrayCheckBox.Name = "minimiseToSystemTrayCheckBox";
            this.minimiseToSystemTrayCheckBox.Size = new System.Drawing.Size(168, 20);
            this.minimiseToSystemTrayCheckBox.TabIndex = 27;
            this.minimiseToSystemTrayCheckBox.Text = "Minimise to System Tray";
            this.minimiseToSystemTrayCheckBox.UseVisualStyleBackColor = true;
            // 
            // processComboBox
            // 
            this.processComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.processComboBox.Enabled = false;
            this.processComboBox.FormattingEnabled = true;
            this.processComboBox.Location = new System.Drawing.Point(161, 198);
            this.processComboBox.Name = "processComboBox";
            this.processComboBox.Size = new System.Drawing.Size(199, 24);
            this.processComboBox.Sorted = true;
            this.processComboBox.TabIndex = 5;
            // 
            // appActivateCheckBox
            // 
            this.appActivateCheckBox.AutoSize = true;
            this.appActivateCheckBox.Location = new System.Drawing.Point(20, 200);
            this.appActivateCheckBox.Name = "appActivateCheckBox";
            this.appActivateCheckBox.Size = new System.Drawing.Size(137, 20);
            this.appActivateCheckBox.TabIndex = 4;
            this.appActivateCheckBox.Text = "Activate application";
            this.appActivateCheckBox.UseVisualStyleBackColor = true;
            // 
            // helpPictureBox
            // 
            this.helpPictureBox.Image = global::Ellanet.Properties.Resources.Help_Image;
            this.helpPictureBox.Location = new System.Drawing.Point(325, 16);
            this.helpPictureBox.Name = "helpPictureBox";
            this.helpPictureBox.Size = new System.Drawing.Size(32, 32);
            this.helpPictureBox.TabIndex = 21;
            this.helpPictureBox.TabStop = false;
            // 
            // insideOutsideComboBox
            // 
            this.insideOutsideComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.insideOutsideComboBox.Enabled = false;
            this.insideOutsideComboBox.FormattingEnabled = true;
            this.insideOutsideComboBox.Items.AddRange(new object[] {
            "outside",
            "inside"});
            this.insideOutsideComboBox.Location = new System.Drawing.Point(15, 79);
            this.insideOutsideComboBox.Name = "insideOutsideComboBox";
            this.insideOutsideComboBox.Size = new System.Drawing.Size(79, 24);
            this.insideOutsideComboBox.TabIndex = 17;
            // 
            // blackoutCheckBox
            // 
            this.blackoutCheckBox.AutoSize = true;
            this.blackoutCheckBox.Location = new System.Drawing.Point(18, 20);
            this.blackoutCheckBox.Name = "blackoutCheckBox";
            this.blackoutCheckBox.Size = new System.Drawing.Size(176, 20);
            this.blackoutCheckBox.TabIndex = 16;
            this.blackoutCheckBox.Text = "Enable blackout schedules";
            this.blackoutCheckBox.UseVisualStyleBackColor = true;
            // 
            // boStartComboBox
            // 
            this.boStartComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.boStartComboBox.Enabled = false;
            this.boStartComboBox.FormattingEnabled = true;
            this.boStartComboBox.Location = new System.Drawing.Point(161, 79);
            this.boStartComboBox.Name = "boStartComboBox";
            this.boStartComboBox.Size = new System.Drawing.Size(79, 24);
            this.boStartComboBox.TabIndex = 18;
            // 
            // boEndComboBox
            // 
            this.boEndComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.boEndComboBox.Enabled = false;
            this.boEndComboBox.FormattingEnabled = true;
            this.boEndComboBox.Location = new System.Drawing.Point(283, 79);
            this.boEndComboBox.Name = "boEndComboBox";
            this.boEndComboBox.Size = new System.Drawing.Size(79, 24);
            this.boEndComboBox.TabIndex = 19;
            // 
            // contactPictureBox
            // 
            this.contactPictureBox.Image = global::Ellanet.Properties.Resources.Contact_Image;
            this.contactPictureBox.Location = new System.Drawing.Point(284, 21);
            this.contactPictureBox.Name = "contactPictureBox";
            this.contactPictureBox.Size = new System.Drawing.Size(32, 23);
            this.contactPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.contactPictureBox.TabIndex = 25;
            this.contactPictureBox.TabStop = false;
            // 
            // paypalPictureBox
            // 
            this.paypalPictureBox.Image = global::Ellanet.Properties.Resources.PayPal_Image;
            this.paypalPictureBox.Location = new System.Drawing.Point(229, 19);
            this.paypalPictureBox.Name = "paypalPictureBox";
            this.paypalPictureBox.Size = new System.Drawing.Size(41, 27);
            this.paypalPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.paypalPictureBox.TabIndex = 26;
            this.paypalPictureBox.TabStop = false;
            // 
            // refreshButton
            // 
            this.refreshButton.Enabled = false;
            this.refreshButton.Location = new System.Drawing.Point(375, 198);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(75, 24);
            this.refreshButton.TabIndex = 6;
            this.refreshButton.Text = "Refresh";
            this.refreshButton.UseVisualStyleBackColor = true;
            // 
            // customScriptsCheckBox
            // 
            this.customScriptsCheckBox.AutoSize = true;
            this.customScriptsCheckBox.Location = new System.Drawing.Point(20, 20);
            this.customScriptsCheckBox.Name = "customScriptsCheckBox";
            this.customScriptsCheckBox.Size = new System.Drawing.Size(151, 20);
            this.customScriptsCheckBox.TabIndex = 12;
            this.customScriptsCheckBox.Text = "Enable custom scripts";
            this.customScriptsCheckBox.UseVisualStyleBackColor = true;
            // 
            // scriptsHelpPictureBox
            // 
            this.scriptsHelpPictureBox.Image = global::Ellanet.Properties.Resources.Help_Image;
            this.scriptsHelpPictureBox.Location = new System.Drawing.Point(220, 201);
            this.scriptsHelpPictureBox.Name = "scriptsHelpPictureBox";
            this.scriptsHelpPictureBox.Size = new System.Drawing.Size(19, 20);
            this.scriptsHelpPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.scriptsHelpPictureBox.TabIndex = 29;
            this.scriptsHelpPictureBox.TabStop = false;
            // 
            // editScriptButton
            // 
            this.editScriptButton.Enabled = false;
            this.editScriptButton.Location = new System.Drawing.Point(252, 200);
            this.editScriptButton.Name = "editScriptButton";
            this.editScriptButton.Size = new System.Drawing.Size(75, 23);
            this.editScriptButton.TabIndex = 13;
            this.editScriptButton.Text = "Edit";
            this.editScriptButton.UseVisualStyleBackColor = true;
            // 
            // optionsTabControl
            // 
            this.optionsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.optionsTabControl.Controls.Add(this.actionsTabPage);
            this.optionsTabControl.Controls.Add(this.blackoutsTabPage);
            this.optionsTabControl.Controls.Add(this.schedulesTabPage);
            this.optionsTabControl.Controls.Add(this.scriptsTabPage);
            this.optionsTabControl.Controls.Add(this.behaviourTabPage);
            this.optionsTabControl.Controls.Add(this.mouseTabPage);
            this.optionsTabControl.Location = new System.Drawing.Point(12, 12);
            this.optionsTabControl.Name = "optionsTabControl";
            this.optionsTabControl.SelectedIndex = 0;
            this.optionsTabControl.Size = new System.Drawing.Size(475, 269);
            this.optionsTabControl.TabIndex = 30;
            // 
            // mouseTabPage
            // 
            this.mouseTabPage.Controls.Add(this.label2);
            this.mouseTabPage.Controls.Add(this.mousePictureBox);
            this.mouseTabPage.Controls.Add(this.paypalPictureBox);
            this.mouseTabPage.Controls.Add(this.contactPictureBox);
            this.mouseTabPage.Controls.Add(this.helpPictureBox);
            this.mouseTabPage.Location = new System.Drawing.Point(4, 25);
            this.mouseTabPage.Name = "mouseTabPage";
            this.mouseTabPage.Size = new System.Drawing.Size(467, 240);
            this.mouseTabPage.TabIndex = 4;
            this.mouseTabPage.Text = "Mouse";
            this.mouseTabPage.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(235, 83);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(330, 16);
            this.label2.TabIndex = 27;
            this.label2.Text = "double click shows famous mice and updates tabe name";
            // 
            // actionsTabPage
            // 
            this.actionsTabPage.Controls.Add(this.delayNumericUpDown);
            this.actionsTabPage.Controls.Add(this.label1);
            this.actionsTabPage.Controls.Add(this.moveMouseCheckBox);
            this.actionsTabPage.Controls.Add(this.clickMouseCheckBox);
            this.actionsTabPage.Controls.Add(this.refreshButton);
            this.actionsTabPage.Controls.Add(this.stealthCheckBox);
            this.actionsTabPage.Controls.Add(this.xNumericUpDown);
            this.actionsTabPage.Controls.Add(this.yNumericUpDown);
            this.actionsTabPage.Controls.Add(this.traceButton);
            this.actionsTabPage.Controls.Add(this.appActivateCheckBox);
            this.actionsTabPage.Controls.Add(this.processComboBox);
            this.actionsTabPage.Controls.Add(this.keystrokeCheckBox);
            this.actionsTabPage.Controls.Add(this.keystrokeComboBox);
            this.actionsTabPage.Controls.Add(this.staticPositionCheckBox);
            this.actionsTabPage.Location = new System.Drawing.Point(4, 25);
            this.actionsTabPage.Name = "actionsTabPage";
            this.actionsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.actionsTabPage.Size = new System.Drawing.Size(467, 240);
            this.actionsTabPage.TabIndex = 0;
            this.actionsTabPage.Text = "Actions";
            this.actionsTabPage.UseVisualStyleBackColor = true;
            // 
            // blackoutsTabPage
            // 
            this.blackoutsTabPage.Controls.Add(this.boEndComboBox);
            this.blackoutsTabPage.Controls.Add(this.blackoutCheckBox);
            this.blackoutsTabPage.Controls.Add(this.insideOutsideComboBox);
            this.blackoutsTabPage.Controls.Add(this.boStartComboBox);
            this.blackoutsTabPage.Location = new System.Drawing.Point(4, 25);
            this.blackoutsTabPage.Name = "blackoutsTabPage";
            this.blackoutsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.blackoutsTabPage.Size = new System.Drawing.Size(467, 240);
            this.blackoutsTabPage.TabIndex = 1;
            this.blackoutsTabPage.Text = "Blackouts";
            this.blackoutsTabPage.UseVisualStyleBackColor = true;
            // 
            // schedulesTabPage
            // 
            this.schedulesTabPage.Location = new System.Drawing.Point(4, 25);
            this.schedulesTabPage.Name = "schedulesTabPage";
            this.schedulesTabPage.Size = new System.Drawing.Size(467, 240);
            this.schedulesTabPage.TabIndex = 5;
            this.schedulesTabPage.Text = "Schedules";
            this.schedulesTabPage.UseVisualStyleBackColor = true;
            // 
            // scriptsTabPage
            // 
            this.scriptsTabPage.Controls.Add(this.customScriptsCheckBox);
            this.scriptsTabPage.Controls.Add(this.editScriptButton);
            this.scriptsTabPage.Controls.Add(this.scriptsHelpPictureBox);
            this.scriptsTabPage.Location = new System.Drawing.Point(4, 25);
            this.scriptsTabPage.Name = "scriptsTabPage";
            this.scriptsTabPage.Size = new System.Drawing.Size(467, 240);
            this.scriptsTabPage.TabIndex = 2;
            this.scriptsTabPage.Text = "Custom Scripts";
            this.scriptsTabPage.UseVisualStyleBackColor = true;
            // 
            // behaviourTabPage
            // 
            this.behaviourTabPage.Controls.Add(this.autoPauseCheckBox);
            this.behaviourTabPage.Controls.Add(this.resumeNumericUpDown);
            this.behaviourTabPage.Controls.Add(this.startOnLaunchCheckBox);
            this.behaviourTabPage.Controls.Add(this.minimiseToSystemTrayCheckBox);
            this.behaviourTabPage.Controls.Add(this.launchAtLogonCheckBox);
            this.behaviourTabPage.Controls.Add(this.minimiseOnStartCheckBox);
            this.behaviourTabPage.Controls.Add(this.minimiseOnPauseCheckBox);
            this.behaviourTabPage.Controls.Add(this.resumeCheckBox);
            this.behaviourTabPage.Location = new System.Drawing.Point(4, 25);
            this.behaviourTabPage.Name = "behaviourTabPage";
            this.behaviourTabPage.Size = new System.Drawing.Size(467, 240);
            this.behaviourTabPage.TabIndex = 3;
            this.behaviourTabPage.Text = "Behaviour";
            this.behaviourTabPage.UseVisualStyleBackColor = true;
            // 
            // MouseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(499, 366);
            this.Controls.Add(this.optionsTabControl);
            this.Controls.Add(this.countdownProgressBar);
            this.Controls.Add(this.actionButton);
            this.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.Name = "MouseForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MouseForm";
            ((System.ComponentModel.ISupportInitialize)(this.mousePictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.delayNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.resumeNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.yNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.helpPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.contactPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.paypalPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.scriptsHelpPictureBox)).EndInit();
            this.optionsTabControl.ResumeLayout(false);
            this.mouseTabPage.ResumeLayout(false);
            this.mouseTabPage.PerformLayout();
            this.actionsTabPage.ResumeLayout(false);
            this.actionsTabPage.PerformLayout();
            this.blackoutsTabPage.ResumeLayout(false);
            this.blackoutsTabPage.PerformLayout();
            this.scriptsTabPage.ResumeLayout(false);
            this.scriptsTabPage.PerformLayout();
            this.behaviourTabPage.ResumeLayout(false);
            this.behaviourTabPage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox mousePictureBox;
        private System.Windows.Forms.Button actionButton;
        private System.Windows.Forms.ProgressBar countdownProgressBar;
        private System.Windows.Forms.NumericUpDown delayNumericUpDown;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox moveMouseCheckBox;
        private System.Windows.Forms.CheckBox clickMouseCheckBox;
        private System.Windows.Forms.CheckBox autoPauseCheckBox;
        private System.Windows.Forms.CheckBox resumeCheckBox;
        private System.Windows.Forms.NumericUpDown resumeNumericUpDown;
        private System.Windows.Forms.CheckBox stealthCheckBox;
        private System.Windows.Forms.CheckBox keystrokeCheckBox;
        private System.Windows.Forms.ComboBox keystrokeComboBox;
        private System.Windows.Forms.CheckBox startOnLaunchCheckBox;
        private System.Windows.Forms.CheckBox launchAtLogonCheckBox;
        private System.Windows.Forms.CheckBox staticPositionCheckBox;
        private System.Windows.Forms.NumericUpDown xNumericUpDown;
        private System.Windows.Forms.NumericUpDown yNumericUpDown;
        private System.Windows.Forms.Button traceButton;
        private System.Windows.Forms.CheckBox minimiseOnPauseCheckBox;
        private System.Windows.Forms.CheckBox minimiseOnStartCheckBox;
        private System.Windows.Forms.CheckBox minimiseToSystemTrayCheckBox;
        private System.Windows.Forms.ComboBox processComboBox;
        private System.Windows.Forms.CheckBox appActivateCheckBox;
        private System.Windows.Forms.PictureBox helpPictureBox;
        private System.Windows.Forms.ComboBox insideOutsideComboBox;
        private System.Windows.Forms.CheckBox blackoutCheckBox;
        private System.Windows.Forms.ComboBox boStartComboBox;
        private System.Windows.Forms.ComboBox boEndComboBox;
        private System.Windows.Forms.PictureBox contactPictureBox;
        private System.Windows.Forms.PictureBox paypalPictureBox;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.CheckBox customScriptsCheckBox;
        private System.Windows.Forms.PictureBox scriptsHelpPictureBox;
        private System.Windows.Forms.Button editScriptButton;
        private System.Windows.Forms.TabControl optionsTabControl;
        private System.Windows.Forms.TabPage actionsTabPage;
        private System.Windows.Forms.TabPage blackoutsTabPage;
        private System.Windows.Forms.TabPage scriptsTabPage;
        private System.Windows.Forms.TabPage mouseTabPage;
        private System.Windows.Forms.TabPage behaviourTabPage;
        private System.Windows.Forms.TabPage schedulesTabPage;
        private System.Windows.Forms.Label label2;
    }
}