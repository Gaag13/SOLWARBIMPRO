using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WARBIMPRO.Views
{
    public partial class MaterialPickerWindow : Window
    {
        public Material SelectedMaterial { get; private set; }
        private readonly List<Material> _allMaterials;

        public MaterialPickerWindow(List<Material> materials, string title)
        {
            _allMaterials = materials;
            Title = title;
            InitializeComponent();
            RefreshList(string.Empty);
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
            => RefreshList(TxtSearch.Text);

        private void RefreshList(string filter)
        {
            var filtered = string.IsNullOrWhiteSpace(filter)
                ? _allMaterials
                : _allMaterials.Where(m =>
                    m.Name.ToLower().Contains(filter.ToLower())).ToList();

            LstMaterials.ItemsSource = filtered;

            if (LstMaterials.Items.Count > 0)
                LstMaterials.SelectedIndex = 0;
        }

        private void LstMaterials_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LstMaterials.SelectedItem is Material mat)
            {
                SelectedMaterial = mat;
                DialogResult = true;
                Close();
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (LstMaterials.SelectedItem is Material mat)
            {
                SelectedMaterial = mat;
                DialogResult = true;
                Close();
            }
            else
                MessageBox.Show("Selecciona un material de la lista.", "Sin selección");
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
