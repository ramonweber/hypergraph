using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Text.Json;
using System.Text.Json.Serialization;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Factorization;
using Accord.Statistics.Analysis;
using Accord.MachineLearning;

using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Math;
using Accord.Math.Distances;
using Accord.Statistics;
using Accord.Statistics.Kernels;
using Accord.Statistics.Visualizations;
using Accord.IO;
using Accord.MachineLearning.Clustering;

namespace RGeoLib
{
    public class DataNode
    {


        // Properties of DataNode
        [JsonPropertyName("name")] public string name { get; set; }
        [JsonPropertyName("area")] public double area { get; set; }
        [JsonPropertyName("angle")] public double angle { get; set; }

        // set room id for merging later
        [JsonPropertyName("mergeid")] public string mergeid { get; set; }

        // set to true if room, to false if hierarchical split
        [JsonPropertyName("final")] public bool final { get; set; }

        // Create a property to hold the children of the DataNode
        [JsonPropertyName("children")] public List<DataNode> children { get; set; }

        [JsonPropertyName("connected")] public List<string> connected { get; set; } // List of "names" of connected rooms

        public bool _subdivisionerror = false;
        public NMesh treeNodeMesh { get; set; }

        //private NMesh treeNodeMesh;
        private NFace bounds;
        private List<Vec3d> connectivityVecs;

        // Constructor to initialize the children property
        // Default constructor needed for deserialization
        
       
        public DataNode()
        {
            this.children = new List<DataNode>();
        }
        
        public DataNode(string name, double area, double angle, bool final, List<DataNode> nodeList, List<string> connectedList)
        {
            this.name = name;
            this.area = area;
            this.angle = angle;
            this.final = final;
            this.mergeid = name;
            this.children = nodeList;
            this.connected = connectedList;
        }
        public DataNode(string name, double area, double angle, bool final, List<DataNode> nodeList)
        {
            this.name = name;
            this.area = area;
            this.angle = angle;
            this.final = final;
            this.mergeid = name;
            this.children = nodeList;
            this.connected = new List<string>();
        }
        
        [JsonConstructor]
        public DataNode(string name, double area, double angle, bool final)
        {
            this.name = name;
            this.area = area;
            this.angle = angle;
            this.final = final;
            this.mergeid = name;
            this.children = new List<DataNode>();
            this.connected = new List<string>();
        }

        // Method to add a child to the DataNode
        public void AddChild(DataNode child)
        {
            children.Add(child);
        }

        public static List<DataNode> GetAllChildren(DataNode node)
        {
            // Create a new List<DataNode> to store the children
            List<DataNode> flattenedList = new List<DataNode>();

            // Add all of the children of the input node to the list
            flattenedList.AddRange(node.children);

            // Call the method on each child to add all of its children to the list
            foreach (DataNode child in node.children)
            {
                flattenedList.AddRange(GetAllChildren(child));
            }

            // Return the list of all children of the input node
            return flattenedList;
        }

        public void AddMesh(NMesh inputMesh)
        {
            this.treeNodeMesh = inputMesh;
        }
        public NMesh returnMesh()
        {
            return this.treeNodeMesh;
        }
        public void AddBounds(NFace inputBounds)
        {
            this.bounds = inputBounds;
        }
        public NFace returnBounds()
        {
            return this.bounds;
        }
        public void AddConnectivityVecs(List<Vec3d> connectedPts)
        {
            this.connectivityVecs = connectedPts;
        }
        public List<Vec3d> returnConnectivityVecs()
        {
            return this.connectivityVecs;
        }

        public static DataNode convertTreeToNode(TreeNode inputTree)
        {
            bool haschildren = true;
            if (inputTree._children.Count < 1)
                haschildren = false;

            DataNode currentNode = new DataNode(inputTree.ID, inputTree.split, inputTree.angle, haschildren);

            // Convert all children of the input tree to DataNodes and add them to the current node
            List<TreeNode> allChildren = inputTree.GetAllChildren();
            foreach (TreeNode treeX in allChildren)
            {
                DataNode childNode = convertTreeToNode(treeX);
                currentNode.AddChild(childNode);
            }

            return currentNode;
        }

        // Create from NMesh and Bounds
        public static DataNode dataNodeFromNMeshAndBounds(NMesh inputMesh, NFace bounds)
        {
            DataNode rootNode = new DataNode("root", bounds.Area, 0, false);
            rootNode.AddMesh(inputMesh);
            rootNode.AddBounds(bounds);

            updateAllNodes(rootNode);

            List<NLine> tempLines = NMesh.identifyLongestAxis(inputMesh);

            List<DataNode> allNodes = DataNode.GetAllChildren(rootNode);

            List<int> childList = new List<int>();
            List<NMesh> outMeshList = new List<NMesh>();
            List<NFace> boundsList = new List<NFace>();

            for (int i = 0; i < allNodes.Count; i++)
            {
                int numChildren = allNodes[i].children.Count;
                if (numChildren == 0)
                {
                    allNodes[i].final = true;

                    NMesh tempMesh = allNodes[i].returnMesh();
                    allNodes[i].mergeid = tempMesh.faceList[0].merge_id;
                    if (tempMesh.faceList.Count > 0)
                    {
                        childList.Add(tempMesh.faceList.Count);
                        outMeshList.Add(tempMesh);

                        NFace tempBounds = allNodes[i].returnBounds();
                        boundsList.Add(tempBounds);
                    }

                    // check if node is final node

                    // if yes, add list of neighbors. 
                }
            }

            return rootNode;
        }

        public static DataNode dataNodeFromNMeshAndBoundsWithConnectivity(NMesh inputMesh, NFace bounds, List<Vec3d> connectedPt)
        {
            DataNode rootNode = new DataNode("root", bounds.Area, 0, false);
            rootNode.AddMesh(inputMesh);
            rootNode.AddBounds(bounds);
            rootNode.AddConnectivityVecs(connectedPt);

            updateAllNodes(rootNode);

            List<NLine> tempLines = NMesh.identifyLongestAxis(inputMesh);

            List<DataNode> allNodes = DataNode.GetAllChildren(rootNode);

            List<int> childList = new List<int>();
            List<NMesh> outMeshList = new List<NMesh>();
            List<NFace> boundsList = new List<NFace>();

            for (int i = 0; i < allNodes.Count; i++)
            {
                int numChildren = allNodes[i].children.Count;

                // check if node is final node
                if (numChildren == 0)
                {
                    allNodes[i].final = true;

                    NMesh tempMesh = allNodes[i].returnMesh();
                    allNodes[i].mergeid = tempMesh.faceList[0].merge_id;
                    if (tempMesh.faceList.Count > 0)
                    {
                        childList.Add(tempMesh.faceList.Count);
                        outMeshList.Add(tempMesh);

                        NFace tempBounds = allNodes[i].returnBounds();
                        boundsList.Add(tempBounds);
                    }

                    // if yes, add list of neighbors. 

                }
            }

            // construct neighbor dict
            // dict, int (connectionVec index), List<string> (list of names of faces that connect to node)
            Dictionary<int, List<string>> connectedDict = new Dictionary<int, List<string>>();

            for (int i = 0; i < connectedPt.Count; i++)
            {
                List<string> namesOfConnected = new List<string>();

                // Go through each of the nodes
                for (int j = 0; j < allNodes.Count; j++)
                {
                    // check if node is final node

                    if (allNodes[j].final)
                    {
                        // check if node mesh touches one of the connected pt
                        // if yes, add list of neighbors. 
                        NMesh tempMesh = allNodes[j].returnMesh();

                        // since this is the final node it should only have one face in the mesh
                        // taking first face
                        // check if face intersects with current connected pt
                        bool intWithConnected = RIntersection.onNFaceEdge(connectedPt[i], tempMesh.faceList[0]);

                        if (intWithConnected)
                        {
                            namesOfConnected.Add(allNodes[j].name);
                        }
                    }

                }

                connectedDict.Add(i, namesOfConnected);
            }

            for (int i = 0; i < allNodes.Count; i++)
            {
                // check if node is final node

                if (allNodes[i].final)
                {
                    // Get name of node
                    string nodeName = allNodes[i].name;
                    // look through dict
                    // check if any dict value contains node name

                    // if yes, add all other node names to the allNodes[i].connected string list
                    List<string> other = new List<string>();

                    foreach (var kvp in connectedDict)
                    {
                        int key = kvp.Key;
                        List<string> nodes = kvp.Value;

                        if (nodes.Contains(nodeName))
                        {
                            other.AddRange(nodes.Where(n => n != nodeName));
                        }
                    }

                    allNodes[i].connected = other;
                }
            }

            return rootNode;
        }

        // Update and Convert
        public static void updateHierarchyNodes(DataNode inputNode)
        {
            // set subdivision actual as true for actual
            // set subdivision actual to false for ratio subdivisions

            updateOneNode(inputNode);

            foreach (var child in inputNode.children)
            {
                NMesh tempMesh = child.returnMesh();
                if (tempMesh.faceList.Count > 0)
                {
                    updateHierarchyNodes(child);
                }
            }
        }
        public static DataNode updateAllNodes(DataNode inputNode)
        {
            //if (inputNode.final == false)
            updateOneNode(inputNode);


            foreach (var child in inputNode.children)
            {
                NMesh tempMesh = child.returnMesh();
                if (tempMesh.faceList.Count > 1)
                {
                    updateAllNodes(child);
                }
            }
            return inputNode;
        }
        public static void updateOneNode(DataNode dataNodeC)
        {
            NMesh inputMesh = dataNodeC.returnMesh();
            NFace bounds = dataNodeC.returnBounds();

            // 02 Identify longest axis
            List<NLine> tempLines = NMesh.identifyLongestAxis(inputMesh);
            List<NLine> inputLines = NMesh.identifyLongestAxis(inputMesh);

            // 03 Split into two

            string treeNodeID = "roomSplit";
            double tolerance = 0.001;
            // go through lines
            bool worked = false;

            NMesh meshRight = inputMesh;
            NMesh meshLeft = inputMesh;
            double angle = Vec3d.Angle(inputLines[0].Direction, Vec3d.UnitX);
            NLine outLine = inputLines[0];

            for (int i = 0; i < inputLines.Count; i++)
            {
                //NLine snappedLine = NLine.snapLineToNFace(inputLines[i], bounds, 0.03);
                //inputLines[i] = snappedLine;
                outLine = inputLines[i];
                // if line ends are on bounds
                bool startOnFace = RIntersection.onNFaceEdge(inputLines[i].start, bounds);
                bool endOnFace = RIntersection.onNFaceEdge(inputLines[i].end, bounds);

                List<NFace> facesLeft = new List<NFace>();
                List<NFace> facesRight = new List<NFace>();

                double areaLeft = 0;
                double areaRight = 0;

                angle = Vec3d.Angle(inputLines[i].Direction, Vec3d.UnitX);

                if (startOnFace == true && endOnFace == true)
                {
                    for (int j = 0; j < inputMesh.faceList.Count; j++)
                    {
                        // closest point of centroid of face to line
                        Vec3d centroid = inputMesh.faceList[j].Centroid;
                        Vec3d perpLineStart = RIntersection.LineClosestPoint2D(centroid, inputLines[i].start, inputLines[i].end);

                        // see how many faces of the mesh are on the right and on the left of the vector
                        Vec3d cross = Vec3d.CrossProduct(inputLines[i].Direction, centroid - perpLineStart);
                        if (cross.Z >= 0)
                        {
                            facesLeft.Add(inputMesh.faceList[j]);
                            areaLeft += inputMesh.faceList[j].Area;
                        }
                        else
                        {
                            facesRight.Add(inputMesh.faceList[j]);
                            areaRight += inputMesh.faceList[j].Area;
                        }
                    }
                }

                if (areaLeft > tolerance && areaRight > tolerance && facesLeft.Count > 0 && facesRight.Count > 0)
                {
                    // output two meshes
                    worked = true;

                    meshLeft = new NMesh(facesLeft);
                    meshRight = new NMesh(facesRight);

                    string leftId = dataNodeC.name + "L";
                    string rightId = dataNodeC.name + "R";

                    DataNode nodeLeft = new DataNode(leftId, areaLeft, angle, false);
                    DataNode nodeRight = new DataNode(rightId, areaRight, angle, false);

                    nodeLeft.AddMesh(meshLeft);
                    nodeRight.AddMesh(meshRight);

                    if (meshLeft.faceList.Count < 2)
                    {
                        nodeLeft.final = true;
                    }
                    if (meshRight.faceList.Count < 2)
                    {
                        nodeRight.final = true;
                    }

                    // output two bounds
                    Tuple<NMesh, bool> boundsTuple = RSplit.divideNFaceWithNLine(bounds, inputLines[i]);
                    if (boundsTuple.Item2)
                    {
                        //nodeLeft.AddBounds(boundsTuple.Item1.faceList[0]);
                        //nodeRight.AddBounds(boundsTuple.Item1.faceList[1]);

                        // closest point of centroid of face to line
                        Vec3d centroidBoundsA = boundsTuple.Item1.faceList[0].Centroid;
                        Vec3d perpLineStartA = RIntersection.LineClosestPoint2D(centroidBoundsA, inputLines[i].start, inputLines[i].end);

                        // see how many faces of the mesh are on the right and on the left of the vector
                        Vec3d cross = Vec3d.CrossProduct(inputLines[i].Direction, centroidBoundsA - perpLineStartA);
                        if (cross.Z >= 0)
                        {
                            nodeLeft.AddBounds(boundsTuple.Item1.faceList[0]);
                            nodeRight.AddBounds(boundsTuple.Item1.faceList[1]);

                        }
                        else
                        {
                            nodeLeft.AddBounds(boundsTuple.Item1.faceList[1]);
                            nodeRight.AddBounds(boundsTuple.Item1.faceList[0]);
                            //facesRight.Add(inputMesh.faceList[j]);
                            //areaRight += inputMesh.faceList[j].Area;



                            // something is wrong heree!!!!??
                            //nodeLeft.angle = Math.PI - angle;
                            //nodeRight.angle = Math.PI - angle;

                            //nodeLeft.area = areaRight;
                            //nodeRight.area = areaLeft;

                        }

                    }

                    dataNodeC.AddChild(nodeLeft);
                    dataNodeC.AddChild(nodeRight);
                    break;
                }

            }


            //return dataNodeC;
            //return new Tuple<DataNode>(dataNodeC);
        }
        public static DataNode updateNodeWithOneSubdivision(DataNode dataNodeC, NMesh inputMesh, NFace bounds)
        {

            // 02 Identify longest axis
            List<NLine> tempLines = NMesh.identifyLongestAxis(inputMesh);
            List<NLine> inputLines = NMesh.identifyLongestAxis(inputMesh);

            // 03 Split into two

            string treeNodeID = "roomSplit";
            TreeNode currentNode = new TreeNode(treeNodeID);

            double tolerance = 0.001;
            // go through lines
            bool worked = false;

            NMesh meshRight = inputMesh;
            NMesh meshLeft = inputMesh;
            double angle = Vec3d.Angle(inputLines[0].Direction, Vec3d.UnitX);
            NLine outLine = inputLines[0];

            for (int i = 0; i < inputLines.Count; i++)
            {
                outLine = inputLines[i];
                // if line ends are on bounds
                bool startOnFace = RIntersection.onNFaceEdge(inputLines[i].start, bounds);
                bool endOnFace = RIntersection.onNFaceEdge(inputLines[i].end, bounds);

                List<NFace> facesLeft = new List<NFace>();
                List<NFace> facesRight = new List<NFace>();

                double areaLeft = 0;
                double areaRight = 0;

                angle = Vec3d.Angle(inputLines[i].Direction, Vec3d.UnitX);

                if (startOnFace == true && endOnFace == true)
                {
                    for (int j = 0; j < inputMesh.faceList.Count; j++)
                    {
                        // closest point of centroid of face to line
                        Vec3d centroid = inputMesh.faceList[j].Centroid;
                        Vec3d perpLineStart = RIntersection.LineClosestPoint2D(centroid, inputLines[i].start, inputLines[i].end);

                        // see how many faces of the mesh are on the right and on the left of the vector
                        Vec3d cross = Vec3d.CrossProduct(inputLines[i].Direction, centroid - perpLineStart);
                        if (cross.Z > 0)
                        {
                            facesLeft.Add(inputMesh.faceList[j]);
                            areaLeft += inputMesh.faceList[j].Area;
                        }
                        else
                        {
                            facesRight.Add(inputMesh.faceList[j]);
                            areaRight += inputMesh.faceList[j].Area;
                        }
                    }
                }

                if (areaLeft > tolerance && areaRight > tolerance && facesLeft.Count > 0 && facesRight.Count > 0)
                {
                    worked = true;

                    meshLeft = new NMesh(facesLeft);
                    meshRight = new NMesh(facesRight);

                    string leftId = currentNode.ID + "L";
                    string rightId = currentNode.ID + "R";
                    TreeNode leftChild = new TreeNode(leftId);
                    TreeNode rightChild = new TreeNode(rightId);

                    leftChild.split = areaLeft;
                    leftChild.angle = angle;
                    rightChild.split = areaRight;
                    rightChild.angle = angle;

                    currentNode.Add(leftChild);
                    currentNode.Add(rightChild);

                    DataNode nodeLeft = new DataNode("leftNode", areaLeft, angle, false);
                    DataNode nodeRight = new DataNode("rightNode", areaRight, angle, false);
                    nodeLeft.treeNodeMesh = meshLeft;
                    nodeRight.treeNodeMesh = meshRight;
                    dataNodeC.AddChild(nodeLeft);
                    dataNodeC.AddChild(nodeRight);

                    break;
                }

            }

            //B = RhConvert.NLineListToRhLineCurveList(tempLines);
            //B = RhConvert.NMeshToRhLinePolylineList(meshLeft);
            //C = RhConvert.NMeshToRhLinePolylineList(meshRight);

            // 04 Extract
            return dataNodeC;
            //DataNode root = DataNode.convertTreeToNode(currentNode);
            //return new Tuple<bool, NMesh, NMesh>(worked, meshLeft, meshRight);
        }


        // SubdivisionSingleEnd
        public static NMesh assembleMesh(DataNode inputNode)
        {
            // deprecated use assemble mesh safe
            if (inputNode.children.Count == 0)
            {
                return inputNode.treeNodeMesh;
            }
            else
            {
                int i = 0;
                while (i < inputNode.children.Count)
                {
                    var child = inputNode.children[i];
                    if (child.treeNodeMesh != null)
                    {
                        NMesh tempMesh = assembleMesh(child);
                        //if (child.treeNodeMesh.faceList.Count == 0)
                        inputNode.treeNodeMesh.faceList.AddRange(tempMesh.faceList);
                    }
                    i++;
                }
                return inputNode.treeNodeMesh;
            }
        }

        public static NMesh assembleMeshSafe(DataNode inputNode, int iteration = 0)
        {
            if (iteration >= 100)
            {
                // Stop condition reached, return a default or error mesh
                return inputNode.treeNodeMesh; // Replace with your desired behaviornew NMesh()
            }

            if (inputNode.children.Count == 0)
            {
                return inputNode.treeNodeMesh;
            }
            else
            {
                int i = 0;
                while (i < inputNode.children.Count)
                {
                    var child = inputNode.children[i];
                    if (child.treeNodeMesh != null)
                    {
                        NMesh tempMesh = assembleMeshSafe(child, iteration + 1); // Pass the current iteration + 1 to the recursive call
                        inputNode.treeNodeMesh.faceList.AddRange(tempMesh.faceList);
                    }
                    i++;
                }
                return inputNode.treeNodeMesh;
            }
        }
        public static NMesh assembleMeshDict(DataNode inputNode)
        {
            if (inputNode.children.Count == 0)
            {
                return inputNode.treeNodeMesh;
            }
            else
            {
                Dictionary<NMesh, DataNode> processedMeshes = new Dictionary<NMesh, DataNode>();

                int i = 0;
                while (i < inputNode.children.Count)
                {
                    var child = inputNode.children[i];
                    if (child.treeNodeMesh != null && !processedMeshes.ContainsKey(child.treeNodeMesh))
                    {
                        NMesh tempMesh = assembleMeshDict(child);
                        inputNode.treeNodeMesh.faceList.AddRange(tempMesh.faceList);
                        processedMeshes.Add(child.treeNodeMesh, child);
                    }
                    i++;
                }
                return inputNode.treeNodeMesh;
            }
        }
        public static NMesh assembleMeshHash(DataNode inputNode)
        {
            if (inputNode.children.Count == 0)
            {
                return inputNode.treeNodeMesh;
            }
            else
            {
                HashSet<DataNode> processedNodes = new HashSet<DataNode>();
                HashSet<NFace> processedFaces = new HashSet<NFace>();

                int i = 0;
                while (i < inputNode.children.Count)
                {
                    var child = inputNode.children[i];
                    if (child.treeNodeMesh != null && !processedNodes.Contains(child))
                    {
                        NMesh tempMesh = assembleMeshHash(child);
                        foreach (var face in tempMesh.faceList)
                        {
                            if (!processedFaces.Contains(face))
                            {
                                inputNode.treeNodeMesh.faceList.Add(face);
                                processedFaces.Add(face);
                            }
                        }
                        processedNodes.Add(child);
                    }
                    i++;
                }
                return inputNode.treeNodeMesh;
            }
        }
        public static NMesh subdivideWithMesh(DataNode inputNode, NMesh inputMesh)
        {
            inputNode.AddMesh(inputMesh);

            DataNode.SubdivideWholeTree(inputNode, true);

            NMesh outMesh = assembleMesh(inputNode);

            return inputNode.treeNodeMesh;
        }
        public static NMesh subdivideWithMeshRatio(DataNode inputNode, NMesh inputMesh)
        {
            inputNode.AddMesh(inputMesh);

            DataNode.SubdivideWholeTree(inputNode, false);

            NMesh outMesh = DataNode.assembleMesh(inputNode);

            return inputNode.treeNodeMesh;
        }
        public static void SubdivideWholeTree(DataNode inputNode, bool subdivisionActual)
        {
            // set subdivision actual as true for actual
            // set subdivision actual to false for ratio subdivisions

            SubdivideTree(inputNode, subdivisionActual);

            foreach (var child in inputNode.children)
            {
                if (child.children.Count > 0)
                {
                    SubdivideWholeTree(child, subdivisionActual);
                }
            }
        }
        public static void SubdivideTree(DataNode inputNode, bool subdivisionActual)
        {
            NFace faceInput = inputNode.treeNodeMesh.faceList[0];
            inputNode.treeNodeMesh.faceList.RemoveAt(0);


            // print values of first children

            List<double> splitRatioList = new List<double>();
            List<double> splitAngleList = new List<double>();
            List<string> mergeIdList = new List<string>();
            List<string> uniqueIdList = new List<string>();
            List<List<string>> neighborIdList = new List<List<string>>();

            foreach (var child in inputNode.children)
            {
                // adds split ratios of all children to list
                double current_split = child.area;
                splitRatioList.Add(current_split);

                double current_angle = child.angle;
                splitAngleList.Add(current_angle);

                string current_merge = child.mergeid;
                mergeIdList.Add(current_merge);

                string current_name = child.name;
                uniqueIdList.Add(current_name);

                List<string> neighbor_list = child.connected;
                neighborIdList.Add(neighbor_list);
            }

            //Console.WriteLine("preparing .... ");


            splitRatioList.RemoveAt(splitRatioList.Count - 1);
            splitAngleList.RemoveAt(splitAngleList.Count - 1);

            Tuple<List<double>, NMesh> splitGroup_Tuple;

            if (subdivisionActual == true)
            {
                /// Added 180check
                faceInput.checkFor180Angle();
                faceInput.checkForZeroEdge();
                faceInput.flipRH();
                faceInput.checkFor180Angle();
                faceInput.checkForZeroEdge();
                faceInput.flipRH();

                splitGroup_Tuple = RSplit.SubdivideNFaceMultipleDirectionActual(faceInput, splitRatioList, splitAngleList);
            }
            else
            {
                /// Added 180check
                faceInput.checkFor180Angle();
                faceInput.checkForZeroEdge();
                faceInput.flipRH();
                faceInput.checkFor180Angle();
                faceInput.checkForZeroEdge();
                faceInput.flipRH();

                splitGroup_Tuple = RSplit.SubdivideNFaceMultipleDirection(faceInput, splitRatioList, splitAngleList);
            }

            int currentIter = 0;
            foreach (NFace face in splitGroup_Tuple.Item2.faceList)
            {
                face.updateEdgeConnectivity();

                /// Added 180check
                face.checkFor180Angle();
                face.checkForZeroEdge();
                face.flipRH();
                face.checkFor180Angle();
                face.checkForZeroEdge();
                face.flipRH();

                // add merge id to each face.
                face.merge_id = mergeIdList[currentIter];
                face.unique_id = uniqueIdList[currentIter];
                face.neighbors_id = neighborIdList[currentIter];
                currentIter++;

                //Console.WriteLine(face.edgeList.Count);
            }
            //Console.WriteLine("split");



            // assign the split meshes to the children of the input tree

            int iterator = 0;
            int numChildren = inputNode.children.Count;

            foreach (var child in inputNode.children)
            {
                // Add a mesh to each child
                child.treeNodeMesh = new NMesh(splitGroup_Tuple.Item2.faceList[0]);
                splitGroup_Tuple.Item2.faceList.RemoveAt(0);
                iterator++;

                // Add the rest of the faces to the last child
                if (iterator >= numChildren)
                    child.treeNodeMesh.faceList.AddRange(splitGroup_Tuple.Item2.faceList);

                //print mesh of child
                //Console.WriteLine(child.name);
                //Console.WriteLine(child.treeNodeMesh);
            }


            //Console.WriteLine(splitGroup_Tuple.Item2);
        }


        /// -----------------------------------------------------------------------------------------------

        public static Tuple<bool, NMesh> TrySubdivideWithMesh(DataNode inputNode, NMesh inputMesh)
        {
            // Deprecated use trySubdividewithmeshsafe
            NMesh outMesh = inputMesh.DeepCopyWithID();
            inputNode.AddMesh(inputMesh);

            DataNode.TrySubdivideWholeTree(inputNode, true);


            List<DataNode> currentchildren = DataNode.GetAllChildren(inputNode);
            List<bool> errorlist = currentchildren.Select(node => node._subdivisionerror).ToList();
            bool isAnyTrue = errorlist.Any(e => e);


            if (isAnyTrue == false)
            {
                outMesh = DataNode.assembleMesh(inputNode);
            }
            else
            {
                inputNode.treeNodeMesh = outMesh;
            }

            return new Tuple<bool, NMesh>(!isAnyTrue, inputNode.treeNodeMesh);
        }

        public static Tuple<bool, NMesh> TrySubdivideWithMeshSafe(DataNode inputNode, NMesh inputMesh)
        {
            NMesh outMesh = inputMesh.DeepCopyWithID();
            inputNode.AddMesh(inputMesh);

            DataNode.TrySubdivideWholeTree(inputNode, true);


            List<DataNode> currentchildren = DataNode.GetAllChildren(inputNode);
            List<bool> errorlist = currentchildren.Select(node => node._subdivisionerror).ToList();
            bool isAnyTrue = errorlist.Any(e => e);


            if (isAnyTrue == false)
            {
                outMesh = DataNode.assembleMeshSafe(inputNode);
            }
            else
            {
                inputNode.treeNodeMesh = outMesh;
            }

            return new Tuple<bool, NMesh>(!isAnyTrue, inputNode.treeNodeMesh);
        }

        /*
        public static NMesh TrysubdivideWithMesh(DataNode inputNode, NMesh inputMesh)
        {
            NMesh outMesh = inputMesh.DeepCopyWithID();

            inputNode.AddMesh(inputMesh);

            DataNode.TrySubdivideWholeTree(inputNode, true);

            if (inputNode._subdivisionerror == false)
            {
                outMesh = assembleMesh(inputNode);
            }
            else
            {
                inputNode.treeNodeMesh = outMesh;
            }

            return inputNode.treeNodeMesh;
        }
        */
        public static void TrySubdivideWholeTree(DataNode inputNode, bool subdivisionActual)
        {
            // set subdivision actual as true for actual
            // set subdivision actual to false for ratio subdivisions

            bool worked = TrySubdivideTree(inputNode, subdivisionActual);

            if (worked)
            {
                foreach (var child in inputNode.children)
                {
                    if (child.children.Count > 0)
                    {
                        TrySubdivideWholeTree(child, subdivisionActual);
                    }
                }
            }
            else
            {
                inputNode._subdivisionerror = true;
            }
            
        }
        public static bool TrySubdivideTree(DataNode inputNode, bool subdivisionActual)
        {
            try
            {
                NFace faceInput = inputNode.treeNodeMesh.faceList[0];
                inputNode.treeNodeMesh.faceList.RemoveAt(0);


                // print values of first children

                List<double> splitRatioList = new List<double>();
                List<double> splitAngleList = new List<double>();
                List<string> mergeIdList = new List<string>();
                List<string> uniqueIdList = new List<string>();
                List<List<string>> neighborIdList = new List<List<string>>();

                foreach (var child in inputNode.children)
                {
                    // adds split ratios of all children to list
                    double current_split = child.area;
                    splitRatioList.Add(current_split);

                    double current_angle = child.angle;
                    splitAngleList.Add(current_angle);

                    string current_merge = child.mergeid;
                    mergeIdList.Add(current_merge);

                    string current_name = child.name;
                    uniqueIdList.Add(current_name);

                    List<string> neighbor_list = child.connected;
                    neighborIdList.Add(neighbor_list);
                }

                //Console.WriteLine("preparing .... ");


                splitRatioList.RemoveAt(splitRatioList.Count - 1);
                splitAngleList.RemoveAt(splitAngleList.Count - 1);

                Tuple<List<double>, NMesh> splitGroup_Tuple;

                if (subdivisionActual == true)
                {
                    /// Added 180check
                    faceInput.checkFor180Angle();
                    faceInput.checkForZeroEdge();
                    faceInput.flipRH();
                    faceInput.checkFor180Angle();
                    faceInput.checkForZeroEdge();
                    faceInput.flipRH();

                    splitGroup_Tuple = RSplit.SubdivideNFaceMultipleDirectionActual(faceInput, splitRatioList, splitAngleList);
                }
                else
                {
                    /// Added 180check
                    faceInput.checkFor180Angle();
                    faceInput.checkForZeroEdge();
                    faceInput.flipRH();
                    faceInput.checkFor180Angle();
                    faceInput.checkForZeroEdge();
                    faceInput.flipRH();

                    splitGroup_Tuple = RSplit.SubdivideNFaceMultipleDirection(faceInput, splitRatioList, splitAngleList);
                }

                int currentIter = 0;
                foreach (NFace face in splitGroup_Tuple.Item2.faceList)
                {
                    face.updateEdgeConnectivity();

                    /// Added 180check
                    face.checkFor180Angle();
                    face.checkForZeroEdge();
                    face.flipRH();
                    face.checkFor180Angle();
                    face.checkForZeroEdge();
                    face.flipRH();

                    // add merge id to each face.
                    face.merge_id = mergeIdList[currentIter];
                    face.unique_id = uniqueIdList[currentIter];
                    face.neighbors_id = neighborIdList[currentIter];
                    currentIter++;

                    //Console.WriteLine(face.edgeList.Count);
                }
                //Console.WriteLine("split");



                // assign the split meshes to the children of the input tree

                int iterator = 0;
                int numChildren = inputNode.children.Count;

                foreach (var child in inputNode.children)
                {
                    // Add a mesh to each child
                    child.treeNodeMesh = new NMesh(splitGroup_Tuple.Item2.faceList[0]);
                    splitGroup_Tuple.Item2.faceList.RemoveAt(0);
                    iterator++;

                    // Add the rest of the faces to the last child
                    if (iterator >= numChildren)
                        child.treeNodeMesh.faceList.AddRange(splitGroup_Tuple.Item2.faceList);

                    //print mesh of child
                    //Console.WriteLine(child.name);
                    //Console.WriteLine(child.treeNodeMesh);
                }
                return true;
            }
            catch (Exception) //ArgumentOutOfRangeException
            { 
                inputNode._subdivisionerror= true;
                return false;
            }
        }

        /// ----------------------------------------------------------------------------------------------- 


        // Analysis

        public static MathNet.Numerics.LinearAlgebra.Vector<System.Numerics.Complex> returnComplex(Matrix<double> laplaceMatrix)
        {
            Evd<double> eigen = laplaceMatrix.Evd();
            MathNet.Numerics.LinearAlgebra.Vector<System.Numerics.Complex> eigenvector = eigen.EigenValues;
            return eigenvector;
        }

        public static List<double> ApplyTSNE_DifferentDims(List<Matrix<double>> matrixList, int perplexity = 30)
        {
            int numMatrices = matrixList.Count;

            // Determine the total number of elements across all matrices
            int totalElements = matrixList.Sum(matrix => matrix.RowCount * matrix.ColumnCount);

            // Flatten matrices into 1D vectors
            double[][] data = new double[totalElements][];
            int dataIndex = 0;
            for (int i = 0; i < numMatrices; i++)
            {
                Matrix<double> matrix = matrixList[i];
                int numRows = matrix.RowCount;
                int numCols = matrix.ColumnCount;

                for (int col = 0; col < numCols; col++)
                {
                    for (int row = 0; row < numRows; row++)
                    {
                        data[dataIndex] = new double[] { matrix[row, col] };
                        dataIndex++;
                    }
                }
            }

            // Perform t-SNE
            var tsne = new TSNE()
            {
                NumberOfOutputs = 1, // Set the number of dimensions for the projected data
                Perplexity = perplexity // Set the perplexity parameter (adjust as needed)
            };

            double[][] projectedData = tsne.Transform(data);

            // Convert the projected data to a List<double>
            List<double> projectedDataList = new List<double>(projectedData.Length);
            for (int i = 0; i < projectedData.Length; i++)
            {
                projectedDataList.Add(projectedData[i][0]);
            }

            // Return the projected data as a List<double>
            return projectedDataList;
        }
        public static List<double> ApplyTSNE(List<Matrix<double>> matrixList, int perplexity=30)
        {
            int numMatrices = matrixList.Count;

            // Flatten matrices into 1D vectors
            double[][] data = new double[numMatrices][];
            for (int i = 0; i < numMatrices; i++)
            {
                data[i] = MatrixToColumnWiseArray(matrixList[i]);
            }

            // Perform t-SNE
            var tsne = new TSNE()
            {
                NumberOfOutputs = 1, // Set the number of dimensions for the projected data
                Perplexity = perplexity // Set the perplexity parameter (adjust as needed)
            };

            double[][] projectedData = tsne.Transform(data);

            // Convert the projected data to a List<double>
            List<double> projectedDataList = new List<double>(projectedData.Length);
            for (int i = 0; i < projectedData.Length; i++)
            {
                projectedDataList.Add(projectedData[i][0]);
            }

            // Return the projected data as a List<double>
            return projectedDataList;
        }

        private static double[] MatrixToColumnWiseArray(Matrix<double> matrix)
        {
            int numRows = matrix.RowCount;
            int numCols = matrix.ColumnCount;
            double[] array = new double[numRows * numCols];

            int index = 0;
            for (int col = 0; col < numCols; col++)
            {
                for (int row = 0; row < numRows; row++)
                {
                    array[index++] = matrix[row, col];
                }
            }

            return array;
        }

        public static List<double> CompareMatricesWithPCA(List<Matrix<double>> matrixList)
        {
            int numMatrices = matrixList.Count;
            int matrixSize = matrixList[0].RowCount * matrixList[0].ColumnCount;

            // Flatten matrices into 1D vectors
            double[][] data = new double[numMatrices][];
            for (int i = 0; i < numMatrices; i++)
            {
                data[i] = RMath.MatrixToColumnWiseArray(matrixList[i]);
            }

            // Perform PCA
            var pca = new PrincipalComponentAnalysis()
            {
                Method = PrincipalComponentMethod.Center,
                Whiten = true
            };
            pca.Learn(data);

            // Project data onto principal components
            double[][] projectedData = pca.Transform(data);
            
            return projectedData[0].ToList();
            // Access singular values as an approximation of eigenvalues
            double[] singularValues = pca.SingularValues;

            // Access principal components
            double[,] principalComponents = pca.ComponentMatrix;

            // Use eigenvalues, projectedData, and principalComponents for further analysis or comparison
        }

        
        // Helper function to convert matrix to column-wise array
       

        public static Tuple<Matrix<double>, List<List<double>>> returnSubdivisionConnectivityTuple(DataNode inputNode)
        {
            // returns matrix with 1 where graph connects to other
            // #1 Matrix of number of children

            List<DataNode> tempAllChildren = DataNode.GetAllChildren(inputNode);
            tempAllChildren.Insert(0, inputNode);

            int dimensions = tempAllChildren.Count;
            Matrix<double> newMatrix = Matrix<double>.Build.Dense(dimensions, dimensions);

            // go through each column
            // go through each row
            // if item is child of column, make 1

            for (int i = 0; i < dimensions; i++)
            {
                List<int> childrenBool = new List<int>();

                string currentName = tempAllChildren[i].name;

                List<string> currentChildrenNames = new List<string>();

                for (int k = 0; k < tempAllChildren[i].children.Count; k++)
                {
                    currentChildrenNames.Add(tempAllChildren[i].children[k].name);
                }

                for (int j = 0; j < dimensions; j++)
                {
                    double value = 0;
                    if ((i != j))   //// added
                    {
                        // we know "name" and
                        if (currentChildrenNames.Contains(tempAllChildren[j].name))
                        {
                            value = 1; //tempAllChildren[j].area;
                        }
                    }
                    newMatrix[i, j] = value;
                }
            }


            List<List<double>> areaList = new List<List<double>>();

            for (int i = 0; i < newMatrix.ColumnCount; i++)
            {
                List<double> tempList = new List<double>();
                for (int j = 0; j < newMatrix.RowCount; j++)
                {
                    tempList.Add(newMatrix[i, j]);
                }
                areaList.Add(tempList);
            }


            // Degree Matrix, summed up rows


            return new Tuple<Matrix<double>, List<List<double>>>(newMatrix, areaList);
        }
        public static Tuple<Matrix<double>, List<List<double>>> returnAreaTuple(DataNode inputNode)
        {
            // #1 Matrix of number of children

            List<DataNode> tempAllChildren = DataNode.GetAllChildren(inputNode);
            tempAllChildren.Insert(0, inputNode);

            int dimensions = tempAllChildren.Count;
            Matrix<double> newMatrix = Matrix<double>.Build.Dense(dimensions, dimensions);

            // go through each column
            // go through each row
            // if item is child of column, make 1

            for (int i = 0; i < dimensions; i++)
            {
                List<int> childrenBool = new List<int>();

                string currentName = tempAllChildren[i].name;

                List<string> currentChildrenNames = new List<string>();

                for (int k = 0; k < tempAllChildren[i].children.Count; k++)
                {
                    currentChildrenNames.Add(tempAllChildren[i].children[k].name);
                }

                for (int j = 0; j < dimensions; j++)
                {
                    double value = 0;
                    if ((i != j))   //// added
                    {
                        // we know "name" and
                        if (currentChildrenNames.Contains(tempAllChildren[j].name))
                        {
                            value = tempAllChildren[j].area;
                        }
                    }
                    newMatrix[i, j] = value;
                }
            }


            List<List<double>> areaList = new List<List<double>>();

            for (int i = 0; i < newMatrix.ColumnCount; i++)
            {
                List<double> tempList = new List<double>();
                for (int j = 0; j < newMatrix.RowCount; j++)
                {
                    tempList.Add(newMatrix[i, j]);
                }
                areaList.Add(tempList);
            }

            return new Tuple<Matrix<double>, List<List<double>>>(newMatrix, areaList);
        }
        public static Tuple<Matrix<double>, List<List<double>>> returnAngleTuple(DataNode inputNode)
        {
            // #1 Matrix of number of children

            List<DataNode> tempAllChildren = DataNode.GetAllChildren(inputNode);
            tempAllChildren.Insert(0, inputNode);

            int dimensions = tempAllChildren.Count;
            Matrix<double> newMatrix = Matrix<double>.Build.Dense(dimensions, dimensions);

            // go through each column
            // go through each row
            // if item is child of column, make 1

            for (int i = 0; i < dimensions; i++)
            {
                List<int> childrenBool = new List<int>();

                string currentName = tempAllChildren[i].name;

                List<string> currentChildrenNames = new List<string>();

                for (int k = 0; k < tempAllChildren[i].children.Count; k++)
                {
                    currentChildrenNames.Add(tempAllChildren[i].children[k].name);
                }

                for (int j = 0; j < dimensions; j++)
                {
                    double value = 0;
                    if ((i != j))   //// added
                    {
                        // we know "name" and
                        if (currentChildrenNames.Contains(tempAllChildren[j].name))
                        {
                            value = tempAllChildren[j].angle;
                            if (value == 0)
                                value = Math.PI * 2;
                        }
                    }
                    newMatrix[i, j] = value;
                }
            }


            List<List<double>> angleList = new List<List<double>>();

            for (int i = 0; i < newMatrix.ColumnCount; i++)
            {
                List<double> tempList = new List<double>();
                for (int j = 0; j < newMatrix.RowCount; j++)
                {
                    tempList.Add(newMatrix[i, j]);
                }
                angleList.Add(tempList);
            }

            return new Tuple<Matrix<double>, List<List<double>>>(newMatrix, angleList);
        }



        //JSON conversion

        public static string serializeDataNode(TreeNode inputTree)
        {
            string jsonString = JsonSerializer.Serialize(inputTree);
            return jsonString;
        }
        public static string serializeDataNode(DataNode inputTree)
        {
            string jsonString = JsonSerializer.Serialize(inputTree);
            return jsonString;
        }
        public static DataNode deserializeDataNode(string jsonString)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                IncludeFields = true
            };

            DataNode reverseNode = JsonSerializer.Deserialize<DataNode>(jsonString, options);
            return reverseNode;
        }


    }
}
