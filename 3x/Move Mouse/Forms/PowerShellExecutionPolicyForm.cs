using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Ellanet.Forms
{
    public partial class PowerShellExecutionPolicyForm : Form
    {
        public PowerShellExecutionPolicyForm()
        {
            InitializeComponent();
            okButton.Click += okButton_Click;
            policyComboBox.SelectedIndex = 0;
        }

        void okButton_Click(object sender, EventArgs e)
        {
            var powershell = new Process
            {
                StartInfo =
                {
                    FileName = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\system32\WindowsPowerShell\v1.0\powershell.exe"), 
                    Arguments = String.Format("-Command \"Set-ExecutionPolicy {0}\"", policyComboBox.SelectedItem), 
                    Verb = "runas", 
                    WindowStyle = ProcessWindowStyle.Normal
                }
            };
            powershell.Start();
            Close();
        }
    }
}
