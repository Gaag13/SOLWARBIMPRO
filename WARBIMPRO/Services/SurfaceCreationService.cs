using Autodesk.Revit.DB;
using WARBIMPRO.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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

                // 3. Construir el contorno exterior como CurveLoop
                // Toposolid.Create necesita el borde exterior del área
                var boundary = BuildBoundaryCurveLoop(points);

                using (var trans = new Transaction(_doc, "WARBIMPRO: Crear Toposolid Vial"))
                {
                    trans.Start();
                    try
                    {
                        // Crear Toposolid con el contorno
                        var toposolid = Toposolid.Create(
                            _doc,
                            new List<CurveLoop> { boundary },
                            typeId,
                            levelId);

                        // Agregar los puntos interiores con sus cotas
                        // Esto es lo que controla la forma de la superficie
                        var editHandle = toposolid.GetSlabShapeEditor();
                        if (editHandle != null)
                        {
                            editHandle.Enable();
                            foreach (var pt in points)
                                editHandle.DrawPoint(pt);
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
        /// Construye el CurveLoop del contorno exterior usando el convex hull de los puntos.
        /// Proyecta todo a Z=0 porque el contorno es plano — las cotas van en los puntos interiores.
        /// </summary>
        private CurveLoop BuildBoundaryCurveLoop(List<XYZ> points)
        {
            var hull = ConvexHull(points);
            var loop = new CurveLoop();

            for (int i = 0; i < hull.Count; i++)
            {
                var a = hull[i];
                var b = hull[(i + 1) % hull.Count];
                // Proyectar a Z del primer punto — el contorno debe ser plano
                var pa = new XYZ(a.X, a.Y, points[0].Z);
                var pb = new XYZ(b.X, b.Y, points[0].Z);
                loop.Append(Line.CreateBound(pa, pb));
            }

            return loop;
        }

        /// <summary>
        /// Convex hull 2D (Andrew's monotone chain).
        /// Devuelve los puntos del contorno exterior en orden antihorario.
        /// </summary>
        private List<XYZ> ConvexHull(List<XYZ> pts)
        {
            var sorted = pts.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
            var hull = new List<XYZ>();

            // Lower hull
            foreach (var p in sorted)
            {
                while (hull.Count >= 2 && Cross(hull[hull.Count - 2], hull[hull.Count - 1], p) <= 0)
                    hull.RemoveAt(hull.Count - 1);
                hull.Add(p);
            }

            // Upper hull
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
    }
}