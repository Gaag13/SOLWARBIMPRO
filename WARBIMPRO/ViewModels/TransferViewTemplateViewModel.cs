using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
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
        private ObservableCollection<View> ViewTemplates { get; set; }

        // Colección que ve el ListBox (filtrada)
        public ObservableCollection<View> FilteredTemplates { get; private set; }

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

        private View _selectedTemplate;
        public View SelectedTemplate
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

        public TransferViewTemplateViewModel(UIApplication uiapp)
        {
            _uiapp = uiapp;

            OpenDocuments = new ObservableCollection<Document>(
                uiapp.Application.Documents
                    .Cast<Document>()
                    .Where(d => !d.IsFamilyDocument && !d.IsLinked && !d.IsModifiable));

            ViewTemplates = new ObservableCollection<View>();
            FilteredTemplates = new ObservableCollection<View>();

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
                ViewTemplates.Add(t);

            // Resetear búsqueda al cambiar de documento
            _searchText = string.Empty;
            OnPropertyChanged(nameof(SearchText));

            ApplyFilter();
        }
        private void SelectAll() => FilteredTemplates.ToList().ForEach(v => v.IsSelected = true);
        private void SelectNone() => FilteredTemplates.ToList().ForEach(v => v.IsSelected = false);


        // Único método que toca FilteredTemplates
        private void ApplyFilter()
        {
            FilteredTemplates.Clear();

            var source = string.IsNullOrWhiteSpace(SearchText)
                ? ViewTemplates
                : (IEnumerable<View>)ViewTemplates.Where(v =>
                    v.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0);

            foreach (var v in source)
                FilteredTemplates.Add(v);
        }

        private void CopyTemplate(object obj)
        {
            if (SelectedSource == null || SelectedTarget == null || SelectedTemplate == null)
                return;

            using (var t = new Transaction(SelectedTarget, "Copy View Template"))
            {
                t.Start();
                ElementTransformUtils.CopyElements(
                    SelectedSource,
                    new List<ElementId> { SelectedTemplate.Id },
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