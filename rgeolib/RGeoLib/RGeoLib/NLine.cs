using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;



namespace RGeoLib
{
    [Serializable]
    public class NLine
    {
        public Vec3d start { get; set; }
        public Vec3d end { get; set; }

        public List<NLine> connectingStart;
        public List<NLine> connectingEnd;

        public bool isIntersecting = false;
        public bool IsProcessed { get; set; } = false;
        public string property { get; set; }

        public NLine(Vec3d v1, Vec3d v2)
        {
            this.start = v1;
            this.end = v2;
        }

        public Vec3d Direction
        {
            get
            {
                Vec3d dir = new Vec3d(this.end - this.start);
                return dir;
            }
        }

        public Vec3d MidPoint
        {
            get 
            {
                return Vec3d.Mid(this.start, this.end);
            }
        }

        public double Length
        {
            get
            {
                Vec3d dir = new Vec3d(this.end - this.start);
                return dir.Mag;
            }
        }

        public NLine DeepCopy()
        {
            Vec3d startCopy = this.start.DeepCopy();
            Vec3d endCopy = this.end.DeepCopy();

            NLine tempLine = new NLine(startCopy, endCopy);
            return tempLine;
        }
        public static List<NLine> DeepCopyList(List<NLine> inputLines)
        {
            List<NLine> linesTemp = new List<NLine>();

            for (int i = 0; i < inputLines.Count; i++)
            {
                NLine tempL = inputLines[i].DeepCopy();
                linesTemp.Add(tempL);
            }
            return linesTemp;
        }
        public void DoLinesConnect(NLine other)
        {
            double tolDist = Constants.IntersectTolerance;

            bool isequal = NLine.IsNLineEqual(this, other);

            if (isequal == false)
            {
                // checks if line connects to line 
                bool adjacentStart = false;
                bool adjacentEnd = false;

                // if connect to start, add to start line list,
                double distStartStart = Vec3d.Distance(this.start, other.start);
                double distStartEnd = Vec3d.Distance(this.start, other.end);

                if ((distStartStart < tolDist) || (distStartEnd < tolDist))
                {
                    this.connectingStart.Add(other);
                    adjacentStart = true;
                }

                // if connect to end, add to end line list
                double distEndEnd = Vec3d.Distance(this.end, other.end);
                double distEndStart = Vec3d.Distance(this.end, other.start);

                if ((distEndEnd < tolDist) || (distEndStart < tolDist))
                {
                    this.connectingEnd.Add(other);
                    adjacentEnd = true;
                }

            }
        }
        public static bool AreLinesConnected(NLine line1, NLine line2)
        {
            double tolDist = 0.001;

            return (Vec3d.Distance(line1.end, line2.start) < tolDist) ||
              (Vec3d.Distance(line1.start, line2.end) < tolDist) ||
              (Vec3d.Distance(line1.end, line2.end) < tolDist) ||
              (Vec3d.Distance(line1.start, line2.start) < tolDist);
        }
        public static bool IsNLineEqual(NLine x, NLine y)
        {
            // Compares two edges and returns true if they are the same
            bool adjacentBool = false;
            double tolDist = Constants.IntersectTolerance;

            double distEndEnd = Vec3d.Distance(x.end, y.end);
            double distStartStart = Vec3d.Distance(x.start, y.start);

            double distStartEnd = Vec3d.Distance(x.start, y.end);
            double distEndStart = Vec3d.Distance(x.end, y.start);

            if (((distEndEnd < tolDist) && (distStartStart < tolDist)) || ((distStartEnd < tolDist) && (distEndStart < tolDist)))
            {
                adjacentBool = true;
            }

            return adjacentBool;
        }
        public static bool IsNLineEqualTol(NLine x, NLine y, double tolDist)
        {
            // Compares two edges and returns true if they are the same
            bool adjacentBool = false;

            double distEndEnd = Vec3d.Distance(x.end, y.end);
            double distStartStart = Vec3d.Distance(x.start, y.start);

            double distStartEnd = Vec3d.Distance(x.start, y.end);
            double distEndStart = Vec3d.Distance(x.end, y.start);

            if (((distEndEnd < tolDist) && (distStartStart < tolDist)) || ((distStartEnd < tolDist) && (distEndStart < tolDist)))
            {
                adjacentBool = true;
            }

            return adjacentBool;
        }

        public static NLine extendLine(NLine inputLine, double newLength)
        {
            Vec3d start = inputLine.start.DeepCopy();
            Vec3d dir = inputLine.Direction.DeepCopy();

            Vec3d dirNew = Vec3d.ScaleToLen(dir, newLength);

            return new NLine(start, start + dirNew);
        }

        public bool minLength(double length)
        {
            // checks if at least of length length

            bool hasLength = false;

            if (this.Length >= length)
                    hasLength = true;
            
            return hasLength;
        }
        public static bool minLength(List<NLine> inputLines, double length)
        {
            // checks if at least one of the lines is of length length

            bool hasLength = false;

            for (int i = 0; i < inputLines.Count; i++)
            {
                if (inputLines[i].Length >= length)
                    hasLength = true;
            }
            return hasLength;
        }
        
        // Points
        public Vec3d PointAt(double ratio)
        {
            double x1 = this.start.X;
            double y1 = this.start.Y;
            double z1 = this.start.Z;

            double x2 = this.end.X;
            double y2 = this.end.Y;
            double z2 = this.end.Z;

            double xp = x1 + ratio * (x2 - x1);
            double yp = y1 + ratio * (y2 - y1);
            double zp = z1 + ratio * (z2 - z1);

            Vec3d insertVec = new Vec3d(xp, yp, zp);

            return insertVec;
        }

        public List<Vec3d> GetPoints()
        {
            List<Vec3d> vecs = new List<Vec3d>();
            vecs.Add(this.start);
            vecs.Add(this.end);
            return vecs;
        }
        public static List<Vec3d> GetPoints(List<NLine> inputLines)
        {
            List<Vec3d> vecs = new List<Vec3d>();
            for (int i = 0; i < inputLines.Count; i++)
            {
                vecs.Add(inputLines[i].start);
                vecs.Add(inputLines[i].end);
            }
            vecs = Vec3d.RemoveDuplicatePoints(vecs, 0.001);
            return vecs;
        }

        //Flip Line
        public void FlipLine()
        {
            Vec3d temp = this.start;

            this.start = this.end;

            this.end = temp;
           
        }
        public void FlipLineWithhSwap()
        {
            Vec3d temp = this.start;

            this.start = this.end;

            this.end = temp;


            List<NLine> tempStart = this.connectingStart;
            List<NLine> tempEnd = this.connectingEnd;

            this.connectingStart = tempEnd;
            this.connectingEnd = tempStart;
        }

        public static List<NLine> conformLines(List<NLine> inputLines)
        {
            // DEprecated use conform lines new

            // reference direction
            if (inputLines.Count > 1)
            {
                List<NLine> outputLines = new List<NLine>();

                Vec3d referenceDirection = inputLines[1].MidPoint - inputLines[0].MidPoint;

                for (int i = 0; i < inputLines.Count; i++)
                {
                    NLine line = inputLines[i].DeepCopy();
                    Vec3d direction = line.Direction;

                    // If the direction of the line is opposite to the reference direction,
                    // then swap the start and end points of the line
                    if (Vec3d.DotProduct(direction, referenceDirection) < 0)
                    {
                        Vec3d temp = line.start;
                        line.start = line.end;
                        line.end = temp;
                    }
                    outputLines.Add(line);
                }
                return outputLines;
            }
            return inputLines;
        }
        public static List<NLine> ConformLinesNew(List<NLine> inputLines)
        {
            if (inputLines.Count <= 1)
                return inputLines;

            List<NLine> outputLines = new List<NLine>
      {
        inputLines[0].DeepCopy() // Add the first line as is
        };

            for (int i = 0; i < inputLines.Count - 1; i++)
            {
                NLine currentLine = outputLines[i];
                NLine nextLine = inputLines[i + 1].DeepCopy();

                if (!AreLinesConnected(currentLine, nextLine))
                {
                    // If current and next lines are not connected, flip the next line
                    nextLine.FlipLineWithhSwap();
                }

                if (currentLine.start.Equals(nextLine.start) || currentLine.end.Equals(nextLine.end))
                {
                    // If lines meet start-to-start or end-to-end, flip the next line
                    nextLine.FlipLineWithhSwap();
                }

                outputLines.Add(nextLine);
            }

            return outputLines;
        }

        // Grouping methods
        public static List<List<NLine>> GroupConnectedLines(List<NLine> lines)
        {
            var groupedLines = new List<List<NLine>>();
            var visited = new HashSet<NLine>();

            foreach (var line in lines)
            {
                if (!visited.Contains(line))
                {
                    var group = new List<NLine>();
                    GroupLinesRecursive(line, lines, group, visited);
                    groupedLines.Add(group);
                }
            }

            return groupedLines;
        }
        private static void GroupLinesRecursive(NLine currentLine, List<NLine> allLines, List<NLine> currentGroup, HashSet<NLine> visited)
        {
            visited.Add(currentLine);
            currentGroup.Add(currentLine);

            foreach (var line in allLines)
            {
                if (!visited.Contains(line) && AreLinesConnected(currentLine, line))
                {
                    GroupLinesRecursive(line, allLines, currentGroup, visited);
                }
            }
        }


        public static List<NLine> simplifyLines(List<NLine> inputLines)
        {
            List<NPolyLine> polyOut = NPolyLine.simplifyIntoPolyLines(inputLines);
            List<NLine> outLines = new List<NLine>();

            for (int i = 0; i < polyOut.Count; i++)
            {
                outLines.AddRange(polyOut[i].lineList);
            }
            return outLines;
        }

        public static NLine inbetweenLine(NLine lineA, NLine lineB, double ratio)
        {
            // make sure lines go in opposite directions!
            NLine direction = lineA;
            NLine direction2 = lineB;

            NLine topLine = new NLine(direction.start, direction2.end);
            NLine botLine = new NLine(direction.end, direction2.start);

            Vec3d startL = topLine.PointAt(ratio);
            Vec3d endL = botLine.PointAt(ratio);

            NLine outline = new NLine(startL, endL);
            return outline;
        }

        // Shatter duplicate combine
        public static NLine combineAxisLines(List<NLine> inputLines, Axis inputAxis)
        {
            // input
            List<NLine> uniqueLineList = getUniqueLines(inputLines);

            NPolyLine tempMainLine = joinNLinesInAxis(uniqueLineList, inputAxis);

            NLine tempEndsLine = new NLine(tempMainLine.start, tempMainLine.end);
            return tempEndsLine;
        }
        public static List<NLine> deleteDuplicateLines(List<NLine> inputLines)
        {
            double dupTol = 0.01;
            List<NLine> outLines = new List<NLine>();
            for (int i = 0; i < inputLines.Count; i++)
            {
                bool isDuplicate = false;
                bool isValid = true;
                for (int j = 0; j < outLines.Count; j++)
                {
                    if (NLine.IsNLineEqualTol(inputLines[i], outLines[j], dupTol))
                        isDuplicate = true;
                }
                if (Vec3d.Distance(inputLines[i].start, inputLines[i].end) < dupTol)
                    isValid = false;

                if (isDuplicate == false && isValid == true)
                    outLines.Add(inputLines[i]);
            }
            return outLines;
        }

        public static List<NLine> deleteDuplicateLinesALL(List<NLine> inputLines)
        {
            // deletes all duplicate lines
            // attention, could create faces with holes!! this function does not care. use shatter and shrink face max instead when dealing with faces. 

            double dupTol = 0.001;
            List<int> duplicateInts = new List<int>();

            List<NLine> falseLines = new List<NLine>();
            List<NLine> trueLines = new List<NLine>();

            for (int i = 0; i < inputLines.Count; i++)
            {
                bool isDuplicate = false;
                for (int j = 0; j < inputLines.Count; j++)
                {
                    if (i != j && duplicateInts.Contains(i) == false && duplicateInts.Contains(j) == false)
                    {
                        if (NLine.IsNLineEqualTol(inputLines[i], inputLines[j], dupTol))
                        {
                            isDuplicate = true;
                        }

                    }
                }
                if (isDuplicate == false)
                    trueLines.Add(inputLines[i]);
                else
                    falseLines.Add(inputLines[i]);
            }
            return trueLines;
        }
        public static List<NPolyLine> shatterLines(List<NLine> inputLines)
        {
            List<NPolyLine> polyLines = new List<NPolyLine>();
            for (int i = 0; i < inputLines.Count; i++)
            {
                string tempProperty = inputLines[i].property;
                List<Vec3d> intersectionPoints = new List<Vec3d>();
                intersectionPoints.Add(inputLines[i].start);
                for (int j = 0; j < inputLines.Count; j++)
                {
                    if (i != j)
                    {
                        NLine lineA = inputLines[i];
                        NLine lineB = inputLines[j];

                        bool isStartIntersecting = RIntersection.PointLineIntersection(lineB.start, lineA.start, lineA.end);
                        if (isStartIntersecting)
                            intersectionPoints.Add(lineB.start);

                        bool isEndIntersecting = RIntersection.PointLineIntersection(lineB.end, lineA.start, lineA.end);
                        if (isEndIntersecting)
                            intersectionPoints.Add(lineB.end);

                    }
                }
                intersectionPoints.Add(inputLines[i].end);

                List<Vec3d> orderedPoints = intersectionPoints.OrderBy(pt => Vec3d.Distance(pt, inputLines[i].start)).ToList(); // inputAxis.v

                NPolyLine tempPoly = new NPolyLine(orderedPoints);
                tempPoly.property = tempProperty;
                for (int r = 0; r < tempPoly.lineList.Count; r++)
                {
                    tempPoly.lineList[r].property = tempProperty;
                }

                if (tempPoly.lineList.Count > 1)
                    polyLines.Add(tempPoly);
            }
            return polyLines;
        }
        public static NLine snapLineToNFace(NLine inputLine, NFace inputFace, double tolerance)
        {
            // sees if line ends are in tolerance to face vertices, if yes, snaps to them
            // picks first in tolerance

            // get all vertices
            List<Vec3d> snapListStart = new List<Vec3d>();
            List<Vec3d> snapListEnd = new List<Vec3d>();

            for (int i = 0; i < inputFace.edgeList.Count; i++)
            {
                snapListStart.Add(inputFace.edgeList[i].v);
                snapListEnd.Add(inputFace.edgeList[i].v);
            }

            Vec3d startPoint = inputLine.start;
            Vec3d endPoint = inputLine.end;

            List<Vec3d> SortedStart = snapListStart.OrderBy(o => Vec3d.Distance(startPoint, o)).ToList();
            List<Vec3d> SortedEnd = snapListEnd.OrderBy(o => Vec3d.Distance(endPoint, o)).ToList();

            if (Vec3d.Distance(startPoint, SortedStart[0]) < tolerance)
                startPoint = SortedStart[0];

            if (Vec3d.Distance(endPoint, SortedEnd[0]) < tolerance)
                endPoint = SortedEnd[0];

            return new NLine(startPoint, endPoint);
        }

        public static List<NLine> getUniqueLines(List<NLine> inputLines)
        {
            // input list of lines can be overlapping! 

            // 1 shatter lines
            List<NPolyLine> tempPolyLines = shatterLines(inputLines);

            // 2 explode polylines
            List<NLine> tempLinesAll = new List<NLine>();
            for (int i = 0; i < tempPolyLines.Count; i++)
            {
                tempLinesAll.AddRange(tempPolyLines[i].lineList);
            }

            // 3 delete duplicate lines

            List<NLine> delDups = deleteDuplicateLines(tempLinesAll);


            return delDups;
        }
        public static List<NLine> deleteDuplicateRKHT(List<NLine> inputLines)
        {
            double dupTol = 0.01;
            // Define the output list and the Rabin-Karp hash table
            List<NLine> outLines = new List<NLine>();
            RabinKarpHashTableLine<NLine> hashTable = new RabinKarpHashTableLine<NLine>(dupTol);
            foreach (NLine line in inputLines)
            {
                // Check if the line is valid and not a duplicate
                if (Vec3d.Distance(line.start, line.end) >= dupTol && !hashTable.Contains(line))
                {
                    // Add the line to the output list and the hash table
                    outLines.Add(line);
                    hashTable.Add(line);
                }
            }
            // Return the output lines
            return outLines;
        }

        private static NPolyLine joinNLinesInAxis(List<NLine> inputLines, Axis inputAxis)
        {
            // joins lines at ends if possible
            // use get unique lines first to delete overlaps!
            double joinTolerance = 0.01;

            List<NLine> sortedLines = inputLines.OrderBy(ln => Vec3d.Distance(ln.MidPoint, inputAxis.v)).ToList();

            // flip axis
            Vec3d orthoVec = Vec3d.CrossProduct(inputAxis.dir, Vec3d.UnitZ);
            for (int i = 0; i < sortedLines.Count; i++)
            {
                Vec3d currentOrtho = Vec3d.CrossProduct(orthoVec, sortedLines[i].Direction);

                if (currentOrtho.Z >= 0)
                {
                    sortedLines[i].FlipLine();
                }
            }

            List<NLine> joinedLines = new List<NLine>();

            for (int i = 0; i < sortedLines.Count; i++)
            {
                if (i == 0)
                    joinedLines.Add(sortedLines[0]);
                else
                {
                    double distStartStart = Vec3d.Distance(sortedLines[i].start, joinedLines[0].start);
                    if (distStartStart < joinTolerance)
                    {
                        inputLines[i].FlipLine();
                        joinedLines.Insert(0, sortedLines[i]);
                    }

                    double distStartEnd = Vec3d.Distance(sortedLines[i].start, joinedLines.Last().end);
                    if (distStartEnd < joinTolerance)
                        joinedLines.Add(sortedLines[i]);

                    double distEndStart = Vec3d.Distance(sortedLines[i].end, joinedLines[0].start);
                    if (distEndStart < joinTolerance)
                    {
                        joinedLines.Insert(0, sortedLines[i]);
                    }

                    double distEndEnd = Vec3d.Distance(sortedLines[i].end, joinedLines.Last().end);
                    if (distEndEnd < joinTolerance)
                    {
                        inputLines[i].FlipLine();
                        joinedLines.Add(sortedLines[i]);
                    }

                }
            }

            NPolyLine combinedLines = new NPolyLine(joinedLines);
            //NLine combinedOutLine = new NLine(combinedLines[0].start, combinedLines[0].end);
            return combinedLines;
        }
        public static NLine trimLine(NLine line1, NLine line2)
        {
            NLine outLine = line1;
            if (RIntersection.AreLinesIntersecting(line1.start, line1.end, line2.start, line2.end, true))
            {
                Vec3d intersectionPt = RIntersection.GetLineLineIntersectionPoint(line1.start, line1.end, line2.start, line2.end);

                outLine = new NLine(line1.start, intersectionPt);
            }
            return outLine;
        }


        public static Tuple<double, bool> getSmallestAngle(NLine line0, Vec3d angleVec)
        {
            // returns angle closest to zero angle with flip of curve

            List<double> angles = new List<double>();

            NLine inputLine = line0.DeepCopy();
            inputLine.FlipLine();
            double angleTot2 = Vec3d.Angle2PI_2d(inputLine.Direction, Vec3d.UnitX);
            double angleOut = (Math.PI - angleTot2);

            bool reversed = false;
            if (angleOut < -0.00001)
            {
                angleOut += Math.PI;
                reversed = true;
            }
            if (angleOut >= Math.PI-0.0001)
            { 
                angleOut -= Math.PI;
                reversed = true;
            }

            return new Tuple<double, bool>(angleOut, reversed);

        }

        // Duplicate Lines (no shatter)

        public static List<NLine> RemoveDuplicateLines(List<NLine> lines, double tolerance)
        {
            HashSet<Tuple<Vec3d, Vec3d>> lineSet = new HashSet<Tuple<Vec3d, Vec3d>>(new Vec3dTupleEqualityComparer(tolerance));
            List<NLine> uniqueLines = new List<NLine>();

            foreach (NLine line in lines)
            {
                Tuple<Vec3d, Vec3d> tuple = new Tuple<Vec3d, Vec3d>(line.start, line.end);

                if (!lineSet.Contains(tuple))
                {
                    lineSet.Add(tuple);
                    uniqueLines.Add(line);
                }
            }

            return uniqueLines;
        }


        // Transformation 
        public static List<NLine> transform(List<NLine> inLines, double angle, Vec3d moveVec1, Vec3d moveVec2)
        {
            List<NLine> outLines = new List<NLine>();

            for (int i = 0; i < inLines.Count; i++)
            {
                NLine tempLine = inLines[i].DeepCopy();

                tempLine.TranslateNLine(moveVec1);
                tempLine.RotateNLineAroundAxisRad(Vec3d.UnitZ, angle);
                tempLine.TranslateNLine(moveVec2);

                outLines.Add(tempLine);
            }
            return outLines;
        }
        public void TranslateNLine(Vec3d moveVec)
        {
            this.start += moveVec;
            this.end += moveVec;
        }
        public static NLine TranslateNLine(Vec3d moveVec, NLine lineToTranslate)
        {
            Vec3d startNew = lineToTranslate.start += moveVec;
            Vec3d endNew = lineToTranslate.end += moveVec;

            NLine tempLine = new NLine(startNew, endNew);
            return tempLine;
        }
        public static List<NLine> TranslateNLineList(Vec3d moveVec, List<NLine>inputLines)
        {
            List<NLine> outputLines= new List<NLine>();

            for (int i = 0; i < inputLines.Count; i++)
            {
                NLine tempLine = TranslateNLine(moveVec, inputLines[i]);
                outputLines.Add(tempLine);
            }
            return outputLines;
        }

        public void RotateNLineAroundAxisRad(Vec3d axis, double angleRad)
        {
            Vec3d tempVecStart = Vec3d.RotateAroundAxisRad(this.start, axis, angleRad);
            Vec3d tempVecEnd = Vec3d.RotateAroundAxisRad(this.end, axis, angleRad);

            this.start = tempVecStart;
            this.end = tempVecEnd;
        }
        public static NLine RotateNLine3d(Vec3d axis, Vec3d point, double radAngle, NLine inputLine)
        {
            NLine outputLine = inputLine.DeepCopy();

            Vec3d vec0toP = point.DeepCopy();
            Vec3d vecPto0 = point.DeepCopy();
            vecPto0.Reverse();

            outputLine.TranslateNLine(vecPto0);
            outputLine.RotateNLineAroundAxisRad(axis,radAngle);
            outputLine.TranslateNLine(vec0toP);

            return outputLine;
        }
        public static List<NLine> RotateNLineList3d(Vec3d axis, Vec3d point, double radAngle, List<NLine> inputLineList)
        {
            List<NLine> outputLines = new List<NLine>();

            for (int i = 0; i < inputLineList.Count; i++)
            {
                NLine outputLine = inputLineList[i].DeepCopy();

                Vec3d vec0toP = point.DeepCopy();
                Vec3d vecPto0 = point.DeepCopy();
                vecPto0.Reverse();

                outputLine.TranslateNLine(vecPto0);
                outputLine.RotateNLineAroundAxisRad(axis, radAngle);
                outputLine.TranslateNLine(vec0toP);

                outputLines.Add(outputLine);
            }

            return outputLines;
            
        }

        // Arrays

        public static List<NLine> polarNLineArray(Vec3d centroid, Vec3d axis, NLine arrayLine, int numLines)
        {
            double angleEach = (2 * Math.PI) / numLines;
            List<NLine> arrayLines = new List<NLine>();
            for (int i = 0; i < numLines; i++)
            {
                NLine tempLine = NLine.RotateNLine3d(axis, centroid, angleEach * i, arrayLine);
                arrayLines.Add(tempLine);
            }

            return arrayLines;
        }

        // Serialize Deserialize (via JLine)
        public string serializeNLine()
        {   
            JLine tempJLine = new JLine(this.start, this.end);
            string jsonString = JsonSerializer.Serialize(tempJLine);
            return jsonString;
        }
        public static NLine deserializeNLine(string jsonString)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                IncludeFields = true
            };

            JLine reverseNode = JsonSerializer.Deserialize<JLine>(jsonString, options);
            NLine outLine = reverseNode.returnNLine();
            return outLine;
        }

        public static string serializeNLineList (List<NLine> inputList)
        {
            List<JLine> tempList = new List<JLine>();

            for (int i = 0; i < inputList.Count; i++)
            {
                JLine tempJLine = new JLine(inputList[i].start, inputList[i].end);
                tempList.Add(tempJLine);
            }
            string jsonString = JsonSerializer.Serialize(tempList);
            return jsonString;
        }
        public static List<NLine> deserializeNLineList(string jsonString)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                IncludeFields = true
            };

            List<JLine> reverseNode = JsonSerializer.Deserialize<List<JLine>>(jsonString, options);
            List<NLine> outList = new List<NLine>();

            for (int i = 0; i < reverseNode.Count; i++)
            {
                NLine outLine = reverseNode[i].returnNLine();
                outList.Add(outLine);
            }
            return outList;
        }

        // Ortho
        public static List<NLine> orthoLineReturnNeg(NLine inputLine)
        {
            Vec3d startVec = inputLine.start;
            Vec3d midVec = inputLine.start.DeepCopy();
            Vec3d midVec2 = inputLine.start.DeepCopy();
            Vec3d endVec = inputLine.end;

            midVec.Y = endVec.Y;
            midVec2.X = endVec.X;

            NLine part1 = new NLine(startVec, midVec2);
            NLine part2 = new NLine(midVec2, endVec);

            List<NLine> lineList = new List<NLine>();

            lineList.Add(part1);
            lineList.Add(part2);

            return lineList;
        }
        public static List<NLine> orthoLineReturnPos(NLine inputLine)
        {
            Vec3d startVec = inputLine.start;
            Vec3d midVec = inputLine.start.DeepCopy();
            Vec3d midVec2 = inputLine.start.DeepCopy();
            Vec3d endVec = inputLine.end;

            midVec.Y = endVec.Y;
            midVec2.X = endVec.X;

            NLine part1 = new NLine(startVec, midVec);
            NLine part2 = new NLine(midVec, endVec);

            List<NLine> lineList = new List<NLine>();

            lineList.Add(part1);
            lineList.Add(part2);

            return lineList;
        }


        // PRINT
        public override string ToString()
        {
            return $"NLine[Start {this.start},End {this.end}]";
        }


    }
}
