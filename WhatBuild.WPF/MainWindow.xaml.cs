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
using WhatBuild.Core.Utils;

namespace WhatBuild.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            FillFormsWithUserSettings();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void FillFormsWithUserSettings()
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

        #region Helpers
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
