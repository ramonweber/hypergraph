using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using NPOI.POIFS.Storage;

namespace RGeoLib
{
    public class NMesh
    {
        public List<NFace> faceList;
        public List<Axis> axisList;

        public Matrix<double> adjacencyMatrix;
        public List<List<int>> adjacencyList;


        //public Dictionary<int, HashSet<int>> adjacencyList = new Dictionary<int, HashSet<int>>();

        // Create new mesh from face list
        public NMesh()
        {
        }
        public NMesh(List<NFace> faces)
        {
            this.faceList = faces;
        }

        // Create new mesh from one face
        public NMesh(NFace faceTemp)
        {
            List<NFace> tempList = new List<NFace>();
            tempList.Add(faceTemp);
            this.faceList = tempList;
        }

        public double Area
        {
            get
            {
                double areaTotalMesh = 0;
                for (int i = 0; i < this.faceList.Count; i++)
                {
                    areaTotalMesh += this.faceList[i].Area;
                }
                return areaTotalMesh;

            }
        }

        public Vec3d Centroid
        {// this is wrong, just an approximation, implement Integral for new. 
            get
            {
                Vec3d tempCentroid = new Vec3d(0, 0, 0);
                for (int i = 0; i < this.faceList.Count; i++)
                {
                    tempCentroid += this.faceList[i].Centroid;
                }
                tempCentroid /= (this.faceList.Count);
                return tempCentroid;
            }
        }

        public NMesh DeepCopyWithID()
        {
            List<NFace> copyFacesList = new List<NFace>();
            foreach (NFace face in this.faceList)
            {
                NFace deepCopyFace = face.DeepCopyWithMID();
                copyFacesList.Add(deepCopyFace);
            }
            NMesh deepCopyMesh = new NMesh(copyFacesList);

            return deepCopyMesh;
        }
        public void cleanMesh()
        {
            foreach (NFace tempFace in this.faceList)
            {
                tempFace.cleanFace();
            }
        }

        public void initializeAdjacencyMatrix()
        {
            // Initializes Empty!!

            // sees if the object exists... if yes ... 
            if (Object.Equals(adjacencyMatrix, default(Matrix<double>)))
            {
                Matrix<double> adjMatrix = Matrix<double>.Build.Dense(this.faceList.Count, this.faceList.Count);
                this.adjacencyMatrix = adjMatrix;
            }
                
        }
        public void UpdateAdjacencyMatrix()
        {
            // Build matrix with xy num faces
            int numFaces = this.faceList.Count;
            Matrix<double> adjMatrix = Matrix<double>.Build.Dense(numFaces, numFaces);

            for (int i = 0; i < numFaces; i++)
            {
                for (int j = 0; j < numFaces; j++)
                {
                    if ((i != j))   //// added
                    {
                        bool adj = RIntersection.intersectNFacesBool(this.faceList[i], this.faceList[j]);
                        if (adj == true)
                        {
                            // update the ones that are adjacent to 1
                            adjMatrix[i, j] = 1;
                            this.faceList[i].neighborNFace.Add(this.faceList[j]);
                        }
                    }
                }
            }

            this.adjacencyMatrix = adjMatrix;
        }
        public void UpdateAdjacencyList()
        {
            UpdateAdjacencyMatrix();

            Matrix<double> m = this.adjacencyMatrix;

            List<List<int>> adjList = new List<List<int>>();

            for (int i = 0; i < m.ColumnCount; i++)
            {
                List<int> tempList = new List<int>();
                for (int j = 0; j < m.RowCount; j++)
                {
                    if ((i != j) && (m[i, j] == 1))
                    {
                        tempList.Add(j);
                    }
                }
                adjList.Add(tempList);
            }

            this.adjacencyList = adjList;
        }

        public void UpdateAdjacencyListFromMatrix()
        {
            // Access the adjacency matrix
            Matrix<double> matrix = this.adjacencyMatrix;

            // Initialize the adjacency list
            List<List<int>> newAdjList = new List<List<int>>();

            // Iterate over each row in the matrix
            for (int i = 0; i < matrix.RowCount; i++)
            {
                // Create a new list for the current row's adjacency
                List<int> tempList = new List<int>();

                // Iterate over each column in the row
                for (int j = 0; j < matrix.ColumnCount; j++)
                {
                    // Check if the current element indicates adjacency and is not the same node
                    if (matrix[i, j] == 1 && i != j)
                    {
                        // Add the adjacent node's index to the list
                        tempList.Add(j);
                    }
                }

                // Add the current row's adjacency list to the main list
                newAdjList.Add(tempList);
            }

            // Assign the generated adjacency list to the class variable
            this.adjacencyList = newAdjList;
        }


        public static List<Vec3d> getMeshPoints(NMesh inputMesh)
        {
            List<Vec3d> ptList = new List<Vec3d>();
            for (int i = 0; i < inputMesh.faceList.Count; i++)
            {
                List<Vec3d> tempPt = inputMesh.faceList[i].getPoints();
                ptList.AddRange(tempPt);
            }
            ptList = Vec3d.RemoveDuplicatePoints(ptList, 0.01);
            return ptList;
        }

        // Adjacency with Constraints
        public void UpdateAdjacencyMatrixWithConnectors(List<Vec3d> inputVecs)
        {
            // Builds adjacency matrix, but only at faces that both touch a point

            // Build matrix with xy num faces
            int numFaces = this.faceList.Count;
            Matrix<double> adjMatrix = Matrix<double>.Build.Dense(numFaces, numFaces);

            for (int i = 0; i < numFaces; i++)
            {
                for (int j = 0; j < numFaces; j++)
                {
                    if ((i != j))   //// added
                    {
                        bool adj = RIntersection.intersectNFacesBool(this.faceList[i], this.faceList[j]);
                        if (adj == true)
                        {
                            // iterate through all points, check if one of them is on the edge of both faces
                            bool vecConnects = false;
                            // if yes Adjacent
                            // if no not adjacent
                            for (int k = 0; k < inputVecs.Count; k++)
                            {
                                bool onFirst = RIntersection.onNFaceEdge(inputVecs[k], this.faceList[i]);
                                bool onSecond = RIntersection.onNFaceEdge(inputVecs[k], this.faceList[j]);

                                if (onFirst == true && onSecond == true)
                                    vecConnects = true;

                            }

                            if (vecConnects == true)
                            {
                                // update the ones that are adjacent to 1
                                adjMatrix[i, j] = 1;
                                this.faceList[i].neighborNFace.Add(this.faceList[j]);
                            }
                        }
                    }
                }
            }

            this.adjacencyMatrix = adjMatrix;
        }
        public void UpdateAdjacencyListWithConnectors(List<Vec3d> inputVecs)
        {
            UpdateAdjacencyMatrixWithConnectors(inputVecs);

            Matrix<double> m = this.adjacencyMatrix;

            List<List<int>> adjList = new List<List<int>>();

            for (int i = 0; i < m.ColumnCount; i++)
            {
                List<int> tempList = new List<int>();
                for (int j = 0; j < m.RowCount; j++)
                {
                    if ((i != j) && (m[i, j] == 1))
                    {
                        tempList.Add(j);
                    }
                }
                adjList.Add(tempList);
            }

            this.adjacencyList = adjList;
        }

        // Adjacency with strings 
        // use when strings were defined from a splitting operation etc. ... 

        public void UpdateAdjacencyMatrixWithFaceStrings()
        {
            // Builds adjacency matrix, but only at faces that are connected by "string neighbor name" 

            // Build matrix with xy num faces
            int numFaces = this.faceList.Count;
            Matrix<double> adjMatrix = Matrix<double>.Build.Dense(numFaces, numFaces);

            for (int i = 0; i < numFaces; i++)
            {
                for (int j = 0; j < numFaces; j++)
                {
                    if ((i != j))   //// added
                    {
                        // check if string is in string list of neighbors
                        bool vecConnects = this.faceList[i].neighbors_id.Contains(this.faceList[j].unique_id);

                        if (vecConnects == true)
                        {
                            // update the ones that are adjacent to 1
                            adjMatrix[i, j] = 1;
                            this.faceList[i].neighborNFace.Add(this.faceList[j]);
                        }
                    }
                }
            }

            this.adjacencyMatrix = adjMatrix;

        }
        public void UpdateAdjacencyListWithFaceStrings()
        {
            UpdateAdjacencyMatrixWithFaceStrings();

            Matrix<double> m = this.adjacencyMatrix;

            List<List<int>> adjList = new List<List<int>>();

            for (int i = 0; i < m.ColumnCount; i++)
            {
                List<int> tempList = new List<int>();
                for (int j = 0; j < m.RowCount; j++)
                {
                    if ((i != j) && (m[i, j] == 1))
                    {
                        tempList.Add(j);
                    }
                }
                adjList.Add(tempList);
            }

            this.adjacencyList = adjList;
        }

        // Sort and shift methods

        public NMesh filterNMeshByProperty(string propertyString)
        { 
            List<NFace> tempFaces = new List<NFace>();

            for (int i = 0; i < this.faceList.Count; i++)
            {
               if (this.faceList[i].merge_id == propertyString)
                    tempFaces.Add(this.faceList[i]);
            }

            NMesh tempMesh = new NMesh(tempFaces);
            return tempMesh;
        }
        public NMesh filterNMeshByProperty(List<string> propertyList)
        {
            List<NFace> tempFaces = new List<NFace>();

            for (int i = 0; i < this.faceList.Count; i++)
            {
                bool faceFilter = false;
                for (int j = 0; j < propertyList.Count; j++)
                {
                    if (this.faceList[i].merge_id == propertyList[j])
                        faceFilter= true;
                }
                if (faceFilter == true)
                    tempFaces.Add(this.faceList[i]);
            }

            NMesh tempMesh = new NMesh(tempFaces);
            return tempMesh;
        }
        public void sortFacesByAngle(double angleRad)
        {
            // sorts all faces of mesh perpendicular to the angle angleRad

            Vec3d sortDirTemp = Vec3d.UnitX;

            Vec3d sortDir = Vec3d.RotateAroundAxisRad(sortDirTemp, Vec3d.UnitZ, angleRad + (Math.PI / 2));

            List<NFace> sortedFaces = this.faceList.OrderBy(x => Vec3d.DotProduct(x.Centroid, sortDir)).ToList();
            this.faceList = sortedFaces;
        }
        public void shiftFacesLeft()
        {
            // moves list 1 forward, last edge start edge

            List<NFace> tempFaces = new List<NFace>();

            for (int i = 1; i < (this.faceList.Count); i++)
            {
                tempFaces.Add(this.faceList[i]);
            }
            tempFaces.Add(this.faceList[0]);

            // Replace edglist and update connectivity
            this.faceList = tempFaces;
        }
        public void shiftFacesByInt(int shifter)
        {
            for (int i = 0; i < shifter; i++)
            {
                shiftFacesLeft();
            }
        }

        // Merge Methods
        public void mergeNMeshVert(double mergeTol = 0.001)
        {

            // terrible implementation o^500000 only use for tiny meshes

            List<Vec3d> existingVerts = new List<Vec3d>();

            // add all mesh vertices to existing list
            int numFaces = this.faceList.Count;

            for (int i = 0; i < numFaces; i++)
            {
                for (int j = 0; j < this.faceList[i].edgeList.Count; j++)
                {
                    existingVerts.Add(this.faceList[i].edgeList[j].v);
                }
            }

            for (int i = 0; i < numFaces; i++)
            {
                for (int j = 0; j < this.faceList[i].edgeList.Count; j++)
                {
                    for (int u = 0; u < existingVerts.Count; u++)
                    {
                        double tempDist = Vec3d.Distance(existingVerts[u], this.faceList[i].edgeList[j].v);
                        if (tempDist < mergeTol)
                        {
                            this.faceList[i].edgeList[j].v = existingVerts[u];
                        }
                    }
                }
            }
        }
        public static NMesh mergeNMeshBySameProperty(NMesh inputMesh)
        {
            // Merges two NFaces next to each other

            //         B------B             A------A
            //         |      |             |      |
            //    A----A      |        A----A      |
            //    |    |      |    --> |           |
            //    | r1 |  r1  |        |      r1   |
            //    A----A      |        A----A      |
            //         |      |             |      |
            //         B------B             A------A

            List<string> mergeStrings = new List<string>();

            // go through each face
            // get list of property strings if stringlength > 0
            for (int i = 0; i < inputMesh.faceList.Count; i++)
            {
                string tempId = inputMesh.faceList[i].merge_id;

                // check if already in lsit
                bool isalreadythere = mergeStrings.Contains(tempId);
                if (tempId.Length > 0 && isalreadythere == false)
                    mergeStrings.Add(tempId);
            }

            // for each stringg in stringlist
            for (int i = 0; i < mergeStrings.Count; i++)
            {
                // do merge by property
                inputMesh = mergeNMeshByProperty(inputMesh, mergeStrings[i]);
            }

            return inputMesh;
        }
        public static NMesh mergeNMeshByProperty(NMesh inputMesh, string mergeString)
        {
            // Merges two NFaces next to each other

            //         B------B             A------A
            //         |      |             |      |
            //    A----A      |        A----A      |
            //    |    |      |    --> |           |
            //    |    |      |        |           |
            //    A----A      |        A----A      |
            //         |      |             |      |
            //         B------B             A------A

            List<NFace> facesExisting = new List<NFace>();
            List<NFace> facesToMerge = new List<NFace>();

            for (int i = 0; i < inputMesh.faceList.Count; i++)
            {
                if (inputMesh.faceList[i].merge_id == mergeString)
                    facesToMerge.Add(inputMesh.faceList[i]);
                else
                    facesExisting.Add(inputMesh.faceList[i]);
            }

            NMesh tempMesh = new NMesh(facesToMerge);

            NMesh mergedTempMesh = RSplit.mergeNMesh(tempMesh);

            facesExisting.AddRange(mergedTempMesh.faceList);

            List<NFace> outFaces = new List<NFace>();
            for (int i = 0; i < facesExisting.Count; i++)
            {
                NFace tempFace = NFace.checkInternalDuplicateEdge(facesExisting[i]);
                outFaces.Add(tempFace);
            }

            NMesh outMesh = new NMesh(outFaces);

            return outMesh;
        }
        public static NMesh mergeOnce(NMesh aptBase, List<bool> aptValid)
        {
            aptBase.UpdateAdjacencyMatrix();

            List<int> mergedInts = new List<int>();

            int current = 0;

            List<NFace> merged = new List<NFace>();
            List<NFace> single = new List<NFace>();

            for (int i = 0; i < aptBase.faceList.Count; i++)
            {
                //check if apt is too small, e.g. needs merge check
                if (aptValid[i] == false)
                {
                    // if not valid, perform merge check
                    for (int j = 0; j < aptBase.faceList.Count; j++)
                    {
                        if (mergedInts.Contains(i) == false && mergedInts.Contains(j) == false && aptBase.adjacencyMatrix[i, j] == 1)
                        {

                            //aptBase.faceList[i].shrinkMax();
                            //aptBase.faceList[j].shrinkMax();

                            List<NFace> toMerge = new List<NFace>();
                            toMerge.Add(aptBase.faceList[i]);
                            toMerge.Add(aptBase.faceList[j]);

                            NMesh tempMesh = new NMesh(toMerge);
                            NMesh mergedMesh = RSplit.mergeNMesh(tempMesh);

                            if (mergedMesh.faceList.Count == 1)
                            {
                                NFace mergedFace = mergedMesh.faceList[0];
                                // if merged mesh has only one face the merge was successful and we add the faces
                                // to the list of merged ones
                                mergedInts.Add(i);
                                mergedInts.Add(j);

                                mergedFace.shrinkMax();
                                mergedFace.updateEdgeConnectivity();

                                merged.Add(mergedFace);
                            }
                        }
                    }
                }

            }

            // check if some got forgotten

            for (int i = 0; i < aptBase.faceList.Count; i++)
            {
                if (mergedInts.Contains(i) == false)
                {
                    single.Add(aptBase.faceList[i]);
                }
            }

            merged.AddRange(single);
            NMesh outMesh = new NMesh(merged);


            return outMesh;
        }

        // Translation Rotation Scale
        public void translate(Vec3d moveVec)
        {
            for (int i = 0; i < this.faceList.Count; i++)
            {
                this.faceList[i].TranslateNFace(moveVec);
            }
        }
        public void rotateAroundAxisRad(Vec3d axis, double angle)
        {
            for (int i = 0; i < this.faceList.Count; i++)
            {
                this.faceList[i].RotateNFaceAroundAxisRad(axis, angle);
            }
        }
        public void scale(double scalingFactor, Vec3d centroid)
        {
            for (int i = 0; i < this.faceList.Count; i++)
            {
                this.faceList[i].ScaleNFace(scalingFactor, centroid);
            }
        }


        // Face Checker
        public static bool checkNanNMesh(NMesh inputMesh)
        {
            bool isValid = true;
            List<Vec3d> allpoints = new List<Vec3d>();
            foreach (NFace face in inputMesh.faceList)
                allpoints.AddRange(face.getPoints());

            foreach (Vec3d point in allpoints)
            {
                if (Double.IsNaN(point.X))
                {
                    isValid = false;
                }
                if (Double.IsNaN(point.Y))
                {
                    isValid = false;
                }
                if (Double.IsNaN(point.Z))
                {
                    isValid = false;
                }
            }
            return isValid;
        }
        public static List<bool> faceAreaValidCheck(NMesh aptBase, double minSize)
        {
            // CHECK 1 Size: larger than minSize
            // returns bool list indicating if face of mesh is smaller or bigger than minSize

            List<bool> aptValid = new List<bool>();

            for (int i = 0; i < aptBase.faceList.Count; i++)
            {
                bool isValid = true;

                // CHECK 1 Size: larger than minSize
                if (aptBase.faceList[i].Area < minSize)
                {
                    isValid = false;
                }
                aptValid.Add(isValid);
            }
            return aptValid;
        }
        public static List<bool> faceAdjacencyLengthCheck(NMesh aptBase, List<NLine> boundsLines, double minAdjacencyLength)
        {
            // CHECK 1 Check if adjacency length is > minAdjacencyLength
            // return bool list

            // example use case for checking if apt. is connected to circulation or facade linws

            List<bool> aptValid = new List<bool>();

            for (int i = 0; i < aptBase.faceList.Count; i++)
            {
                bool isValid = true;

                List<NLine> tempLineList = RhConvert.NFaceToLineList(aptBase.faceList[i]);
                List<NLine> boolIntersectionExt = RIntersection.lineListBooleanIntersection(boundsLines, tempLineList);

                double lengthTemp = 0;
                for (int j = 0; j < boolIntersectionExt.Count; j++)
                {
                    lengthTemp += boolIntersectionExt[j].Length;
                }

                if (lengthTemp < minAdjacencyLength)
                {
                    isValid = false;
                }

                aptValid.Add(isValid);
            }
            return aptValid;

        }
        public static bool NMeshFacesInBounds(NMesh inputMesh, NFace bounds)
        {
            // checks if all mesh faces are inside the bounds. // 2d only
            bool inbounds = true;

            for (int i = 0; i < inputMesh.faceList.Count; i++)
            {
                inputMesh.faceList[i].updateEdgeConnectivity();
                Vec3d centroidTemp = NFace.centroidInsideFace(inputMesh.faceList[i]);
                bool currentInside = RIntersection.insideNFace(centroidTemp, bounds);
                if (currentInside == false)
                    return false;
            }
            return inbounds;
        }

        // Cleanup Methods
        public static NMesh deepCleaner(NMesh inputMesh, double mergeDistance)
        {

            for (int i = 0; i < inputMesh.faceList.Count; i++)
            {
                string tempMergeID = inputMesh.faceList[i].merge_id;
                inputMesh.faceList[i] = NFace.deepCleaner(inputMesh.faceList[i], mergeDistance);
                inputMesh.faceList[i].merge_id = tempMergeID;
            }

            return inputMesh;
        }
        public void deleteFacesWithZeroArea()
        {
            List<NFace> tempList = new List<NFace>();
            for (int i = 0; i < this.faceList.Count; i++)
            {
                if (this.faceList[i].Area > Constants.DocTolerance)
                {
                    tempList.Add(this.faceList[i]);
                } 
            }
            this.faceList = tempList;
        }
        public void Round(int decimals)
        {
            // rounds vertex coordinates to 
            for (int i = 0; i < this.faceList.Count; i++)
            {
                this.faceList[i].Round(decimals);
            }
        }
        public void facesClockwise()
        {
            for (int i = 0; i < this.faceList.Count; i++)
            {
                bool counter = this.faceList[i].IsClockwise;
                if (counter == false)
                    this.faceList[i].flipRH();
            }
        }

        public static NMesh cleanInOut(NMesh inputMesh, double delta)
        {
            List<NFace> outList = new List<NFace>();
            for (int i = 0; i < inputMesh.faceList.Count; i++)
            {
                string id = inputMesh.faceList[i].merge_id;
                NMesh outMesh = RClipper.clipperInflatePathsMitterPolygon(inputMesh.faceList[i], delta);
                for (int j = 0; j < outMesh.faceList.Count; j++)
                {
                    outMesh.faceList[j].merge_id = id;
                }
                outList.AddRange(outMesh.faceList);
            }


            NMesh outM = new NMesh(outList);
            return outM;
        }


        public static NMesh internalSimplification(NMesh inputMesh)
        {
            // removes additional points that are between two faces of mesh but leaves outside intact

            //         A------A             A------A
            //         |      |             |      |
            //    A----A      |        A----A      |
            //    |    |      |    --> |    |      |
            //    A    A      |        A    |      |
            //    |    |      |        |    |      |
            //    A----A      |        A----A      |
            //         |      |             |      |
            //         A------A             A------A

            // identify inside pts
            List<Vec3d> insidePts = new List<Vec3d>();
            List<Vec3d> outsidePts = new List<Vec3d>();

            for (int i = 0; i < inputMesh.faceList.Count; i++)
            {
                //////////////////////////////////
                // Get current face and other faces
                int MasterIndex = i;
                NFace currentFace = inputMesh.faceList[MasterIndex];
                List<NFace> otherFaces = inputMesh.faceList.Where((number, index) => index != MasterIndex).ToList();
                NMesh others = new NMesh(otherFaces);

                // convert faces to lines for id
                List<NLine> currentLines = RhConvert.NFaceToLineList(inputMesh.faceList[MasterIndex]);
                List<NLine> otherLines = NMesh.GetUniqueMeshLines(others, 0.001);
                //////////////////////////////////

                List<NLine> combinedLines = RIntersection.lineListBooleanIntersection(currentLines, otherLines);
                List<NLine> diffLines = RIntersection.lineListBooleanDifference(currentLines, otherLines);


                // convert lines to pt for marking
                insidePts.AddRange(NLine.GetPoints(combinedLines));
                outsidePts.AddRange(NLine.GetPoints(diffLines));
            }

            // DEBUG Visualize
            //A = RhConvert.NLineListToRhLineCurveList(combinedLines);
            //B = RhConvert.NLineListToRhLineCurveList(diffLines);
            //C = RhConvert.Vec3dListToRhPoint3dList(insidePts);
            //D = RhConvert.Vec3dListToRhPoint3dList(outsidePts);

            // go through each face and check for pt inside
            for (int i = 0; i < inputMesh.faceList.Count; i++)
            {
                for (int j = 0; j < inputMesh.faceList[i].edgeList.Count; j++)
                {
                    for (int k = 0; k < insidePts.Count; k++)
                    {
                        if (Vec3d.Distance(inputMesh.faceList[i].edgeList[j].v, insidePts[k]) < 0.0001)
                            inputMesh.faceList[i].edgeList[j].property = "inside";
                    }

                }
            }

            // go through each and check for bounds
            // go through each face and check for pt
            for (int i = 0; i < inputMesh.faceList.Count; i++)
            {
                for (int j = 0; j < inputMesh.faceList[i].edgeList.Count; j++)
                {
                    for (int k = 0; k < outsidePts.Count; k++)
                    {
                        if (Vec3d.Distance(inputMesh.faceList[i].edgeList[j].v, outsidePts[k]) < 0.0001)
                            inputMesh.faceList[i].edgeList[j].property = "outside";
                    }

                }
            }

            // cull points by property
            for (int i = 0; i < inputMesh.faceList.Count; i++)
            {
                List<NEdge> dupTempEdges = new List<NEdge>();

                for (int j = 0; j < inputMesh.faceList[i].edgeList.Count; j++)
                {
                    if (inputMesh.faceList[i].edgeList[j].property != "inside")
                        dupTempEdges.Add(inputMesh.faceList[i].edgeList[j]);
                }
                NFace tempFace = new NFace(dupTempEdges);
                inputMesh.faceList[i] = tempFace;
            }

            return inputMesh;

        }



        // A recursive function used by topologicalSort adapted from https //www geeksforgeeks org/topological-sorting/
        public void TopologicalSortUtil(int v, bool[] visited, Stack<int> stack)
        {
            // Mark the current node as visited.
            visited[v] = true;

            // Recur for all the vertices
            // adjacent to this vertex
            //for (int i = 0; i < adj[v].Count; i++)
            foreach (int vertex in this.adjacencyList[v])
            {
                if (!visited[vertex])
                    TopologicalSortUtil(vertex, visited, stack);
            }

            // Push current vertex to
            // stack which stores result
            stack.Push(v);
        }

        // The function to do Topological Sort.
        // It uses recursive topologicalSortUtil()  adapted from https //www geeksforgeeks org/topological-sorting/
        public void TopoSortFaces()
        {
            UpdateAdjacencyMatrix();
            UpdateAdjacencyList();

            int numFaces = this.faceList.Count;
            Stack<int> stack = new Stack<int>();

            // Mark all the vertices as not visited
            var visited = new bool[numFaces];

            // Call the recursive helper function
            // to store Topological Sort starting
            // from all vertices one by one
            for (int i = 0; i < numFaces; i++)
            {
                if (visited[i] == false)
                    TopologicalSortUtil(i, visited, stack);
            }

            // Output contents of stack as List
            List<int> outList = new List<int>();
            foreach (var num in stack)
            {
                outList.Add(num);
            }
            //return outList;

            // Output faces in correct numbering
            List<NFace> tempFaces = new List<NFace>();
            int offsetInt = 0;
            int iter = 0;
            foreach (var num in stack)
            {
                tempFaces.Add(this.faceList[num]);
                if (num == 0)
                {
                    offsetInt = iter;
                }
                iter++;
            } 

            this.faceList = tempFaces;
            shiftFacesByInt(offsetInt);
        }




        // Axis Methods
        public void UpdateAxisTol(int tol, double maxDist = 0.1, double maxAngle = 0.1)
        {
            //Tuple<NMesh, string> outTuple = SnapMeshToAxis(tempMesh);
            double inputMaxAngle = maxAngle;
            double inputMaxDist = maxDist;

            // global axis list
            List<Axis> globalAxisList = new List<Axis>();

            // for every face in mesh
            for (int i = 0; i < this.faceList.Count; i++)
            {
                // for every edge in face
                this.faceList[i].updateEdgeConnectivity();

                for (int j = 0; j < this.faceList[i].edgeList.Count; j++)
                {

                    // create axis for edge
                    Axis tempAxis = new Axis(this.faceList[i].edgeList[j], tol);
                    Axis currentAxis = tempAxis;
                    currentAxis.property = this.faceList[i].edgeList[j].property;

                    // check if axis exists(/is close to other) in global axis list
                    bool axisExists = false;
                    for (int k = 0; k < globalAxisList.Count; k++)
                    {
                        if (Axis.IsAxisSame(globalAxisList[k], tempAxis, inputMaxAngle, inputMaxDist))
                        {
                            axisExists = true;
                            currentAxis = globalAxisList[k];

                            NLine tempLine = new NLine(this.faceList[i].edgeList[j].v, this.faceList[i].edgeList[j].nextNEdge.v);
                            globalAxisList[k].incNLines.Add(tempLine);

                        }
                    }

                    // if no
                    // add to global axis list
                    // add axis to edge
                    if (axisExists == false)
                    {
                        NLine tempLine = new NLine(this.faceList[i].edgeList[j].v, this.faceList[i].edgeList[j].nextNEdge.v);
                        currentAxis.incNLines.Add(tempLine);

                        globalAxisList.Add(currentAxis);
                    }

                    // if yes
                    // add existing axis to edge
                    this.faceList[i].edgeList[j].axis = currentAxis;

                }
            }

            this.axisList = globalAxisList;

        }
        public void UpdateAxis(double maxDist = 0.1, double maxAngle = 0.1)
        {
            //Tuple<NMesh, string> outTuple = SnapMeshToAxis(tempMesh);
            double inputMaxAngle = maxAngle;
            double inputMaxDist = maxDist;

            // global axis list
            List<Axis> globalAxisList = new List<Axis>();

            // for every face in mesh
            for (int i = 0; i < this.faceList.Count; i++)
            {
                // for every edge in face
                this.faceList[i].updateEdgeConnectivity();

                for (int j = 0; j < this.faceList[i].edgeList.Count; j++)
                {

                    // create axis for edge
                    Axis tempAxis = new Axis(this.faceList[i].edgeList[j]);
                    Axis currentAxis = tempAxis;
                    currentAxis.property = this.faceList[i].edgeList[j].property;


                    // check if axis exists(/is close to other) in global axis list
                    bool axisExists = false;
                    for (int k = 0; k < globalAxisList.Count; k++)
                    {
                        if (Axis.IsAxisSame(globalAxisList[k], tempAxis, inputMaxAngle, inputMaxDist))
                        {
                            axisExists = true;
                            string tempproperty = currentAxis.property;
                            currentAxis = globalAxisList[k];

                            NLine tempLine = new NLine(this.faceList[i].edgeList[j].v, this.faceList[i].edgeList[j].nextNEdge.v);
                            globalAxisList[k].incNLines.Add(tempLine);
                            globalAxisList[k].property += tempproperty;
                        }
                    }

                    // if no
                    // add to global axis list
                    // add axis to edge
                    if (axisExists == false)
                    {
                        NLine tempLine = new NLine(this.faceList[i].edgeList[j].v, this.faceList[i].edgeList[j].nextNEdge.v);
                        currentAxis.incNLines.Add(tempLine);

                        globalAxisList.Add(currentAxis);
                    }
                    
                    // if yes
                    // add existing axis to edge
                    this.faceList[i].edgeList[j].axis = currentAxis;

                }
            }

            this.axisList = globalAxisList;

        }
        public void SnapToAxis(double maxDist = 0.1, double maxAngle = 0.1)
        {
            // deprecated... use snapNMeshToAxisList instead
            this.UpdateAxis(maxDist, maxAngle);

            for (int i = 0; i < this.faceList.Count; i++)
            {
                // for every edge in face
                for (int j = 0; j < this.faceList[i].edgeList.Count; j++)
                {
                    // snap to axis
                    this.faceList[i].edgeList[j].SnapToAxis2D();
                }
                this.faceList[i].updateEdgeConnectivity();
            }
        }

 
        public static NMesh snapNMeshToAxisList(NMesh outMesh, List<Axis> tempAxis)
        {
            for (int i = 0; i < outMesh.faceList.Count; i++)
            {
                outMesh.faceList[i].updateEdgeConnectivity();

                // snap edges to closest axis
                for (int j = 0; j < outMesh.faceList[i].edgeList.Count; j++)
                {
                    //int j = iter;

                    Vec3d testVec = outMesh.faceList[i].edgeList[j].v;
                    List<Vec3d> closestVecs = new List<Vec3d>();
                    for (int k = 0; k < tempAxis.Count; k++)
                    {
                        Vec3d outVec = RIntersection.AxisClosestPoint2D(testVec, tempAxis[k]);
                        closestVecs.Add(outVec);
                    }
                    List<Vec3d> vecsSorted = closestVecs.OrderBy(o => Vec3d.Distance(o, testVec)).ToList();

                    outMesh.faceList[i].edgeList[j].v = vecsSorted[0]; // RIntersection.AxisClosestPoint2D(outMesh.faceList[i].edgeList[j].v, averagedAxisListSorted[0]);
                }
            }
            outMesh = deepCleaner(outMesh, 0.01);

            return outMesh;
        }
        public static NMesh snapNMeshToAxisListTol(NMesh outMesh, List<Axis> tempAxis, double tol)
        {
            for (int i = 0; i < outMesh.faceList.Count; i++)
            {
                outMesh.faceList[i].updateEdgeConnectivity();

                // snap edges to closest axis
                for (int j = 0; j < outMesh.faceList[i].edgeList.Count; j++)
                {
                    //int j = iter;

                    Vec3d testVec = outMesh.faceList[i].edgeList[j].v;
                    List<Vec3d> closestVecs = new List<Vec3d>();
                    for (int k = 0; k < tempAxis.Count; k++)
                    {
                        Vec3d outVec = RIntersection.AxisClosestPoint2D(testVec, tempAxis[k]);
                        closestVecs.Add(outVec);
                    }
                    List<Vec3d> vecsSorted = closestVecs.OrderBy(o => Vec3d.Distance(o, testVec)).ToList();

                    if (Vec3d.Distance(vecsSorted[0], outMesh.faceList[i].edgeList[j].v) < tol)
                        outMesh.faceList[i].edgeList[j].v = vecsSorted[0]; // RIntersection.AxisClosestPoint2D(outMesh.faceList[i].edgeList[j].v, averagedAxisListSorted[0]);
                }
            }
            outMesh = deepCleaner(outMesh, 0.01);

            return outMesh;
        }

        // NEW Snap Methods
        public static NMesh orthoSnapReduceMesh(NMesh inputMesh, double axisDist)
        {

            inputMesh.UpdateAxisTol(3, 0.01, 0.01);
            List<Axis> tempAxis = inputMesh.axisList;

            tempAxis = Axis.unifyAxisDirections(tempAxis, Vec3d.UnitX, Vec3d.UnitY);
            tempAxis = RDbscan.DbscanAxis(tempAxis, axisDist);
            inputMesh.axisList = tempAxis;

            Tuple<List<Axis>, List<Axis>> axisTuple = Axis.sortTwoAxisDirections(tempAxis, Vec3d.UnitX, Vec3d.UnitY);

            inputMesh = snapNMeshToAxisListTol(inputMesh, axisTuple.Item1, axisDist);
            inputMesh = snapNMeshToAxisListTol(inputMesh, axisTuple.Item2, axisDist);


            return inputMesh;
        }

        public static NMesh orthoSnapMeshToAxisList(NMesh inputMesh, List<Axis> inputAxis, double snapTol)
        {

            List<Axis> tempAxis = inputAxis;

            tempAxis = Axis.unifyAxisDirections(tempAxis, Vec3d.UnitX, Vec3d.UnitY);
            inputMesh.axisList = tempAxis;
            Tuple<List<Axis>, List<Axis>> axisTuple = Axis.sortTwoAxisDirections(tempAxis, Vec3d.UnitX, Vec3d.UnitY);

            inputMesh = snapNMeshToAxisListTol(inputMesh, axisTuple.Item1, snapTol);
            inputMesh = snapNMeshToAxisListTol(inputMesh, axisTuple.Item2, snapTol);



            return inputMesh;
        }

        public static List<NLine> identifyLongestAxis(NMesh inputMesh)
        {
            //returns list of lines axis inside poly sorted by length
            inputMesh.UpdateAxis();
            List<NLine> combinedLines = new List<NLine>();

            for (int i = 0; i < inputMesh.axisList.Count; i++)
            {
                List<NLine> currentLines = inputMesh.axisList[i].incNLines;
                if (currentLines.Count > 1)
                {
                    //NPolyLine newPoly = straightSoupToPolyLine(currentLines);
                    //NLine tempLine = new NLine(newPoly.start, newPoly.end);
                    //combinedLines.Add(tempLine);
                    NPolyLine tempLine = new NPolyLine(currentLines);
                    //NLine tempSplitLine = lineFromStraightPoly(tempLine);
                    NLine lineout = NPolyLine.lineFromStraightPoly(tempLine);
                    combinedLines.Add(lineout);//combinedLinesnewPoly
                }
            }

            // sort lines
            var precision = 0.001;
            List<NLine> sortedLines = combinedLines.OrderBy(p => Math.Round(p.Length / precision)).ToList();
            sortedLines.Reverse();

            return sortedLines;
        }
        public static List<NPolyLine> identifyLongestAxisPolyLine(NMesh inputMesh)
        {
            //returns continuous polylines inside mesh
            inputMesh.UpdateAxis();
            List<NLine> combinedLines = new List<NLine>();
            List<NPolyLine> combinedPolyLines = new List<NPolyLine>();

            for (int i = 0; i < inputMesh.axisList.Count; i++)
            {
                List<NLine> currentLines = inputMesh.axisList[i].incNLines;
                if (currentLines.Count > 1)
                {
                    NPolyLine tempLine = new NPolyLine(currentLines);
                    NPolyLine tempPoly = NPolyLine.cleanStraightPolyline(tempLine);
                    combinedPolyLines.Add(tempPoly);
                }
            }
            return combinedPolyLines;
            //return sortedLines;
        }



        // Ortho Methods
        public static NMesh orthoNMeshAll(NMesh inputMesh, Vec3d dirVec)
        {
            List<NFace> tempFaces = new List<NFace>();
            for (int i = 0; i < inputMesh.faceList.Count; i++)
            {
                NFace dupFace = inputMesh.faceList[i].DeepCopy();

                NFace orthoTemp = NFace.orthoFaceLoop(dupFace, dirVec);
                tempFaces.Add(orthoTemp);
            }

            NMesh outMesh = new NMesh(tempFaces);
            return outMesh;
        }
        public static NMesh orthoNMeshInternal(NMesh inputMesh, Vec3d dirVec)
        {
            List<NFace> tempFaces = new List<NFace>();
            for (int i = 0; i < inputMesh.faceList.Count; i++)
            {
                NFace dupFace = inputMesh.faceList[i].DeepCopy();
                dupFace.updateEdgeConnectivity();
                tempFaces.Add(dupFace);
            }

            NMesh outMesh = new NMesh(tempFaces);

            outMesh.UpdateAdjacencyList();

            List<List<int>> makeOrtho = new List<List<int>>();

            for (int i = 0; i < outMesh.faceList.Count; i++)
            {
                // go through each edge
                for (int j = 0; j < outMesh.faceList[i].edgeList.Count; j++)
                {
                    List<int> edgesThatShouldBeOrthoInt = new List<int>();
                    // go through all adjacent Faces
                    for (int k = 0; k < outMesh.adjacencyList[i].Count; k++)
                    {
                        // go through all edges of adjacent Faces
                        for (int l = 0; l < outMesh.faceList[outMesh.adjacencyList[i][k]].edgeList.Count; l++)
                        {
                            if (NEdge.IsNEdgeEqual(outMesh.faceList[outMesh.adjacencyList[i][k]].edgeList[l], outMesh.faceList[i].edgeList[j]))
                                outMesh.faceList[i].edgeList[j].isIntersecting = true;
                        }
                    }
                }
            }

            List<NFace> facesOut = new List<NFace>();

            for (int d = 0; d < outMesh.faceList.Count; d++)
            {
                List<NEdge> outListTempEdges = new List<NEdge>();

                for (int i = 0; i < outMesh.faceList[d].edgeList.Count; i++)
                {
                    if (outMesh.faceList[d].edgeList[i].isIntersecting == true)
                    {
                        List<NEdge> tempList = NFace.orthoFaceEdgeReturn(outMesh.faceList[d], i, dirVec);
                        outListTempEdges.AddRange(tempList);
                    }
                    else
                    {
                        outListTempEdges.Add(outMesh.faceList[d].edgeList[i]);
                    }
                }
                NFace faceOut = new NFace(outListTempEdges);
                facesOut.Add(faceOut);
            }

            NMesh tempOutMesh = new NMesh(facesOut);
            return tempOutMesh;
        }

        // Inside Rect Methods
        public static List<NFace> returnInsideRectsFromNMesh(NMesh inputMesh)
        {
            List<NFace> outFace = new List<NFace>();

            for (int i = 0; i < inputMesh.faceList.Count; i++)
            {
                Tuple<bool, NFace> rectTuple = NFace.returnInsideRect(inputMesh.faceList[i]);

                if (rectTuple.Item1)
                    outFace.Add(rectTuple.Item2);
                else
                    outFace.Add(inputMesh.faceList[i]);
            }
            return outFace;
        }
        public static List<NFace> returnInsideRectListFromNMesh(NMesh inputMesh)
        {
            List<NFace> outFace = new List<NFace>();

            for (int i = 0; i < inputMesh.faceList.Count; i++)
            {
                Tuple<bool, List<NFace>> rectTuple = NFace.returnInsideRectList(inputMesh.faceList[i]);

                if (rectTuple.Item1)
                    outFace.AddRange(rectTuple.Item2);
                else
                    outFace.Add(inputMesh.faceList[i]);
            }
            return outFace;
        }

        // Return Inside
        public static List<NLine> getInteriorLines(NMesh outMesh)
        {
           // Work in progress... the interior mesh lines gives errors!!

            List < NLine> internalWalls = new List<NLine>();
            outMesh.UpdateAdjacencyList();

            for (int i = 0; i < outMesh.faceList.Count; i++)
            {
                // go through all neighbor faces
                NFace tempFace = outMesh.faceList[i];

                // get intersection edges
                for (int j = 0; j < outMesh.faceList[i].neighborNFace.Count; j++)
                {
                    NFace neighborFace = outMesh.faceList[i].neighborNFace[j];

                    //bool areIntersecting = RIntersection.intersectNFacesBool(tempFace, neighborFace);
                    Tuple<NFace, NFace> tempTuple = RIntersection.intersectNFacesTuple(tempFace, neighborFace, true);

                    // go through each edges of temp face , see which one is equal, if yes add to lines
                    foreach (NEdge tempEdge in tempFace.edgeList)
                    {
                        foreach (NEdge neighborEdge in neighborFace.edgeList)
                        {
                            bool isequal = NEdge.IsNEdgeEqual(tempEdge, neighborEdge);
                            if (isequal)
                            {
                                NLine tempLine = new NLine(tempEdge.v, tempEdge.nextNEdge.v);
                                internalWalls.Add(tempLine);
                            }
                        }
                    }

                }
            }
            List<NLine> uniqueLines = NLine.getUniqueLines(internalWalls);

            return uniqueLines;
        }

        public List<string> getFaceMergeID()
        {
            List<string> roomNamesAll = new List<string>();

            for (int i = 0; i < this.faceList.Count; i++)
            {
                roomNamesAll.Add(this.faceList[i].merge_id);
            }

            return roomNamesAll;
        }

        /*
        Work in progress... the interior mesh lines gives errors!!
        public static List<NLine> getInteriorLinesXX(NMesh outMesh)
        {

            // Get interior lines from bounds and all curves

            List<NLine> roomLines = RhConvert.RhPolylineListToNLineList(inputRooms);
            List<NLine> linesInteriorRaw = RIntersection.lineListBooleanDifference(roomLines, bounds);


            List<NPolyLine> comboShattered = NLine.shatterLines(linesInteriorRaw);
            List<NLine> shatteredAll = new List<NLine>();
            for (int i = 0; i < comboShattered.Count; i++)
            {
                shatteredAll.AddRange(comboShattered[i].lineList);
            }

            List<NLine> shatteredAllClean = new List<NLine>();
            for (int i = 0; i < shatteredAll.Count; i++)
            {
                if (shatteredAll[i].Length > 0.001)
                    shatteredAllClean.Add(shatteredAll[i]);
            }

            List<NLine> unique = NLine.deleteDuplicateLines(shatteredAllClean);




            internalWallsOut = RhConvert.NLineListToRhLineCurveList(unique);

        }
        */

        // Convex Hull

        public NFace ConvexHullJarvis()
        {
            // returns face that is convex hull of this face

            // convert edges to vec list
            List<Vec3d> points = NMesh.getMeshPoints(this);
             
            // sort points
            List<Vec3d> sortedPointsX = points.OrderBy(o => o.X).ToList();

            if (points.Count < 3)
            {
                throw new ArgumentException("At least 3 points reqired", "points");
            }

            List<Vec3d> hull = new List<Vec3d>();

            // get leftmost point
            Vec3d vPointOnHull = points.OrderBy(o => o.X).ToList().First();

            Vec3d vEndpoint;
            do
            {
                hull.Add(vPointOnHull);
                vEndpoint = points[0];

                for (int i = 1; i < points.Count; i++)
                {
                    if ((vPointOnHull == vEndpoint)
                      || (Vec3d.OrientationXY(vPointOnHull, vEndpoint, points[i]) == -1))
                    {
                        vEndpoint = points[i];
                    }
                }

                vPointOnHull = vEndpoint;

            }
            while (vEndpoint != hull[0]);

            NFace hullFace = new NFace(hull);

            return hullFace;
        }

        public static List<NLine> GetUniqueMeshLines(NMesh inputMesh, double tolerance)
        {
            // does not shatter???

            HashSet<Tuple<Vec3d, Vec3d>> lineSet = new HashSet<Tuple<Vec3d, Vec3d>>(new Vec3dTupleEqualityComparer(tolerance));
            List<NLine> uniqueLines = new List<NLine>();

            for (int i = 0; i < inputMesh.faceList.Count; i++)
            {
                foreach (NEdge tempEdge in inputMesh.faceList[i].edgeList)
                {
                    Tuple<Vec3d, Vec3d> tuple = new Tuple<Vec3d, Vec3d>(tempEdge.v, tempEdge.nextNEdge.v);

                    if (!lineSet.Contains(tuple))
                    {
                        lineSet.Add(tuple);
                        NLine tempLine = new NLine(tempEdge.v, tempEdge.nextNEdge.v);
                        uniqueLines.Add(tempLine);
                    }
                }
            }

            return uniqueLines;
        }
        public static List<NLine> GetAllMeshLines(NMesh inputMesh)
        {

            List<NLine> internalWalls = new List<NLine>();

            for (int i = 0; i < inputMesh.faceList.Count; i++)
            {
                inputMesh.faceList[i].updateEdgeConnectivity();
                // go through each edges of temp face , see which one is equal, if yes add to lines
                foreach (NEdge tempEdge in inputMesh.faceList[i].edgeList)
                {
                    NLine tempLine = new NLine(tempEdge.v, tempEdge.nextNEdge.v);
                    internalWalls.Add(tempLine);
                }
            }

            return internalWalls;
        }
        public static List<Vec3d> GetAllCentroids(NMesh inputMesh)
        {
            List<Vec3d> centroids = new List<Vec3d>();
            for (int i=0;i<inputMesh.faceList.Count;i++)
            {
                centroids.Add(NFace.centroidInsideFace(inputMesh.faceList[i]));
            }
            return centroids;
        }

        public List<NLine> GetNMeshBoundsLines()
        {
            NMesh unionMesh = RClipper.clipperUnion(this.faceList);
            List<NLine> startLines = NMesh.GetAllMeshLines(unionMesh);
            return startLines;
        }
        public NFace GetNMeshBounds()
        {
            NMesh unionMesh = RClipper.clipperUnion(this.faceList);
            return unionMesh.faceList[0];
        }
        public static List<NLine> getBounds(NMesh outMesh)
        {
            // deprecated use  GetNMeshBoundsLines or GetNMeshBoundsNFace
            NMesh mergedMesh = RSplit.mergeNMesh(outMesh);
            List<NLine> lines = NFace.returnLinesFromNFace(mergedMesh.faceList[0]);
            return lines;
        }

        public static Tuple<NMesh, NMesh> getFacesTouchingLines(NMesh startMesh, List<NLine> newBoundsLines)
        {
            // returns faces that are on the inside, touch just the unionborder, touch the boundsAll border, or the unionborder but not the boundsAllborder

            NMesh unionMesh = RClipper.clipperUnion(startMesh.faceList);

            List<NFace> borderGapFaces = new List<NFace>();
            List<NFace> borderGapInvertedFaces = new List<NFace>();

            for (int j = 0; j < startMesh.faceList.Count; j++)
            {
                bool adjacentToStartLines = false;
                bool adjacentToNewBounds = false;

                for (int k = 0; k < startMesh.faceList[j].edgeList.Count; k++)
                {

                    for (int i = 0; i < newBoundsLines.Count; i++)
                    {
                        NLine tempLine2 = new NLine(startMesh.faceList[j].edgeList[k].v, startMesh.faceList[j].edgeList[k].nextNEdge.v);
                        if (NLine.IsNLineEqual(tempLine2, newBoundsLines[i]))
                            adjacentToNewBounds = true;
                    }
                }

                if (adjacentToNewBounds)
                    borderGapFaces.Add(startMesh.faceList[j]);
                else
                    borderGapInvertedFaces.Add(startMesh.faceList[j]);
            }


            NMesh borderGapMesh = new NMesh(borderGapFaces);
            NMesh borderGapInvertMesh = new NMesh(borderGapInvertedFaces);

            return new Tuple<NMesh, NMesh>(borderGapMesh, borderGapInvertMesh);
        }
        public static Tuple<NMesh, NMesh, NMesh, NMesh> getInternalFaces(NMesh startMesh, List<NLine> newBoundsLines)
        {
            // returns faces that are on the inside, touch just the unionborder, touch the boundsAll border, or the unionborder but not the boundsAllborder

            NMesh unionMesh = RClipper.clipperUnion(startMesh.faceList);
            List<NLine> startLines = NMesh.GetAllMeshLines(unionMesh);

            List<NFace> insideFaces = new List<NFace>();
            List<NFace> borderFaces = new List<NFace>();
            List<NFace> borderGapFaces = new List<NFace>();
            List<NFace> borderGapInvertedFaces = new List<NFace>();

            for (int j = 0; j < startMesh.faceList.Count; j++)
            {
                bool adjacentToStartLines = false;
                bool adjacentToNewBounds = false;

                for (int k = 0; k < startMesh.faceList[j].edgeList.Count; k++)
                {
                    for (int i = 0; i < startLines.Count; i++)
                    {
                        NLine tempLine = new NLine(startMesh.faceList[j].edgeList[k].v, startMesh.faceList[j].edgeList[k].nextNEdge.v);
                        if (NLine.IsNLineEqual(tempLine, startLines[i]))
                            adjacentToStartLines = true;
                    }

                    for (int i = 0; i < newBoundsLines.Count; i++)
                    {
                        NLine tempLine2 = new NLine(startMesh.faceList[j].edgeList[k].v, startMesh.faceList[j].edgeList[k].nextNEdge.v);
                        if (NLine.IsNLineEqual(tempLine2, newBoundsLines[i]))
                            adjacentToNewBounds = true;
                    }
                }

                if (adjacentToStartLines)
                    borderFaces.Add(startMesh.faceList[j]);
                else if (adjacentToStartLines == false)
                    insideFaces.Add(startMesh.faceList[j]);

                if (adjacentToNewBounds)
                    borderGapFaces.Add(startMesh.faceList[j]);
                else if (adjacentToStartLines)
                    borderGapInvertedFaces.Add(startMesh.faceList[j]);
            }

            NMesh borderMesh = new NMesh(borderFaces);
            NMesh insideMesh = new NMesh(insideFaces);
            NMesh borderGapMesh = new NMesh(borderGapFaces);
            NMesh borderGapInvertMesh = new NMesh(borderGapInvertedFaces);

            return new Tuple<NMesh, NMesh, NMesh, NMesh>(borderMesh, insideMesh, borderGapMesh, borderGapInvertMesh);
        }
        public static Tuple<NMesh, NMesh> getInternalFaces(NMesh startMesh)
        {
            // returns faces that touch just the unionborder or are on the inside

            NMesh unionMesh = RClipper.clipperUnion(startMesh.faceList);
            List<NLine> startLines = NMesh.GetAllMeshLines(unionMesh);

            List<NFace> insideFaces = new List<NFace>();
            List<NFace> borderFaces = new List<NFace>();

            for (int j = 0; j < startMesh.faceList.Count; j++)
            {
                bool adjacentToStartLines = false;
                bool adjacentToNewBounds = false;

                for (int k = 0; k < startMesh.faceList[j].edgeList.Count; k++)
                {
                    for (int i = 0; i < startLines.Count; i++)
                    {
                        NLine tempLine = new NLine(startMesh.faceList[j].edgeList[k].v, startMesh.faceList[j].edgeList[k].nextNEdge.v);
                        if (NLine.IsNLineEqual(tempLine, startLines[i]))
                            adjacentToStartLines = true;
                    }
                }

                if (adjacentToStartLines)
                    borderFaces.Add(startMesh.faceList[j]);
                else if (adjacentToStartLines == false)
                    insideFaces.Add(startMesh.faceList[j]);
            }

            NMesh borderMesh = new NMesh(borderFaces);
            NMesh insideMesh = new NMesh(insideFaces);

            return new Tuple<NMesh, NMesh>(borderMesh, insideMesh);
        }

        // Expansion

        public static NMesh expandMeshToBounds(NMesh inputMesh, NFace inputBounds)
        {
            NMesh intersectedMesh = RClipper.clipperIntersection(inputMesh, inputBounds);
            NMesh outMesh = expandMeshToBoundsSolver(intersectedMesh, inputBounds);
            return outMesh;
        }
        public static NMesh expandMeshToBoundsSolver(NMesh inputMeshT, NFace inputBounds)
        {


            // returns expanded mesh 
            // STEP 1 Expand Mesh to Bounds
            // search radius as bounding box long edge
            NLine bb = NFace.BoundingBoxDirectionMainLine(inputBounds);
            double searchRadius = bb.Length * 10;

            //double searchRadius = -10;
            /////////////////////////////
            //// 01 Initialize
            /////////////////////////////
            inputBounds.makeClockwise();

            NMesh startMesh = inputMeshT.DeepCopyWithID();
            NMesh inputMesh = inputMeshT.DeepCopyWithID();

            // Convert mesh to lines
            NFace newBounds = inputBounds.DeepCopy();
            List<NLine> extendedBoundLines = RhConvert.NFaceToLineList(newBounds);

            List<NLine> allLines = NMesh.GetAllMeshLines(startMesh);
            NMesh boundsMesh = RClipper.clipperUnion(startMesh.faceList);
            List<NLine> boundLines = NMesh.GetAllMeshLines(boundsMesh);
            List<Vec3d> boundVecs = NLine.GetPoints(boundLines);
            List<NLine> outerInteriorLines = getOuterInteriorLines(boundLines, allLines);
            //outerInteriorLines = RIntersection.lineListBooleanDifference(outerInteriorLines, boundLines);

            Tuple<NMesh, NMesh, NMesh, NMesh> internalTuple = getInternalFaces(startMesh, extendedBoundLines);

            NMesh borderMesh = internalTuple.Item1;
            NMesh insideMesh = internalTuple.Item2;
            NMesh borderGapMesh = internalTuple.Item3;
            NMesh borderGapInvertMesh = internalTuple.Item4;

            // reconstruct border faces

            /////////////////////////////////////////////////////////////////////
            // for each border face
            List<NFace> borderFacesNew = new List<NFace>();
            


            for (int i = 0; i < borderMesh.faceList.Count; i++)
            {
                NFace tempFace = borderMesh.faceList[i];
                //E = RhConvert.NFaceToRhPolyline(tempFace);

                tempFace.makeClockwise();
                List<NLine> tempLineList = RhConvert.NFaceToLineList(tempFace);

                // construct back polyline
                tempLineList = RIntersection.lineListBooleanDifference(tempLineList, boundLines);
                //F = RhConvert.NLineListToRhLineCurveList(tempLineList);

                // extend ends to new boundary
                tempLineList = NPolyLine.ConformLines(tempLineList);
                List<NPolyLine> tempPolyBounds = NPolyLine.simplifyIntoPolyLines(tempLineList);
                if (tempPolyBounds.Count == 1)
                {
                    //AA = RhConvert.NLineToRhLineCurve(tempPolyBounds[0].lineList[0]);
                    List<NLine> newFaceLines = NLine.DeepCopyList(tempPolyBounds[0].lineList);

                    NLine extendLineStart = NLine.extendLine(newFaceLines[0], searchRadius);
                    NLine lineStartTrim = RIntersection.trimShortestNLineWithBounds(extendLineStart, extendedBoundLines);
                    lineStartTrim.FlipLineWithhSwap();
                    newFaceLines.Insert(0, lineStartTrim);

                    NLine extendLineEnd = NLine.extendLine(newFaceLines[newFaceLines.Count - 1], -searchRadius);
                    NLine lineEndTrim = RIntersection.trimShortestNLineWithBounds(extendLineEnd, extendedBoundLines);
                    newFaceLines[newFaceLines.Count - 1] = lineEndTrim;

                    // Divide bounds
                    NLine cutLine = new NLine(newFaceLines[0].start, newFaceLines[newFaceLines.Count - 1].end);
                    Tuple<NMesh, bool> splitTupleFace = RSplit.divideNFaceWithNLine(inputBounds, cutLine);

                    NPolyLine newInsidePL = new NPolyLine(newFaceLines);
                    NFace tempFaceInside = new NFace(newInsidePL.VecsAll);
                    tempFaceInside.updateEdgeConnectivity();

                    //AE = RhConvert.NFaceToRhPolyline(tempFaceInside);

                    ////////////////////////////////////////////////////////////////////////////////////////////
                    /// COMBINE FACES
                    ////////////////////////////////////////////////////////////////////////////////////////////

                    List<NFace> combinedFaces = new List<NFace>();
                    List<NFace> combinedValidFaces = new List<NFace>();
                    List<NFace> combinedValidFacesTrial = new List<NFace>();

                    // go throgh all faces in the splitTuple, and try combine
                    for (int u = 0; u < splitTupleFace.Item1.faceList.Count; u++)
                    {
                        Tuple<bool, NFace> combTupleA = RSplit.CombineFacesBest(tempFaceInside, splitTupleFace.Item1.faceList[u]);
                        combinedFaces.Add(combTupleA.Item2);
                    }

                    // check which face has the centroid of tempfaceinside inside
                    Vec3d oldCentroid = NFace.centroidInsideFace(tempFace);

                    for (int u = 0; u < combinedFaces.Count; u++)
                    {
                        bool isInside = RIntersection.insideNFace(oldCentroid, combinedFaces[u]);
                        if (isInside)
                            combinedValidFaces.Add(combinedFaces[u]);
                    }
                    // If list empty
                    // maybe full split of bounds, check if one side of the og face split has centroid inside
                    NMesh tempOut = new NMesh(combinedValidFaces);
                    if (combinedValidFaces.Count == 0 || tempOut.Area < 0.001)
                    {
                        for (int u = 0; u < splitTupleFace.Item1.faceList.Count; u++)
                        {
                            bool isInside = RIntersection.insideNFace(oldCentroid, splitTupleFace.Item1.faceList[u]);
                            if (isInside)
                                combinedValidFaces.Add(combinedFaces[u]);
                        }
                    }

                    tempOut = new NMesh(combinedValidFaces);

                    borderFacesNew.Add(combinedValidFaces[0]);
                }
            }
            borderFacesNew.AddRange(insideMesh.faceList);

            // if list is empty or has just one item . output new border
            if (borderFacesNew.Count == 0 || borderFacesNew.Count == 1)
                borderFacesNew.Add(inputBounds);

            for (int i = 0; i < borderFacesNew.Count; i++)
            {
                borderFacesNew[i].makeClockwise();
            }

           

            NMesh outFinalMesh = new NMesh(borderFacesNew);
            //AF = RhConvert.NMeshToRhLinePolylineList(outFinalMesh);
            return outFinalMesh;
        }
        private static List<NLine> getOuterInteriorLines(List<NLine> boundLines, List<NLine> allLines)
        {
            // Returns lines of mesh that touch bounds but are not bounds

            //
            //   X-----X--X          X-----O--X
            //   |     |  |          |     *  |
            //   |     |  |          |     *  |
            //   X--X--X--X   -->    O**O--O**O      O**O (extracted edges)
            //   |  |  |  |          |  *  *  |
            //   |  |  |  |          |  *  *  |
            //   X--X--X--X          X--O--O--X



            List<NLine> outerInteriorLines = new List<NLine>();
            for (int i = 0; i < allLines.Count; i++)
            {
                bool isbounds = false;
                bool touchesbounds = false;
                for (int j = 0; j < boundLines.Count; j++)
                {
                    if (NLine.IsNLineEqual(boundLines[j], allLines[i]))
                        isbounds = true;

                    if (RIntersection.LineLineIntersectionBool(boundLines[j], allLines[i]))
                        touchesbounds = true;
                }

                if (isbounds == false && touchesbounds == true)
                    outerInteriorLines.Add(allLines[i]);
            }

            return outerInteriorLines;
        }
        public static NMesh flipAllFaces(NMesh inputMesh)
        {
            for (int i = 0; i < inputMesh.faceList.Count; i++)
            {
                inputMesh.faceList[i].flipRH();
            }
            return inputMesh;
        }
        // Overrides

        public override string ToString()
        {
            string outstring = "";
            string faceSpecs = "";
            int totEdges = 0;

            for (int i = 0; i < this.faceList.Count; i++)
            {
                totEdges += this.faceList[i].edgeList.Count;

                faceSpecs += $" Face {i} [NumEdges: {this.faceList[i].edgeList.Count}]";
                faceSpecs += "\n";

                for (int j = 0; j < this.faceList[i].edgeList.Count; j++)
                {
                    faceSpecs += $"  Vertex {i}.{j} [{this.faceList[i].edgeList[j].v.X}, {this.faceList[i].edgeList[j].v.Y}, {this.faceList[i].edgeList[j].v.Z}]";
                    faceSpecs += "\n";
                }
                
            }
            outstring += $"NMesh [NumFaces: {this.faceList.Count}, NumEdges: {totEdges} ]";
            outstring += "\n";
            outstring += faceSpecs;
            outstring += "\n";
            return outstring;
        }

        // Serializer

        public static string serializeNMesh(NMesh inputNMesh)
        {
            JMesh inputJMesh = new JMesh(inputNMesh);
            return JMesh.serializeJMesh(inputJMesh);
        }
        public static NMesh deserializeNMesh(string jsonString)
        {
            JMesh reverseNode = JMesh.deserializeJMesh(jsonString); //  JsonSerializer.Deserialize<JMesh>(jsonString, options);
            NMesh convertedNMesh = reverseNode.returnNMesh();
            return convertedNMesh;
        }
    }
}
