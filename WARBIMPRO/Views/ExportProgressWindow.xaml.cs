using System.Windows;
using System.Windows.Data;
using System;
using WARBIMPRO.ViewModels;

namespace WARBIMPRO.Views
{
    public partial class ExportProgressWindow : Window
    {
        public ExportProgressViewModel ProgressViewModel { get; }

        public ExportProgressWindow()
        {
            InitializeComponent();
            ProgressViewModel = new ExportProgressViewModel();
            DataContext = ProgressViewModel;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    // Convierte Progress (0-100) al ancho real en píxeles del contenedor
    public class PercentToWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values[0] is int percent && values[1] is double totalWidth)
                return Math.Max(0, totalWidth * percent / 100.0);
            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
            => throw new NotImplementedException();
    }
}