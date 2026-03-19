using System.Windows;
using WARBIMPRO.ViewModels;

namespace WARBIMPRO.Views
{
    public partial class RoadProgressWindow : Window
    {
        public RoadProgressViewModel ProgressViewModel { get; }

        public RoadProgressWindow()
        {
            InitializeComponent();
            ProgressViewModel = new RoadProgressViewModel();
            DataContext = ProgressViewModel;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
