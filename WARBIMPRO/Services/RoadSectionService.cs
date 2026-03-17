using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using WARBIMPRO.Models;

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

        public Result CreateRoadSubdivision(
            Toposolid hostToposolid,
            List<XYZ> axisPoints,
            RoadSectionParams p,
            out string message)
        {
            message = string.Empty;
            try
            {
                double halfRoad = (p.RoadWidthMeters / 2.0) * M2F;
                double leftSW = p.HasLeftSidewalk ? p.LeftSidewalkWidthMeters * M2F : 0;
                double rightSW = p.HasRightSidewalk ? p.RightSidewalkWidthMeters * M2F : 0;
                double crossSlope = p.CrossSlopePercent / 100.0;
                double swSlope = p.SidewalkSlopePercent / 100.0;
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
                    double roadEdgeZ = axisZ - halfRoad * crossSlope;
                    double leftSwEdgeZ = roadEdgeZ - leftSW * swSlope;
                    double rightSwEdgeZ = roadEdgeZ - rightSW * swSlope;

                    XYZ perp = new XYZ(-axisDir.Y, axisDir.X, 0).Normalize();

                    double outerOffset = halfRoad + Math.Max(leftSW, rightSW) + 0.5 * M2F;
                    double terrainZL = GetTerrainElevation(hostToposolid,
                        origin.X + perp.X * outerOffset,
                        origin.Y + perp.Y * outerOffset, startZ);
                    double terrainZR = GetTerrainElevation(hostToposolid,
                        origin.X - perp.X * outerOffset,
                        origin.Y - perp.Y * outerOffset, startZ);

                    var st = new StationData
                    {
                        AxisPt = new XYZ(origin.X, origin.Y, axisZ),
                        LeftRoadEdge = new XYZ(origin.X + perp.X * halfRoad,
                                                origin.Y + perp.Y * halfRoad, roadEdgeZ),
                        RightRoadEdge = new XYZ(origin.X - perp.X * halfRoad,
                                                origin.Y - perp.Y * halfRoad, roadEdgeZ),
                        LeftOutPt = new XYZ(origin.X + perp.X * outerOffset,
                                                origin.Y + perp.Y * outerOffset, terrainZL),
                        RightOutPt = new XYZ(origin.X - perp.X * outerOffset,
                                                origin.Y - perp.Y * outerOffset, terrainZR)
                    };

                    if (p.HasLeftSidewalk)
                        st.LeftSwEdge = new XYZ(
                            origin.X + perp.X * (halfRoad + leftSW),
                            origin.Y + perp.Y * (halfRoad + leftSW),
                            leftSwEdgeZ);

                    if (p.HasRightSidewalk)
                        st.RightSwEdge = new XYZ(
                            origin.X - perp.X * (halfRoad + rightSW),
                            origin.Y - perp.Y * (halfRoad + rightSW),
                            rightSwEdgeZ);

                    stations.Add(st);
                }

                using (var trans = new Transaction(_doc, "WARBIMPRO: Crear Vía y Andenes"))
                {
                    trans.Start();
                    try
                    {
                        var editor = hostToposolid.GetSlabShapeEditor();
                        editor.Enable();

                        // Crear todos los vértices sobre el terreno base
                        var axisVerts = stations.Select(s => editor.DrawPoint(s.AxisPt)).ToList();
                        var leftRoadVerts = stations.Select(s => editor.DrawPoint(s.LeftRoadEdge)).ToList();
                        var rightRoadVerts = stations.Select(s => editor.DrawPoint(s.RightRoadEdge)).ToList();
                        var leftOutVerts = stations.Select(s => editor.DrawPoint(s.LeftOutPt)).ToList();
                        var rightOutVerts = stations.Select(s => editor.DrawPoint(s.RightOutPt)).ToList();

                        List<SlabShapeVertex> leftSwVerts = null;
                        List<SlabShapeVertex> rightSwVerts = null;

                        if (p.HasLeftSidewalk && stations.All(s => s.LeftSwEdge != null))
                            leftSwVerts = stations.Select(s => editor.DrawPoint(s.LeftSwEdge)).ToList();
                        if (p.HasRightSidewalk && stations.All(s => s.RightSwEdge != null))
                            rightSwVerts = stations.Select(s => editor.DrawPoint(s.RightSwEdge)).ToList();

                        // Split lines longitudinales
                        for (int i = 0; i < stations.Count - 1; i++)
                        {
                            editor.DrawSplitLine(axisVerts[i], axisVerts[i + 1]);
                            editor.DrawSplitLine(leftRoadVerts[i], leftRoadVerts[i + 1]);
                            editor.DrawSplitLine(rightRoadVerts[i], rightRoadVerts[i + 1]);
                            editor.DrawSplitLine(leftOutVerts[i], leftOutVerts[i + 1]);
                            editor.DrawSplitLine(rightOutVerts[i], rightOutVerts[i + 1]);
                            if (leftSwVerts != null) editor.DrawSplitLine(leftSwVerts[i], leftSwVerts[i + 1]);
                            if (rightSwVerts != null) editor.DrawSplitLine(rightSwVerts[i], rightSwVerts[i + 1]);
                        }

                        // Split lines transversales
                        for (int i = 0; i < stations.Count; i++)
                        {
                            editor.DrawSplitLine(leftRoadVerts[i], axisVerts[i]);
                            editor.DrawSplitLine(axisVerts[i], rightRoadVerts[i]);

                            if (leftSwVerts != null)
                            {
                                editor.DrawSplitLine(leftRoadVerts[i], leftSwVerts[i]);
                                editor.DrawSplitLine(leftSwVerts[i], leftOutVerts[i]);
                            }
                            else
                                editor.DrawSplitLine(leftRoadVerts[i], leftOutVerts[i]);

                            if (rightSwVerts != null)
                            {
                                editor.DrawSplitLine(rightRoadVerts[i], rightSwVerts[i]);
                                editor.DrawSplitLine(rightSwVerts[i], rightOutVerts[i]);
                            }
                            else
                                editor.DrawSplitLine(rightRoadVerts[i], rightOutVerts[i]);
                        }

                        // ── Subdivisión 1: VÍA ──────────────────────────────
                        var roadBorder = new List<XYZ>();
                        roadBorder.AddRange(stations.Select(s => s.LeftRoadEdge));
                        roadBorder.AddRange(stations.Select(s => s.RightRoadEdge).Reverse());
                        var roadLoop = BuildCurveLoop(roadBorder, startZ);
                        hostToposolid.CreateSubDivision(_doc, new List<CurveLoop> { roadLoop });

                        // ── Subdivisión 2: ANDÉN IZQUIERDO ──────────────────
                        if (p.HasLeftSidewalk && leftSwVerts != null)
                        {
                            var leftBorder = new List<XYZ>();
                            leftBorder.AddRange(stations.Select(s => s.LeftSwEdge));
                            leftBorder.AddRange(stations.Select(s => s.LeftRoadEdge).Reverse());
                            var leftLoop = BuildCurveLoop(leftBorder, startZ);
                            hostToposolid.CreateSubDivision(_doc, new List<CurveLoop> { leftLoop });
                        }

                        // ── Subdivisión 3: ANDÉN DERECHO ────────────────────
                        if (p.HasRightSidewalk && rightSwVerts != null)
                        {
                            var rightBorder = new List<XYZ>();
                            rightBorder.AddRange(stations.Select(s => s.RightRoadEdge));
                            rightBorder.AddRange(stations.Select(s => s.RightSwEdge).Reverse());
                            var rightLoop = BuildCurveLoop(rightBorder, startZ);
                            hostToposolid.CreateSubDivision(_doc, new List<CurveLoop> { rightLoop });
                        }

                        trans.Commit();
                        message = $"Creado: vía {p.RoadWidthMeters}m" +
                                  (p.HasLeftSidewalk ? $" + andén izq {p.LeftSidewalkWidthMeters}m" : "") +
                                  (p.HasRightSidewalk ? $" + andén der {p.RightSidewalkWidthMeters}m" : "") +
                                  $" — {stations.Count} estaciones.";
                        return Result.Succeeded;
                    }
                    catch { trans.RollBack(); throw; }
                }
            }
            catch (Exception ex)
            {
                message = $"Error: {ex.Message}";
                return Result.Failed;
            }
        }

        private double GetTerrainElevation(Toposolid topo, double x, double y, double defaultZ)
        {
            try
            {
                var editor = topo.GetSlabShapeEditor();
                if (editor == null) return defaultZ;
                double wSum = 0, zSum = 0;
                foreach (SlabShapeVertex v in editor.SlabShapeVertices)
                {
                    double dx = v.Position.X - x, dy = v.Position.Y - y;
                    double dist = Math.Sqrt(dx * dx + dy * dy);
                    if (dist < 0.001) return v.Position.Z;
                    double w = 1.0 / (dist * dist);
                    wSum += w; zSum += w * v.Position.Z;
                }
                return wSum > 0 ? zSum / wSum : defaultZ;
            }
            catch { return defaultZ; }
        }

        private class StationData
        {
            public XYZ AxisPt { get; set; }
            public XYZ LeftRoadEdge { get; set; }
            public XYZ RightRoadEdge { get; set; }
            public XYZ LeftSwEdge { get; set; }
            public XYZ RightSwEdge { get; set; }
            public XYZ LeftOutPt { get; set; }
            public XYZ RightOutPt { get; set; }
        }

        private class Segment
        {
            public XYZ Start { get; }
            public XYZ Dir { get; }
            public double Length { get; }
            public Segment(XYZ s, XYZ d, double l) { Start = s; Dir = d; Length = l; }
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
                { pt = s.Start + s.Dir * (dist - acc); dir = s.Dir; return; }
                acc += s.Length;
            }
            var last = segs.Last();
            pt = last.Start + last.Dir * last.Length;
            dir = last.Dir;
        }

        /// <summary>
        /// Construye un CurveLoop válido usando el Convex Hull de los puntos.
        /// Esto garantiza que el contorno sea convexo y no tenga segmentos cruzados
        /// independientemente de la orientación del eje (recto, diagonal, curvo).
        /// </summary>
        private CurveLoop BuildCurveLoop(List<XYZ> pts, double baseZ)
        {
            var hull = ConvexHull(pts);
            var loop = new CurveLoop();

            for (int i = 0; i < hull.Count; i++)
            {
                var a = new XYZ(hull[i].X, hull[i].Y, baseZ);
                var b = new XYZ(hull[(i + 1) % hull.Count].X, hull[(i + 1) % hull.Count].Y, baseZ);
                if (a.DistanceTo(b) > 0.01)
                    loop.Append(Line.CreateBound(a, b));
            }
            return loop;
        }

        /// <summary>
        /// Convex Hull 2D — Andrew's monotone chain.
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
    }
}