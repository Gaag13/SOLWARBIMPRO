using System.ComponentModel;

namespace WARBIMPRO.Models
{
    public class CategoryItem : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string Name { get; set; }
        public string Icon { get; set; }
        public string GroupName { get; set; }
        public int ElementCount { get; set; }
        public Autodesk.Revit.DB.BuiltInCategory BuiltInCategory { get; set; }

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
}