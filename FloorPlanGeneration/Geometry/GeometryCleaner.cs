using System;
using System.Collections.Generic;
using System.Linq;
using FloorPlanGeneration.Schema;

namespace FloorPlanGeneration.Geometry
{
    public sealed class CleanPolygonResult
    {
        public CleanPolygonResult()
        {
            Polygon = new Polygon2();
            Diagnostics = new List<Diagnostic>();
            IsValid = false;
        }

        public Polygon2 Polygon { get; set; }
        public List<Diagnostic> Diagnostics { get; set; }
        public bool IsValid { get; set; }
    }

    public static class GeometryCleaner
    {
        public static CleanPolygonResult CleanPolygon(PolygonInput input, double tolerance, bool clockwise)
        {
            CleanPolygonResult result = new CleanPolygonResult();
            string sourceId = input != null ? input.Id : string.Empty;

            if (input == null || input.Points == null || input.Points.Count < 3)
            {
                result.Diagnostics.Add(Diagnostic.Error("geometry.too_few_points", "Polygon requires at least three points.", sourceId));
                return result;
            }

            List<Point2> sanitized = new List<Point2>();
            foreach (Point2 raw in input.Points)
            {
                if (raw == null || double.IsNaN(raw.X) || double.IsNaN(raw.Y) || double.IsInfinity(raw.X) || double.IsInfinity(raw.Y))
                {
                    result.Diagnostics.Add(Diagnostic.Error("geometry.invalid_coordinate", "Polygon contains a non-finite coordinate.", sourceId));
                    return result;
                }

                Point2 point = new Point2(SnapNearZero(raw.X, tolerance), SnapNearZero(raw.Y, tolerance));

                if (sanitized.Count > 0 && point.EqualsWithin(sanitized[sanitized.Count - 1], tolerance))
                {
                    result.Diagnostics.Add(Diagnostic.Warning("geometry.duplicate_consecutive_point", "Removed duplicate consecutive polygon point.", sourceId));
                    continue;
                }

                int duplicateIndex = FindDuplicateIndex(sanitized, point, tolerance);
                if (duplicateIndex >= 0)
                {
                    bool closesPolygon = duplicateIndex == 0 && input.Points.IndexOf(raw) == input.Points.Count - 1;
                    if (closesPolygon)
                    {
                        result.Diagnostics.Add(Diagnostic.Info("geometry.closed_ring", "Input ring already included a closing point; internal polygon stores unique vertices.", sourceId));
                    }
                    else
                    {
                        result.Diagnostics.Add(Diagnostic.Warning("geometry.duplicate_point", "Removed repeated polygon vertex within tolerance.", sourceId));
                    }

                    continue;
                }

                sanitized.Add(point);
            }

            if (sanitized.Count >= 2)
            {
                Point2 first = sanitized[0];
                Point2 lastInput = input.Points[input.Points.Count - 1];
                if (!first.EqualsWithin(lastInput, tolerance))
                {
                    result.Diagnostics.Add(Diagnostic.Warning("geometry.implicit_close", "Input polygon was not explicitly closed; engine closed the ring between last and first vertices.", sourceId));
                }
            }

            sanitized = RemoveCollinear(sanitized, tolerance, sourceId, result.Diagnostics);
            if (sanitized.Count < 3)
            {
                result.Diagnostics.Add(Diagnostic.Error("geometry.degenerate_polygon", "Polygon collapsed below three vertices after cleanup.", sourceId));
                return result;
            }

            Polygon2 polygon = new Polygon2(sourceId, sanitized);
            if (GeometryPredicates.PolygonSelfIntersects(polygon, tolerance))
            {
                result.Diagnostics.Add(Diagnostic.Error("geometry.self_intersection", "Polygon has a self-intersection after cleanup.", sourceId));
                result.Polygon = polygon;
                return result;
            }

            double area = polygon.Area();
            if (area <= tolerance * tolerance)
            {
                result.Diagnostics.Add(Diagnostic.Error("geometry.zero_area", "Polygon area is below tolerance after cleanup.", sourceId));
                return result;
            }

            bool isClockwise = polygon.IsClockwise();
            if (isClockwise != clockwise)
            {
                polygon = polygon.Reversed();
                result.Diagnostics.Add(Diagnostic.Info("geometry.normalized_winding", clockwise ? "Normalized polygon winding to clockwise." : "Normalized polygon winding to counter-clockwise.", sourceId));
            }

            result.Polygon = polygon;
            result.IsValid = true;
            return result;
        }

        public static PolygonInput ToPolygonInput(Polygon2 polygon)
        {
            return new PolygonInput
            {
                Id = polygon.SourceId,
                Points = polygon.ClosedVertices()
            };
        }

        private static double SnapNearZero(double value, double tolerance)
        {
            return Math.Abs(value) <= tolerance ? 0.0 : value;
        }

        private static int FindDuplicateIndex(List<Point2> points, Point2 point, double tolerance)
        {
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].EqualsWithin(point, tolerance))
                {
                    return i;
                }
            }

            return -1;
        }

        private static List<Point2> RemoveCollinear(List<Point2> points, double tolerance, string sourceId, List<Diagnostic> diagnostics)
        {
            bool changed = true;
            List<Point2> current = points.Select(p => p.Clone()).ToList();
            while (changed && current.Count >= 3)
            {
                changed = false;
                for (int i = 0; i < current.Count; i++)
                {
                    Point2 previous = current[(i - 1 + current.Count) % current.Count];
                    Point2 point = current[i];
                    Point2 next = current[(i + 1) % current.Count];
                    double edgeScale = Math.Max(previous.DistanceTo(point), point.DistanceTo(next));
                    double crossTolerance = Math.Max(tolerance, tolerance * edgeScale);
                    if (Math.Abs(GeometryPredicates.Cross(previous, point, next)) <= crossTolerance)
                    {
                        current.RemoveAt(i);
                        diagnostics.Add(Diagnostic.Warning("geometry.collinear_vertex_removed", "Removed nearly collinear polygon vertex.", sourceId));
                        changed = true;
                        break;
                    }
                }
            }

            return current;
        }
    }
}
