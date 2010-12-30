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
            this.label2 = new System.Windows.Forms.Label();
            this.stealthCheckBox = new System.Windows.Forms.CheckBox();
            this.keystrokeCheckBox = new System.Windows.Forms.CheckBox();
            this.keystrokeComboBox = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.mousePictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.delayNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.resumeNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // mousePictureBox
            // 
            this.mousePictureBox.Image = global::Ellanet.Properties.Resources.Mouse_Image;
            this.mousePictureBox.Location = new System.Drawing.Point(12, 12);
            this.mousePictureBox.Name = "mousePictureBox";
            this.mousePictureBox.Size = new System.Drawing.Size(133, 129);
            this.mousePictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.mousePictureBox.TabIndex = 0;
            this.mousePictureBox.TabStop = false;
            // 
            // actionButton
            // 
            this.actionButton.Location = new System.Drawing.Point(41, 161);
            this.actionButton.Name = "actionButton";
            this.actionButton.Size = new System.Drawing.Size(75, 23);
            this.actionButton.TabIndex = 0;
            this.actionButton.Text = "Start";
            this.actionButton.UseVisualStyleBackColor = true;
            // 
            // countdownProgressBar
            // 
            this.countdownProgressBar.Location = new System.Drawing.Point(12, 203);
            this.countdownProgressBar.Name = "countdownProgressBar";
            this.countdownProgressBar.Size = new System.Drawing.Size(499, 17);
            this.countdownProgressBar.TabIndex = 2;
            // 
            // delayNumericUpDown
            // 
            this.delayNumericUpDown.Location = new System.Drawing.Point(155, 12);
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
            this.label1.Location = new System.Drawing.Point(211, 14);
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
            this.moveMouseCheckBox.Location = new System.Drawing.Point(155, 41);
            this.moveMouseCheckBox.Name = "moveMouseCheckBox";
            this.moveMouseCheckBox.Size = new System.Drawing.Size(143, 20);
            this.moveMouseCheckBox.TabIndex = 2;
            this.moveMouseCheckBox.Text = "Move mouse pointer";
            this.moveMouseCheckBox.UseVisualStyleBackColor = true;
            // 
            // clickMouseCheckBox
            // 
            this.clickMouseCheckBox.AutoSize = true;
            this.clickMouseCheckBox.Location = new System.Drawing.Point(155, 93);
            this.clickMouseCheckBox.Name = "clickMouseCheckBox";
            this.clickMouseCheckBox.Size = new System.Drawing.Size(157, 20);
            this.clickMouseCheckBox.TabIndex = 4;
            this.clickMouseCheckBox.Text = "Click left mouse button";
            this.clickMouseCheckBox.UseVisualStyleBackColor = true;
            // 
            // autoPauseCheckBox
            // 
            this.autoPauseCheckBox.AutoSize = true;
            this.autoPauseCheckBox.Location = new System.Drawing.Point(155, 145);
            this.autoPauseCheckBox.Name = "autoPauseCheckBox";
            this.autoPauseCheckBox.Size = new System.Drawing.Size(304, 20);
            this.autoPauseCheckBox.TabIndex = 6;
            this.autoPauseCheckBox.Text = "Automatically pause when mouse pointer moved";
            this.autoPauseCheckBox.UseVisualStyleBackColor = true;
            // 
            // resumeCheckBox
            // 
            this.resumeCheckBox.AutoSize = true;
            this.resumeCheckBox.Location = new System.Drawing.Point(155, 171);
            this.resumeCheckBox.Name = "resumeCheckBox";
            this.resumeCheckBox.Size = new System.Drawing.Size(181, 20);
            this.resumeCheckBox.TabIndex = 7;
            this.resumeCheckBox.Text = "Automatically resume after";
            this.resumeCheckBox.UseVisualStyleBackColor = true;
            // 
            // resumeNumericUpDown
            // 
            this.resumeNumericUpDown.Enabled = false;
            this.resumeNumericUpDown.Location = new System.Drawing.Point(334, 170);
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
            this.resumeNumericUpDown.TabIndex = 8;
            this.resumeNumericUpDown.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(390, 172);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(122, 16);
            this.label2.TabIndex = 10;
            this.label2.Text = "seconds of inactivity";
            // 
            // stealthCheckBox
            // 
            this.stealthCheckBox.AutoSize = true;
            this.stealthCheckBox.Location = new System.Drawing.Point(155, 67);
            this.stealthCheckBox.Name = "stealthCheckBox";
            this.stealthCheckBox.Size = new System.Drawing.Size(322, 20);
            this.stealthCheckBox.TabIndex = 3;
            this.stealthCheckBox.Text = "Enable stealth mode (pointer movement not visible)";
            this.stealthCheckBox.UseVisualStyleBackColor = true;
            // 
            // keystrokeCheckBox
            // 
            this.keystrokeCheckBox.AutoSize = true;
            this.keystrokeCheckBox.Location = new System.Drawing.Point(155, 119);
            this.keystrokeCheckBox.Name = "keystrokeCheckBox";
            this.keystrokeCheckBox.Size = new System.Drawing.Size(114, 20);
            this.keystrokeCheckBox.TabIndex = 5;
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
            this.keystrokeComboBox.Location = new System.Drawing.Point(275, 117);
            this.keystrokeComboBox.Name = "keystrokeComboBox";
            this.keystrokeComboBox.Size = new System.Drawing.Size(138, 24);
            this.keystrokeComboBox.TabIndex = 11;
            // 
            // MouseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(523, 235);
            this.Controls.Add(this.keystrokeComboBox);
            this.Controls.Add(this.keystrokeCheckBox);
            this.Controls.Add(this.stealthCheckBox);
            this.Controls.Add(this.label2);
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
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox stealthCheckBox;
        private System.Windows.Forms.CheckBox keystrokeCheckBox;
        private System.Windows.Forms.ComboBox keystrokeComboBox;
    }
}