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

        public override bool IsValid => !String.IsNullOrWhiteSpace(FilePath) && File.Exists(FilePath);

        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                OnPropertyChanged();
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

        public override bool CanExecute()
        {
            return IsValid;
        }

        public override void Execute()
        {
            try
            {
                StaticCode.Logger?.Here().Information(ToString());

                if (File.Exists(FilePath))
                {
                    var process = new Process
                    {
                        StartInfo =
                        {
                            FileName = FilePath.Trim(),
                            Arguments = Arguments,
                            WindowStyle = Hidden ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal
                        }
                    };

                    process.Start();
                    if (WaitForExit) process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public override string ToString()
        {
            return $"{this.GetType().Name} | Name = {Name} | FilePath = {FilePath} | Arguments = {Arguments} | WaitForExit = {WaitForExit} | Hidden = {Hidden} | Trigger = {Trigger} | Repeat = {Repeat} | RepeatMode = {RepeatMode} | IntervalThrottle = {IntervalThrottle} | IntervalExecutionCount = {IntervalExecutionCount} | InterruptsIdleTime = {InterruptsIdleTime}";
        }
    }
}