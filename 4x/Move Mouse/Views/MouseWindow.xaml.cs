using ellabi.ViewModels;
using ellabi.Wrappers;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Shell;

namespace ellabi.Views
{
    public partial class MouseWindow
    {
        private MouseWindowViewModel _vm;
        private SettingsWindow _settingsWindow;
        private bool _buttonsOnShow;
        private bool _hideButtons;
        private DateTime _hideTime;
        private readonly TimeSpan _hideTimeDelay = TimeSpan.FromMilliseconds(2500);
        private readonly object _lock = new object();
        //private IntPtr _hookId = IntPtr.Zero;
        //private Key _hookKey;
        //private NativeMethods.LowLevelKeyboardProc _hookProc;

        public MouseWindow()
        {
            InitializeComponent();
            Loaded += MouseWindow_Loaded;
        }

        private void MouseWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                TaskbarItemInfo = new TaskbarItemInfo();
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }

            _vm = new MouseWindowViewModel();
            DataContext = _vm;
            _vm.MouseStateChanged += _vm_MouseStateChanged;
            _vm.AltTabVisibilityChanged += _vm_AltTabVisibilityChanged;
            _vm.RequestActivate += _vm_RequestActivate;
            _vm.RequestMinimise += _vm_RequestMinimise;
            //_vm.HookKeyEnabledChanged += _vm_HookKeyEnabledChanged;

            if (_vm.SettingsVm.Settings.HideFromAltTab)
            {
                _vm_AltTabVisibilityChanged(this, false);
            }

            if (_vm.SettingsVm.Settings.StartAtLaunch)
            {
                _vm.Start();
            }
            else if (_vm.SettingsVm.Settings.MinimiseOnStop)
            {
                WindowState = WindowState.Minimized;
            }
        }

        private void _vm_RequestMinimise(object sender)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate { WindowState = WindowState.Minimized; }));
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        //private void _vm_HookKeyEnabledChanged(object sender, bool enabled, Key key)
        //{
        //    if (_hookId != IntPtr.Zero)
        //    {
        //        //Debug.WriteLine(String.Format("Unhooking {0}...", _hookKey));
        //        NativeMethods.UnhookWindowsHookEx(_hookId);
        //    }

        //    if (enabled && !key.Equals(Key.None))
        //    {
        //        _hookKey = key;
        //        //Debug.WriteLine(String.Format("Hooking {0}...", _hookKey));
        //        _hookId = SetHook(_hookProc);
        //    }
        //}

        private void _vm_RequestActivate(object sender)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
                {
                    WindowState = WindowState.Normal;
                    Activate();
                }));
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void _vm_AltTabVisibilityChanged(object sender, bool visible)
        {
            StaticCode.Logger?.Here().Debug(visible.ToString());

            try
            {
                var wndHelper = new WindowInteropHelper(this);
                int exStyle = (int)NativeMethods.GetWindowLong(wndHelper.Handle, (int)NativeMethods.GetWindowLongFields.GWL_EXSTYLE);

                if (!visible)
                {
                    exStyle |= (int)NativeMethods.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
                }
                else
                {
                    exStyle ^= (int)NativeMethods.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
                }

                NativeMethods.SetWindowLong(wndHelper.Handle, (int)NativeMethods.GetWindowLongFields.GWL_EXSTYLE, exStyle);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void _vm_MouseStateChanged(object sender, MouseWindowViewModel.MouseState state)
        {
            StaticCode.Logger?.Here().Debug(state.ToString());

            try
            {
                try
                {
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
                    {
                        if (TaskbarItemInfo != null)
                        {
                            TaskbarItemInfo.ProgressState =
                            state.Equals(MouseWindowViewModel.MouseState.Running) ? TaskbarItemProgressState.Normal :
                            state.Equals(MouseWindowViewModel.MouseState.Executing) ? TaskbarItemProgressState.Error :
                            state.Equals(MouseWindowViewModel.MouseState.Paused) ? TaskbarItemProgressState.Paused :
                            state.Equals(MouseWindowViewModel.MouseState.Sleeping) ? TaskbarItemProgressState.Paused :
                            state.Equals(MouseWindowViewModel.MouseState.OnBattery) ? TaskbarItemProgressState.Paused :
                            TaskbarItemProgressState.None;
                        }
                    }));
                }
                catch (Exception ex)
                {
                    StaticCode.Logger?.Here().Error(ex.Message);
                }

                //Debug.WriteLine($"state = {state}");
                var duration = _vm.ExecutionTime.Subtract(DateTime.Now);
                //Debug.WriteLine($"duration = {duration}");

                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
                {
                    CountDownProgressBar.BeginAnimation(RangeBase.ValueProperty, state.Equals(MouseWindowViewModel.MouseState.Running) ? new DoubleAnimation(100, 0, new Duration(duration > TimeSpan.Zero ? duration : TimeSpan.Zero)) : null);
                }));
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void Window_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(e.ChangedButton.ToString());

            try
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    DragMove();
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                MouseTaskbarIcon.TrayPopupResolved.IsOpen = false;
                ShowSettingsWindow();
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void ShowSettingsWindow()
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                if ((_settingsWindow == null) || !_settingsWindow.IsLoaded)
                {
                    _settingsWindow = new SettingsWindow
                    {
                        DataContext = ((MouseWindowViewModel)DataContext).SettingsVm
                    };
                    _settingsWindow.Show();
                }

                _settingsWindow.Activate();
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void PayPalButton_OnClick(object sender, RoutedEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                MouseTaskbarIcon.TrayPopupResolved.IsOpen = false;
                Process.Start(StaticCode.PayPalUrl);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void HomeButton_OnClick(object sender, RoutedEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                MouseTaskbarIcon.TrayPopupResolved.IsOpen = false;
                Process.Start(StaticCode.HomePageUrl);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void HelpButton_OnClick(object sender, RoutedEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                MouseTaskbarIcon.TrayPopupResolved.IsOpen = false;
                Process.Start(StaticCode.HelpPageUrl);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void TwitterButton_OnClick(object sender, RoutedEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                Process.Start(StaticCode.TwitterUrl);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                //MouseTaskbarIcon.TrayPopupResolved.IsOpen = false;

                //if ((_settingsWindow != null) && _settingsWindow.IsLoaded)
                //{
                //    Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate { _settingsWindow.Close(); Thread.Sleep(2500); }));
                //}

                //_vm.Dispose();
                //Close();
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void MouseButton_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                if (e.RightButton.Equals(MouseButtonState.Pressed))
                {
                    ShowSettingsWindow();
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void MouseGrid_OnMouseEnter(object sender, MouseEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);
            ShowButtons();
        }

        private void ShowButtons()
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                _hideButtons = false;

                if (!_buttonsOnShow)
                {
                    if (_vm.SettingsVm.Settings.DisableButtonAnimation)
                    {
                        StartInstantAnimation(ref FlyingSettingsButton, 1);
                        //StartInstantAnimation(ref FlyingContactButton, 2);
                        StartInstantAnimation(ref FlyingPayPalButton, 2);
                        StartInstantAnimation(ref FlyingHelpButton, 3);
                        StartInstantAnimation(ref FlyingTwitterButton, 4);
                        StartInstantAnimation(ref FlyingCloseButton, 5);
                    }
                    else
                    {
                        StartFlyingAnimation(ref FlyingSettingsButton, 1);
                        //StartFlyingAnimation(ref FlyingContactButton, 2);
                        StartFlyingAnimation(ref FlyingPayPalButton, 2);
                        StartFlyingAnimation(ref FlyingHelpButton, 3);
                        //StartFlyingAnimation(ref FlyingHomeButton, 3);
                        StartFlyingAnimation(ref FlyingTwitterButton, 4);
                        StartFlyingAnimation(ref FlyingCloseButton, 5);
                    }
                }

                _buttonsOnShow = true;
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void StartFlyingAnimation(ref Button button, int animationIndex)
        {
            try
            {
                if (button.Content is Image buttonImage)
                {
                    var buttonSb = FindResource($"FlyingButtonAnimation{animationIndex}") as Storyboard;
                    var imageSb = FindResource($"FlyingImageAnimation{animationIndex}") as Storyboard;

                    if (buttonSb != null)
                    {
                        Storyboard.SetTarget(buttonSb, button);
                        buttonSb.Begin();
                    }

                    if (imageSb != null)
                    {
                        Storyboard.SetTarget(imageSb, buttonImage);
                        imageSb.Begin();
                    }
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void StartInstantAnimation(ref Button button, int animationIndex)
        {
            try
            {
                if (button.Content is Image buttonImage)
                {
                    var buttonSb = FindResource($"InstantButtonAnimation{animationIndex}") as Storyboard;
                    var imageSb = FindResource($"InstantImageAnimation{animationIndex}") as Storyboard;

                    if (buttonSb != null)
                    {
                        Storyboard.SetTarget(buttonSb, button);
                        buttonSb.Begin();
                    }

                    if (imageSb != null)
                    {
                        Storyboard.SetTarget(imageSb, buttonImage);
                        imageSb.Begin();
                    }
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void StartFadeOutAnimation(Button button)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
                {
                    if (FindResource(_vm.SettingsVm.Settings.DisableButtonAnimation ? "ButtonInstantFadeOutAnimation" : "ButtonFadeOutAnimation") is Storyboard buttonSb)
                    {
                        Storyboard.SetTarget(buttonSb, button);
                        buttonSb.Begin();
                    }
                }));
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void UIElement_OnMouseEnter(object sender, MouseEventArgs e)
        {
            _hideButtons = false;
        }

        private void UIElement_OnMouseLeave(object sender, MouseEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(HideButtons);
        }

        private void HideButtons(object stateInfo)
        {
            try
            {
                _hideButtons = true;
                _hideTime = DateTime.Now.Add(_hideTimeDelay);

                lock (_lock)
                {
                    while (_hideButtons && (DateTime.Now < _hideTime))
                    {
                        Thread.Sleep(100);
                    }

                    if (_hideButtons)
                    {
                        StartFadeOutAnimation(FlyingSettingsButton);
                        //StartFadeOutAnimation(FlyingContactButton);
                        StartFadeOutAnimation(FlyingPayPalButton);
                        StartFadeOutAnimation(FlyingHelpButton);
                        //StartFadeOutAnimation(FlyingHomeButton);
                        StartFadeOutAnimation(FlyingTwitterButton);
                        StartFadeOutAnimation(FlyingCloseButton);
                        _buttonsOnShow = false;
                    }
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void UpdateCallout_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(StaticCode.UpdateUrl))
                {
                    Process.Start(StaticCode.UpdateUrl);
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        //private IntPtr SetHook(NativeMethods.LowLevelKeyboardProc proc)
        //{
        //    try
        //    {
        //        return NativeMethods.SetWindowsHookEx(NativeMethods.WH_KEYBOARD_LL, proc, NativeMethods.GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
        //    }
        //    catch (Exception ex)
        //    {
        //        StaticCode.Logger?.Here().Error(ex.Message);
        //    }

        //    return IntPtr.Zero;
        //}

        private void ContactButton_OnClick(object sender, RoutedEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                Process.Start(StaticCode.ContactMailToAddress);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void MouseWindow_OnClosing(object sender, CancelEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(e.ToString());

            try
            {
                MouseTaskbarIcon.TrayPopupResolved.IsOpen = false;

                if ((_settingsWindow != null) && _settingsWindow.IsLoaded)
                {
                    _settingsWindow.Close();
                }

                _vm.Dispose();
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void CountDownProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                //Debug.WriteLine(e.NewValue);

                if (TaskbarItemInfo != null)
                {
                    TaskbarItemInfo.ProgressValue = e.NewValue / 100;
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }
    }
}