using Autodesk.Revit.DB.PointClouds;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace WARBIMPRO.Views
{
    /// <summary>
    /// Lógica de interacción para ViewResistencia.xaml
    /// </summary>
    public partial class ViewResistencia : Window
    {

        public ViewResistencia()
        {
            InitializeComponent();
        }

        public string Resistecia { get; set; }
        public string Cemento { get; set; }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string resistencia = ((ComboBoxItem)ResistenciasComboBox.SelectedItem).Content.ToString();

            Resistecia = resistencia;

            string cemento = ((ComboBoxItem)CementoComboBox.SelectedItem).Content.ToString();
            Cemento = cemento;

            Close();
        }
        #region BOTONES CERRAR

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        //private void MaximizeRestoreButton_Click(object sender, MouseButtonEventArgs e)
        //{
        //    WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        //}

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(Resistecia) || string.IsNullOrEmpty(Cemento))
                {
                    MessageBox.Show("Por favor seleccione una resistencia y un tipo de cemento.");
                    return;
                }
            }
            catch (System.NullReferenceException)
            {

                Close();
            }

        }
        #endregion
    }
}



            
