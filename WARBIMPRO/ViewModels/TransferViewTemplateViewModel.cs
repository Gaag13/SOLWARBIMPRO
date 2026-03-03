using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FilteredTemplates)); // refresca la lista
            }
        }
        public IEnumerable<View> FilteredTemplates =>
    string.IsNullOrWhiteSpace(SearchText)
        ? ViewTemplates
        : ViewTemplates.Where(v =>
            v.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        public ObservableCollection<Document> OpenDocuments { get; set; }
        public ObservableCollection<View> ViewTemplates { get; set; }

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

        public ICommand CopyCommand { get; }

        public TransferViewTemplateViewModel(UIApplication uiapp)
        {
            _uiapp = uiapp;

            OpenDocuments = new ObservableCollection<Document>(
                uiapp.Application.Documents
                    .Cast<Document>()
                    .Where(d => !d.IsFamilyDocument &&
                    !d.IsLinked &&
                    !d.IsModifiable));

            ViewTemplates = new ObservableCollection<View>();

            CopyCommand = new RelayCommand(CopyTemplate);
        }

        private void LoadViewTemplates()
        {
            ViewTemplates.Clear();

            if (SelectedSource == null)
                return;

            var templates = new FilteredElementCollector(SelectedSource)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(v => v.IsTemplate)
                .ToList();

            foreach (var t in templates)
                ViewTemplates.Add(t);

            OnPropertyChanged(nameof(FilteredTemplates)); 
        }

        private void CopyTemplate(object obj)
        {
            if (SelectedSource == null ||
                SelectedTarget == null ||
                SelectedTemplate == null)
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
