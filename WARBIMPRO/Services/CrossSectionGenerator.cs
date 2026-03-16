//using Autodesk.Revit.DB;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace WARBIMPRO.Services
//{
//    /// <summary>
//    /// Genera vistas de sección transversal perpendiculares a un eje de vía.
//    /// Cada sección se nombra con notación de estación: K00+000.000
//    /// </summary>
//    public class CrossSectionGenerator
//    {
//        private readonly Document _doc;
//        private const double M2F = 3.28083989501; // metros a pies

//        public CrossSectionGenerator(Document doc) => _doc = doc;

//        public int Generate(
//            List<XYZ> axisPoints,
//            double spacingMeters,
//            double halfWidthMeters,
//            int viewScale)
//        {
//            double spacing  = spacingMeters  * M2F;
//            double halfW    = halfWidthMeters * M2F;
//            double depth    = 1.0 * M2F;   // profundidad de la vista
//            double height   = 15.0 * M2F;  // alto de exploración

//            var sectionTypeId = GetSectionTypeId();
//            var segments = BuildSegments(axisPoints);
//            double totalLen = segments.Sum(s => s.Len);

//            int count = 0;

//            using var trans = new Transaction(_doc, "WARBIMPRO: Secciones Transversales");
//            trans.Start();
//            try
//            {
//                for (double d = 0; d <= totalLen; d += spacing)
//                {
//                    GetStationPose(segments, d, out XYZ origin, out XYZ axisDir);

//                    XYZ right = new XYZ(-axisDir.Y, axisDir.X, 0).Normalize();
//                    XYZ up    = XYZ.BasisZ;

//                    var t = Transform.Identity;
//                    t.BasisX = right;
//                    t.BasisY = up;
//                    t.BasisZ = axisDir;
//                    t.Origin = origin;

//                    var bb = new BoundingBoxXYZ
//                    {
//                        Transform = t,
//                        Min = new XYZ(-halfW, -2.0 * M2F, -depth),
//                        Max = new XYZ( halfW,  height,     depth)
//                    };

//                    var view = ViewSection.CreateSection(_doc, sectionTypeId, bb);
//                    view.Scale = viewScale;
//                    view.Name  = StationName(d / M2F);
//                    count++;
//                }

//                trans.Commit();
//            }
//            catch { trans.RollBack(); throw; }

//            return count;
//        }

//        // ── Helpers ──────────────────────────────────────────────────────────

//        private record Seg(XYZ Start, XYZ Dir, double Len);

//        private List<Seg> BuildSegments(List<XYZ> pts)
//        {
//            var segs = new List<Seg>();
//            for (int i = 0; i < pts.Count - 1; i++)
//            {
//                double len = pts[i].DistanceTo(pts[i + 1]);
//                if (len > 0.001)
//                    segs.Add(new Seg(pts[i], (pts[i + 1] - pts[i]).Normalize(), len));
//            }
//            return segs;
//        }

//        private void GetStationPose(List<Seg> segs, double dist, out XYZ pt, out XYZ dir)
//        {
//            double acc = 0;
//            foreach (var s in segs)
//            {
//                if (acc + s.Len >= dist)
//                {
//                    pt  = s.Start + s.Dir * (dist - acc);
//                    dir = s.Dir;
//                    return;
//                }
//                acc += s.Len;
//            }
//            var last = segs.Last();
//            pt  = last.Start + last.Dir * last.Len;
//            dir = last.Dir;
//        }

//        private ElementId GetSectionTypeId() =>
//            new FilteredElementCollector(_doc)
//                .OfClass(typeof(ViewFamilyType))
//                .Cast<ViewFamilyType>()
//                .First(vft => vft.ViewFamily == ViewFamily.Section).Id;

//        private static string StationName(double meters)
//        {
//            int km = (int)(meters / 1000);
//            double m = meters % 1000;
//            return $"Sección K{km:D2}+{m:000.000}";
//        }
//    }
//}
