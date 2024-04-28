using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickGraph;
using QuickGraph.Algorithms;


namespace RGeoLib
{
    public class RGraph
    {
        public AdjacencyGraph<string, Edge<string>> graph;
        public Dictionary<Edge<string>, double> costs;


        // uses quickgraph

        public RGraph(List<NLine> lineList)
        {
            AdjacencyGraph<string, Edge<string>> _graph = new AdjacencyGraph<string, Edge<string>>();
            Dictionary<Edge<string>, double> _costs = new Dictionary<Edge<string>, double>();

            this.graph = _graph;
            this.costs = _costs;

            foreach (var line in lineList)
            {
                AddNLineWithCosts(line);
            }
        }
        
        private void AddNLineWithCosts(NLine inputLine)
        {
            string vecStartString = Vec3d.serializeVec(inputLine.start);
            string vecEndString = Vec3d.serializeVec(inputLine.end);

            var edge = new Edge<string>(vecStartString, vecEndString);
            this.graph.AddVerticesAndEdge(edge);
            double cost = inputLine.Length;
            this.costs.Add(edge, cost);
        }

        public List<Vec3d> ReturnShortestPathAsLine(Vec3d startVec, Vec3d endVec)
        {
            string @from = Vec3d.serializeVec(startVec);
            string to = Vec3d.serializeVec(endVec);


            var edgeCost = AlgorithmExtensions.GetIndexer(costs);
            var tryGetPath = this.graph.ShortestPathsDijkstra(edgeCost, @from);
            List<Vec3d> outVecs = new List<Vec3d>();
            IEnumerable<Edge<string>> path;
            if (tryGetPath(to, out path))
            {
                PrintPath(@from, to, path);
                outVecs = PrintAndReturnPath(@from, to, path);
            }
            else
            {
                Console.WriteLine("No path found from {0} to {1}.");
            }

            return outVecs;
        }
        public static List<Vec3d> PrintAndReturnPath(string @from, string to, IEnumerable<Edge<string>> path)
        {
            List<Vec3d> vecList = new List<Vec3d>();

            Console.Write("Path found from {0} to {1}: {0}", @from, to);
            foreach (var e in path)
            {
                Console.Write(" > {0}", e.Target);
                Vec3d tempVec = Vec3d.deserializeVec(e.Target);
                vecList.Add(tempVec);
            }
            Console.WriteLine();
            return vecList;
        }

        public static void PrintPath(string @from, string to, IEnumerable<Edge<string>> path)
        {
            Console.Write("Path found from {0} to {1}: {0}", @from, to);
            foreach (var e in path)
                Console.Write(" > {0}", e.Target);
            Console.WriteLine();
        }
        /*

        public List<Vec3d> ReturnShortestPathAsLine(string @from, string to)
        {
            var edgeCost = AlgorithmExtensions.GetIndexer(costs);
            var tryGetPath = this.graph.ShortestPathsDijkstra(edgeCost, @from);
            List<Vec3d> outVecs = new List<Vec3d>();
            IEnumerable<Edge<string>> path;
            if (tryGetPath(to, out path))
            {
                PrintPath(@from, to, path);
                outVecs = PrintAndReturnPath(@from, to, path);
            }
            else
            {
                Console.WriteLine("No path found from {0} to {1}.");
            }

            return outVecs;
        }
        public void PrintShortestPath(string @from, string to)
        {
            var edgeCost = AlgorithmExtensions.GetIndexer(costs);
            var tryGetPath = this.graph.ShortestPathsDijkstra(edgeCost, @from);

            IEnumerable<Edge<string>> path;
            if (tryGetPath(to, out path))
            {
                PrintPath(@from, to, path);
            }
            else
            {
                Console.WriteLine("No path found from {0} to {1}.");
            }
        }



        

        



        
        private void AddEdgeWithCosts(string source, string target, double cost)
        {
            var edge = new Edge<string>(source, target);
            _graph.AddVerticesAndEdge(edge);
            _costs.Add(edge, cost);
        }
        */

    }
}


/*
 * 
 * 
 * 
 * 
 * 
 * 
 */