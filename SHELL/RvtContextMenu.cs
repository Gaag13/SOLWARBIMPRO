using System;
using System.Runtime.InteropServices;
using System.Windows;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using System.Windows.Forms;
using System.Linq;
using WARBIMPRO.ShellExtension.Services;
using WARBIMPRO.ShellExtension.Views;

// ─────────────────────────────────────────────────────────────────────────────
// IMPORTANTE: Este GUID identifica tu Shell Extension en el registro de Windows.
// NUNCA cambies este GUID una vez distribuido — rompería el registro en PCs existentes.
// ─────────────────────────────────────────────────────────────────────────────
[assembly: Guid("97a8f1c7-a405-43fa-865f-34b094495733")]

namespace WARBIMPRO.ShellExtension
{
    /// <summary>
    /// Shell Extension que agrega "WARBIMPRO - Info del proyecto" al menú contextual
    /// de archivos .rvt en el Explorador de Windows.
    ///
    /// Cómo funciona:
    /// 1. Windows registra esta DLL como un handler COM para archivos .rvt
    /// 2. Cuando el usuario hace clic derecho en un .rvt, Windows llama a CanShowMenu()
    /// 3. Si retorna true, llama a CreateMenu() para obtener los ítems del menú
    /// 4. Cuando el usuario elige el ítem, se ejecuta OnItemClicked()
    /// </summary>
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".rvt")]
    [DisplayName("WARBIMPRO Shell Extension")]
    public class RvtContextMenu : SharpContextMenu
    {
        /// <summary>
        /// ¿Debe mostrarse el menú? Solo para archivos .rvt individuales.
        /// </summary>
        protected override bool CanShowMenu()
        {
            // Mostrar solo si hay exactamente un archivo seleccionado
            return SelectedItemPaths.Count() == 1;
        }

        /// <summary>
        /// Construye el menú contextual con el ítem de WARBIMPRO
        /// </summary>
        protected override ContextMenuStrip CreateMenu()
        {
            var menu = new ContextMenuStrip();

            // Ítem principal
            var item = new ToolStripMenuItem
            {
                Text  = "WARBIMPRO — Info del proyecto",
                //Image = Properties.Resources.WarbimproIcon16 // Ícono 16x16 (ver nota abajo)
            };

            item.Click += OnItemClicked;
            menu.Items.Add(item);

            return menu;
        }

        /// <summary>
        /// Se ejecuta cuando el usuario hace clic en el ítem del menú.
        /// Lee el archivo .rvt y muestra la ventana WPF con la información.
        /// </summary>
        private void OnItemClicked(object sender, EventArgs e)
        {
            try
            {
                string filePath = SelectedItemPaths.First();

                // Leer info del archivo .rvt (sin abrir Revit)
                var revitInfo = RevitFileReader.Read(filePath);

                // Mostrar ventana WPF
                // Nota: necesitamos un STA thread para WPF desde una Shell Extension
                var thread = new System.Threading.Thread(() =>
                {
                    var window = new RevitInfoWindow(revitInfo);
                    window.ShowDialog();
                });

                thread.SetApartmentState(System.Threading.ApartmentState.STA);
                thread.Start();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"Error al leer el archivo Revit:\n{ex.Message}",
                    "WARBIMPRO Shell Extension",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
