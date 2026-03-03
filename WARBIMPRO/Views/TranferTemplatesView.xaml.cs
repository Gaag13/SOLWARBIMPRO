using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WARBIMPRO.ViewModels;

namespace WARBIMPRO.Views
{
    /// <summary>
    /// Lógica de interacción para TranferTemplatesView.xaml
    /// </summary>
    public partial class TranferTemplatesView :Window
    {
        public TranferTemplatesView(TransferViewTemplateViewModel ViewModel)
        {
            InitializeComponent();
            DataContext = ViewModel;
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
