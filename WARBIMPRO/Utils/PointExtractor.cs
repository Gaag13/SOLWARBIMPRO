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
            var segments = new List<List<XYZ>>();

            try
            {
                var refs = uidoc.Selection.PickObjects(
                    ObjectType.Element,
                    new LineFilter(),
                    "Selecciona las líneas del eje — Enter para confirmar");

                foreach (var r in refs)
                {
                    var elem = uidoc.Document.GetElement(r);
                    if (elem?.Location is LocationCurve lc)
                    {
                        var curve = lc.Curve;
                        var segPts = new List<XYZ>();

                        if (curve is Line)
                        {
                            // Línea recta — solo extremos
                            segPts.Add(curve.GetEndPoint(0));
                            segPts.Add(curve.GetEndPoint(1));
                        }
                        else
                        {
                            // Curva — usar parámetros reales cada 0.5m
                            segPts.AddRange(GetPointsAlongCurve(curve, 0.5 * MetersToFeet));

                            // Asegurar que los extremos estén incluidos
                            if (segPts.Count == 0 || segPts.First().DistanceTo(curve.GetEndPoint(0)) > 0.01)
                                segPts.Insert(0, curve.GetEndPoint(0));
                            if (segPts.Last().DistanceTo(curve.GetEndPoint(1)) > 0.01)
                                segPts.Add(curve.GetEndPoint(1));
                        }

                        segments.Add(segPts);
                    }
                }

                var ordered = OrderSegments(segments);
                message = $"{ordered.Count} puntos extraídos de {refs.Count} líneas.";
                return ordered;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                message = "Selección cancelada.";
                return new List<XYZ>();
            }
        }

        /// <summary>
        /// Genera puntos a lo largo de una curva usando sus parámetros reales.
        /// Más preciso que dividir por índice — respeta la parametrización de la curva.
        /// </summary>
        private static List<XYZ> GetPointsAlongCurve(Curve curve, double step = 0.3)
        {
            double pt0 = curve.GetEndParameter(0);
            double pt1 = curve.GetEndParameter(1);
            var pts = new List<XYZ>();
            int n = 1;

            while (true)
            {
                double dist = step * n;
                n++;
                if (dist > curve.Length) break;

                double paramCalc = pt0 + ((pt1 - pt0) * dist / curve.Length);
                if (curve.IsInside(paramCalc))
                {
                    double normParam = curve.ComputeNormalizedParameter(paramCalc);
                    pts.Add(curve.Evaluate(normParam, true));
                }
            }

            return pts;
        }

        /// <summary>
        /// Ordena los segmentos en cadena continua.
        /// Cada segmento se conecta al extremo más cercano del anterior.
        /// </summary>
        private static List<XYZ> OrderSegments(List<List<XYZ>> segments)
        {
            if (!segments.Any()) return new List<XYZ>();
            if (segments.Count == 1) return segments[0];

            var result = new List<XYZ>();
            var pending = new List<List<XYZ>>(segments);

            result.AddRange(pending[0]);
            pending.RemoveAt(0);

            while (pending.Any())
            {
                XYZ lastPt = result.Last();
                double minDist = double.MaxValue;
                int bestIdx = 0;
                bool bestReverse = false;

                for (int i = 0; i < pending.Count; i++)
                {
                    double dStart = lastPt.DistanceTo(pending[i].First());
                    double dEnd = lastPt.DistanceTo(pending[i].Last());

                    if (dStart < minDist) { minDist = dStart; bestIdx = i; bestReverse = false; }
                    if (dEnd < minDist) { minDist = dEnd; bestIdx = i; bestReverse = true; }
                }

                var next = pending[bestIdx];
                pending.RemoveAt(bestIdx);

                if (bestReverse) next = next.AsEnumerable().Reverse().ToList();

                result.AddRange(next.Skip(1));
            }

            return result;
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

    public class LineFilter : ISelectionFilter
    {
        public bool AllowElement(Element e) =>
            e is ModelLine ||
            e is ModelCurve ||
            e is DetailLine ||
            e is DetailCurve ||
            e?.Location is LocationCurve;
        public bool AllowReference(Reference r, XYZ p) => true;
    }

    public class ToposolidFilter : ISelectionFilter
    {
        public bool AllowElement(Element e) => e is Toposolid;
        public bool AllowReference(Reference r, XYZ p) => true;
    }
}