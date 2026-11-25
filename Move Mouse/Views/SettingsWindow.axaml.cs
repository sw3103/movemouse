using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ellabi.Actions;
using ellabi.Platform;
using ellabi.Schedules;
using ellabi.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ellabi.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private SettingsWindowViewModel ViewModel => DataContext as SettingsWindowViewModel;

        private void SettingsWindow_OnOpened(object sender, EventArgs e)
        {
            try
            {
                ViewModel?.StartSystemIdleTimer();
                ViewModel?.RefreshStartupTask();
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void SettingsWindow_OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            try
            {
                if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && e.Source is not Control { Focusable: true })
                {
                    BeginMoveDrag(e);
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e) => Close();

        private void AddActionButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel != null && !ViewModel.Settings.ActionsHaveBeenClicked)
                {
                    ViewModel.Settings.ActionsHaveBeenClicked = true;
                }

                if (sender is Control btn && btn.ContextMenu != null)
                {
                    btn.ContextMenu.Open(btn);
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void AddScheduleButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is Control btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.Open(btn);
            }
        }

        private void AddKeystrokeButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is not Control btn || ViewModel == null) return;

                var menu = new ContextMenu { MaxHeight = 300 };
                var items = new List<MenuItem>();

                foreach (var kv in StaticCode.VirtualKeys.Value)
                {
                    int vk = kv.Key;
                    var item = new MenuItem
                    {
                        Header = $"{kv.Value.Key} — {kv.Value.Value}",
                        Tag = vk
                    };
                    ToolTip.SetTip(item, kv.Value.Value);
                    item.Click += (_, _) => ViewModel.AddKeystroke(vk);
                    items.Add(item);
                }

                menu.ItemsSource = items;
                menu.Open(btn);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void AddMoveMouseCursorAction_OnClick(object sender, RoutedEventArgs e) => ViewModel?.AddAction(typeof(MoveMouseCursorAction));
        private void AddClickMouseAction_OnClick(object sender, RoutedEventArgs e) => ViewModel?.AddAction(typeof(ClickMouseAction));
        private void AddScrollMouseAction_OnClick(object sender, RoutedEventArgs e) => ViewModel?.AddAction(typeof(ScrollMouseAction));
        private void AddPositionMouseCursorAction_OnClick(object sender, RoutedEventArgs e) => ViewModel?.AddAction(typeof(PositionMouseCursorAction));
        private void AddKeystrokeAction_OnClick(object sender, RoutedEventArgs e) => ViewModel?.AddAction(typeof(KeystrokeAction));
        private void ActivateApplicationAction_OnClick(object sender, RoutedEventArgs e) => ViewModel?.AddAction(typeof(ActivateApplicationAction));
        private void AddCommandAction_OnClick(object sender, RoutedEventArgs e) => ViewModel?.AddAction(typeof(CommandAction));
        private void AddScriptAction_OnClick(object sender, RoutedEventArgs e) => ViewModel?.AddAction(typeof(ScriptAction));
        private void AddSleepAction_OnClick(object sender, RoutedEventArgs e) => ViewModel?.AddAction(typeof(SleepAction));

        private void AddSimpleSchedule_OnClick(object sender, RoutedEventArgs e) => ViewModel?.AddSchedule(typeof(SimpleSchedule));
        private void AddAdvancedSchedule_OnClick(object sender, RoutedEventArgs e) => ViewModel?.AddSchedule(typeof(AdvancedSchedule));

        private async void CommandPathBrowseButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Select Program",
                    AllowMultiple = false
                });

                var path = files.FirstOrDefault()?.TryGetLocalPath();
                if (!string.IsNullOrEmpty(path) && ViewModel?.SelectedAction is CommandAction cmd)
                {
                    cmd.FilePath = path;
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private async void ScriptPathBrowseButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Open Script",
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {
                        new FilePickerFileType("Supported Scripts") { Patterns = new[] { "*.sh", "*.py", "*.ps1" } },
                        FilePickerFileTypes.All
                    }
                });

                var path = files.FirstOrDefault()?.TryGetLocalPath();
                if (!string.IsNullOrEmpty(path) && ViewModel?.SelectedAction is ScriptAction script)
                {
                    script.ScriptPath = path;
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void CronHelpButton_OnClick(object sender, RoutedEventArgs e) => LinuxDesktopService.OpenTarget(StaticCode.CronHelpUrl);
        private void HomeLink_OnPointerPressed(object sender, PointerPressedEventArgs e) => LinuxDesktopService.OpenTarget(StaticCode.HomePageUrl);
        private void TwitterLink_OnPointerPressed(object sender, PointerPressedEventArgs e) => LinuxDesktopService.OpenTarget(StaticCode.TwitterUrl);
        private void PayPalLink_OnPointerPressed(object sender, PointerPressedEventArgs e) => LinuxDesktopService.OpenTarget(StaticCode.PayPalUrl);
        private void GitHubLink_OnPointerPressed(object sender, PointerPressedEventArgs e) => LinuxDesktopService.OpenTarget(StaticCode.GitHubUrl);

        private async void ContactLink_OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            try
            {
                bool visitWiki = await MessageDialog.ShowYesNoAsync(
                    this,
                    "Visit Troubleshooting Page",
                    "If you are having issues with Move Mouse or have any questions, you may find the Troubleshooting section in the Wiki useful.\n\nWould you like to visit the Wiki now?");

                LinuxDesktopService.OpenTarget(visitWiki ? StaticCode.TroubleshootingUrl : StaticCode.ContactMailToAddress);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void OpenLogButton_Click(object sender, RoutedEventArgs e) => LinuxDesktopService.OpenTarget(StaticCode.LogPath);
        private void FollowLogButton_Click(object sender, RoutedEventArgs e) => LinuxDesktopService.FollowLog(StaticCode.LogPath);

        private void SettingsWindow_Closing(object sender, WindowClosingEventArgs e)
        {
            try
            {
                ViewModel?.StopSystemIdleTimer();
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }
    }
}
