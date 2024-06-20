using Microsoft.Win32;
using System;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Xml.Linq;

namespace GTA_V___Mega_Map___Redux_Installer
{
    public partial class GTAVMegaMapReduxInstaller : Window
    {
        private string _oivFilePath;
        private string _gameDirectory;

        public GTAVMegaMapReduxInstaller()
        {
            InitializeComponent();
        }

        private void BrowseOivFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "OIV files (*.oiv)|*.oiv"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                _oivFilePath = openFileDialog.FileName;
            }
        }

        private void BrowseGameDirectory_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                _gameDirectory = dialog.SelectedPath;
            }
        }

        private void Install_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_oivFilePath) || string.IsNullOrEmpty(_gameDirectory))
            {
                errorLabel.Content = "Both OIV file and game directory must be selected.";
                return;
            }

            bool installToMods = installToModsRadio.IsChecked ?? false;
            try
            {
                InstallMod(_oivFilePath, _gameDirectory, installToMods);
                System.Windows.MessageBox.Show("Installation completed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Installation failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InstallMod(string oivFilePath, string gameDirectory, bool installToMods)
        {
            string extractPath = Path.Combine(Path.GetTempPath(), "temp_oiv_extracted");

            // Extract OIV file
            if (Directory.Exists(extractPath))
                Directory.Delete(extractPath, true);

            ZipFile.ExtractToDirectory(oivFilePath, extractPath);

            // Parse assembly.xml
            var assemblyXmlPath = Path.Combine(extractPath, "assembly.xml");
            if (!File.Exists(assemblyXmlPath))
                throw new FileNotFoundException("assembly.xml not found in OIV file.");

            var xdoc = XDocument.Load(assemblyXmlPath);
            var items = xdoc.Descendants("Item");

            string targetDirectory = installToMods ? Path.Combine(gameDirectory, "mods") : gameDirectory;

            foreach (var item in items)
            {
                string source = item.Element("source").Value;
                string destination = item.Element("destination").Value;

                string sourcePath = Path.Combine(extractPath, source);
                string destPath = Path.Combine(targetDirectory, destination);

                Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                File.Copy(sourcePath, destPath, true);
            }

            // Cleanup
            Directory.Delete(extractPath, true);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double currentHeight = e.NewSize.Height;
            double currentWidth = e.NewSize.Width;

            double fontSize = Math.Min(currentHeight, currentWidth) * 0.03; // 3% of the smaller dimension
            foreach (var child in LogicalTreeHelper.GetChildren(this))
            {
                if (child is FrameworkElement element)
                {
                    element.FontSize = fontSize;
                }
            }
        }
    }
}