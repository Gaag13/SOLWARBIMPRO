using Autodesk.Revit.DB;
using WARBIMPRO.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WARBIMPRO.Services
{
    public class RoadSectionService
    {
        private readonly Document _doc;
        private const double M2F = 3.28083989501;

        public RoadSectionService(Document doc)
        {
            _doc = doc;
        }

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
                double crossSlope = p.CrossSlopePercent / 100.0;
                double longSlope = p.LongSlopePercent / 100.0;
                double spacing = p.StationSpacingMeters * M2F;
                double startZ = p.StartElevationMeters * M2F;

                var segments = BuildSegments(axisPoints);
                double totalLen = segments.Sum(s => s.Length);

                var stations = new List<StationData>();
                for (double d = 0; d <= totalLen + 0.001; d += spacing)
                {
                    double dist = Math.Min(d, totalLen);
                    GetPoseAtDistance(segments, dist, out XYZ origin, out XYZ axisDir);

                    double axisZ = startZ - (dist * longSlope);
                    double borderZ = axisZ - halfWidth * crossSlope;
                    XYZ perp = new XYZ(-axisDir.Y, axisDir.X, 0).Normalize();

                    stations.Add(new StationData
                    {
                        AxisPt = new XYZ(origin.X, origin.Y, axisZ),
                        LeftPt = new XYZ(origin.X + perp.X * halfWidth,
                                          origin.Y + perp.Y * halfWidth, borderZ),
                        RightPt = new XYZ(origin.X - perp.X * halfWidth,
                                          origin.Y - perp.Y * halfWidth, borderZ)
                    });
                }

                // Contorno para subdivisión
                var roadBorder = new List<XYZ>();
                roadBorder.AddRange(stations.Select(s => s.LeftPt));
                roadBorder.AddRange(stations.Select(s => s.RightPt).Reverse());
                var loop = BuildCurveLoop(roadBorder, startZ);

                using (var trans = new Transaction(_doc, "WARBIMPRO: Crear Vía"))
                {
                    trans.Start();
                    try
                    {
                        var editor = hostToposolid.GetSlabShapeEditor();
                        if (editor == null)
                            throw new InvalidOperationException("No se pudo obtener el editor del Toposolid.");

                        editor.Enable();

                        // Primero crear todos los vértices con DrawPoint
                        // DrawPoint devuelve SlabShapeVertex que se usa en DrawSplitLine
                        var leftVerts = stations.Select(s => editor.DrawPoint(s.LeftPt)).ToList();
                        var rightVerts = stations.Select(s => editor.DrawPoint(s.RightPt)).ToList();
                        var axisVerts = stations.Select(s => editor.DrawPoint(s.AxisPt)).ToList();

                        // Split lines LONGITUDINALES — borde izquierdo
                        for (int i = 0; i < stations.Count - 1; i++)
                            editor.DrawSplitLine(leftVerts[i], leftVerts[i + 1]);

                        // Split lines LONGITUDINALES — borde derecho
                        for (int i = 0; i < stations.Count - 1; i++)
                            editor.DrawSplitLine(rightVerts[i], rightVerts[i + 1]);

                        // Split lines LONGITUDINALES — eje central
                        for (int i = 0; i < stations.Count - 1; i++)
                            editor.DrawSplitLine(axisVerts[i], axisVerts[i + 1]);

                        // Split lines TRANSVERSALES — una por estación
                        for (int i = 0; i < stations.Count; i++)
                            editor.DrawSplitLine(leftVerts[i], rightVerts[i]);

                        // Subdivisión encima para material
                        var subdivision = hostToposolid.CreateSubDivision(
                            _doc,
                            new List<CurveLoop> { loop });

                        trans.Commit();
                        message = $"Vía creada: {stations.Count} estaciones, " +
                                  $"ancho {p.RoadWidthMeters}m, " +
                                  $"pendiente {p.LongSlopePercent}%, " +
                                  $"bombeo {p.CrossSlopePercent}%.";
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

        private class StationData
        {
            public XYZ AxisPt { get; set; }
            public XYZ LeftPt { get; set; }
            public XYZ RightPt { get; set; }
        }

        private class Segment
        {
            public XYZ Start { get; }
            public XYZ Dir { get; }
            public double Length { get; }
            public Segment(XYZ start, XYZ dir, double length)
            { Start = start; Dir = dir; Length = length; }
        }

        private List<Segment> BuildSegments(List<XYZ> pts)
        {
            var segs = new List<Segment>();
            for (int i = 0; i < pts.Count - 1; i++)
            {
                double len = pts[i].DistanceTo(pts[i + 1]);
                if (len > 0.001)
                    segs.Add(new Segment(
                        pts[i],
                        (pts[i + 1] - pts[i]).Normalize(),
                        len));
            }
            return segs;
        }

        private void GetPoseAtDistance(List<Segment> segs, double dist,
            out XYZ pt, out XYZ dir)
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

        private CurveLoop BuildCurveLoop(List<XYZ> pts, double baseZ)
        {
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