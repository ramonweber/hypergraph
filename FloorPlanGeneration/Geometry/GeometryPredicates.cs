using System;
using System.Collections.Generic;
using System.Linq;

namespace FloorPlanGeneration.Geometry
{
    public sealed class Interval1D
    {
        public Interval1D(double start, double end)
        {
            Start = Math.Min(start, end);
            End = Math.Max(start, end);
        }

        public double Start { get; set; }
        public double End { get; set; }

        public double Length
        {
            get { return Math.Max(0.0, End - Start); }
        }
    }

    public static class GeometryPredicates
    {
        public static double Cross(Point2 a, Point2 b, Point2 c)
        {
            return ((b.X - a.X) * (c.Y - a.Y)) - ((b.Y - a.Y) * (c.X - a.X));
        }

        public static bool OnSegment(Point2 a, Point2 b, Point2 p, double tolerance)
        {
            return Math.Abs(Cross(a, b, p)) <= tolerance &&
                   p.X >= Math.Min(a.X, b.X) - tolerance &&
                   p.X <= Math.Max(a.X, b.X) + tolerance &&
                   p.Y >= Math.Min(a.Y, b.Y) - tolerance &&
                   p.Y <= Math.Max(a.Y, b.Y) + tolerance;
        }

        public static bool SegmentsIntersect(LineSegment2 first, LineSegment2 second, double tolerance)
        {
            Point2 a = first.Start;
            Point2 b = first.End;
            Point2 c = second.Start;
            Point2 d = second.End;

            double c1 = Cross(a, b, c);
            double c2 = Cross(a, b, d);
            double c3 = Cross(c, d, a);
            double c4 = Cross(c, d, b);

            if (((c1 > tolerance && c2 < -tolerance) || (c1 < -tolerance && c2 > tolerance)) &&
                ((c3 > tolerance && c4 < -tolerance) || (c3 < -tolerance && c4 > tolerance)))
            {
                return true;
            }

            if (Math.Abs(c1) <= tolerance && OnSegment(a, b, c, tolerance)) return true;
            if (Math.Abs(c2) <= tolerance && OnSegment(a, b, d, tolerance)) return true;
            if (Math.Abs(c3) <= tolerance && OnSegment(c, d, a, tolerance)) return true;
            if (Math.Abs(c4) <= tolerance && OnSegment(c, d, b, tolerance)) return true;
            return false;
        }

        public static bool ProperSegmentsCross(LineSegment2 first, LineSegment2 second, double tolerance)
        {
            Point2 a = first.Start;
            Point2 b = first.End;
            Point2 c = second.Start;
            Point2 d = second.End;

            double c1 = Cross(a, b, c);
            double c2 = Cross(a, b, d);
            double c3 = Cross(c, d, a);
            double c4 = Cross(c, d, b);

            return ((c1 > tolerance && c2 < -tolerance) || (c1 < -tolerance && c2 > tolerance)) &&
                   ((c3 > tolerance && c4 < -tolerance) || (c3 < -tolerance && c4 > tolerance));
        }

        public static bool PolygonSelfIntersects(Polygon2 polygon, double tolerance)
        {
            List<LineSegment2> edges = polygon.Edges().ToList();
            for (int i = 0; i < edges.Count; i++)
            {
                for (int j = i + 1; j < edges.Count; j++)
                {
                    if (AreAdjacentEdges(i, j, edges.Count))
                    {
                        continue;
                    }

                    if (SegmentsIntersect(edges[i], edges[j], tolerance))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool ContainsPoint(Polygon2 polygon, Point2 point, double tolerance, bool includeBoundary)
        {
            foreach (LineSegment2 edge in polygon.Edges())
            {
                if (OnSegment(edge.Start, edge.End, point, tolerance))
                {
                    return includeBoundary;
                }
            }

            bool inside = false;
            int count = polygon.Vertices.Count;
            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                Point2 pi = polygon.Vertices[i];
                Point2 pj = polygon.Vertices[j];
                bool crosses = (pi.Y > point.Y) != (pj.Y > point.Y);
                if (!crosses)
                {
                    continue;
                }

                double xAtY = ((pj.X - pi.X) * (point.Y - pi.Y) / (pj.Y - pi.Y)) + pi.X;
                if (point.X < xAtY)
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        public static bool ContainsPolygon(Polygon2 container, Polygon2 candidate, double tolerance)
        {
            foreach (Point2 point in candidate.Vertices)
            {
                if (!ContainsPoint(container, point, tolerance, true))
                {
                    return false;
                }
            }

            foreach (LineSegment2 candidateEdge in candidate.Edges())
            {
                foreach (LineSegment2 containerEdge in container.Edges())
                {
                    if (ProperSegmentsCross(candidateEdge, containerEdge, tolerance))
                    {
                        return false;
                    }
                }

                Point2 midpoint = candidateEdge.Midpoint();
                if (!ContainsPoint(container, midpoint, tolerance, true))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool PolygonsOverlapArea(Polygon2 first, Polygon2 second, double tolerance)
        {
            if (!first.Bounds().Intersects(second.Bounds(), tolerance))
            {
                return false;
            }

            foreach (LineSegment2 firstEdge in first.Edges())
            {
                foreach (LineSegment2 secondEdge in second.Edges())
                {
                    if (ProperSegmentsCross(firstEdge, secondEdge, tolerance))
                    {
                        return true;
                    }
                }
            }

            foreach (Point2 point in first.Vertices)
            {
                if (ContainsPoint(second, point, tolerance, false))
                {
                    return true;
                }
            }

            foreach (Point2 point in second.Vertices)
            {
                if (ContainsPoint(first, point, tolerance, false))
                {
                    return true;
                }
            }

            if (ContainsPoint(first, second.Centroid(), tolerance, false) ||
                ContainsPoint(second, first.Centroid(), tolerance, false))
            {
                return true;
            }

            return false;
        }

        public static bool TouchesOrOverlaps(Polygon2 first, Polygon2 second, double tolerance)
        {
            if (!first.Bounds().Intersects(second.Bounds(), tolerance))
            {
                return false;
            }

            foreach (LineSegment2 firstEdge in first.Edges())
            {
                foreach (LineSegment2 secondEdge in second.Edges())
                {
                    if (SegmentsIntersect(firstEdge, secondEdge, tolerance))
                    {
                        return true;
                    }
                }
            }

            return ContainsPoint(first, second.Vertices[0], tolerance, true) ||
                   ContainsPoint(second, first.Vertices[0], tolerance, true);
        }

        public static double SharedBoundaryLength(Polygon2 first, Polygon2 second, double tolerance)
        {
            double length = 0.0;
            foreach (LineSegment2 a in first.Edges())
            {
                foreach (LineSegment2 b in second.Edges())
                {
                    length += SharedSegmentLength(a, b, tolerance);
                }
            }

            return length;
        }

        public static double SharedSegmentLength(LineSegment2 first, LineSegment2 second, double tolerance)
        {
            if (first.IsHorizontal(tolerance) && second.IsHorizontal(tolerance) &&
                Math.Abs(first.Start.Y - second.Start.Y) <= tolerance)
            {
                double start = Math.Max(Math.Min(first.Start.X, first.End.X), Math.Min(second.Start.X, second.End.X));
                double end = Math.Min(Math.Max(first.Start.X, first.End.X), Math.Max(second.Start.X, second.End.X));
                return Math.Max(0.0, end - start);
            }

            if (first.IsVertical(tolerance) && second.IsVertical(tolerance) &&
                Math.Abs(first.Start.X - second.Start.X) <= tolerance)
            {
                double start = Math.Max(Math.Min(first.Start.Y, first.End.Y), Math.Min(second.Start.Y, second.End.Y));
                double end = Math.Min(Math.Max(first.Start.Y, first.End.Y), Math.Max(second.Start.Y, second.End.Y));
                return Math.Max(0.0, end - start);
            }

            return 0.0;
        }

        public static List<Interval1D> HorizontalInsideIntervals(Polygon2 polygon, double y, double tolerance)
        {
            List<double> intersections = new List<double>();
            foreach (LineSegment2 edge in polygon.Edges())
            {
                Point2 a = edge.Start;
                Point2 b = edge.End;
                if (Math.Abs(a.Y - b.Y) <= tolerance)
                {
                    continue;
                }

                double minY = Math.Min(a.Y, b.Y);
                double maxY = Math.Max(a.Y, b.Y);
                if (y >= minY - tolerance && y < maxY - tolerance)
                {
                    double x = a.X + ((y - a.Y) * (b.X - a.X) / (b.Y - a.Y));
                    intersections.Add(x);
                }
            }

            intersections.Sort();
            List<Interval1D> intervals = new List<Interval1D>();
            for (int i = 0; i + 1 < intersections.Count; i += 2)
            {
                if (intersections[i + 1] - intersections[i] > tolerance)
                {
                    intervals.Add(new Interval1D(intersections[i], intersections[i + 1]));
                }
            }

            return intervals;
        }

        public static List<Interval1D> VerticalInsideIntervals(Polygon2 polygon, double x, double tolerance)
        {
            List<double> intersections = new List<double>();
            foreach (LineSegment2 edge in polygon.Edges())
            {
                Point2 a = edge.Start;
                Point2 b = edge.End;
                if (Math.Abs(a.X - b.X) <= tolerance)
                {
                    continue;
                }

                double minX = Math.Min(a.X, b.X);
                double maxX = Math.Max(a.X, b.X);
                if (x >= minX - tolerance && x < maxX - tolerance)
                {
                    double y = a.Y + ((x - a.X) * (b.Y - a.Y) / (b.X - a.X));
                    intersections.Add(y);
                }
            }

            intersections.Sort();
            List<Interval1D> intervals = new List<Interval1D>();
            for (int i = 0; i + 1 < intersections.Count; i += 2)
            {
                if (intersections[i + 1] - intersections[i] > tolerance)
                {
                    intervals.Add(new Interval1D(intersections[i], intersections[i + 1]));
                }
            }

            return intervals;
        }

        public static List<Interval1D> IntersectIntervals(IEnumerable<Interval1D> first, IEnumerable<Interval1D> second, double tolerance)
        {
            List<Interval1D> result = new List<Interval1D>();
            foreach (Interval1D a in first)
            {
                foreach (Interval1D b in second)
                {
                    double start = Math.Max(a.Start, b.Start);
                    double end = Math.Min(a.End, b.End);
                    if (end - start > tolerance)
                    {
                        result.Add(new Interval1D(start, end));
                    }
                }
            }

            return result.OrderBy(i => i.Start).ToList();
        }

        private static bool AreAdjacentEdges(int i, int j, int count)
        {
            return Math.Abs(i - j) == 1 || (i == 0 && j == count - 1);
        }
    }
}
