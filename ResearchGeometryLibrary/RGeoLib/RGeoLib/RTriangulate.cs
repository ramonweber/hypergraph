using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGeoLib
{
    public class RTriangulate
    {
		public static List<HTri> TriangulateConvexPolygon(List<HVertex> convexHullpoints)
		{
			List<HTri> triangles = new List<HTri>();

			for (int i = 2; i < convexHullpoints.Count; i++)
			{
				HVertex a = convexHullpoints[0];
				HVertex b = convexHullpoints[i - 1];
				HVertex c = convexHullpoints[i];

				triangles.Add(new HTri(a, b, c));
			}

			return triangles;
		}

		//This assumes that we have a polygon and now we want to triangulate it
		//The points on the polygon should be ordered counter-clockwise
		//This alorithm is called ear clipping and it's O(n*n) Another common algorithm is dividing it into trapezoids and it's O(n log n)
		//One can maybe do it in O(n) time but no such version is known
		//Assumes we have at least 3 points
		public static List<HTri> TriangulateConcavePolygon(List<Vec3d> points)
		{
			//The list with triangles the method returns
			List<HTri> triangles = new List<HTri>();

			//If we just have three points, then we dont have to do all calculations
			if (points.Count == 3)
			{
				triangles.Add(new HTri(points[0], points[1], points[2]));

				return triangles;
			}



			//Step 1. Store the vertices in a list and we also need to know the next and prev vertex
			List<HVertex> vertices = new List<HVertex>();

			for (int i = 0; i < points.Count; i++)
			{
				vertices.Add(new HVertex(points[i]));
			}

			//Find the next and previous vertex
			for (int i = 0; i < vertices.Count; i++)
			{
				int nextPos = RMath.ClampListIndex(i + 1, vertices.Count);

				int prevPos = RMath.ClampListIndex(i - 1, vertices.Count);

				vertices[i].prevVertex = vertices[prevPos];

				vertices[i].nextVertex = vertices[nextPos];
			}



			//Step 2. Find the reflex (concave) and convex vertices, and ear vertices
			for (int i = 0; i < vertices.Count; i++)
			{
				CheckIfReflexOrConvex(vertices[i]);
			}

			//Have to find the ears after we have found if the vertex is reflex or convex
			List<HVertex> earVertices = new List<HVertex>();

			for (int i = 0; i < vertices.Count; i++)
			{
				IsVertexEar(vertices[i], vertices, earVertices);
			}



			//Step 3. Triangulate!
			while (true)
			{
				//This means we have just one triangle left
				if (vertices.Count == 3)
				{
					//The final triangle
					triangles.Add(new HTri(vertices[0], vertices[0].prevVertex, vertices[0].nextVertex));

					break;
				}

				//Make a triangle of the first ear
				HVertex earVertex = earVertices[0];

				HVertex earVertexPrev = earVertex.prevVertex;
				HVertex earVertexNext = earVertex.nextVertex;

				HTri newTriangle = new HTri(earVertex, earVertexPrev, earVertexNext);

				triangles.Add(newTriangle);

				//Remove the vertex from the lists
				earVertices.Remove(earVertex);

				vertices.Remove(earVertex);

				//Update the previous vertex and next vertex
				earVertexPrev.nextVertex = earVertexNext;
				earVertexNext.prevVertex = earVertexPrev;

				//...see if we have found a new ear by investigating the two vertices that was part of the ear
				CheckIfReflexOrConvex(earVertexPrev);
				CheckIfReflexOrConvex(earVertexNext);

				earVertices.Remove(earVertexPrev);
				earVertices.Remove(earVertexNext);

				IsVertexEar(earVertexPrev, vertices, earVertices);
				IsVertexEar(earVertexNext, vertices, earVertices);
			}

			//Debug.Log(triangles.Count);

			return triangles;
		}

		//Check if a vertex if reflex or convex, and add to appropriate list
		private static void CheckIfReflexOrConvex(HVertex v)
		{
			v.isReflex = false;
			v.isConvex = false;

			//This is a reflex vertex if its triangle is oriented clockwise
			Vec3d a = v.prevVertex.position;
			Vec3d b = v.position;
			Vec3d c = v.nextVertex.position;

			if (HMeshOperations.IsTriangleOrientedClockwise(a, b, c))
			{
				v.isReflex = true;
			}
			else
			{
				v.isConvex = true;
			}
		}

		//Check if a vertex is an ear
		private static void IsVertexEar(HVertex v, List<HVertex> vertices, List<HVertex> earVertices)
		{
			//A reflex vertex cant be an ear!
			if (v.isReflex)
			{
				return;
			}

			//This triangle to check point in triangle
			Vec3d a = v.prevVertex.position;
			Vec3d b = v.position;
			Vec3d c = v.nextVertex.position;

			bool hasPointInside = false;

			for (int i = 0; i < vertices.Count; i++)
			{
				//We only need to check if a reflex vertex is inside of the triangle
				if (vertices[i].isReflex)
				{
					Vec3d p = vertices[i].position;

					//This means inside and not on the hull
					if (RIntersection.IsPointInTriangle(a, b, c, p))
					{
						hasPointInside = true;

						break;
					}
				}
			}

			if (!hasPointInside)
			{
				earVertices.Add(v);
			}
		}

		//Alternative 1. Triangulate with some algorithm - then flip edges until we have a delaunay triangulation
		public static List<HTri> TriangulateByFlippingEdges(List<Vec3d> sites)
		{
			//Step 1. Triangulate the points with some algorithm
			//Vector3 to vertex
			List<HVertex> vertices = new List<HVertex>();

			for (int i = 0; i < sites.Count; i++)
			{
				vertices.Add(new HVertex(sites[i]));
			}

			//Triangulate the convex hull of the sites
			List<HTri> triangles = IncrementalTriangulationAlgorithm.TriangulatePoints(vertices);
			//List triangles = TriangulatePoints.TriangleSplitting(vertices);

			//Step 2. Change the structure from triangle to half-edge to make it faster to flip edges
			List<HEdge> halfEdges = HMeshOperations.TransformFromTriangleToHalfEdge(triangles);

			//Step 3. Flip edges until we have a delaunay triangulation
			int safety = 0;

			int flippedEdges = 0;

			while (true)
			{
				safety += 1;

				if (safety > 100000)
				{
					//Debug.Log("Stuck in endless loop");

					break;
				}

				bool hasFlippedEdge = false;

				//Search through all edges to see if we can flip an edge
				for (int i = 0; i < halfEdges.Count; i++)
				{
					HEdge thisEdge = halfEdges[i];

					//Is this edge sharing an edge, otherwise its a border, and then we cant flip the edge
					if (thisEdge.oppositeEdge == null)
					{
						continue;
					}

					//The vertices belonging to the two triangles, c-a are the edge vertices, b belongs to this triangle
					HVertex a = thisEdge.v;
					HVertex b = thisEdge.nextEdge.v;
					HVertex c = thisEdge.prevEdge.v;
					HVertex d = thisEdge.oppositeEdge.nextEdge.v;

					Vec3d aPos = a.position;
					Vec3d bPos = b.position;
					Vec3d cPos = c.position;
					Vec3d dPos = d.position;

					//Use the circle test to test if we need to flip this edge
					if (RGeoFunctions.IsPointInsideOutsideOrOnCircle(aPos, bPos, cPos, dPos) < 0f)
					{
						//Are these the two triangles that share this edge forming a convex quadrilateral?
						//Otherwise the edge cant be flipped
						if (HMeshOperations.IsQuadrilateralConvex(aPos, bPos, cPos, dPos))
						{
							//If the new triangle after a flip is not better, then dont flip
							//This will also stop the algoritm from ending up in an endless loop
							if (RGeoFunctions.IsPointInsideOutsideOrOnCircle(bPos, cPos, dPos, aPos) < 0f)
							{
								continue;
							}

							//Flip the edge
							flippedEdges += 1;

							hasFlippedEdge = true;

							FlipEdge(thisEdge);
						}
					}
				}

				//We have searched through all edges and havent found an edge to flip, so we have a Delaunay triangulation!
				if (!hasFlippedEdge)
				{
					//Debug.Log("Found a delaunay triangulation");

					break;
				}
			}

			//Debug.Log("Flipped edges: " + flippedEdges);

			//Dont have to convert from half edge to triangle because the algorithm will modify the objects, which belongs to the 
			//original triangles, so the triangles have the data we need

			return triangles;
		}

		private static void FlipEdge(HEdge one)
		{
			//The data we need
			//This edge's triangle
			HEdge two = one.nextEdge;
			HEdge three = one.prevEdge;
			//The opposite edge's triangle
			HEdge four = one.oppositeEdge;
			HEdge five = one.oppositeEdge.nextEdge;
			HEdge six = one.oppositeEdge.prevEdge;
			//The vertices
			HVertex a = one.v;
			HVertex b = one.nextEdge.v;
			HVertex c = one.prevEdge.v;
			HVertex d = one.oppositeEdge.nextEdge.v;



			//Flip

			//Change vertex
			a.edge = one.nextEdge;
			c.edge = one.oppositeEdge.nextEdge;

			//Change half-edge
			//Half-edge - half-edge connections
			one.nextEdge = three;
			one.prevEdge = five;

			two.nextEdge = four;
			two.prevEdge = six;

			three.nextEdge = five;
			three.prevEdge = one;

			four.nextEdge = six;
			four.prevEdge = two;

			five.nextEdge = one;
			five.prevEdge = three;

			six.nextEdge = two;
			six.prevEdge = four;

			//Half-edge - vertex connection
			one.v = b;
			two.v = b;
			three.v = c;
			four.v = d;
			five.v = d;
			six.v = a;

			//Half-edge - triangle connection
			HTri t1 = one.triFace;
			HTri t2 = four.triFace;

			one.triFace = t1;
			three.triFace = t1;
			five.triFace = t1;

			two.triFace = t2;
			four.triFace = t2;
			six.triFace = t2;

			//Opposite-edges are not changing!

			//Triangle connection
			t1.v1 = b;
			t1.v2 = c;
			t1.v3 = d;

			t2.v1 = b;
			t2.v2 = d;
			t2.v3 = a;

			t1.hEdge = three;
			t2.hEdge = four;
		}
	}

	//Sort the points along one axis. The first 3 points form a triangle. Consider the next point and connect it with all
	//previously connected points which are visible to the point. An edge is visible if the center of the edge is visible to the point.
	public static class IncrementalTriangulationAlgorithm
	{
		public static List<HTri> TriangulatePoints(List<HVertex> points)
		{
			List<HTri> triangles = new List<HTri>();

			//Sort the points along x-axis
			//OrderBy is always soring in ascending order - use OrderByDescending to get in the other order
			points = points.OrderBy(n => n.position.X).ToList();

			//The first 3 vertices are always forming a triangle
			HTri newTriangle = new HTri(points[0].position, points[1].position, points[2].position);

			triangles.Add(newTriangle);

			//All edges that form the triangles, so we have something to test against
			List<NLine> edges = new List<NLine>();

			edges.Add(new NLine(newTriangle.v1.position, newTriangle.v2.position));
			edges.Add(new NLine(newTriangle.v2.position, newTriangle.v3.position));
			edges.Add(new NLine(newTriangle.v3.position, newTriangle.v1.position));

			//Add the other triangles one by one
			//Starts at 3 because we have already added 0,1,2
			for (int i = 3; i < points.Count; i++)
			{
				Vec3d currentPoint = points[i].position;

				//The edges we add this loop or we will get stuck in an endless loop
				List<NLine> newEdges = new List<NLine>();

				//Is this edge visible? We only need to check if the midpoint of the edge is visible 
				for (int j = 0; j < edges.Count; j++)
				{
					NLine currentEdge = edges[j];

					Vec3d midPoint = (currentEdge.start + currentEdge.end) / 2f;

					NLine edgeToMidpoint = new NLine(currentPoint, midPoint);

					//Check if this line is intersecting
					bool canSeeEdge = true;

					for (int k = 0; k < edges.Count; k++)
					{
						//Dont compare the edge with itself
						if (k == j)
						{
							continue;
						}

						if (AreLinesIntersecting(edgeToMidpoint, edges[k]))
						{
							canSeeEdge = false;

							break;
						}
					}

					//This is a valid triangle
					if (canSeeEdge)
					{
						NLine edgeToPoint1 = new NLine(currentEdge.start, new HVertex(currentPoint).position);
						NLine edgeToPoint2 = new NLine(currentEdge.end, new HVertex(currentPoint).position);

						newEdges.Add(edgeToPoint1);
						newEdges.Add(edgeToPoint2);

						HTri newTri = new HTri(edgeToPoint1.start, edgeToPoint1.end, edgeToPoint2.start);

						triangles.Add(newTri);
					}
				}


				for (int j = 0; j < newEdges.Count; j++)
				{
					edges.Add(newEdges[j]);
				}
			}


			return triangles;
		}


		private static bool AreLinesIntersecting(NLine edge1, NLine edge2)
		{
			Vec3d l1_p1 = new Vec3d(edge1.start);
			Vec3d l1_p2 = new Vec3d(edge1.end);

			Vec3d l2_p1 = new Vec3d(edge2.start);
			Vec3d l2_p2 = new Vec3d(edge2.end);

			bool isIntersecting = RIntersection.AreLinesIntersecting(l1_p1, l1_p2, l2_p1, l2_p2, true);

			return isIntersecting;
		}
	}


}
