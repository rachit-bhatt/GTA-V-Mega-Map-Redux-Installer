using System.ComponentModel;
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
        private bool _isInstalling;
        private OIVInstaller _oivInstaller;

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

        private void SaveGameDirectory(string path)
        {
            Environment.SetEnvironmentVariable("GTAVGameDirectory", path, EnvironmentVariableTarget.User);
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

                SaveGameDirectory(_gameDirectory);
            }
        }

        private async void Install_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_gameDirectory) || string.IsNullOrEmpty(_oivFilePath))
            {
                MessageBox.Show("Please select both the game directory and the OIV file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            installButton.Visibility = Visibility.Collapsed;
            installationProgressBar.Visibility = Visibility.Visible;
            cancelButton.Visibility = Visibility.Visible;

            _oivInstaller = new OIVInstaller();
            // _oivInstaller.ProgressChanged += OIVInstaller_ProgressChanged;

            try
            {
                // await _oivInstaller.InstallOIVPackageAsync(_oivFilePath, _gameDirectory);
                MessageBox.Show($"Installation of { Path.GetFileName(_oivFilePath) } completed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during installation: { ex.Message }", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                installButton.Visibility = Visibility.Visible;
                installationProgressBar.Visibility = Visibility.Collapsed;
                cancelButton.Visibility = Visibility.Collapsed;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // _oivInstaller?.Cancel();
        }

        private void OIVInstaller_ProgressChanged(object sender, int e)
        {
            Dispatcher.Invoke(() => installationProgressBar.Value = e);
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