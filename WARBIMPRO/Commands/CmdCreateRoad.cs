using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using WARBIMPRO.Services;
using WARBIMPRO.Utils;
using WARBIMPRO.Views;
using System;

namespace WARBIMPRO.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class CmdCreateRoad : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;

            try
            {
                var window = new RoadSectionWindow();
                if (window.ShowDialog() != true)
                    return Result.Cancelled;

                var sectionParams = window.Params;

                TaskDialog.Show("Paso 1/2", "Selecciona el Toposolid base.");
                var topoRef = uidoc.Selection.PickObject(ObjectType.Element,
                    new ToposolidFilter(), "Clic sobre el Toposolid base");
                var toposolid = doc.GetElement(topoRef) as Toposolid;

                if (toposolid == null)
                {
                    TaskDialog.Show("Error", "El elemento no es un Toposolid.");
                    return Result.Failed;
                }

                TaskDialog.Show("Paso 2/2", "Selecciona las líneas del eje — Enter para confirmar.");
                var axisPoints = PointExtractor.FromModelLines(uidoc, out string extractMsg);

                if (axisPoints.Count < 2)
                {
                    TaskDialog.Show("Error", $"Se necesitan al menos 2 puntos.\n{extractMsg}");
                    return Result.Failed;
                }

                var svc = new RoadSectionService(doc);
                var result = svc.CreateRoadSubdivision(toposolid, axisPoints, sectionParams, out string roadMsg);

                TaskDialog.Show("Resultado",
                    result == Result.Succeeded
                        ? $"✓ {roadMsg}\n{extractMsg}"
                        : $"✗ {roadMsg}");

                return result;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                // Error detallado para debug
                TaskDialog.Show("Error detallado",
                    $"Tipo: {ex.GetType().Name}\n\n" +
                    $"Mensaje: {ex.Message}\n\n" +
                    $"Inner: {ex.InnerException?.Message}\n\n" +
                    $"Stack:\n{ex.StackTrace}");
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}