using System;
using System.Collections.Generic;
using System.Linq;
using Dbscan;


namespace RGeoLib
{
    public class RDbscan
    {
        // implements the dbscan class
        public static List<Vec3d> DbscanVec3d(List<Vec3d> inputVecs, double maxDist)
        {
            // Convert vectorList to a double array
            double[][] data = inputVecs.Select(v => new double[] { v.X, v.Y }).ToArray();

            IList<SimplePoint> points = Vec3dToDBList(inputVecs);

            // Create a KMeans algorithm with 3 clusters
            var dbscancluster =  Dbscan.Dbscan.CalculateClusters(points, epsilon: maxDist, minimumPointsPerCluster: 1);

            Console.WriteLine(dbscancluster.Clusters);
            Console.WriteLine("------------------");

            List<Vec3d> outVecs = new List<Vec3d>();

            for (int i = 0;i < dbscancluster.Clusters.Count;i++)
            {   
                List<Vec3d> tempList = new List<Vec3d>();
                for (int j = 0; j < dbscancluster.Clusters[i].Objects.Count; j++)
                {
                    //Console.WriteLine(dbscancluster.Clusters[i].Objects[j]);
                    Vec3d vecSingle = SimplePointToVec3d(dbscancluster.Clusters[i].Objects[j]);
                    tempList.Add(vecSingle);
                }
                NFace tempFace = new NFace(tempList);
                Vec3d centroid = tempFace.Centroid;
                outVecs.Add(centroid);
            }

            for (int i = 0; i < dbscancluster.UnclusteredObjects.Count; i++)
            {
                    //Console.WriteLine(dbscancluster.Clusters[i].Objects[j]);
                    Vec3d vecList = SimplePointToVec3d(dbscancluster.UnclusteredObjects[i]);
                    outVecs.Add(vecList);
               
            }


            Console.WriteLine("------------------");

            Console.WriteLine(dbscancluster);

            //List<Vec3d> outVecs = DBListToVec3d(dbscancluster.Clusters);
            //KMeans kmeans = new KMeans(k: numClusters);

            // Compute the clusters and centroids
            //var clusters = kmeans.Learn(data);
            //double[][] centroids = kmeans.Clusters.Centroids;

            // Convert centroids back to Vec3d list
            //List<Vec3d> centroidList = centroids.Select(c => new Vec3d(c[0], c[1], c[2])).ToList();

            return outVecs;
        }

        public static List<Axis> DbscanAxis(List<Axis> inputAxis, double maxDist)
        {
            // DBScan reduction with AxisList
            
            List<Vec3d> inputVecs = inputAxis.Select(axis => axis.v).ToList();
            
            Dictionary<Vec3d, Axis> axisDict = inputVecs.Zip(inputAxis, (v, a) => new { v, a })
                                     .ToDictionary(x => x.v, x => x.a);


            IList<SimplePoint> points = Vec3dToDBList(inputVecs);

            // Create a KMeans algorithm with 3 clusters
            var dbscancluster = Dbscan.Dbscan.CalculateClusters(points, epsilon: maxDist, minimumPointsPerCluster: 1);

            //Console.WriteLine("------------------");

            List<Axis> outAxis = new List<Axis>();

            for (int i = 0; i < dbscancluster.Clusters.Count; i++)
            {
                List<Vec3d> tempList = new List<Vec3d>();
                List<Axis> axisOfCluster = new List<Axis>();

                for (int j = 0; j < dbscancluster.Clusters[i].Objects.Count; j++)
                {
                    //Console.WriteLine(dbscancluster.Clusters[i].Objects[j]);
                    Vec3d vecSingle = SimplePointToVec3d(dbscancluster.Clusters[i].Objects[j]);
                    Axis axis;
                    // get axis from input axis that has the point vecSingle as .v
                    if (axisDict.TryGetValue(vecSingle, out axis))
                    {
                        // Do something with the axis value...
                        axisOfCluster.Add(axis);
                    }

                    tempList.Add(vecSingle);
                }

                Axis averageOut = Axis.averageAxisList(axisOfCluster);
                outAxis.Add(averageOut);
            }

            for (int i = 0; i < dbscancluster.UnclusteredObjects.Count; i++)
            {
                //Console.WriteLine(dbscancluster.Clusters[i].Objects[j]);
                Vec3d vecSingle = SimplePointToVec3d(dbscancluster.UnclusteredObjects[i]);
                Axis axis;
                // get axis from input axis that has the point vecSingle as .v
                if (axisDict.TryGetValue(vecSingle, out axis))
                {
                    // Do something with the axis value...
                    outAxis.Add(axis);
                }
            }

            return outAxis;
        }

        public class SimplePoint : IPointData
        {
            public SimplePoint(double x, double y) =>
                Point = new Point(x, y);

            public Point Point { get; }
        }
        public static Vec3d SimplePointToVec3d(SimplePoint inputPoint)
        {
            Vec3d tempPt = new Vec3d(inputPoint.Point.X, inputPoint.Point.Y, 0);
            
            return tempPt;
        }
        public static IList<SimplePoint> Vec3dToDBList( List<Vec3d> inputVecs)
	    {
            IList<SimplePoint> pointListOut = new List<SimplePoint>();

            for (int i = 0; i < inputVecs.Count; i++)
            {
                var tempP = new SimplePoint(x: inputVecs[i].X, y: inputVecs[i].Y);
                pointListOut.Add(tempP);
            }
            return pointListOut;
	    }

        public static List<Vec3d> DBListToVec3d(IList<SimplePoint> inputPoints)
        {
            //IList<SimplePoint> pointListOut = new List<SimplePoint>();
            List < Vec3d > outVecs = new List<Vec3d>();

            for (int i = 0; i < inputPoints.Count; i++)
            {
                Vec3d tempPt = new Vec3d(inputPoints[i].Point.X, inputPoints[i].Point.Y, 0);
                outVecs.Add(tempPt);
            }
            return outVecs;
        }

    }
}
