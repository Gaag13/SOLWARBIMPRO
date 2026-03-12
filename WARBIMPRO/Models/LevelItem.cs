using System.ComponentModel;

namespace WARBIMPRO.Models
{
    public class LevelItem : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string Id { get; set; }
        public string Name { get; set; }
        public double Elevation { get; set; }

        public string ElevationLabel => $"{(Elevation >= 0 ? "+" : "")}{Elevation:F2}m";

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