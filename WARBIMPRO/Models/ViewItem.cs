using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WARBIMPRO.Models
{
    public class ViewItem : ObservableObject
    {
        public ElementId Id { get; }
        public string Name { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
        public ViewItem(View view)
        {
            Id = view.Id;
            Name = view.Name;
        }
    }
}
