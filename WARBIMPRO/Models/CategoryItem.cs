using System.ComponentModel;
using System.Collections.ObjectModel;

namespace WARBIMPRO.Models
{
    public class CategoryItem : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string Name { get; set; }
        public string Icon { get; set; }
        public string TapGroup { get; set; }
        public int ElementCount { get; set; }
        public Autodesk.Revit.DB.BuiltInCategory BuiltInCategory { get; set; }

        // ─── Soporte para filtro por tipos ──────────────────────────────────
        public bool SupportTypes { get; set; } = false;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    //// ─── Modelo para cada tipo de familia ───────────────────────────────────
    //public class TypeItem : INotifyPropertyChanged
    //{
    //    private bool _isSelected;

    //    public string Name { get; set; }
    //    public Autodesk.Revit.DB.ElementId TypeId { get; set; }
    //    public int ElementCount { get; set; }

    //    public bool IsSelected
    //    {
    //        get => _isSelected;
    //        set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
    //    }

    //    public event PropertyChangedEventHandler PropertyChanged;
    //    protected void OnPropertyChanged(string name) =>
    //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    //}
}