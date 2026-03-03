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
    /// Lógica de interacción para ViewDuplicateElement.xaml
    /// </summary>
    public partial class ViewDuplicateElement : Window
    {
        public ViewDuplicateElement(DuplicateElementViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
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
