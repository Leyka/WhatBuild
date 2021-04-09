using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WhatBuild.Core;
using WhatBuild.Core.BuildSources;
using WhatBuild.Core.Interfaces;
using WhatBuild.Core.Stores;
using WhatBuild.Core.Utils;
using WhatBuild.Core.ViewModels;
using WhatBuild.WPF.Utils;

namespace WhatBuild.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public CancellationTokenSource CancelTokenSource { get; set; }

        public string AppVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public bool IsCheckedRemoveOutdated { get; set; } = true;
        public bool IsCheckedShowSkillOrders { get; set; } = true;
        public bool IsCheckedSourceOPGG { get; set; } = true;

        public MainWindow()
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.AppStarting;

            CancelTokenSource = new CancellationTokenSource();

            // UI
            InitializeComponent();
            FillFormsWithUserSettings();

            // Bind DataContext to self
            DataContext = this;
        }

        #region Init
        private void FillFormsWithUserSettings()
        {
            string lolDirectory = Properties.Settings.Default.LoLDirectory;

            txtLoLDirectory.Text = lolDirectory;

            // Check if LoL directory is valid
            if (LoLPathUtil.IsValidLeagueOfLegendsDirectory(lolDirectory))
            {
                ChangeLoLStatusToFound();
            }
            else
            {
                ChangeLoLStatusToNotFound();
            }
        }
        #endregion

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await ShowAllVersionsAsync();
            Mouse.OverrideCursor = null;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void btnImport_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Validate path
            string lolDirectory = Properties.Settings.Default.LoLDirectory;

            Reset();

            // Show progress UI
            grpMetadata.Visibility = Visibility.Collapsed;
            grpProgress.Visibility = Visibility.Visible;
            pbProgress.Visibility = Visibility.Visible;

            ToggleUIImport();

            // Fetch configuration 
            var config = new ConfigurationViewModel
            {
                ApplicationPrefixName = Properties.Settings.Default.AppPrefixName,
                LoLDirectory = lolDirectory,
                RemoveOutdatedItems = IsCheckedRemoveOutdated,
                ShowSkillsOrder = IsCheckedShowSkillOrders
            };

            // Start generation
            if (IsCheckedSourceOPGG)
            {
                var opggGenerator = new ItemSetGenerator<OPGG>(config, Log, UpdateProgressBar);

                try
                {
                    await opggGenerator.GenerateItemSetForAllChampionsAsync(CancelTokenSource.Token);
                }
                catch (OperationCanceledException ex) when (ex.CancellationToken == CancelTokenSource.Token)
                {
                    // Log that operation has been cancelled
                    Mouse.OverrideCursor = null;
                    Log("Cancelled");
                }
                finally
                {
                    ToggleUIImport();
                    pbProgress.Value = 0;
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CancelTokenSource.Cancel();

            Log("Cancelling...");
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        }

        private void txtLoLDirectory_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Ask user to browse League of Legends directory
            var folderBrowserDialog = new FolderBrowserDialog();

            DialogResult result = folderBrowserDialog.ShowDialog();

            bool hasContent = result == System.Windows.Forms.DialogResult.OK
                && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath);

            if (hasContent)
            {
                string path = folderBrowserDialog.SelectedPath;
                txtLoLDirectory.Text = path;

                // Validate path
                bool isValidDirectory = LoLPathUtil.IsValidLeagueOfLegendsDirectory(path);
                Properties.Settings.Default.LoLDirectory = path;

                if (isValidDirectory)
                {
                    ChangeLoLStatusToFound();
                }
                else
                {
                    ChangeLoLStatusToNotFound();
                }

                Properties.Settings.Default.Save();
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true
            });

            e.Handled = true;
        }

        #region Helpers
        private async Task ShowAllVersionsAsync()
        {
            // TODO: Find better way to fetch version?
            IBuildSource opgg = new OPGG();
            Task<LoLStore> taskLoLStore = StoreManager<LoLStore>.GetAsync();
            Task taskInitOpgg = opgg.InitAsync("annie");

            await Task.WhenAll(taskLoLStore, taskInitOpgg);

            // Local version
            string localItemsVersion = string.IsNullOrEmpty(Properties.Settings.Default.LocalItemsVersion)
                ? "None"
                : Properties.Settings.Default.LocalItemsVersion;

            if (grpMetadata.Visibility == Visibility.Visible)
            {
                lblLoLVersion.Content = $"LoL version: {taskLoLStore.Result.Version}";
                lblOPGGVersion.Content = $"op.gg version: {opgg.GetVersion()}";
                lblLocalItemsVersion.Content = $"Local items version: {localItemsVersion}";
            }
        }

        private void ToggleUIImport()
        {
            WPFUtil.ToggleVisibility(btnImport);
            WPFUtil.ToggleVisibility(btnCancel);

            btnDelete.IsEnabled = !btnDelete.IsEnabled;
        }

        private void ChangeLoLStatusToFound()
        {
            lblLoLDirectoryStatus.Content = "League of Legends found";
            lblLoLDirectoryStatus.Foreground = Brushes.DarkGreen;
        }

        private void ChangeLoLStatusToNotFound()
        {
            lblLoLDirectoryStatus.Content = "League of Legends not found";
            lblLoLDirectoryStatus.Foreground = Brushes.DarkRed;
        }

        private void Log(string message)
        {
            Dispatcher.Invoke(() =>
            {
                tbLogs.Text += message + Environment.NewLine;
                svLogs.ScrollToBottom();
            });
        }

        private void UpdateProgressBar(double progress)
        {
            Dispatcher.Invoke(() =>
            {
                // Only update progress if value bigger due to threads race condition
                if (progress > pbProgress.Value)
                {
                    pbProgress.Value = progress;
                }
            });
        }

        private void Reset()
        {
            CancelTokenSource = new CancellationTokenSource();

            tbLogs.Text = "";
        }
        #endregion
    }
}
