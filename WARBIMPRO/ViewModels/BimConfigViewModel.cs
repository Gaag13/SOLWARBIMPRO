using MvvmHelpers;
using System.Windows;
using System.Windows.Input;
using WARBIMPRO.Services;
using WARBIMPRO.Utils;
using RelayCommand = WARBIMPRO.Utils.RelayCommand;

namespace WARBIMPRO.ViewModels
{
    public class BimConfigViewModel : BaseViewModel
    {
        private readonly Window _window;

        private string _apiKey = string.Empty;
        private string _libraryPath = string.Empty;
        private string _statusText = "Ingresa tu API Key de Claude.";

        public string ApiKey
        {
            get => _apiKey;
            set
            {
                _apiKey = value;
                OnPropertyChanged();
                // Re-evaluar si el botón Guardar debe habilitarse
                (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string LibraryPath
        {
            get => _libraryPath;
            set { _libraryPath = value; OnPropertyChanged(); }
        }

        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand BrowseFolderCommand { get; }

        public BimConfigViewModel(Window window)
        {
            _window = window;

            // Cargar valores existentes al abrir
            var cfg = ConfigService.Load();
            ApiKey = cfg.ApiKey ?? string.Empty;
            LibraryPath = cfg.LibraryPath ?? string.Empty;

            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(() => _window.Close());
            BrowseFolderCommand = new RelayCommand(BrowseFolder);
        }

        private bool CanSave()
            => !string.IsNullOrWhiteSpace(ApiKey);

        private void Save()
        {
            ConfigService.Save(new AppConfig
            {
                ApiKey = ApiKey.Trim(),
                LibraryPath = LibraryPath.Trim()
            });

            StatusText = "Guardado correctamente.";

            MessageBox.Show(
                "Configuración guardada.",
                "WARBIMPRO", MessageBoxButton.OK, MessageBoxImage.Information);

            _window.Close();
        }

        private void BrowseFolder()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Selecciona la carpeta de biblioteca JSON",
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Seleccionar carpeta",
                Filter = "JSON Files|*.json"
            };

            if (dlg.ShowDialog() == true)
            {
                LibraryPath = System.IO.Path.GetDirectoryName(dlg.FileName)!;
                StatusText = $"Carpeta: {LibraryPath}";
            }
        }
    }
}