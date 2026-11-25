using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ellabi.Platform;
using ellabi.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace ellabi.Views
{
    public partial class MouseWindow : Window
    {
        private const double OrbitCenter = 300.0;
        private const double OrbitRadius = 150.0;
        private const double ButtonHalfSize = 25.0;

        private MouseWindowViewModel _vm;
        private SettingsWindow _settingsWindow;
        private TrayIcon _trayIcon;
        private bool _buttonsOnShow;
        private bool _hideButtons;
        private bool _isClosing;
        private DateTime _hideTime;
        private readonly TimeSpan _hideTimeDelay = TimeSpan.FromMilliseconds(2500);
        private readonly object _lock = new object();

        // Tray click / double-click detection
        private DateTime _lastTrayClickTime = DateTime.MinValue;
        private DispatcherTimer _traySingleClickTimer;
        private static readonly TimeSpan DoubleClickThreshold = TimeSpan.FromMilliseconds(380);

        // Timers for smooth Avalonia animations
        private DispatcherTimer _countdownTimer;
        private DateTime _countdownStart;
        private TimeSpan _countdownDuration;

        private DispatcherTimer _blinkTimer;
        private int _blinkPhase;

        private DispatcherTimer _flyingAnimTimer;
        private DateTime _flyingAnimStart;
        private bool _flyingFadingOut;

        private DispatcherTimer _screenBurnTimer;
        private bool _screenBurnToggle;
        private bool _isPointerOverWindow;

        private DispatcherTimer _updateCalloutTimer;

        private readonly Dictionary<string, (Bitmap Dark, Bitmap Light)> _iconCache = new();
        private readonly Dictionary<MouseWindowViewModel.MouseState, WindowIcon> _trayIcons = new();

        private sealed class FlyingSlot
        {
            public Button Button { get; init; }
            public Image Image { get; init; }
            public double TargetAngle { get; init; }
            public double DurationSec { get; init; }
            public double DelaySec { get; init; }
        }

        private List<FlyingSlot> _flyingSlots;

        public MouseWindow()
        {
            InitializeComponent();
            Opened += MouseWindow_Opened;
        }

        private void MouseWindow_Opened(object sender, EventArgs e)
        {
            if (_vm != null) return;

            _vm = new MouseWindowViewModel();
            DataContext = _vm;
            _vm.MouseStateChanged += _vm_MouseStateChanged;
            _vm.AltTabVisibilityChanged += _vm_AltTabVisibilityChanged;
            _vm.RequestActivate += _vm_RequestActivate;
            _vm.RequestMinimise += _vm_RequestMinimise;
            _vm.RequestNotification += _vm_RequestNotification;
            _vm.PropertyChanged += _vm_PropertyChanged;
            _vm.SettingsVm.Settings.PropertyChanged += Settings_PropertyChanged;

            InitializeFlyingSlots();
            InitializeTrayIcon();
            InitializeTimers();
            ApplyWindowSettings();

            if (_vm.SettingsVm.Settings.StartAtLaunch)
            {
                _vm.Start();
                _vm.ShowNotification("Automatically started at launch.");
            }
            else if (_vm.SettingsVm.Settings.MinimiseOnStop)
            {
                WindowState = WindowState.Minimized;
            }
        }

        private void InitializeFlyingSlots()
        {
            _flyingSlots = new List<FlyingSlot>
            {
                CreateSlot(FlyingMinimiseButton, FlyingMinimiseImage, "Minimise", 420, 1.75, 0.0),
                CreateSlot(FlyingSettingsButton, FlyingSettingsImage, "Settings", 360, 1.60, 0.15),
                CreateSlot(FlyingPayPalButton, FlyingPayPalImage, "PayPal", 300, 1.45, 0.30),
                CreateSlot(FlyingTwitterButton, FlyingTwitterImage, "Twitter", 240, 1.30, 0.45),
                CreateSlot(FlyingHelpButton, FlyingHelpImage, "Help", 180, 1.15, 0.60),
                CreateSlot(FlyingCloseButton, FlyingCloseImage, "Close", 120, 1.15, 0.60),
            };
        }

        private FlyingSlot CreateSlot(Button btn, Image img, string prefix, double targetAngle, double durationSec, double delaySec)
        {
            var dark = LoadBitmap($"avares://MoveMouse/Resources/{prefix}_Dark_64x64.png");
            var light = LoadBitmap($"avares://MoveMouse/Resources/{prefix}_Light_64x64.png");
            _iconCache[prefix] = (dark, light);

            btn.PointerEntered += (_, _) => img.Source = light;
            btn.PointerExited += (_, _) => img.Source = dark;

            SetSlotAngle(btn, 0);

            return new FlyingSlot
            {
                Button = btn,
                Image = img,
                TargetAngle = targetAngle,
                DurationSec = durationSec,
                DelaySec = delaySec
            };
        }

        private static void SetSlotAngle(Button btn, double angleDeg)
        {
            // Angle 0° starts at 9 o'clock (-OrbitRadius, 0) and sweeps clockwise
            double rad = angleDeg * Math.PI / 180.0;
            double left = OrbitCenter - OrbitRadius * Math.Cos(rad) - ButtonHalfSize;
            double top = OrbitCenter - OrbitRadius * Math.Sin(rad) - ButtonHalfSize;
            Canvas.SetLeft(btn, left);
            Canvas.SetTop(btn, top);
        }

        private static Bitmap LoadBitmap(string uri)
        {
            using var stream = AssetLoader.Open(new Uri(uri));
            return new Bitmap(stream);
        }

        private static WindowIcon LoadWindowIcon(string uri)
        {
            using var stream = AssetLoader.Open(new Uri(uri));
            return new WindowIcon(stream);
        }

        private void InitializeTrayIcon()
        {
            try
            {
                _trayIcons[MouseWindowViewModel.MouseState.Idle] = LoadWindowIcon("avares://MoveMouse/Resources/Mouse.ico");
                _trayIcons[MouseWindowViewModel.MouseState.Running] = LoadWindowIcon("avares://MoveMouse/Resources/Mouse-SystemTray-Active.ico");
                _trayIcons[MouseWindowViewModel.MouseState.Executing] = LoadWindowIcon("avares://MoveMouse/Resources/Mouse-SystemTray-Execute.ico");
                _trayIcons[MouseWindowViewModel.MouseState.Paused] = LoadWindowIcon("avares://MoveMouse/Resources/Mouse-SystemTray-Pause.ico");
                _trayIcons[MouseWindowViewModel.MouseState.Sleeping] = LoadWindowIcon("avares://MoveMouse/Resources/Mouse-SystemTray-Sleep.ico");
                _trayIcons[MouseWindowViewModel.MouseState.OnBattery] = LoadWindowIcon("avares://MoveMouse/Resources/Mouse-SystemTray-Battery.ico");
                _trayIcons[MouseWindowViewModel.MouseState.Locked] = LoadWindowIcon("avares://MoveMouse/Resources/Mouse.ico");

                var menu = new NativeMenu();
                var toggleItem = new NativeMenuItem("Start / Stop");
                toggleItem.Click += (_, _) => _vm.StartStopToggle();

                var showItem = new NativeMenuItem("Show Move Mouse");
                showItem.Click += (_, _) => _vm_RequestActivate(this);

                var settingsItem = new NativeMenuItem("Settings");
                settingsItem.Click += (_, _) => ShowSettingsWindow();

                var helpItem = new NativeMenuItem("Help");
                helpItem.Click += (_, _) => LinuxDesktopService.OpenTarget(StaticCode.HelpPageUrl);

                var homeItem = new NativeMenuItem("Home");
                homeItem.Click += (_, _) => LinuxDesktopService.OpenTarget(StaticCode.HomePageUrl);

                var paypalItem = new NativeMenuItem("Donate (PayPal)");
                paypalItem.Click += (_, _) => LinuxDesktopService.OpenTarget(StaticCode.PayPalUrl);

                var twitterItem = new NativeMenuItem("Twitter");
                twitterItem.Click += (_, _) => LinuxDesktopService.OpenTarget(StaticCode.TwitterUrl);

                var exitItem = new NativeMenuItem("Close");
                exitItem.Click += (_, _) => ShutdownApp();

                menu.Items.Add(toggleItem);
                menu.Items.Add(showItem);
                menu.Items.Add(settingsItem);
                menu.Items.Add(new NativeMenuItemSeparator());
                menu.Items.Add(helpItem);
                menu.Items.Add(homeItem);
                menu.Items.Add(paypalItem);
                menu.Items.Add(twitterItem);
                menu.Items.Add(new NativeMenuItemSeparator());
                menu.Items.Add(exitItem);

                _traySingleClickTimer = new DispatcherTimer { Interval = DoubleClickThreshold };
                _traySingleClickTimer.Tick += (_, _) =>
                {
                    _traySingleClickTimer.Stop();
                    _vm_RequestActivate(this);
                };

                _trayIcon = new TrayIcon
                {
                    Icon = _trayIcons[MouseWindowViewModel.MouseState.Idle],
                    ToolTipText = _vm.CurrentState.ToString(),
                    Menu = menu,
                    IsVisible = true
                };
                _trayIcon.Clicked += (_, _) => HandleTrayIconClicked();

                var icons = new TrayIcons { _trayIcon };
                TrayIcon.SetIcons(Application.Current, icons);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        internal void HandleTrayIconClicked()
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.Post(HandleTrayIconClicked);
                return;
            }

            var now = DateTime.UtcNow;
            if (now - _lastTrayClickTime <= DoubleClickThreshold)
            {
                _traySingleClickTimer?.Stop();
                _lastTrayClickTime = DateTime.MinValue;
                _vm?.StartStopToggle();
            }
            else
            {
                _lastTrayClickTime = now;
                _traySingleClickTimer?.Stop();
                _traySingleClickTimer?.Start();
            }
        }

        private void InitializeTimers()
        {
            _countdownTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) };
            _countdownTimer.Tick += (_, _) =>
            {
                if (_vm.CurrentState != MouseWindowViewModel.MouseState.Running || _countdownDuration <= TimeSpan.Zero)
                {
                    _countdownTimer.Stop();
                    return;
                }

                double elapsed = (DateTime.Now - _countdownStart).TotalMilliseconds;
                double pct = Math.Clamp(100.0 * (1.0 - elapsed / _countdownDuration.TotalMilliseconds), 0.0, 100.0);
                CountDownProgressBar.Value = pct;
            };

            _blinkTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(7) };
            _blinkTimer.Tick += (_, _) =>
            {
                if (_vm.CurrentState == MouseWindowViewModel.MouseState.Sleeping)
                {
                    ClosedEyesImage.Opacity = 1.0;
                    _blinkTimer.Interval = TimeSpan.FromSeconds(2);
                    return;
                }

                switch (_blinkPhase)
                {
                    case 0:
                        ClosedEyesImage.Opacity = 1.0;
                        _blinkPhase = 1;
                        _blinkTimer.Interval = TimeSpan.FromMilliseconds(150);
                        break;
                    default:
                        ClosedEyesImage.Opacity = 0.0;
                        _blinkPhase = 0;
                        _blinkTimer.Interval = TimeSpan.FromSeconds(7);
                        break;
                }
            };
            _blinkTimer.Start();

            _flyingAnimTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _flyingAnimTimer.Tick += FlyingAnimTimer_Tick;

            _screenBurnTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(15) };
            _screenBurnTimer.Tick += (_, _) =>
            {
                try
                {
                    if (_vm.SettingsVm.Settings.PreventScreenBurn && !_vm.SettingsVm.Settings.HideMainWindow && !_isPointerOverWindow)
                    {
                        _screenBurnToggle = !_screenBurnToggle;
                        int delta = _screenBurnToggle ? 10 : -10;
                        Position = new PixelPoint(Position.X + delta, Position.Y + delta);
                    }
                }
                catch
                {
                }
            };
            _screenBurnTimer.Start();
        }

        private void FlyingAnimTimer_Tick(object sender, EventArgs e)
        {
            double elapsed = (DateTime.Now - _flyingAnimStart).TotalSeconds;

            if (_flyingFadingOut)
            {
                double t = Math.Clamp(elapsed / 0.5, 0.0, 1.0);
                double opacity = 1.0 - t;
                foreach (var slot in _flyingSlots)
                {
                    slot.Button.Opacity = opacity;
                    if (t >= 1.0) slot.Button.IsHitTestVisible = false;
                }

                if (t >= 1.0) _flyingAnimTimer.Stop();
                return;
            }

            bool allDone = true;
            foreach (var slot in _flyingSlots)
            {
                double localElapsed = elapsed - slot.DelaySec;
                if (localElapsed < 0)
                {
                    allDone = false;
                    continue;
                }

                double t = Math.Clamp(localElapsed / slot.DurationSec, 0.0, 1.0);
                if (t < 1.0) allDone = false;

                double eased = t < 0.5 ? 2 * t * t : 1 - Math.Pow(-2 * t + 2, 2) / 2;
                slot.Button.Opacity = t;
                slot.Button.IsHitTestVisible = true;
                SetSlotAngle(slot.Button, slot.TargetAngle * eased);
            }

            if (allDone) _flyingAnimTimer.Stop();
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Dispatcher.UIThread.Post(ApplyWindowSettings);
        }

        private void _vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MouseWindowViewModel.ForceSystrayIconVisibility))
            {
                Dispatcher.UIThread.Post(UpdateTrayVisibility);
            }
            else if (e.PropertyName == nameof(MouseWindowViewModel.UpdateAvailable))
            {
                Dispatcher.UIThread.Post(ShowUpdateCalloutIfNeeded);
            }
        }

        private void ShowUpdateCalloutIfNeeded()
        {
            if (_vm.UpdateAvailable && _vm.SettingsVm.Settings.MoveMouseHasBeenClicked)
            {
                UpdateCallout.IsVisible = true;
                _updateCalloutTimer?.Stop();
                _updateCalloutTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(12) };
                _updateCalloutTimer.Tick += (_, _) =>
                {
                    UpdateCallout.IsVisible = false;
                    _updateCalloutTimer.Stop();
                };
                _updateCalloutTimer.Start();
            }
        }

        private void ApplyWindowSettings()
        {
            if (_vm == null || _isClosing) return;
            var s = _vm.SettingsVm.Settings;

            Title = s.OverrideWindowTitle && !string.IsNullOrWhiteSpace(s.WindowTitle)
                ? s.WindowTitle
                : "Move Mouse";

            Topmost = !s.HideMainWindow &&
                      (s.Topmost || (s.TopmostWhenRunning && _vm.CurrentState == MouseWindowViewModel.MouseState.Running));

            ShowInTaskbar = !s.HideMainWindow && !s.HideFromTaskbar;

            if (s.HideMainWindow)
            {
                if (IsVisible) Hide();
            }
            else
            {
                if (!IsVisible) Show();
            }

            UpdateTrayVisibility();
        }

        private void UpdateTrayVisibility()
        {
            if (_trayIcon == null || _vm == null) return;
            _trayIcon.IsVisible = !_vm.SettingsVm.Settings.HideSystemTrayIcon || _vm.ForceSystrayIconVisibility;
        }

        private void _vm_RequestNotification(object sender, string title, string message, MouseWindowViewModel.NotificationIcon symbol)
        {
            LinuxDesktopService.ShowNotification(title, message);
        }

        private void _vm_RequestMinimise(object sender)
        {
            Dispatcher.UIThread.Post(() => WindowState = WindowState.Minimized);
        }

        private void _vm_RequestActivate(object sender)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (_isClosing || _vm == null) return;
                if (!_vm.SettingsVm.Settings.HideMainWindow)
                {
                    if (!IsVisible) Show();
                    WindowState = WindowState.Normal;
                    Activate();
                }
            });
        }

        private void _vm_AltTabVisibilityChanged(object sender, bool visible)
        {
            Dispatcher.UIThread.Post(ApplyWindowSettings);
        }

        private void _vm_MouseStateChanged(object sender, MouseWindowViewModel.MouseState state)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (_isClosing || _vm == null) return;
                ApplyWindowSettings();

                if (_trayIcon != null)
                {
                    _trayIcon.ToolTipText = state.ToString();
                    if (_trayIcons.TryGetValue(state, out var icon))
                    {
                        _trayIcon.Icon = icon;
                    }
                }

                ClosedEyesImage.Opacity = state == MouseWindowViewModel.MouseState.Sleeping ? 1.0 : 0.0;

                if (state == MouseWindowViewModel.MouseState.Running)
                {
                    var duration = _vm.ExecutionTime.Subtract(DateTime.Now);
                    _countdownDuration = duration > TimeSpan.Zero ? duration : TimeSpan.FromMilliseconds(1);
                    _countdownStart = DateTime.Now;
                    CountDownProgressBar.Value = 100;
                    _countdownTimer.Start();
                }
                else
                {
                    _countdownTimer.Stop();
                    CountDownProgressBar.Value = 100;
                }
            });
        }

        private void Window_OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            _isPointerOverWindow = true;
            if (e.Source is Visual v && (v.FindAncestorOfType<Button>(true) != null || v.FindAncestorOfType<Controls.CalloutBubble>(true) != null))
            {
                return;
            }

            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
        }

        private void MouseButton_OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            {
                ShowSettingsWindow();
                e.Handled = true;
            }
        }

        private void SettingsButton_OnClick(object sender, RoutedEventArgs e) => ShowSettingsWindow();

        private void ShowSettingsWindow()
        {
            try
            {
                if (_settingsWindow == null || !_settingsWindow.IsVisible)
                {
                    _settingsWindow = new SettingsWindow
                    {
                        DataContext = _vm.SettingsVm
                    };
                    _settingsWindow.Closed += (_, _) => _settingsWindow = null;
                    _settingsWindow.Show();
                }

                _settingsWindow.Activate();
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void PayPalButton_OnClick(object sender, RoutedEventArgs e) => LinuxDesktopService.OpenTarget(StaticCode.PayPalUrl);
        private void HelpButton_OnClick(object sender, RoutedEventArgs e) => LinuxDesktopService.OpenTarget(StaticCode.HelpPageUrl);
        private void TwitterButton_OnClick(object sender, RoutedEventArgs e) => LinuxDesktopService.OpenTarget(StaticCode.TwitterUrl);
        private void CloseButton_OnClick(object sender, RoutedEventArgs e) => ShutdownApp();
        private void MinimiseButton_OnClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void UpdateCallout_OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(StaticCode.UpdateUrl))
            {
                LinuxDesktopService.OpenTarget(StaticCode.UpdateUrl);
            }
        }

        private void MouseGrid_OnPointerEntered(object sender, PointerEventArgs e)
        {
            _isPointerOverWindow = true;
            ShowButtons();
        }

        private void UIElement_OnPointerEntered(object sender, PointerEventArgs e)
        {
            _isPointerOverWindow = true;
            _hideButtons = false;
        }

        private void UIElement_OnPointerExited(object sender, PointerEventArgs e)
        {
            _isPointerOverWindow = false;
            ThreadPool.QueueUserWorkItem(HideButtons);
        }

        private void ShowButtons()
        {
            try
            {
                _hideButtons = false;

                if (!_buttonsOnShow)
                {
                    if (_vm.SettingsVm.Settings.DisableButtonAnimation)
                    {
                        _flyingAnimTimer.Stop();
                        foreach (var slot in _flyingSlots)
                        {
                            slot.Button.Opacity = 1.0;
                            slot.Button.IsHitTestVisible = true;
                            SetSlotAngle(slot.Button, slot.TargetAngle);
                        }
                    }
                    else
                    {
                        _flyingFadingOut = false;
                        _flyingAnimStart = DateTime.Now;
                        foreach (var slot in _flyingSlots)
                        {
                            slot.Button.IsHitTestVisible = true;
                        }
                        _flyingAnimTimer.Start();
                    }
                }

                _buttonsOnShow = true;
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void HideButtons(object stateInfo)
        {
            try
            {
                _hideButtons = true;
                _hideTime = DateTime.Now.Add(_hideTimeDelay);

                lock (_lock)
                {
                    while (_hideButtons && DateTime.Now < _hideTime)
                    {
                        Thread.Sleep(100);
                    }

                    if (_hideButtons)
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            if (_vm.SettingsVm.Settings.DisableButtonAnimation)
                            {
                                _flyingAnimTimer.Stop();
                                foreach (var slot in _flyingSlots)
                                {
                                    slot.Button.Opacity = 0.0;
                                    slot.Button.IsHitTestVisible = false;
                                }
                            }
                            else
                            {
                                _flyingFadingOut = true;
                                _flyingAnimStart = DateTime.Now;
                                _flyingAnimTimer.Start();
                            }
                        });
                        _buttonsOnShow = false;
                    }
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void ShutdownApp()
        {
            try
            {
                _isClosing = true;
                _settingsWindow?.Close();
                _trayIcon?.Dispose();
                _vm?.Dispose();
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    desktop.Shutdown();
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void MouseWindow_OnClosing(object sender, WindowClosingEventArgs e)
        {
            _isClosing = true;
            try
            {
                if (_vm != null)
                {
                    _vm.MouseStateChanged -= _vm_MouseStateChanged;
                    _vm.AltTabVisibilityChanged -= _vm_AltTabVisibilityChanged;
                    _vm.RequestActivate -= _vm_RequestActivate;
                    _vm.RequestMinimise -= _vm_RequestMinimise;
                    _vm.RequestNotification -= _vm_RequestNotification;
                    _vm.PropertyChanged -= _vm_PropertyChanged;
                    _vm.SettingsVm.Settings.PropertyChanged -= Settings_PropertyChanged;
                }
                _traySingleClickTimer?.Stop();
                _countdownTimer?.Stop();
                _blinkTimer?.Stop();
                _flyingAnimTimer?.Stop();
                _screenBurnTimer?.Stop();
                _updateCalloutTimer?.Stop();
                _settingsWindow?.Close();
                _trayIcon?.Dispose();
                _vm?.Dispose();
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }
    }
}
