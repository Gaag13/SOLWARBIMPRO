using Microsoft.Win32;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WARBIMPRO.Models;
using WARBIMPRO.Services;

namespace WARBIMPRO.ViewModels
{
    public partial class ExportFamiliesViewModel : ObservableObject
    {
        private readonly Document _doc;
        private readonly FamilyService _service;

        public ObservableCollection<FamilyItem> Families { get; set; }

        public ExportFamiliesViewModel(Document doc)
        {
            _doc = doc;
            _service = new FamilyService();

            Families = new ObservableCollection<FamilyItem>(_service.GetFamilies(doc));
        }

        [RelayCommand]
        private void SelectAll()
        {
            foreach (var family in Families)
                family.IsChecked = true;
        }

        [RelayCommand]
        private void DeselectAll()
        {
            foreach (var family in Families)
                family.IsChecked = false;
        }

        [RelayCommand]
        private void Export()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.CheckFileExists = false;
            dialog.FileName = "Seleccionar carpeta";

            if (dialog.ShowDialog() != true)
                return;

            string folder = System.IO.Path.GetDirectoryName(dialog.FileName);

            _service.ExportFamilies(_doc, Families.Where(x => x.IsChecked), folder);
        }
    }
}
