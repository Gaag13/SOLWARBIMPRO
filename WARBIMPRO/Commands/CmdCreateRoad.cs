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
                // 1. Mostrar ventana de parámetros
                var window = new RoadSectionWindow();
                if (window.ShowDialog() != true)
                    return Result.Cancelled;

                var sectionParams = window.Params;

                // 2. Seleccionar Toposolid base
                TaskDialog.Show("Paso 1/2", "Selecciona el Toposolid base (terreno existente).");

                var topoRef = uidoc.Selection.PickObject(
                    ObjectType.Element,
                    new ToposolidFilter(),
                    "Clic sobre el Toposolid base");

                var toposolid = doc.GetElement(topoRef) as Toposolid;
                if (toposolid == null)
                {
                    TaskDialog.Show("Error", "El elemento seleccionado no es un Toposolid.");
                    return Result.Failed;
                }

                // 3. Seleccionar líneas del eje
                TaskDialog.Show("Paso 2/2", "Selecciona las líneas del eje de la vía — Enter para confirmar.");

                var axisPoints = PointExtractor.FromModelLines(uidoc, out string extractMsg);

                if (axisPoints.Count < 2)
                {
                    TaskDialog.Show("Error", $"Se necesitan al menos 2 puntos de eje.\n{extractMsg}");
                    return Result.Failed;
                }

                // 4. Crear la subdivisión de la vía
                var svc = new RoadSectionService(doc);
                var id = svc.CreateRoadSubdivision(toposolid, axisPoints, sectionParams, out string roadMsg);

                TaskDialog.Show("Resultado",
                    id != ElementId.InvalidElementId
                        ? $"✓ {roadMsg}\n{extractMsg}"
                        : $"✗ {roadMsg}");

                return id != ElementId.InvalidElementId ? Result.Succeeded : Result.Failed;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}