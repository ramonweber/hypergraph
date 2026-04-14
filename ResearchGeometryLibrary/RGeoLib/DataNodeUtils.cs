using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGeoLib
{
    /// <summary>
    /// Utility functions for working with DataNode using GeometryUtils.
    /// This is a standalone class that can be compiled separately.
    /// </summary>
    public static class DataNodeUtils
    {
        /// <summary>
        /// Modified version of DataNode.updateOneNode that uses GeometryUtils functions.
        /// After identifying longest axis, expands lines to face boundaries and uses
        /// vertex-side checking to determine if lines cross faces.
        /// </summary>
        public static void UpdateOneNodeWithGeometryUtils(DataNode dataNodeC)
        {
            NMesh inputMesh = dataNodeC.returnMesh();
            NFace bounds = dataNodeC.returnBounds();

            // 02 Identify longest axis - using modified version
            List<NLine> tempLines = IdentifyLongestAxisModified(inputMesh);
            List<NLine> inputLines = IdentifyLongestAxisModified(inputMesh);

            // Expand lines to face boundaries using GeometryUtils
            for (int i = 0; i < inputLines.Count; i++)
            {
                NLine expandedLine = GeometryUtils.ExpandLineToNFace(inputLines[i], bounds);
                inputLines[i] = expandedLine;
            }

            // 03 Split into two
            string treeNodeID = "roomSplit";
            double tolerance = 0.001;
            bool worked = false;

            NMesh meshRight = inputMesh;
            NMesh meshLeft = inputMesh;
            double angle = Vec3d.Angle(inputLines[0].Direction, Vec3d.UnitX);
            NLine outLine = inputLines[0];

            for (int i = 0; i < inputLines.Count; i++)
            {
                // Step 1: Calculate directional angle [0, 2π]
                double directionalAngle = Math.Atan2(inputLines[i].Direction.Y, inputLines[i].Direction.X);
                if (directionalAngle < 0) 
                    directionalAngle += 2 * Math.PI;

                // Step 2: Check if angle is near π or 3π/2, if so flip the line
                NLine workingLine = inputLines[i];
                double angleThreshold = 0.2; // Threshold for considering angles "near" π or 3π/2
                
                bool nearPI = Math.Abs(directionalAngle - Math.PI) < angleThreshold;
                bool near3PI2 = Math.Abs(directionalAngle - 3 * Math.PI / 2) < angleThreshold;
                
                if (nearPI || near3PI2)
                {
                    // Flip the line by swapping start and end
                    workingLine = new NLine(inputLines[i].end, inputLines[i].start);
                }

                // Step 3: Calculate undirectional angle [0, π] for storage in nodes
                angle = Vec3d.Angle(workingLine.Direction, Vec3d.UnitX);
                
                outLine = workingLine;

                List<NFace> facesLeft = new List<NFace>();
                List<NFace> facesRight = new List<NFace>();

                double areaLeft = 0;
                double areaRight = 0;

                // Check each face in the mesh
                for (int j = 0; j < inputMesh.faceList.Count; j++)
                {
                    // Use GeometryUtils to check if line crosses this face
                    bool lineCrossesFace = GeometryUtils.DoesLineCrossFaceByVertexSides(workingLine, inputMesh.faceList[j]);
                    
                    if (lineCrossesFace)
                    {
                        continue; // Skip faces that the line crosses through
                    }

                    // Determine which side of the line this face is on
                    Vec3d centroid = inputMesh.faceList[j].Centroid;
                    Vec3d perpLineStart = RIntersection.LineClosestPoint2D(centroid, workingLine.start, workingLine.end);

                    Vec3d cross = Vec3d.CrossProduct(workingLine.Direction, centroid - perpLineStart);
                    
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

                // Validate split: all faces must be assigned (none crossed by the line)
                int totalFacesAssigned = facesLeft.Count + facesRight.Count;
                bool allFacesAssigned = (totalFacesAssigned == inputMesh.faceList.Count);
                
                if (allFacesAssigned && areaLeft > tolerance && areaRight > tolerance && facesLeft.Count > 0 && facesRight.Count > 0)
                {
                    // output two meshes
                    worked = true;

                    meshLeft = new NMesh(facesLeft);
                    meshRight = new NMesh(facesRight);

                    string leftId = dataNodeC.name + "L";
                    string rightId = dataNodeC.name + "R";

                    // angle already contains the undirectional angle [0, π]
                    // Round angle to common values if near them
                    double angleThresholdForRounding = 0.1; // ~5.7 degrees
                    double roundedAngle = angle;
                    
                    if (Math.Abs(angle) < angleThresholdForRounding)
                    {
                        roundedAngle = 0; // Near 0
                    }
                    else if (Math.Abs(angle - Math.PI / 2) < angleThresholdForRounding)
                    {
                        roundedAngle = Math.PI / 2; // Near π/2
                    }
                    else if (Math.Abs(angle - Math.PI) < angleThresholdForRounding)
                    {
                        roundedAngle = Math.PI; // Near π
                    }
                    
                    DataNode nodeLeft = new DataNode(leftId, areaLeft, roundedAngle, false);
                    DataNode nodeRight = new DataNode(rightId, areaRight, roundedAngle, false);

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
                    Tuple<NMesh, bool> boundsTuple = RSplit.divideNFaceWithNLine(bounds, workingLine);
                    if (boundsTuple.Item2)
                    {
                        // closest point of centroid of face to line
                        Vec3d centroidBoundsA = boundsTuple.Item1.faceList[0].Centroid;
                        Vec3d perpLineStartA = RIntersection.LineClosestPoint2D(centroidBoundsA, workingLine.start, workingLine.end);

                        Vec3d cross = Vec3d.CrossProduct(workingLine.Direction, centroidBoundsA - perpLineStartA);
                        if (cross.Z >= 0)
                        {
                            nodeLeft.AddBounds(boundsTuple.Item1.faceList[0]);
                            nodeRight.AddBounds(boundsTuple.Item1.faceList[1]);
                        }
                        else
                        {
                            nodeLeft.AddBounds(boundsTuple.Item1.faceList[1]);
                            nodeRight.AddBounds(boundsTuple.Item1.faceList[0]);
                        }
                    }

                    dataNodeC.AddChild(nodeLeft);
                    dataNodeC.AddChild(nodeRight);
                    break;
                }
            }
        }

        /// <summary>
        /// Modified version of DataNode.updateAllNodes that uses the GeometryUtils version.
        /// </summary>
        public static DataNode UpdateAllNodesWithGeometryUtils(DataNode inputNode)
        {
            UpdateOneNodeWithGeometryUtils(inputNode);

            foreach (var child in inputNode.children)
            {
                NMesh tempMesh = child.returnMesh();
                if (tempMesh.faceList.Count > 1)
                {
                    UpdateAllNodesWithGeometryUtils(child);
                }
            }
            return inputNode;
        }

        /// <summary>
        /// Modified version of DataNode.dataNodeFromNMeshAndBounds that uses GeometryUtils functions.
        /// Uses UpdateOneNodeWithGeometryUtils instead of updateOneNode.
        /// </summary>
        public static DataNode CreateDataNodeWithGeometryUtils(NMesh inputMesh, NFace bounds)
        {
            // Calculate root area from sum of actual mesh faces
            double totalMeshArea = inputMesh.faceList.Sum(face => face.Area);
            
            DataNode rootNode = new DataNode("root", totalMeshArea, 0, false);
            rootNode.AddMesh(inputMesh);
            rootNode.AddBounds(bounds);

            // Use modified update function
            UpdateAllNodesWithGeometryUtils(rootNode);

            // Use modified version of identifyLongestAxis
            List<NLine> tempLines = IdentifyLongestAxisModified(inputMesh);

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
                }
            }
            
            return rootNode;
        }

        /// <summary>
        /// Modified version of NMesh.identifyLongestAxis.
        /// Changed condition from (currentLines.Count > 1) to (currentLines.Count > 0)
        /// to include axis with single lines as well.
        /// Handles single-line cases by using the line directly.
        /// </summary>
        public static List<NLine> IdentifyLongestAxisModified(NMesh inputMesh)
        {
            // Returns list of lines axis inside poly sorted by length
            inputMesh.UpdateAxis(0.05);
            List<NLine> combinedLines = new List<NLine>();

            for (int i = 0; i < inputMesh.axisList.Count; i++)
            {
                List<NLine> currentLines = inputMesh.axisList[i].incNLines;
                
                if (currentLines.Count == 1)
                {
                    // For single line, use it directly
                    combinedLines.Add(currentLines[0]);
                }
                else if (currentLines.Count > 1)
                {
                    // For multiple lines, create a polyline and extract the straight line
                    NPolyLine tempLine = new NPolyLine(currentLines);
                    NLine lineout = NPolyLine.lineFromStraightPoly(tempLine);
                    combinedLines.Add(lineout);
                }
            }

            // Sort lines by length
            var precision = 0.001;
            List<NLine> sortedLines = combinedLines.OrderBy(p => Math.Round(p.Length / precision)).ToList();
            sortedLines.Reverse();

            return sortedLines;
        }

        /// <summary>
        /// Recursively converts all node areas in the tree to ratios relative to their parent.
        /// After conversion, each node's area represents the fraction of its parent's area (0.0 to 1.0).
        /// The root node's area is set to 1.0 (representing 100% of the total).
        /// </summary>
        /// <param name="node">The node to process (typically the root)</param>
        private static void ConvertTreeAreasToRatios(DataNode node)
        {
            double parentArea = node.area;
            
            foreach (var child in node.children)
            {
                // First recursively process children (depth-first) while they still have actual areas
                ConvertTreeAreasToRatios(child);
                
                // Then convert this child's area to ratio relative to parent
                if (parentArea > 0)
                {
                    child.area = child.area / parentArea;
                }
            }
            
            // Set root's area to 1.0 (it represents 100% of itself)
            if (node.name == "root")
            {
                node.area = 1.0;
            }
        }

        /// <summary>
        /// Creates a DataNode from NMesh and bounds with pre-known connectivity information.
        /// Unlike dataNodeFromNMeshAndBoundsWithConnectivity which computes connectivity geometrically,
        /// this method accepts a dictionary mapping room merge_ids to their connected neighbors.
        /// Converts merge_id connections to node name connections for proper graph visualization.
        /// Areas are stored as ratios relative to parent (0.0 to 1.0), not actual values.
        /// </summary>
        /// <param name="inputMesh">The input mesh containing all room faces</param>
        /// <param name="bounds">The boundary face</param>
        /// <param name="connectivityDict">Dictionary mapping merge_id to list of connected merge_ids</param>
        /// <returns>DataNode with connectivity information populated and areas as ratios</returns>
        public static DataNode CreateDataNodeWithConnectivity(NMesh inputMesh, NFace bounds, Dictionary<string, List<string>> connectivityDict)
        {
            // Calculate root area from sum of actual mesh faces
            double totalMeshArea = inputMesh.faceList.Sum(face => face.Area);
            
            DataNode rootNode = new DataNode("root", totalMeshArea, 0, false);
            rootNode.AddMesh(inputMesh);
            rootNode.AddBounds(bounds);

            // Use modified update function to build the tree structure
            UpdateAllNodesWithGeometryUtils(rootNode);

            // Convert all areas to ratios (relative to parent)
            // Each child's area becomes a fraction of its parent's area
            ConvertTreeAreasToRatios(rootNode);

            List<DataNode> allNodes = DataNode.GetAllChildren(rootNode);

            List<int> childList = new List<int>();
            List<NMesh> outMeshList = new List<NMesh>();
            List<NFace> boundsList = new List<NFace>();

            // First pass: build mapping from merge_id to node names
            Dictionary<string, List<string>> mergeIdToNodeNames = new Dictionary<string, List<string>>();
            
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

                    // Build merge_id to node name mapping
                    string nodeMergeId = allNodes[i].mergeid;
                    if (!mergeIdToNodeNames.ContainsKey(nodeMergeId))
                    {
                        mergeIdToNodeNames[nodeMergeId] = new List<string>();
                    }
                    mergeIdToNodeNames[nodeMergeId].Add(allNodes[i].name);
                }
            }

            // Second pass: assign connectivity using node names instead of merge_ids
            for (int i = 0; i < allNodes.Count; i++)
            {
                if (allNodes[i].final)
                {
                    string nodeMergeId = allNodes[i].mergeid;
                    List<string> connectedNodeNames = new List<string>();

                    if (connectivityDict.ContainsKey(nodeMergeId))
                    {
                        // Get the list of connected merge_ids
                        List<string> connectedMergeIds = connectivityDict[nodeMergeId];
                        
                        // Convert each connected merge_id to actual node names
                        foreach (string connectedMergeId in connectedMergeIds)
                        {
                            if (mergeIdToNodeNames.ContainsKey(connectedMergeId))
                            {
                                // Add all node names that have this merge_id
                                connectedNodeNames.AddRange(mergeIdToNodeNames[connectedMergeId]);
                            }
                        }
                    }

                    allNodes[i].connected = connectedNodeNames;
                }
            }
            
            return rootNode;
        }

        /// <summary>
        /// Creates a DataNode with connectivity using unique_id instead of merge_id.
        /// This variant uses the name of the node for connectivity mapping.
        /// </summary>
        /// <param name="inputMesh">The input mesh containing all room faces</param>
        /// <param name="bounds">The boundary face</param>
        /// <param name="connectivityDict">Dictionary mapping node names/unique_ids to list of connected names</param>
        /// <returns>DataNode with connectivity information populated</returns>
        public static DataNode CreateDataNodeWithConnectivityByName(NMesh inputMesh, NFace bounds, Dictionary<string, List<string>> connectivityDict)
        {
            // Calculate root area from sum of actual mesh faces
            double totalMeshArea = inputMesh.faceList.Sum(face => face.Area);
            
            DataNode rootNode = new DataNode("root", totalMeshArea, 0, false);
            rootNode.AddMesh(inputMesh);
            rootNode.AddBounds(bounds);

            // Use modified update function to build the tree structure
            UpdateAllNodesWithGeometryUtils(rootNode);

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

                    // Assign connectivity using the node name
                    string nodeName = allNodes[i].name;
                    if (connectivityDict.ContainsKey(nodeName))
                    {
                        allNodes[i].connected = connectivityDict[nodeName];
                    }
                    else
                    {
                        // Initialize empty list if no connectivity info found
                        allNodes[i].connected = new List<string>();
                    }
                }
            }
            
            return rootNode;
        }
    }
}
