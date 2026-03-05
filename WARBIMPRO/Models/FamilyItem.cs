using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WARBIMPRO.Models
{
    public partial class FamilyItem: ObservableObject
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string DisplayName=> $"{Category} - {Name}";
        public Family Family { get; set; }

       
        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }

    }
}
