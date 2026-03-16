//using Autodesk.Revit.Attributes;
//using Autodesk.Revit.DB;
//using Autodesk.Revit.UI;
//using WARBIMPRO.DockablePanes;
//using System;

//namespace WARBIMPRO.Commands
//{
//    /// <summary>
//    /// Abre / muestra el panel de Superficies Viales.
//    /// Regístralo en Application.cs igual que tu CmdKeyFireSharp:
//    ///
//    ///   panel.AddPushButton&lt;CmdSurfaceTool&gt;("Superficies\nViales")
//    ///        .SetLargeImage("/WARBIMPRO;component/Resources/Icons/Surface_dark.png")
//    ///        ...
//    /// </summary>
//    [Transaction(TransactionMode.Manual)]
//    [Regeneration(RegenerationOption.Manual)]
//    public class CmdSurfaceTool : IExternalCommand
//    {
//        // GUID que registraste en Application.cs para este pane
//        public static readonly Guid PaneGuid = new Guid("C1D2E3F4-A5B6-7890-CDEF-123456789ABC");

//        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
//        {
//            try
//            {
//                var uiApp = commandData.Application;
//                var paneId = new DockablePaneId(PaneGuid);
//                var pane   = uiApp.GetDockablePane(paneId);

//                // Pasar el contexto de Revit al ViewModel
//                if (uiApp.ActiveUIDocument != null)
//                {
//                    var provider = SurfaceToolPaneProvider.Instance;
//                    provider?.Pane.SetRevitContext(uiApp.ActiveUIDocument);
//                }

//                // Mostrar si está oculto, ocultar si está visible
//                if (pane.IsShown())
//                    pane.Hide();
//                else
//                    pane.Show();

//                return Result.Succeeded;
//            }
//            catch (Exception ex)
//            {
//                message = ex.Message;
//                return Result.Failed;
//            }
//        }
//    }

//    /// <summary>
//    /// Provider del DockablePane — registra el panel en Revit.
//    ///
//    /// En tu Application.cs → OnStartup(), agrega:
//    ///
//    ///   DockablePaneProvider.Register(Application, CmdSurfaceTool.PaneGuid, "Superficies Viales")
//    ///       .SetConfiguration(data => {
//    ///           SurfaceToolPaneProvider.Instance = new SurfaceToolPaneProvider();
//    ///           SurfaceToolPaneProvider.Instance.SetupDockablePane(data);
//    ///       });
//    /// </summary>
//    public class SurfaceToolPaneProvider : IDockablePaneProvider
//    {
//        public static SurfaceToolPaneProvider? Instance { get; set; }

//        public SurfaceToolPane Pane { get; } = new SurfaceToolPane();

//        public void SetupDockablePane(DockablePaneProviderData data)
//        {
//            Pane.SetupDockablePane(data);
//        }
//    }
//}
