using System.Windows;
using System.Windows.Input;
using WARBIMPRO.Models;
using WARBIMPRO.ViewModels;

namespace WARBIMPRO.Views
{
    public partial class FiltroElementosView : Window
    {
       
        private readonly FiltroElementosViewModel _vm;
        public FiltroElementosView( FiltroElementosViewModel viewModel)
        {
            InitializeComponent();
            _vm=viewModel;
            DataContext = _vm;
        }

        // ─── Toggle scope ─────────────────────────────────────────────────
        private void BtnPorNivel_Click(object sender, MouseButtonEventArgs e)
        {
            if (_vm != null) _vm.IsAllModelMode = false;
        }

        private void BtnTodoModelo_Click(object sender, MouseButtonEventArgs e)
        {
            if (_vm != null) _vm.IsAllModelMode = true;
        }

        // ─── Select All Levels ────────────────────────────────────────────
        private void SelectAllLevels_Click(object sender, MouseButtonEventArgs e)
        {
            if (_vm != null)
                _vm.AllLevelsSelected = !_vm.AllLevelsSelected;
        }

        // ─── Level item click ─────────────────────────────────────────────
        private void LevelItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is LevelItem level)
                level.IsSelected = !level.IsSelected;
        }

        // ─── Category card click ──────────────────────────────────────────
        private void CategoryCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is CategoryItem cat)
                cat.IsSelected = !cat.IsSelected;
        }

        // ─── Preset color click ───────────────────────────────────────────
        private void Preset_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is string hex && _vm != null)
                _vm.HexColor = hex;
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {

            Close();
        }
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}