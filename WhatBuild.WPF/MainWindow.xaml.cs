using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
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
        private CancellationTokenSource CancelTokenSource { get; set; }

        // Build sources
        private IBuildSource OPGG { get; set; }

        // Binded UI 
        public string AppVersion
        {
            get
            {
                // Returns AssemblyInformationalVersion 
                Assembly assembly = Assembly.GetEntryAssembly();
                return FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;
            }
        }
        public bool IsCheckedRemoveOutdated { get; set; } = true;
        public bool IsCheckedShowSkillOrders { get; set; } = true;
        public bool IsCheckedSourceOPGG { get; set; } = true;
        public bool IsCheckedDownloadAramBuilds { get; set; } = true;

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

            // Check if new update
            CheckToUpdateItemSets();

            Mouse.OverrideCursor = null;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void btnImport_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateAndImportLoLPath())
            {
                return;
            }

            Reset();

            if (IsCheckedRemoveOutdated)
            {
                DeleteItemSets();
            }

            // Show progress UI
            grpMetadata.Visibility = Visibility.Collapsed;
            grpProgress.Visibility = Visibility.Visible;
            pbProgress.Visibility = Visibility.Visible;

            ToggleUIImport();

            await ImportItemSetsAsync();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CancelTokenSource.Cancel();

            Log("Cancelling...");
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            btnDelete.IsEnabled = false;

            if (!ValidateAndImportLoLPath())
            {
                return;
            }

            grpMetadata.Visibility = Visibility.Collapsed;
            grpProgress.Visibility = Visibility.Visible;

            Reset();
            DeleteItemSets();

            // Update local item set to null
            Properties.Settings.Default.LocalItemsVersion = null;
            Properties.Settings.Default.Save();

            btnDelete.IsEnabled = true;
        }

        private void txtLoLDirectory_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            AskUserToImportLoLPath();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true
            });

            e.Handled = true;
        }

        #region UI Helpers
        private async Task ShowAllVersionsAsync()
        {
            // TODO: Find better way to fetch version?
            OPGG = new OPGG();
            Task<LoLStore> taskLoLStore = StoreManager<LoLStore>.GetAsync();
            Task taskInitOpgg = OPGG.InitAsync("annie");

            await Task.WhenAll(taskLoLStore, taskInitOpgg);

            // Local version
            string localItemsVersion = string.IsNullOrEmpty(Properties.Settings.Default.LocalItemsVersion)
                ? "None"
                : Properties.Settings.Default.LocalItemsVersion;

            if (grpMetadata.Visibility == Visibility.Visible)
            {
                lblLoLVersion.Content = $"LoL version: {taskLoLStore.Result.Version}";
                lblOPGGVersion.Content = $"OP.GG version: {OPGG.GetVersion()}";
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

        private void ShowInfoMessageBox(string title, string message)
        {
            System.Windows.Forms.MessageBox.Show(
                message,
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private DialogResult ShowWarningMessageBox(string title, string message)
        {
            return System.Windows.Forms.MessageBox.Show(
                    message,
                    title,
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning);
        }

        #endregion

        #region Helpers
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

        private void DeleteItemSets()
        {
            Log("Deleting existing item sets...");

            string appPrefix = Properties.Settings.Default.AppPrefixName;
            string lolDirectory = Properties.Settings.Default.LoLDirectory;
            LoLPathUtil.DeleteItemSets(lolDirectory, appPrefix);

            Log("Finished deleting!");
        }

        private async Task ImportItemSetsAsync()
        {

            // Fetch configuration 
            var config = new ConfigurationViewModel
            {
                ApplicationPrefixName = Properties.Settings.Default.AppPrefixName,
                LoLDirectory = Properties.Settings.Default.LoLDirectory,
                RemoveOutdatedItems = IsCheckedRemoveOutdated,
                ShowSkillsOrder = IsCheckedShowSkillOrders,
                DownloadAramBuilds = IsCheckedDownloadAramBuilds
            };

            // Start generation
            if (IsCheckedSourceOPGG)
            {
                var opggGenerator = new ItemSetGenerator<OPGG>(config, Log, UpdateProgressBar);

                try
                {
                    List<ChampionViewModel> failedChampions = await opggGenerator.GenerateItemSetForAllChampionsAsync(CancelTokenSource.Token);

                    // Update local version with OPGG version for now
                    Properties.Settings.Default.LocalItemsVersion = OPGG.GetVersion();
                    Properties.Settings.Default.Save();

                    string msgInfoBox = "Your item sets have been imported.";
                    if (failedChampions.Count > 0)
                    {
                        msgInfoBox += "\nHowever, these champions didn't do really well:";
                        failedChampions.ForEach(champion => msgInfoBox += $"\n-> {champion.Name}");
                    }

                    ShowInfoMessageBox("Import completed", msgInfoBox);
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

        private void Reset()
        {
            CancelTokenSource = new CancellationTokenSource();

            tbLogs.Text = "";
        }

        private bool AskUserToImportLoLPath()
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
                return true;
            }

            // Cancelled or invalid 
            return false;
        }

        private bool ValidateAndImportLoLPath()
        {
            while (!LoLPathUtil.IsValidLeagueOfLegendsDirectory(Properties.Settings.Default.LoLDirectory))
            {
                string errMsg = "Your League of Legends path seems invalid.\n";
                errMsg += "Please set a valid path before proceeding.";

                DialogResult result = ShowWarningMessageBox("Invalid PLeague of Legends Path", errMsg);

                if (result == System.Windows.Forms.DialogResult.OK || result == System.Windows.Forms.DialogResult.Yes)
                {
                    bool importCompleted = AskUserToImportLoLPath();
                    if (!importCompleted)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private void CheckToUpdateItemSets()
        {
            // For now, only check for OPGG until we support more build sources
            string opggVersion = OPGG.GetVersion();
            string localVersion = Properties.Settings.Default.LocalItemsVersion;

            bool shouldUpdate = VersionUtil.ShouldUpdateItemSet(opggVersion, localVersion);

            if (shouldUpdate)
            {
                ShowInfoMessageBox("Outdated item sets", "Your item sets are outdated compared to current OP.GG version.");
            }
        }
        #endregion
    }
}
