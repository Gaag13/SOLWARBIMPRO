// ─────────────────────────────────────────────────────────────────────────────
// BimFamilyCreatorView.xaml.cs  —  code-behind mínimo, solo UI pura
// ─────────────────────────────────────────────────────────────────────────────
using System.Windows;
using System.Windows.Input;
using WARBIMPRO.ViewModels;

namespace WARBIMPRO.Views
{
    public partial class BimFamilyCreatorView : Window
    {
        public BimFamilyCreatorView(BimFamilyCreatorViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            => DragMove();

        private void CloseButton_Click(object sender, RoutedEventArgs e)
            => Close();

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;
    }
}