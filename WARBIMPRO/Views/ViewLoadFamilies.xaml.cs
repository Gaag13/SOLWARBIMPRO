using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WARBIMPRO.ViewModels;

namespace WARBIMPRO.Views
{
    /// <summary>
    /// Lógica de interacción para ViewSettings.xaml
    /// </summary>
    public partial class ViewLoadFamilies : Window
    {
        public ViewLoadFamilies(LoadFamiliesViewModel viewModel)
        {
            InitializeComponent();

            viewModel.OnLoadStarted = () => this.WindowState = WindowState.Minimized;
            viewModel.OnLoadFinished = () => this.WindowState = WindowState.Normal;

            DataContext = viewModel;
        }
        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {

            Close();
        }
        private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
