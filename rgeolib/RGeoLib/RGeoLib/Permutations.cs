using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGeoLib
{
    public class Permutations
    {
        // Usage
        //var vals1 = new[] { "500", "200", "300" };
        //foreach (var v1 in Permutations.Perm(vals1))
        //{
        //  Console.WriteLine(string.Join(",", v1));
        //}

        // input "25", "26", "27", "28"
        // 

        // 1. Permutate order
        // 2. Permutate number of clusters

        // with 2 clusters
        // input A, B, C, D

        // ClusterSize1 = 0-4 ClusterSize2 length-clusterSize2
        // output - / A,B,C,D
        // output A / B,C,D
        // output A, B  /  C,D
        // output A,B,C / D
       

        // input A,B,C,D

        // output 

        // 4 / 0
        // 3 / 1
        // 2 / 2
        // 1 / 3



        // MaxNumber of clusters = length of list - 1, minimum number of clusters = 1

        public static List<List<int>> Clustering<T>(T[] values)
        {
            List<List<int>> clusterSize = new List<List<int>>();
            
            int maxClusterNumber = values.Length;
            int minClusterNumber = 1;

            for (int i = 0; i < maxClusterNumber; i++)
            {
                int firstValue = i;
                int secondValue = maxClusterNumber-i;

                List<int> current = new List<int> { firstValue, secondValue };
                clusterSize.Add(current);
            }
            return clusterSize;
        }

        public static IEnumerable<T[]> Perm<T>(T[] values, int fromInd = 0)
        {
            if (fromInd + 1 == values.Length)
                yield return values;
            else
            {
                foreach (var v in Perm(values, fromInd + 1))
                    yield return v;

                for (var i = fromInd + 1; i < values.Length; i++)
                {
                    SwapValues(values, fromInd, i);
                    foreach (var v in Perm(values, fromInd + 1))
                        yield return v;
                    SwapValues(values, fromInd, i);
                }
            }
        }

        public static void SwapValues<T>(T[] values, int pos1, int pos2)
        {
            if (pos1 != pos2)
            {
                T tmp = values[pos1];
                values[pos1] = values[pos2];
                values[pos2] = tmp;
            }
        }

        // Console.WriteLine("---------------------------");
        //    Console.WriteLine("Permutations of {rotation values}");
        //    Console.WriteLine();
        //
        //    var permoutputs = Permutations.GetPermutationsWithRept(rotVals, roomSizes.Length);
        //
        //    foreach (var v in permoutputs)
        //        Console.WriteLine(string.Join(",", v));
        //
        //    Console.WriteLine("---------------------------");

        // Permutations for Rots
        public static IEnumerable<IEnumerable<T>> GetPermutationsWithRept<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });
            return GetPermutationsWithRept(list, length - 1)
                .SelectMany(t => list,
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }
    }
}
