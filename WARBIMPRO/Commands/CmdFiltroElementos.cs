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

                if (uiDoc?.ActiveView == null)
                {
                    TaskDialog.Show("WARBIMPRO", "No hay una vista activa válida.");
                    return; 
                }
               
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
                var viewModel = new FiltroElementosViewModel(uiApp);
                var window = new FiltroElementosView(viewModel);
                viewModel.CloseAction = () => window.Close();
                window.ShowDialog();
            }
            catch (Exception ex)
            {
               
                TaskDialog.Show("WARBIMPRO — Error", ex.Message + "\n\n" + ex.StackTrace);
            }
        }
    }
}