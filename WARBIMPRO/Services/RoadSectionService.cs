using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using WARBIMPRO.Models;

#if REVIT2024_OR_GREATER

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



        // onProgress es opcional — si no se pasa, funciona igual que antes
        public Result CreateRoadSubdivision(
            Toposolid hostToposolid,
            List<XYZ> axisPoints,
            RoadSectionParams p,
            out string message,
            Action<int, string> onProgress = null)
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

                // ── Fase 1: Calcular estaciones (0 → 70%) ───────────────────
                onProgress?.Invoke(0, "Calculando estaciones...");

                double totalStations = Math.Ceiling(totalLen / spacing) + 1;
                int stationIdx = 0;

                for (double d = 0; d <= totalLen + 0.001; d += spacing)
                {
                    double dist = Math.Min(d, totalLen);
                    GetPoseAtDistance(segments, dist, out XYZ origin, out XYZ axisDir);

                    double axisZ = startZ - dist * longSlope;
                    double roadEdgeZ = axisZ - halfRoad * crossSlope;
                    double leftSwEdgeZ = roadEdgeZ - leftSW * swSlope;
                    double rightSwEdgeZ = roadEdgeZ - rightSW * swSlope;

                    XYZ perp = new XYZ(-axisDir.Y, axisDir.X, 0).Normalize();

                    double outerOffset = halfRoad + Math.Max(leftSW, rightSW) + 0.5 * M2F;
                    double terrainZL = GetTerrainElevation(hostToposolid,
                        origin.X + perp.X * outerOffset, origin.Y + perp.Y * outerOffset, startZ);
                    double terrainZR = GetTerrainElevation(hostToposolid,
                        origin.X - perp.X * outerOffset, origin.Y - perp.Y * outerOffset, startZ);

                    var st = new StationData
                    {
                        AxisPt = new XYZ(origin.X, origin.Y, axisZ),
                        LeftRoadEdge = new XYZ(origin.X + perp.X * halfRoad, origin.Y + perp.Y * halfRoad, roadEdgeZ),
                        RightRoadEdge = new XYZ(origin.X - perp.X * halfRoad, origin.Y - perp.Y * halfRoad, roadEdgeZ),
                        LeftOutPt = new XYZ(origin.X + perp.X * outerOffset, origin.Y + perp.Y * outerOffset, terrainZL),
                        RightOutPt = new XYZ(origin.X - perp.X * outerOffset, origin.Y - perp.Y * outerOffset, terrainZR),
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

                    // Reportar progreso 0-70%
                    stationIdx++;
                    int pct = (int)(stationIdx / totalStations * 70);
                    onProgress?.Invoke(pct, $"Estación {stationIdx} de {(int)totalStations}...");
                }

                using (var trans = new Transaction(_doc, "WARBIMPRO: Crear Vía y Andenes"))
                {
                    trans.Start();
                    try
                    {
                        // ── Fase 2: Vértices (70 → 78%) ─────────────────────
                        onProgress?.Invoke(70, "Insertando vértices...");

                        var editor = hostToposolid.GetSlabShapeEditor();
                        editor.Enable();

#if !REVIT2025_OR_GREATER

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

                        // ── Fase 3: Split Lines (78 → 88%) ──────────────────
                        onProgress?.Invoke(78, "Dibujando Split Lines longitudinales...");

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

                        onProgress?.Invoke(84, "Dibujando Split Lines transversales...");

                        for (int i = 0; i < stations.Count; i++)
                        {
                            editor.DrawSplitLine(leftRoadVerts[i], axisVerts[i]);
                            editor.DrawSplitLine(axisVerts[i], rightRoadVerts[i]);

                            if (leftSwVerts != null)
                            {
                                editor.DrawSplitLine(leftRoadVerts[i], leftSwVerts[i]);
                                editor.DrawSplitLine(leftSwVerts[i], leftOutVerts[i]);
                            }
                            else editor.DrawSplitLine(leftRoadVerts[i], leftOutVerts[i]);

                            if (rightSwVerts != null)
                            {
                                editor.DrawSplitLine(rightRoadVerts[i], rightSwVerts[i]);
                                editor.DrawSplitLine(rightSwVerts[i], rightOutVerts[i]);
                            }
                            else editor.DrawSplitLine(rightRoadVerts[i], rightOutVerts[i]);
                        }
#else

                        var axisVerts = stations.Select(s => editor.AddPoint(s.AxisPt)).ToList();
                        var leftRoadVerts = stations.Select(s => editor.AddPoint(s.LeftRoadEdge)).ToList();
                        var rightRoadVerts = stations.Select(s => editor.AddPoint(s.RightRoadEdge)).ToList();
                        var leftOutVerts = stations.Select(s => editor.AddPoint(s.LeftOutPt)).ToList();
                        var rightOutVerts = stations.Select(s => editor.AddPoint(s.RightOutPt)).ToList();

                        List<SlabShapeVertex> leftSwVerts = null;
                        List<SlabShapeVertex> rightSwVerts = null;

                        if (p.HasLeftSidewalk && stations.All(s => s.LeftSwEdge != null))
                            leftSwVerts = stations.Select(s => editor.AddPoint(s.LeftSwEdge)).ToList();
                        if (p.HasRightSidewalk && stations.All(s => s.RightSwEdge != null))
                            rightSwVerts = stations.Select(s => editor.AddPoint(s.RightSwEdge)).ToList();

                        // ── Fase 3: Split Lines (78 → 88%) ──────────────────
                        onProgress?.Invoke(78, "Dibujando Split Lines longitudinales...");

                        for (int i = 0; i < stations.Count - 1; i++)
                        {
                            editor.AddSplitLine(axisVerts[i], axisVerts[i + 1]);
                            editor.AddSplitLine(leftRoadVerts[i], leftRoadVerts[i + 1]);
                            editor.AddSplitLine(rightRoadVerts[i], rightRoadVerts[i + 1]);
                            editor.AddSplitLine(leftOutVerts[i], leftOutVerts[i + 1]);
                            editor.AddSplitLine(rightOutVerts[i], rightOutVerts[i + 1]);
                            if (leftSwVerts != null) editor.AddSplitLine(leftSwVerts[i], leftSwVerts[i + 1]);
                            if (rightSwVerts != null) editor.AddSplitLine(rightSwVerts[i], rightSwVerts[i + 1]);
                        }

                        onProgress?.Invoke(84, "Dibujando Split Lines transversales...");

                        for (int i = 0; i < stations.Count; i++)
                        {
                            editor.AddSplitLine(leftRoadVerts[i], axisVerts[i]);
                            editor.AddSplitLine(axisVerts[i], rightRoadVerts[i]);

                            if (leftSwVerts != null)
                            {
                                editor.AddSplitLine(leftRoadVerts[i], leftSwVerts[i]);
                                editor.AddSplitLine(leftSwVerts[i], leftOutVerts[i]);
                            }
                            else editor.AddSplitLine(leftRoadVerts[i], leftOutVerts[i]);

                            if (rightSwVerts != null)
                            {
                                editor.AddSplitLine(rightRoadVerts[i], rightSwVerts[i]);
                                editor.AddSplitLine(rightSwVerts[i], rightOutVerts[i]);
                            }
                            else editor.AddSplitLine(rightRoadVerts[i], rightOutVerts[i]);
                        }

#endif


                        // ── Fase 4: Subdivisiones (88 → 100%) ───────────────
                        onProgress?.Invoke(88, "Creando subdivisión de vía...");

                        var roadLoop = BuildRealBorderLoop(
                            stations.Select(s => s.LeftRoadEdge).ToList(),
                            stations.Select(s => s.RightRoadEdge).ToList());
                        if (roadLoop != null)
                        {
                            var roadSubDiv = hostToposolid.CreateSubDivision(_doc, new List<CurveLoop> { roadLoop });
                            ApplyMaterial(roadSubDiv, p.RoadMaterialId);
                        }

                        if (p.HasLeftSidewalk && leftSwVerts != null)
                        {
                            onProgress?.Invoke(93, "Creando andén izquierdo...");
                            var leftLoop = BuildRealBorderLoop(
                                stations.Select(s => s.LeftSwEdge).ToList(),
                                stations.Select(s => s.LeftRoadEdge).ToList());
                            if (leftLoop != null)
                            {
                                var leftSubDiv = hostToposolid.CreateSubDivision(_doc, new List<CurveLoop> { leftLoop });
                                ApplyMaterial(leftSubDiv, p.LeftMaterialId);
                            }
                        }

                        if (p.HasRightSidewalk && rightSwVerts != null)
                        {
                            onProgress?.Invoke(97, "Creando andén derecho...");
                            var rightLoop = BuildRealBorderLoop(
                                stations.Select(s => s.RightRoadEdge).ToList(),
                                stations.Select(s => s.RightSwEdge).ToList());
                            if (rightLoop != null)
                            {
                                var rightSubDiv = hostToposolid.CreateSubDivision(_doc, new List<CurveLoop> { rightLoop });
                                ApplyMaterial(rightSubDiv, p.RightMaterialId);
                            }
                        }

                        trans.Commit();
                        onProgress?.Invoke(100, "¡Completado!");

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



                        // ── BuildRealBorderLoop ──────────────────────────────────────────────

        private CurveLoop BuildRealBorderLoop(List<XYZ> leftSide, List<XYZ> rightSide)
        {
            if (leftSide == null || rightSide == null) return null;
            if (leftSide.Count < 2 || rightSide.Count < 2) return null;

            const double minLen = 0.01;

            var left = leftSide.Select(p => new XYZ(p.X, p.Y, 0.0)).ToList();
            var right = rightSide.Select(p => new XYZ(p.X, p.Y, 0.0)).ToList();

            var border = new List<XYZ>();
            border.AddRange(left);
            border.AddRange(Enumerable.Reverse(right));

            var clean = new List<XYZ> { border[0] };
            for (int i = 1; i < border.Count; i++)
                if (border[i].DistanceTo(clean.Last()) > minLen)
                    clean.Add(border[i]);

            while (clean.Count > 2 && clean.Last().DistanceTo(clean[0]) < minLen)
                clean.RemoveAt(clean.Count - 1);

            if (clean.Count < 3) return null;

            var curves = new List<Curve>();
            for (int i = 0; i < clean.Count; i++)
            {
                XYZ a = clean[i];
                XYZ b = clean[(i + 1) % clean.Count];
                if (a.DistanceTo(b) > minLen)
                    curves.Add(Line.CreateBound(a, b));
            }

            if (curves.Count < 3) return null;

            for (int i = 0; i < curves.Count; i++)
            {
                double gap = curves[i].GetEndPoint(1)
                                      .DistanceTo(curves[(i + 1) % curves.Count].GetEndPoint(0));
                if (gap > 0.001) return null;
            }

            var loop = new CurveLoop();
            foreach (var c in curves)
                loop.Append(c);

            return loop;
        }

        // ── ApplyMaterial ────────────────────────────────────────────────────

        private void ApplyMaterial(Element subdivision, ElementId materialId)
        {
            if (subdivision == null) return;
            if (materialId == null || materialId == ElementId.InvalidElementId) return;

            try
            {
                var matParam = subdivision.get_Parameter(BuiltInParameter.MATERIAL_ID_PARAM);
                if (matParam != null && !matParam.IsReadOnly)
                {
                    matParam.Set(materialId);
                    return;
                }

                foreach (Parameter param in subdivision.Parameters)
                {
                    if (param.StorageType == StorageType.ElementId
                        && !param.IsReadOnly
                        && param.Definition.Name.ToLower().Contains("material"))
                    {
                        param.Set(materialId);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ApplyMaterial: {ex.Message}");
            }
        }

        // ── Helpers sin cambios ──────────────────────────────────────────────

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
    }
}
#endif