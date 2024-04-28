using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelaunatorSharp;

namespace RGeoLib
{
    public class RVoronoi
    {
		// Interfacing with Delaunator Sharp
		public static NMesh VoronoiMeshFromVecs(List<Vec3d> inputVec)
		{
			//DelaunatorSharp.Delaunator delaunator = new DelaunatorSharp.Delaunator(inputPoints.Select(p => new DelaunatorSharp.Point(p.X, p.Y)).ToList());


			// inputs list of vec3d, converts them to the delaunator list

			List<DelaunatorSharp.Point> inputPoints = new List<DelaunatorSharp.Point>(); // your list of points

			for (int i = 0; i < inputVec.Count; i++)
			{
				DelaunatorSharp.Point tempPoint = new DelaunatorSharp.Point(inputVec[i].X, inputVec[i].Y);
				inputPoints.Add(tempPoint);
			}

			DelaunatorSharp.IPoint[] points = inputPoints.Select(p => (DelaunatorSharp.IPoint)p).ToArray();

			DelaunatorSharp.Delaunator delaunator = new DelaunatorSharp.Delaunator(points);

			IEnumerable<IVoronoiCell> voronoiCells = delaunator.GetVoronoiCells();

			List<NFace> faceList = new List<NFace>();

			foreach (IVoronoiCell cell in voronoiCells)
			{
				List<Vec3d> tempVecs = new List<Vec3d>();

				foreach (IPoint TPointSingle in cell.Points)
				{
					tempVecs.Add(new Vec3d(TPointSingle.X, TPointSingle.Y, 0));
				}

				NFace tempFace = new NFace(tempVecs);
				faceList.Add(tempFace);
			}

			NMesh tempMesh = new NMesh(faceList);
			return tempMesh;

		}
		public static NMesh VoronoiCircumcenterMeshFromVecs(List<Vec3d> inputVec)
		{
			//DelaunatorSharp.Delaunator delaunator = new DelaunatorSharp.Delaunator(inputPoints.Select(p => new DelaunatorSharp.Point(p.X, p.Y)).ToList());


			// inputs list of vec3d, converts them to the delaunator list

			List<DelaunatorSharp.Point> inputPoints = new List<DelaunatorSharp.Point>(); // your list of points

			for (int i = 0; i < inputVec.Count; i++)
			{
				DelaunatorSharp.Point tempPoint = new DelaunatorSharp.Point(inputVec[i].X, inputVec[i].Y);
				inputPoints.Add(tempPoint);
			}

			DelaunatorSharp.IPoint[] points = inputPoints.Select(p => (DelaunatorSharp.IPoint)p).ToArray();

			DelaunatorSharp.Delaunator delaunator = new DelaunatorSharp.Delaunator(points);

			IEnumerable<IVoronoiCell> voronoiCells = delaunator.GetVoronoiCellsBasedOnCircumcenters();

			List<NFace> faceList = new List<NFace>();

			foreach (IVoronoiCell cell in voronoiCells)
			{
				List<Vec3d> tempVecs = new List<Vec3d>();

				foreach (IPoint TPointSingle in cell.Points)
				{
					tempVecs.Add(new Vec3d(TPointSingle.X, TPointSingle.Y, 0));
				}

				NFace tempFace = new NFace(tempVecs);
				faceList.Add(tempFace);
			}

			NMesh tempMesh = new NMesh(faceList);
			return tempMesh;

		}
		public static NMesh VoronoiCentroidMeshFromVecs(List<Vec3d> inputVec)
		{
			//DelaunatorSharp.Delaunator delaunator = new DelaunatorSharp.Delaunator(inputPoints.Select(p => new DelaunatorSharp.Point(p.X, p.Y)).ToList());


			// inputs list of vec3d, converts them to the delaunator list

			List<DelaunatorSharp.Point> inputPoints = new List<DelaunatorSharp.Point>(); // your list of points

			for (int i = 0; i < inputVec.Count; i++)
			{
				DelaunatorSharp.Point tempPoint = new DelaunatorSharp.Point(inputVec[i].X, inputVec[i].Y);
				inputPoints.Add(tempPoint);
			}

			DelaunatorSharp.IPoint[] points = inputPoints.Select(p => (DelaunatorSharp.IPoint)p).ToArray();

			DelaunatorSharp.Delaunator delaunator = new DelaunatorSharp.Delaunator(points);

			IEnumerable<IVoronoiCell> voronoiCells = delaunator.GetVoronoiCellsBasedOnCentroids();

			List<NFace> faceList = new List<NFace>();

			foreach (IVoronoiCell cell in voronoiCells)
			{
				List<Vec3d> tempVecs = new List<Vec3d>();

				foreach (IPoint TPointSingle in cell.Points)
				{
					tempVecs.Add(new Vec3d(TPointSingle.X, TPointSingle.Y, 0));
				}

				NFace tempFace = new NFace(tempVecs);
				faceList.Add(tempFace);
			}

			NMesh tempMesh = new NMesh(faceList);
			return tempMesh;

		}


		// Interfacing with RClipper for boundary output

		public static NMesh VoronoiMeshFromVecsAndBounds(NFace bounds, List<Vec3d> inputVecs)
		{
			// get bounding box of nface
			NFace convexHull = bounds.ConvexHullJarvis();

			NFace BB = NFace.BoundingBox2dMaxEdge(convexHull);

			NLine bbMain = NFace.BoundingBoxDirectionMainLine(convexHull);
			Vec3d vecA = bbMain.start - bbMain.Direction * 4;
			Vec3d vecB = bbMain.end + bbMain.Direction * 4;

			NLine bbSecond = NFace.BoundingBoxDirectionSecondaryLine(convexHull);
			Vec3d vecC = bbSecond.start - bbSecond.Direction * 4;
			Vec3d vecD = bbSecond.end + bbSecond.Direction * 4;

			// add one point on each side to prep for voroni diagram
			List<Vec3d> additionalVecs = new List<Vec3d>() { vecA, vecB, vecC, vecD };
			inputVecs.AddRange(additionalVecs);

			// create voronoi
			NMesh voronoiMesh = RVoronoi.VoronoiCircumcenterMeshFromVecs(inputVecs);

			// cut out voronoi with bounds
			List<NFace> cutoutFaces = new List<NFace>();

			for (int i = 0; i < voronoiMesh.faceList.Count; i++)
			{
				NMesh cutoutMeshTemp = RClipper.clipperIntersection(voronoiMesh.faceList[i], bounds);
				cutoutFaces.AddRange(cutoutMeshTemp.faceList);
			}

			// return all faces inside bounds
			NMesh outMesh = new NMesh(cutoutFaces);

			return outMesh;
		}
		public static NMesh VoronoiMeshFromVecsAndBoundsMerged(NFace bounds, List<Vec3d> inputVecs)
		{
			// same as voronoi from ves and bounds, but adds merge function to merge together

			// get bounding box of nface
			NFace convexHull = bounds.ConvexHullJarvis();

			NFace BB = NFace.BoundingBox2dMaxEdge(convexHull);

			NLine bbMain = NFace.BoundingBoxDirectionMainLine(convexHull);
			Vec3d vecA = bbMain.start - bbMain.Direction * 4;
			Vec3d vecB = bbMain.end + bbMain.Direction * 4;

			NLine bbSecond = NFace.BoundingBoxDirectionSecondaryLine(convexHull);
			Vec3d vecC = bbSecond.start - bbSecond.Direction * 4;
			Vec3d vecD = bbSecond.end + bbSecond.Direction * 4;

			// add one point on each side to prep for voroni diagram
			List<Vec3d> additionalVecs = new List<Vec3d>() { vecA, vecB, vecC, vecD };
			inputVecs.AddRange(additionalVecs);

			// create voronoi
			NMesh voronoiMesh = RVoronoi.VoronoiCircumcenterMeshFromVecs(inputVecs);

			// cut out voronoi with bounds
			List<NFace> cutoutFaces = new List<NFace>();

			for (int i = 0; i < voronoiMesh.faceList.Count; i++)
			{
				NMesh cutoutMeshTemp = RClipper.clipperIntersection(voronoiMesh.faceList[i], bounds);
				cutoutFaces.AddRange(cutoutMeshTemp.faceList);
			}



			// 01 Check in what faces the original points are inside
			List<NFace> mainFaces = new List<NFace>();
			List<NFace> adjacentFaces = new List<NFace>();

			for (int i = 0; i < cutoutFaces.Count; i++)
			{
				bool hasPoint = false;
				for (int j = 0; j < inputVecs.Count; j++)
				{
					bool isinside = RIntersection.insideORonEdgeOfNFace(inputVecs[j], cutoutFaces[i]);
					if (isinside)
						hasPoint = true;
				}
				if (hasPoint)
					mainFaces.Add(cutoutFaces[i]);
				else
					adjacentFaces.Add(cutoutFaces[i]);
			}


			List<NFace> finalFaces = new List<NFace>();

			if (adjacentFaces.Count > 0)
			{
				// 02 Boolean Union of Adjacent
				NMesh unionAdjacent = RClipper.clipperUnion(adjacentFaces);
				List<NFace> restFaces = unionAdjacent.faceList;
				// 03 go through Main and try merger with adjacents
				List<bool> unionWorked = new List<bool>();

				for (int i = 0; i < mainFaces.Count; i++)
				{
					bool currentBool = false;
					List<int> removedInt = new List<int>();
					for (int j = 0; j < restFaces.Count; j++)
					{
						//try union
						NMesh tempUnion = RClipper.clipperUnion(mainFaces[i], restFaces[j]);

						if (tempUnion.faceList.Count == 1)
						{
							currentBool = true;
							finalFaces.AddRange(tempUnion.faceList);
							removedInt.Add(j);
						}
					}
					if (removedInt.Count > 0)
					{
						removedInt.Sort();
						removedInt.Reverse();

						foreach (int index in removedInt)
						{
							restFaces.RemoveAt(index);
						}
					}

					if (currentBool == false)
					{
						finalFaces.Add(mainFaces[i]);
					}
				}
			}
			else
			{
				finalFaces.AddRange(mainFaces);
			}

			// return all faces inside bounds
			//NMesh outMesh = new NMesh(cutoutFaces);
			NMesh outMesh = new NMesh(finalFaces);


			//NMesh outMesh = new NMesh(mainFaces);

			return outMesh;
			//return voronoiMesh;
		}
		public static NMesh MeshPointRelaxationVoronoi(NFace bounds, List<Vec3d> inputVecs, int numIterations)
		{
			NMesh tempMesh = VoronoiMeshFromVecsAndBoundsMerged(bounds, inputVecs);

			for (int i = 0; i < numIterations; i++)
			{
				// get centroids of faces

				List<Vec3d> centroids = new List<Vec3d>();
				for (int j = 0; j < tempMesh.faceList.Count; j++)
				{
					Vec3d tempC = NFace.centroidInsideFace(tempMesh.faceList[j]);
					centroids.Add(tempC);
				}

				inputVecs = centroids;
				tempMesh = VoronoiMeshFromVecsAndBoundsMerged(bounds, inputVecs);
			}

			return tempMesh;


		}



	}


}
