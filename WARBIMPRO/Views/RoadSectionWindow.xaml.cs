using Autodesk.Revit.DB;
using WARBIMPRO.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WARBIMPRO.Views
{
    public partial class RoadSectionWindow : Window
    {
        private readonly Document _doc;
        public RoadSectionParams Params { get; private set; }

        private ElementId _roadMaterialId = ElementId.InvalidElementId;
        private ElementId _leftMaterialId = ElementId.InvalidElementId;
        private ElementId _rightMaterialId = ElementId.InvalidElementId;

        public RoadSectionWindow(Document doc)
        {
            _doc = doc;
            InitializeComponent();
        }


        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) return; // evitar doble clic
            DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }


        private void ChkSidewalk_Changed(object sender, RoutedEventArgs e)
        {
            if (PanelLeft != null) PanelLeft.IsEnabled = ChkLeftSidewalk.IsChecked == true;
            if (PanelRight != null) PanelRight.IsEnabled = ChkRightSidewalk.IsChecked == true;
        }

        private void BtnRoadMaterial_Click(object sender, RoutedEventArgs e)
        {
            var mat = PickMaterial("Material — Vía");
            if (mat == null) return;
            _roadMaterialId = mat.Id;
            BtnRoadMaterial.Content = "● " + mat.Name;
        }

        private void BtnLeftMaterial_Click(object sender, RoutedEventArgs e)
        {
            var mat = PickMaterial("Material — Andén Izquierdo");
            if (mat == null) return;
            _leftMaterialId = mat.Id;
            BtnLeftMaterial.Content = "● " + mat.Name;
        }

        private void BtnRightMaterial_Click(object sender, RoutedEventArgs e)
        {
            var mat = PickMaterial("Material — Andén Derecho");
            if (mat == null) return;
            _rightMaterialId = mat.Id;
            BtnRightMaterial.Content = "● " + mat.Name;
        }

        private Material PickMaterial(string title)
        {
            var materials = new FilteredElementCollector(_doc)
                .OfClass(typeof(Material))
                .Cast<Material>()
                .OrderBy(m => m.Name)
                .ToList();

            var picker = new MaterialPickerWindow(materials, title) { Owner = this };
            return picker.ShowDialog() == true ? picker.SelectedMaterial : null;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (!TryParse(TxtWidth.Text, out double width) || width <= 0)
            { MessageBox.Show("Ancho de vía inválido."); TxtWidth.Focus(); return; }
            if (!TryParse(TxtCrossSlope.Text, out double crossSlope))
            { MessageBox.Show("Pendiente transversal inválida."); TxtCrossSlope.Focus(); return; }
            if (!TryParse(TxtStartElev.Text, out double startElev))
            { MessageBox.Show("Cota de inicio inválida."); TxtStartElev.Focus(); return; }
            if (!TryParse(TxtLongSlope.Text, out double longSlope))
            { MessageBox.Show("Pendiente longitudinal inválida."); TxtLongSlope.Focus(); return; }
            if (!TryParse(TxtSpacing.Text, out double spacing) || spacing <= 0)
            { MessageBox.Show("Espaciado inválido."); TxtSpacing.Focus(); return; }
            if (!TryParse(TxtSidewalkSlope.Text, out double swSlope))
            { MessageBox.Show("Pendiente de andenes inválida."); TxtSidewalkSlope.Focus(); return; }

            double leftW = 0, rightW = 0;
            if (ChkLeftSidewalk.IsChecked == true)
            {
                if (!TryParse(TxtLeftWidth.Text, out leftW) || leftW <= 0)
                { MessageBox.Show("Ancho andén izquierdo inválido."); TxtLeftWidth.Focus(); return; }
            }
            if (ChkRightSidewalk.IsChecked == true)
            {
                if (!TryParse(TxtRightWidth.Text, out rightW) || rightW <= 0)
                { MessageBox.Show("Ancho andén derecho inválido."); TxtRightWidth.Focus(); return; }
            }

            Params = new RoadSectionParams
            {
                RoadWidthMeters = width,
                CrossSlopePercent = crossSlope,
                StartElevationMeters = startElev,
                LongSlopePercent = longSlope,
                StationSpacingMeters = spacing,
                HasLeftSidewalk = ChkLeftSidewalk.IsChecked == true,
                LeftSidewalkWidthMeters = leftW,
                HasRightSidewalk = ChkRightSidewalk.IsChecked == true,
                RightSidewalkWidthMeters = rightW,
                SidewalkSlopePercent = swSlope,
                RoadMaterialId = _roadMaterialId,
                LeftMaterialId = _leftMaterialId,
                RightMaterialId = _rightMaterialId,
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
