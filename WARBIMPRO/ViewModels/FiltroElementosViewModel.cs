using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using Autodesk.Revit.UI;
using WARBIMPRO.Models;
using WARBIMPRO.Services;
using WARBIMPRO.Utils;
using Color = System.Windows.Media.Color;
using RelayCommand = WARBIMPRO.Utils.RelayCommand;

namespace WARBIMPRO.ViewModels
{
    public class FiltroElementosViewModel : INotifyPropertyChanged
    {
        private readonly FiltroElementosService _service;
        private readonly UIApplication _uiApp;

        // ─── Propiedades Niveles ─────────────────────────────────────────────
        public ObservableCollection<LevelItem> Levels { get; } = new();

        private bool _allLevelsSelected;
        public bool AllLevelsSelected
        {
            get => _allLevelsSelected;
            set
            {
                _allLevelsSelected = value;
                foreach (var l in Levels) l.IsSelected = value;
                OnPropertyChanged(nameof(AllLevelsSelected));
                UpdateStatus();
            }
        }

        // ─── Modo de alcance ─────────────────────────────────────────────────
        private bool _isAllModelMode;
        public bool IsAllModelMode
        {
            get => _isAllModelMode;
            set
            {
                _isAllModelMode = value;
                OnPropertyChanged(nameof(IsAllModelMode));
                OnPropertyChanged(nameof(IsLevelMode));
                UpdateStatus();
            }
        }
        public bool IsLevelMode => !_isAllModelMode;

        // ─── Propiedades Categorías ──────────────────────────────────────────
        public ObservableCollection<CategoryItem> Categories { get; } = new();

        public IEnumerable<IGrouping<string, CategoryItem>> GroupedCategories =>
            Categories.GroupBy(c => c.GroupName);

        // ─── Color ───────────────────────────────────────────────────────────
        private double _hue = 210;
        private double _saturation = 85;
        private double _lightness = 55;
        private int _opacity = 100;

        public double Hue
        {
            get => _hue;
            set { _hue = value; OnPropertyChanged(nameof(Hue)); UpdatePreviewColor(); }
        }
        public double Saturation
        {
            get => _saturation;
            set { _saturation = value; OnPropertyChanged(nameof(Saturation)); UpdatePreviewColor(); }
        }
        public double Lightness
        {
            get => _lightness;
            set { _lightness = value; OnPropertyChanged(nameof(Lightness)); UpdatePreviewColor(); }
        }
        public int Opacity
        {
            get => _opacity;
            set { _opacity = value; OnPropertyChanged(nameof(Opacity)); OnPropertyChanged(nameof(OpacityLabel)); }
        }
        public string OpacityLabel => $"{_opacity}%";

        private Color _selectedColor = Color.FromRgb(79, 142, 247);
        public Color SelectedColor
        {
            get => _selectedColor;
            set
            {
                _selectedColor = value;
                OnPropertyChanged(nameof(SelectedColor));
                OnPropertyChanged(nameof(PreviewBrush));
                OnPropertyChanged(nameof(HexColor));
            }
        }

        public SolidColorBrush PreviewBrush =>
            new SolidColorBrush(SelectedColor);

        private string _hexColor = "#4F8EF7";
        public string HexColor
        {
            get => _hexColor;
            set
            {
                _hexColor = value;
                OnPropertyChanged(nameof(HexColor));
                TrySetColorFromHex(value);
            }
        }

        // ─── Status ──────────────────────────────────────────────────────────
        private string _statusText = "Sin selección activa";
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(nameof(StatusText)); }
        }

        private int _selectedElementCount;
        public int SelectedElementCount
        {
            get => _selectedElementCount;
            set { _selectedElementCount = value; OnPropertyChanged(nameof(SelectedElementCount)); OnPropertyChanged(nameof(SelectionSummary)); }
        }
        public string SelectionSummary =>
            $"{Categories.Count(c => c.IsSelected)} categ. · " +
            $"{Levels.Count(l => l.IsSelected)} niveles · " +
            $"~{_selectedElementCount} elementos";

        // ─── Comandos ────────────────────────────────────────────────────────
        public ICommand ApplyColorCommand { get; }
        public ICommand IsolateCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand SetPresetCommand { get; }
        public ICommand CloseCommand { get; }

        // Acción de cierre (asignada desde el code-behind)
        public Action CloseAction { get; set; }

        // ─── Constructor ─────────────────────────────────────────────────────
        public FiltroElementosViewModel(UIApplication uiApp)
        {
            _uiApp = uiApp;
            _service = new FiltroElementosService(uiApp);

            ApplyColorCommand = new RelayCommand(_ => ExecuteApplyColor());
            IsolateCommand = new RelayCommand(_ => ExecuteIsolate());
            ResetCommand = new RelayCommand(_ => ExecuteReset());
            SetPresetCommand = new RelayCommand(param => SetPresetColor(param as string));
            CloseCommand = new RelayCommand(_ => CloseAction?.Invoke());

            LoadData();
        }

        // ─── Carga de datos ──────────────────────────────────────────────────
        private void LoadData()
        {
            // Niveles
            var levels = _service.GetLevels();
            foreach (var l in levels)
            {
                l.PropertyChanged += (s, e) => { if (e.PropertyName == nameof(LevelItem.IsSelected)) UpdateStatus(); };
                Levels.Add(l);
            }

            // Categorías
            var cats = _service.GetAvailableCategories();
            foreach (var c in cats)
            {
                // Contar elementos en modelo
                c.ElementCount = _service.CountElements(c.BuiltInCategory);
                c.PropertyChanged += (s, e) => { if (e.PropertyName == nameof(CategoryItem.IsSelected)) UpdateStatus(); };
                Categories.Add(c);
            }

            UpdatePreviewColor();
            UpdateStatus();
        }

        // ─── Lógica de comandos ──────────────────────────────────────────────
        private void ExecuteApplyColor()
        {
            try
            {
                var ids = GetCurrentElementIds();
                if (!ids.Any())
                {
                    StatusText = "⚠ Selecciona al menos una categoría";
                    return;
                }
                _service.ApplyColor(ids, SelectedColor, Opacity);
                StatusText = $"✓ Color aplicado a {ids.Count} elementos";
            }
            catch (Exception ex)
            {
                StatusText = $"✗ Error: {ex.Message}";
            }
        }

        private void ExecuteIsolate()
        {
            try
            {
                var ids = GetCurrentElementIds();
                if (!ids.Any())
                {
                    StatusText = "⚠ Selecciona al menos una categoría";
                    return;
                }
                _service.IsolateElements(ids);
                StatusText = $"⬡ {ids.Count} elementos aislados en vista";
            }
            catch (Exception ex)
            {
                StatusText = $"✗ Error: {ex.Message}";
            }
        }

        private void ExecuteReset()
        {
            try
            {
                _service.ResetOverrides();
                foreach (var l in Levels) l.IsSelected = false;
                foreach (var c in Categories) c.IsSelected = false;
                AllLevelsSelected = false;
                StatusText = "↺ Filtros y overrides restablecidos";
            }
            catch (Exception ex)
            {
                StatusText = $"✗ Error: {ex.Message}";
            }
        }

        // ─── Helpers ─────────────────────────────────────────────────────────
        private List<Autodesk.Revit.DB.ElementId> GetCurrentElementIds()
        {
            var selCats = Categories.Where(c => c.IsSelected).ToList();
            var selLevels = Levels.Where(l => l.IsSelected).ToList();
            return _service.GetFilteredElementIds(selLevels, selCats, IsAllModelMode);
        }

        private void UpdateStatus()
        {
            var selCats = Categories.Where(c => c.IsSelected).ToList();
            var selLevels = Levels.Where(l => l.IsSelected).ToList();

            if (!selCats.Any())
            {
                StatusText = "Sin selección activa";
                SelectedElementCount = 0;
                return;
            }

            var catNames = string.Join(" + ", selCats.Select(c => c.Name));
            var levelScope = IsAllModelMode
                ? "Todo el Modelo"
                : selLevels.Any()
                    ? string.Join(", ", selLevels.Select(l => l.Name))
                    : "Sin nivel";

            var ids = GetCurrentElementIds();
            SelectedElementCount = ids.Count;
            StatusText = $"{levelScope}  ·  {catNames}  ·  {ids.Count} elementos";
        }

        // ─── Color ───────────────────────────────────────────────────────────
        private void UpdatePreviewColor()
        {
            var color = HslToRgb(_hue, _saturation / 100.0, _lightness / 100.0);
            SelectedColor = color;
            _hexColor = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            OnPropertyChanged(nameof(HexColor));
        }

        private void TrySetColorFromHex(string hex)
        {
            if (hex?.Length == 7 && hex.StartsWith("#"))
            {
                try
                {
                    var c = (Color)ColorConverter.ConvertFromString(hex);
                    SelectedColor = c;
                }
                catch { /* hex incompleto, ignorar */ }
            }
        }

        public void SetPresetColor(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return;
            HexColor = hex;
        }

        private static Color HslToRgb(double h, double s, double l)
        {
            double c = (1 - Math.Abs(2 * l - 1)) * s;
            double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
            double m = l - c / 2;
            double r1, g1, b1;
            if (h < 60) { r1 = c; g1 = x; b1 = 0; }
            else if (h < 120) { r1 = x; g1 = c; b1 = 0; }
            else if (h < 180) { r1 = 0; g1 = c; b1 = x; }
            else if (h < 240) { r1 = 0; g1 = x; b1 = c; }
            else if (h < 300) { r1 = x; g1 = 0; b1 = c; }
            else { r1 = c; g1 = 0; b1 = x; }
            return Color.FromRgb(
                (byte)((r1 + m) * 255),
                (byte)((g1 + m) * 255),
                (byte)((b1 + m) * 255));
        }

        // ─── INotifyPropertyChanged ──────────────────────────────────────────
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}