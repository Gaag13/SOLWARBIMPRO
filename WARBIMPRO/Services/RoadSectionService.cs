using Autodesk.Revit.DB;
using WARBIMPRO.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WARBIMPRO.Services
{
    /// <summary>
    /// Calcula los puntos de la vía a partir del eje y la sección tipo,
    /// luego crea la subdivisión dentro del Toposolid base.
    /// </summary>
    public class RoadSectionService
    {
        private readonly Document _doc;
        private const double M2F = 3.28083989501;

        public RoadSectionService(Document doc)
        {
            _doc = doc;
        }

        /// <summary>
        /// Genera la subdivisión de vía dentro del Toposolid base.
        /// </summary>
        /// <param name="hostToposolid">Toposolid del terreno existente</param>
        /// <param name="axisPoints">Puntos del eje en unidades internas de Revit (pies)</param>
        /// <param name="p">Parámetros de la sección tipo</param>
        public ElementId CreateRoadSubdivision(
            Toposolid hostToposolid,
            List<XYZ> axisPoints,
            RoadSectionParams p,
            out string message)
        {
            message = string.Empty;
            try
            {
                double halfWidth = (p.RoadWidthMeters / 2.0) * M2F;
                double slope = p.CrossSlopePercent / 100.0;
                double spacing = p.StationSpacingMeters * M2F;

                // 1. Construir segmentos del eje
                var segments = BuildSegments(axisPoints);
                double totalLen = segments.Sum(s => s.Length);

                // 2. Generar puntos de la vía en cada estación
                var leftPoints = new List<XYZ>();
                var rightPoints = new List<XYZ>();

                for (double d = 0; d <= totalLen + 0.001; d += spacing)
                {
                    double dist = Math.Min(d, totalLen);
                    GetPoseAtDistance(segments, dist, out XYZ origin, out XYZ axisDir);

                    // Perpendicular al eje en plano horizontal
                    XYZ perp = new XYZ(-axisDir.Y, axisDir.X, 0).Normalize();

                    // Cota del eje — proyectar sobre el terreno existente o usar Z del punto
                    double axisZ = origin.Z;

                    // Borde izquierdo: sube halfWidth * slope
                    double leftZ = axisZ + halfWidth * slope;
                    // Borde derecho: sube halfWidth * slope (vía a dos aguas desde el centro)
                    double rightZ = axisZ + halfWidth * slope;

                    XYZ leftPt = new XYZ(
                        origin.X + perp.X * halfWidth,
                        origin.Y + perp.Y * halfWidth,
                        leftZ);

                    XYZ rightPt = new XYZ(
                        origin.X - perp.X * halfWidth,
                        origin.Y - perp.Y * halfWidth,
                        rightZ);

                    leftPoints.Add(leftPt);
                    rightPoints.Add(rightPt);
                }

                // 3. Construir la lista de puntos de la vía:
                //    borde izquierdo de inicio a fin + borde derecho de fin a inicio
                var roadPoints = new List<XYZ>();
                roadPoints.AddRange(leftPoints);
                rightPoints.Reverse();
                roadPoints.AddRange(rightPoints);

                // 4. Construir el CurveLoop del contorno
                var loop = BuildCurveLoop(roadPoints);

                // 5. Crear la subdivisión
                using (var trans = new Transaction(_doc, "WARBIMPRO: Crear Vía"))
                {
                    trans.Start();
                    try
                    {
                        var subdivision = hostToposolid.CreateSubDivision(
                                 _doc,
                                 new List<CurveLoop> { loop });

                        // Aplicar cotas a los puntos del eje y bordes
                        var editor = subdivision.GetSlabShapeEditor();
                        if (editor != null)
                        {
                            editor.Enable();

                            // Puntos del eje con su Z
                            foreach (var pt in axisPoints)
                                editor.DrawPoint(pt);

                            // Puntos de los bordes
                            foreach (var pt in leftPoints)
                                editor.DrawPoint(pt);
                            foreach (var pt in rightPoints)
                                editor.DrawPoint(pt);
                        }

                        trans.Commit();
                        message = $"Vía creada: {leftPoints.Count} estaciones, ancho {p.RoadWidthMeters}m, pendiente {p.CrossSlopePercent}%.";
                        return subdivision.Id;
                    }
                    catch { trans.RollBack(); throw; }
                }
            }
            catch (Exception ex)
            {
                message = $"Error: {ex.Message}";
                return ElementId.InvalidElementId;
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private class Segment
        {
            public XYZ Start { get; }
            public XYZ Dir { get; }
            public double Length { get; }

            public Segment(XYZ start, XYZ dir, double length)
            {
                Start = start;
                Dir = dir;
                Length = length;
            }
        }

        private List<Segment> BuildSegments(List<XYZ> pts)
        {
            var segs = new List<Segment>();
            for (int i = 0; i < pts.Count - 1; i++)
            {
                double len = pts[i].DistanceTo(pts[i + 1]);
                if (len > 0.001)
                    segs.Add(new Segment(pts[i], (pts[i + 1] - pts[i]).Normalize(), len));
            }
            return segs;
        }

        private void GetPoseAtDistance(List<Segment> segs, double dist, out XYZ pt, out XYZ dir)
        {
            double acc = 0;
            foreach (var s in segs)
            {
                if (acc + s.Length >= dist)
                {
                    pt = s.Start + s.Dir * (dist - acc);
                    dir = s.Dir;
                    return;
                }
                acc += s.Length;
            }
            var last = segs.Last();
            pt = last.Start + last.Dir * last.Length;
            dir = last.Dir;
        }

        private CurveLoop BuildCurveLoop(List<XYZ> pts)
        {
            // Proyectar a Z base para el contorno plano
            double baseZ = pts[0].Z;
            var loop = new CurveLoop();

            for (int i = 0; i < pts.Count; i++)
            {
                var a = new XYZ(pts[i].X, pts[i].Y, baseZ);
                var b = new XYZ(pts[(i + 1) % pts.Count].X, pts[(i + 1) % pts.Count].Y, baseZ);
                if (a.DistanceTo(b) > 0.01)
                    loop.Append(Line.CreateBound(a, b));
            }
            return loop;
        }
    }
}