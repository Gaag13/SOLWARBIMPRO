using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Windows.Input;
using WARBIMPRO.Services;
using WARBIMPRO.Utils;
using WARBIMPRO.Views;

namespace WARBIMPRO.ViewModels
{
    public class LoadFamiliesViewModel : ObservableObject
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
                SaveConfiguration();
            }
        }

        private string _customLibraryPath = "";
        public string CustomLibraryPath
        {
            get => _customLibraryPath;
            set
            {
                SetProperty(ref _customLibraryPath, value);
                SaveConfiguration();
            }
        }

        // ================================
        // ACCIONES para minimizar/restaurar la ventana padre
        // ================================
        public Action OnLoadStarted { get; set; }
        public Action OnLoadFinished { get; set; }

        // ================================
        // COMANDOS
        // ================================
        public IRelayCommand BrowserPathCommand { get; }
        public IRelayCommand LoadNowCommand { get; }
        public IRelayCommand LoadBasicCommand { get; }

        // ================================
        // CONSTRUCTOR
        // ================================
        public LoadFamiliesViewModel(UIDocument uidoc)
        {
            _uidoc = uidoc;

            BrowserPathCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(BrowsePath);
            LoadNowCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(LoadNow);
            LoadBasicCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(LoadBasicFamilies);

            LoadConfiguration();

            if (LoadBasicOnStartup)
                LoadBasicFamilies();
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

            if (dialog.ShowDialog() == true)
                CustomLibraryPath = System.IO.Path.GetDirectoryName(dialog.FileName);
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

            RunWithProgress(CustomLibraryPath);
        }

        private void LoadBasicFamilies()
        {
            if (string.IsNullOrEmpty(CustomLibraryPath))
            {
                TaskDialog.Show("Info", "La ruta de familias básicas no está configurada.");
                return;
            }

            if (!Directory.Exists(CustomLibraryPath))
            {
                TaskDialog.Show("Error", "No existe la ruta");
                return;
            }

            RunWithProgress(CustomLibraryPath);
        }

        /// <summary>
        /// Método central que abre el progress window y ejecuta la carga.
        /// </summary>
        private void RunWithProgress(string path)
        {
            Document doc = _uidoc.Document;

            // Abrir ventana de progreso
            var progressWindow = new ExportProgressWindow();
            progressWindow.Closed += (s, e) => OnLoadFinished?.Invoke();
            progressWindow.Show();

            // Minimizar la ventana padre
            OnLoadStarted?.Invoke();

            // Ejecutar carga reportando progreso
            var log = FamilyLoader.LoadFamiliesFromPath(doc, path, (percent, name) =>
            {
                progressWindow.ProgressViewModel.Report(percent, name);
                System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(() => { },
                    System.Windows.Threading.DispatcherPriority.Render);
            });

            // Contar resultados
            string logText = log.ToString();
            int cargadas = logText.Split('\n').Count(l => l.Contains("✔ Familia cargada"));
            int yaExistian = logText.Split('\n').Count(l => l.Contains("✔ Ya cargada"));
            int errores = logText.Split('\n').Count(l => l.Contains("❌ No se pudo"));

            // Mostrar solo si hay errores
            if (errores > 0)
                TaskDialog.Show("Load Results",
                    $"✔ Cargadas: {cargadas}\n" +
                    $"↩ Ya existían: {yaExistian}\n" +
                    $"❌ Errores: {errores}");
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
                            _loadBasicOnStartup = bool.Parse(parts[1]);
                            break;
                        case "CustomLibraryPath":
                            _customLibraryPath = parts[1];
                            break;
                    }
                }

                OnPropertyChanged(nameof(LoadBasicOnStartup));
                OnPropertyChanged(nameof(CustomLibraryPath));
            }
            catch { }
        }

        private void SaveConfiguration()
        {
            try
            {
                var directory = Path.GetDirectoryName(_configPath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                using var sw = new StreamWriter(_configPath, false);
                sw.WriteLine($"LoadBasicOnStartup={LoadBasicOnStartup}");
                sw.WriteLine($"CustomLibraryPath={CustomLibraryPath}");
            }
            catch { }
        }
    }
}