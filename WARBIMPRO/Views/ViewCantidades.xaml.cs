using Autodesk.Revit.UI;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WARBIMPRO.Models;
using WARBIMPRO.ViewModels;
using ClosedXML.Excel;
using System.Diagnostics;


namespace WARBIMPRO.Views
{
    /// <summary>
    /// Lógica de interacción para ViewCantidades.xaml
    /// </summary>
    public partial class ViewCantidades : Window
    {
        private CantidadesViewModel _viewModel;
        public ViewCantidades(CantidadesViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            DataContext = _viewModel;

            LlenarTreeView(_viewModel.TiposElementosNivel);
            dta_grid1.ItemsSource = _viewModel.ContenidoTabla;

            int totalElementos = dta_grid1.Items.Count;
            double volumenTotal = CalculandoVolumen(dta_grid1);

            lbl_TotalElementos.Content = $"Total de elementos: {totalElementos}";
            lbl_VolumenTotal.Content = $"El volumen total es: {volumenTotal.ToString("F2")} m³";

            double totalArena = _viewModel.ContenidoTabla.Sum(data => data.Arena);
            double totalGrava = _viewModel.ContenidoTabla.Sum(data => data.Grava);
            double totalAgua = _viewModel.ContenidoTabla.Sum(data => data.Agua);

            
        }

        private void CalcularCantidades_Click(object sender, RoutedEventArgs e)
        {
            var WARBIMPRO = _viewModel.CalcularCantidadesMaterial();
            MessageBox.Show($"Cemento: {WARBIMPRO["Cemento"]}\nArena: {WARBIMPRO["Arena"]}\nGrava: {WARBIMPRO["Grava"]}\nAgua: {WARBIMPRO["Agua"]}");

        }
        private void dra_grid1_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedItems = dta_grid1.SelectedItems;
            var ids = selectedItems.OfType<Data>().Select(data => data.ID).ToList();

            _viewModel.SeleccionarElementosRevit(ids); // Delegamos la selección de elementos al ViewModel
        }
        private void LlenarTreeView(Dictionary<string, List<string>> tiposElementosNivel)
        {
            tre_v1.Items.Clear();
            foreach (var nivel in tiposElementosNivel)
            {
                TreeViewItem nivelItem = new TreeViewItem { Header = nivel.Key };
                foreach (var categoria in nivel.Value)
                {
                    nivelItem.Items.Add(new TreeViewItem { Header = categoria });
                }
                tre_v1.Items.Add(nivelItem);
            }
        }
        private void tre_v1_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var selectedItem = tre_v1.SelectedItem as TreeViewItem;
            if (selectedItem != null && selectedItem.Parent != null)
            {
                var parentItem = (selectedItem.Parent as TreeViewItem).Header.ToString();
                var selectedCategory = selectedItem.Header.ToString();

                List<Data> contenidoTablas = _viewModel.ContenidoTabla
                    .Where(ct => ct.NIVEL == parentItem && ct.CATEGORIA == selectedCategory)
                    .ToList();

                dta_grid1.ItemsSource = contenidoTablas;
            }
            else if (selectedItem != null)
            {
                var selectedLevel = selectedItem.Header.ToString();
                List<Data> contenidoTablas = _viewModel.ContenidoTabla
                    .Where(ct => ct.NIVEL == selectedLevel)
                    .ToList();

                dta_grid1.ItemsSource = contenidoTablas;
            }
            lbl_TotalElementos.Content = dta_grid1.Items.Count.ToString();
            lbl_VolumenTotal.Content = CalculandoVolumen(dta_grid1).ToString();
        }

        private double CalculandoVolumen(DataGrid dataGrid)
        {
            double volumenTotal = 0;

            foreach (var item in dataGrid.Items)
            {
                var fila = item as Data;  // Reemplaza 'Data' con el tipo correcto de tu fila
                if (fila != null)
                {
                    if (fila.VOLUMEN != null)
                    {
                        // Elimina las unidades "m³" y cualquier espacio en blanco
                        string volumenString = fila.VOLUMEN.ToString()
                            .Replace("m³", "")   // Elimina la unidad "m³"
                            .Trim();             // Elimina cualquier espacio extra

                        // Intenta convertir el valor numérico a double
                        if (double.TryParse(volumenString, System.Globalization.NumberStyles.Any,
                                            System.Globalization.CultureInfo.InvariantCulture, out double volumen))
                        {
                            volumenTotal += volumen;
                        }
                        else
                        {
                            // Aquí puedes manejar el error si la conversión falla
                            throw new FormatException($"El valor '{fila.VOLUMEN}' no tiene un formato numérico válido.");
                        }
                    }
                }
            }
            // Redondea el volumen total a 2 decimales
            return System.Math.Round(volumenTotal, 2);
        }

        private void btn_ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel (*.xlsx)|*.xlsx",
                FileName = "Exportacion.xlsx"
            };

            if (saveDialog.ShowDialog() != true)
                return;

            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Datos");

                int fila = 1;
                int col = 1;

                // Encabezados
                foreach (var column in dta_grid1.Columns)
                {
                    ws.Cell(fila, col).Value = column.Header?.ToString();
                    ws.Cell(fila, col).Style.Font.Bold = true;
                    col++;
                }

                // Datos
                foreach (var item in dta_grid1.Items)
                {
                    fila++;
                    col = 1;

                    if (item is Data d)
                    {
                        ws.Cell(fila, col++).Value = d.ID;
                        ws.Cell(fila, col++).Value = d.NIVEL;
                        ws.Cell(fila, col++).Value = d.CATEGORIA;
                        ws.Cell(fila, col++).Value = d.AREA;
                        ws.Cell(fila, col++).Value = d.VOLUMEN;
                        ws.Cell(fila, col++).Value = d.Cemento;
                        ws.Cell(fila, col++).Value = d.Arena;
                        ws.Cell(fila, col++).Value = d.Grava;
                        ws.Cell(fila, col++).Value = d.Agua;
                    }
                }

                ws.Columns().AdjustToContents();
                wb.SaveAs(saveDialog.FileName);
            }

            //Process.Start(new ProcessStartInfo
            //{
            //    FileName = saveDialog.FileName,
            //    UseShellExecute = true
            //});

            TaskDialog.Show("Exportación", "Excel generado correctamente 🚀");
        }
        //private void btn_ExportExcel_Click(object sender, RoutedEventArgs e)
        //{
        //    var excel = new Microsoft.Office.Interop.Excel.Application();
        //    excel.Workbooks.Add(true);

        //    int IndiceColumna = 1;
        //    int IndiceFilas = 1;

        //    // Configurar la cultura para formatear los números
        //    var culturaEspañola = new System.Globalization.CultureInfo("es-ES");

        //    foreach (var column in dta_grid1.Columns)
        //    {
        //        excel.Cells[IndiceFilas, IndiceColumna] = column.Header;
        //        IndiceColumna++;
        //    }

        //    foreach (var row in dta_grid1.Items)
        //    {
        //        IndiceFilas++;
        //        IndiceColumna = 1;

        //        var fila = row as Data;
        //        if (fila != null)
        //        {
        //            excel.Cells[IndiceFilas, IndiceColumna] = fila.ID;
        //            excel.Cells[IndiceFilas, IndiceColumna + 1] = fila.NIVEL;
        //            excel.Cells[IndiceFilas, IndiceColumna + 2] = fila.CATEGORIA;                    
        //            excel.Cells[IndiceFilas, IndiceColumna + 3] = fila.AREA;
        //            excel.Cells[IndiceFilas, IndiceColumna + 4] = fila.VOLUMEN;
        //            excel.Cells[IndiceFilas, IndiceColumna + 5] = fila.Cemento;
        //            excel.Cells[IndiceFilas, IndiceColumna + 6] = fila.Arena;
        //            excel.Cells[IndiceFilas, IndiceColumna + 7] = fila.Grava;
        //            excel.Cells[IndiceFilas, IndiceColumna + 8] = fila.Agua;
        //        }
        //    }
           


        //    excel.Visible = true;
        //}

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
            
            Close();
        }

        #endregion
    }
}
