using System;
using System.Diagnostics;
using System.IO;

namespace ellabi.Actions
{
    public class CommandAction : ActionBase
    {
        private string _filePath;
        private string _arguments;
        private bool _waitForExit;
        private bool _hidden;

        public override bool IsValid => !string.IsNullOrWhiteSpace(FilePath) && File.Exists(FilePath);

        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                OnPropertyChanged();
                ExecuteCommand?.RaiseCanExecuteChanged();
            }
        }

        public string Arguments
        {
            get => _arguments;
            set
            {
                _arguments = value;
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

        public override bool CanExecute() => IsValid;

        public override void Execute()
        {
            try
            {
                IntervalExecutionCount++;
                StaticCode.Logger?.Here().Information(ToString());

                if (File.Exists(FilePath))
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = FilePath.Trim(),
                        Arguments = Arguments ?? string.Empty,
                        UseShellExecute = false,
                        CreateNoWindow = Hidden
                    };

                    var process = Process.Start(psi);
                    if (WaitForExit && process != null)
                    {
                        process.WaitForExit();
                    }
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public override string ToString()
        {
            return $"{GetType().Name} | Name = {Name} | FilePath = {FilePath} | Arguments = {Arguments} | WaitForExit = {WaitForExit} | Hidden = {Hidden} | Trigger = {Trigger} | Repeat = {Repeat} | RepeatMode = {RepeatMode} | IntervalThrottle = {IntervalThrottle} | IntervalExecutionCount = {IntervalExecutionCount} | InterruptsIdleTime = {InterruptsIdleTime}";
        }
    }
}
