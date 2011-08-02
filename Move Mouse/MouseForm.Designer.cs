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
            ((System.ComponentModel.ISupportInitialize)(this.mousePictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.delayNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.resumeNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.yNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // mousePictureBox
            // 
            this.mousePictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.mousePictureBox.Image = global::Ellanet.Properties.Resources.Mouse_Image;
            this.mousePictureBox.Location = new System.Drawing.Point(379, 185);
            this.mousePictureBox.Name = "mousePictureBox";
            this.mousePictureBox.Size = new System.Drawing.Size(133, 129);
            this.mousePictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.mousePictureBox.TabIndex = 0;
            this.mousePictureBox.TabStop = false;
            // 
            // actionButton
            // 
            this.actionButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.actionButton.Location = new System.Drawing.Point(218, 291);
            this.actionButton.Name = "actionButton";
            this.actionButton.Size = new System.Drawing.Size(89, 23);
            this.actionButton.TabIndex = 0;
            this.actionButton.Text = "Start";
            this.actionButton.UseVisualStyleBackColor = true;
            // 
            // countdownProgressBar
            // 
            this.countdownProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.countdownProgressBar.Location = new System.Drawing.Point(12, 330);
            this.countdownProgressBar.Name = "countdownProgressBar";
            this.countdownProgressBar.Size = new System.Drawing.Size(500, 17);
            this.countdownProgressBar.TabIndex = 2;
            // 
            // delayNumericUpDown
            // 
            this.delayNumericUpDown.Location = new System.Drawing.Point(12, 12);
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
            this.label1.Location = new System.Drawing.Point(68, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 16);
            this.label1.TabIndex = 4;
            this.label1.Text = "second delay";
            // 
            // moveMouseCheckBox
            // 
            this.moveMouseCheckBox.AutoSize = true;
            this.moveMouseCheckBox.Checked = true;
            this.moveMouseCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.moveMouseCheckBox.Location = new System.Drawing.Point(12, 41);
            this.moveMouseCheckBox.Name = "moveMouseCheckBox";
            this.moveMouseCheckBox.Size = new System.Drawing.Size(143, 20);
            this.moveMouseCheckBox.TabIndex = 2;
            this.moveMouseCheckBox.Text = "Move mouse pointer";
            this.moveMouseCheckBox.UseVisualStyleBackColor = true;
            // 
            // clickMouseCheckBox
            // 
            this.clickMouseCheckBox.AutoSize = true;
            this.clickMouseCheckBox.Location = new System.Drawing.Point(12, 119);
            this.clickMouseCheckBox.Name = "clickMouseCheckBox";
            this.clickMouseCheckBox.Size = new System.Drawing.Size(157, 20);
            this.clickMouseCheckBox.TabIndex = 8;
            this.clickMouseCheckBox.Text = "Click left mouse button";
            this.clickMouseCheckBox.UseVisualStyleBackColor = true;
            // 
            // autoPauseCheckBox
            // 
            this.autoPauseCheckBox.AutoSize = true;
            this.autoPauseCheckBox.Location = new System.Drawing.Point(12, 171);
            this.autoPauseCheckBox.Name = "autoPauseCheckBox";
            this.autoPauseCheckBox.Size = new System.Drawing.Size(224, 20);
            this.autoPauseCheckBox.TabIndex = 11;
            this.autoPauseCheckBox.Text = "Pause when mouse pointer moved";
            this.autoPauseCheckBox.UseVisualStyleBackColor = true;
            // 
            // resumeCheckBox
            // 
            this.resumeCheckBox.AutoSize = true;
            this.resumeCheckBox.Location = new System.Drawing.Point(12, 197);
            this.resumeCheckBox.Name = "resumeCheckBox";
            this.resumeCheckBox.Size = new System.Drawing.Size(359, 20);
            this.resumeCheckBox.TabIndex = 12;
            this.resumeCheckBox.Text = "Automatically resume after                seconds of inactivity";
            this.resumeCheckBox.UseVisualStyleBackColor = true;
            // 
            // resumeNumericUpDown
            // 
            this.resumeNumericUpDown.Enabled = false;
            this.resumeNumericUpDown.Location = new System.Drawing.Point(191, 196);
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
            this.resumeNumericUpDown.TabIndex = 13;
            this.resumeNumericUpDown.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
            // 
            // stealthCheckBox
            // 
            this.stealthCheckBox.AutoSize = true;
            this.stealthCheckBox.Location = new System.Drawing.Point(12, 67);
            this.stealthCheckBox.Name = "stealthCheckBox";
            this.stealthCheckBox.Size = new System.Drawing.Size(282, 20);
            this.stealthCheckBox.TabIndex = 3;
            this.stealthCheckBox.Text = "Stealth mode (pointer movement not visible)";
            this.stealthCheckBox.UseVisualStyleBackColor = true;
            // 
            // keystrokeCheckBox
            // 
            this.keystrokeCheckBox.AutoSize = true;
            this.keystrokeCheckBox.Location = new System.Drawing.Point(12, 145);
            this.keystrokeCheckBox.Name = "keystrokeCheckBox";
            this.keystrokeCheckBox.Size = new System.Drawing.Size(114, 20);
            this.keystrokeCheckBox.TabIndex = 9;
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
            this.keystrokeComboBox.Location = new System.Drawing.Point(132, 143);
            this.keystrokeComboBox.Name = "keystrokeComboBox";
            this.keystrokeComboBox.Size = new System.Drawing.Size(138, 24);
            this.keystrokeComboBox.TabIndex = 10;
            // 
            // startOnLaunchCheckBox
            // 
            this.startOnLaunchCheckBox.AutoSize = true;
            this.startOnLaunchCheckBox.Location = new System.Drawing.Point(12, 223);
            this.startOnLaunchCheckBox.Name = "startOnLaunchCheckBox";
            this.startOnLaunchCheckBox.Size = new System.Drawing.Size(267, 20);
            this.startOnLaunchCheckBox.TabIndex = 14;
            this.startOnLaunchCheckBox.Text = "Automatically start Move Mouse on launch";
            this.startOnLaunchCheckBox.UseVisualStyleBackColor = true;
            // 
            // launchAtLogonCheckBox
            // 
            this.launchAtLogonCheckBox.AutoSize = true;
            this.launchAtLogonCheckBox.Location = new System.Drawing.Point(12, 248);
            this.launchAtLogonCheckBox.Name = "launchAtLogonCheckBox";
            this.launchAtLogonCheckBox.Size = new System.Drawing.Size(325, 20);
            this.launchAtLogonCheckBox.TabIndex = 15;
            this.launchAtLogonCheckBox.Text = "Automatically launch Move Mouse at Windows logon";
            this.launchAtLogonCheckBox.UseVisualStyleBackColor = true;
            // 
            // staticPositionCheckBox
            // 
            this.staticPositionCheckBox.AutoSize = true;
            this.staticPositionCheckBox.Location = new System.Drawing.Point(12, 93);
            this.staticPositionCheckBox.Name = "staticPositionCheckBox";
            this.staticPositionCheckBox.Size = new System.Drawing.Size(321, 20);
            this.staticPositionCheckBox.TabIndex = 4;
            this.staticPositionCheckBox.Text = "Enable mouse pointer static position   x                y";
            this.staticPositionCheckBox.UseVisualStyleBackColor = true;
            // 
            // xNumericUpDown
            // 
            this.xNumericUpDown.Enabled = false;
            this.xNumericUpDown.Location = new System.Drawing.Point(260, 92);
            this.xNumericUpDown.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.xNumericUpDown.Name = "xNumericUpDown";
            this.xNumericUpDown.Size = new System.Drawing.Size(53, 23);
            this.xNumericUpDown.TabIndex = 5;
            // 
            // yNumericUpDown
            // 
            this.yNumericUpDown.Enabled = false;
            this.yNumericUpDown.Location = new System.Drawing.Point(330, 92);
            this.yNumericUpDown.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.yNumericUpDown.Name = "yNumericUpDown";
            this.yNumericUpDown.Size = new System.Drawing.Size(53, 23);
            this.yNumericUpDown.TabIndex = 6;
            // 
            // traceButton
            // 
            this.traceButton.Enabled = false;
            this.traceButton.Location = new System.Drawing.Point(395, 92);
            this.traceButton.Name = "traceButton";
            this.traceButton.Size = new System.Drawing.Size(89, 23);
            this.traceButton.TabIndex = 7;
            this.traceButton.Text = "Trace";
            this.traceButton.UseVisualStyleBackColor = true;
            // 
            // minimiseOnPauseCheckBox
            // 
            this.minimiseOnPauseCheckBox.AutoSize = true;
            this.minimiseOnPauseCheckBox.Checked = true;
            this.minimiseOnPauseCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.minimiseOnPauseCheckBox.Location = new System.Drawing.Point(12, 274);
            this.minimiseOnPauseCheckBox.Name = "minimiseOnPauseCheckBox";
            this.minimiseOnPauseCheckBox.Size = new System.Drawing.Size(133, 20);
            this.minimiseOnPauseCheckBox.TabIndex = 16;
            this.minimiseOnPauseCheckBox.Text = "Minimise on pause";
            this.minimiseOnPauseCheckBox.UseVisualStyleBackColor = true;
            // 
            // minimiseOnStartCheckBox
            // 
            this.minimiseOnStartCheckBox.AutoSize = true;
            this.minimiseOnStartCheckBox.Checked = true;
            this.minimiseOnStartCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.minimiseOnStartCheckBox.Location = new System.Drawing.Point(12, 300);
            this.minimiseOnStartCheckBox.Name = "minimiseOnStartCheckBox";
            this.minimiseOnStartCheckBox.Size = new System.Drawing.Size(125, 20);
            this.minimiseOnStartCheckBox.TabIndex = 17;
            this.minimiseOnStartCheckBox.Text = "Minimise on start";
            this.minimiseOnStartCheckBox.UseVisualStyleBackColor = true;
            // 
            // MouseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(524, 362);
            this.Controls.Add(this.minimiseOnStartCheckBox);
            this.Controls.Add(this.minimiseOnPauseCheckBox);
            this.Controls.Add(this.traceButton);
            this.Controls.Add(this.yNumericUpDown);
            this.Controls.Add(this.xNumericUpDown);
            this.Controls.Add(this.staticPositionCheckBox);
            this.Controls.Add(this.launchAtLogonCheckBox);
            this.Controls.Add(this.startOnLaunchCheckBox);
            this.Controls.Add(this.keystrokeComboBox);
            this.Controls.Add(this.keystrokeCheckBox);
            this.Controls.Add(this.stealthCheckBox);
            this.Controls.Add(this.resumeNumericUpDown);
            this.Controls.Add(this.resumeCheckBox);
            this.Controls.Add(this.autoPauseCheckBox);
            this.Controls.Add(this.clickMouseCheckBox);
            this.Controls.Add(this.moveMouseCheckBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.delayNumericUpDown);
            this.Controls.Add(this.countdownProgressBar);
            this.Controls.Add(this.actionButton);
            this.Controls.Add(this.mousePictureBox);
            this.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
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
            this.ResumeLayout(false);
            this.PerformLayout();

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
    }
}