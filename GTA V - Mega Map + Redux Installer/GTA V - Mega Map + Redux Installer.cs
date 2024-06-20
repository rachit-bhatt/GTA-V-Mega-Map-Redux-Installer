using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
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
        private string _openIVDirectory;
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
            _openIVDirectory = Environment.GetEnvironmentVariable("OpenIVDirectory", EnvironmentVariableTarget.User);

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

        private void OpenIVDirectory_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "EXE files (*.exe)|*.exe"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                _openIVDirectory = openFileDialog.FileName;
                openIVDirectoryPathLabel.Content = _openIVDirectory;

                Environment.SetEnvironmentVariable("OpenIVDirectory", _openIVDirectory, EnvironmentVariableTarget.User);
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
            // Verify the OIV file exists
            if (!File.Exists(oivFilePath))
            {
                Console.WriteLine($"OIV file not found: { oivFilePath }");
                return;
            }

            // Determine install location
            string installLocation = installToMods ? Path.Combine(gameDirectory, "mods") : gameDirectory;

            // Path to OpenIV executable
            string openIVPath = _openIVDirectory;

            // Command to install the OIV package using OpenIV
            string arguments = $"\"{ oivFilePath }\" -core.game.select:false -core.game:Five \"{ gameDirectory }\"";

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = openIVPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string errors = process.StandardError.ReadToEnd();

                process.WaitForExit();

                Console.WriteLine($"Output from { oivFilePath }:");
                Console.WriteLine(output);
                if (!string.IsNullOrEmpty(errors))
                {
                    Console.WriteLine($"Errors from { oivFilePath }:");
                    Console.WriteLine(errors);
                }
            }
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