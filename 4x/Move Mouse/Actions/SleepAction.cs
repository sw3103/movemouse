using System;
using System.Threading;

namespace ellabi.Actions
{
    public class SleepAction : ActionBase
    {
        private double _seconds;
        private double _upperSeconds;
        private bool _random;

        public override bool IsValid => Seconds > 0.0;

        public double Seconds
        {
            get => _seconds;
            set
            {
                if (value > UpperSeconds)
                {
                    UpperSeconds = value;
                }

                _seconds = value < .1 ? .1 : value;
                OnPropertyChanged();
            }
        }

        public double UpperSeconds
        {
            get => _upperSeconds;
            set
            {
                if (value < Seconds)
                {
                    Seconds = value;
                }

                _upperSeconds = value < .1 ? .1 : value;
                OnPropertyChanged();
            }
        }

        public bool Random
        {
            get => _random;
            set
            {
                _random = value; OnPropertyChanged();
            }
        }

        public SleepAction()
        {
            _seconds = 1;
            _upperSeconds = 2;
        }

        public override bool CanExecute()
        {
            return IsValid;
        }

        public override void Execute()
        {
            try
            {
                var sleep = TimeSpan.FromSeconds(Random ? new Random().Next(Convert.ToInt32(Seconds), Convert.ToInt32(UpperSeconds)) : Seconds);
                StaticCode.Logger?.Here().Information(ToString());
                StaticCode.Logger?.Here().Information(sleep.ToString());
                Thread.Sleep(sleep);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public override string ToString()
        {
            return $"Name = {Name} | Random = {Random} | Seconds = {Seconds} | UpperSeconds = {UpperSeconds} | Trigger = {Trigger} | Repeat = {Repeat}";
        }
    }
}