using Autodesk.Revit.DB;
using WARBIMPRO.Models;
using System;
using System.Collections.Generic;
using System.Linq;


#if REVIT2024_OR_GREATER
namespace WARBIMPRO.Services
{
    public class SurfaceCreationService
    {
        private readonly Document _doc;

        public SurfaceCreationService(Document doc)
        {
            _doc = doc;
        }

        public ElementId CreateSurface(List<XYZ> points, out string message)
        {
            message = string.Empty;

            if (points == null || points.Count < 3)
            {
                message = "Se necesitan al menos 3 puntos.";
                return ElementId.InvalidElementId;
            }

            try
            {
                // 1. Triangulación Delaunay
                var tinPoints = points
                    .Select((p, i) => new TinPoint(p.X, p.Y, p.Z, i))
                    .ToList();

                var triangles = DelaunayTriangulator.Triangulate(tinPoints);

                // 2. Obtener tipo y nivel
                var typeId = GetToposolidTypeId();
                var levelId = GetDefaultLevelId();

                // 3. Convex hull (lo calculamos UNA vez y lo reutilizamos)
                var hull = ConvexHull(points);

                // 4. Construir el contorno exterior como CurveLoop a partir del hull
                var boundary = BuildBoundaryCurveLoop(points, hull);

                using (var trans = new Transaction(_doc, "WARBIMPRO: Crear Toposolid Vial"))
                {
                    trans.Start();
                    try
                    {
                        var toposolid = Toposolid.Create(
                            _doc,
                            new List<CurveLoop> { boundary },
                            typeId,
                            levelId);

                        var editHandle = toposolid.GetSlabShapeEditor();
                        if (editHandle != null)
                        {
                            editHandle.Enable();

#if !REVIT2025_OR_GREATER
                            // 2024: DrawPoint es tolerante, se puede pasar todos los puntos
                            foreach (var pt in points)
                                editHandle.DrawPoint(pt);
#else
                            // 2025+: AddPoint falla si el punto coincide con un vértice
                            // de borde que ya existe (los del hull). Filtramos esos.
                            var hullSet = new HashSet<XYZ>(hull, new XYZEqualityComparer(1e-6));

                            foreach (var pt in points)
                            {
                                if (hullSet.Contains(pt))
                                    continue; // ya es un vértice del contorno, no re-agregar

                                editHandle.AddPoint(pt);
                            }
#endif
                        }

                        trans.Commit();
                        message = $"Toposolid creado: {points.Count} puntos, {triangles.Count} triángulos Delaunay.";
                        return toposolid.Id;
                    }
                    catch
                    {
                        trans.RollBack();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                message = $"Error: {ex.Message}";
                return ElementId.InvalidElementId;
            }
        }

        /// <summary>
        /// Construye el CurveLoop del contorno exterior a partir del hull ya calculado.
        /// Proyecta todo a Z=0 (Z del primer punto) porque el contorno es plano.
        /// </summary>
        private CurveLoop BuildBoundaryCurveLoop(List<XYZ> points, List<XYZ> hull)
        {
            var loop = new CurveLoop();

            for (int i = 0; i < hull.Count; i++)
            {
                var a = hull[i];
                var b = hull[(i + 1) % hull.Count];
                var pa = new XYZ(a.X, a.Y, points[0].Z);
                var pb = new XYZ(b.X, b.Y, points[0].Z);
                loop.Append(Line.CreateBound(pa, pb));
            }

            return loop;
        }

        /// <summary>
        /// Convex hull 2D (Andrew's monotone chain).
        /// </summary>
        private List<XYZ> ConvexHull(List<XYZ> pts)
        {
            var sorted = pts.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
            var hull = new List<XYZ>();

            foreach (var p in sorted)
            {
                while (hull.Count >= 2 && Cross(hull[hull.Count - 2], hull[hull.Count - 1], p) <= 0)
                    hull.RemoveAt(hull.Count - 1);
                hull.Add(p);
            }

            int lower = hull.Count + 1;
            for (int i = sorted.Count - 2; i >= 0; i--)
            {
                var p = sorted[i];
                while (hull.Count >= lower && Cross(hull[hull.Count - 2], hull[hull.Count - 1], p) <= 0)
                    hull.RemoveAt(hull.Count - 1);
                hull.Add(p);
            }

            hull.RemoveAt(hull.Count - 1);
            return hull;
        }

        private double Cross(XYZ o, XYZ a, XYZ b)
            => (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X);

        private ElementId GetToposolidTypeId()
        {
            var type = new FilteredElementCollector(_doc)
                .OfClass(typeof(ToposolidType))
                .Cast<ToposolidType>()
                .FirstOrDefault();

            if (type == null)
                throw new InvalidOperationException("No se encontró ningún tipo de Toposolid en el proyecto.");

            return type.Id;
        }

        private ElementId GetDefaultLevelId()
        {
            var level = new FilteredElementCollector(_doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .OrderBy(l => l.Elevation)
                .FirstOrDefault();

            if (level == null)
                throw new InvalidOperationException("No se encontró ningún nivel en el proyecto.");

            return level.Id;
        }

        /// <summary>
        /// Comparador de XYZ con tolerancia, solo en X,Y (para detectar si un punto
        /// coincide en planta con un vértice del hull, sin importar su Z).
        /// </summary>
        private class XYZEqualityComparer : IEqualityComparer<XYZ>
        {
            private readonly double _tol;
            public XYZEqualityComparer(double tol) => _tol = tol;

            public bool Equals(XYZ a, XYZ b) =>
                Math.Abs(a.X - b.X) < _tol && Math.Abs(a.Y - b.Y) < _tol;

            public int GetHashCode(XYZ p) =>
                (Math.Round(p.X, 4), Math.Round(p.Y, 4)).GetHashCode();
        }
    }
}
#endif