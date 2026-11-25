using ellabi.Platform;
using System;
using System.IO;

namespace ellabi.Actions
{
    public class ScriptAction : ActionBase
    {
        private string _scriptPath;
        private bool _waitForExit;
        private bool _hidden;

        public override bool IsValid =>
            !string.IsNullOrWhiteSpace(ScriptPath) &&
            File.Exists(ScriptPath) &&
            (ScriptPath.EndsWith(".sh", StringComparison.OrdinalIgnoreCase) ||
             ScriptPath.EndsWith(".py", StringComparison.OrdinalIgnoreCase) ||
             ScriptPath.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase));

        public string ScriptPath
        {
            get => _scriptPath;
            set
            {
                _scriptPath = value;
                OnPropertyChanged();
                ExecuteCommand?.RaiseCanExecuteChanged();
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

        public override bool CanExecute() => IsValid;

        public override void Execute()
        {
            try
            {
                IntervalExecutionCount++;
                StaticCode.Logger?.Here().Information(ToString());
                LinuxDesktopService.RunScript(ScriptPath, WaitForExit, Hidden);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public override string ToString()
        {
            return $"{GetType().Name} | Name = {Name} | ScriptPath = {ScriptPath} | WaitForExit = {WaitForExit} | Hidden = {Hidden} | Trigger = {Trigger} | Repeat = {Repeat} | RepeatMode = {RepeatMode} | IntervalThrottle = {IntervalThrottle} | IntervalExecutionCount = {IntervalExecutionCount} | InterruptsIdleTime = {InterruptsIdleTime}";
        }
    }
}
