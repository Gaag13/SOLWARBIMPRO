using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using WARBIMPRO.Services;
using WARBIMPRO.Utils;

#if REVIT2024_OR_GREATER

namespace WARBIMPRO.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class CmdTestDelaunay : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;

            // 1. El usuario selecciona las líneas de borde en el modelo
            var points = PointExtractor.FromModelLines(uidoc, out string extractMsg);

            if (points.Count < 3)
            {
                TaskDialog.Show("Puntos insuficientes",
                    $"Se necesitan al menos 3 puntos.\n{extractMsg}");
                return Result.Cancelled;
            }

            // 2. Crear el Toposolid con triangulación Delaunay
            var svc = new SurfaceCreationService(doc);
            var id = svc.CreateSurface(points, out string surfaceMsg);

            TaskDialog.Show("Resultado",
                id != ElementId.InvalidElementId
                    ? $"✓ {surfaceMsg}\n\n{extractMsg}"
                    : $"✗ {surfaceMsg}");

            return id != ElementId.InvalidElementId ? Result.Succeeded : Result.Failed;
        }
    }
}
#endif