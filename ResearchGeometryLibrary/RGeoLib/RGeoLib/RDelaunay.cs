using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelaunatorSharp;

namespace RGeoLib
{
    public class RDelaunay
    {

		// interfacing with Delaunator Sharp Class

		public static NMesh DelaunayMeshFromVecs(List<Vec3d> inputVec)
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

			IEnumerable<ITriangle> triangleList = delaunator.GetTriangles();

			List<NFace> faceList = new List<NFace>();

			foreach (ITriangle triangle in triangleList)
            {
				List<Vec3d> tempVecs = new List<Vec3d>();

				foreach (IPoint TPointSingle in triangle.Points)
				{
					tempVecs.Add(new Vec3d(TPointSingle.X, TPointSingle.Y, 0));
				}

				NFace tempFace = new NFace(tempVecs);
				faceList.Add(tempFace);
            }

			NMesh tempMesh = new NMesh(faceList);
			return tempMesh;

		}
		
	}
}
