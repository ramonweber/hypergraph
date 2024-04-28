using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGeoLib
{
    public class HMeshOperations
    {
		public static void OrientTrianglesClockwise(List<HTri> triangles)
		{
			for (int i = 0; i < triangles.Count; i++)
			{
				HTri tri = triangles[i];

				Vec3d v1 = new Vec3d(tri.v1.position);
				Vec3d v2 = new Vec3d(tri.v2.position);
				Vec3d v3 = new Vec3d(tri.v3.position);

				if (!HMeshOperations.IsTriangleOrientedClockwise(v1, v2, v3))
				{
					tri.ChangeOrientation();
				}
			}
		}

		//Is a triangle in 2d space oriented clockwise or counter-clockwise
		//https://math.stackexchange.com/questions/1324179/how-to-tell-if-3-connected-points-are-connected-clockwise-or-counter-clockwise
		//https://en.wikipedia.org/wiki/Curve_orientation
		public static bool IsTriangleOrientedClockwise(Vec3d p1, Vec3d p2, Vec3d p3)
		{
			bool isClockWise = true;

			double determinant = p1.X * p2.Y + p3.X * p1.Y + p2.X * p3.Y - p1.X * p3.Y - p3.X * p2.Y - p2.X * p1.Y;

			if (determinant > 0f)
			{
				isClockWise = false;
			}

			return isClockWise;
		}

		//Is a quadrilateral convex? Assume no 3 points are colinear and the shape doesnt look like an hourglass
		public static bool IsQuadrilateralConvex(Vec3d a, Vec3d b, Vec3d c, Vec3d d)
		{
			bool isConvex = false;

			bool abc = HMeshOperations.IsTriangleOrientedClockwise(a, b, c);
			bool abd = HMeshOperations.IsTriangleOrientedClockwise(a, b, d);
			bool bcd = HMeshOperations.IsTriangleOrientedClockwise(b, c, d);
			bool cad = HMeshOperations.IsTriangleOrientedClockwise(c, a, d);

			if (abc && abd && bcd & !cad)
			{
				isConvex = true;
			}
			else if (abc && abd && !bcd & cad)
			{
				isConvex = true;
			}
			else if (abc && !abd && bcd & cad)
			{
				isConvex = true;
			}
			//The opposite sign, which makes everything inverted
			else if (!abc && !abd && !bcd & cad)
			{
				isConvex = true;
			}
			else if (!abc && !abd && bcd & !cad)
			{
				isConvex = true;
			}
			else if (!abc && abd && !bcd & !cad)
			{
				isConvex = true;
			}


			return isConvex;
		}

		public static List<HEdge> TransformFromTriangleToHalfEdge(List<HTri> triangles)
		{
			//Make sure the triangles have the same orientation
			HMeshOperations.OrientTrianglesClockwise(triangles);

			//First create a list with all possible half-edges
			List<HEdge> halfEdges = new List<HEdge>(triangles.Count * 3);

			for (int i = 0; i < triangles.Count; i++)
			{

				HTri t = triangles[i];

				HEdge he1 = new HEdge(t.v1);
				HEdge he2 = new HEdge(t.v2);
				HEdge he3 = new HEdge(t.v3);

				he1.nextEdge = he2;
				he2.nextEdge = he3;
				he3.nextEdge = he1;

				he1.prevEdge = he3;
				he2.prevEdge = he1;
				he3.prevEdge = he2;

				//The vertex needs to know of an edge going from it
				he1.v.edge = he2;
				he2.v.edge = he3;
				he3.v.edge = he1;

				//The face the half-edge is connected to
				t.hEdge = he1;

				he1.triFace = t;
				he2.triFace = t;
				he3.triFace = t;

				//Add the half-edges to the list
				halfEdges.Add(he1);
				halfEdges.Add(he2);
				halfEdges.Add(he3);
			}

			//Find the half-edges going in the opposite direction
			for (int i = 0; i < halfEdges.Count; i++)
			{
				HEdge he = halfEdges[i];

				HVertex goingToVertex = he.v;
				HVertex goingFromVertex = he.prevEdge.v;

				for (int j = 0; j < halfEdges.Count; j++)
				{
					//Dont compare with itself
					if (i == j)
					{
						continue;
					}

					HEdge heOpposite = halfEdges[j];

					//Is this edge going between the vertices in the opposite direction
					if (goingFromVertex.position == heOpposite.v.position && goingToVertex.position == heOpposite.prevEdge.v.position)
					{
						he.oppositeEdge = heOpposite;

						break;
					}
				}
			}


			return halfEdges;
		}


	}
}
