using System;
using System.Collections.Generic;
using System.Linq;

namespace RGeoLib
{
    public class NPolyLine
    {
        public List<NLine> lineList;
        public List<double> lengthList; 
        public string property { get; set; }
        public NPolyLine(List<NLine> lines)
        {
            // initialize lines
            this.lineList = lines;
        }

        public NPolyLine(List<Vec3d> vecs)
        {
            List<NLine> lines = new List<NLine>();
            List<double> lengths = new List<double>();
            
            for (int i = 0; i < vecs.Count-1; i++)
            { 
                NLine tempLine = new NLine(vecs[i], vecs[i+1]);
                lines.Add(tempLine);
                lengths.Add(tempLine.Length);
            }

            this.lengthList = lengths;
            this.lineList = lines;
        }

        // Get Length
        public double Length
        { 
            get 
            {
                double lenTot = 0;
                for (int i = 0; i < this.lineList.Count; i++)
                { 
                    lenTot+=this.lineList[i].Length;
                }
                return lenTot ; 
            }
        }

        public Vec3d start
        {
            get
            {
                return this.lineList[0].start;
            }
        }
        public Vec3d end
        {
            get
            {
                return this.lineList.Last().end;
            }
        }
        public List<Vec3d> VecsStart
        {
            get
            {
                List<Vec3d> tempVecs = new List<Vec3d>();
                for (int i = 0; i < this.lineList.Count;i++)
                {
                    tempVecs.Add(this.lineList[i].start);
                }
                return tempVecs;
            }
        }

        public List<Vec3d> VecsAll
        {
            get
            {
                List<Vec3d> tempVecs = new List<Vec3d>();
                for (int i = 0; i < this.lineList.Count; i++)
                {
                    tempVecs.Add(this.lineList[i].start);
                }
                tempVecs.Add(this.lineList[this.lineList.Count - 1].end);
                return tempVecs;
            }
        }


        // Eval Curve
        // Input: Loc on curve e.g. 0.75
        // Tuple output <Vec3d LOC, Vec3d Direction>
        public Vec3d EvalPoint(double locAtDomain)
        {
            double fullLength = this.Length;
            double domainReal = locAtDomain * fullLength;

            double currentLen = 0;
            double tempLen = 0;
            int chosen = 0;
            for (int i = 0; i < this.lineList.Count; i++)
            {
                currentLen += this.lineList[i].Length;
                if (currentLen >= domainReal)
                {
                    chosen = i;
                    break;
                }
                tempLen = currentLen;
            }

            // case 0, point is in second + segment
            if (chosen > 0)
            {
                // get len of previous segments
                domainReal -= tempLen;
            }

            // case 1, point is in first segment

            // convert domain real to domain of segment
            double tempSegmentLen = this.lineList[chosen].Length;
            double domainSegment = domainReal / tempSegmentLen;

            Vec3d pointOut = this.lineList[chosen].PointAt(domainSegment);
            return pointOut;
        }
        public Vec3d EvalDirection(double locAtDomain)
        {
            double fullLength = this.Length;
            double domainReal = locAtDomain * fullLength;

            double currentLen = 0;
            double tempLen = 0;
            int chosen = 0;
            for (int i = 0; i < this.lineList.Count; i++)
            {
                currentLen += this.lineList[i].Length;
                if (currentLen >= domainReal)
                {
                    chosen = i;
                    break;
                }
                tempLen = currentLen;
            }

            Vec3d dirOut = this.lineList[chosen].Direction;
            dirOut.Norm();
            return dirOut;
        }

        public void flip()
        {

            // Creates empty list
            List<NLine> tempLines = new List<NLine>();

            // Go through all edges backwards
            for (int i = (this.lineList.Count - 1); i >= 0; i--)
            {
                this.lineList[i].FlipLine();
                tempLines.Add(this.lineList[i]);
            }
           
        }
        public Tuple<NPolyLine, NPolyLine> SplitPolylineAt(double locAtDomain)
        {
            double fullLength = this.Length;
            double domainReal = locAtDomain * fullLength;

            double currentLen = 0;
            double tempLen = 0;
            int chosen = 0;

            List<NLine> leftLines = new List<NLine>();
            List<NLine> rightLines = new List<NLine>();


            for (int i = 0; i < this.lineList.Count; i++)
            {
                currentLen += this.lineList[i].Length;
                if (currentLen >= domainReal)
                {
                    chosen = i;
                    break;
                }
                tempLen = currentLen;
                leftLines.Add(this.lineList[i]);
            }

            // GET MidPoint
            // case 0, point is in second + segment
            if (chosen > 0)
            {
                // get len of previous segments
                domainReal -= tempLen;
            }

            // case 1, point is in first segment

            // convert domain real to domain of segment
            double tempSegmentLen = this.lineList[chosen].Length;
            double domainSegment = domainReal / tempSegmentLen;

            Vec3d pointOut = this.lineList[chosen].PointAt(domainSegment);

            NLine leftTemp = new NLine(this.lineList[chosen].PointAt(0), this.lineList[chosen].PointAt(domainSegment));
            NLine rightTemp = new NLine(this.lineList[chosen].PointAt(domainSegment), this.lineList[chosen].PointAt(1));

            leftLines.Add(leftTemp);

            rightLines.Add(rightTemp);

            if (chosen + 1 <= this.lineList.Count)
            {
                for (int i = chosen + 1; i < this.lineList.Count; i++)
                {
                    rightLines.Add(this.lineList[i]);
                }
            }

            NPolyLine polyLeft = new NPolyLine(leftLines);
            NPolyLine polyRight = new NPolyLine(rightLines);

            return new Tuple<NPolyLine, NPolyLine>(polyLeft, polyRight);

        }
        public static Tuple<NPolyLine, NPolyLine> SplitPolylineAt(NPolyLine inputPoly, double locAtDomain)
        {
            return inputPoly.SplitPolylineAt(locAtDomain);
        }

        public static NPolyLine cleanStraightPolyline(NPolyLine inputPoly)
        {
            // get all end points of lines
            List<Vec3d> dupVecs = new List<Vec3d>();
            for (int i = 0; i < inputPoly.lineList.Count; i++)
            {
                dupVecs.Add(inputPoly.lineList[i].start);
                dupVecs.Add(inputPoly.lineList[i].end);
            }

            // delete duplicate vecs
            double precision = 0.001;
            List<Vec3d> uniqueVecs = Vec3d.deleteDuplicatePointsRKHT(dupVecs);
            List<Vec3d> sortedVecs = uniqueVecs.OrderBy(p => Math.Round(p.X / precision)).ThenBy(p => Math.Round(p.Y / precision)).ThenBy(p => Math.Round(p.Z / precision)).ToList();



            return new NPolyLine(sortedVecs);
        }

        public static NLine lineFromStraightPoly(NPolyLine inputPoly)
        {
            NPolyLine tempPoly = cleanStraightPolyline(inputPoly);
            return new NLine(tempPoly.start, tempPoly.end);
        }
        public static List<NPolyLine> SplitPolyLineWithList(NPolyLine inputPoly, List<double> splitDomains)
        {
            // sample List<double> splitDomains = new List<double>() { 0.1, 0.5, 0.1, 0.3 };

            List<NPolyLine> polyList = new List<NPolyLine>();

            NPolyLine inputPolyX = inputPoly;
            double globalLength = inputPoly.Length;

            for (int i = 0; i < splitDomains.Count; i++)
            {
                double realLength = splitDomains[i] * globalLength;
                double thisDomain = realLength / inputPolyX.Length;
                Tuple<NPolyLine, NPolyLine> plyTempTupleX = SplitPolylineAt(inputPolyX, thisDomain);
                polyList.Add(plyTempTupleX.Item1);
                inputPolyX = plyTempTupleX.Item2;
            }

            return polyList;    
        }

        // Create from Lines
        public static List<NPolyLine> mergeLines(List<NLine> inputLines)
        {
            //DEPRECATED  use addLinestoPolylines

            // tries to join all lines into polyLines

            List<NPolyLine> connectedPolyLineList = new List<NPolyLine>();

            foreach (var inputLine in inputLines)
            {
                NLine tempLine = new NLine(inputLine.start, inputLine.end);
                bool lineAdded = false;

                for (int i = 0; i < connectedPolyLineList.Count; i++)
                {
                    if (CanAddLine(connectedPolyLineList[i], tempLine))
                    {
                        connectedPolyLineList[i] = AddLine(connectedPolyLineList[i], tempLine);
                        lineAdded = true;
                        break;
                    }
                }

                if (!lineAdded)
                {
                    List<NLine> currentPolyLines = new List<NLine> { tempLine };
                    NPolyLine tempPolyLine = new NPolyLine(currentPolyLines);
                    connectedPolyLineList.Add(tempPolyLine);
                }
            }

            // Merge polylines that can be connected
            bool mergeOccurred;
            do
            {
                mergeOccurred = false;
                for (int i = 0; i < connectedPolyLineList.Count; i++)
                {
                    for (int j = i + 1; j < connectedPolyLineList.Count; j++)
                    {
                        if (CanConnect(connectedPolyLineList[i], connectedPolyLineList[j]))
                        {
                            connectedPolyLineList[i] = Merge(connectedPolyLineList[i], connectedPolyLineList[j]);
                            connectedPolyLineList.RemoveAt(j);
                            mergeOccurred = true;
                            break;
                        }
                    }
                    if (mergeOccurred) break; // Break the outer loop if a merge occurred to avoid invalid indices
                }
            } while (mergeOccurred);

            return connectedPolyLineList;
        }
        public static List<NPolyLine> mergeLinesNEW(List<NLine> inputLines)
        {
            //DEPRECATED  use addLinestoPolylines

            List<NLine> deepCopyLineList = NLine.DeepCopyList(inputLines);
            List<NPolyLine> nonConnectedLines = new List<NPolyLine>();
            List<NPolyLine> connectedPolyLineList = new List<NPolyLine>();

            foreach (var inputLine in inputLines)
            {
                NLine tempLine = new NLine(inputLine.start, inputLine.end);
                bool lineAdded = false;

                for (int i = 0; i < connectedPolyLineList.Count; i++)
                {
                    if (NPolyLine.CanAddLine(connectedPolyLineList[i], tempLine))
                    {
                        connectedPolyLineList[i] = NPolyLine.AddLine(connectedPolyLineList[i], tempLine);
                        lineAdded = true;
                        inputLine.IsProcessed = true; // Mark the line as processed
                        break;
                    }
                }

                if (!lineAdded)
                {
                    List<NLine> currentPolyLines = new List<NLine> { tempLine };
                    NPolyLine tempPolyLine = new NPolyLine(currentPolyLines);
                    connectedPolyLineList.Add(tempPolyLine);
                    inputLine.IsProcessed = true; // Mark the line as processed
                }
            }

            // Merge polylines that can be connected
            bool mergeOccurred;
            do
            {
                mergeOccurred = false;
                for (int i = 0; i < connectedPolyLineList.Count; i++)
                {
                    for (int j = i + 1; j < connectedPolyLineList.Count; j++)
                    {
                        if (NPolyLine.CanConnect(connectedPolyLineList[i], connectedPolyLineList[j]))
                        {
                            connectedPolyLineList[i] = NPolyLine.Merge(connectedPolyLineList[i], connectedPolyLineList[j]);
                            connectedPolyLineList.RemoveAt(j);
                            mergeOccurred = true;
                            break;
                        }
                    }
                    if (mergeOccurred) break;
                }
            } while (mergeOccurred);

            // Add unprocessed lines as individual polylines
            foreach (var line in inputLines)
            {
                if (!line.IsProcessed)
                {
                    connectedPolyLineList.Add(new NPolyLine(new List<NLine> { new NLine(line.start, line.end) }));
                }
            }


            return connectedPolyLineList;

        }
        public static List<NPolyLine> simplifyIntoPolyLinesOLD(List<NLine> inputLines)
        {
            //DEPRECATED  use addLinestoPolylines
            List<NLine> uniqueLines = NLine.getUniqueLines(inputLines);
            List<List<NLine>> groupedLines = GroupConnectedLines(inputLines);

            List<NPolyLine> polyOut = new List<NPolyLine>();

            for (int i = 0; i < groupedLines.Count; i++)
            {
                groupedLines[i] = NLine.ConformLinesNew(groupedLines[i]);
                List<NPolyLine> polyTempMerged = mergeLinesNEW(groupedLines[i]);

                for (int j = 0; j < polyTempMerged.Count; j++)
                {
                    NPolyLine straightPoly = ReduceStraightPoints(polyTempMerged[j]);
                    polyOut.Add(straightPoly);
                }
            }
            return polyOut;
        }

        private static void AddLinesToPolylines(List<NPolyLine> connectedPolyLineList, List<NLine> inputLines, int inputLineIndex = 0)
        {
            if (inputLineIndex >= inputLines.Count)
            {
                // Base case: All lines processed
                return;
            }

            NLine currentLine = inputLines[inputLineIndex];
            bool lineAdded = false;

            // Try to add the line to an existing polyline
            for (int j = 0; j < connectedPolyLineList.Count; j++)
            {
                if (CanAddLineAtEnd(connectedPolyLineList[j], currentLine))
                {
                    NPolyLine.AddLine(connectedPolyLineList[j], currentLine);
                    lineAdded = true;
                    break;
                }
            }

            // If the line couldn't be added to any polyline, create a new polyline
            if (!lineAdded)
            {
                List<NLine> lineListTemp = new List<NLine>() { currentLine };
                NPolyLine newPolyline = new NPolyLine(lineListTemp);
                connectedPolyLineList.Add(newPolyline);
            }

            // Recursive call for the next line
            AddLinesToPolylines(connectedPolyLineList, inputLines, inputLineIndex + 1);
        }
        public static bool CanAddLineAtEnd(NPolyLine inputPoly, NLine line)
        {
            //bool connectsToVec = false;
            bool connectsToEnds = false;
            double tolerance = 0.001;

            Vec3d endPoint = inputPoly.EvalPoint(0);
            Vec3d startPoint = inputPoly.EvalPoint(1);

            // check if connection at end?
            if (Vec3d.Distance(line.start, startPoint) < tolerance)
                connectsToEnds = true;
            if (Vec3d.Distance(line.end, startPoint) < tolerance)
                connectsToEnds = true;
            // check if connection at start?
            if (Vec3d.Distance(line.start, endPoint) < tolerance)
                connectsToEnds = true;
            if (Vec3d.Distance(line.end, endPoint) < tolerance)
                connectsToEnds = true;

            return connectsToEnds;
        }
        public static List<NPolyLine> simplifyIntoPolyLines(List<NLine> inputLines)
        {
            List<NLine> uniqueLines = NLine.getUniqueLines(inputLines);
            List<List<NLine>> groupedLines = GroupConnectedLines(inputLines);

            List<NPolyLine> polyOut = new List<NPolyLine>();

            for (int i = 0; i < groupedLines.Count; i++)
            {
                groupedLines[i] = ConformLines(groupedLines[i]);
                List<NPolyLine> connectedTemp = new List<NPolyLine>();
                AddLinesToPolylines(connectedTemp, groupedLines[i]);

                //List<NPolyLine> polyTempMerged = mergeLinesNEW(groupedLines[i]);

                for (int j = 0; j < connectedTemp.Count; j++)
                {
                    NPolyLine straightPoly = ReduceStraightPoints(connectedTemp[j]);
                    polyOut.Add(straightPoly);
                }
            }
            return polyOut;
        }

        private static bool AreLinesConnected(NLine line1, NLine line2)
        {
            double tolDist = 0.001;

            return (Vec3d.Distance(line1.end, line2.start) < tolDist) ||
              (Vec3d.Distance(line1.start, line2.end) < tolDist) ||
              (Vec3d.Distance(line1.end, line2.end) < tolDist) ||
              (Vec3d.Distance(line1.start, line2.start) < tolDist);
        }

        public static List<NLine> ConformLines(List<NLine> inputLines)
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

        public static NPolyLine ReduceStraightPoints(NPolyLine inputPoly)
        {
            List<NLine> reducedLines = new List<NLine>();
            Vec3d currentStart = inputPoly.lineList[0].start;

            for (int i = 0; i < inputPoly.lineList.Count - 1; i++)
            {
                Vec3d jointPoint = inputPoly.lineList[i].end;
                Vec3d nextEnd = inputPoly.lineList[i + 1].end;

                if (!IsCollinear(currentStart, jointPoint, nextEnd))
                {
                    // If the points are not collinear, add the current line to the list and update the current start
                    reducedLines.Add(new NLine(currentStart, jointPoint));
                    currentStart = jointPoint;
                }
            }

            // Add the last line
            reducedLines.Add(new NLine(currentStart, inputPoly.lineList[inputPoly.lineList.Count - 1].end));

            // Return the new polyline with reduced lines
            return new NPolyLine(reducedLines);
        }

        private static bool IsCollinear(Vec3d a, Vec3d b, Vec3d c)
        {
            // Check if three points are collinear in 3D space
            // This can be done by checking if the cross product of AB and BC is zero (or very close to zero)
            Vec3d AB = new Vec3d(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
            Vec3d BC = new Vec3d(c.X - b.X, c.Y - b.Y, c.Z - b.Z);
            Vec3d crossProduct = Vec3d.CrossProduct(AB, BC);

            return Vec3d.IsAlmostZero(crossProduct);
        }

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
                if (!visited.Contains(line) && NLine.AreLinesConnected(currentLine, line))
                {
                    GroupLinesRecursive(line, allLines, currentGroup, visited);
                }
            }
        }

        // Helper functions for mergeLines
        public static bool CanAddLine(NPolyLine inputPoly, NLine line)
        {
            for (int i = 0; i < inputPoly.lineList.Count; i++)
            {
                if (lineEndsConnect(line, inputPoly.lineList[i], 0.001))
                    return true;
            }
            return false;
        }
        public static NPolyLine AddLine(NPolyLine inputPoly, NLine line)
        {

            if (Vec3d.Distance(line.end, inputPoly.start) < 0.01)
            {
                inputPoly.lineList.Insert(0, line);
            }
            else
                inputPoly.lineList.Add(line);
            // Add line to this polyLine

            List<NLine> tempList = inputPoly.lineList;

            NPolyLine outputPolyLine = new NPolyLine(tempList);
            return outputPolyLine;
        }
        public static bool CanConnect(NPolyLine polyLineA, NPolyLine polyLineB)
        {
            // return true if this polyLine can be connected with polyLine, otherwise false. // flips to same dir as other
            double tolerance = 0.001;
            bool canConnect = false;

            if (Vec3d.Distance(polyLineA.start, polyLineB.end) < tolerance)
            {
                canConnect = true;
            }
            else if (Vec3d.Distance(polyLineA.start, polyLineB.start) < tolerance)
            {
                polyLineB.flip();
                canConnect = true;
            }
            else if (Vec3d.Distance(polyLineA.end, polyLineB.start) < tolerance)
            {
                canConnect = true;
            }
            else if (Vec3d.Distance(polyLineA.end, polyLineB.end) < tolerance)
            {
                polyLineB.flip();
                canConnect = true;
            }

            return canConnect;
        }
        public static NPolyLine Merge(NPolyLine polyLineA, NPolyLine polyLineB)
        {
            // Merge this polyLine with polyLine.    
            List<NLine> linesNew = new List<NLine>();
            linesNew.AddRange(polyLineA.lineList);
            linesNew.AddRange(polyLineB.lineList);

            linesNew = NLine.conformLines(linesNew);

            NPolyLine polyMerged = new NPolyLine(linesNew);
            return polyMerged;
        }
        public static bool lineEndsConnect(NLine lineA, NLine lineB, double tolDist)
        {
            int adjacentInt = 0;

            double distEndEnd = Vec3d.Distance(lineA.end, lineB.end);
            double distStartStart = Vec3d.Distance(lineA.start, lineB.start);

            double distStartEnd = Vec3d.Distance(lineA.start, lineB.end);
            double distEndStart = Vec3d.Distance(lineA.end, lineB.start);

            if (Math.Abs(distStartEnd) < tolDist ^ Math.Abs(distEndStart) < tolDist)
            {
                adjacentInt = 1;
            }
            else if (Math.Abs(distEndEnd) < tolDist ^ Math.Abs(distStartStart) < tolDist)
            {
                adjacentInt = 2;
            }

            if (adjacentInt > 0)
                return true;
            else
                return false;
        }









    }
}
