using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml.Linq;
using Button = System.Windows.Controls.Button;
using Label = System.Windows.Controls.Label;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using RadioButton = System.Windows.Controls.RadioButton;

namespace GTA_V___Mega_Map___Redux_Installer
{
    public partial class GTAVMegaMapReduxInstaller : Window, INotifyPropertyChanged
    {
        private string _oivFilePath;
        private string _gameDirectory;
        private bool _isInstalling;

        public bool IsInstalling
        {
            get => _isInstalling;
            set
            {
                _isInstalling = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public GTAVMegaMapReduxInstaller()
        {
            InitializeComponent();
            DataContext = this;
            LoadGameDirectory();
        }

        private void LoadGameDirectory()
        {
            // Load game directory from environment variable or appsettings.json
            _gameDirectory = Environment.GetEnvironmentVariable("GTAVGameDirectory", EnvironmentVariableTarget.User);

            if (!string.IsNullOrEmpty(_gameDirectory))
            {
                gameDirectoryPathLabel.Content = _gameDirectory;
            }
            else
            {
                // MessageBox.Show("Game directory not set. Please set the GameDirectory environment variable.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Environment.Exit(1); // Optionally, handle this error gracefully
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
            var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                _gameDirectory = dialog.SelectedPath;
                gameDirectoryPathLabel.Content = _gameDirectory;

                Environment.SetEnvironmentVariable("GTAVGameDirectory", _gameDirectory, EnvironmentVariableTarget.User);
            }
        }

        private async void Install_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_oivFilePath) || string.IsNullOrEmpty(_gameDirectory))
            {
                errorLabel.Content = "Both OIV file and game directory must be selected.";
                return;
            }

            bool installToMods = installToModsRadio.IsChecked ?? false;
            IsInstalling = true;

            try
            {
                await Task.Run(() => InstallMod(_oivFilePath, _gameDirectory, installToMods));
                MessageBox.Show($"Installation of { Path.GetFileName(_oivFilePath) } completed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                errorLabel.Content = $"Installation failed: { ex.Message }";
            }
            finally
            {
                IsInstalling = false;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            IsInstalling = false;
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

            int totalItems = items.Count();
            int processedItems = 0;

            foreach (var item in items)
            {
                if (!IsInstalling) // Check for cancellation
                {
                    throw new OperationCanceledException("Installation was cancelled.");
                }

                string source = item.Element("source").Value;
                string destination = item.Element("destination").Value;

                string sourcePath = Path.Combine(extractPath, source);
                string destPath = Path.Combine(targetDirectory, destination);

                Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                File.Copy(sourcePath, destPath, true);

                processedItems++;
                UpdateProgressBar(processedItems, totalItems);
            }

            // Cleanup
            Directory.Delete(extractPath, true);
        }

        private void UpdateProgressBar(int processedItems, int totalItems)
        {
            Dispatcher.Invoke(() =>
            {
                installationProgressBar.Value = (double)processedItems / totalItems * 100;
            });
        }

        private void EnableControls(bool enable)
        {
            Dispatcher.Invoke(() =>
            {
                foreach (var element in LayoutRoot.Children)
                {
                    if (element is Label label)
                    {
                        label.IsEnabled = enable;
                    }
                    else if (element is Button button)
                    {
                        button.IsEnabled = enable;
                    }
                    else if (element is RadioButton radioButton)
                    {
                        radioButton.IsEnabled = enable;
                    }
                }
            });
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
                else if (element is Grid grid)
                    {
                        foreach (var grid_element in grid.Children)
                        {
                            if (grid_element is Button b)
                            {
                                b.FontSize = fontSize;
                            }
                        }
                    }
            }
        }
    }
}