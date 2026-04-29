// ─────────────────────────────────────────────────────────────
// BimConfigView.xaml.cs  —  code-behind mínimo
// ─────────────────────────────────────────────────────────────

using System.Windows;
using System.Windows.Input;
using WARBIMPRO.Services;
using WARBIMPRO.ViewModels;

namespace WARBIMPRO.Views
{
    public partial class BimConfigView : Window
    {
        public BimConfigView()
        {
            InitializeComponent();
            DataContext = new BimConfigViewModel(this);
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            => DragMove();

        private void CloseButton_Click(object sender, RoutedEventArgs e)
            => Close();
    }
}


