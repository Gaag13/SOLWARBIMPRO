using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Windows.Input;
using WARBIMPRO.Services;
using WARBIMPRO.Utils;

namespace WARBIMPRO.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        private readonly UIDocument _uidoc;

        // ================================
        // PROPIEDADES
        // ================================
        private bool _loadBasicOnStartup = true;
        public bool LoadBasicOnStartup
        {
            get => _loadBasicOnStartup;
            set
            {
                SetProperty(ref _loadBasicOnStartup, value);
                SaveConfiguration(); // Guardar automáticamente al cambiar
            }
        }

        private string _customLibraryPath = "";
        public string CustomLibraryPath
        {
            get => _customLibraryPath;
            set
            {
                SetProperty(ref _customLibraryPath, value);
                SaveConfiguration(); // Guardar automáticamente al cambiar
            }
        }

        //private bool _autoLoadOnOpen;
        //public bool AutoLoadOnOpen
        //{
        //    get => _autoLoadOnOpen;
        //    set
        //    {
        //        SetProperty(ref _autoLoadOnOpen, value);
        //        SaveConfiguration(); // Guardar automáticamente al cambiar
        //    }
        //}

        // ================================
        // COMANDOS
        // ================================
        public IRelayCommand BrowserPathCommand { get; }
        public IRelayCommand LoadNowCommand { get; }
        public IRelayCommand LoadBasicCommand { get; }

        // ================================
        // CONSTRUCTOR
        // ================================
        public SettingsViewModel(UIDocument uidoc)
        {
            _uidoc = uidoc;

            BrowserPathCommand = new RelayCommand(BrowsePath);
            LoadNowCommand = new RelayCommand(LoadNow);
            LoadBasicCommand = new RelayCommand(LoadBasicFamilies);

            // Cargar configuración guardada
            LoadConfiguration();

            // Si está activado "Load basic on startup", cargar familias básicas
            if (LoadBasicOnStartup )
            {
                LoadBasicFamilies();
            }
        }

        // ================================
        // MÉTODOS
        // ================================
        private void BrowsePath()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                Title = "Selecciona la carpeta de familias",
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Familias",
                Filter = "Family Files|*.rfa"
            };

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                CustomLibraryPath = System.IO.Path.GetDirectoryName(dialog.FileName);
            }
        }

        private void LoadNow()
        {
            if (string.IsNullOrWhiteSpace(CustomLibraryPath))
            {
                TaskDialog.Show("Error", "Selecciona una carpeta válida primero.");
                return;
            }

            if (!Directory.Exists(CustomLibraryPath))
            {
                TaskDialog.Show("Error", "La ruta seleccionada no existe.");
                return;
            }

            Document doc = _uidoc.Document;
            var result = FamilyLoader.LoadFamiliesFromPath(doc, CustomLibraryPath);

            TaskDialog.Show("Load Results", result.ToString());
        }

        private void LoadBasicFamilies()
        {
    
            string rutaC = CustomLibraryPath;

            if (string.IsNullOrEmpty(rutaC))
            {
                TaskDialog.Show("Info",
                    "La ruta de familias básicas no está configurada.");
                return;
            }
            if (!Directory.Exists(rutaC))
            {
                TaskDialog.Show("Error", "No existe la ruta");
                return;
            }

            Document doc = _uidoc.Document;
            var familiasCargadas = FamilyLoader.LoadFamiliesFromPath(doc, rutaC);

        }

        // ================================
        // CONFIGURACIÓN LOCAL
        // ================================
        private readonly string _configPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WARBIMPRO",
            "settings.ini"
        );

        private void LoadConfiguration()
        {
            try
            {
                if (!File.Exists(_configPath)) return;

                var lines = File.ReadAllLines(_configPath);
                foreach (var line in lines)
                {
                    var parts = line.Split('=');
                    if (parts.Length != 2) continue;

                    switch (parts[0])
                    {
                        case "LoadBasicOnStartup":
                            // Usar el campo privado para no triggear SaveConfiguration
                            _loadBasicOnStartup = bool.Parse(parts[1]);
                            break;
                        case "CustomLibraryPath":
                            _customLibraryPath = parts[1];
                            break;
                        //case "AutoLoadOnOpen":
                        //    _autoLoadOnOpen = bool.Parse(parts[1]);
                        //    break;
                    }
                }

                // Notificar cambios después de cargar todo
                OnPropertyChanged(nameof(LoadBasicOnStartup));
                OnPropertyChanged(nameof(CustomLibraryPath));
                //OnPropertyChanged(nameof(AutoLoadOnOpen));
            }
            catch
            {
                // Si falla no explota Revit
            }
        }

        private void SaveConfiguration()
        {
            try
            {
                var directory = Path.GetDirectoryName(_configPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using var sw = new StreamWriter(_configPath, false);
                sw.WriteLine($"LoadBasicOnStartup={LoadBasicOnStartup}");
                sw.WriteLine($"CustomLibraryPath={CustomLibraryPath}");
                //sw.WriteLine($"AutoLoadOnOpen={AutoLoadOnOpen}");
            }
            catch
            {
                // silencioso para no joder al usuario en medio de Revit
            }
        }
    }
}