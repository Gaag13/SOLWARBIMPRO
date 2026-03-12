using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;
using WARBIMPRO.ViewModels;
using WARBIMPRO.Views;

namespace WARBIMPRO.Commands
{
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class CmdFiltroElementos : ExternalCommand
    {
        public override void Execute()
        {
            try
            {
                var uiDoc = UiDocument;
                var uiApp = UiApplication;

                // Verificar vista activa
                if (uiDoc?.ActiveView == null)
                {
                    TaskDialog.Show("WARBIMPRO", "No hay una vista activa válida.");
                    return; // ← faltaba el return
                }

                // Verificar tipo de vista compatible
                var viewType = uiDoc.ActiveView.ViewType;
                if (viewType == ViewType.Schedule ||
                    viewType == ViewType.ColumnSchedule ||
                    viewType == ViewType.PanelSchedule)
                {
                    TaskDialog.Show("WARBIMPRO",
                        "Esta herramienta no está disponible en vistas de tabla.\n" +
                        "Por favor, activa una vista 3D, planta o sección.");
                    return;
                }

                // Crear ViewModel y View
                // ✅ El converter ya está en Window.Resources del XAML, no hace falta agregarlo aquí
                // ✅ DataContext ya se asigna dentro del constructor, no hace falta repetirlo
                var viewModel = new FiltroElementosViewModel(uiApp);
                var window = new FiltroElementosView(viewModel);
                viewModel.CloseAction = () => window.Close();

                window.ShowDialog();
            }
            catch (Exception ex)
            {
                // ✅ Nunca dejes el catch vacío — así ves qué falla
                TaskDialog.Show("WARBIMPRO — Error", ex.Message + "\n\n" + ex.StackTrace);
            }
        }
    }
}