using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.MachineLearning;

namespace RGeoLib
{
    public class RAccord
    {
        public static List<Vec3d> KMeansVec3d(List<Vec3d> inputVecs, int numClusters)
        {
            // Convert vectorList to a double array
            double[][] data = inputVecs.Select(v => new double[] { v.X, v.Y, v.Z }).ToArray();

            // Create a KMeans algorithm with 3 clusters
            KMeans kmeans = new KMeans(k: numClusters);

            // Compute the clusters and centroids
            var clusters = kmeans.Learn(data);
            double[][] centroids = kmeans.Clusters.Centroids;

            // Convert centroids back to Vec3d list
            List<Vec3d> centroidList = centroids.Select(c => new Vec3d(c[0], c[1], c[2])).ToList();

            return centroidList;
        }


    }
}
