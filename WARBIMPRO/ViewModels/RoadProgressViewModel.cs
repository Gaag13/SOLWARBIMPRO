using CommunityToolkit.Mvvm.ComponentModel;
using DocumentFormat.OpenXml.Wordprocessing;

namespace WARBIMPRO.ViewModels
{
    public partial class RoadProgressViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _progress = 0;

        [ObservableProperty]
        private string _statusText = "Iniciando...";

        [ObservableProperty]
        private bool _isCompleted = false;

        public void Report(int percent, string status)
        {
            Progress = percent;
            StatusText = percent < 100
                ? status
                : "¡Vía y andenes creados correctamente!";

            if (percent >= 100)
                IsCompleted = true;
        }
    }
}
