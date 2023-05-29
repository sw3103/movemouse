using System;
using System.Diagnostics;
using System.IO;

namespace ellabi.Actions
{
    public class ScriptAction : ActionBase
    {
        private const string PowerShellPath = @"%SystemRoot%\system32\WindowsPowerShell\v1.0\powershell.exe";

        private string _scriptPath;
        private bool _waitForExit;
        private bool _hidden;

        public override bool IsValid => !String.IsNullOrWhiteSpace(ScriptPath) && File.Exists(ScriptPath) && ScriptPath.EndsWith(".ps1", StringComparison.CurrentCultureIgnoreCase);

        public string ScriptPath
        {
            get => _scriptPath;
            set
            {
                _scriptPath = value;
                OnPropertyChanged();
            }
        }

        public bool WaitForExit
        {
            get => _waitForExit;
            set
            {
                _waitForExit = value;
                OnPropertyChanged();
            }
        }

        public bool Hidden
        {
            get => _hidden;
            set
            {
                _hidden = value;
                OnPropertyChanged();
            }
        }

        public override bool CanExecute()
        {
            return IsValid;
        }

        public override void Execute()
        {
            try
            {
                StaticCode.Logger?.Here().Information(ToString());
                string powerShellPath = Environment.ExpandEnvironmentVariables(PowerShellPath);

                if (File.Exists(powerShellPath) && File.Exists(ScriptPath))
                {
                    var powershell = new Process
                    {
                        StartInfo =
                        {
                            FileName = powerShellPath,
                            Arguments = String.Format("-ExecutionPolicy Bypass -File \"{0}\"", ScriptPath.Trim()),
                            WindowStyle = Hidden ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal
                        }
                    };

                    powershell.Start();
                    if (WaitForExit) powershell.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public override string ToString()
        {
            return $"{this.GetType().Name} | Name = {Name} | ScriptPath = {ScriptPath} | WaitForExit = {WaitForExit} | Hidden = {Hidden} | Trigger = {Trigger} | Repeat = {Repeat} | RepeatMode = {RepeatMode} | IntervalThrottle = {IntervalThrottle} | IntervalExecutionCount = {IntervalExecutionCount} | InterruptsIdleTime = {InterruptsIdleTime}";
        }
    }
}