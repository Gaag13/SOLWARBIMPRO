using CommunityToolkit.Mvvm.ComponentModel;
using DocumentFormat.OpenXml.Wordprocessing;

namespace WARBIMPRO.ViewModels
{
    public partial class ExportProgressViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _progress = 0;

        [ObservableProperty]
        private string _statusText = "Iniciando exportación...";

        [ObservableProperty]
        private bool _isCompleted = false;

        [ObservableProperty]
        private string _currentFamily = "";

        public void Report(int percent, string familyName)
        {
            Progress = percent;
            CurrentFamily = familyName;
            StatusText = percent < 100
                ? $"Exportando: {familyName}"
                : "¡Familias exportadas correctamente!";

            if (percent >= 100)
                IsCompleted = true;
        }
    }
}