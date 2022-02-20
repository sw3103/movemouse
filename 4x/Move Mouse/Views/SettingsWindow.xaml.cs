using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using ellabi.Actions;
using ellabi.Schedules;
using ellabi.ViewModels;
using Microsoft.Win32;

namespace ellabi.Views
{
    public partial class SettingsWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void SettingsWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ((SettingsWindowViewModel)DataContext).RefreshStartupTask();
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void SettingsWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(e.ToString());

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

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);
            Close();
        }

        private void AddMoveMouseCursorAction_OnClick(object sender, RoutedEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                ((SettingsWindowViewModel)DataContext).AddAction(typeof(MoveMouseCursorAction));
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void AddClickMouseAction_OnClick(object sender, RoutedEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                ((SettingsWindowViewModel)DataContext).AddAction(typeof(ClickMouseAction));
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void AddScrollMouseAction_OnClick(object sender, RoutedEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                ((SettingsWindowViewModel)DataContext).AddAction(typeof(ScrollMouseAction));
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void AddPositionMouseCursorAction_OnClick(object sender, RoutedEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                ((SettingsWindowViewModel)DataContext).AddAction(typeof(PositionMouseCursorAction));
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void ActivateApplicationAction_OnClick(object sender, RoutedEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                ((SettingsWindowViewModel)DataContext).AddAction(typeof(ActivateApplicationAction));
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void AddCommandAction_OnClick(object sender, RoutedEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                ((SettingsWindowViewModel)DataContext).AddAction(typeof(CommandAction));
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void AddScriptAction_OnClick(object sender, RoutedEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                ((SettingsWindowViewModel)DataContext).AddAction(typeof(ScriptAction));
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void AddSleepAction_OnClick(object sender, RoutedEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                ((SettingsWindowViewModel)DataContext).AddAction(typeof(SleepAction));
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void CommandPathBrowseButton_OnClick(object sender, RoutedEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                var ofd = new OpenFileDialog();
                var result = ofd.ShowDialog();

                if (result.HasValue && result.Value)
                {
                    ((CommandAction)((SettingsWindowViewModel)DataContext).SelectedAction).FilePath = ofd.FileName;
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        //private void IconPathBrowseButton_OnClick(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        var ofd = new OpenFileDialog();
        //        var result = ofd.ShowDialog();

        //        if (result.HasValue && result.Value)
        //        {
        //            ((SettingsWindowViewModel) DataContext).Settings.IconPath = ofd.FileName;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        StaticCode.Logger?.Here().Error(ex.Message);
        //    }
        //}

        private void AddSimpleSchedule_OnClick(object sender, RoutedEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                ((SettingsWindowViewModel)DataContext).AddSchedule(typeof(SimpleSchedule));
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void AddAdvancedSchedule_OnClick(object sender, RoutedEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                ((SettingsWindowViewModel)DataContext).AddSchedule(typeof(AdvancedSchedule));
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void ScriptPathBrowseButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var ofd = new OpenFileDialog
                {
                    Title = "Open PowerShell Script",
                    Filter = "PowerShell Script (*.ps1)|*.ps1"
                };
                var result = ofd.ShowDialog();

                if (result.HasValue && result.Value)
                {
                    StaticCode.Logger?.Here().Debug(result.ToString());
                    ((ScriptAction)((SettingsWindowViewModel)DataContext).SelectedAction).ScriptPath = ofd.FileName;
                }
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void CronHelpButton_OnClick(object sender, RoutedEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                Process.Start(StaticCode.CronHelpUrl);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void AddActionButton_OnClick(object sender, RoutedEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                if (!((SettingsWindowViewModel)DataContext).Settings.ActionsHaveBeenClicked) ((SettingsWindowViewModel)DataContext).Settings.ActionsHaveBeenClicked = true;
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void HomeLink_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                Process.Start(StaticCode.HomePageUrl);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void ContactLink_OnMouseDown(object sender, MouseButtonEventArgs e)
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

        private void TwitterLink_OnMouseDown(object sender, MouseButtonEventArgs e)
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

        private void PayPalLink_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                Process.Start(StaticCode.PayPalUrl);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void GitHubLink_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                Process.Start(StaticCode.GitHubUrl);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void OpenLogButton_Click(object sender, RoutedEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                Process.Start(StaticCode.LogPath);
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }

        private void FollowLogButton_Click(object sender, RoutedEventArgs e)
        {
            StaticCode.Logger?.Here().Debug(String.Empty);

            try
            {
                var powershell = new Process();
                powershell.StartInfo.FileName = "powershell";
                powershell.StartInfo.Arguments = $"-ExecutionPolicy Bypass -Command \"$Host.UI.RawUI.WindowTitle = 'Move Mouse Log';Get-Content -Path:'{StaticCode.LogPath}' -Tail:500 -Wait";
                powershell.Start();
            }
            catch (Exception ex)
            {
                StaticCode.Logger?.Here().Error(ex.Message);
            }
        }
    }
}