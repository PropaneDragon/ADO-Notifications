using ADO_Notifications.ADO;
using ADO_Notifications.Properties;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ADO_Notifications
{
    public partial class MainWindow : Window
    {
        private readonly string _assemblyPath = Process.GetCurrentProcess().MainModule.FileName;
        private RegistryKey _startupKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        private bool StartWithWindows { get => _startupKey.GetValue(_assemblyPath) != null; set { if (value) { _startupKey?.SetValue(Title, $"\"{_assemblyPath}\""); } else { _startupKey?.DeleteValue(_assemblyPath, false); } } }

        public MainWindow()
        {
            InitializeComponent();
        }

        private async Task LoadSettings()
        {
            Checkbox_NotifyOnNewPullRequests.IsChecked = Settings.Default.NotifyOnNewPR;
            Checkbox_NotifyOnNewPullRequestsDrafts.IsChecked = Settings.Default.NotifyOnNewPRIncludeDraft;
            Checkbox_PullRequestReminder.IsChecked = Settings.Default.RemindAboutUnreviewedPRs;
            Checkbox_NotifyOnNewBuilds.IsChecked = Settings.Default.NotifyOnNewBuilds;
            Checkbox_NotifyOnCompleteBuilds.IsChecked = Settings.Default.NotifyOnCompletedBuilds;
            Checkbox_NotifyOnBuildStatusChanges.IsChecked = Settings.Default.NotifyOnBuildStatusChanges;
            Checkbox_NotifyOnSucceededBuilds.IsChecked = Settings.Default.NotifyOnCompletedSuccessfulBuilds;
            Checkbox_NotifyOnFailedBuilds.IsChecked = Settings.Default.NotifyOnCompletedFailedBuilds;
            Checkbox_NotifyOnUpdatedPullRequests.IsChecked = Settings.Default.NotifyOnUpdatedPullRequests;
            Checkbox_StartMinimised.IsChecked = Settings.Default.StartMinimised;
            Checkbox_MinimiseToTray.IsChecked = Settings.Default.MinimiseToTray;
            Checkbox_StartWithWindows.IsChecked = StartWithWindows;
            Checkbox_NotifyWhenAddedAsReviewer.IsChecked = Settings.Default.NotifyWhenAddedAsReviewer;

            Text_AccessToken.Text = Settings.Default.AccessToken;
            Text_PullRequestReminderMinutes.Text = Settings.Default.RemindAboutUnreviewedPRIntervalMinutes.ToString();
        }

        private async Task SaveSettings()
        {
            Settings.Default.NotifyOnNewPR = Checkbox_NotifyOnNewPullRequests.IsChecked ?? false;
            Settings.Default.NotifyOnNewPRIncludeDraft = Checkbox_NotifyOnNewPullRequestsDrafts.IsChecked ?? false;
            Settings.Default.RemindAboutUnreviewedPRs = Checkbox_PullRequestReminder.IsChecked ?? false;
            Settings.Default.NotifyOnNewBuilds = Checkbox_NotifyOnNewBuilds.IsChecked ?? false;
            Settings.Default.NotifyOnCompletedBuilds = Checkbox_NotifyOnCompleteBuilds.IsChecked ?? false;
            Settings.Default.NotifyOnBuildStatusChanges = Checkbox_NotifyOnBuildStatusChanges.IsChecked ?? false;
            Settings.Default.NotifyOnCompletedSuccessfulBuilds = Checkbox_NotifyOnSucceededBuilds.IsChecked ?? false;
            Settings.Default.NotifyOnCompletedFailedBuilds = Checkbox_NotifyOnFailedBuilds.IsChecked ?? false;
            Settings.Default.NotifyOnUpdatedPullRequests = Checkbox_NotifyOnUpdatedPullRequests.IsChecked ?? false;
            Settings.Default.StartMinimised = Checkbox_StartMinimised.IsChecked ?? false;
            Settings.Default.MinimiseToTray = Checkbox_MinimiseToTray.IsChecked ?? false;
            Settings.Default.NotifyWhenAddedAsReviewer = Checkbox_NotifyWhenAddedAsReviewer.IsChecked ?? false;
            Settings.Default.AccessToken = Text_AccessToken.Text;

            StartWithWindows = Checkbox_StartWithWindows.IsChecked ?? false;

            if (int.TryParse(Text_PullRequestReminderMinutes.Text, out var PullRequestReminderMinutes))
            {
                Settings.Default.RemindAboutUnreviewedPRIntervalMinutes = PullRequestReminderMinutes;
            }

            try
            {
                if (!await ConnectionHolder.SetCredentialsAsync(new VssBasicCredential(string.Empty, Settings.Default.AccessToken)))
                {
                    _ = MessageBox.Show("Credentials provided appear to be invalid");
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Credentials provided appear to be invalid.\n{ex.Message}");
            }

            Settings.Default.Save();
        }

        private void UnhideWindow()
        {
            if (!IsVisible)
            {
                Show();
            }

            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }

            _ = Activate();
            Topmost = true;
            Topmost = false;
            _ = Focus();
        }

        private void CollapseWindowIfNeeded()
        {
            if (WindowState == WindowState.Minimized && Settings.Default.MinimiseToTray)
            {
                Visibility = Visibility.Collapsed;
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.StartMinimised)
            {
                WindowState = WindowState.Minimized;
            }

            await LoadSettings();

            var initialiser = new Initialiser();
            await initialiser.InitialiseAsync();
        }

        private async void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).IsEnabled = false;

            await SaveSettings();

            (sender as Button).IsEnabled = true;
        }

        private void TaskBarIcon_Main_TrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            UnhideWindow();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            CollapseWindowIfNeeded();
        }

        private void MenuItem_Settings_Click(object sender, RoutedEventArgs e)
        {
            UnhideWindow();
        }

        private void MenuItem_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
