using Microsoft.Win32;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WARBIMPRO.Models;
using WARBIMPRO.Services;

namespace WARBIMPRO.ViewModels
{
    public partial class ExportFamiliesViewModel : ObservableObject
    {
        private readonly Document _doc;
        private readonly FamilyService _service;

        // Colección fuente (todas las familias)
        private ObservableCollection<FamilyItem> Families { get; set; }

        // Colección filtrada que ve el ListBox
        public ObservableCollection<FamilyItem> FilteredFamilies { get; private set; }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }

        public ExportFamiliesViewModel(Document doc)
        {
            _doc = doc;
            _service = new FamilyService();
            Families = new ObservableCollection<FamilyItem>(_service.GetFamilies(doc));
            FilteredFamilies = new ObservableCollection<FamilyItem>(Families);
        }

        private void ApplyFilter()
        {
            FilteredFamilies.Clear();

            var source = string.IsNullOrWhiteSpace(SearchText)
                ? Families
                : (IEnumerable<FamilyItem>)Families.Where(f =>
                    f.DisplayName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    f.Category.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0);

            foreach (var f in source)
                FilteredFamilies.Add(f);
        }

        [RelayCommand]
        private void SelectAll()
        {
            foreach (var family in FilteredFamilies)
                family.IsChecked = true;
        }

        [RelayCommand]
        private void DeselectAll()
        {
            foreach (var family in FilteredFamilies)
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
            _service.ExportFamilies(_doc, FilteredFamilies.Where(x => x.IsChecked), folder);
        }
    }
}