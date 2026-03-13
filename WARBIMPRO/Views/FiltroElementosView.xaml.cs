using System.Windows;
using System.Windows.Input;
using WARBIMPRO.Models;
using WARBIMPRO.ViewModels;

namespace WARBIMPRO.Views
{
    public partial class FiltroElementosView : Window
    {
        private readonly FiltroElementosViewModel _vm;

        public FiltroElementosView(FiltroElementosViewModel viewModel)
        {
            InitializeComponent();
            _vm = viewModel;
            DataContext = _vm;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        // ─── Tabs ─────────────────────────────────────────────────────────────
        private void Tab_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement el && el.Tag is string tab)
                _vm.ActiveTab = tab;
        }

        // ─── Modo nivel / todo modelo ─────────────────────────────────────────
        private void BtnPorNivel_Click(object sender, MouseButtonEventArgs e) => _vm.IsAllModelMode = false;
        private void BtnTodoModelo_Click(object sender, MouseButtonEventArgs e) => _vm.IsAllModelMode = true;
        private void SelectAllLevels_Click(object sender, MouseButtonEventArgs e) => _vm.AllLevelsSelected = !_vm.AllLevelsSelected;

        // ─── Nivel ───────────────────────────────────────────────────────────
        private void LevelItem_Click(object sender, MouseButtonEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is LevelItem level)
                level.IsSelected = !level.IsSelected;
        }

        // ─── Categoría ───────────────────────────────────────────────────────
        private void CategoryCard_Click(object sender, MouseButtonEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is CategoryItem cat)
                cat.IsSelected = !cat.IsSelected;
        }

        // ─── Tipo ─────────────────────────────────────────────────────────────
        private void TypePill_Click(object sender, MouseButtonEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is TypeItem type)
                type.IsSelected = !type.IsSelected;
        }

        // ─── Presets de color ─────────────────────────────────────────────────
        private void Preset_Click(object sender, MouseButtonEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is string hex)
                _vm.SetPresetColor(hex);
        }
    }
}
