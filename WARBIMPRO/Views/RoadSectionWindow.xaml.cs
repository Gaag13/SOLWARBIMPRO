using WARBIMPRO.Models;
using System.Globalization;
using System.Windows;

namespace WARBIMPRO.Views
{
    public partial class RoadSectionWindow : Window
    {
        public RoadSectionParams Params { get; private set; }

        public RoadSectionWindow()
        {
            InitializeComponent();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (!TryParseDouble(TxtWidth.Text, out double width) || width <= 0)
            {
                MessageBox.Show("Ancho de vía inválido.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtWidth.Focus();
                return;
            }

            if (!TryParseDouble(TxtSlope.Text, out double slope))
            {
                MessageBox.Show("Pendiente transversal inválida.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtSlope.Focus();
                return;
            }

            if (!TryParseDouble(TxtSpacing.Text, out double spacing) || spacing <= 0)
            {
                MessageBox.Show("Espaciado de estaciones inválido.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtSpacing.Focus();
                return;
            }

            Params = new RoadSectionParams
            {
                RoadWidthMeters = width,
                CrossSlopePercent = slope,
                StationSpacingMeters = spacing
            };

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool TryParseDouble(string text, out double value) =>
            double.TryParse(
                text.Trim().Replace(',', '.'),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out value);
    }
}