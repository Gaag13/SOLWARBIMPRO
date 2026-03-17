using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WARBIMPRO.Utils
{
    public static class PointExtractor
    {
        private const double MetersToFeet = 3.28083989501;

        public static List<XYZ> FromModelLines(UIDocument uidoc, out string message)
        {
            message = string.Empty;
            var points = new List<XYZ>();

            try
            {
                var refs = uidoc.Selection.PickObjects(
                    ObjectType.Element,
                    new LineFilter(),
                    "Selecciona las líneas del borde — Enter para confirmar");

                foreach (var r in refs)
                {
                    var elem = uidoc.Document.GetElement(r);
                    if (elem?.Location is LocationCurve lc)
                    {
                        AddUnique(points, lc.Curve.GetEndPoint(0));
                        AddUnique(points, lc.Curve.GetEndPoint(1));
                    }
                }

                message = $"{points.Count} puntos extraídos de {refs.Count} líneas.";
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                message = "Selección cancelada.";
            }

            return points;
        }

        public static List<XYZ> FromCsv(string filePath, bool convertFromMeters, out string message)
        {
            message = string.Empty;
            var points = new List<XYZ>();
            int skipped = 0;

            try
            {
                foreach (var line in System.IO.File.ReadAllLines(filePath))
                {
                    var t = line.Trim();
                    if (string.IsNullOrEmpty(t) || t.StartsWith("#") || char.IsLetter(t[0]))
                    { skipped++; continue; }

                    var parts = t.Split(new[] { ',', ';', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 3) { skipped++; continue; }

                    if (TryParse(parts[0], out double x) &&
                        TryParse(parts[1], out double y) &&
                        TryParse(parts[2], out double z))
                    {
                        double f = convertFromMeters ? MetersToFeet : 1.0;
                        AddUnique(points, new XYZ(x * f, y * f, z * f));
                    }
                    else skipped++;
                }

                message = $"{points.Count} puntos cargados. {skipped} líneas omitidas.";
            }
            catch (Exception ex)
            {
                message = $"Error leyendo CSV: {ex.Message}";
            }

            return points;
        }

        private static void AddUnique(List<XYZ> list, XYZ p, double tol = 0.01)
        {
            if (!list.Any(q => Math.Abs(q.X - p.X) < tol && Math.Abs(q.Y - p.Y) < tol))
                list.Add(p);
        }

        private static bool TryParse(string s, out double v) =>
            double.TryParse(s.Trim().Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out v);
    }

    /// <summary>Solo permite seleccionar líneas de modelo.</summary>
    public class LineFilter : ISelectionFilter
    {
        public bool AllowElement(Element e) =>
            e is ModelLine || e is ModelCurve || e is DetailLine;
        public bool AllowReference(Reference r, XYZ p) => true;
    }

    /// <summary>Solo permite seleccionar Toposolids.</summary>
    public class ToposolidFilter : ISelectionFilter
    {
        public bool AllowElement(Element e) => e is Toposolid;
        public bool AllowReference(Reference r, XYZ p) => true;
    }
}