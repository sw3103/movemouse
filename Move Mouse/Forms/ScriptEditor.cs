using System;
using System.IO;
using System.Windows.Forms;

namespace Ellanet.Forms
{
    public partial class ScriptEditor : Form
    {
        private const int MaxScriptPathLength = 25;

        private readonly ErrorProvider _ep = new ErrorProvider();

        public string ScriptEditorPath { get; private set; }
        public string ScriptToEdit { get; private set; }

        public ScriptEditor(string editorPath)
        {
            InitializeComponent();
            ScriptEditorPath = editorPath;
            scriptEditorLabel.TextChanged += scriptEditorLabel_TextChanged;
            scriptEditorLabel.Text = editorPath;
            Icon = Properties.Resources.ScriptEditor_Icon;
            scriptsComboBox.SelectedIndex = 1;
            editButton.Click += editButton_Click;
            browseButton.Click += browseButton_Click;
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

        private void browseButton_Click(object sender, EventArgs e)
        {
            _ep.Dispose();
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
                ScriptEditorPath = ofd.FileName;
                scriptEditorLabel.Text = ScriptEditorPath;
            }
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            _ep.Dispose();

            if (File.Exists(ScriptEditorPath))
            {
                DialogResult = DialogResult.OK;
                ScriptToEdit = scriptsComboBox.Text;
                Close();
            }
            else
            {
                _ep.SetError(scriptEditorLabel, "Script editor could not be located. Use the Browser button to locate your script editor.");
            }
        }
    }
}
