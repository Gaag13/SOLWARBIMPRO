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

        // ─── Niveles ─────────────────────────────────────────────────────────
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

        // ─── Modo alcance ─────────────────────────────────────────────────────
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

        // ─── Categorías ───────────────────────────────────────────────────────
        public ObservableCollection<CategoryItem> Categories { get; } = new();

        // Tab activo
        private string _activeTab = "Estructura";
        public string ActiveTab
        {
            get => _activeTab;
            set
            {
                _activeTab = value;
                OnPropertyChanged(nameof(ActiveTab));
                OnPropertyChanged(nameof(IsTabEstructura));
                OnPropertyChanged(nameof(IsTabArquitectura));
                OnPropertyChanged(nameof(IsTabMEP));
                OnPropertyChanged(nameof(CategoriesInTab));
                RefreshAvailableTypes();
            }
        }

        public bool IsTabEstructura => _activeTab == "Estructura";
        public bool IsTabArquitectura => _activeTab == "Arquitectura";
        public bool IsTabMEP => _activeTab == "MEP";

        // Categorías visibles según tab activo
        public IEnumerable<CategoryItem> CategoriesInTab =>
            Categories.Where(c => c.TapGroup == _activeTab);

        // ─── Tipos ────────────────────────────────────────────────────────────
        public ObservableCollection<TypeItem> AvailableTypes { get; } = new();

        private bool _hasAvailableTypes;
        public bool HasAvailableTypes
        {
            get => _hasAvailableTypes;
            set { _hasAvailableTypes = value; OnPropertyChanged(nameof(HasAvailableTypes)); }
        }

        private string _typePanelTitle = "TIPOS";
        public string TypePanelTitle
        {
            get => _typePanelTitle;
            set { _typePanelTitle = value; OnPropertyChanged(nameof(TypePanelTitle)); }
        }

        // ─── Color ────────────────────────────────────────────────────────────
        private double _hue = 210;
        private double _saturation = 85;
        private double _lightness = 55;
        private int _opacity = 100;

        public double Hue { get => _hue; set { _hue = value; OnPropertyChanged(nameof(Hue)); UpdatePreviewColor(); } }
        public double Saturation { get => _saturation; set { _saturation = value; OnPropertyChanged(nameof(Saturation)); UpdatePreviewColor(); } }
        public double Lightness { get => _lightness; set { _lightness = value; OnPropertyChanged(nameof(Lightness)); UpdatePreviewColor(); } }

        public int Opacity
        {
            get => _opacity;
            set { _opacity = value; OnPropertyChanged(nameof(Opacity)); OnPropertyChanged(nameof(OpacityLabel)); }
        }
        public string OpacityLabel => $"{_opacity}%";

        private Color _selectedColor = Color.FromRgb(52, 152, 219);
        public Color SelectedColor
        {
            get => _selectedColor;
            set { _selectedColor = value; OnPropertyChanged(nameof(SelectedColor)); OnPropertyChanged(nameof(PreviewBrush)); OnPropertyChanged(nameof(HexColor)); }
        }
        public SolidColorBrush PreviewBrush => new SolidColorBrush(SelectedColor);

        private string _hexColor = "#3498DB";
        public string HexColor
        {
            get => _hexColor;
            set { _hexColor = value; OnPropertyChanged(nameof(HexColor)); TrySetColorFromHex(value); }
        }

        // ─── Status ───────────────────────────────────────────────────────────
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

        // ─── Comandos ─────────────────────────────────────────────────────────
        public ICommand ApplyColorCommand { get; }
        public ICommand IsolateCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand SetPresetCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand SetTabCommand { get; }

        public Action CloseAction { get; set; }

        // ─── Constructor ──────────────────────────────────────────────────────
        public FiltroElementosViewModel(UIApplication uiApp)
        {
            _uiApp = uiApp;
            _service = new FiltroElementosService(uiApp);

            ApplyColorCommand = new RelayCommand(_ => ExecuteApplyColor());
            IsolateCommand = new RelayCommand(_ => ExecuteIsolate());
            ResetCommand = new RelayCommand(_ => ExecuteReset());
            SetPresetCommand = new RelayCommand(param => SetPresetColor(param as string));
            CloseCommand = new RelayCommand(_ => CloseAction?.Invoke());
            SetTabCommand = new RelayCommand(param => { if (param is string tab) ActiveTab = tab; });

            LoadData();
        }

        // ─── Carga de datos ───────────────────────────────────────────────────
        private void LoadData()
        {
            foreach (var l in _service.GetLevels())
            {
                l.PropertyChanged += (s, e) => { if (e.PropertyName == nameof(LevelItem.IsSelected)) UpdateStatus(); };
                Levels.Add(l);
            }

            foreach (var c in _service.GetAvailableCategories())
            {
                c.ElementCount = _service.CountElements(c.BuiltInCategory);
                c.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(CategoryItem.IsSelected))
                    {
                        RefreshAvailableTypes();
                        UpdateStatus();
                    }
                };
                Categories.Add(c);
            }

            UpdatePreviewColor();
            UpdateStatus();
        }

        // ─── Refrescar panel de tipos ─────────────────────────────────────────
        private void RefreshAvailableTypes()
        {
            AvailableTypes.Clear();

            var selCat = Categories
                .Where(c => c.IsSelected && c.SupportTypes && c.TapGroup == _activeTab)
                .ToList();

            if (!selCat.Any()) { HasAvailableTypes = false; return; }

            // Con múltiples categorías seleccionadas no cargamos tipos
            if (selCat.Count > 1)
            {
                TypePanelTitle = "TIPOS — múltiples categorías";
                HasAvailableTypes = false;
                return;
            }

            var cat = selCat[0];
            var types = _service.GetTypesForCategory(cat.BuiltInCategory);

            if (!types.Any()) { HasAvailableTypes = false; return; }

            TypePanelTitle = $"TIPOS — {cat.Name.ToUpper()}";
            foreach (var t in types)
            {
                t.PropertyChanged += (s, e) => { if (e.PropertyName == nameof(TypeItem.IsSelected)) UpdateStatus(); };
                AvailableTypes.Add(t);
            }
            HasAvailableTypes = true;
        }

        // ─── Ejecutar comandos ────────────────────────────────────────────────
        private void ExecuteApplyColor()
        {
            try
            {
                var ids = GetCurrentElementIds();
                if (!ids.Any()) { StatusText = "⚠ Selecciona al menos una categoría"; return; }
                _service.ApplyColor(ids, SelectedColor, Opacity);
                StatusText = $"✓ Color aplicado a {ids.Count} elementos";
            }
            catch (Exception ex) { StatusText = $"✗ Error: {ex.Message}"; }
        }

        private void ExecuteIsolate()
        {
            try
            {
                var ids = GetCurrentElementIds();
                if (!ids.Any()) { StatusText = "⚠ Selecciona al menos una categoría"; return; }
                _service.IsolateElements(ids);
                StatusText = $"⬡ {ids.Count} elementos aislados en vista";
            }
            catch (Exception ex) { StatusText = $"✗ Error: {ex.Message}"; }
        }

        private void ExecuteReset()
        {
            try
            {
                _service.ResetOverrides();
                foreach (var l in Levels) l.IsSelected = false;
                foreach (var c in Categories) c.IsSelected = false;
                foreach (var t in AvailableTypes) t.IsSelected = false;
                AllLevelsSelected = false;
                AvailableTypes.Clear();
                HasAvailableTypes = false;
                StatusText = "↺ Filtros y overrides restablecidos";
            }
            catch (Exception ex) { StatusText = $"✗ Error: {ex.Message}"; }
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private List<Autodesk.Revit.DB.ElementId> GetCurrentElementIds()
        {
            var selCats = Categories.Where(c => c.IsSelected).ToList();
            var selLevels = Levels.Where(l => l.IsSelected).ToList();

            // Proyectamos directamente los IDs de los tipos seleccionados
            var selTypeIds = AvailableTypes.Where(t => t.IsSelected).Select(t => t.TypeId).ToList();

            return _service.GetFilteredElementIds(
                selLevels,
                selCats,
                IsAllModelMode,
                selTypeIds.Any() ? selTypeIds : null);
        }

        private void UpdateStatus()
        {
            var selCats = Categories.Where(c => c.IsSelected).ToList();
            var selLevels = Levels.Where(l => l.IsSelected).ToList();

            if (!selCats.Any()) { StatusText = "Sin selección activa"; SelectedElementCount = 0; return; }

            var scope = IsAllModelMode ? "Todo el Modelo"
                         : selLevels.Any() ? string.Join(", ", selLevels.Select(l => l.Name))
                         : "Sin nivel";
            var selTypes = AvailableTypes.Where(t => t.IsSelected).ToList();
            var typeStr = selTypes.Any() ? $" [{string.Join(", ", selTypes.Select(t => t.Name))}]" : "";
            var ids = GetCurrentElementIds();
            SelectedElementCount = ids.Count;
            StatusText = $"{scope}  ·  {string.Join(" + ", selCats.Select(c => c.Name))}{typeStr}  ·  {ids.Count} elem.";
        }

        // ─── Color ────────────────────────────────────────────────────────────
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
                try { SelectedColor = (Color)ColorConverter.ConvertFromString(hex); } catch { }
        }

        public void SetPresetColor(string hex)
        {
            if (!string.IsNullOrEmpty(hex)) HexColor = hex;
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}