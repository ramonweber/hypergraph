using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RGeoLib
{
    public class NFace
    {

        public List<NEdge> edgeList;
        
        public List<NFace> neighborNFace = new List<NFace>();

        public TreeNode faceTree;
        public Dictionary<int, List<double>> ratioDict { get; set; }

        public Vec3d Orientation { get; set; }
        public NLine lineTop;
        public NLine lineBottom;
        public Vec3d Normal { get; set; }
        public bool hasNormal = false;
        public bool isReverse { get; set; }

        public string merge_id = "";
        public string unique_id = "";
        public List<string> neighbors_id = new List<string>();
        public DataNode dataNodeObject { get; set; }

        public NFace(List<NEdge> edges)
        {
            // initialize edges
            this.edgeList = edges;
        }

        public NFace(List<Vec3d> corners)
        {
            this.edgeList = new List<NEdge>();
            for (int i = 0; i < corners.Count; i++)
            {
                NEdge tempEdge = new NEdge(corners[i]);
                this.edgeList.Add(tempEdge);
            }
            this.updateEdgeConnectivity();
        }

        public NFace DeepCopy()
        {
            List<NEdge> copyEdgeList = new List<NEdge>();
            foreach (NEdge edge in this.edgeList)
            {
                NEdge copyEdge = edge.DeepCopy();
                copyEdgeList.Add(copyEdge);
            } 
            NFace deepCopyFace = new NFace(copyEdgeList);
            return deepCopyFace;
        }

        public NFace DeepCopyWithMID()
        {
            string tempString = "";
            tempString += this.merge_id;
            List<NEdge> copyEdgeList = new List<NEdge>();
            foreach (NEdge edge in this.edgeList)
            {
                NEdge copyEdge = edge.DeepCopy();
                copyEdgeList.Add(copyEdge);
            }
            NFace deepCopyFace = new NFace(copyEdgeList);
            deepCopyFace.merge_id = tempString;
            return deepCopyFace;
        }

        // Works only with regular polygons
        public bool IsClockwise
        {
            get { return SignedArea < 0; }
        }
        public double SignedArea
        {
            get
            {
                double fArea = 0;
                int N = this.edgeList.Count;
                if (N == 0)
                    return 0;
                Vec3d v1 = this.edgeList[0].v, v2 = Vec3d.Zero;
                for (int i = 0; i < N; ++i)
                {
                    v2 = this.edgeList[(i + 1) % N].v;
                    fArea += v1.X * v2.Y - v1.Y * v2.X;
                    v1 = v2;
                }
                return fArea * 0.5;
            }
        }
        public double Area
        {
            get
            {
                return Math.Abs(SignedArea);
            }
        }

        public Vec3d Centroid
        {
            get
            {
                // returns area centroid

                Vec3d tempCentroid = new Vec3d(0,0,0);
                for (int i = 0; i < this.edgeList.Count; i++)
                {
                    tempCentroid+=this.edgeList[i].v;
                }
                tempCentroid /= (this.edgeList.Count);
                return tempCentroid;
            }
        }

        public double Perimeter
        {
            get
            {
                List<NLine> tempLines = RhConvert.NFaceToLineList(this);
                double tempLen = 0;
                for (int i=0; i < tempLines.Count;i++)
                {
                    tempLen += tempLines[i].Length;
                }
                return Math.Abs(tempLen);
            }
        }

        public static bool isConvex(NFace face0)
        {
            NFace inputFace = face0.DeepCopy();
            inputFace.updateEdgeConnectivity();

            if (inputFace.IsClockwise == false)
            {
                inputFace.flipRH();
            }

            bool isConvex = true;
            for (int i = 0; i < inputFace.edgeList.Count; i++)
            {
                double angle = Vec3d.Angle2PI_2d(inputFace.edgeList[i].Direction, inputFace.edgeList[i].nextNEdge.Direction.GetReverse());
                if (angle > Math.PI)
                    isConvex = false;
            }
            return isConvex;
        }
        public static bool isRectangle(NFace inputFace)
        {
            if (inputFace.edgeList.Count != 4)
                return false;

            // Check if all angles are 90 degrees (or close to it)
            double angleTolerance = 1e-10;
            for (int i = 0; i < inputFace.edgeList.Count; i++)
            {
                Vec3d p1 = inputFace.edgeList[i].v;
                Vec3d p2 = inputFace.edgeList[(i + 1) % inputFace.edgeList.Count].v;
                Vec3d p3 = inputFace.edgeList[(i + 2) % inputFace.edgeList.Count].v;

                double dx1 = p2.X - p1.X;
                double dy1 = p2.Y - p1.Y;
                double dx2 = p3.X - p2.X;
                double dy2 = p3.Y - p2.Y;

                double dotProduct = dx1 * dx2 + dy1 * dy2;
                double magnitudeProduct = Math.Sqrt(dx1 * dx1 + dy1 * dy1) * Math.Sqrt(dx2 * dx2 + dy2 * dy2);
                double angle = Math.Acos(dotProduct / magnitudeProduct) * 180 / Math.PI;

                if (Math.Abs(angle - 90) > angleTolerance)
                    return false;
            }

            return true;
        }

        public void makeCounterClockwise()
        {
            if (this.IsClockwise == true)
            {
                this.flipRH();
            }
        }
        public void makeClockwise()
        {
            if (this.IsClockwise == false)
            {
                this.flipRH();
            }
        }

        public void initializeRatioDict()
        {
            this.ratioDict = new Dictionary<int, List<double>>();
            for (int i = 0; i < this.edgeList.Count; i++)
            {
                List<double> emptyNE = new List<double>();
                this.ratioDict.Add(i, emptyNE);
            }
        }

        public void subdivideNFaceEdgeByRatioDict()
        {
            int tempEntry = 0;
            foreach (KeyValuePair<int, List<double>> entry in this.ratioDict)
            {
                this.subdivideNFaceEdgeByRatioList(entry.Key + tempEntry, entry.Value);
                tempEntry += (entry.Value.Count);

                this.updateEdgeConnectivity();
                this.mergeDuplicateVertex();
            }
        }

        public void cleanFace()
        {

            //NFace tempFace0 = tempFace;

            List<Vec3d> allVerts = new List<Vec3d>();

            for (int i = 0; i < this.edgeList.Count; i++)
            {
                allVerts.Add(this.edgeList[i].v);
            }


            for (int i = 0; i < this.edgeList.Count; i++)
            {
                for (int j = 0; j < allVerts.Count; j++)
                {
                    this.insertVertInEdgeInt(allVerts[j], i);
                }
            }
            if (this.IsClockwise)
            {
                this.flipRH();
                this.shrinkFace();
                this.flipRH();
            }
            else
            {
                this.shrinkFace();
            }
            //return tempFace0;
        }
        public bool addIntersectVertNFace(Vec3d vecToAdd)
        {
            double tolerance = Constants.IntersectTolerance;
            // Inserts vert in edge
            //
            //      x--x             x--x  
            //     /  /      --->   /  /   
            //    /  vecToAdd      /  x    
            //   x--x             x--x     

            // check on what face edge the point intersects
            bool intersects = false;
            bool exists = false;
            int pointLocInt = 0;
            for (int i = 0; i < this.edgeList.Count; i++)
            {
                bool isbetween = Vec3d.IsVecBetweenVecs(this.edgeList[i].v, this.edgeList[i].nextNEdge.v, vecToAdd);
                double dist = Vec3d.Distance(vecToAdd, this.edgeList[i].v);
                if (isbetween == true)
                {
                    intersects = true;
                    pointLocInt = i;
                }
                if (dist < tolerance)
                {
                    exists = true;
                }
            }

            // split the NEdge

            if ((intersects == true) && (exists == false))
            {
                NEdge edgeInsert = new NEdge(vecToAdd);
                edgeInsert.isIntersecting = true;
                this.edgeList.Insert(pointLocInt + 1, edgeInsert);
                updateEdgeConnectivity();
            }

            return intersects;
        }
        public int addIntersectVertNFaceReturnInt(Vec3d vecToAdd)
        {
            double tolerance = Constants.IntersectTolerance;
            // Inserts vert in edge
            //
            //      x--x             x--x  
            //     /  /      --->   /  /   
            //    /  vecToAdd      /  x    
            //   x--x             x--x     

            // check on what face edge the point intersects
            bool intersects = false;
            bool exists = false;
            int pointLocInt = 0;
            for (int i = 0; i < this.edgeList.Count; i++)
            {
                bool isbetween = Vec3d.IsVecBetweenVecs(this.edgeList[i].v, this.edgeList[i].nextNEdge.v, vecToAdd);
                double dist = Vec3d.Distance(vecToAdd, this.edgeList[i].v);
                if (isbetween == true)
                {
                    intersects = true;
                    pointLocInt = i;
                }
                if (dist < tolerance)
                {
                    exists = true;
                }
            }

            // split the NEdge

            if ((intersects == true) && (exists == false))
            {
                NEdge edgeInsert = new NEdge(vecToAdd);
                edgeInsert.isIntersecting = true;
                this.edgeList.Insert(pointLocInt + 1, edgeInsert);
                updateEdgeConnectivity();
            }

            return pointLocInt+1;
        }
        public bool insertVertInEdgeInt(Vec3d vecToAdd, int i)
        {
            // i = edge number whhere insert is happening
            double tolerance = Constants.IntersectTolerance;
            // Inserts vert in edge
            //
            //      x--x             x--x
            //     /  /      --->   /  /
            //    /  vecToAdd      /  x
            //   x--x             x--x

            // check on what face edge the point intersects
            bool intersects = false;
            bool exists = false;
            int pointLocInt = 0;

            bool isbetween = Vec3d.IsVecBetweenVecs(this.edgeList[i].v, this.edgeList[i].nextNEdge.v, vecToAdd);
            double dist = Vec3d.Distance(vecToAdd, this.edgeList[i].v);
            if (isbetween == true)
            {
             intersects = true;
             pointLocInt = i;
             }
             if (dist < tolerance)
             {
                    exists = true;
             }

            // split the NEdge

            if ((intersects == true) && (exists == false))
            {
                NEdge edgeInsert = new NEdge(vecToAdd);
                this.edgeList.Insert(i + 1, edgeInsert);
                updateEdgeConnectivity();
            }

            return intersects;
        }



        public void deleteDuplicateNEdge()
        {
            List<NEdge> inputEdges = this.edgeList;
            List<NEdge> outputEdges = new List<NEdge>();
            //string outstring = "";

            for (int i = 0; i < inputEdges.Count; i++)
            {
                inputEdges[i].SetTempEnd();
            }

            //bool isEqual = NEdge.IsNEdgeEqual(inputEdges[2], inputEdges[0]);

            for (int i = 0; i < inputEdges.Count; i++)
            {
                //outstring += inputEdges[i];
                //outstring += "\n";

                bool hasCopy = false;
                for (int j = 0; j < inputEdges.Count; j++)
                {
                    if (i != j)
                    {
                        NEdge edge1 = inputEdges[i];
                        NEdge edge2 = inputEdges[j];

                        if (NEdge.IsNEdgeEqual(edge1, edge2))
                        {
                            hasCopy = true;
                            //outstring += "here";
                        }
                    }

                }

                if (hasCopy == false)
                    outputEdges.Add(inputEdges[i]);
            }

            this.edgeList = outputEdges;
            updateEdgeConnectivity();
        }

        public void shrinkFaceTol(double tolerance)
        {
            //doesnt work??
            //this.deleteDuplicateNEdge();

            double areaTolerance = tolerance; // 0.0001
            double area = this.Area;
            NFace doubleFace = this.DeepCopy();

            for (int u = 0; u < doubleFace.edgeList.Count - 1; u++)
            {
                NFace duplicateFace = doubleFace.DeepCopy();

                List<NEdge> duplicateEdges = duplicateFace.edgeList;

                duplicateEdges.RemoveAt(u);

                NFace checkFace = new NFace(duplicateEdges);
                checkFace.updateEdgeConnectivity();

                if (checkFace.Area > area - areaTolerance && checkFace.Area < area + areaTolerance)
                    doubleFace = checkFace.DeepCopy();
            }

            this.edgeList = doubleFace.edgeList;
            updateEdgeConnectivity();
            // return doubleFace;
        }
        public void shrinkFace()
        {
            //doesnt work??
            //this.deleteDuplicateNEdge();

            double areaTolerance = 0.00001; // 0.0001
            double area = this.Area;
            NFace doubleFace = this.DeepCopy();

            for (int u = 0; u < doubleFace.edgeList.Count - 1; u++)
            {
                NFace duplicateFace = doubleFace.DeepCopy();

                List<NEdge> duplicateEdges = duplicateFace.edgeList;
                duplicateEdges[u+1].property += duplicateEdges[u].property;

                duplicateEdges.RemoveAt(u);

                NFace checkFace = new NFace(duplicateEdges);
                checkFace.updateEdgeConnectivity();

                if (checkFace.Area > area - areaTolerance && checkFace.Area < area + areaTolerance)
                {
                    doubleFace = checkFace.DeepCopy();
                    
                }
                    
            }

            this.edgeList = doubleFace.edgeList;
            updateEdgeConnectivity();
            // return doubleFace;
        }
        public void shrinkMax()
        {
            // shrinks edges until the bottom is reached, max 100;
            int breaker = 0;
            bool solved = false;

            while (breaker < 100 && solved == false)
            {
                int numEdges = this.edgeList.Count;

                this.shrinkFace();
                this.shrinkFace();

                int numEdgesShrunk = this.edgeList.Count;

                if (numEdgesShrunk == numEdges)
                    solved = true;

                breaker++;
            }
            this.updateEdgeConnectivity();
        }

        public void checkForZeroEdge()
        {
            // does not really work, do not use yet

            // checks for edge with 0 face area
            //
            //   x------x----x     -->    x------x 
            //   |       \                |       \
            //   |        \               |        \
            //   x---------x              x---------x
            //

            double tolDist = Constants.DocTolerance; 
            updateEdgeConnectivity();

            // iterate through all edges

            List<NEdge> edgesOk = new List<NEdge> ();

            for (int i = 0; i < this.edgeList.Count; i++)
            {
                // if edge next edge start == previous edge start, delete edge
                double dist0 = Vec3d.Distance(this.edgeList[i].nextNEdge.v, this.edgeList[i].prevNEdge.v);
                if (dist0 > tolDist)
                {
                    edgesOk.Add(this.edgeList[i]);
                }
            }

            // Replace edglist and update connectivity
            this.edgeList = edgesOk;
            updateEdgeConnectivity();
        }
        public void checkFor180Angle()
        {

            // Removes 180 angle vertex
            //
            //   X----X                   X----X
            //   |    |    will find and  |    |
            //   |    |    remove D       |    |
            //   |    D                   |    |
            //   |    |        --->       |    |
            //   X----X                   X----X
            //
            //

            double tolAngle = Constants.AngleEqualityTolerance;

            NFace face = new NFace(this.edgeList);

            face.updateEdgeConnectivity();
            List<double> anglesList = new List<double>();
            // iterate through all edges

            List<NEdge> edgesNot = new List<NEdge>();
            List<NEdge> edgesOk = face.edgeList;

            if (face.edgeList.Count > 3)
            {
                for (int i = 0; i < face.edgeList.Count; i++)
                {
                    // if next edge direction == prev edge direction reversed, delete current vec
                    double angle0 = Vec3d.Angle(face.edgeList[i].nextNEdge.v - face.edgeList[i].v, face.edgeList[i].prevNEdge.v - face.edgeList[i].v);
                    //anglesList.Add(angle0);
                    if ((angle0 < (Math.PI) + tolAngle) && (angle0 > (Math.PI) - tolAngle))
                    {
                        edgesOk.Remove(face.edgeList[i]);
                        //edgesNot.Add(this.edgeList[i]);
                        //i++;
                    }
                    else if ((angle0 < -(Math.PI) + tolAngle) && (angle0 > -(Math.PI) - tolAngle))
                    {
                        edgesOk.Remove(face.edgeList[i]);
                    }
                    else
                    {
                        //edgesOk.Add(this.edgeList[i]);
                    }
                }
                // Replace edglist and update connectivity
                face.edgeList = edgesOk;
            }


            face.updateEdgeConnectivity();
            this.edgeList = face.edgeList;
        }

        public void checkFor180Angle(double tolAngle)
        {

            // Removes 180 angle vertex
            //
            //   X----X                   X----X
            //   |    |    will find and  |    |
            //   |    |    remove D       |    |
            //   |    D                   |    |
            //   |    |        --->       |    |
            //   X----X                   X----X
            //
            //

            //double tolAngle = Constants.AngleEqualityTolerance;

            NFace face = new NFace(this.edgeList);

            face.updateEdgeConnectivity();
            List<double> anglesList = new List<double>();
            // iterate through all edges

            List<NEdge> edgesNot = new List<NEdge>();
            List<NEdge> edgesOk = face.edgeList;

            if (face.edgeList.Count > 3)
            {
                for (int i = 0; i < face.edgeList.Count; i++)
                {
                    // if next edge direction == prev edge direction reversed, delete current vec
                    double angle0 = Vec3d.Angle(face.edgeList[i].nextNEdge.v - face.edgeList[i].v, face.edgeList[i].prevNEdge.v - face.edgeList[i].v);
                    //anglesList.Add(angle0);
                    if ((angle0 < (Math.PI) + tolAngle) && (angle0 > (Math.PI) - tolAngle))
                    {
                        edgesOk.Remove(face.edgeList[i]);
                        //edgesNot.Add(this.edgeList[i]);
                        //i++;
                    }
                    else if ((angle0 < -(Math.PI) + tolAngle) && (angle0 > -(Math.PI) - tolAngle))
                    {
                        edgesOk.Remove(face.edgeList[i]);
                    }
                    else
                    {
                        //edgesOk.Add(this.edgeList[i]);
                    }
                }
                // Replace edglist and update connectivity
                face.edgeList = edgesOk;
            }


            face.updateEdgeConnectivity();
            this.edgeList = face.edgeList;
        }
        public void checkForZeroEdgeAngle()
        {   
            // uses angles

            // checks for edge with 0 face area
            //
            //    ___________
            //   x------x----x     -->    x------x 
            //   |       \                |       \
            //   |        \               |        \
            //   x---------x              x---------x
            double tolAngle = 0.000001;
            updateEdgeConnectivity();

            // iterate through all edges

            List<NEdge> edgesNot = new List<NEdge>();
            List<NEdge> edgesOk = this.edgeList;

            for (int i = 1; i < this.edgeList.Count; i++)
            {
                // if next edge direction == prev edge direction reversed, delete current vec
                double angle0 = Vec3d.AngleDeg(this.edgeList[i].nextNEdge.v - this.edgeList[i].v, this.edgeList[i].prevNEdge.v - this.edgeList[i].v);
                //if (angle0 > 180)
                //{
                //  angle0 -= 360;
                //}
                //double dist0 = Vec3d.Distance(this.edgeList[i].nextNEdge.v, this.edgeList[i].prevNEdge.v);
                if ((angle0 < tolAngle) && (angle0 > -tolAngle))
                {
                    edgesOk.Remove(this.edgeList[i]);
                    //edgesNot.Add(this.edgeList[i]);
                    //i++;
                }
                else
                {
                    //edgesOk.Add(this.edgeList[i]);
                }
            }

            // Replace edglist and update connectivity
            this.edgeList = edgesOk;
            updateEdgeConnectivity();
        }
        public bool checkSelfIntersect()
        {
            bool selfIntersection = false;

            // Create list to check against
            //NFace faceToCheck = face;
            NFace faceToCheck = new NFace(this.edgeList);
            for (int k = 0; k < this.edgeList.Count; k++)
            {
                for (int l = 0; l < faceToCheck.edgeList.Count; l++)
                {
                    bool shouldIncludeEndPoints = false;
                    bool intersectTrue = RIntersection.AreLinesIntersecting(this.edgeList[k].v, this.edgeList[k].nextNEdge.v, faceToCheck.edgeList[l].v, faceToCheck.edgeList[l].nextNEdge.v, shouldIncludeEndPoints);
                    if (intersectTrue == true)
                    {
                        selfIntersection = true;
                    }
                }
            }
            return selfIntersection;
        }
        public static bool selfIntersectingNEdgesVecs(NFace inputFace)
        {
            NFace tempFace = inputFace.DeepCopy();
            tempFace.updateEdgeConnectivity();
            bool selfIntersectingEdges = tempFace.checkSelfIntersect();

            List<Vec3d> ptList = tempFace.getPoints();

            int count1 = ptList.Count;
            List<Vec3d> unique = Vec3d.ITER_RemoveDuplicatesWithTolerance(ptList, 0.001);
            int count2 = unique.Count;


            bool selfIntersectingPts = false;

            if (count1 != count2)
                selfIntersectingPts = true;

            return selfIntersectingPts;
        }


        // Destructive Cleaning Routines

        public static NFace checkInternalDuplicateEdge(NFace inputFace)
        {
            //deprecated use delete internal Lines
            List<NLine> linesGooIn = RhConvert.NFaceToLineList(inputFace);

            // 1------------------

            List<NLine> linesGooDupRemove = new List<NLine>();

            for (int i = 0; i < linesGooIn.Count; i++)
            {
                if (linesGooIn[i].Length > 0.00001)   // 0.01
                    linesGooDupRemove.Add(linesGooIn[i]);
            }

            List<NLine> linesGoo = new List<NLine>();

            for (int i = 0; i < linesGooDupRemove.Count; i++)
            {
                bool hasDup = true;
                for (int j = 0; j < linesGooDupRemove.Count; j++)
                {
                    if (i != j)
                    {
                        if (NLine.IsNLineEqualTol(linesGooDupRemove[i], linesGooDupRemove[j], 0.01))
                            hasDup = false;
                    }
                }
                if (hasDup == true)
                {
                    linesGoo.Add(linesGooDupRemove[i]);
                }
            }

            // 2------------------



            //initialize connection lists
            for (int i = 0; i < linesGoo.Count; i++)
            {
                List<NLine> connectingStartList = new List<NLine>();
                linesGoo[i].connectingStart = connectingStartList;

                List<NLine> connectingEndList = new List<NLine>();
                linesGoo[i].connectingEnd = connectingEndList;
            }

            for (int i = 0; i < linesGoo.Count; i++)
            {
                for (int j = 0; j < linesGoo.Count; j++)
                {
                    if (i != j)
                    {
                        linesGoo[i].DoLinesConnect(linesGoo[j]);
                    }
                }
            }

            List<NLine> outLines = new List<NLine>();

            for (int j = 0; j < linesGoo.Count; j++)
            {
                // go through all curves
                if (linesGoo[j].connectingStart.Count > 1 && linesGoo[j].connectingEnd.Count > 1)
                {
                    // has a lot of 
                }
                else
                {
                    outLines.Add(linesGoo[j]);
                }
            }


            List<Vec3d> vecList = RhConvert.NLineListToVec3dList(linesGoo);

            NFace outFace = new NFace(vecList);
            return outFace;
        }
        public static NFace deleteInternalLines(NFace inputFace)
        {
            List<NLine> linesIn = new List<NLine>();

            for (int i = 0; i < inputFace.edgeList.Count; i++)
            {
                NLine tempLine = new NLine(inputFace.edgeList[i].v, inputFace.edgeList[i].nextNEdge.v);
                tempLine.property = inputFace.edgeList[i].property;
                linesIn.Add(tempLine);
            }

            List<NPolyLine> comboShattered = NLine.shatterLines(linesIn);


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

            List<NLine> unique = NLine.deleteDuplicateLinesALL(shatteredAllClean);

            List<NEdge> edgeNew = new List<NEdge>();
            for (int i = 0; i < unique.Count; i++)
            {
                NEdge tempEdge = new NEdge(unique[i].start);
                tempEdge.property = unique[i].property;
                edgeNew.Add(tempEdge);
            }

            NFace outFace = new NFace(edgeNew);
            return outFace;
        }
        public static NFace deepCleaner(NFace selectFace, double mergeDistance)
        {
            // removes shards and internal bad stuff from face.
            selectFace.updateEdgeConnectivity();
            selectFace = deleteInternalLines(selectFace);
            selectFace.updateEdgeConnectivity();

            List<Vec3d> vecsToCheck = selectFace.getPoints();

            // go through each edge
            double tolerance = mergeDistance;
            //int intersectionInt = 0;
            //int currentEdge = 0;

            // Split ONCE

            bool splitHappened = false;
            List<NLine> splitLineList = new List<NLine>();
            for (int i = 0; i < selectFace.edgeList.Count; i++)
            {
                string edgeProperty_i = selectFace.edgeList[i].property;
                // go through each vertex
                for (int j = 0; j < vecsToCheck.Count; j++)
                {
                    Vec3d closestPt = RIntersection.LineClosestPoint2D(vecsToCheck[j], selectFace.edgeList[i].v, selectFace.edgeList[i].nextNEdge.v);
                    double distance = Vec3d.Distance(closestPt, vecsToCheck[j]);


                    // if vertex closest, distance < tol, add new vertex at that location
                    if (Vec3d.Distance(closestPt, vecsToCheck[j]) < tolerance)
                    {
                        int startEdgeCount = selectFace.edgeList.Count;

                        int intersectionInt = selectFace.addIntersectVertNFaceReturnInt(closestPt);
                        int endEdgeCount = selectFace.edgeList.Count;
                        //
                        if (startEdgeCount != endEdgeCount)
                        {
                            //outstring += "is touching" + intersectionInt + "\n";
                            NLine tempLine = new NLine(vecsToCheck[j], closestPt);
                            splitLineList.Add(tempLine);
                            //startClosestInts.Add(i);
                            //endClosestInts.Add(intersectionInt);
                            //splitHappened = true;
                            //break;
                        }

                    }
                }
            }


            //C = RhConvert.NLineListToRhLineCurveList(splitLineList);

            // check that flip lines are oriented correctly
            List<int> startClosestInts = new List<int>();
            List<int> endClosestInts = new List<int>();


            for (int i = 0; i < selectFace.edgeList.Count; i++)
            {
                for (int j = 0; j < splitLineList.Count; j++)
                {
                    double distanceToStart = Vec3d.Distance(selectFace.edgeList[i].v, splitLineList[j].start);
                    double distanceToEnd = Vec3d.Distance(selectFace.edgeList[i].v, splitLineList[j].end);

                    // if touching at end
                    // flip line

                    if (distanceToStart < 0.0001)
                        splitLineList[j].FlipLine();

                    // if touching at start
                    //do nothing

                    // if does not touch
                    // do nothing
                }
            }

            // cut into smaller mesh pieces

            for (int i = 0; i < splitLineList.Count; i++)
            {
                Tuple<NMesh, bool> MeshTuple = RSplit.divideNFaceWithNLine(selectFace, splitLineList[i]);

                List<NFace> selectionList = MeshTuple.Item1.faceList;
                selectionList = selectionList.OrderBy(o => o.Area).ToList();
                selectionList.Reverse();
                selectFace = selectionList[0];
            }

            selectFace.updateEdgeConnectivity();
            selectFace.shrinkMax();

            return selectFace;
        }

        public void flipRH()
        {   
            // flips edges in accordance to the rhino flip command. 
            // first edge is preserved

            // Creates empty list
            List<NEdge> tempEdges = new List<NEdge>();

            // Go through all edges backwards
            for (int i = (this.edgeList.Count - 1); i >= 0; i--)
            {
                tempEdges.Add(this.edgeList[i]);
            }

            NEdge firstTemp = tempEdges[tempEdges.Count-1];
            tempEdges.RemoveAt(tempEdges.Count - 1);
            tempEdges.Insert(0,firstTemp);

            // Replace edglist and update connectivity
            this.edgeList = tempEdges;
            updateEdgeConnectivity();
        }
        public void flipEdges()
        {
            //Deprecated, use flip RH instead

            // Creates empty list
            List<NEdge> tempEdges = new List<NEdge>();

            // Go through all edges backwards
            for (int i = (this.edgeList.Count-1); i >= 0;i--)
            {
                tempEdges.Add(this.edgeList[i]);
            }
            // Replace edglist and update connectivity
            this.edgeList = tempEdges;
            updateEdgeConnectivity();
        }

        public List<Vec3d> getPoints()
        { 
            List<Vec3d> tempPoints= new List<Vec3d>();

            for (int i = 0; i < this.edgeList.Count; i++)
            {
                tempPoints.Add(this.edgeList[i].v);
            }

            return tempPoints;
        }
        public static Vec3d centroidInsideFace(NFace bounds)
        {
            //returns area Centroid inside face
            NFace tempCurrent = bounds.DeepCopy();
            tempCurrent.updateEdgeConnectivity();

            NFace convexHull = tempCurrent.ConvexHullJarvis();

            NLine mainLine = NFace.BoundingBoxDirectionMainLine(convexHull);
            NLine secondLine = NFace.BoundingBoxDirectionSecondaryLine(convexHull);

            List<NLine> res = RIntersection.trimNLineWithNFace(mainLine, tempCurrent);
            List<NLine> SortedLines = res.OrderBy(o => o.Length).ToList();
            List<NLine> res2 = RIntersection.trimNLineWithNFace(secondLine, tempCurrent);
            List<NLine> SortedLines2 = res2.OrderBy(o => o.Length).ToList();

            Vec3d mid = mainLine.MidPoint;
            if (SortedLines.Count > 0)
            {
                mid = SortedLines[0].MidPoint;
            }

            Vec3d mid2 = secondLine.MidPoint;
            if (SortedLines2.Count > 0)
            {
                mid2 = SortedLines[0].MidPoint;
            }

            //Vec3d mid = SortedLines[0].MidPoint;
            //Vec3d mid2 = SortedLines2[0].MidPoint;

            // get midpoint that is closer to areacentroid
            Vec3d areaMid = bounds.Centroid;
            if (Vec3d.Distance(mid, areaMid) <= Vec3d.Distance(mid2, areaMid))
            {
                return mid;
            }
            else
                return mid2;
        }
        public int longestEdgeIndex()
        {
            double longestEdgeDist = this.edgeList[0].Direction.Mag;
            int longestEdgeIndex = 0;
            for (int i = 1; i < this.edgeList.Count; i++)
            {
                double tempLength = this.edgeList[i].Direction.Mag;
                if (tempLength > longestEdgeDist)
                {
                    longestEdgeDist = tempLength;
                    longestEdgeIndex = i;
                }
            }
            return longestEdgeIndex;
        }

        public int shortestEdgeIndex()
        {
            double shortestEdgeDist = this.edgeList[0].Direction.Mag;
            int shortestEdgeIndex = 0;
            for (int i = 1; i < this.edgeList.Count; i++)
            {
                double tempLength = this.edgeList[i].Direction.Mag;
                if (tempLength < shortestEdgeDist)
                {
                    shortestEdgeDist = tempLength;
                    shortestEdgeIndex = i;
                }
            }
            return shortestEdgeIndex;
        }


        public void mergeDuplicateVertex()
        {
            double tolerance = Constants.DocTolerance;

            List<NEdge> tempEdges = new List<NEdge>();

            for (int i = 0; i < this.edgeList.Count; i++)
            {
                if (i == 0)
                {
                    tempEdges.Add(this.edgeList[i]);
                }
                else
                {
                    // check if distance is > tol.
                    double distance = Vec3d.Distance(this.edgeList[i].prevNEdge.v, this.edgeList[i].v);

                    // if so, add to edgelist, otherwise skip
                    if (distance > tolerance)
                    {
                        tempEdges.Add(this.edgeList[i]);
                    }
                }

            }

            // Replace edglist and update connectivity
            this.edgeList = tempEdges;
            updateEdgeConnectivity();

        }
        public void mergeVertexWithTol(double tolerance)
        {
            List<NEdge> tempEdges = new List<NEdge>();

            for (int i = 0; i < this.edgeList.Count; i++)
            {
                if (i == 0)
                {
                    tempEdges.Add(this.edgeList[i]);
                }
                else
                {
                    // check if distance is > tol.
                    double distance = Vec3d.Distance(this.edgeList[i].prevNEdge.v, this.edgeList[i].v);

                    // if so, add to edgelist, otherwise skip
                    if (distance > tolerance)
                    {
                        tempEdges.Add(this.edgeList[i]);
                    }
                }

            }

            // Replace edglist and update connectivity
            this.edgeList = tempEdges;
            updateEdgeConnectivity();

        }

        public void Round(int decimals)
        {
            // go through each edge of face and round edge
            for (int i = 0; i < this.edgeList.Count; i++)
            {
                this.edgeList[i].Round(decimals);
            }
        }

        // Transform
        public static Tuple<NFace, NLine, double, Vec3d, Vec3d> moveNFaceToLineWithHistory(NFace inputFace, NLine selectLine, int edgeNum, bool leftAnchor = true)
        {
            // moves nface at edge[edgenum] to line
            NFace insertFace = inputFace.DeepCopy();


            int numEdges = insertFace.edgeList.Count;

            int edgeNumEnd = edgeNum + 1;
            if (edgeNumEnd >= numEdges)
                edgeNumEnd = 0;


            Vec3d bottomLine = selectLine.end - selectLine.start;
            Vec3d rootLine = selectLine.end;

            // get bottom corner
            Vec3d bottomLeftV = insertFace.edgeList[edgeNum].v;
            Vec3d bottomRightV = insertFace.edgeList[edgeNumEnd].v;

            Vec3d bottomVec = bottomRightV - bottomLeftV;

            // rotate box to same angle
            double angleRad = Vec3d.Angle2PI_2d(bottomVec, bottomLine);
            double angleDeg = RMath.ToDegrees(angleRad);

            Vec3d axis = new Vec3d(0, 0, 1);

            Vec3d translationVec = rootLine;

            // move to 0,0,0
            Vec3d moveTo0 = Vec3d.Zero - bottomLeftV;
            insertFace.TranslateNFace(moveTo0);

            // create the rotation Quaternion
            //RGeoLib.Quaternion rotation = new RGeoLib.Quaternion(angleDeg, axis);

            // at 0,0,0 translate to target
            insertFace.RotateNFaceAroundAxisRad(axis, angleRad + Math.PI);
            insertFace.TranslateNFace(translationVec);

            double remainingLineLength = selectLine.Length - bottomVec.Mag;
            Vec3d newBottomLine = Vec3d.ScaleToLen(bottomLine, remainingLineLength);
            NLine restLine = new NLine(selectLine.start, selectLine.start + newBottomLine);

            // FLIP TO OTHER SIDE
            if (leftAnchor == false)
            {
                restLine.FlipLine();
                insertFace.TranslateNFace(restLine.Direction);
                NLine restLine2 = new NLine(selectLine.end, selectLine.end + restLine.Direction);
                restLine2.FlipLine();

                return new Tuple<NFace, NLine, double, Vec3d, Vec3d>(insertFace, restLine2, angleRad + Math.PI, moveTo0, translationVec + restLine.Direction);
            }
            else
                return new Tuple<NFace, NLine, double, Vec3d, Vec3d>(insertFace, restLine, angleRad + Math.PI, moveTo0, translationVec);
        }
        public static NFace transform(NFace inFace, double angle, Vec3d moveVec1, Vec3d moveVec2)
        {
            NFace inputFace = inFace.DeepCopyWithMID();
            inputFace.TranslateNFace(moveVec1);
            inputFace.RotateNFaceAroundAxisRad(Vec3d.UnitZ, angle);
            inputFace.TranslateNFace(moveVec2);
            return inputFace;
        }

        public void RotateNFaceAroundAxis(Vec3d axis, double angleDegrees)
        {
            for (int i = 0; i < this.edgeList.Count; i++)
            {
                this.edgeList[i].RotateNEdgeAroundAxis(axis, angleDegrees);
            }
        }

        public void RotateNFaceAroundAxisRad(Vec3d axis, double angleDegrees)
        {
            for (int i = 0; i < this.edgeList.Count; i++)
            {
                this.edgeList[i].RotateNEdgeAroundAxisRad(axis, angleDegrees);
            }
        }
        public static NFace RotateNFace3d(Vec3d axis, Vec3d point, double radAngle, NFace inputFace)
        {
            NFace outputFace = inputFace.DeepCopy();

            Vec3d vec0toP = point.DeepCopy();
            Vec3d vecPto0 = point.DeepCopy();
            vecPto0.Reverse();

            outputFace.TranslateNFace(vecPto0);
            outputFace.RotateNFaceAroundAxisRad(axis, radAngle);
            outputFace.TranslateNFace(vec0toP);

            return outputFace;
        }
        public void TranslateNFace(Vec3d moveVec)
        {
            for (int i = 0; i < this.edgeList.Count; i++)
            {
                this.edgeList[i].TranslateNEdge(moveVec);
            }
        }
        public void ScaleNFace(double scalingFactor)
        {
            Vec3d centroid = NFace.centroidInsideFace(this);
            centroid.Reverse();
            this.TranslateNFace(centroid);

            for (int i = 0; i < this.edgeList.Count; i++)
            {
                this.edgeList[i].v = this.edgeList[i].v * scalingFactor;
            }
            centroid.Reverse();

            this.TranslateNFace(centroid);
        }
        public void ScaleNFace(double scalingFactor, Vec3d centroid)
        {
            centroid.Reverse();
            this.TranslateNFace(centroid);

            for (int i = 0; i < this.edgeList.Count; i++)
            {
                this.edgeList[i].v = this.edgeList[i].v * scalingFactor;
            }
            centroid.Reverse();

            this.TranslateNFace(centroid);
        }
        public void ScaleNFaceToArea(double desiredArea)
        {
            double factor = Math.Sqrt(desiredArea / this.Area);
            this.ScaleNFace(factor);
        }
        public void ScaleNFaceToArea(double desiredArea, Vec3d centroid)
        {
            double factor = Math.Sqrt(desiredArea / this.Area);
            this.ScaleNFace(factor, centroid);
        }
 

        // this doesnt work somehow??? 
        //public void RotateNFaceAroundAxis2dPoint(Vec3d axis, double angleDegrees)
        //{
        //    for (int i = 0; i < this.edgeList.Count; i++)
        //    {
        //        this.edgeList[i].RotateNEdgeAround2dPoint(axis, angleDegrees);
        //    }
        //}

        public void shiftEdgesLeft()
        { 
            // moves list 1 forward, last edge start edge

            List<NEdge> tempEdges = new List<NEdge>();

            for (int i = 1; i < (this.edgeList.Count); i++)
            {
                tempEdges.Add(this.edgeList[i]);
            }
            tempEdges.Add(this.edgeList[0]);

            // Replace edglist and update connectivity
            this.edgeList = tempEdges;
            updateEdgeConnectivity();
        }
        public void shiftEdgesByInt(int shifter)
        {
            for (int i = 0; i < shifter; i++)
            {
                shiftEdgesLeft();
            }
        }
        public void shiftNQuadToAngle(double radAngle)
        {   
            // Works Only with Quads
            // rotates Quad so that the split goes in the direction away from the edge with angle angleRad
            //

            List<double> debugList = new List<double>();
            List<NEdge> faceEdges = new List<NEdge>();
            faceEdges.AddRange(this.edgeList);
            NFace tempFace = new NFace(faceEdges);

            Vec3d unitX = Vec3d.UnitX;
            unitX.Reverse();

            int startEdge = 0;
            //Vec3d reverseSort = new Vec3d(sortDir);
            //reverseSort.Reverse();
            tempFace.updateEdgeConnectivity();

            for (int j = 0; j < faceEdges.Count; j++)
            {
                Vec3d faceDirection = Vec3d.ScaleToLen(faceEdges[j].Direction, 1);
                double currentAngle = Vec3d.Angle(unitX, faceDirection);
                //debugList.Add(currentAngle);

                Vec3d vecFlipped = Vec3d.CrossProduct(unitX, faceEdges[j].Direction);
                //bool angleReversed = false;
                if (vecFlipped.Z < 0)
                {
                    //angleReversed = true;
                    currentAngle = (-currentAngle);
                }

                debugList.Add(currentAngle);
                if ((currentAngle + 0.005 >= radAngle) && (currentAngle - 0.005 <= radAngle))
                {
                    startEdge = j;
                }
            }
            tempFace.shiftEdgesByInt(startEdge + 3);

            //face.isReverse = false;
            this.edgeList = tempFace.edgeList;
            this.updateEdgeConnectivity();
        }
        public void shiftNTriToAngle(double angleRad)
        {
            // Works only with triangles
            // rotates Triangle so that the split goes in the direction away from the edge with angle angleRad

            // TRI START
            // go through each side direction print angle
            int startEdge = 0;
            Vec3d unitX = Vec3d.UnitX;
            unitX.Reverse();
            for (int i = 0; i < this.edgeList.Count; i++)
            {
                double currentAngle = Vec3d.Angle(unitX, this.edgeList[i].Direction);
                Vec3d vecFlipped = Vec3d.CrossProduct(unitX, this.edgeList[i].Direction);

                if ((currentAngle + 0.005 >= angleRad) && (currentAngle - 0.005 <= angleRad))
                {
                    // vector in direction
                    startEdge = i;
                    if (vecFlipped.Z < 0)
                    {
                        this.isReverse = true;
                    }

                }
                else if ((currentAngle + 0.005 >= Math.PI - angleRad) && (currentAngle - 0.005 <= Math.PI - angleRad))
                {
                    // vector in direction
                    startEdge = i;
                    this.isReverse = true;
                }
            }

            this.shiftEdgesByInt(startEdge + 2);
            this.updateEdgeConnectivity();
        }

        
        public void subdivideNFaceEdgeByRatioList(int edgeNum, List<double> ratios)
        {
            this.updateEdgeConnectivity();
            List<NEdge> dividedEdge = NEdge.subdivideNEdgeByRatioList(this.edgeList[edgeNum], ratios);
            this.edgeList.RemoveAt(edgeNum);
            this.edgeList.InsertRange(edgeNum, dividedEdge);
            this.updateEdgeConnectivity();
        }
        public static NFace subdivideNFaceEdgeByRatioList(NFace inputFace, int edgeNum, List<double> ratios)
        {
            NFace outputFace = inputFace.DeepCopy();
            outputFace.updateEdgeConnectivity();
            List<NEdge> dividedEdge = NEdge.subdivideNEdgeByRatioList(outputFace.edgeList[edgeNum], ratios);
            outputFace.edgeList.RemoveAt(edgeNum);
            outputFace.edgeList.InsertRange(edgeNum, dividedEdge);
            outputFace.updateEdgeConnectivity();
            return outputFace;
        }

        public void updateEdgeConnectivity()
        {
            // Assigns connectivity properties to edges, assumes clockwise order of edges in edgelist.
            for (int i = 0; i < this.edgeList.Count; i++)
            {
                // Wraps elements
                int i2 = i + 1;
                if (i2 >= this.edgeList.Count)
                {
                    i2 = 0;
                }

                int i0 = i - 1;
                if (i0 < 0)
                {
                    i0 = (this.edgeList.Count - 1);
                }

                this.edgeList[i].nextNEdge = this.edgeList[i2];
                this.edgeList[i].prevNEdge = this.edgeList[i0];
            }
        }
        public void UpdateNormal()
        {
            // Implement something better based on edges. works only for 2d now
            this.Normal = new Vec3d(0, 0, 1);
        }
        public void UpdateTempEnd()
        {
            for (int i = 0; i < this.edgeList.Count; i++)
            {
                this.edgeList[i].SetTempEnd();
            }
        }

        public static Tuple<NFace, NLine> moveNFaceToLine(NFace inputFace, NLine selectLine, int edgeNum = 0)
        {
            // moves nface at edge[ edgenum ] to line
            NFace insertFace = inputFace.DeepCopy();

            int numEdges = insertFace.edgeList.Count;

            if (edgeNum >= numEdges)
            {
                edgeNum = edgeNum % insertFace.edgeList.Count;
            }

            int edgeNumEnd = edgeNum + 1;
            if (edgeNumEnd >= numEdges)
                edgeNumEnd = 0;

            Vec3d bottomLine = selectLine.end - selectLine.start;
            Vec3d rootLine = selectLine.end;

            // get bottom corner
            Vec3d bottomLeftV = insertFace.edgeList[edgeNum].v;
            Vec3d bottomRightV = insertFace.edgeList[edgeNumEnd].v;

            Vec3d bottomVec = bottomRightV - bottomLeftV;


            //Vec3d rootVec = rootTuple.Item1;
            //Vec3d rootLine = selectLine.end;
            //C = boxHeight;

            // rotate box to same angle

            double angleRad = Vec3d.Angle2PI_2d(bottomVec, bottomLine);
            double angleDeg = RMath.ToDegrees(angleRad);

            Vec3d axis = new Vec3d(0, 0, 1);

            Vec3d translationVec = rootLine;

            // move to 0,0,0
            Vec3d moveTo0 = Vec3d.Zero - bottomLeftV;
            insertFace.TranslateNFace(moveTo0);


            // at 0,0,0 translate to target
            insertFace.RotateNFaceAroundAxisRad(axis, angleRad + Math.PI);
            insertFace.TranslateNFace(translationVec);


            //

            double remainingLineLength = selectLine.Length - bottomVec.Mag;
            Vec3d newBottomLine = Vec3d.ScaleToLen(bottomLine, remainingLineLength);
            NLine restLine = new NLine(selectLine.start, selectLine.start + newBottomLine);

            return new Tuple<NFace, NLine>(insertFace, restLine);
        }

        public NFace ConvexHullJarvis()
        {
            // returns face that is convex hull of this face

            // convert edges to vec list
            List<Vec3d> points = new List<Vec3d>();
            
            for (int i = 0; i < this.edgeList.Count; i++)
            {
                points.Add(this.edgeList[i].v);
            }

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
        // STATIC

        public static List<NLine> returnLinesFromNFace(NFace inputFace)
        {
            inputFace.updateEdgeConnectivity();
            List<NLine> lineList = new List<NLine>();

            for (int i = 0; i < inputFace.edgeList.Count; i++)
            {
                NLine tempLine = new NLine(inputFace.edgeList[i].v, inputFace.edgeList[i].nextNEdge.v);
                lineList.Add(tempLine);
            }
            return lineList;
        }
        public List<double> actualToRatio(List<double> listActual)
        {
            // Outputs splitting list of ratios for this face
            // based on     actual areas
            // input list: {20, 5, 50} (with face area 100) --> output list: {0.2, 0.05, 0.5}

            List<double> ratios = new List<double>();
            double faceArea = this.Area;

            for (int i = 0; i < listActual.Count; i++)
            {
                double faceAdd = Math.Abs(listActual[i]) / faceArea;
                if (listActual[i] > 0)
                {
                    faceAdd *= -1;
                }
                ratios.Add(faceAdd);
            }
            return ratios;
        }


        // Bounding Box
        
        public static NFace BoundingBox2d(NFace currentFace, int edgeNumber)
        {
            //rotate so that current edge is straight
            int currentEdge = edgeNumber;

            double rotAngle = Vec3d.Angle2PI_2d(Vec3d.UnitX, currentFace.edgeList[currentEdge].Direction);

            Vec3d axisVec = new Vec3d(0, 0, 1);

            NFace outputFace = RotateNFace3d(axisVec, currentFace.edgeList[currentEdge].nextNEdge.v, -rotAngle, currentFace);


            // fit rectangle around
            NFace tempFace = outputFace;
            // largest x value, largest y value is top right corner

            var maxX = tempFace.edgeList.OrderByDescending(item => item.v.X).First();
            var maxY = tempFace.edgeList.OrderByDescending(item => item.v.Y).First();
            var minX = tempFace.edgeList.OrderByDescending(item => item.v.X).Last();
            var minY = tempFace.edgeList.OrderByDescending(item => item.v.Y).Last();
            double maxXD = maxX.v.X;
            double maxYD = maxY.v.Y;
            double minXD = minX.v.X;
            double minYD = minY.v.Y;

            List<NEdge> tempBBEdges = new List<NEdge>();

            Vec3d topRightV = new Vec3d(maxXD, maxYD, 0);
            NEdge topRightE = new NEdge(topRightV);
            tempBBEdges.Add(topRightE);

            Vec3d topLeftV = new Vec3d(minXD, maxYD, 0);
            NEdge topLeftE = new NEdge(topLeftV);
            tempBBEdges.Add(topLeftE);

            Vec3d bottomLeftV = new Vec3d(minXD, minYD, 0);
            NEdge bottomLeftE = new NEdge(bottomLeftV);
            tempBBEdges.Add(bottomLeftE);

            Vec3d bottomRightV = new Vec3d(maxXD, minYD, 0);
            NEdge bottomRightE = new NEdge(bottomRightV);
            tempBBEdges.Add(bottomRightE);

            NFace BBNFace = new NFace(tempBBEdges);

            // transform BB back to location of NFAce
            NFace outputBB = RotateNFace3d(axisVec, currentFace.edgeList[currentEdge].nextNEdge.v, rotAngle, BBNFace);

            return outputBB;
        }
        public static NFace BoundingBox2dMinArea(NFace currentFace)
        {
            // input convex hull of object for best use 

            //Returns bounding box with smallest area

            List<NFace> BBFaceList = new List<NFace>();
            for (int i = 0; i < currentFace.edgeList.Count; i++)
            {
                NFace tempBB = BoundingBox2d(currentFace, i);
                BBFaceList.Add(tempBB);
            }
            NFace finalBB = BBFaceList.OrderByDescending(item => item.Area).First();
            return finalBB;
        }
        public static NFace BoundingBox2dMaxEdge(NFace currentFace)
        {
            // input convex hull of object for best use 

            // returns bounding box from largestEdge 
            int index = currentFace.edgeList.IndexOf(currentFace.edgeList.OrderByDescending(item => item.Length).First());

            NFace tempBB = BoundingBox2d(currentFace, index);
            return tempBB;
        }

        public static NLine BoundingBoxDirectionMainLine(NFace inputFace)
        {
            // returns median line of bb
            //
            //  x-----x      x..a..x
            //  |     |      .  |  .
            //  |     |  ->  .  |  .
            //  |     |      .  |  . 
            //  x-----x      x..a..x

            NFace BBLongest = NFace.BoundingBox2dMaxEdge(inputFace);


            Vec3d startV = Vec3d.Mid(BBLongest.edgeList[1].v, BBLongest.edgeList[2].v);
            Vec3d endV = Vec3d.Mid(BBLongest.edgeList[3].v, BBLongest.edgeList[0].v);
            NLine centerLine = new NLine(startV, endV);

            return centerLine;

        }
        public static NLine BoundingBoxDirectionSecondaryLine(NFace inputFace)
        {
            // returns median line of bb
            //
            //  x-----x      x.....x
            //  |     |      .     .
            //  |     |  ->  a-----a
            //  |     |      .     . 
            //  x-----x      x.....x

            NFace BBLongest = NFace.BoundingBox2dMaxEdge(inputFace);


            Vec3d startV = Vec3d.Mid(BBLongest.edgeList[0].v, BBLongest.edgeList[1].v);
            Vec3d endV = Vec3d.Mid(BBLongest.edgeList[2].v, BBLongest.edgeList[3].v);
            NLine centerLine = new NLine(startV, endV);

            return centerLine;

        }

        // Inside Rectangle
        public static List<NFace> getInsideRectsComplex(NFace bounds)
        {
            HashSet<double> doubleSet = new HashSet<double>(new RoundedDoubleEqualityComparer(2));
            bounds.updateEdgeConnectivity();
            for (int i = 0; i < bounds.edgeList.Count; i++)
            {
                Vec3d crossVec = Vec3d.CrossProduct(bounds.edgeList[i].Direction, Vec3d.UnitZ);
                double normAngle = Vec3d.Angle2PI_2d(Vec3d.UnitX, crossVec);
                doubleSet.Add(normAngle);
            }

            List<NFace> finalSplitFaces = new List<NFace>();

            for (int i = 0; i < doubleSet.Count; i++)
            {
                double normAngle = Vec3d.Angle2PI_2d(Vec3d.UnitX, bounds.edgeList[i].Direction);
                Tuple<int, NMesh, string> splitTuple = RSplit.SplitDirectionSingleDir_RAW(bounds, normAngle);

                NMesh tempSplitMesh = splitTuple.Item2;
                if (splitTuple.Item1 > 0)
                {

                    for (int j = 0; j < tempSplitMesh.faceList.Count; j++)
                    {
                        Tuple<int, NMesh> splitOppositeTuple = RSplit.SplitDirectionNMeshBoth_RAW(tempSplitMesh.faceList[j], normAngle + Math.PI * 0.5);

                        for (int k = 0; k < splitOppositeTuple.Item2.faceList.Count; k++)
                        {
                            if (splitOppositeTuple.Item2.faceList[k].Area > 0.1)
                            {
                                splitOppositeTuple.Item2.faceList[k].shrinkMax();
                                //if (isRectangle(splitOppositeTuple.Item2.faceList[k]))
                                finalSplitFaces.Add(splitOppositeTuple.Item2.faceList[k]);
                            }
                        }
                        //finalSplitFaces.AddRange(splitOppositeTuple.Item2.faceList);
                    }
                }
            }

            finalSplitFaces = finalSplitFaces
              .OrderBy(n => n.Area)
              .ToList();
            finalSplitFaces.Reverse();
            return finalSplitFaces;
        }
        public static Tuple<bool, NFace> returnInsideRect(NFace bounds)
        {
            List<NFace> rectsList = new List<NFace>();
            bounds.updateEdgeConnectivity();

            if (isRectangle(bounds) == false)
            {
                List<NFace> insideRects = getInsideRectsComplex(bounds);
                rectsList.AddRange(insideRects);
            }
            else
            {
                NFace convexHull = bounds.ConvexHullJarvis();
                NFace BB = NFace.BoundingBox2d(convexHull, 0);
                rectsList.Add(BB);
            }
            // FLIP
            if (rectsList.Count == 0)
            {
                bounds.flipRH();
                if (isRectangle(bounds) == false)
                {
                    List<NFace> insideRects = getInsideRectsComplex(bounds);
                    rectsList.AddRange(insideRects);
                }
                else
                {
                    NFace convexHull = bounds.ConvexHullJarvis();
                    NFace BB = NFace.BoundingBox2d(convexHull, 0);
                    rectsList.Add(BB);
                }
            }
            if (rectsList.Count == 0)
                return new Tuple<bool, NFace>(false, bounds);
            else
                return new Tuple<bool, NFace>(true, rectsList[0]);
        }
        public static Tuple<bool, List<NFace>> returnInsideRectList(NFace bounds)
        {
            List<NFace> rectsList = new List<NFace>();

            if (isRectangle(bounds) == false)
            {
                List<NFace> insideRects = getInsideRectsComplex(bounds);
                rectsList.AddRange(insideRects);
            }
            else
            {
                NFace convexHull = bounds.ConvexHullJarvis();
                NFace BB = NFace.BoundingBox2d(convexHull, 0);
                rectsList.Add(BB);
            }
            // FLIP
            if (rectsList.Count == 0)
            {
                bounds.flipRH();
                if (isRectangle(bounds) == false)
                {
                    List<NFace> insideRects = getInsideRectsComplex(bounds);
                    rectsList.AddRange(insideRects);
                }
                else
                {
                    NFace convexHull = bounds.ConvexHullJarvis();
                    NFace BB = NFace.BoundingBox2d(convexHull, 0);
                    rectsList.Add(BB);
                }
            }
            if (rectsList.Count == 0)
            {
                rectsList.Add(bounds);
                return new Tuple<bool, List<NFace>>(false, rectsList);
            }
            else
                return new Tuple<bool, List<NFace>>(true, rectsList);
        }

        public static bool facesMinSize_ALL(List<NFace> inputFaces, double minArea, double minWidth)
        {
            bool minSizeBool = true;

            for (int i = 0; i < inputFaces.Count; i++)
            {
                bool tempBool = faceMinSize(inputFaces[i], minArea, minWidth);
                if (tempBool == false)
                    minSizeBool = false;
            }
            return minSizeBool;
        }
        public static bool facesMinSize_MinOne(List<NFace> inputFaces, double minArea, double minWidth)
        {
            bool minSizeBool = false;

            for (int i = 0; i < inputFaces.Count; i++)
            {
                bool tempBool = faceMinSize(inputFaces[i], minArea, minWidth);
                if (tempBool == true)
                    minSizeBool = true;
            }
            return minSizeBool;
        }
        public static bool faceMinSize(NFace inputFace, double minArea, double minWidth)
        {
            bool minAreaBool = false;

            bool minWidthBool = true;


            NFace face0 = inputFace.DeepCopy();

            if (face0.Area >= minArea)
                minAreaBool = true;

            NFace convexHull = face0.ConvexHullJarvis();

            NFace BB = NFace.BoundingBox2d(convexHull, 0);

            NLine main = NFace.BoundingBoxDirectionMainLine(convexHull);
            NLine second = NFace.BoundingBoxDirectionSecondaryLine(convexHull);

            if (main.Length < minWidth)
                minWidthBool = false;

            if (second.Length < minWidth)
                minWidthBool = false;

            if (minAreaBool == true && minWidthBool == true)
                return true;
            else
                return false;
        }


        // Orthogonalize
        public static NFace orthoFaceLoop(NFace inputFace, Vec3d dirVec)
        {
            List<NEdge> outList = new List<NEdge>();

            for (int i = 0; i < inputFace.edgeList.Count; i++)
            {
                List<NEdge> tempList = orthoFaceEdgeReturn(inputFace, i, dirVec);
                outList.AddRange(tempList);
            }

            NFace faceOut = new NFace(outList);
            faceOut.shrinkFace();
            return faceOut;
        }
        public static List<NEdge> orthoFaceEdgeReturn(NFace inputFace, int edgeInt, Vec3d dirVec)
        {
            double tolerance = 0.1;
            NFace face0 = inputFace.DeepCopy();
            face0.updateEdgeConnectivity();
            //NEdge editNEdge = face0.edgeList[edgeInt];
            List<NEdge> edgeList0 = face0.edgeList;

            Vec3d endVec = face0.edgeList[edgeInt].nextNEdge.v;
            Vec3d midVec = Vec3d.Mid(face0.edgeList[edgeInt].v, face0.edgeList[edgeInt].nextNEdge.v);
            Vec3d startVec = face0.edgeList[edgeInt].v;

            // rechtwinkliges dreieck aus c and b_dir
            Vec3d c = midVec - startVec;

            double beta = Vec3d.Angle2PI_2d(dirVec.GetReverse(), face0.edgeList[edgeInt].Direction);
            double alpha = (Math.PI / 2) - beta;

            double len_b = Math.Sin(alpha) * c.Mag;

            Vec3d ptB = midVec + (Vec3d.ScaleToLen(dirVec, len_b));
            Vec3d ptC = midVec - (Vec3d.ScaleToLen(dirVec, len_b));



            List<NEdge> edgesOut = new List<NEdge>();
            Vec3d baseVec = startVec.DeepCopy();
            NEdge outBase = new NEdge(baseVec);
            edgesOut.Add(outBase);

            if (Vec3d.Distance(ptB, ptC) < tolerance)
            {
                return edgesOut;
                //return returnFace;
            }
            else
            {

                NEdge eB = new NEdge(ptB);
                NEdge eC = new NEdge(ptC);

                if (Vec3d.Distance(startVec, ptB) > tolerance && Vec3d.Distance(endVec, ptB) > tolerance)
                    edgesOut.Add(eB);

                if (Vec3d.Distance(startVec, ptC) > tolerance && Vec3d.Distance(endVec, ptC) > tolerance)
                    edgesOut.Add(eC);

                return edgesOut;
            }
        }
        public static NFace orthoFace(NFace inputFace, int edgeInt, Vec3d dirVec)
        {
            if (edgeInt >= inputFace.edgeList.Count)
                return inputFace;

            NFace face0 = inputFace.DeepCopy();
            face0.updateEdgeConnectivity();
            //NEdge editNEdge = face0.edgeList[edgeInt];
            List<NEdge> edgeList0 = face0.edgeList;

            Vec3d endVec = face0.edgeList[edgeInt].nextNEdge.v;
            Vec3d midVec = Vec3d.Mid(face0.edgeList[edgeInt].v, face0.edgeList[edgeInt].nextNEdge.v);
            Vec3d startVec = face0.edgeList[edgeInt].v;

            // rechtwinkliges dreieck aus c and b_dir
            Vec3d c = midVec - startVec;

            double beta = Vec3d.Angle2PI_2d(dirVec.GetReverse(), face0.edgeList[edgeInt].Direction);
            double alpha = (Math.PI / 2) - beta;

            double len_b = Math.Sin(alpha) * c.Mag;

            Vec3d ptB = midVec + (Vec3d.ScaleToLen(dirVec, len_b));
            Vec3d ptC = midVec - (Vec3d.ScaleToLen(dirVec, len_b));



            // check if actually something was created, if yes return
            if (Vec3d.Distance(ptB, ptC) < 0.0001)
            {
                return inputFace;
                //return returnFace;
            }
            else
            {
                NEdge eB = new NEdge(ptB);
                NEdge eC = new NEdge(ptC);

                if (Vec3d.Distance(startVec, ptB) > 0.0001 && Vec3d.Distance(endVec, ptB) > 0.0001)
                    edgeList0.Insert(edgeInt + 1, eB);

                if (Vec3d.Distance(startVec, ptC) > 0.0001 && Vec3d.Distance(endVec, ptC) > 0.0001)
                    edgeList0.Insert(edgeInt + 2, eC);

                NFace returnFace = new NFace(edgeList0);

                returnFace.updateEdgeConnectivity();

                return returnFace;
            }

        }


        // Serialize

        public static string serializeNFace(NFace inputFace)
        {
            JFace inputJFace = new JFace(inputFace);
            string jsonString = JsonSerializer.Serialize(inputJFace);
            return jsonString;
        }
        public static NFace deserializeNFace(string jsonString)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                IncludeFields = true
            };

            JFace reverseNode = JsonSerializer.Deserialize<JFace>(jsonString, options);
            NFace outFace = reverseNode.returnNFace();
            return outFace;
        }

        public override string ToString()
        {
            return $"NFace[NumEdges: {this.edgeList.Count}]";
        }
    }
    class RoundedDoubleEqualityComparer : IEqualityComparer<double>
    {
        private int decimalPlaces;

        public RoundedDoubleEqualityComparer(int decimalPlaces)
        {
            this.decimalPlaces = decimalPlaces;
        }

        public bool Equals(double x, double y)
        {
            double roundedX = Math.Round(x, decimalPlaces);
            double roundedY = Math.Round(y, decimalPlaces);
            return roundedX.Equals(roundedY);
        }

        public int GetHashCode(double obj)
        {
            double roundedValue = Math.Round(obj, decimalPlaces);
            return roundedValue.GetHashCode();
        }
    }


}
