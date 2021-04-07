using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
using WhatBuild.Core.BuildSources;
using WhatBuild.Core.Interfaces;
using WhatBuild.Core.Stores;
using WhatBuild.Core.Utils;

namespace WhatBuild.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Only op.gg will be supported for first versions
        private IBuildSource opgg = new OPGG();

        public MainWindow()
        {
            InitializeComponent();

            FillFormsWithUserSettings();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await ShowAllVersionsAsync();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void btnImport_Click(object sender, RoutedEventArgs e)
        {
            grpMetadata.Visibility = Visibility.Collapsed;
            grpProgress.Visibility = Visibility.Visible;
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
                bool isValidDirectory = PathUtil.IsValidLeagueOfLegendsDirectory(path);
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

        private void FillFormsWithUserSettings()
        {
            string lolDirectory = Properties.Settings.Default.LoLDirectory;

            txtLoLDirectory.Text = lolDirectory;

            // Check if LoL directory is valid
            if (PathUtil.IsValidLeagueOfLegendsDirectory(lolDirectory))
            {
                ChangeLoLStatusToFound();
            }
            else
            {
                ChangeLoLStatusToNotFound();
            }
        }

        #region Helpers
        private async Task ShowAllVersionsAsync()
        {
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
        #endregion
    }
}
