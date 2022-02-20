using System;
using System.Windows.Forms;

namespace Ellanet.Forms
{
    public partial class AddScheduleForm : Form
    {
        public TimeSpan Time { get; internal set; }
        public string Action { get; internal set; }

        public AddScheduleForm() : this(DateTime.Now.TimeOfDay, "Start")
        {
        }

        public AddScheduleForm(TimeSpan time, string action)
        {
            InitializeComponent();
            hourNumericUpDown.Value = time.Hours;
            minuteNumericUpDown.Value = time.Minutes;
            secondNumericUpDown.Value = time.Seconds;

            if (!String.IsNullOrEmpty(action) && (actionComboBox.Items.Contains(action)))
            {
                actionComboBox.SelectedItem = action;
            }
            else
            {
                actionComboBox.SelectedIndex = 0;
            }

            okButton.Click += okButton_Click;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Time = new TimeSpan(Convert.ToInt32(hourNumericUpDown.Value), Convert.ToInt32(minuteNumericUpDown.Value), Convert.ToInt32(secondNumericUpDown.Value));
            Action = actionComboBox.SelectedItem.ToString();
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
