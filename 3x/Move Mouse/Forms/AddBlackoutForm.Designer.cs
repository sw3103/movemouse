namespace Ellanet.Forms
{
    partial class AddBlackoutForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.startHourNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.startMinuteNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.startSecondNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.canButton = new System.Windows.Forms.Button();
            this.endSecondNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.endMinuteNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.endHourNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.startHourNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.startMinuteNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.startSecondNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.endSecondNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.endMinuteNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.endHourNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Start:";
            // 
            // startHourNumericUpDown
            // 
            this.startHourNumericUpDown.Location = new System.Drawing.Point(72, 18);
            this.startHourNumericUpDown.Maximum = new decimal(new int[] {
            23,
            0,
            0,
            0});
            this.startHourNumericUpDown.Name = "startHourNumericUpDown";
            this.startHourNumericUpDown.Size = new System.Drawing.Size(35, 23);
            this.startHourNumericUpDown.TabIndex = 1;
            // 
            // startMinuteNumericUpDown
            // 
            this.startMinuteNumericUpDown.Location = new System.Drawing.Point(113, 18);
            this.startMinuteNumericUpDown.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.startMinuteNumericUpDown.Name = "startMinuteNumericUpDown";
            this.startMinuteNumericUpDown.Size = new System.Drawing.Size(35, 23);
            this.startMinuteNumericUpDown.TabIndex = 2;
            // 
            // startSecondNumericUpDown
            // 
            this.startSecondNumericUpDown.Location = new System.Drawing.Point(154, 18);
            this.startSecondNumericUpDown.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.startSecondNumericUpDown.Name = "startSecondNumericUpDown";
            this.startSecondNumericUpDown.Size = new System.Drawing.Size(35, 23);
            this.startSecondNumericUpDown.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(106, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(13, 16);
            this.label2.TabIndex = 4;
            this.label2.Text = ":";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(147, 21);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(13, 16);
            this.label3.TabIndex = 5;
            this.label3.Text = ":";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(20, 59);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(34, 16);
            this.label4.TabIndex = 6;
            this.label4.Text = "End:";
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(21, 96);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 8;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // canButton
            // 
            this.canButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.canButton.Location = new System.Drawing.Point(114, 96);
            this.canButton.Name = "canButton";
            this.canButton.Size = new System.Drawing.Size(75, 23);
            this.canButton.TabIndex = 9;
            this.canButton.Text = "Cancel";
            this.canButton.UseVisualStyleBackColor = true;
            // 
            // endSecondNumericUpDown
            // 
            this.endSecondNumericUpDown.Location = new System.Drawing.Point(154, 57);
            this.endSecondNumericUpDown.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.endSecondNumericUpDown.Name = "endSecondNumericUpDown";
            this.endSecondNumericUpDown.Size = new System.Drawing.Size(35, 23);
            this.endSecondNumericUpDown.TabIndex = 6;
            // 
            // endMinuteNumericUpDown
            // 
            this.endMinuteNumericUpDown.Location = new System.Drawing.Point(113, 57);
            this.endMinuteNumericUpDown.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.endMinuteNumericUpDown.Name = "endMinuteNumericUpDown";
            this.endMinuteNumericUpDown.Size = new System.Drawing.Size(35, 23);
            this.endMinuteNumericUpDown.TabIndex = 5;
            // 
            // endHourNumericUpDown
            // 
            this.endHourNumericUpDown.Location = new System.Drawing.Point(72, 57);
            this.endHourNumericUpDown.Maximum = new decimal(new int[] {
            23,
            0,
            0,
            0});
            this.endHourNumericUpDown.Name = "endHourNumericUpDown";
            this.endHourNumericUpDown.Size = new System.Drawing.Size(35, 23);
            this.endHourNumericUpDown.TabIndex = 4;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(147, 60);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(13, 16);
            this.label5.TabIndex = 14;
            this.label5.Text = ":";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(106, 60);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(13, 16);
            this.label6.TabIndex = 13;
            this.label6.Text = ":";
            // 
            // AddBlackoutForm
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.canButton;
            this.ClientSize = new System.Drawing.Size(211, 139);
            this.Controls.Add(this.endSecondNumericUpDown);
            this.Controls.Add(this.endMinuteNumericUpDown);
            this.Controls.Add(this.endHourNumericUpDown);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.canButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.startSecondNumericUpDown);
            this.Controls.Add(this.startMinuteNumericUpDown);
            this.Controls.Add(this.startHourNumericUpDown);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "AddBlackoutForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Blackout";
            ((System.ComponentModel.ISupportInitialize)(this.startHourNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.startMinuteNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.startSecondNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.endSecondNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.endMinuteNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.endHourNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown startHourNumericUpDown;
        private System.Windows.Forms.NumericUpDown startMinuteNumericUpDown;
        private System.Windows.Forms.NumericUpDown startSecondNumericUpDown;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button canButton;
        private System.Windows.Forms.NumericUpDown endSecondNumericUpDown;
        private System.Windows.Forms.NumericUpDown endMinuteNumericUpDown;
        private System.Windows.Forms.NumericUpDown endHourNumericUpDown;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
    }
}