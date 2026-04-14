using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGeoLib
{
    
    public static class GeometryUtils
    {
        


        /// <summary>
        /// Expands a line to intersect with the edges of an NFace.
        /// Checks where the line's extension intersects the face edges and extends the line accordingly.
        /// </summary>
        /// <param name="inputLine">The line to expand</param>
        /// <param name="face">The NFace boundary to expand to</param>
        /// <returns>Expanded NLine that intersects with the face edges, or original line if no valid intersections</returns>
        public static NLine ExpandLineToNFace(NLine inputLine, NFace face)
        {
            // Get all edges of the NFace as lines
            List<NLine> faceEdges = RhConvert.NFaceToLineList(face);

            Vec3d l1_start = inputLine.start;
            Vec3d l1_end = inputLine.end;

            // Track the furthest extensions
            double minParametric = 0.0;  // Most negative (backward extension)
            double maxParametric = 1.0;  // Most positive (forward extension)
            
            Vec3d newStart = l1_start;
            Vec3d newEnd = l1_end;

            // Check intersection with each edge
            foreach (NLine edge in faceEdges)
            {
                Vec3d l2_start = edge.start;
                Vec3d l2_end = edge.end;

                // Calculate denominator (check if lines are parallel)
                double denominator = (l2_end.Y - l2_start.Y) * (l1_end.X - l1_start.X) - 
                                   (l2_end.X - l2_start.X) * (l1_end.Y - l1_start.Y);

                // Skip if lines are parallel
                if (Math.Abs(denominator) < 0.000001)
                    continue;

                // Get intersection point
                Vec3d intersectionPoint = RIntersection.GetLineLineIntersectionPoint(
                    l1_start, l1_end, l2_start, l2_end
                );

                // Check if intersection lies within the face edge bounds
                bool onEdge = RIntersection.PointLineIntersection(
                    intersectionPoint, l2_start, l2_end
                );

                if (!onEdge)
                    continue;

                // Calculate parametric value on line 1 (u_a)
                double u_a = ((l2_end.X - l2_start.X) * (l1_start.Y - l2_start.Y) - 
                             (l2_end.Y - l2_start.Y) * (l1_start.X - l2_start.X)) / denominator;

                // Update bounds based on parametric value
                if (u_a < minParametric)
                {
                    minParametric = u_a;
                    newStart = intersectionPoint;
                }
                
                if (u_a > maxParametric)
                {
                    maxParametric = u_a;
                    newEnd = intersectionPoint;
                }
            }

            // Return the expanded line
            return new NLine(newStart, newEnd);
        }

        /// <summary>
        /// Checks if a line crosses through an NFace (not just lying on boundary).
        /// Returns true only if the line properly crosses the face edges at a point.
        /// A line that only lies on the boundary does NOT count as crossing.
        /// </summary>
        /// <param name="lineA">The line to check</param>
        /// <param name="faceA">The face to check against</param>
        /// <returns>True if line crosses face, false if it only lies on boundary or doesn't intersect</returns>
        public static bool DoesLineCrossFace(NLine lineA, NFace faceA)
        {
            bool hasCrossing = false;

            for (int i = 0; i < faceA.edgeList.Count; i++)
            {
                Tuple<bool, bool, bool, Vec3d, Vec3d, NLine, string> intTuple = 
                    RIntersection.intersectionQuery(
                        faceA.edgeList[i].v, 
                        faceA.edgeList[i].nextNEdge.v, 
                        lineA.start, 
                        lineA.end
                    );

                // Print intersection information for this edge
                if (intTuple.Item1) // Any intersection
                {
                    Console.WriteLine($"Edge {i} - Intersected: {intTuple.Item1}, Single: {intTuple.Item2}, Segment: {intTuple.Item3}");
                    Console.WriteLine($"  Description: {intTuple.Item7}");
                }

                // Item2: intersectedSingle (crossing at single point)
                // This is a TRUE crossing - line passes through the face edge
                if (intTuple.Item2 == true)
                {
                    hasCrossing = true;
                    break; // Found a crossing, no need to check further
                }
            }

            return hasCrossing;
        }

        /// <summary>
        /// Checks if a line crosses a face by testing if face vertices are on opposite sides of the line.
        /// This is a simpler geometric approach: if vertices are split by the line, it crosses the face.
        /// Works best for convex faces.
        /// </summary>
        /// <param name="lineA">The line to check</param>
        /// <param name="faceA">The face to check against</param>
        /// <returns>True if line splits the face vertices (some vertices on each side)</returns>
        public static bool DoesLineCrossFaceByVertexSides(NLine lineA, NFace faceA)
        {
            bool hasPositive = false;
            bool hasNegative = false;
            double tolerance = 0.00001;

            // Check each vertex of the face
            for (int i = 0; i < faceA.edgeList.Count; i++)
            {
                Vec3d vertex = faceA.edgeList[i].v;
                
                // Calculate which side of the line this vertex is on
                // Using cross product: (lineEnd - lineStart) × (vertex - lineStart)
                Vec3d lineDir = lineA.end - lineA.start;
                Vec3d toVertex = vertex - lineA.start;
                Vec3d cross = Vec3d.CrossProduct(lineDir, toVertex);
                
                double sideValue = cross.Z; // Z component gives us the 2D cross product result
                
                // Console.WriteLine($"Vertex {i}: ({vertex.X:F2}, {vertex.Y:F2}) - Side value: {sideValue:F6}");
                
                if (sideValue > tolerance)
                    hasPositive = true;
                else if (sideValue < -tolerance)
                    hasNegative = true;
            }

            // If we have vertices on both sides, the line crosses the face
            bool crosses = hasPositive && hasNegative;
            
            // Console.WriteLine($"Has positive: {hasPositive}, Has negative: {hasNegative} -> Crosses: {crosses}");
            
            return crosses;
        }
        
    }
}
