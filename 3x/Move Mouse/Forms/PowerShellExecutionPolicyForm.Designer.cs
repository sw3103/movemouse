namespace Ellanet.Forms
{
    partial class PowerShellExecutionPolicyForm
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
            this.canButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.policyComboBox = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // canButton
            // 
            this.canButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.canButton.Location = new System.Drawing.Point(193, 194);
            this.canButton.Name = "canButton";
            this.canButton.Size = new System.Drawing.Size(75, 23);
            this.canButton.TabIndex = 11;
            this.canButton.Text = "Cancel";
            this.canButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(100, 194);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 10;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // policyComboBox
            // 
            this.policyComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.policyComboBox.FormattingEnabled = true;
            this.policyComboBox.Items.AddRange(new object[] {
            "Restricted",
            "AllSigned",
            "RemoteSigned",
            "Unrestricted"});
            this.policyComboBox.Location = new System.Drawing.Point(124, 88);
            this.policyComboBox.Name = "policyComboBox";
            this.policyComboBox.Size = new System.Drawing.Size(121, 24);
            this.policyComboBox.TabIndex = 13;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(20, 91);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(103, 16);
            this.label4.TabIndex = 0;
            this.label4.Text = "Execution Policy:";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(20, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(336, 56);
            this.label1.TabIndex = 14;
            this.label1.Text = "It is recommended that your PowerShell execution policy is either \"Unrestricted\" " +
    "or \"RemoteSigned\" if you would like to use Move Mouse\'s scripting capabilities.";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(20, 131);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(336, 56);
            this.label2.TabIndex = 15;
            this.label2.Text = "Administrator privileges are required to change PowerShell\'s execution policy, so" +
    " you may see a prompt asking if you will allow this change after clicking OK.";
            // 
            // PowerShellExecutionPolicyForm
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.canButton;
            this.ClientSize = new System.Drawing.Size(368, 237);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.policyComboBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.canButton);
            this.Controls.Add(this.okButton);
            this.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "PowerShellExecutionPolicyForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "PowerShell Execution Policy";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button canButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.ComboBox policyComboBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}