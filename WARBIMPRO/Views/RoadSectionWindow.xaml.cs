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
            if (!TryParse(TxtWidth.Text, out double width) || width <= 0)
            { MessageBox.Show("Ancho de vía inválido.", "Error"); TxtWidth.Focus(); return; }

            if (!TryParse(TxtCrossSlope.Text, out double crossSlope))
            { MessageBox.Show("Pendiente transversal inválida.", "Error"); TxtCrossSlope.Focus(); return; }

            if (!TryParse(TxtStartElev.Text, out double startElev))
            { MessageBox.Show("Cota de inicio inválida.", "Error"); TxtStartElev.Focus(); return; }

            if (!TryParse(TxtLongSlope.Text, out double longSlope))
            { MessageBox.Show("Pendiente longitudinal inválida.", "Error"); TxtLongSlope.Focus(); return; }

            if (!TryParse(TxtSpacing.Text, out double spacing) || spacing <= 0)
            { MessageBox.Show("Espaciado inválido.", "Error"); TxtSpacing.Focus(); return; }

            Params = new RoadSectionParams
            {
                RoadWidthMeters = width,
                CrossSlopePercent = crossSlope,
                StartElevationMeters = startElev,
                LongSlopePercent = longSlope,
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

        private bool TryParse(string text, out double value) =>
            double.TryParse(text.Trim().Replace(',', '.'),
                NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }
}