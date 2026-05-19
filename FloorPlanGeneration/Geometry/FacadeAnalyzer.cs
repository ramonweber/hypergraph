using System;
using FloorPlanGeneration.Schema;

namespace FloorPlanGeneration.Geometry
{
    public static class FacadeAnalyzer
    {
        public static double DaylightFacadeLength(Polygon2 polygon, Polygon2 floorplate, FacadeInput facade, double tolerance)
        {
            if (facade != null && facade.Segments != null && facade.Segments.Count > 0)
            {
                double length = 0.0;
                foreach (LineSegment2 edge in polygon.Edges())
                {
                    foreach (FacadeSegmentInput segment in facade.Segments)
                    {
                        if (!segment.DaylightCapable)
                        {
                            continue;
                        }

                        length += GeometryPredicates.SharedSegmentLength(edge, new LineSegment2(segment.Start, segment.End), tolerance);
                    }
                }

                return length;
            }

            return GeometryPredicates.SharedBoundaryLength(polygon, floorplate, tolerance);
        }

        public static bool HasDaylightExposure(Polygon2 polygon, Polygon2 floorplate, FacadeInput facade, double tolerance)
        {
            return DaylightFacadeLength(polygon, floorplate, facade, tolerance) > Math.Max(tolerance, 0.05);
        }
    }
}
