using System;
using System.Collections.Generic;
using System.Linq;

namespace WARBIMPRO.Models
{
    public class TinPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public int Index { get; set; }

        public TinPoint(double x, double y, double z, int index = -1)
        {
            X = x; Y = y; Z = z; Index = index;
        }
    }

    public class TinTriangle
    {
        public int A { get; }
        public int B { get; }
        public int C { get; }
        public TinTriangle(int a, int b, int c) { A = a; B = b; C = c; }

        public IEnumerable<(int, int)> Edges()
        {
            yield return (A, B);
            yield return (B, C);
            yield return (C, A);
        }
    }

    public static class DelaunayTriangulator
    {
        private const double Eps = 1e-10;

        public static List<TinTriangle> Triangulate(List<TinPoint> points)
        {
            if (points == null || points.Count < 3)
                throw new ArgumentException("Se necesitan mínimo 3 puntos.");

            int n = points.Count;

            double minX = points.Min(p => p.X), maxX = points.Max(p => p.X);
            double minY = points.Min(p => p.Y), maxY = points.Max(p => p.Y);
            double cx = (minX + maxX) / 2.0;
            double cy = (minY + maxY) / 2.0;
            double r = Math.Max(maxX - minX, maxY - minY) + 1.0;

            // Puntos originales + super-triángulo al final
            var all = new List<TinPoint>(n + 3);
            for (int i = 0; i < n; i++)
                all.Add(new TinPoint(points[i].X, points[i].Y, points[i].Z, i));

            all.Add(new TinPoint(cx, cy + 3 * r, 0, n));
            all.Add(new TinPoint(cx - 3 * r, cy - r, 0, n + 1));
            all.Add(new TinPoint(cx + 3 * r, cy - r, 0, n + 2));

            var triangles = new List<TinTriangle> { new TinTriangle(n, n + 1, n + 2) };

            for (int pi = 0; pi < n; pi++)
            {
                var p = all[pi];

                var bad = triangles
                    .Where(t => InCircumcircle(p, all[t.A], all[t.B], all[t.C]))
                    .ToList();

                var boundary = GetBoundaryEdges(bad);

                foreach (var t in bad) triangles.Remove(t);
                foreach (var (eA, eB) in boundary)
                    triangles.Add(new TinTriangle(pi, eA, eB));
            }

            triangles.RemoveAll(t => t.A >= n || t.B >= n || t.C >= n);

            for (int i = 0; i < n; i++)
                points[i].Index = i;

            return triangles;
        }

        /// <summary>
        /// Calcula el centro del circuncírculo y verifica si P está dentro.
        /// Más robusto que el determinante para puntos cocíclicos.
        /// </summary>
        private static bool InCircumcircle(TinPoint p, TinPoint a, TinPoint b, TinPoint c)
        {
            double ax = a.X, ay = a.Y;
            double bx = b.X, by = b.Y;
            double cx = c.X, cy = c.Y;

            double D = 2.0 * (ax * (by - cy) + bx * (cy - ay) + cx * (ay - by));
            if (Math.Abs(D) < 1e-15) return false;

            double ax2ay2 = ax * ax + ay * ay;
            double bx2by2 = bx * bx + by * by;
            double cx2cy2 = cx * cx + cy * cy;

            double ux = (ax2ay2 * (by - cy) + bx2by2 * (cy - ay) + cx2cy2 * (ay - by)) / D;
            double uy = (ax2ay2 * (cx - bx) + bx2by2 * (ax - cx) + cx2cy2 * (bx - ax)) / D;

            double r2 = (ax - ux) * (ax - ux) + (ay - uy) * (ay - uy);
            double dist2 = (p.X - ux) * (p.X - ux) + (p.Y - uy) * (p.Y - uy);

            return dist2 < r2 + Eps;
        }

        private static List<(int, int)> GetBoundaryEdges(List<TinTriangle> bad)
        {
            var count = new Dictionary<(int, int), int>();
            foreach (var t in bad)
                foreach (var (a, b) in t.Edges())
                {
                    var key = a < b ? (a, b) : (b, a);
                    count.TryGetValue(key, out int cnt);
                    count[key] = cnt + 1;
                }
            return count.Where(kv => kv.Value == 1).Select(kv => kv.Key).ToList();
        }
    }
}