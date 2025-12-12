using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WARBIMPRO.Models;
using WARBIMPRO.Services;

namespace WARBIMPRO.ViewModels
{
    public class FindReplaceViewModel : ObservableObject
    {
        private readonly Document _doc;       

        // Lista completa de vistas (sin filtrar)
        private ObservableCollection<ViewItem> _allViews = new ObservableCollection<ViewItem>();
        // Lista visible en el ListBox (filtrada)
        private ObservableCollection<ViewItem> _views = new ObservableCollection<ViewItem>();
        public ObservableCollection<ViewItem> Views
        {
            get => _views;
            set => SetProperty(ref _views, value);
        }
        // Propiedad para el texto de búsqueda
        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                FilterViews(); // Filtrar cuando cambie el texto
            }
        }
        private string _replace = "";
        public string Replace { get => _replace; set => SetProperty(ref _replace, value); }

        private string _prefix = "";
        public string Prefix { get => _prefix; set => SetProperty(ref _prefix, value); }

        private string _suffix = "";
        public string Suffix { get => _suffix; set => SetProperty(ref _suffix, value); }

        public ICommand SelectAllCommand { get; }
        public ICommand SelectNoneCommand { get; }
        public ICommand RenameCommand { get; }

        public FindReplaceViewModel(Document doc)
        {
            _doc = doc;
           
            LoadViews();

            SelectAllCommand = new RelayCommand(SelectAll);
            SelectNoneCommand = new RelayCommand(SelectNone);
            RenameCommand = new RelayCommand(RenameViews);           
        }
        private void LoadViews()
        {
            var views = new FilteredElementCollector(_doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(v => !v.IsTemplate);

            _allViews.Clear();
            Views.Clear();

            foreach (var v in views)
            {
                var viewItem = new ViewItem(v);
                _allViews.Add(viewItem);
                Views.Add(viewItem);
            }
        }
        // Método que filtra las vistas según el texto de búsqueda
        private void FilterViews()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                // Si no hay texto, mostrar todas
                Views = new ObservableCollection<ViewItem>(_allViews);
            }
            else
            {
                // Filtrar por nombre (case insensitive)
                var filtered = _allViews.Where(v =>
                    v.Name.ToLower().Contains(SearchText.ToLower())
                ).ToList();

                Views = new ObservableCollection<ViewItem>(filtered);
            }
        }
        private void SelectAll() => Views.ToList().ForEach(v => v.IsSelected = true);

        private void SelectNone() => Views.ToList().ForEach(v => v.IsSelected = false);

        private void RenameViews()
        {
            var service = new ViewRenameService(_doc);

            // Obtener las vistas seleccionadas de TODAS las vistas (no solo las filtradas)
            var selected = _allViews.Where(v => v.IsSelected).ToList();

            service.Rename(selected, SearchText, Replace, Prefix, Suffix);
            MessageBox.Show("Renaming completed.");

        }   
    }
}