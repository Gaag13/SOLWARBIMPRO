//using System.ComponentModel;

//namespace WARBIMPRO.Models
//{
//    public class TypeItem : INotifyPropertyChanged
//    {
//        private bool _isSelected;

//        public string Id { get; set; }
//        public string Name { get; set; }
//        public int ElementCount { get; set; }

//        public bool IsSelected
//        {
//            get => _isSelected;
//            set
//            {
//                _isSelected = value;
//                OnPropertyChanged(nameof(IsSelected));
//            }
//        }

//        public event PropertyChangedEventHandler PropertyChanged;
//        protected void OnPropertyChanged(string name) =>
//            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
//    }
//}