using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Xml.Linq;
using Button = System.Windows.Controls.Button;
using Label = System.Windows.Controls.Label;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using RadioButton = System.Windows.Controls.RadioButton;

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
            var openFileDialog = new OpenFileDialog
            {
                Filter = "OIV files (*.oiv)|*.oiv"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                _oivFilePath = openFileDialog.FileName;
                oivFilePathLabel.Content = _oivFilePath;
            }
        }

        private void BrowseGameDirectory_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                _gameDirectory = dialog.SelectedPath;
                gameDirectoryPathLabel.Content = _gameDirectory;
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
                MessageBox.Show("Installation completed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                errorLabel.Content = $"Installation failed: {ex.Message}";
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

            foreach (var element in LayoutRoot.Children)
            {
                if (element is Label label)
                {
                    label.FontSize = fontSize;
                }
                else if (element is Button button)
                {
                    button.FontSize = fontSize;
                }
                else if (element is RadioButton radioButton)
                {
                    radioButton.FontSize = fontSize;
                }
            }
        }
    }
}