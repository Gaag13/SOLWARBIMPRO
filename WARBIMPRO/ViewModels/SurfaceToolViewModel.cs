//using Autodesk.Revit.DB;
//using Autodesk.Revit.UI;
//using WARBIMPRO.Services;
//using WARBIMPRO.Utils;
//using System;
//using System.Collections.ObjectModel;
//using System.ComponentModel;
//using System.Runtime.CompilerServices;
//using System.Windows.Input;

//namespace WARBIMPRO.ViewModels
//{
//    /// <summary>
//    /// ViewModel del panel de superficies viales.
//    /// Implementa INotifyPropertyChanged para binding WPF.
//    /// Aquí puedes inyectar tus propios RelayCommand / BaseViewModel cuando lo adaptes a tu MVVM.
//    /// </summary>
//    public class SurfaceToolViewModel : INotifyPropertyChanged
//    {
//        // ── Estado externo de Revit (se setea desde el Command) ──────────────
//        public UIDocument? UiDoc { get; set; }

//        private Document? Doc => UiDoc?.Document;

//        // ── Propiedades bindeadas al View ────────────────────────────────────

//        private string _statusMessage = "Listo.";
//        public string StatusMessage
//        {
//            get => _statusMessage;
//            set { _statusMessage = value; OnPropertyChanged(); }
//        }

//        private bool _isBusy;
//        public bool IsBusy
//        {
//            get => _isBusy;
//            set { _isBusy = value; OnPropertyChanged(); }
//        }

//        private double _thickness = 0.5;
//        public double Thickness
//        {
//            get => _thickness;
//            set { _thickness = value; OnPropertyChanged(); }
//        }

//        private double _sectionSpacing = 5.0;
//        public double SectionSpacing
//        {
//            get => _sectionSpacing;
//            set { _sectionSpacing = value; OnPropertyChanged(); }
//        }

//        private double _sectionHalfWidth = 10.0;
//        public double SectionHalfWidth
//        {
//            get => _sectionHalfWidth;
//            set { _sectionHalfWidth = value; OnPropertyChanged(); }
//        }

//        private int _viewScale = 100;
//        public int ViewScale
//        {
//            get => _viewScale;
//            set { _viewScale = value; OnPropertyChanged(); }
//        }

//        private string _csvPath = string.Empty;
//        public string CsvPath
//        {
//            get => _csvPath;
//            set { _csvPath = value; OnPropertyChanged(); }
//        }

//        private bool _csvInMeters = true;
//        public bool CsvInMeters
//        {
//            get => _csvInMeters;
//            set { _csvInMeters = value; OnPropertyChanged(); }
//        }

//        // Log de operaciones — lista de mensajes visibles en el panel
//        public ObservableCollection<string> Log { get; } = new();

//        // ── Comandos WPF ─────────────────────────────────────────────────────

//        public ICommand CreateFromLinesCommand  => new RelayCmd(CreateFromLines,  () => UiDoc != null && !IsBusy);
//        public ICommand CreateFromCsvCommand    => new RelayCmd(CreateFromCsv,    () => !string.IsNullOrEmpty(CsvPath) && !IsBusy);
//        public ICommand CreateSectionsCommand   => new RelayCmd(CreateSections,   () => UiDoc != null && !IsBusy);
//        public ICommand BrowseCsvCommand        => new RelayCmd(BrowseCsv);
//        public ICommand ClearLogCommand         => new RelayCmd(() => Log.Clear());

//        // ── Acciones ─────────────────────────────────────────────────────────

//        private void CreateFromLines()
//        {
//            if (UiDoc == null || Doc == null) return;

//            IsBusy = true;
//            AddLog("Selecciona las líneas de borde en el modelo...");

//            try
//            {
//                var pts = PointExtractor.FromModelLines(UiDoc, out string extractMsg);
//                AddLog(extractMsg);

//                if (pts.Count < 3)
//                {
//                    AddLog("✗ Puntos insuficientes.");
//                    return;
//                }

//                var svc = new SurfaceCreationService(Doc);
//                var result = svc.CreateSurface(pts, thicknessMeters: Thickness, message: out string msg);

//                AddLog(result == Result.Succeeded ? $"✓ {msg}" : $"✗ {msg}");
//                StatusMessage = msg;
//            }
//            catch (Exception ex)
//            {
//                AddLog($"✗ {ex.Message}");
//            }
//            finally
//            {
//                IsBusy = false;
//            }
//        }

//        private void CreateFromCsv()
//        {
//            if (Doc == null) return;

//            IsBusy = true;
//            AddLog($"Leyendo: {System.IO.Path.GetFileName(CsvPath)}");

//            try
//            {
//                var pts = PointExtractor.FromCsv(CsvPath, CsvInMeters, out string extractMsg);
//                AddLog(extractMsg);

//                if (pts.Count < 3)
//                {
//                    AddLog("✗ Puntos insuficientes en el CSV.");
//                    return;
//                }

//                var svc = new SurfaceCreationService(Doc);
//                var result = svc.CreateSurface(pts, thicknessMeters: Thickness, message: out string msg);

//                AddLog(result == Result.Succeeded ? $"✓ {msg}" : $"✗ {msg}");
//                StatusMessage = msg;
//            }
//            catch (Exception ex)
//            {
//                AddLog($"✗ {ex.Message}");
//            }
//            finally
//            {
//                IsBusy = false;
//            }
//        }

//        private void CreateSections()
//        {
//            if (UiDoc == null || Doc == null) return;

//            IsBusy = true;
//            AddLog("Selecciona las líneas del eje de vía...");

//            try
//            {
//                var pts = PointExtractor.FromModelLines(UiDoc, out string extractMsg);
//                AddLog(extractMsg);

//                if (pts.Count < 2)
//                {
//                    AddLog("✗ Se necesitan al menos 2 puntos de eje.");
//                    return;
//                }

//                var svc = new SurfaceCreationService(Doc);
//                var result = svc.CreateCrossSections(
//                    pts, SectionSpacing, SectionHalfWidth, ViewScale,
//                    out string msg);

//                AddLog(result == Result.Succeeded ? $"✓ {msg}" : $"✗ {msg}");
//                StatusMessage = msg;
//            }
//            catch (Exception ex)
//            {
//                AddLog($"✗ {ex.Message}");
//            }
//            finally
//            {
//                IsBusy = false;
//            }
//        }

//        private void BrowseCsv()
//        {
//            var dlg = new Microsoft.Win32.OpenFileDialog
//            {
//                Filter = "CSV / TXT (*.csv;*.txt)|*.csv;*.txt|Todos|*.*",
//                Title = "Seleccionar archivo de puntos"
//            };
//            if (dlg.ShowDialog() == true)
//                CsvPath = dlg.FileName;
//        }

//        // ── Helpers ──────────────────────────────────────────────────────────

//        private void AddLog(string msg)
//        {
//            Log.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {msg}");
//            StatusMessage = msg;
//        }

//        // ── INotifyPropertyChanged ────────────────────────────────────────────
//        public event PropertyChangedEventHandler? PropertyChanged;
//        protected void OnPropertyChanged([CallerMemberName] string? name = null)
//            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
//    }

//    /// <summary>
//    /// RelayCommand mínimo — reemplaza con el tuyo propio si ya tienes uno en tu proyecto.
//    /// </summary>
//    public class RelayCmd : ICommand
//    {
//        private readonly Action _execute;
//        private readonly Func<bool>? _canExecute;

//        public RelayCmd(Action execute, Func<bool>? canExecute = null)
//        {
//            _execute    = execute;
//            _canExecute = canExecute;
//        }

//        public bool CanExecute(object? _) => _canExecute?.Invoke() ?? true;
//        public void Execute(object? _)    => _execute();
//        public event EventHandler? CanExecuteChanged
//        {
//            add    => CommandManager.RequerySuggested += value;
//            remove => CommandManager.RequerySuggested -= value;
//        }
//    }
//}
