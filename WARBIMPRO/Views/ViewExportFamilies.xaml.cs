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
using System.Windows.Shapes;
using WARBIMPRO.ViewModels;

namespace WARBIMPRO.Views
{
    /// <summary>
    /// Lógica de interacción para ViewExportFamilies.xaml
    /// </summary>
    public partial class ViewExportFamilies : Window
    {
        public ViewExportFamilies(Document doc)
        {
            InitializeComponent();
            DataContext = new ExportFamiliesViewModel(doc);
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
