using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGeoLib
{
    public class NSkeleton
    {
        // Class with advanced geometric operations around creation of straight skeletons
        public NFace boundsNFace;
        public List<NLine> oldForks;
        public List<NLine> acrossBranches;

        public List<NLine> currentBranches;

        public NSkeleton(NFace inputFace)
        {
            this.boundsNFace = inputFace.DeepCopy();
            this.currentBranches = getAllInteriorBisectors(inputFace);
            this.oldForks = new List<NLine>();
            this.acrossBranches = new List<NLine>();

        }

        public void iterate(NFace inputFace, int numIters)
        {
            for (int i = 0; i < numIters; i++)
            {
                this.updateSkeleton(inputFace);
            }
        }

        public void updateSkeleton(NFace inputFace)
        {
            this.trimCurrentLines();
            this.mergeSkeletonForks();
            this.extendSkeleton();
            this.trimToFace(inputFace);
        }


        public void trimCurrentLines()
        {
            List<NLine> singleBranches = new List<NLine>();
            List<NLine> mergedBranches = new List<NLine>();

            for (int i = 0; i < this.currentBranches.Count; i++)
            {
                bool doesIntersect = false;
                bool intersectedOnce = false;
                bool collinear = false;

                double lengthOfTrim = this.currentBranches[i].Length;

                NLine outTrim = this.currentBranches[i];
                for (int j = 0; j < this.currentBranches.Count; j++)
                {
                    if (i != j)
                    {

                        var line1 = this.currentBranches[i];
                        var line2 = this.currentBranches[j];

                        // add collinearity check
                        //if(areLineSegmentsCollinear(line2.start, line2.end, line1.start, line1.end))
                        //{
                        // both mipoints lie on same segment
                        //  this.currentBranches[i].end = this.currentBranches[j].start;
                        //  this.currentBranches[j].end = this.currentBranches[i].start;
                        //}

                        if (RIntersection.AreLinesIntersecting(line1.start, line1.end, line2.start, line2.end, true))
                        {
                            doesIntersect = true;
                            intersectedOnce = true;
                        }
                        else
                        {
                            doesIntersect = false;
                        }

                        if (doesIntersect)
                        {
                            var trimmedLine = NLine.trimLine(line1, line2);
                            if (trimmedLine.Length < outTrim.Length)
                                outTrim = trimmedLine;
                        }
                    }

                }


                if (outTrim.Length == lengthOfTrim)
                {
                    //NLine halfLine = new NLine(outTrim.start, outTrim.MidPoint);
                    //outTrim = halfLine;
                    singleBranches.Add(outTrim);
                }
                else
                {
                    mergedBranches.Add(outTrim);
                }

                //trimmedLinesList.Add(outTrim);
            }

            this.currentBranches = mergedBranches;
            this.acrossBranches = singleBranches;
        }

        public static bool areLineSegmentsCollinear(Vec3d a1, Vec3d a2, Vec3d b1, Vec3d b2)
        {

            Vec3d a = a2 - a1;
            Vec3d b = b2 - b1;

            Vec3d cross = Vec3d.CrossProduct(a, b);

            return cross.Mag < 1e-6;
        }

        public void shrinkTrimCurrent()
        {
            for (int i = 0; i < this.currentBranches.Count; i++)
            {
                for (int j = 0; j < this.currentBranches.Count; j++)
                {
                    if (i != j)
                    {
                        var line1 = this.currentBranches[i].DeepCopy();
                        var line2 = this.currentBranches[j].DeepCopy();
                        double dist = Vec3d.DistanceToSegment(line1.end, line2.start, line2.end);
                        if (dist < 0.001)
                            this.currentBranches[j].end = line1.end;
                    }
                }

            }
        }
        public void trimWithNext()
        {
            List<NLine> singleBranches = new List<NLine>();
            List<NLine> mergedBranches = new List<NLine>();

            for (int i = 0; i < this.currentBranches.Count; i++)
            {
                bool doesIntersect = false;
                bool intersectedOnce = false;
                double lengthOfTrim = this.currentBranches[i].Length;

                NLine outTrim = this.currentBranches[i];

                int j = i + 1;
                if (j < 0)
                {
                    j = this.currentBranches.Count - 1;
                }
                else if (j >= this.currentBranches.Count)
                {
                    j = 0;
                }

                var line1 = this.currentBranches[i];
                var line2 = this.currentBranches[j];
                if (RIntersection.AreLinesIntersecting(line1.start, line1.end, line2.start, line2.end, true))
                {
                    doesIntersect = true;
                    intersectedOnce = true;
                }
                else
                {
                    doesIntersect = false;
                }

                if (doesIntersect)
                {
                    var trimmedLine = NLine.trimLine(line1, line2);
                    if (trimmedLine.Length < outTrim.Length)
                        outTrim = trimmedLine;
                }

                if (outTrim.Length == lengthOfTrim)
                {
                    //NLine halfLine = new NLine(outTrim.start, outTrim.MidPoint);
                    //outTrim = halfLine;
                    singleBranches.Add(outTrim);
                }
                else
                {
                    mergedBranches.Add(outTrim);
                }

                //trimmedLinesList.Add(outTrim);
            }

            this.currentBranches = mergedBranches;
            this.acrossBranches = singleBranches;
        }

        public void trimWithNeighborLines()
        {
            List<NLine> singleBranches = new List<NLine>();
            List<NLine> mergedBranches = new List<NLine>();

            for (int i = 0; i < this.currentBranches.Count; i++)
            {
                bool doesIntersect = false;
                bool intersectedOnce = false;
                double lengthOfTrim = this.currentBranches[i].Length;

                NLine outTrim = this.currentBranches[i];
                for (int j = i - 1; j < 2; j++)
                {
                    if (j < 0)
                    {
                        j = this.currentBranches.Count - 1;
                    }
                    else if (j >= this.currentBranches.Count)
                    {
                        j = 0;
                    }

                    if (i != j)
                    {
                        var line1 = this.currentBranches[i];
                        var line2 = this.currentBranches[j];
                        if (RIntersection.AreLinesIntersecting(line1.start, line1.end, line2.start, line2.end, true))
                        {
                            doesIntersect = true;
                            intersectedOnce = true;
                        }
                        else
                        {
                            doesIntersect = false;
                        }

                        if (doesIntersect)
                        {
                            var trimmedLine = NLine.trimLine(line1, line2);
                            if (trimmedLine.Length < outTrim.Length)
                                outTrim = trimmedLine;
                        }
                    }

                }

                if (outTrim.Length == lengthOfTrim)
                {
                    //NLine halfLine = new NLine(outTrim.start, outTrim.MidPoint);
                    //outTrim = halfLine;
                    singleBranches.Add(outTrim);
                }
                else
                {
                    mergedBranches.Add(outTrim);
                }

                //trimmedLinesList.Add(outTrim);
            }

            this.currentBranches = mergedBranches;
            this.acrossBranches = singleBranches;
        }

        public void mergeSkeletonForks()
        {
            // identify forks

            List<NLine> mergedOutLines = new List<NLine>();

            List<NLine> oldForks = new List<NLine>();

            for (int i = 0; i < this.currentBranches.Count; i++)
            {
                bool hasfork = false;

                // line to extend by default just the branch (if fork recognized, replace with bisector of fork)
                NLine lineToExtend = this.currentBranches[i];

                // Identify forks, extend bisector of fork.
                for (int j = 0; j < this.currentBranches.Count; j++)
                {
                    if (i != j)
                    {
                        if (Vec3d.Distance(this.currentBranches[i].end, this.currentBranches[j].end) < 0.001)
                        {
                            // foundfork
                            hasfork = true;

                            Vec3d norm1 = this.currentBranches[i].Direction.DeepCopy();
                            Vec3d norm2 = this.currentBranches[j].Direction.DeepCopy();

                            norm1.Norm();
                            norm2.Norm();

                            Vec3d base1 = this.currentBranches[i].end - norm1;
                            Vec3d base2 = this.currentBranches[j].end - norm2;

                            Vec3d basePt = Vec3d.Mid(base1, base2);
                            Vec3d dirVec = this.currentBranches[i].end - basePt;
                            Vec3d newEndVec = this.currentBranches[i].end + dirVec;

                            lineToExtend = new NLine(this.currentBranches[i].end, newEndVec);
                        }
                    }
                }

                NLine forkcopy = this.currentBranches[i].DeepCopy();
                if (hasfork)
                    this.oldForks.Add(forkcopy);

                mergedOutLines.Add(lineToExtend);
            }
            // all others extend normally
            List<NLine> uniqueMerged = NLine.deleteDuplicateLines(mergedOutLines);
            this.currentBranches = uniqueMerged;

            //return new Tuple<List<NLine>, List<NLine>>(uniqueMerged, oldForks);
            // add the single branches to the mix

            // do another shrink round.
        }
        public void extendSkeleton()
        {
            // extend all lines
            double searchRadius = 500;
            List<NLine> mergedExtended = new List<NLine>();

            for (int i = 0; i < this.currentBranches.Count; i++)
            {
                NLine tempLine = this.currentBranches[i].DeepCopy();
                NLine newLine = NLine.extendLine(tempLine, searchRadius);

                //List<NLine> trimmedList = trimNLineWithNFaceXX(newLine, this.boundsNFace);

                // add the trimmed bisector to the list
                //if (trimmedList.Count > 0)
                //{
                // mergedExtended.Add(trimmedList[0]);

                //
                //}
                mergedExtended.Add(newLine);
            }



            this.currentBranches = mergedExtended;
        }

        public void trimToFace(NFace inputFace)
        {
            List<NLine> tempLines = new List<NLine>();

            for (int i = 0; i < this.currentBranches.Count; i++)
            {
                NLine tempLine = this.currentBranches[i].DeepCopy();

                List<NLine> trimmedList = RIntersection.trimNLineWithNFace(tempLine, inputFace);

                if (trimmedList.Count > 0)
                    tempLines.Add(trimmedList[0]);
            }
            this.currentBranches = tempLines;
            // add the trimmed bisector to the list
        }

        // BISECTOR creatioin
        public static List<NLine> getAllInteriorBisectors(NFace inputFace)
        {
            // create a list to store the bisectors
            List<NLine> bisectors = new List<NLine>();

            // compute the interior bisector of each edge
            foreach (NEdge edge in inputFace.edgeList)
            {
                // compute the interior bisector of the edge
                Tuple<NLine, bool> bisectorTuple = interiorBisector(edge, 500);
                NLine bisector = bisectorTuple.Item1;

                // trim the bisector with the input face
                // List<NLine> trimmedList = RIntersection.trimNLineWithNFace(bisector, inputFace);
                List<NLine> trimmedList = RIntersection.trimNLineWithNFace(bisector, inputFace);

                // add the trimmed bisector to the list
                if (trimmedList.Count > 0)
                    bisectors.Add(trimmedList[0]);
                //bisectors.Add(bisector);
            }

            // return the list of bisectors
            return bisectors;
        }
        public static Tuple<NLine, bool> interiorBisector(NEdge inputEdge, double vecLength)
        {
            // compute the unit direction vectors of the input edge and the previous edge
            Vec3d vecA = inputEdge.Direction.GetNorm();
            Vec3d vecB = inputEdge.prevNEdge.Direction.GetNorm();

            // reverse the direction of vecB
            vecB.Reverse();

            // compute the sum of the direction vectors
            Vec3d vecs = vecA + vecB;

            // scale vecs to the desired length
            vecs = Vec3d.ScaleToLen(vecs, vecLength);

            // determine whether the input edge is convex
            bool isConvex = false;
            Vec3d additionVec = inputEdge.v - vecs;
            if (Vec3d.CrossProduct(vecA, vecB).Z >= 0)
            {
                isConvex = true;
                additionVec = inputEdge.v + vecs;
            }

            // construct the bisector line
            NLine Bline = new NLine(inputEdge.v, additionVec);
            return new Tuple<NLine, bool>(Bline, isConvex);
        }
        public static Tuple<NLine, bool> bisectorLine(NEdge inputEdge)
        {
            Vec3d vecA = inputEdge.Direction.GetNorm();
            Vec3d vecB = inputEdge.prevNEdge.Direction.GetNorm();

            vecB.Reverse();
            Vec3d vecs = vecA + vecB;

            vecs = Vec3d.ScaleToLen(vecs, 3);

            bool isConvex = false;
            if (Vec3d.CrossProduct(vecA, vecB).Z >= 0)
                isConvex = true;

            NLine Bline = new NLine(inputEdge.v, inputEdge.v + vecs);
            return new Tuple<NLine, bool>(Bline, isConvex);
        }

        public static List<NLine> trimNLineWithNFaceXX(NLine inputLine, NFace tFace)
        {
            NFace inputFace = tFace.DeepCopy();
            // splits a line with face boundaries and outputs all lines inside the face as a list
            List<NLine> outLines = new List<NLine>();

            List<Vec3d> splitPoints = new List<Vec3d>();
            Vec3d startP = inputLine.start;
            Vec3d endP = inputLine.end;

            splitPoints.Add(startP);
            // check if line intersects with points of face
            for (int i = 0; i < inputFace.edgeList.Count; i++)
            {
                // check for intersection with vertex
                bool vertexHit = RIntersection.PointLineIntersection(inputFace.edgeList[i].v, inputLine.start, inputLine.end);
                if (vertexHit && Vec3d.Distance(inputLine.start, inputFace.edgeList[i].v) > 0.001)
                {
                    Vec3d tempP = inputFace.edgeList[i].v.DeepCopy();
                    splitPoints.Add(tempP);
                }

                // check for intersection with edge
                bool isintersecting = RIntersection.AreLinesIntersecting(inputLine.start, inputLine.end, inputFace.edgeList[i].v, inputFace.edgeList[i].nextNEdge.v, true);
                if (isintersecting)
                {
                    Vec3d tempP = RIntersection.GetLineLineIntersectionPoint(inputLine.start, inputLine.end, inputFace.edgeList[i].v, inputFace.edgeList[i].nextNEdge.v);
                    splitPoints.Add(tempP);
                }
            }
            splitPoints.Add(endP);


            /// go through all sublines in polyline, check which ones are inside the face... output those. all lines
            List<Vec3d> SortedPt = splitPoints.OrderBy(o => Vec3d.Distance(startP, o)).ToList();

            NPolyLine pline_out = new NPolyLine(SortedPt);

            for (int i = 0; i < pline_out.lineList.Count; i++)
            {
                if (RIntersection.insideNFace(pline_out.lineList[i].MidPoint, inputFace))
                {
                    outLines.Add(pline_out.lineList[i]);
                }
            }


            return outLines;

        }

    }

}
