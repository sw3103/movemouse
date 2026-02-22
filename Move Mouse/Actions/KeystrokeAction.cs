using ellabi.Utilities;
using ellabi.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using static ellabi.Wrappers.NativeMethods;

namespace ellabi.Actions
{
    public class KeystrokeAction : ActionBase
    {
        private int[] _keystrokes;
        private RelayCommand _addKeystrokeCommand;
        private RelayCommand _removeSelectedKeystrokeCommand;
        private RelayCommand _moveUpSelectedKeystrokeCommand;
        private RelayCommand _moveDownSelectedKeystrokeCommand;
        private int _selectedIndex = -1;
        private InputMethod _inputMethod;
        private bool _abortIfUserActivityDetected;
        private bool _pause;
        private double _pauseInterval;

        public enum InputMethod
        {
            Sequential,
            Simultaneous
        }

        public IEnumerable<InputMethod> InputMethodValues => Enum.GetValues(typeof(InputMethod)).Cast<InputMethod>();

        public int[] Keystrokes
        {
            get => _keystrokes;
            set
            {
                _keystrokes = value;
                OnPropertyChanged(nameof(Keystrokes));
            }
        }

        public InputMethod Method
        {
            get => _inputMethod;
            set
            {
                _inputMethod = value;
                OnPropertyChanged();
            }
        }

        public bool AbortIfUserActivityDetected
        {
            get => _abortIfUserActivityDetected;
            set
            {
                _abortIfUserActivityDetected = value;
                OnPropertyChanged();
            }
        }

        public bool Pause
        {
            get => _pause;
            set
            {
                _pause = value;
                OnPropertyChanged();
            }
        }

        public double PauseInterval
        {
            get => _pauseInterval;
            set
            {
                _pauseInterval = value;
                OnPropertyChanged();
            }
        }

        [XmlIgnore]
        public RelayCommand AddKeystrokeCommand
        {
            get => _addKeystrokeCommand;
            set
            {
                _addKeystrokeCommand = value;
                OnPropertyChanged();
            }
        }

        [XmlIgnore]
        public RelayCommand RemoveSelectedKeystrokeCommand
        {
            get => _removeSelectedKeystrokeCommand;
            set
            {
                _removeSelectedKeystrokeCommand = value;
                OnPropertyChanged();
            }
        }

        [XmlIgnore]
        public RelayCommand MoveUpSelectedKeystrokeCommand
        {
            get => _moveUpSelectedKeystrokeCommand;
            set
            {
                _moveUpSelectedKeystrokeCommand = value;
                OnPropertyChanged();
            }
        }

        [XmlIgnore]
        public RelayCommand MoveDownSelectedKeystrokeCommand
        {
            get => _moveDownSelectedKeystrokeCommand;
            set
            {
                _moveDownSelectedKeystrokeCommand = value;
                OnPropertyChanged();
            }
        }

        [XmlIgnore]
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                OnPropertyChanged();
            }
        }

        public override bool IsValid => _keystrokes?.Length > 0;

        public override bool CanExecute() => IsValid;

        public KeystrokeAction()
        {
            _pauseInterval = 0.1;
            InterruptsIdleTime = true;
            _removeSelectedKeystrokeCommand = new RelayCommand(param => RemoveSelectedKeystroke(), param => CanRemoveSelectedKeystroke);
            _moveUpSelectedKeystrokeCommand = new RelayCommand(param => MoveUpSelectedKeystroke(), param => CanMoveUpSelectedKeystroke);
            _moveDownSelectedKeystrokeCommand = new RelayCommand(param => MoveDownSelectedKeystroke(), param => CanMoveDownSelectedKeystroke);
        }

        public override void Execute()
        {
            try
            {
                IntervalExecutionCount++;
                StaticCode.Logger?.Here().Information(ToString());
                Aborted = false;

                if (Method == InputMethod.Sequential)
                {
                    var initialCursorPosition = new MouseCursorWrapper().GetCursorPosition();

                    foreach (var key in _keystrokes)
                    {
                        if (AbortIfUserActivityDetected && (initialCursorPosition != new MouseCursorWrapper().GetCursorPosition()))
                        {
                            Aborted = true;
                            break;
                        }
                        else
                        {
                            keybd_event((byte)key, 0, 0, 0);
                            keybd_event((byte)key, 0, (uint)KeyEventFlags.KEYUP, 0);

                            if (Pause)
                            {
                                System.Threading.Thread.Sleep((int)(PauseInterval * 1000));
                            }
                        }
                    }
                }
                else if (Method == InputMethod.Simultaneous)
                {
                    foreach (var key in _keystrokes)
                    {
                        keybd_event((byte)key, 0, 0, 0);
                    }

                    foreach (var key in _keystrokes)
                    {
                        keybd_event((byte)key, 0, (uint)KeyEventFlags.KEYUP, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public void AddKeystroke(int key)
        {
            try
            {
                var selectedIndex = SelectedIndex;
                var keystrokeList = Keystrokes?.ToList() ?? new List<int>();
                keystrokeList.Insert(SelectedIndex + 1, key);
                Keystrokes = keystrokeList.ToArray();
                SelectedIndex = selectedIndex + 1;
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        public void RemoveSelectedKeystroke()
        {
            try
            {
                var selectedIndex = SelectedIndex;
                var keystrokeList = Keystrokes?.ToList() ?? new List<int>();
                keystrokeList.RemoveAt(SelectedIndex);
                Keystrokes = keystrokeList.ToArray();

                if (selectedIndex > 0)
                {
                    SelectedIndex = selectedIndex - 1;
                }
                else if (keystrokeList.Count > 0)
                {
                    SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private bool CanRemoveSelectedKeystroke => SelectedIndex > -1;

        public void MoveUpSelectedKeystroke()
        {
            try
            {
                var selectedIndex = SelectedIndex;
                var keystrokeList = Keystrokes?.ToList() ?? new List<int>();

                if (selectedIndex > 0)
                {
                    var temp = keystrokeList[selectedIndex - 1];
                    keystrokeList[selectedIndex - 1] = keystrokeList[selectedIndex];
                    keystrokeList[selectedIndex] = temp;
                    Keystrokes = keystrokeList.ToArray();
                    SelectedIndex = selectedIndex - 1;
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private bool CanMoveUpSelectedKeystroke => SelectedIndex > 0;

        public void MoveDownSelectedKeystroke()
        {
            try
            {
                var selectedIndex = SelectedIndex;
                var keystrokeList = Keystrokes?.ToList() ?? new List<int>();

                if (selectedIndex < keystrokeList.Count - 1)
                {
                    var temp = keystrokeList[selectedIndex + 1];
                    keystrokeList[selectedIndex + 1] = keystrokeList[selectedIndex];
                    keystrokeList[selectedIndex] = temp;
                    Keystrokes = keystrokeList.ToArray();
                    SelectedIndex = selectedIndex + 1;
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private bool CanMoveDownSelectedKeystroke => (SelectedIndex > -1) && (Keystrokes != null) && (SelectedIndex < Keystrokes.Length - 1);

        public override string ToString()
        {
            return $"{this.GetType().Name} | Name = {Name} | Keystrokes = {Keystrokes} | Trigger = {Trigger} | Repeat = {Repeat} | RepeatMode = {RepeatMode} | IntervalThrottle = {IntervalThrottle} | IntervalExecutionCount = {IntervalExecutionCount} | InterruptsIdleTime = {InterruptsIdleTime}";
        }
    }
}
