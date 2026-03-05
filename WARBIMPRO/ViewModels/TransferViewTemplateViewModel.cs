using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WARBIMPRO.Models;
using WARBIMPRO.Utils;
using RelayCommand = WARBIMPRO.Utils.RelayCommand;

namespace WARBIMPRO.ViewModels
{
    public class TransferViewTemplateViewModel : ObservableObject
    {
        private readonly UIApplication _uiapp;
        private string _searchText = string.Empty;

        public ObservableCollection<Document> OpenDocuments { get; set; }

        // Colección fuente (todos los templates del doc origen)
        private ObservableCollection<ViewItem> ViewTemplates { get; set; }

        // Colección que ve el ListBox (filtrada)
        public ObservableCollection<ViewItem> FilteredTemplates { get; private set; }

        private Document _selectedSource;
        public Document SelectedSource
        {
            get => _selectedSource;
            set
            {
                _selectedSource = value;
                OnPropertyChanged();
                LoadViewTemplates();
            }
        }

        private Document _selectedTarget;
        public Document SelectedTarget
        {
            get => _selectedTarget;
            set
            {
                _selectedTarget = value;
                OnPropertyChanged();
            }
        }

        private ViewItem _selectedTemplate;
        public ViewItem SelectedTemplate
        {
            get => _selectedTemplate;
            set
            {
                _selectedTemplate = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ApplyFilter(); // reconstruye FilteredTemplates
            }
        }

        public ICommand CopyCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand SelectNoneCommand { get; }


        public TransferViewTemplateViewModel(UIApplication uiapp)
        {
            _uiapp = uiapp;

            OpenDocuments = new ObservableCollection<Document>(
                uiapp.Application.Documents
                    .Cast<Document>()
                    .Where(d => !d.IsFamilyDocument && !d.IsLinked && !d.IsModifiable));

            ViewTemplates = new ObservableCollection<ViewItem>();
            FilteredTemplates = new ObservableCollection<ViewItem>();

            // En el constructor:
            SelectAllCommand = new RelayCommand(_ => FilteredTemplates.ToList().ForEach(v => v.IsSelected = true));
            SelectNoneCommand = new RelayCommand(_ => FilteredTemplates.ToList().ForEach(v => v.IsSelected = false));

            CopyCommand = new RelayCommand(CopyTemplate);
        }

        private void LoadViewTemplates()
        {
            ViewTemplates.Clear();
            SelectedTemplate = null;

            if (SelectedSource == null)
            {
                ApplyFilter();
                return;
            }

            var templates = new FilteredElementCollector(SelectedSource)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(v => v.IsTemplate)
                .OrderBy(v => v.Name)
                .ToList();

            foreach (var t in templates)
                ViewTemplates.Add(new ViewItem(t));

            // Resetear búsqueda al cambiar de documento
            _searchText = string.Empty;
            OnPropertyChanged(nameof(SearchText));

            ApplyFilter();
        }
       

        // Único método que toca FilteredTemplates
        private void ApplyFilter()
        {
            FilteredTemplates.Clear();

            var source = string.IsNullOrWhiteSpace(SearchText)
                ? ViewTemplates
                : (IEnumerable<ViewItem>)ViewTemplates.Where(v =>
                    v.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0);

            foreach (var v in source)
                FilteredTemplates.Add(v);
        }

        private void CopyTemplate(object obj)
        {
            if (SelectedSource == null || SelectedTarget == null) return;

            var selectedIds = FilteredTemplates
                .Where(v => v.IsSelected)
                .Select(v => v.Id)
                .ToList();

            if (!selectedIds.Any()) return;

            using (var t = new Transaction(SelectedTarget, "Copy View Templates"))
            {
                t.Start();
                ElementTransformUtils.CopyElements(
                    SelectedSource,
                    selectedIds,
                    SelectedTarget,
                    Transform.Identity,
                    new CopyPasteOptions());
                t.Commit();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}