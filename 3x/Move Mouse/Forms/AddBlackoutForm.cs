using System;
using System.Windows.Forms;

namespace Ellanet.Forms
{
    public partial class AddBlackoutForm : Form
    {
        public TimeSpan Start { get; internal set; }
        public TimeSpan End { get; internal set; }

        public AddBlackoutForm() : this(new TimeSpan(17, 0, 0), new TimeSpan(9, 0, 0))
        {
        }

        public AddBlackoutForm(TimeSpan start, TimeSpan end)
        {
            InitializeComponent();
            startHourNumericUpDown.Value = start.Hours;
            startMinuteNumericUpDown.Value = start.Minutes;
            startSecondNumericUpDown.Value = start.Seconds;
            endHourNumericUpDown.Value = end.Hours;
            endMinuteNumericUpDown.Value = end.Minutes;
            endSecondNumericUpDown.Value = end.Seconds;
            okButton.Click += okButton_Click;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Start = new TimeSpan(Convert.ToInt32(startHourNumericUpDown.Value), Convert.ToInt32(startMinuteNumericUpDown.Value), Convert.ToInt32(startSecondNumericUpDown.Value));
            End = new TimeSpan(Convert.ToInt32(endHourNumericUpDown.Value), Convert.ToInt32(endMinuteNumericUpDown.Value), Convert.ToInt32(endSecondNumericUpDown.Value));
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
