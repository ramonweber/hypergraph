using NPOI.SS.Formula.Functions;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGeoLib
{
    public class RIntersection
    {
        // Boolean LOW TOL for curves next to each other

        public static List<NLine> lowTolBoolIntersection(List<NLine> listTarget, List<NLine> listOther, double thresholdDist = 0.5, double sameTolerance = 0.01)
        {
            listTarget = RIntersection.splitLinesWithLines(listTarget, listOther, sameTolerance);
            listOther = RIntersection.splitLinesWithLines(listOther, listTarget, sameTolerance);

            List<NLine> outLinesIntersection = new List<NLine>();

            for (int i = 0; i < listTarget.Count; i++)
            {
                for (int j = 0; j < listOther.Count; j++)
                {
                    Tuple<List<NLine>, List<NLine>, List<NLine>> boolTuple = RIntersection.lowTolBool(listTarget[i], listOther[j], thresholdDist, sameTolerance);
                    outLinesIntersection.AddRange(boolTuple.Item3);
                }
            }

            return outLinesIntersection;
        }
        public static Tuple<List<NLine>, List<NLine>, List<NLine>> lowTolBool(List<NLine> listTarget, List<NLine> listOther, double thresholdDist = 0.5, double sameTolerance = 0.01)
        {
            listTarget = RIntersection.splitLinesWithLines(listTarget, listOther, sameTolerance);
            listOther = RIntersection.splitLinesWithLines(listOther, listTarget, sameTolerance);

            List<NLine> outLinesShattered = listTarget;
            List<NLine> outLinesIntersection = new List<NLine>();

            for (int i = 0; i < listTarget.Count; i++)
            {
                for (int j = 0; j < listOther.Count; j++)
                {
                    Tuple<List<NLine>, List<NLine>, List<NLine>> boolTuple = RIntersection.lowTolBool(listTarget[i], listOther[j], thresholdDist, sameTolerance);
                    outLinesIntersection.AddRange(boolTuple.Item3);
                }
            }

            List<NLine> outLinesDifference = RIntersection.lineListBooleanDifferenceTol(listTarget, outLinesIntersection, sameTolerance);

            return new Tuple<List<NLine>, List<NLine>, List<NLine>>(outLinesShattered, outLinesDifference, outLinesIntersection);
        }
        public static Tuple<List<NLine>, List<NLine>, List<NLine>> lowTolBool(NLine lineBool, NLine lineOther, double thresholdDist = 0.5, double sameTolerance = 0.01)
        {
            // all boolean operations on lineB ,will output all lines for line B
            NLine lineA = lineOther.DeepCopy();
            NLine lineB = lineBool.DeepCopy();
            // for all pointsA, project pointsA to lineB
            List<NLine> outLinesShattered = new List<NLine>(); // All lines
            List<NLine> outLinesIntersection = new List<NLine>(); // bool Intersection
            List<NLine> outLinesDifference = new List<NLine>();  // bool difference

            List<Vec3d> pointsA = new List<Vec3d>() { lineA.start, lineA.end };

            //Tuple<string, Vec3d, Vec3d, NLine, List<NLine>, List<NLine>> tupleInt = RIntersection.intersectionQueryLinesOutTol(lineA.start, lineA.end, lineB.start, lineB.end, thresholdDist);


            Vec3d intVecAStart = RIntersection.LineClosestPoint2D(lineA.start, lineB.start, lineB.end);
            //double distAStart = Vec3d.Distance(intVecAStart, lineA.start);

            Vec3d intVecAEnd = RIntersection.LineClosestPoint2D(lineA.end, lineB.start, lineB.end);
            //double distAEnd = Vec3d.Distance(intVecAEnd, lineA.end);

            // from intVecAEnd to closest point on line A
            Vec3d sIntAS = RIntersection.LineClosestPoint2D(intVecAStart, lineA.start, lineA.end);
            Vec3d sIntAE = RIntersection.LineClosestPoint2D(intVecAEnd, lineA.start, lineA.end);

            double distAStart = Vec3d.Distance(intVecAStart, sIntAS);
            double distAEnd = Vec3d.Distance(intVecAEnd, sIntAE);


            double distASBS = Vec3d.Distance(lineA.start, lineB.start);
            double distASBE = Vec3d.Distance(lineA.start, lineB.end);
            double distAEBS = Vec3d.Distance(lineA.end, lineB.start);
            double distAEBE = Vec3d.Distance(lineA.end, lineB.end);


            // case 0 , they are the same
            if (distASBS < sameTolerance && distAEBE < sameTolerance || distASBE < sameTolerance && distAEBS < sameTolerance)
            {
                //outstring += "CASE 0: same curve, output b \n";
                outLinesShattered.Add(lineB);
                outLinesIntersection.Add(lineB);
            }
            // case 1 both are in between
            else if (distAStart < thresholdDist && distAEnd < thresholdDist)
            {
                //outstring += "CASE 1: both end points of curveA in between curveB \n";
                double distBsAs = Vec3d.Distance(lineB.start, intVecAStart);
                double distBsAe = Vec3d.Distance(lineB.start, intVecAEnd);
                //outstring += "distBsAs: " + distBsAs + " distBsAe: " + distBsAe + "\n";
                // case 0a if As closer than Ae further than a from start
                if (distBsAs < distBsAe)
                {
                    //outstring += "case 1a if As closer than Ae further than a from start";
                    NLine lineBStart = new NLine(lineB.start, intVecAStart);
                    NLine lineBMid = new NLine(intVecAStart, intVecAEnd);
                    NLine lineBEnd = new NLine(intVecAEnd, lineB.end);
                    // Add shattered
                    outLinesShattered.Add(lineBStart);
                    outLinesShattered.Add(lineBMid);
                    outLinesShattered.Add(lineBEnd);
                    // Add bool int
                    outLinesIntersection.Add(lineBMid);
                    // Add bool diff
                    outLinesDifference.Add(lineBStart);
                    outLinesDifference.Add(lineBEnd);
                }
                else
                {
                    //outstring += "case 1b if Ae closer than As further than a from start";
                    NLine lineBStart = new NLine(lineB.start, intVecAEnd);
                    NLine lineBMid = new NLine(intVecAEnd, intVecAStart);
                    NLine lineBEnd = new NLine(intVecAStart, lineB.end);
                    // Add shattered
                    outLinesShattered.Add(lineBStart);
                    outLinesShattered.Add(lineBMid);
                    outLinesShattered.Add(lineBEnd);
                    // Add bool int
                    outLinesIntersection.Add(lineBMid);
                    // Add bool diff
                    outLinesDifference.Add(lineBStart);
                    outLinesDifference.Add(lineBEnd);
                }
            }
            else
            {
                //outstring += "CASE 4: They don't touch";
                outLinesShattered.Add(lineB);
                outLinesDifference.Add(lineB);
            }

            return new Tuple<List<NLine>, List<NLine>, List<NLine>>(outLinesShattered, outLinesDifference, outLinesIntersection);
        }


        // Boolean Line List Operations
        public static List<NLine> lineListBooleanUnion(List<NLine> lineInputA, List<NLine> lineInputB)
        {
            List<NLine> unionAB = new List<NLine>();

            for (int i = 0; i < lineInputA.Count; i++)
            {
                // go through each line of line B

                for (int j = 0; j < lineInputB.Count; j++)
                {
                    // boolean union
                    Tuple<string, List<NLine>> boolUnionTuple = curveBooleanUnion(lineInputA[i].start, lineInputA[i].end, lineInputB[j].start, lineInputB[j].end);
                    unionAB.AddRange(boolUnionTuple.Item2);
                }

            }
            List<NLine> unionAB_U = NLine.deleteDuplicateLines(unionAB);
            return unionAB_U;
        }
        public static List<NLine> lineListBooleanDifference(List<NLine> lineInputA, List<NLine> lineInputB)
        {
            for (int i = 0; i < lineInputB.Count; i++)
            {
                List<NLine> differenceAB1 = lineListSingleBooleanDifference(lineInputA, lineInputB[i]);
                lineInputA = differenceAB1;
            }

            return lineInputA;
        }
        public static List<NLine> lineListBooleanDifferenceTol(List<NLine> lineInputA, List<NLine> lineInputB, double tol)
        {
            for (int i = 0; i < lineInputB.Count; i++)
            {
                List<NLine> differenceAB1 = lineListSingleBooleanDifferenceTol(lineInputA, lineInputB[i], tol);
                lineInputA = differenceAB1;
            }

            return lineInputA;
        }
        public static List<NLine> lineListSingleBooleanDifference(List<NLine> lineInputA, NLine lineInputB)
        {
            List<NLine> differenceAB = new List<NLine>();

            for (int i = 0; i < lineInputA.Count; i++)
            {
                // boolean difference
                Tuple<string, List<NLine>> boolDifferenceTuple = curveBooleanDifference(lineInputA[i].start, lineInputA[i].end, lineInputB.start, lineInputB.end);
                if (boolDifferenceTuple.Item2.Count > 0)
                    differenceAB.AddRange(boolDifferenceTuple.Item2);
            }
            List<NLine> differenceAB_U = NLine.deleteDuplicateLines(differenceAB);
            return differenceAB_U;
        }
        public static List<NLine> lineListSingleBooleanDifferenceTol(List<NLine> lineInputA, NLine lineInputB, double tol)
        {
            List<NLine> differenceAB = new List<NLine>();

            for (int i = 0; i < lineInputA.Count; i++)
            {
                // boolean difference
                Tuple<string, List<NLine>> boolDifferenceTuple = curveBooleanDifferenceTol(lineInputA[i].start, lineInputA[i].end, lineInputB.start, lineInputB.end, tol);
                if (boolDifferenceTuple.Item2.Count > 0)
                    differenceAB.AddRange(boolDifferenceTuple.Item2);
            }
            List<NLine> differenceAB_U = NLine.deleteDuplicateLines(differenceAB);
            return differenceAB_U;
        }

        public static List<NLine> lineListBooleanIntersection(List<NLine> lineInputA, List<NLine> lineInputB)
        {
            List<NLine> intersectionAB = new List<NLine>();

            for (int i = 0; i < lineInputA.Count; i++)
            {
                // go through each line of line B

                for (int j = 0; j < lineInputB.Count; j++)
                {
                    // boolean intersection
                    Tuple<string, List<NLine>> boolIntersectionTuple = curveBooleanIntersection(lineInputA[i].start, lineInputA[i].end, lineInputB[j].start, lineInputB[j].end);
                    intersectionAB.AddRange(boolIntersectionTuple.Item2);
                }

            }
            List<NLine> intersectionAB_U = NLine.deleteDuplicateLines(intersectionAB);
            return intersectionAB_U;
        }

        public static List<NLine> lineListBooleanIntersectionTol(List<NLine> lineInputA, List<NLine> lineInputB, double tol)
        {
            List<NLine> intersectionAB = new List<NLine>();

            for (int i = 0; i < lineInputA.Count; i++)
            {
                // go through each line of line B

                for (int j = 0; j < lineInputB.Count; j++)
                {
                    // boolean intersection
                    Tuple<string, List<NLine>> boolIntersectionTuple = curveBooleanIntersectionTol(lineInputA[i].start, lineInputA[i].end, lineInputB[j].start, lineInputB[j].end, tol);
                    intersectionAB.AddRange(boolIntersectionTuple.Item2);
                }

            }
            List<NLine> intersectionAB_U = NLine.deleteDuplicateLines(intersectionAB);
            return intersectionAB_U;
        }


        // Boolean Single Line Operations
        public static Tuple<string, List<NLine>> curveBooleanDifference(Vec3d a0, Vec3d a1, Vec3d b0, Vec3d b1)
        {
            // returns boolean difference, overlap of a and b, (a - b)

            Tuple<string, Vec3d, Vec3d, NLine, List<NLine>, List<NLine>> intersectionTuple = intersectionQueryLinesOut(a0, a1, b0, b1);

            return new Tuple<string, List<NLine>>(intersectionTuple.Item1, intersectionTuple.Item5);
        }
        public static Tuple<string, List<NLine>> curveBooleanDifferenceTol(Vec3d a0, Vec3d a1, Vec3d b0, Vec3d b1, double tol)
        {
            // returns boolean difference, overlap of a and b, (a - b)

            Tuple<string, Vec3d, Vec3d, NLine, List<NLine>, List<NLine>> intersectionTuple = intersectionQueryLinesOutTol(a0, a1, b0, b1, tol);

            return new Tuple<string, List<NLine>>(intersectionTuple.Item1, intersectionTuple.Item5);
        }
        public static Tuple<string, List<NLine>> curveBooleanIntersection(Vec3d a0, Vec3d a1, Vec3d b0, Vec3d b1)
        {
            // returns boolean intersection of a and b

            Tuple<string, Vec3d, Vec3d, NLine, List<NLine>, List<NLine>> intersectionTuple = intersectionQueryLinesOut(a0, a1, b0, b1);

            List<NLine> outLines = new List<NLine>();
            outLines.Add(intersectionTuple.Item4);

            return new Tuple<string, List<NLine>>(intersectionTuple.Item1, outLines);
        }
        public static Tuple<string, List<NLine>> curveBooleanIntersectionTol(Vec3d a0, Vec3d a1, Vec3d b0, Vec3d b1, double tol)
        {
            // returns boolean intersection of a and b

            Tuple<string, Vec3d, Vec3d, NLine, List<NLine>, List<NLine>> intersectionTuple = intersectionQueryLinesOutTol(a0, a1, b0, b1, tol);

            List<NLine> outLines = new List<NLine>();
            outLines.Add(intersectionTuple.Item4);

            return new Tuple<string, List<NLine>>(intersectionTuple.Item1, outLines);
        }

        public static Tuple<string, List<NLine>> curveBooleanUnion(Vec3d a0, Vec3d a1, Vec3d b0, Vec3d b1)
        {
            // returns boolean union of a and b

            Tuple<string, Vec3d, Vec3d, NLine, List<NLine>, List<NLine>> intersectionTuple = intersectionQueryLinesOut(a0, a1, b0, b1);

            List<NLine> outLines = new List<NLine>();
            outLines.AddRange(intersectionTuple.Item5);
            outLines.Add(intersectionTuple.Item4);
            outLines.AddRange(intersectionTuple.Item6);

            return new Tuple<string, List<NLine>>(intersectionTuple.Item1, outLines);
        }
        public static Tuple<string, List<NLine>> curveBooleanUnion(Vec3d a0, Vec3d a1, Vec3d b0, Vec3d b1, double tol)
        {
            // returns boolean union of a and b

            Tuple<string, Vec3d, Vec3d, NLine, List<NLine>, List<NLine>> intersectionTuple = intersectionQueryLinesOutTol(a0, a1, b0, b1, tol);

            List<NLine> outLines = new List<NLine>();
            outLines.AddRange(intersectionTuple.Item5);
            outLines.Add(intersectionTuple.Item4);
            outLines.AddRange(intersectionTuple.Item6);

            return new Tuple<string, List<NLine>>(intersectionTuple.Item1, outLines);
        }


        // Detailed Intersection Queries
        public static Tuple<string, Vec3d, Vec3d, NLine, List<NLine>, List<NLine>> intersectionQueryLinesOut(Vec3d a0, Vec3d a1, Vec3d b0, Vec3d b1)
        {
            // intersected, intersectedSingle, intersectedSegment, intersectionPointA, intersectionPointB, intersectionSegment, string describing intersection
            bool trim = false; // if trim true, intersections will cut tip (use in separate function, because otherwise booleans wont work anymore)
                               // returns number of intersections
            int numIntersections = 0;
            double tol = 0.0001;
            string outstring = "";

            bool intersected = false;
            bool intersectedSingle = false;
            Vec3d intersectionPointA = Vec3d.Zero;

            bool intersectedSegment = false;
            Vec3d intersectionPointB = Vec3d.Zero;
            NLine intersectionSegment = new NLine(Vec3d.Zero, Vec3d.Zero);

            NLine leftoverA = new NLine(a0, a1);
            NLine leftoverB = new NLine(b0, b1);

            List<NLine> leftoverAList = new List<NLine>();
            List<NLine> leftoverBList = new List<NLine>();

            // calc distances
            double dist_a0_b0 = Vec3d.Distance(a0, b0);
            double dist_a0_b1 = Vec3d.Distance(a0, b1);

            double dist_a1_b0 = Vec3d.Distance(a1, b0);
            double dist_a1_b1 = Vec3d.Distance(a1, b1);

            double dist_a0_a1 = Vec3d.Distance(a0, a1);
            double dist_b0_b1 = Vec3d.Distance(b0, b1);

            // case 1, starts connect, ends are different
            if ((dist_a0_b0 < tol) && (dist_a1_b1 > tol))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 1, starts connect, ends are different";
                intersectionPointA = a0;

                // double distPtStart, double distPtEnd, double distStartEnd, double tol)
                bool inbetweenA = PointLineIntersectionDistTol(dist_a1_b0, dist_a1_b1, dist_b0_b1, tol);
                bool inbetweenB = PointLineIntersectionDistTol(dist_a0_b1, dist_a1_b1, dist_a0_a1, tol);

                if (inbetweenA)
                {
                    outstring += " / AEnd is on BLine";
                    intersectionPointB = a1;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a0, a1);
                    leftoverB = new NLine(a1, b1);
                    leftoverA = new NLine(a0, a0);

                    leftoverBList.Add(leftoverB);
                }
                else if (inbetweenB)
                {
                    outstring += " / BEnd is on ALine";
                    intersectionPointB = b1;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a0, b1);
                    leftoverA = new NLine(b1, a1);
                    leftoverB = new NLine(a0, a0);

                    leftoverAList.Add(leftoverA);
                }
                else
                {
                    // dont intersect
                    leftoverA = new NLine(a0, a1);
                    leftoverB = new NLine(b0, b1);
                    leftoverAList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }

            }

            // case 2, ends connect, starts are different
            else if ((dist_a1_b1 < tol) && (dist_a0_b0 > tol))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 2, ends connect, starts are different";
                intersectionPointA = a1;

                // double distPtStart, double distPtEnd, double distStartEnd, double tol)
                bool inbetweenA = PointLineIntersectionDistTol(dist_a0_b0, dist_a0_b1, dist_b0_b1, tol);
                bool inbetweenB = PointLineIntersectionDistTol(dist_a0_b0, dist_a1_b0, dist_a0_a1, tol);

                if (inbetweenA)
                {
                    outstring += " / AStart is on BLine";
                    intersectionPointB = a0;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a1, a0);
                    leftoverB = new NLine(b0, a0);
                    leftoverA = new NLine(a0, a0);

                    leftoverBList.Add(leftoverB);

                }
                else if (inbetweenB)
                {
                    outstring += " / BStart is on ALine";
                    intersectionPointB = b0;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a1, b0);
                    leftoverA = new NLine(a0, b0);
                    leftoverB = new NLine(b0, b0);

                    leftoverAList.Add(leftoverA);
                }
                else
                {
                    leftoverA = new NLine(a0, a1);
                    leftoverB = new NLine(b0, b1);
                    leftoverAList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }

            }

            // case 3, endstart connect, startsend are different
            else if ((dist_a1_b0 < tol) && (dist_a0_b1 > tol))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 3, endstart connect, startsend are different";
                intersectionPointA = a1;

                // double distPtStart, double distPtEnd, double distStartEnd, double tol)
                bool inbetweenA = PointLineIntersectionDistTol(dist_a0_b0, dist_a0_b1, dist_b0_b1, tol);
                bool inbetweenB = PointLineIntersectionDistTol(dist_a0_b1, dist_a1_b1, dist_a0_a1, tol);

                if (inbetweenA)
                {
                    outstring += " / AStart is on BLine";
                    intersectionPointB = a0;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a0, a1);
                    leftoverA = new NLine(a0, a0);
                    leftoverB = new NLine(a0, b1);
                    leftoverBList.Add(leftoverB);

                }
                else if (inbetweenB)
                {
                    outstring += " / BEnd is on ALine";
                    intersectionPointB = b1;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a1, b1);
                    leftoverB = new NLine(a0, a0);
                    leftoverA = new NLine(a0, b1);
                    leftoverAList.Add(leftoverA);
                }
                else
                {
                    leftoverA = new NLine(a0, a1);
                    leftoverB = new NLine(b0, b1);
                    leftoverAList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }
            }

            // case 4, startend connect, endstart are different
            else if ((dist_a0_b1 < tol) && (dist_a1_b0 > tol))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 4, startend connect, endstart are different";
                intersectionPointA = a0;

                // double distPtStart, double distPtEnd, double distStartEnd, double tol)
                bool inbetweenA = PointLineIntersectionDistTol(dist_a1_b0, dist_a1_b1, dist_b0_b1, tol);
                bool inbetweenB = PointLineIntersectionDistTol(dist_a0_b0, dist_a1_b0, dist_a0_a1, tol);
                if (inbetweenA)
                {
                    outstring += " / AEnd is on BLine";
                    intersectionPointB = a1;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a0, a1);
                    leftoverB = new NLine(b0, a1);
                    leftoverA = new NLine(a0, a0);
                    leftoverBList.Add(leftoverB);
                }
                else if (inbetweenB)
                {
                    outstring += " / BStart is on ALine";
                    intersectionPointB = b0;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a0, b0);
                    leftoverA = new NLine(b0, a1);
                    leftoverB = new NLine(b0, b0);
                    leftoverAList.Add(leftoverA);
                }
                else
                {
                    leftoverA = new NLine(a0, a1);
                    leftoverB = new NLine(b0, b1);
                    leftoverAList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }
            }
            // case 5, all connect, line the same
            else if ((dist_a0_b1 < tol) && (dist_a1_b0 < tol) || (dist_a0_b0 < tol) && (dist_a1_b1 < tol))
            {
                intersected = true;
                intersectedSingle = false;
                intersectedSegment = true;

                outstring = "case 5, all connect, line the same";

                intersectionPointA = a0;
                intersectionPointB = a1;

                intersectionSegment = new NLine(a0, a1);
            }
            // case 6, A is inside B
            else if (PointLineIntersectionDistTol(dist_a0_b0, dist_a0_b1, dist_b0_b1, tol) && PointLineIntersectionDistTol(dist_a1_b0, dist_a1_b1, dist_b0_b1, tol))
            {
                intersected = true;
                intersectedSingle = false;
                intersectedSegment = true;

                outstring = "case 6,  A is inside B";

                intersectionPointA = a0;
                intersectionPointB = a1;

                intersectionSegment = new NLine(a0, a1);
                if (Vec3d.DotProduct(a1 - a0, b1 - b0) > 0)
                {
                    leftoverA = new NLine(b0, a0);
                    leftoverB = new NLine(a1, b1);
                    leftoverBList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }
                else
                {
                    leftoverA = new NLine(b0, a1);
                    leftoverB = new NLine(a0, b1);
                    intersectionSegment = new NLine(a1, a0);
                    leftoverBList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }

            }
            // case 7 B is inside A
            else if (PointLineIntersectionDistTol(dist_a0_b0, dist_a1_b0, dist_a0_a1, tol) && PointLineIntersectionDistTol(dist_a0_b1, dist_a1_b1, dist_a0_a1, tol))
            {
                intersected = true;
                intersectedSingle = false;
                intersectedSegment = true;

                outstring = "case 7, B is inside A";

                intersectionPointA = b0;
                intersectionPointB = b1;

                intersectionSegment = new NLine(b0, b1);

                if (Vec3d.DotProduct(a1 - a0, b1 - b0) > 0)
                {
                    leftoverB = new NLine(a0, b0);
                    leftoverA = new NLine(b1, a1);
                    leftoverAList.Add(leftoverA);
                    leftoverAList.Add(leftoverB);
                }
                else
                {
                    leftoverA = new NLine(b0, a1);
                    leftoverB = new NLine(a0, b1);
                    intersectionSegment = new NLine(b1, b0);

                    leftoverAList.Add(leftoverA);
                    leftoverAList.Add(leftoverB);
                }

            }
            // case 8 collinear but overlapping
            else if (RIntersection.areLineSegmentsCollinear(a0, a1, b0, b1))
            {
                outstring = "case 8 collinear";

                // check if a0 is inside b
                bool inbetweenA0 = PointLineIntersectionDistTol(dist_a0_b0, dist_a0_b1, dist_b0_b1, tol);
                // check if a1 is inside b
                bool inbetweenA1 = PointLineIntersectionDistTol(dist_a1_b0, dist_a1_b1, dist_b0_b1, tol);

                if (inbetweenA0)
                {
                    outstring += " / AStart is on BLine";
                    intersectionPointA = a0;
                    intersected = true;

                    // check if B Start is on ALine
                    if (PointLineIntersectionDistTol(dist_a0_b0, dist_a1_b0, dist_a0_a1, tol))
                    {
                        outstring += " / BStart is on ALine";

                        intersectedSingle = false;
                        intersectedSegment = true;

                        intersectionPointB = b0;

                        intersectionSegment = new NLine(a0, b0);
                        leftoverA = new NLine(b0, a1);
                        leftoverB = new NLine(a0, b1);

                        leftoverAList.Add(leftoverA);
                        leftoverBList.Add(leftoverB);

                    }
                    // check if B End is on BLine
                    if (PointLineIntersectionDistTol(dist_a0_b1, dist_a1_b1, dist_a0_a1, tol))
                    {
                        outstring += " / BEnd is on ALine";

                        intersectedSingle = false;
                        intersectedSegment = true;

                        intersectionPointB = b1;

                        intersectionSegment = new NLine(a0, b1);

                        leftoverA = new NLine(b1, a1);
                        leftoverB = new NLine(b0, a0);

                        leftoverAList.Add(leftoverA);
                        leftoverBList.Add(leftoverB);
                    }
                }

                else if (inbetweenA1)
                {
                    outstring += " / AEnd is on BLine";
                    intersected = true;
                    intersectionPointA = a1;

                    // check if B Start is on ALine
                    if (PointLineIntersectionDistTol(dist_a0_b0, dist_a1_b0, dist_a0_a1, tol))
                    {
                        outstring += " / BStart is on ALine";


                        intersectedSingle = false;
                        intersectedSegment = true;

                        intersectionPointB = b0;

                        intersectionSegment = new NLine(b0, a1);

                        leftoverA = new NLine(a0, b0);
                        leftoverB = new NLine(a1, b1);
                        leftoverAList.Add(leftoverA);
                        leftoverBList.Add(leftoverB);
                    }
                    // check if B End is on BLine
                    if (PointLineIntersectionDistTol(dist_a0_b1, dist_a1_b1, dist_a0_a1, tol))
                    {
                        outstring += " / BEnd is on ALine";

                        intersectedSingle = false;
                        intersectedSegment = true;

                        intersectionPointB = b1;

                        intersectionSegment = new NLine(b1, a1);

                        leftoverA = new NLine(a0, b1);
                        leftoverB = new NLine(b0, a1);
                        leftoverAList.Add(leftoverA);
                        leftoverBList.Add(leftoverB);
                    }
                }

                else
                {
                    outstring += " X no overlap";

                    leftoverA = new NLine(a0, a1);
                    leftoverB = new NLine(b0, b1);
                    leftoverAList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }
            }
            // case 9 a0 touches Line B
            else if ((PointLineIntersectionDistTol(dist_a0_b0, dist_a0_b1, dist_b0_b1, tol) == true) && (PointLineIntersectionDistTol(dist_a1_b0, dist_a1_b1, dist_b0_b1, tol) == false))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 9 a0 touches Line B";
                intersectionPointA = a0;

                if (trim)
                {
                    leftoverB = new NLine(b0, a0);
                    leftoverBList.Add(leftoverB);
                }
                else
                {
                    leftoverAList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }

            }
            // case 10 a1 touches Line B
            else if ((PointLineIntersectionDistTol(dist_a1_b0, dist_a1_b1, dist_b0_b1, tol) == true) && (PointLineIntersectionDistTol(dist_a0_b0, dist_a0_b1, dist_b0_b1, tol) == false))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 10 a1 touches Line B";
                intersectionPointA = a1;

                if (trim)
                {
                    leftoverB = new NLine(b0, a1);
                    leftoverBList.Add(leftoverB);
                }
                else
                {
                    leftoverAList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }
            }
            // case 11 b0 touches Line A
            else if ((PointLineIntersectionDistTol(dist_a0_b0, dist_a1_b0, dist_a0_a1, tol) == true) && (PointLineIntersectionDistTol(dist_a0_b1, dist_a1_b1, dist_a0_a1, tol) == false))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 11 b0 touches Line A";
                intersectionPointA = b0;

                if (trim)
                {
                    leftoverA = new NLine(a0, b0);
                    leftoverAList.Add(leftoverA);
                }
                else
                {
                    leftoverAList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }
            }
            // case 12 b1 touches Line A
            else if ((PointLineIntersectionDistTol(dist_a0_b1, dist_a1_b1, dist_a0_a1, tol) == true) && (PointLineIntersectionDistTol(dist_a0_b0, dist_a1_b0, dist_a0_a1, tol) == false))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 12 b1 touches Line B";
                intersectionPointA = b1;

                if (trim)
                {
                    leftoverA = new NLine(a0, b1);
                    leftoverAList.Add(leftoverA);
                }
                else
                {
                    leftoverAList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }
            }
            // case 13 Line A and B intersect in middle
            else if (RIntersection.AreLinesIntersecting(a0, a1, b0, b1, false))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                Vec3d intersectionPoint = RIntersection.GetLineLineIntersectionPoint(a0, a1, b0, b1);
                outstring = "case 13, Lines intersect in middle at " + intersectionPoint;
                intersectionPointA = intersectionPoint;

                if (trim)
                {
                    leftoverA = new NLine(a0, intersectionPoint);
                    leftoverAList.Add(leftoverA);
                    leftoverB = new NLine(b0, intersectionPoint);
                    leftoverBList.Add(leftoverB);
                }
                else
                {
                    leftoverAList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }

            }
            // case 14 None connect
            else
            {
                outstring = "case 14, none connect";

                // case for collinearity with one point inside the curve.....
                leftoverA = new NLine(a0, a1);
                leftoverB = new NLine(b0, b1);
                leftoverAList.Add(leftoverA);
                leftoverBList.Add(leftoverB);
            }

            return new Tuple<string, Vec3d, Vec3d, NLine, List<NLine>, List<NLine>>(outstring, intersectionPointA, intersectionPointB, intersectionSegment, leftoverAList, leftoverBList);

        }
        public static Tuple<string, Vec3d, Vec3d, NLine, List<NLine>, List<NLine>> intersectionQueryLinesOutTol(Vec3d a0, Vec3d a1, Vec3d b0, Vec3d b1, double tol)
        {
            // intersected, intersectedSingle, intersectedSegment, intersectionPointA, intersectionPointB, intersectionSegment, string describing intersection
            bool trim = false; // if trim true, intersections will cut tip (use in separate function, because otherwise booleans wont work anymore)
                               // returns number of intersections
            int numIntersections = 0;
            //double tol = 0.0001;
            string outstring = "";

            bool intersected = false;
            bool intersectedSingle = false;
            Vec3d intersectionPointA = Vec3d.Zero;

            bool intersectedSegment = false;
            Vec3d intersectionPointB = Vec3d.Zero;
            NLine intersectionSegment = new NLine(Vec3d.Zero, Vec3d.Zero);

            NLine leftoverA = new NLine(a0, a1);
            NLine leftoverB = new NLine(b0, b1);

            List<NLine> leftoverAList = new List<NLine>();
            List<NLine> leftoverBList = new List<NLine>();

            // calc distances
            double dist_a0_b0 = Vec3d.Distance(a0, b0);
            double dist_a0_b1 = Vec3d.Distance(a0, b1);

            double dist_a1_b0 = Vec3d.Distance(a1, b0);
            double dist_a1_b1 = Vec3d.Distance(a1, b1);

            double dist_a0_a1 = Vec3d.Distance(a0, a1);
            double dist_b0_b1 = Vec3d.Distance(b0, b1);

            // case 1, starts connect, ends are different
            if ((dist_a0_b0 < tol) && (dist_a1_b1 > tol))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 1, starts connect, ends are different";
                intersectionPointA = a0;

                // double distPtStart, double distPtEnd, double distStartEnd, double tol)
                bool inbetweenA = PointLineIntersectionDistTol(dist_a1_b0, dist_a1_b1, dist_b0_b1, tol);
                bool inbetweenB = PointLineIntersectionDistTol(dist_a0_b1, dist_a1_b1, dist_a0_a1, tol);

                if (inbetweenA)
                {
                    outstring += " / AEnd is on BLine";
                    intersectionPointB = a1;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a0, a1);
                    leftoverB = new NLine(a1, b1);
                    leftoverA = new NLine(a0, a0);

                    leftoverBList.Add(leftoverB);
                }
                else if (inbetweenB)
                {
                    outstring += " / BEnd is on ALine";
                    intersectionPointB = b1;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a0, b1);
                    leftoverA = new NLine(b1, a1);
                    leftoverB = new NLine(a0, a0);

                    leftoverAList.Add(leftoverA);
                }
                else
                {
                    // dont intersect
                    leftoverA = new NLine(a0, a1);
                    leftoverB = new NLine(b0, b1);
                    leftoverAList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }

            }

            // case 2, ends connect, starts are different
            else if ((dist_a1_b1 < tol) && (dist_a0_b0 > tol))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 2, ends connect, starts are different";
                intersectionPointA = a1;

                // double distPtStart, double distPtEnd, double distStartEnd, double tol)
                bool inbetweenA = PointLineIntersectionDistTol(dist_a0_b0, dist_a0_b1, dist_b0_b1, tol);
                bool inbetweenB = PointLineIntersectionDistTol(dist_a0_b0, dist_a1_b0, dist_a0_a1, tol);

                if (inbetweenA)
                {
                    outstring += " / AStart is on BLine";
                    intersectionPointB = a0;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a1, a0);
                    leftoverB = new NLine(b0, a0);
                    leftoverA = new NLine(a0, a0);

                    leftoverBList.Add(leftoverB);

                }
                else if (inbetweenB)
                {
                    outstring += " / BStart is on ALine";
                    intersectionPointB = b0;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a1, b0);
                    leftoverA = new NLine(a0, b0);
                    leftoverB = new NLine(b0, b0);

                    leftoverAList.Add(leftoverA);
                }
                else
                {
                    leftoverA = new NLine(a0, a1);
                    leftoverB = new NLine(b0, b1);
                    leftoverAList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }

            }

            // case 3, endstart connect, startsend are different
            else if ((dist_a1_b0 < tol) && (dist_a0_b1 > tol))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 3, endstart connect, startsend are different";
                intersectionPointA = a1;

                // double distPtStart, double distPtEnd, double distStartEnd, double tol)
                bool inbetweenA = PointLineIntersectionDistTol(dist_a0_b0, dist_a0_b1, dist_b0_b1, tol);
                bool inbetweenB = PointLineIntersectionDistTol(dist_a0_b1, dist_a1_b1, dist_a0_a1, tol);

                if (inbetweenA)
                {
                    outstring += " / AStart is on BLine";
                    intersectionPointB = a0;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a0, a1);
                    leftoverA = new NLine(a0, a0);
                    leftoverB = new NLine(a0, b1);
                    leftoverBList.Add(leftoverB);

                }
                else if (inbetweenB)
                {
                    outstring += " / BEnd is on ALine";
                    intersectionPointB = b1;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a1, b1);
                    leftoverB = new NLine(a0, a0);
                    leftoverA = new NLine(a0, b1);
                    leftoverAList.Add(leftoverA);
                }
                else
                {
                    leftoverA = new NLine(a0, a1);
                    leftoverB = new NLine(b0, b1);
                    leftoverAList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }
            }

            // case 4, startend connect, endstart are different
            else if ((dist_a0_b1 < tol) && (dist_a1_b0 > tol))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 4, startend connect, endstart are different";
                intersectionPointA = a0;

                // double distPtStart, double distPtEnd, double distStartEnd, double tol)
                bool inbetweenA = PointLineIntersectionDistTol(dist_a1_b0, dist_a1_b1, dist_b0_b1, tol);
                bool inbetweenB = PointLineIntersectionDistTol(dist_a0_b0, dist_a1_b0, dist_a0_a1, tol);
                if (inbetweenA)
                {
                    outstring += " / AEnd is on BLine";
                    intersectionPointB = a1;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a0, a1);
                    leftoverB = new NLine(b0, a1);
                    leftoverA = new NLine(a0, a0);
                    leftoverBList.Add(leftoverB);
                }
                else if (inbetweenB)
                {
                    outstring += " / BStart is on ALine";
                    intersectionPointB = b0;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a0, b0);
                    leftoverA = new NLine(b0, a1);
                    leftoverB = new NLine(b0, b0);
                    leftoverAList.Add(leftoverA);
                }
                else
                {
                    leftoverA = new NLine(a0, a1);
                    leftoverB = new NLine(b0, b1);
                    leftoverAList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }
            }
            // case 5, all connect, line the same
            else if ((dist_a0_b1 < tol) && (dist_a1_b0 < tol) || (dist_a0_b0 < tol) && (dist_a1_b1 < tol))
            {
                intersected = true;
                intersectedSingle = false;
                intersectedSegment = true;

                outstring = "case 5, all connect, line the same";

                intersectionPointA = a0;
                intersectionPointB = a1;

                intersectionSegment = new NLine(a0, a1);
            }
            // case 6, A is inside B
            else if (PointLineIntersectionDistTol(dist_a0_b0, dist_a0_b1, dist_b0_b1, tol) && PointLineIntersectionDistTol(dist_a1_b0, dist_a1_b1, dist_b0_b1, tol))
            {
                intersected = true;
                intersectedSingle = false;
                intersectedSegment = true;

                outstring = "case 6,  A is inside B";

                intersectionPointA = a0;
                intersectionPointB = a1;

                intersectionSegment = new NLine(a0, a1);
                if (Vec3d.DotProduct(a1 - a0, b1 - b0) > 0)
                {
                    leftoverA = new NLine(b0, a0);
                    leftoverB = new NLine(a1, b1);
                    leftoverBList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }
                else
                {
                    leftoverA = new NLine(b0, a1);
                    leftoverB = new NLine(a0, b1);
                    intersectionSegment = new NLine(a1, a0);
                    leftoverBList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }

            }
            // case 7 B is inside A
            else if (PointLineIntersectionDistTol(dist_a0_b0, dist_a1_b0, dist_a0_a1, tol) && PointLineIntersectionDistTol(dist_a0_b1, dist_a1_b1, dist_a0_a1, tol))
            {
                intersected = true;
                intersectedSingle = false;
                intersectedSegment = true;

                outstring = "case 7, B is inside A";

                intersectionPointA = b0;
                intersectionPointB = b1;

                intersectionSegment = new NLine(b0, b1);

                if (Vec3d.DotProduct(a1 - a0, b1 - b0) > 0)
                {
                    leftoverB = new NLine(a0, b0);
                    leftoverA = new NLine(b1, a1);
                    leftoverAList.Add(leftoverA);
                    leftoverAList.Add(leftoverB);
                }
                else
                {
                    leftoverA = new NLine(b0, a1);
                    leftoverB = new NLine(a0, b1);
                    intersectionSegment = new NLine(b1, b0);

                    leftoverAList.Add(leftoverA);
                    leftoverAList.Add(leftoverB);
                }

            }
            // case 8 collinear but overlapping
            else if (RIntersection.areLineSegmentsCollinear(a0, a1, b0, b1))
            {
                outstring = "case 8 collinear";

                // check if a0 is inside b
                bool inbetweenA0 = PointLineIntersectionDistTol(dist_a0_b0, dist_a0_b1, dist_b0_b1, tol);
                // check if a1 is inside b
                bool inbetweenA1 = PointLineIntersectionDistTol(dist_a1_b0, dist_a1_b1, dist_b0_b1, tol);

                if (inbetweenA0)
                {
                    outstring += " / AStart is on BLine";
                    intersectionPointA = a0;
                    intersected = true;

                    // check if B Start is on ALine
                    if (PointLineIntersectionDistTol(dist_a0_b0, dist_a1_b0, dist_a0_a1, tol))
                    {
                        outstring += " / BStart is on ALine";

                        intersectedSingle = false;
                        intersectedSegment = true;

                        intersectionPointB = b0;

                        intersectionSegment = new NLine(a0, b0);
                        leftoverA = new NLine(b0, a1);
                        leftoverB = new NLine(a0, b1);

                        leftoverAList.Add(leftoverA);
                        leftoverBList.Add(leftoverB);

                    }
                    // check if B End is on BLine
                    if (PointLineIntersectionDistTol(dist_a0_b1, dist_a1_b1, dist_a0_a1, tol))
                    {
                        outstring += " / BEnd is on ALine";

                        intersectedSingle = false;
                        intersectedSegment = true;

                        intersectionPointB = b1;

                        intersectionSegment = new NLine(a0, b1);

                        leftoverA = new NLine(b1, a1);
                        leftoverB = new NLine(b0, a0);

                        leftoverAList.Add(leftoverA);
                        leftoverBList.Add(leftoverB);
                    }
                }

                else if (inbetweenA1)
                {
                    outstring += " / AEnd is on BLine";
                    intersected = true;
                    intersectionPointA = a1;

                    // check if B Start is on ALine
                    if (PointLineIntersectionDistTol(dist_a0_b0, dist_a1_b0, dist_a0_a1, tol))
                    {
                        outstring += " / BStart is on ALine";


                        intersectedSingle = false;
                        intersectedSegment = true;

                        intersectionPointB = b0;

                        intersectionSegment = new NLine(b0, a1);

                        leftoverA = new NLine(a0, b0);
                        leftoverB = new NLine(a1, b1);
                        leftoverAList.Add(leftoverA);
                        leftoverBList.Add(leftoverB);
                    }
                    // check if B End is on BLine
                    if (PointLineIntersectionDistTol(dist_a0_b1, dist_a1_b1, dist_a0_a1, tol))
                    {
                        outstring += " / BEnd is on ALine";

                        intersectedSingle = false;
                        intersectedSegment = true;

                        intersectionPointB = b1;

                        intersectionSegment = new NLine(b1, a1);

                        leftoverA = new NLine(a0, b1);
                        leftoverB = new NLine(b0, a1);
                        leftoverAList.Add(leftoverA);
                        leftoverBList.Add(leftoverB);
                    }
                }

                else
                {
                    outstring += " X no overlap";

                    leftoverA = new NLine(a0, a1);
                    leftoverB = new NLine(b0, b1);
                    leftoverAList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }
            }
            // case 9 a0 touches Line B
            else if ((PointLineIntersectionDistTol(dist_a0_b0, dist_a0_b1, dist_b0_b1, tol) == true) && (PointLineIntersectionDistTol(dist_a1_b0, dist_a1_b1, dist_b0_b1, tol) == false))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 9 a0 touches Line B";
                intersectionPointA = a0;

                if (trim)
                {
                    leftoverB = new NLine(b0, a0);
                    leftoverBList.Add(leftoverB);
                }
                else
                {
                    leftoverAList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }

            }
            // case 10 a1 touches Line B
            else if ((PointLineIntersectionDistTol(dist_a1_b0, dist_a1_b1, dist_b0_b1, tol) == true) && (PointLineIntersectionDistTol(dist_a0_b0, dist_a0_b1, dist_b0_b1, tol) == false))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 10 a1 touches Line B";
                intersectionPointA = a1;

                if (trim)
                {
                    leftoverB = new NLine(b0, a1);
                    leftoverBList.Add(leftoverB);
                }
                else
                {
                    leftoverAList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }
            }
            // case 11 b0 touches Line A
            else if ((PointLineIntersectionDistTol(dist_a0_b0, dist_a1_b0, dist_a0_a1, tol) == true) && (PointLineIntersectionDistTol(dist_a0_b1, dist_a1_b1, dist_a0_a1, tol) == false))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 11 b0 touches Line A";
                intersectionPointA = b0;

                if (trim)
                {
                    leftoverA = new NLine(a0, b0);
                    leftoverAList.Add(leftoverA);
                }
                else
                {
                    leftoverAList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }
            }
            // case 12 b1 touches Line A
            else if ((PointLineIntersectionDistTol(dist_a0_b1, dist_a1_b1, dist_a0_a1, tol) == true) && (PointLineIntersectionDistTol(dist_a0_b0, dist_a1_b0, dist_a0_a1, tol) == false))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 12 b1 touches Line B";
                intersectionPointA = b1;

                if (trim)
                {
                    leftoverA = new NLine(a0, b1);
                    leftoverAList.Add(leftoverA);
                }
                else
                {
                    leftoverAList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }
            }
            // case 13 Line A and B intersect in middle
            else if (RIntersection.AreLinesIntersecting(a0, a1, b0, b1, false))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                Vec3d intersectionPoint = RIntersection.GetLineLineIntersectionPoint(a0, a1, b0, b1);
                outstring = "case 13, Lines intersect in middle at " + intersectionPoint;
                intersectionPointA = intersectionPoint;

                if (trim)
                {
                    leftoverA = new NLine(a0, intersectionPoint);
                    leftoverAList.Add(leftoverA);
                    leftoverB = new NLine(b0, intersectionPoint);
                    leftoverBList.Add(leftoverB);
                }
                else
                {
                    leftoverAList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }

            }
            // case 14 None connect
            else
            {
                outstring = "case 14, none connect";

                // case for collinearity with one point inside the curve.....
                leftoverA = new NLine(a0, a1);
                leftoverB = new NLine(b0, b1);
                leftoverAList.Add(leftoverA);
                leftoverBList.Add(leftoverB);
            }

            return new Tuple<string, Vec3d, Vec3d, NLine, List<NLine>, List<NLine>>(outstring, intersectionPointA, intersectionPointB, intersectionSegment, leftoverAList, leftoverBList);

        }
        public static Tuple<bool, bool, bool, Vec3d, Vec3d, NLine, string> intersectionQuery(Vec3d a0, Vec3d a1, Vec3d b0, Vec3d b1)
        {
            // intersected, intersectedSingle, intersectedSegment, intersectionPointA, intersectionPointB, intersectionSegment, string describing intersection

            // returns number of intersections
            int numIntersections = 0;
            double tol = 0.0001;
            string outstring = "";

            bool intersected = false;
            bool intersectedSingle = false;
            Vec3d intersectionPointA = Vec3d.Zero;

            bool intersectedSegment = false;
            Vec3d intersectionPointB = Vec3d.Zero;
            NLine intersectionSegment = new NLine(Vec3d.Zero, Vec3d.Zero);

            // calc distances
            double dist_a0_b0 = Vec3d.Distance(a0, b0);
            double dist_a0_b1 = Vec3d.Distance(a0, b1);

            double dist_a1_b0 = Vec3d.Distance(a1, b0);
            double dist_a1_b1 = Vec3d.Distance(a1, b1);

            double dist_a0_a1 = Vec3d.Distance(a0, a1);
            double dist_b0_b1 = Vec3d.Distance(b0, b1);

            // case 1, starts connect, ends are different
            if ((dist_a0_b0 < tol) && (dist_a1_b1 > tol))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 1, starts connect, ends are different";
                intersectionPointA = a0;

                // double distPtStart, double distPtEnd, double distStartEnd, double tol)
                bool inbetweenA = PointLineIntersectionDistTol(dist_a1_b0, dist_a1_b1, dist_b0_b1, tol);
                if (inbetweenA)
                {
                    outstring += " / AEnd is on BLine";
                    intersectionPointB = a1;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a0, a1);
                }

                bool inbetweenB = PointLineIntersectionDistTol(dist_a0_b1, dist_a1_b1, dist_a0_a1, tol);
                if (inbetweenB)
                {
                    outstring += " / BEnd is on ALine";
                    intersectionPointB = b1;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a0, b1);
                }

            }

            // case 2, ends connect, starts are different
            else if ((dist_a1_b1 < tol) && (dist_a0_b0 > tol))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 2, ends connect, starts are different";
                intersectionPointA = a1;

                // double distPtStart, double distPtEnd, double distStartEnd, double tol)
                bool inbetweenA = PointLineIntersectionDistTol(dist_a0_b0, dist_a0_b1, dist_b0_b1, tol);
                if (inbetweenA)
                {
                    outstring += " / AStart is on BLine";
                    intersectionPointB = a0;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a1, a0);
                }
                bool inbetweenB = PointLineIntersectionDistTol(dist_a0_b0, dist_a1_b0, dist_a0_a1, tol);
                if (inbetweenB)
                {
                    outstring += " / BStart is on ALine";
                    intersectionPointB = b0;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a1, b0);
                }

            }

            // case 3, endstart connect, startsend are different
            else if ((dist_a1_b0 < tol) && (dist_a0_b1 > tol))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 3, endstart connect, startsend are different";
                intersectionPointA = a1;

                // double distPtStart, double distPtEnd, double distStartEnd, double tol)
                bool inbetweenA = PointLineIntersectionDistTol(dist_a0_b0, dist_a0_b1, dist_b0_b1, tol);
                if (inbetweenA)
                {
                    outstring += " / AStart is on BLine";
                    intersectionPointB = a0;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a1, a0);
                }
                bool inbetweenB = PointLineIntersectionDistTol(dist_a0_b1, dist_a1_b1, dist_a0_a1, tol);
                if (inbetweenB)
                {
                    outstring += " / BEnd is on ALine";
                    intersectionPointB = b1;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a1, b1);
                }
            }

            // case 4, startend connect, endstart are different
            else if ((dist_a0_b1 < tol) && (dist_a1_b0 > tol))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 4, startend connect, endstart are different";
                intersectionPointA = a0;

                // double distPtStart, double distPtEnd, double distStartEnd, double tol)
                bool inbetweenA = PointLineIntersectionDistTol(dist_a1_b0, dist_a1_b1, dist_b0_b1, tol);
                if (inbetweenA)
                {
                    outstring += " / AEnd is on BLine";
                    intersectionPointB = a1;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a0, a1);
                }
                bool inbetweenB = PointLineIntersectionDistTol(dist_a0_b0, dist_a1_b0, dist_a0_a1, tol);
                if (inbetweenB)
                {
                    outstring += " / BStart is on ALine";
                    intersectionPointB = b0;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a0, b0);
                }
            }
            // case 5, all connect, line the same
            else if ((dist_a0_b1 < tol) && (dist_a1_b0 < tol) || (dist_a0_b0 < tol) && (dist_a1_b1 < tol))
            {
                intersected = true;
                intersectedSingle = false;
                intersectedSegment = true;

                outstring = "case 5, all connect, line the same";

                intersectionPointA = a0;
                intersectionPointB = a1;

                intersectionSegment = new NLine(a0, a1);
            }
            // case 6, A is inside B
            else if (PointLineIntersectionDistTol(dist_a0_b0, dist_a0_b1, dist_b0_b1, tol) && PointLineIntersectionDistTol(dist_a1_b0, dist_a1_b1, dist_b0_b1, tol))
            {
                intersected = true;
                intersectedSingle = false;
                intersectedSegment = true;

                outstring = "case 6,  A is inside B";

                intersectionPointA = a0;
                intersectionPointB = a1;

                intersectionSegment = new NLine(a0, a1);
            }
            // case 7 B is inside A
            else if (PointLineIntersectionDistTol(dist_a0_b0, dist_a1_b0, dist_a0_a1, tol) && PointLineIntersectionDistTol(dist_a0_b1, dist_a1_b1, dist_a0_a1, tol))
            {
                intersected = true;
                intersectedSingle = false;
                intersectedSegment = true;

                outstring = "case 7, B is inside A";

                intersectionPointA = b0;
                intersectionPointB = b1;

                intersectionSegment = new NLine(b0, b1);
            }
            // case 8 collinear but overlapping
            else if (RIntersection.areLineSegmentsCollinear(a0, a1, b0, b1))
            {
                outstring = "case 8 collinear";

                // check if a0 is inside b  
                bool inbetweenA0 = PointLineIntersectionDistTol(dist_a0_b0, dist_a0_b1, dist_b0_b1, tol);
                // check if a1 is inside b
                bool inbetweenA1 = PointLineIntersectionDistTol(dist_a1_b0, dist_a1_b1, dist_b0_b1, tol);

                if (inbetweenA0)
                {
                    outstring += " / AStart is on BLine";
                    intersectionPointA = a0;
                    intersected = true;

                    // check if B Start is on ALine
                    if (PointLineIntersectionDistTol(dist_a0_b0, dist_a1_b0, dist_a0_a1, tol))
                    {
                        outstring += " / BStart is on ALine";

                        intersectedSingle = false;
                        intersectedSegment = true;

                        intersectionPointB = b0;

                        intersectionSegment = new NLine(a0, b0);
                    }
                    // check if B End is on BLine
                    if (PointLineIntersectionDistTol(dist_a0_b1, dist_a1_b1, dist_a0_a1, tol))
                    {
                        outstring += " / BEnd is on ALine";

                        intersectedSingle = false;
                        intersectedSegment = true;

                        intersectionPointB = b1;

                        intersectionSegment = new NLine(a0, b1);
                    }
                }

                else if (inbetweenA1)
                {
                    outstring += " / AEnd is on BLine";
                    intersected = true;
                    intersectionPointA = a1;

                    // check if B Start is on ALine
                    if (PointLineIntersectionDistTol(dist_a0_b0, dist_a1_b0, dist_a0_a1, tol))
                    {
                        outstring += " / BStart is on ALine";


                        intersectedSingle = false;
                        intersectedSegment = true;

                        intersectionPointB = b0;

                        intersectionSegment = new NLine(a1, b0);
                    }
                    // check if B End is on BLine
                    if (PointLineIntersectionDistTol(dist_a0_b1, dist_a1_b1, dist_a0_a1, tol))
                    {
                        outstring += " / BEnd is on ALine";

                        intersectedSingle = false;
                        intersectedSegment = true;

                        intersectionPointB = b1;

                        intersectionSegment = new NLine(a1, b1);
                    }
                }

                else
                    outstring += " X no overlap";

            }
            // case 9 a0 touches Line B
            else if ((PointLineIntersectionDistTol(dist_a0_b0, dist_a0_b1, dist_b0_b1, tol) == true) && (PointLineIntersectionDistTol(dist_a1_b0, dist_a1_b1, dist_b0_b1, tol) == false))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 9 a0 touches Line B";
                intersectionPointA = a0;

            }
            // case 10 a1 touches Line B
            else if ((PointLineIntersectionDistTol(dist_a1_b0, dist_a1_b1, dist_b0_b1, tol) == true) && (PointLineIntersectionDistTol(dist_a0_b0, dist_a0_b1, dist_b0_b1, tol) == false))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 10 a1 touches Line B";
                intersectionPointA = a1;
            }
            // case 11 b0 touches Line A
            else if ((PointLineIntersectionDistTol(dist_a0_b0, dist_a1_b0, dist_a0_a1, tol) == true) && (PointLineIntersectionDistTol(dist_a0_b1, dist_a1_b1, dist_a0_a1, tol) == false))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 11 b0 touches Line B";
                intersectionPointA = b0;
            }
            // case 12 b1 touches Line A
            else if ((PointLineIntersectionDistTol(dist_a0_b1, dist_a1_b1, dist_a0_a1, tol) == true) && (PointLineIntersectionDistTol(dist_a0_b0, dist_a1_b0, dist_a0_a1, tol) == false))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 12 b1 touches Line B";
                intersectionPointA = b1;

            }
            // case 13 Line A and B intersect in middle
            else if (RIntersection.AreLinesIntersecting(a0, a1, b0, b1, false))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                Vec3d intersectionPoint = RIntersection.GetLineLineIntersectionPoint(a0, a1, b0, b1);
                outstring = "case 13, Lines intersect in middle at " + intersectionPoint;
                intersectionPointA = intersectionPoint;

            }
            // case 14 None connect
            else
            {
                outstring = "case 14, none connect";

                // case for collinearity with one point inside the curve..... 
            }

            return new Tuple<bool, bool, bool, Vec3d, Vec3d, NLine, string>(intersected, intersectedSingle, intersectedSegment, intersectionPointA, intersectionPointB, intersectionSegment, outstring);

        }

        // Trim functions
        public static NLine trimNLineWithNFaceNEW(NLine tempLine, NFace inputFace)
        {
            // intersect line with NFace
            List<NLine> leftovers = new List<NLine>();
            for (int i = 0; i < inputFace.edgeList.Count; i++)
            {
                Tuple<string, Vec3d, Vec3d, NLine, List<NLine>, List<NLine>> intersectionTupleX = trimQueryLinesOut(tempLine.start, tempLine.end, inputFace.edgeList[i].v, inputFace.edgeList[i].nextNEdge.v);
                leftovers.AddRange(intersectionTupleX.Item5);
            }
            // get shortest
            List<NLine> sortedLeftovers = leftovers.OrderBy(x => x.Length).ToList();
            return sortedLeftovers[0];
        }
        public static Tuple<string, Vec3d, Vec3d, NLine, List<NLine>, List<NLine>> trimQueryLinesOut(Vec3d a0, Vec3d a1, Vec3d b0, Vec3d b1)
        {
            // intersected, intersectedSingle, intersectedSegment, intersectionPointA, intersectionPointB, intersectionSegment, string describing intersection
            bool trim = true; // if trim true, intersections will cut tip (use in separate function, because otherwise booleans wont work anymore)
                              // returns number of intersections
            int numIntersections = 0;
            double tol = 0.0001;
            string outstring = "";

            bool intersected = false;
            bool intersectedSingle = false;
            Vec3d intersectionPointA = Vec3d.Zero;

            bool intersectedSegment = false;
            Vec3d intersectionPointB = Vec3d.Zero;
            NLine intersectionSegment = new NLine(Vec3d.Zero, Vec3d.Zero);

            NLine leftoverA = new NLine(a0, a1);
            NLine leftoverB = new NLine(b0, b1);

            List<NLine> leftoverAList = new List<NLine>();
            List<NLine> leftoverBList = new List<NLine>();

            // calc distances
            double dist_a0_b0 = Vec3d.Distance(a0, b0);
            double dist_a0_b1 = Vec3d.Distance(a0, b1);

            double dist_a1_b0 = Vec3d.Distance(a1, b0);
            double dist_a1_b1 = Vec3d.Distance(a1, b1);

            double dist_a0_a1 = Vec3d.Distance(a0, a1);
            double dist_b0_b1 = Vec3d.Distance(b0, b1);

            // case 1, starts connect, ends are different
            if ((dist_a0_b0 < tol) && (dist_a1_b1 > tol))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 1, starts connect, ends are different";
                intersectionPointA = a0;

                // double distPtStart, double distPtEnd, double distStartEnd, double tol)
                bool inbetweenA = RIntersection.PointLineIntersectionDistTol(dist_a1_b0, dist_a1_b1, dist_b0_b1, tol);
                bool inbetweenB = RIntersection.PointLineIntersectionDistTol(dist_a0_b1, dist_a1_b1, dist_a0_a1, tol);

                if (inbetweenA)
                {
                    outstring += " / AEnd is on BLine";
                    intersectionPointB = a1;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a0, a1);
                    leftoverB = new NLine(a1, b1);
                    leftoverA = new NLine(a0, a0);

                    leftoverBList.Add(leftoverB);
                }
                else if (inbetweenB)
                {
                    outstring += " / BEnd is on ALine";
                    intersectionPointB = b1;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a0, b1);
                    leftoverA = new NLine(b1, a1);
                    leftoverB = new NLine(a0, a0);

                    leftoverAList.Add(leftoverA);
                }
                else
                {
                    // dont intersect
                    leftoverA = new NLine(a0, a1);
                    leftoverB = new NLine(b0, b1);
                    leftoverAList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }

            }

            // case 2, ends connect, starts are different
            else if ((dist_a1_b1 < tol) && (dist_a0_b0 > tol))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 2, ends connect, starts are different";
                intersectionPointA = a1;

                // double distPtStart, double distPtEnd, double distStartEnd, double tol)
                bool inbetweenA = RIntersection.PointLineIntersectionDistTol(dist_a0_b0, dist_a0_b1, dist_b0_b1, tol);
                bool inbetweenB = RIntersection.PointLineIntersectionDistTol(dist_a0_b0, dist_a1_b0, dist_a0_a1, tol);

                if (inbetweenA)
                {
                    outstring += " / AStart is on BLine";
                    intersectionPointB = a0;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a1, a0);
                    leftoverB = new NLine(b0, a0);
                    leftoverA = new NLine(a0, a0);

                    leftoverBList.Add(leftoverB);

                }
                else if (inbetweenB)
                {
                    outstring += " / BStart is on ALine";
                    intersectionPointB = b0;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a1, b0);
                    leftoverA = new NLine(a0, b0);
                    leftoverB = new NLine(b0, b0);

                    leftoverAList.Add(leftoverA);
                }
                else
                {
                    leftoverA = new NLine(a0, a1);
                    leftoverB = new NLine(b0, b1);
                    leftoverAList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }

            }

            // case 3, endstart connect, startsend are different
            else if ((dist_a1_b0 < tol) && (dist_a0_b1 > tol))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 3, endstart connect, startsend are different";
                intersectionPointA = a1;

                // double distPtStart, double distPtEnd, double distStartEnd, double tol)
                bool inbetweenA = RIntersection.PointLineIntersectionDistTol(dist_a0_b0, dist_a0_b1, dist_b0_b1, tol);
                bool inbetweenB = RIntersection.PointLineIntersectionDistTol(dist_a0_b1, dist_a1_b1, dist_a0_a1, tol);

                if (inbetweenA)
                {
                    outstring += " / AStart is on BLine";
                    intersectionPointB = a0;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a0, a1);
                    leftoverA = new NLine(a0, a0);
                    leftoverB = new NLine(a0, b1);
                    leftoverBList.Add(leftoverB);

                }
                else if (inbetweenB)
                {
                    outstring += " / BEnd is on ALine";
                    intersectionPointB = b1;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a1, b1);
                    leftoverB = new NLine(a0, a0);
                    leftoverA = new NLine(a0, b1);
                    leftoverAList.Add(leftoverA);
                }
                else
                {
                    leftoverA = new NLine(a0, a1);
                    leftoverB = new NLine(b0, b1);
                    leftoverAList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }
            }

            // case 4, startend connect, endstart are different
            else if ((dist_a0_b1 < tol) && (dist_a1_b0 > tol))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 4, startend connect, endstart are different";
                intersectionPointA = a0;

                // double distPtStart, double distPtEnd, double distStartEnd, double tol)
                bool inbetweenA = RIntersection.PointLineIntersectionDistTol(dist_a1_b0, dist_a1_b1, dist_b0_b1, tol);
                bool inbetweenB = RIntersection.PointLineIntersectionDistTol(dist_a0_b0, dist_a1_b0, dist_a0_a1, tol);
                if (inbetweenA)
                {
                    outstring += " / AEnd is on BLine";
                    intersectionPointB = a1;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a0, a1);
                    leftoverB = new NLine(b0, a1);
                    leftoverA = new NLine(a0, a0);
                    leftoverBList.Add(leftoverB);
                }
                else if (inbetweenB)
                {
                    outstring += " / BStart is on ALine";
                    intersectionPointB = b0;

                    intersectedSingle = false;
                    intersectedSegment = true;
                    intersectionSegment = new NLine(a0, b0);
                    leftoverA = new NLine(b0, a1);
                    leftoverB = new NLine(b0, b0);
                    leftoverAList.Add(leftoverA);
                }
                else
                {
                    leftoverA = new NLine(a0, a1);
                    leftoverB = new NLine(b0, b1);
                    leftoverAList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }
            }
            // case 5, all connect, line the same
            else if ((dist_a0_b1 < tol) && (dist_a1_b0 < tol) || (dist_a0_b0 < tol) && (dist_a1_b1 < tol))
            {
                intersected = true;
                intersectedSingle = false;
                intersectedSegment = true;

                outstring = "case 5, all connect, line the same";

                intersectionPointA = a0;
                intersectionPointB = a1;

                intersectionSegment = new NLine(a0, a1);
            }
            // case 6, A is inside B
            else if (RIntersection.PointLineIntersectionDistTol(dist_a0_b0, dist_a0_b1, dist_b0_b1, tol) && RIntersection.PointLineIntersectionDistTol(dist_a1_b0, dist_a1_b1, dist_b0_b1, tol))
            {
                intersected = true;
                intersectedSingle = false;
                intersectedSegment = true;

                outstring = "case 6,  A is inside B";

                intersectionPointA = a0;
                intersectionPointB = a1;

                intersectionSegment = new NLine(a0, a1);
                if (Vec3d.DotProduct(a1 - a0, b1 - b0) > 0)
                {
                    leftoverA = new NLine(b0, a0);
                    leftoverB = new NLine(a1, b1);
                    leftoverBList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }
                else
                {
                    leftoverA = new NLine(b0, a1);
                    leftoverB = new NLine(a0, b1);
                    intersectionSegment = new NLine(a1, a0);
                    leftoverBList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }

            }
            // case 7 B is inside A
            else if (RIntersection.PointLineIntersectionDistTol(dist_a0_b0, dist_a1_b0, dist_a0_a1, tol) && RIntersection.PointLineIntersectionDistTol(dist_a0_b1, dist_a1_b1, dist_a0_a1, tol))
            {
                intersected = true;
                intersectedSingle = false;
                intersectedSegment = true;

                outstring = "case 7, B is inside A";

                intersectionPointA = b0;
                intersectionPointB = b1;

                intersectionSegment = new NLine(b0, b1);

                if (Vec3d.DotProduct(a1 - a0, b1 - b0) > 0)
                {
                    leftoverB = new NLine(a0, b0);
                    leftoverA = new NLine(b1, a1);
                    leftoverAList.Add(leftoverA);
                    leftoverAList.Add(leftoverB);
                }
                else
                {
                    leftoverA = new NLine(b0, a1);
                    leftoverB = new NLine(a0, b1);
                    intersectionSegment = new NLine(b1, b0);

                    leftoverAList.Add(leftoverA);
                    leftoverAList.Add(leftoverB);
                }

            }
            // case 8 collinear but overlapping
            else if (RIntersection.areLineSegmentsCollinear(a0, a1, b0, b1))
            {
                outstring = "case 8 collinear";

                // check if a0 is inside b
                bool inbetweenA0 = RIntersection.PointLineIntersectionDistTol(dist_a0_b0, dist_a0_b1, dist_b0_b1, tol);
                // check if a1 is inside b
                bool inbetweenA1 = RIntersection.PointLineIntersectionDistTol(dist_a1_b0, dist_a1_b1, dist_b0_b1, tol);

                if (inbetweenA0)
                {
                    outstring += " / AStart is on BLine";
                    intersectionPointA = a0;
                    intersected = true;

                    // check if B Start is on ALine
                    if (RIntersection.PointLineIntersectionDistTol(dist_a0_b0, dist_a1_b0, dist_a0_a1, tol))
                    {
                        outstring += " / BStart is on ALine";

                        intersectedSingle = false;
                        intersectedSegment = true;

                        intersectionPointB = b0;

                        intersectionSegment = new NLine(a0, b0);
                        leftoverA = new NLine(b0, a1);
                        leftoverB = new NLine(a0, b1);

                        leftoverAList.Add(leftoverA);
                        leftoverBList.Add(leftoverB);

                    }
                    // check if B End is on BLine
                    if (RIntersection.PointLineIntersectionDistTol(dist_a0_b1, dist_a1_b1, dist_a0_a1, tol))
                    {
                        outstring += " / BEnd is on ALine";

                        intersectedSingle = false;
                        intersectedSegment = true;

                        intersectionPointB = b1;

                        intersectionSegment = new NLine(a0, b1);

                        leftoverA = new NLine(b1, a1);
                        leftoverB = new NLine(b0, a0);

                        leftoverAList.Add(leftoverA);
                        leftoverBList.Add(leftoverB);
                    }
                }

                else if (inbetweenA1)
                {
                    outstring += " / AEnd is on BLine";
                    intersected = true;
                    intersectionPointA = a1;

                    // check if B Start is on ALine
                    if (RIntersection.PointLineIntersectionDistTol(dist_a0_b0, dist_a1_b0, dist_a0_a1, tol))
                    {
                        outstring += " / BStart is on ALine";


                        intersectedSingle = false;
                        intersectedSegment = true;

                        intersectionPointB = b0;

                        intersectionSegment = new NLine(b0, a1);

                        leftoverA = new NLine(a0, b0);
                        leftoverB = new NLine(a1, b1);
                        leftoverAList.Add(leftoverA);
                        leftoverBList.Add(leftoverB);
                    }
                    // check if B End is on BLine
                    if (RIntersection.PointLineIntersectionDistTol(dist_a0_b1, dist_a1_b1, dist_a0_a1, tol))
                    {
                        outstring += " / BEnd is on ALine";

                        intersectedSingle = false;
                        intersectedSegment = true;

                        intersectionPointB = b1;

                        intersectionSegment = new NLine(b1, a1);

                        leftoverA = new NLine(a0, b1);
                        leftoverB = new NLine(b0, a1);
                        leftoverAList.Add(leftoverA);
                        leftoverBList.Add(leftoverB);
                    }
                }

                else
                {
                    outstring += " X no overlap";

                    leftoverA = new NLine(a0, a1);
                    leftoverB = new NLine(b0, b1);
                    leftoverAList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }
            }
            // case 9 a0 touches Line B
            else if ((RIntersection.PointLineIntersectionDistTol(dist_a0_b0, dist_a0_b1, dist_b0_b1, tol) == true) && (RIntersection.PointLineIntersectionDistTol(dist_a1_b0, dist_a1_b1, dist_b0_b1, tol) == false))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 9 a0 touches Line B";
                intersectionPointA = a0;

                if (trim)
                {
                    leftoverB = new NLine(b0, a0);
                    leftoverBList.Add(leftoverB);
                }
                else
                {
                    leftoverAList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }

            }
            // case 10 a1 touches Line B
            else if ((RIntersection.PointLineIntersectionDistTol(dist_a1_b0, dist_a1_b1, dist_b0_b1, tol) == true) && (RIntersection.PointLineIntersectionDistTol(dist_a0_b0, dist_a0_b1, dist_b0_b1, tol) == false))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 10 a1 touches Line B";
                intersectionPointA = a1;

                if (trim)
                {
                    leftoverB = new NLine(b0, a1);
                    leftoverBList.Add(leftoverB);
                }
                else
                {
                    leftoverAList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }
            }
            // case 11 b0 touches Line A
            else if ((RIntersection.PointLineIntersectionDistTol(dist_a0_b0, dist_a1_b0, dist_a0_a1, tol) == true) && (RIntersection.PointLineIntersectionDistTol(dist_a0_b1, dist_a1_b1, dist_a0_a1, tol) == false))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 11 b0 touches Line A";
                intersectionPointA = b0;

                if (trim)
                {
                    leftoverA = new NLine(a0, b0);
                    leftoverAList.Add(leftoverA);
                }
                else
                {
                    leftoverAList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }
            }
            // case 12 b1 touches Line A
            else if ((RIntersection.PointLineIntersectionDistTol(dist_a0_b1, dist_a1_b1, dist_a0_a1, tol) == true) && (RIntersection.PointLineIntersectionDistTol(dist_a0_b0, dist_a1_b0, dist_a0_a1, tol) == false))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                outstring = "case 12 b1 touches Line B";
                intersectionPointA = b1;

                if (trim)
                {
                    leftoverA = new NLine(a0, b1);
                    leftoverAList.Add(leftoverA);
                }
                else
                {
                    leftoverAList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }
            }
            // case 13 Line A and B intersect in middle
            else if (RIntersection.AreLinesIntersecting(a0, a1, b0, b1, false))
            {
                intersected = true;
                intersectedSingle = true;
                intersectedSegment = false;

                Vec3d intersectionPoint = RIntersection.GetLineLineIntersectionPoint(a0, a1, b0, b1);
                outstring = "case 13, Lines intersect in middle at " + intersectionPoint;
                intersectionPointA = intersectionPoint;

                if (trim)
                {
                    leftoverA = new NLine(a0, intersectionPoint);
                    leftoverAList.Add(leftoverA);
                    leftoverB = new NLine(b0, intersectionPoint);
                    leftoverBList.Add(leftoverB);
                }
                else
                {
                    leftoverAList.Add(leftoverA);
                    leftoverBList.Add(leftoverB);
                }

            }
            // case 14 None connect
            else
            {
                outstring = "case 14, none connect";

                // case for collinearity with one point inside the curve.....
                leftoverA = new NLine(a0, a1);
                leftoverB = new NLine(b0, b1);
                leftoverAList.Add(leftoverA);
                leftoverBList.Add(leftoverB);
            }

            return new Tuple<string, Vec3d, Vec3d, NLine, List<NLine>, List<NLine>>(outstring, intersectionPointA, intersectionPointB, intersectionSegment, leftoverAList, leftoverBList);

        }
        public static List<NLine> trimMultipleLinesWithFace(List<NLine> inputLines, NFace inputFace)
        {
            List<NLine> outLines = new List<NLine>();
            for (int i = 0; i < inputLines.Count; i++)
            {
                NLine tempLine = trimNLineWithNFaceNEW(inputLines[i], inputFace);
                outLines.Add(tempLine);
            }
            return outLines;
        }

        // Conform Lines
        public static List<NLine> conformLinesToNFace(List<NLine> lineList, NFace inputFace)
        {
            // conforms lines on outline of face 
            List<NLine> faceList = RhConvert.NFaceToLineList(inputFace);
            List<NLine> booleanList = RIntersection.lineListBooleanIntersection(faceList, lineList);
            return booleanList;
        }
        

        // Vec3d functions
        public static bool IsPointInTriangle(Vec3d p1, Vec3d p2, Vec3d p3, Vec3d p)
        {
            //From http //totologic blogspot se/2014/01/accurate-point-in-triangle-test.html
            //p is the testpoint, and the other points are corners in the triangle

            bool isWithinTriangle = false;

            //Based on Barycentric coordinates
            double denominator = ((p2.Y - p3.Y) * (p1.X - p3.X) + (p3.X - p2.X) * (p1.Y - p3.Y));

            double a = ((p2.Y - p3.Y) * (p.X - p3.X) + (p3.X - p2.X) * (p.Y - p3.Y)) / denominator;
            double b = ((p3.Y - p1.Y) * (p.X - p3.X) + (p1.X - p3.X) * (p.Y - p3.Y)) / denominator;
            double c = 1 - a - b;

            //The point is within the triangle
            if (a > 0f && a < 1f && b > 0f && b < 1f && c > 0f && c < 1f)
            {
                isWithinTriangle = true;
            }

            return isWithinTriangle;
        }
        public static bool PointLineIntersectionTol(Vec3d pt, Vec3d lineStart, Vec3d lineEnd, double tol)
        {
            double distStart = Vec3d.Distance(pt, lineStart);
            double distEnd = Vec3d.Distance(pt, lineEnd);
            double distLength = Vec3d.Distance(lineStart, lineEnd);

            if (distStart + distEnd > distLength - tol && distStart + distEnd < distLength + tol)
                return true;
            else
                return false;
        }
        public static bool AreLinesIntersecting(Vec3d l1_p1, Vec3d l1_p2, Vec3d l2_p1, Vec3d l2_p2, bool shouldIncludeEndPoints)
        {
            //To avoid floating point precision issues we can add a small value
            float epsilon = 0.000001f;

            bool isIntersecting = false;

            double denominator = (l2_p2.Y - l2_p1.Y) * (l1_p2.X - l1_p1.X) - (l2_p2.X - l2_p1.X) * (l1_p2.Y - l1_p1.Y);

            //Make sure the denominator is > 0, if not the lines are parallel
            if (denominator != 0f)
            {
                double u_a = ((l2_p2.X - l2_p1.X) * (l1_p1.Y - l2_p1.Y) - (l2_p2.Y - l2_p1.Y) * (l1_p1.X - l2_p1.X)) / denominator;
                double u_b = ((l1_p2.X - l1_p1.X) * (l1_p1.Y - l2_p1.Y) - (l1_p2.Y - l1_p1.Y) * (l1_p1.X - l2_p1.X)) / denominator;

                //Are the line segments intersecting if the end points are the same
                if (shouldIncludeEndPoints)
                {
                    //Is intersecting if u_a and u_b are between 0 and 1 or exactly 0 or 1
                    if (u_a >= 0f + epsilon && u_a <= 1f - epsilon && u_b >= 0f + epsilon && u_b <= 1f - epsilon)
                    {
                        isIntersecting = true;
                    }
                }
                else
                {
                    //Is intersecting if u_a and u_b are between 0 and 1
                    if (u_a > 0f + epsilon && u_a < 1f - epsilon && u_b > 0f + epsilon && u_b < 1f - epsilon)
                    {
                        isIntersecting = true;
                    }
                }
            }

            return isIntersecting;
        }

        // Intersections Line
        public static bool PointLineIntersectionDistTol(double distPtStart, double distPtEnd, double distStartEnd, double tol)
        {
            // solves point line interection with precalculated distances
            // returns true if Pt is inbetween start and end
            if (distPtStart + distPtEnd > distStartEnd - tol && distPtStart + distPtEnd < distStartEnd + tol)
                return true;
            else
                return false;
        }
        public static Vec3d GetLineLineIntersectionPoint(Vec3d l1_p1, Vec3d l1_p2, Vec3d l2_p1, Vec3d l2_p2)
        {
            double denominator = (l2_p2.Y - l2_p1.Y) * (l1_p2.X - l1_p1.X) - (l2_p2.X - l2_p1.X) * (l1_p2.Y - l1_p1.Y);

            double u_a = ((l2_p2.X - l2_p1.X) * (l1_p1.Y - l2_p1.Y) - (l2_p2.Y - l2_p1.Y) * (l1_p1.X - l2_p1.X)) / denominator;

            Vec3d intersectionPoint = l1_p1 + u_a * (l1_p2 - l1_p1);

            return intersectionPoint;
        }

        public static Vec3d GetRayPlaneIntersectionCoordinate(Vec3d planePos, Vec3d planeNormal, Vec3d rayStart, Vec3d rayDir)
        {
            double denominator = Vec3d.DotProduct(((-1) * planeNormal), rayDir);

            Vec3d vecBetween = planePos - rayStart;

            double t = Vec3d.DotProduct(vecBetween, ((-1) * planeNormal)) / denominator;

            Vec3d intersectionPoint = rayStart + rayDir * t;

            return intersectionPoint;
        }
        public static double GetLineLineIntersectionRatio(Vec3d l1_p1, Vec3d l1_p2, Vec3d l2_p1, Vec3d l2_p2)
        {
            Vec3d intersectionPoint = RIntersection.GetLineLineIntersectionPoint(l1_p1, l1_p2, l2_p1, l2_p2);

            double ratio = GetPointLineIntersectionRatio(intersectionPoint, l2_p1, l2_p2);
            return ratio;
        }
        public static double GetPointLineIntersectionRatio(Vec3d pt, Vec3d lineStart, Vec3d lineEnd)
        {
            double distStart = Vec3d.Distance(lineStart, pt);
            double distEnd = Vec3d.Distance(pt, lineEnd);
            double wholeDist = distStart + distEnd;
            return distStart / wholeDist;
        }
        public static Vec3d ProjectEdgeToNearestAxis(NEdge inputEdge)
        {
            // returns vector on x or y axis that is closest to the input edge

            Vec3d startVec = inputEdge.v;
            Vec3d endVec = inputEdge.nextNEdge.v;
            Vec3d dirVec = endVec - startVec;

            // test x axis
            Vec3d zeroVec = Vec3d.Zero;
            Vec3d xNorm = Vec3d.UnitY;
            Vec3d yNorm = Vec3d.UnitX;
            Vec3d zNorm = Vec3d.UnitZ;

            Vec3d vecOut = dirVec;

            // check if dirVec perpendicular to X-Axis
            if (dirVec.Y <= Constants.AxisTolerance && dirVec.Y >= -Constants.AxisTolerance)
            {
                // if yes, use X coordinate as axis coordinate
                Vec3d tempVec = new Vec3d();
                tempVec.Y = startVec.Y;
                vecOut = tempVec;
            }
            // check if dirVec perpendicular to Y-Axis
            else if (dirVec.X <= Constants.AxisTolerance && dirVec.X >= -Constants.AxisTolerance)
            {
                // if yes, use Y coordinate as axis coordinate
                Vec3d tempVec = new Vec3d();
                tempVec.X = startVec.X;
                vecOut = tempVec;
            }
            else
            {
                // intersect with X axis
                //vecOut = RIntersection.GetRayPlaneIntersectionCoordinate(zeroVec, yNorm, startVec, dirVec);
                Vec3d vecOutX = RIntersection.GetRayPlaneIntersectionCoordinate(zeroVec, yNorm, startVec, dirVec);
                Vec3d vecOutY = RIntersection.GetRayPlaneIntersectionCoordinate(zeroVec, xNorm, startVec, dirVec);

                if (vecOutX.Mag > vecOutY.Mag)
                    vecOut = vecOutY;
                else
                    vecOut = vecOutX;
            }
            return vecOut;

        }
        public static Vec3d ProjectEdgeToNearestAxis(Vec3d startVec, Vec3d endVec)
        {
            // returns vector on x or y axis that is closest to the input edge

            Vec3d dirVec = endVec - startVec;

            // test x axis
            Vec3d zeroVec = Vec3d.Zero;
            Vec3d xNorm = Vec3d.UnitY;
            Vec3d yNorm = Vec3d.UnitX;
            Vec3d zNorm = Vec3d.UnitZ;

            Vec3d vecOut = dirVec;

            // check if dirVec perpendicular to X-Axis
            if (dirVec.Y <= Constants.AxisTolerance && dirVec.Y >= -Constants.AxisTolerance)
            {
                // if yes, use X coordinate as axis coordinate
                Vec3d tempVec = new Vec3d();
                tempVec.Y = startVec.Y;
                vecOut = tempVec;
            }
            // check if dirVec perpendicular to Y-Axis
            else if (dirVec.X <= Constants.AxisTolerance && dirVec.X >= -Constants.AxisTolerance)
            {
                // if yes, use Y coordinate as axis coordinate
                Vec3d tempVec = new Vec3d();
                tempVec.X = startVec.X;
                vecOut = tempVec;
            }
            else
            {
                // intersect with X axis
                //vecOut = RIntersection.GetRayPlaneIntersectionCoordinate(zeroVec, yNorm, startVec, dirVec);

                Vec3d vecOutX = RIntersection.GetRayPlaneIntersectionCoordinate(zeroVec, yNorm, startVec, dirVec);
                Vec3d vecOutY = RIntersection.GetRayPlaneIntersectionCoordinate(zeroVec, xNorm, startVec, dirVec);

                if (vecOutX.Mag > vecOutY.Mag)
                    vecOut = vecOutY;
                else
                    vecOut = vecOutX;
            }

            return vecOut;

        }

        public static Vec3d LineClosestPoint2D(Vec3d v, Vec3d vecA, Vec3d vecB)
        {
            //returns point that is on line closest to v 
            
            Vec3d p1 = vecA;
            Vec3d p2 = vecB;

            Vec3d p3 = v;

            Vec3d p2p1 = p2 - p1;

            double x1 = p1.X;
            double x2 = p2.X;
            double x3 = p3.X;
            double y1 = p1.Y;
            double y2 = p2.Y;
            double y3 = p3.Y;

            double u = ((x3 - x1) * (x2 - x1) + (y3 - y1) * (y2 - y1)) / ((p2p1.Mag) * (p2p1.Mag));

            double x = x1 + u * (x2 - x1);
            double y = y1 + u * (y2 - y1);

            Vec3d outVec = new Vec3d(x, y, 0);

            // u zwischen 0 und 1;
            if (u < 1 && u > 0)
                return outVec;
            else if (u < 0)
                return p1;
            else
                return p2;
        }
        public static Vec3d AxisClosestPoint2D(Vec3d v, Vec3d vecA, Vec3d vecB)
        {
            Vec3d p1 = vecA;
            Vec3d p2 = vecB;

            Vec3d p3 = v;

            Vec3d p2p1 = p2 - p1;

            double x1 = p1.X;
            double x2 = p2.X;
            double x3 = p3.X;
            double y1 = p1.Y;
            double y2 = p2.Y;
            double y3 = p3.Y;

            double u = ((x3 - x1) * (x2 - x1) + (y3 - y1) * (y2 - y1)) / ((p2p1.Mag) * (p2p1.Mag));

            double x = x1 + u * (x2 - x1);
            double y = y1 + u * (y2 - y1);

            Vec3d outVec = new Vec3d(x, y, 0);

            return outVec;
        }
        public static Vec3d AxisClosestPoint2D(Vec3d v, Axis inputAxis)
        {
            Vec3d p1 = inputAxis.v;
            Vec3d p2 = inputAxis.v + inputAxis.dir;

            Vec3d p3 = v;

            Vec3d p2p1 = p2 - p1;

            double x1 = p1.X;
            double x2 = p2.X;
            double x3 = p3.X;
            double y1 = p1.Y;
            double y2 = p2.Y;
            double y3 = p3.Y;

            double u = ((x3 - x1) * (x2 - x1) + (y3 - y1) * (y2 - y1)) / ((p2p1.Mag) * (p2p1.Mag));

            double x = x1 + u * (x2 - x1);
            double y = y1 + u * (y2 - y1);

            Vec3d outVec = new Vec3d(x, y, 0);

            return outVec;
        }


        public static bool PointsCollinear(Vec3d pt, Vec3d lineStart, Vec3d lineEnd)
        {
            // does not seem to work???
            // checks if point pt is on the slope of line lineStart-lineEnd

            double tolerance = 0.00001;

            double x1 = lineEnd.X;
            double y1 = lineEnd.Y;
            double x2 = pt.X;
            double y2 = pt.Y;
            double x3 = lineStart.X;
            double y3 = lineStart.Y;

            double det = (x1 - x3) * (y2 - y3) - (x2 - x3) * (y1 - y3);

            if (det < tolerance)
                return true;
            else
                return false;
        }
        public static bool PointLineIntersection(Vec3d pt, Vec3d lineStart, Vec3d lineEnd)
        {
            double tolerance = 0.000001;
            double distStart = Vec3d.Distance(pt, lineStart);
            double distEnd = Vec3d.Distance(pt, lineEnd);
            double distLength = Vec3d.Distance(lineStart, lineEnd);

            if (distStart + distEnd > distLength - tolerance && distStart + distEnd < distLength + tolerance)
                return true;
            else
                return false;
        }


        public static bool LineLineIntersectionBool(NLine lineA, NLine lineB)
        {
            Tuple<bool, bool, bool, Vec3d, Vec3d, NLine, string> intTuple = RIntersection.intersectionQuery(lineA.start, lineA.end, lineB.start, lineB.end);

            return intTuple.Item1;
        }
        public static bool LineListFaceIntersectionBool(List<NLine> lineList, NFace faceA)
        {
            bool intersected = false;
            for (int i = 0; i < lineList.Count; i++)
            {
                bool tempInt = LineFaceIntersectionBool(lineList[i], faceA);

                if (tempInt)
                    intersected = true;
            }

            return intersected;
        }
        public static bool LineFaceIntersectionBool(NLine lineA, NFace faceA)
        {
            bool intersected = false;
            for (int i = 0; i < faceA.edgeList.Count; i++)
            {
                Tuple<bool, bool, bool, Vec3d, Vec3d, NLine, string> intTuple = RIntersection.intersectionQuery(faceA.edgeList[i].v, faceA.edgeList[i].nextNEdge.v, lineA.start, lineA.end);
                if (intTuple.Item1 == true)
                    intersected = true;
            }

            return intersected;
        }

        public static List<NLine> splitLinesWithVecs(List<NLine> lineList, List<Vec3d> vecList, double tolerance)
        {

            List<NLine> outList = new List<NLine>();
            for (int i = 0; i < lineList.Count; i++)
            {
                for (int j = 0; j < vecList.Count; j++)
                {

                    Vec3d closest = RIntersection.LineClosestPoint2D(vecList[j], lineList[i].start, lineList[i].end);
                    double distance = Vec3d.Distance(closest, vecList[j]);

                    if (distance < tolerance)
                    {
                        NLine atemp = new NLine(lineList[i].start, closest);
                        NLine btemp = new NLine(closest, lineList[i].end);
                        if (atemp.Length > 0.05 && btemp.Length > 0.05)
                        {
                            outList.Add(atemp);
                            outList.Add(btemp);
                        }
                        else
                        {
                            outList.Add(lineList[i]);
                        }

                    }
                    else
                    {
                        outList.Add(lineList[i]);
                    }
                }
            }
            outList = NLine.getUniqueLines(outList);

            return outList;
        }
        public static List<NLine> splitLinesWithLines(List<NLine> lineList, List<NLine> splitLineList, double tolerance)
        {
            // returns lines that are split with the end points of other lines (if in the tolerance)

            List<Vec3d> splitVecs = NLine.GetPoints(splitLineList);
            List<NLine> newLinesA = splitLinesWithVecs(lineList, splitVecs, tolerance);
            return newLinesA;
        }

        // NEdge functions
        public static int adjacentNEdge(NEdge x, NEdge y)
        {
            // Compares two edges and returns if they are the adjacent
            // returns 0 if not adjacent
            // returns 1 if adjacent start-end start-end or end-start end-start
            // returns 2 if adjacent start-end end-start or end-start start-end

            // set a tempEnd first via SetTempEnd()

            int adjacentInt = 0;
            double tolDist = Constants.IntersectTolerance;
            double distEndEnd = Vec3d.Distance(x.endv, y.endv);
            double distStartStart = Vec3d.Distance(x.v, y.v);

            double distStartEnd = Vec3d.Distance(x.v, y.endv);
            double distEndStart = Vec3d.Distance(x.endv, y.v);

            if (Math.Abs(distStartEnd) < tolDist ^ Math.Abs(distEndStart) < tolDist)
            {
                adjacentInt = 1;
            }
            else if (Math.Abs(distEndEnd) < tolDist ^ Math.Abs(distStartStart) < tolDist)
            {
                adjacentInt = 2;
            }

            return adjacentInt;
        }
        public static bool equalNEdge(NEdge x, NEdge y)
        {
            // Compares two edges and returns true if they are the same
            bool adjacentBool = false;
            double tolDist = Constants.IntersectTolerance;
            double distEndEnd = Vec3d.Distance(x.nextNEdge.v, y.nextNEdge.v);
            double distStartStart = Vec3d.Distance(x.v, y.v);

            double distStartEnd = Vec3d.Distance(x.v, y.nextNEdge.v);
            double distEndStart = Vec3d.Distance(x.nextNEdge.v, y.v);

            if ((Math.Abs(distEndEnd) < tolDist && Math.Abs(distStartStart) < tolDist) || (Math.Abs(distStartEnd) < tolDist && Math.Abs(distEndStart) < tolDist))
            {
                adjacentBool = true;
            }

            return adjacentBool;
        }
        public static bool adjacentNFace(NFace inputFace0, NFace inputFace1)
        {
            // Checks if other face is adjacent to this face
            // returns face2
            //bool success = false;
            //
            //   x------x                 x------x
            //   |      |                 |   0  |
            //   |   0  |                 x------x
            //   |      |  -->   TRUE                 ---> False
            //   x------x                 x------x
            //   |   1   \                |   1   \
            //   |        \               |        \
            //   x---------x              x---------x
            //

            // 1 ------------------------------------------
            // update face0 and face1 edge end temp
            NFace face0 = inputFace0.DeepCopy();
            NFace face1 = inputFace1.DeepCopy();

            if (face0.IsClockwise == true)
            {
                face0.flipRH();
            }
            if (face1.IsClockwise == true)
            {
                face1.flipRH();
            }


            List<int> debugList = new List<int>();


            face0.updateEdgeConnectivity();
            face1.updateEdgeConnectivity();



            face0.UpdateTempEnd();
            face1.UpdateTempEnd();

            // 2 ------------------------------------------
            // check if adjacent and delete duplicate edges

            bool overlapBool = false;

            for (int i = 0; i < face0.edgeList.Count; i++)
            {
                // if edge next edge start == previous edge start, delete edge
                for (int j = 0; j < face1.edgeList.Count; j++)
                {
                    bool thisExists = NEdge.IsNEdgeEqual(face0.edgeList[i], face1.edgeList[j]);
                    if (thisExists == true)
                    {
                        overlapBool = true;
                    }
                }
            }

            return overlapBool;
        }

        //NLine functions
        public static List<NLine> trimNLineWithNFace(NLine inputLine, NFace inputFace)
        {

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
        public static NLine trimShortestNLineWithBounds(NLine extendLineStart, List<NLine> extendedBoundLines)
        {
            // Gets only shortest line from start

            List<Vec3d> splitPoints = new List<Vec3d>();
            for (int m = 0; m < extendedBoundLines.Count; m++)
            {
                bool intersectionTrue = RIntersection.LineLineIntersectionBool(extendedBoundLines[m], extendLineStart);
                if (intersectionTrue)
                {
                    Vec3d intersectionVec = RIntersection.GetLineLineIntersectionPoint(extendedBoundLines[m].start, extendedBoundLines[m].end, extendLineStart.start, extendLineStart.end);
                    splitPoints.Add(intersectionVec);
                    //inputMesh.faceList[i].edgeList[j].v = intersectionVec;
                }
            }

            // return new if sucessful, return none if not.
            if (splitPoints.Count > 0)
            {
                List<Vec3d> SortedPt = splitPoints.OrderBy(o => Vec3d.Distance(extendLineStart.start, o)).ToList();
                //AC = RhConvert.Vec3dToRhPoint(SortedPt[0]);
                return new NLine(extendLineStart.start, SortedPt[0]);
            }
            else
                return extendLineStart;

        }
        public static bool areLineSegmentsCollinear(Vec3d a1, Vec3d a2, Vec3d b1, Vec3d b2)
        {

            Vec3d a = a2 - a1;
            Vec3d b = b2 - b1;

            Vec3d cross = Vec3d.CrossProduct(a, b);

            return cross.Mag < 1e-6;
        }
        public static NPolyLine straightSoupToPolyLine(List<NLine> inputLines)
        {
            // converts lines on an on an axis into clean polyline (lines can overlap)

            List<NPolyLine> comboShattered = NLine.shatterLines(inputLines);
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

            List<NLine> unique = NLine.getUniqueLines(shatteredAllClean);
            var precision = 0.001;
            List<NLine> sortedLines = unique.OrderBy(p => Math.Round(p.start.X / precision)).ThenBy(p => Math.Round(p.start.Y / precision)).ToList();
            List<NLine> conformed3 = NLine.conformLines(sortedLines);
            NPolyLine pnew = new NPolyLine(conformed3);

            return pnew;
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

        public static List<Vec3d> GetMultiLineIntersectionPoints(List<NLine> lines)
        {
            List<Vec3d> intersectionPoints = new List<Vec3d>();
            Dictionary<Vec3d, int> intersectionCounts = new Dictionary<Vec3d, int>();

            for (int i = 0; i < lines.Count - 1; i++)
            {
                for (int j = i + 1; j < lines.Count; j++)
                {
                    if (LineLineIntersectionBool(lines[i], lines[j]))
                    {
                        Vec3d intersectionPoint = RIntersection.GetLineLineIntersectionPoint(lines[i].start, lines[i].end, lines[j].start, lines[j].end);
                        if (intersectionCounts.ContainsKey(intersectionPoint))
                        {
                            intersectionCounts[intersectionPoint]++;
                        }
                        else
                        {
                            intersectionCounts.Add(intersectionPoint, 1);
                        }
                    }
                }
            }

            foreach (var pair in intersectionCounts)
            {
                if (pair.Value > 1)
                {
                    intersectionPoints.Add(pair.Key);
                }
            }

            return intersectionPoints;
        }
        public static List<bool> GetInsideLinesBool(NFace bounds, List<NLine> linesAll)
        {
            List<bool> nonIntLines = new List<bool>();
            for (int i = 0; i < linesAll.Count; i++)
            {
                bool intersectingS = RIntersection.onNFaceEdge(linesAll[i].start, bounds);
                bool intersectingE = RIntersection.onNFaceEdge(linesAll[i].end, bounds);

                if (intersectingS == false && intersectingE == false)
                    nonIntLines.Add(true);
                else
                    nonIntLines.Add(false);
            }
            return nonIntLines;
        }
        public static List<NLine> GetInsideLines(NFace bounds, List<NLine> linesAll)
        {
            List<NLine> nonIntLines = new List<NLine>();
            for (int i = 0; i < linesAll.Count; i++)
            {
                bool intersectingS = RIntersection.onNFaceEdge(linesAll[i].start, bounds);
                bool intersectingE = RIntersection.onNFaceEdge(linesAll[i].end, bounds);

                if (intersectingS == false && intersectingE == false)
                    nonIntLines.Add(linesAll[i]);
            }
            return nonIntLines;
        }
        public static List<bool> GetMultiIntersectionLinesBool(List<NLine> lines, List<Vec3d> knotVecs)
        {
            //returns bool if line intersects with more than one knot in knotvecs
            List<bool> results = new List<bool>();

            // Go through each line and check if it intersects with more than one knot
            foreach (NLine line in lines)
            {
                int count = 0;
                foreach (Vec3d knotVec in knotVecs)
                {
                    if (RIntersection.PointLineIntersection(knotVec, line.start, line.end))
                    {
                        count++;
                    }
                }

                // Add true if line intersects with more than one knot, false otherwise
                results.Add(count > 1);
            }

            return results;
        }
        public static List<NLine> GetMultiIntersectionLines(List<NLine> lines, List<Vec3d> knotVecs)
        {
            //returns line if line intersects with more than one knot in knotvecs


            // Create a dictionary to store the count of intersections for each line
            Dictionary<NLine, int> intersectionCounts = new Dictionary<NLine, int>();

            // Go through each line and count the number of intersections with knotVecs
            foreach (NLine line in lines)
            {
                int count = 0;
                foreach (Vec3d knotVec in knotVecs)
                {
                    if (RIntersection.PointLineIntersection(knotVec, line.start, line.end))
                    {
                        count++;
                    }
                }

                if (count > 1) // if line intersects with more than one knot
                {
                    intersectionCounts[line] = count;
                }
            }

            // Get all lines with more than one intersection
            List<NLine> multiIntersectionLines = intersectionCounts.Keys.ToList();

            return multiIntersectionLines;
        }

        public static List<NLine> shatterWithDistance(List<NLine> referenceLines, List<NLine> linesToMove)
        {
            // moves linesToMove line to on top of referenceLines (reference lines are being shattered with the projection of lines to move)
            // outputs linesToMove snapped to referenceLines

            double thresholdDist = 0.2;
            double sameTolerance = 0.01;

            List<NLine> outLinesShattered = new List<NLine>();
            List<NLine> outLinesDifference = new List<NLine>();
            List<NLine> outLinesIntersection = new List<NLine>();
            // loop through all crvs in a
            for (int i = 0; i < referenceLines.Count; i++)
            {
                for (int j = 0; j < linesToMove.Count; j++)
                {
                    Tuple<List<NLine>, List<NLine>, List<NLine>> boolTuple = RIntersection.lowTolBool(referenceLines[i], linesToMove[j], thresholdDist, sameTolerance);
                    outLinesShattered.AddRange(boolTuple.Item1);
                    outLinesDifference.AddRange(boolTuple.Item2);
                    outLinesIntersection.AddRange(boolTuple.Item3);
                }
            }

            //A = RhConvert.NLineListToRhLineCurveList(outLinesShattered);
            //B = RhConvert.NLineListToRhLineCurveList(outLinesDifference);
            //C = RhConvert.NLineListToRhLineCurveList(outLinesIntersection);
            return outLinesIntersection;
        }


        // Intersect Neighboring Geo With each other
        public static Tuple<NFace, NFace> intersectNFacesTuple(NFace faceA, NFace faceB, bool intersectBoth = false)
        {
            // if intersectBoth == false. intersect face0 with face1 and add points at face0
            // if intersectBoth == true. intersect face0 with face1 and add points in both faces

            //NFace face0 = faceA.DeepCopy();
            //NFace face1 = faceB.DeepCopy();

            List<Vec3d> intVecs = new List<Vec3d>();

            for (int i = 0; i < faceA.edgeList.Count; i++)
            {
                for (int j = 0; j < faceB.edgeList.Count; j++)
                {
                    faceA.edgeList[i].SetTempEnd();
                    faceB.edgeList[j].SetTempEnd();
                    List<Vec3d> tempVecs = intersectingNEdge(faceA.edgeList[i], faceB.edgeList[j]);
                    intVecs.AddRange(tempVecs);
                }
            }

            NFace tempSort = new NFace(intVecs);
            tempSort.mergeDuplicateVertex();

            if (tempSort.edgeList.Count >= 2)
            {
                for (int d = 0; d < tempSort.edgeList.Count; d++)
                {
                    faceA.addIntersectVertNFace(tempSort.edgeList[d].v);
                    if (intersectBoth == true)
                    {
                        faceB.addIntersectVertNFace(tempSort.edgeList[d].v);
                    }
                }
            }


            return new Tuple<NFace, NFace>(faceA, faceB);
        }
        public static bool intersectNFacesBool(NFace face0, NFace face1)
        {
            bool doesIntersect = false;
            List<Vec3d> intVecs = new List<Vec3d>();

            for (int i = 0; i < face0.edgeList.Count; i++)
            {
                for (int j = 0; j < face1.edgeList.Count; j++)
                {
                    face0.edgeList[i].SetTempEnd();
                    face1.edgeList[j].SetTempEnd();
                    List<Vec3d> tempVecs = intersectingNEdge(face0.edgeList[i], face1.edgeList[j]);
                    intVecs.AddRange(tempVecs);
                }
            }

            NFace tempSort = new NFace(intVecs);
            tempSort.mergeDuplicateVertex();

            if (tempSort.edgeList.Count >= 2)
                doesIntersect = true;

            return doesIntersect;
        }
        public static List<Vec3d> intersectingNEdge(NEdge x, NEdge y, bool cornerAdjacents = false)
        {
            // Compares two edges and returns if they are the adjacent
            // returns 0 if not adjacent
            // returns 1 if adjacent start-end start-end or end-start end-start
            // returns 2 if adjacent start-end end-start or end-start start-end

            // set a tempEnd first via SetTempEnd()
            x.SetTempEnd();
            y.SetTempEnd();


            List<Vec3d> intersectionVecs = new List<Vec3d>();

            int adjacentInt = 0;
            double tolDist = 0.000001;
            double distEndEnd = Vec3d.Distance(x.endv, y.endv);
            double distStartStart = Vec3d.Distance(x.v, y.v);

            double distStartEnd = Vec3d.Distance(x.v, y.endv);
            double distEndStart = Vec3d.Distance(x.endv, y.v);

            bool insideA01 = RIntersection.PointLineIntersection(x.v, y.v, y.endv);
            bool insideA10 = RIntersection.PointLineIntersection(x.endv, y.v, y.endv);

            bool insideB01 = RIntersection.PointLineIntersection(y.v, x.v, x.endv);
            bool insideB10 = RIntersection.PointLineIntersection(y.endv, x.v, x.endv);

            // END END
            if (Math.Abs(distStartEnd) < tolDist && Math.Abs(distEndStart) < tolDist)
            {
                adjacentInt = 1;
                intersectionVecs.Add(x.v);
                intersectionVecs.Add(x.endv);
            }
            // START START
            else if (Math.Abs(distEndEnd) < tolDist && Math.Abs(distStartStart) < tolDist)
            {
                adjacentInt = 2;
                intersectionVecs.Add(x.v);
                intersectionVecs.Add(x.endv);
            }
            // START inside 1 END inside 1
            else if (insideA01 == true && insideA10 == true)
            {
                intersectionVecs.Add(x.v);
                intersectionVecs.Add(x.endv);
                adjacentInt = 3;
            }

            // START inside 0 END inside 0
            else if (insideB01 == true && insideB10 == true)
            {
                intersectionVecs.Add(y.v);
                intersectionVecs.Add(y.endv);
                adjacentInt = 4;
            }
            else if (insideA01 == true && Math.Abs(distEndEnd) < tolDist)
            {
                intersectionVecs.Add(x.v);
                intersectionVecs.Add(x.endv);
            }
            else if (insideA10 == true && Math.Abs(distStartStart) < tolDist)
            {
                intersectionVecs.Add(x.endv);
                intersectionVecs.Add(y.v);
            }
            else if (insideB01 == true && insideA01 == true)
            {
                intersectionVecs.Add(y.v);
                intersectionVecs.Add(x.v);
            }
            else if (insideB10 == true && insideA10 == true)
            {
                intersectionVecs.Add(x.endv);
                intersectionVecs.Add(y.endv);
            }


            //return adjacentInt;
            return intersectionVecs;
        }

        public static List<NLine> getIntersectionLines(NFace faceA, NFace faceB)
        {
            NFace face0 = faceA.DeepCopy();

            NFace face1 = faceB.DeepCopy();

            List<NLine> divisionLines = new List<NLine>();

            // make sure they are flipped in correct direction
            if (face0.IsClockwise == false)
                face0.flipRH();
            if (face1.IsClockwise == true)
                face1.flipRH();

            face0.updateEdgeConnectivity();
            face1.updateEdgeConnectivity();

            // go through each edge
            for (int j = 0; j < face0.edgeList.Count; j++)
            {
                for (int l = 0; l < face1.edgeList.Count; l++)
                {
                    if (NEdge.IsNEdgeEqual(face0.edgeList[j], face1.edgeList[l]))
                    {
                        if (face1.edgeList[l].isIntersecting == false)
                        {
                            face0.edgeList[j].isIntersecting = true;
                            // add line
                            NLine tempLine = new NLine(face0.edgeList[j].v, face0.edgeList[j].nextNEdge.v);
                            divisionLines.Add(tempLine);
                        }
                    }
                }
            }

            return divisionLines;
        }

        public static bool quadsOverlap(NFace face0, NFace face1)
        {
            // constructs BBox for each face to ensure Quadrilateral
            // returns true if BBoxes overlap or are touching

            NFace convexHull0 = face0.ConvexHullJarvis();
            NFace BB0 = NFace.BoundingBox2dMaxEdge(convexHull0);
            BB0.updateEdgeConnectivity();

            NFace convexHull1 = face1.ConvexHullJarvis();
            NFace BB1 = NFace.BoundingBox2dMaxEdge(convexHull1);
            BB1.updateEdgeConnectivity();

            bool doOverlap = false;
            for (int i = 0; i < BB0.edgeList.Count; i++)
            {
                bool tempBool = isPointInQuad(BB1.edgeList[0].v, BB1.edgeList[1].v, BB1.edgeList[2].v, BB1.edgeList[3].v, BB0.edgeList[i].v);
                if (tempBool == true)
                {
                    doOverlap = true;
                }
            }

            for (int i = 0; i < BB1.edgeList.Count; i++)
            {
                bool tempBool = isPointInQuad(BB0.edgeList[0].v, BB0.edgeList[1].v, BB0.edgeList[2].v, BB0.edgeList[3].v, BB1.edgeList[i].v);
                if (tempBool == true)
                {
                    doOverlap = true;
                }
            }

            return doOverlap;
        }
        public static bool isPointInQuad(Vec3d p1, Vec3d p2, Vec3d p3, Vec3d p4, Vec3d p)
        {
            bool isinside = false;
            double tolerance = 0.0001;

            // triangle from each edge to point. If all 4 triangles sum up to the area of the triangle, then inside
            // otherwise outside.
            double areaTopLeftTri = Vec3d.AreaTri2d(p1, p2, p3);
            double areaBotRightTri = Vec3d.AreaTri2d(p3, p4, p2);
            double areaOfQuad = areaTopLeftTri + areaBotRightTri;

            double testA1 = Vec3d.AreaTri2d(p1, p2, p);
            double testA2 = Vec3d.AreaTri2d(p2, p3, p);
            double testA3 = Vec3d.AreaTri2d(p3, p4, p);
            double testA4 = Vec3d.AreaTri2d(p4, p1, p);

            double testArea = testA1 + testA2 + testA3 + testA4;

            if ((areaOfQuad + tolerance > testArea) && (areaOfQuad - tolerance < testArea))
                isinside = true;

            return isinside;
        }



        // Minkowski Difference intersection solver
        // slow, only use for small, implement GJK

        public static bool doNFacesIntersectMinkowski(NFace s1, NFace s2)
        {
            // checks if origin lies in mink sum face 
            // if yes return 0

            NFace minkFace = minkowskiDifference(s1, s2);

            bool isinside = insideNFace(Vec3d.Zero, minkFace);
            return isinside;
        }
        public static NFace minkowskiDifference(NFace s1, NFace s2)
        {
            List<Vec3d> minkVecs = new List<Vec3d>();

            for (int i = 0; i < s1.edgeList.Count; i++)
            {
                for (int j = 0; j < s2.edgeList.Count; j++)
                {
                    Vec3d tempDiff = s1.edgeList[i].v - s2.edgeList[j].v;
                    minkVecs.Add(tempDiff);
                }
            }

            NFace tempFace = new NFace(minkVecs);
            NFace hull = tempFace.ConvexHullJarvis();
            return hull;
        }
        public static bool insideBounds(NFace outFace, NFace bounds)
        {
            bool isinside = true;
            outFace.updateEdgeConnectivity();
            bounds.updateEdgeConnectivity();

            for (int i = 0; i < outFace.edgeList.Count; i++)
            {
                bool isinBounds = RIntersection.insideNFace(outFace.edgeList[i].v, bounds);
                if (isinBounds == false)
                    isinside = false;
            }

            return isinside;
        }

        // GJK // Intersections 
        public static Tuple<bool, bool> GJK(NFace s1, NFace s2)
        {
            // Gilbert Johnson Keerthi (GJK) algorithm
            // Returns True (1) if intersecting, returns True (2) if Adjacent

            // Checks if origin is inside(intersecting) or on the line (adjacent) of the minkowski difference

            Vec3d d = (s2.Centroid - s1.Centroid);
            d.Norm();

            List<Vec3d> simplex = new List<Vec3d>();
            simplex.Add(support(s1, s2, d));
            d = Vec3d.Zero - simplex[0];
            d.Norm();

            Vec3d A1 = support(s1, s2, d);

            Vec3d Ad = support(s1, s2, d);

            bool isIntersecting = false;
            bool isAdjacent = false;

            while (true)
            {
                Vec3d As = support(s1, s2, d);
                if (Vec3d.DotProduct(As, d) < 0)
                {
                    isIntersecting = false;
                    break;
                    //return false;
                }
                simplex.Add(As);

                Tuple<bool, bool, Vec3d, List<Vec3d>> simplexTuple = handleSimplex(simplex, d);
                bool simplexBool = simplexTuple.Item1;
                isAdjacent = simplexTuple.Item2;
                d = simplexTuple.Item3;
                simplex = simplexTuple.Item4;

                if (simplexBool == true)
                {
                    isIntersecting = true;
                    break;
                }
            }

            return new Tuple<bool, bool>(isIntersecting, isAdjacent);
        }
        
        // GJK helper functions start
        private static Tuple<bool, bool, Vec3d, List<Vec3d>> handleSimplex(List<Vec3d> simplex, Vec3d direction)
        {
            if (simplex.Count == 2)
            {
                return lineCase(simplex, direction);
            }
            return triangleCase(simplex, direction);
        }
        private static Tuple<bool, bool, Vec3d, List<Vec3d>> lineCase(List<Vec3d> simplex, Vec3d direction)
        {
            bool returnbool = false;
            bool adjacentbool = false;

            Vec3d B = simplex[0];
            Vec3d A = simplex[1];

            Vec3d AB = B - A;
            Vec3d AO = Vec3d.Zero - A;
            Vec3d ABperp = Vec3d.tripleProd(AB, AO, AB);

            if (RIntersection.PointLineIntersection(Vec3d.Zero, A, B))
            { // ab passes through origin
                returnbool = true;
                adjacentbool = true;
            }

            return new Tuple<bool, bool, Vec3d, List<Vec3d>>(returnbool, adjacentbool, ABperp, simplex);
        }
        private static Tuple<bool, bool, Vec3d, List<Vec3d>> triangleCase(List<Vec3d> simplex, Vec3d direction)
        {
            bool returnbool = true;
            bool adjacentbool = false;
            Vec3d C = simplex[0];
            Vec3d B = simplex[1];
            Vec3d A = simplex[2];

            Vec3d AB = B - A;
            Vec3d AC = C - A;
            Vec3d AO = Vec3d.Zero - A;
            Vec3d ABperp = Vec3d.tripleProd(AC, AB, AB);
            Vec3d ACperp = Vec3d.tripleProd(AB, AC, AC);

            if (Vec3d.DotProduct(ABperp, AO) > 0)
            {
                simplex.RemoveAt(0);
                direction = ABperp.GetNorm();
                returnbool = false;
            }
            else if (Vec3d.DotProduct(ACperp, AO) > 0)
            {
                simplex.RemoveAt(1);
                direction = ACperp.GetNorm();
                returnbool = false;
            }
            else if (RIntersection.PointLineIntersection(Vec3d.Zero, A, B))
            {
                // ab passes through origin
                returnbool = true;
                adjacentbool = true;
            }
            else if (RIntersection.PointLineIntersection(Vec3d.Zero, A, C))
            { // ac passes through origin
                returnbool = true;
                adjacentbool = true;
            }
            else
            {
                returnbool = true;
            }
            return new Tuple<bool, bool, Vec3d, List<Vec3d>>(returnbool, adjacentbool, direction, simplex);
        }
        private static Vec3d support(NFace s1, NFace s2, Vec3d direction)
        {
            Vec3d a = findFurthest(s1, direction);
            Vec3d reverseDirection = direction.GetReverse();
            Vec3d b = findFurthest(s2, reverseDirection);

            return a - b;

        }
        private static Vec3d findFurthest(NFace inputFace, Vec3d direction)
        {
            // support function takes direction d and returns edgepoint v furthest in direction d
            List<NEdge> sortedNEdges = inputFace.edgeList.OrderBy(o => Vec3d.DotProduct(o.v, direction)).ToList();
            sortedNEdges.Reverse();
            Vec3d outVec = sortedNEdges[0].v.DeepCopy();
            return outVec;
        }
        
        /////////////////////////////////
        public static bool insideNFace(Vec3d d, NFace poly)
        {
            // returns false if d is on polyEdge
            // first check if on line
            if (onNFaceEdge(d, poly))
            {
                return false;
            }
            else
            {

                //  Globals which should be set before calling this function:
                //
                //  int    polyCorners  =  how many corners the polygon has (no repeats)
                //  float  polyX[]      =  horizontal coordinates of corners
                //  float  polyY[]      =  vertical coordinates of corners
                //  float  x, y         =  point to be tested
                //
                //  (Globals are used in this example for purposes of speed.  Change as
                //  desired.)
                //
                //  The function will return YES if the point x,y is inside the polygon, or
                //  NO if it is not.  If the point is exactly on the edge of the polygon,
                //  then the function may return YES or NO.
                //
                //  Note that division by zero is avoided because the division is protected
                //  by the "if" clause which surrounds it.

                int polyCorners = poly.edgeList.Count;

                int i, j = polyCorners - 1;
                bool oddNodes = false;

                for (i = 0; i < polyCorners; i++)
                {
                    if (poly.edgeList[i].v.Y < d.Y && poly.edgeList[j].v.Y >= d.Y || poly.edgeList[j].v.Y < d.Y && poly.edgeList[i].v.Y >= d.Y)
                    {
                        if (poly.edgeList[i].v.X + (d.Y - poly.edgeList[i].v.Y) / (poly.edgeList[j].v.Y - poly.edgeList[i].v.Y) * (poly.edgeList[j].v.X - poly.edgeList[i].v.X) < d.X)
                        {
                            oddNodes = !oddNodes;
                        }
                    }
                    j = i;
                }

                return oddNodes;
            }
        }
        public static bool insideORonEdgeOfNFace(Vec3d d, NFace poly)
        {
            // returns false if d is on polyEdge
            // first check if on line
            if (onNFaceEdge(d, poly))
            {
                return true;
            }
            else
            {

                //  Globals which should be set before calling this function:
                //
                //  int    polyCorners  =  how many corners the polygon has (no repeats)
                //  float  polyX[]      =  horizontal coordinates of corners
                //  float  polyY[]      =  vertical coordinates of corners
                //  float  x, y         =  point to be tested
                //
                //  (Globals are used in this example for purposes of speed.  Change as
                //  desired.)
                //
                //  The function will return YES if the point x,y is inside the polygon, or
                //  NO if it is not.  If the point is exactly on the edge of the polygon,
                //  then the function may return YES or NO.
                //
                //  Note that division by zero is avoided because the division is protected
                //  by the "if" clause which surrounds it.

                int polyCorners = poly.edgeList.Count;

                int i, j = polyCorners - 1;
                bool oddNodes = false;

                for (i = 0; i < polyCorners; i++)
                {
                    if (poly.edgeList[i].v.Y < d.Y && poly.edgeList[j].v.Y >= d.Y || poly.edgeList[j].v.Y < d.Y && poly.edgeList[i].v.Y >= d.Y)
                    {
                        if (poly.edgeList[i].v.X + (d.Y - poly.edgeList[i].v.Y) / (poly.edgeList[j].v.Y - poly.edgeList[i].v.Y) * (poly.edgeList[j].v.X - poly.edgeList[i].v.X) < d.X)
                        {
                            oddNodes = !oddNodes;
                        }
                    }
                    j = i;
                }

                return oddNodes;
            }
        }
        public static bool onNFaceEdge(Vec3d p, NFace face0)
        {
            bool intersects = false;
            foreach (NEdge edge in face0.edgeList)
            {
                bool currentInt = RIntersection.PointLineIntersection(p, edge.v, edge.nextNEdge.v);
                if (currentInt == true)
                    intersects = true;
            }
            return intersects;
        }

        //////////////////////////////////
        ///
        public static List<NLine> getInsideLines(NFace bounds, List<NLine> inputLines, double minDistanceToBorder)
        {

            // Identify longest of the long axis
            List<NLine> linesOutsideBounds = new List<NLine>();
            List<NLine> linesInsideBounds = new List<NLine>();

            for (int i = 0; i < inputLines.Count; i++)
            {
                /// LINES INSide bounds with Tolerance!!!
                //Lines inside bounds with tolerance
                bool isinsideWithTol = pointInsideBoundsWithTol(bounds, inputLines[i].MidPoint, minDistanceToBorder);
                if (isinsideWithTol)
                    linesInsideBounds.Add(inputLines[i]);
                else
                    linesOutsideBounds.Add(inputLines[i]);
            }

            return linesInsideBounds;
        }

        private static bool pointInsideBoundsWithTol(NFace inputFace, Vec3d inputPt, double tolerance)
        {
            // check if insideoronEdge

            bool isInside = RIntersection.insideORonEdgeOfNFace(inputPt, inputFace);

            // check if distance to edge is greater than tolerance
            if (isInside == true)
            {
                bool tolgreater = onNFaceEdgeTol(inputPt, inputFace, tolerance);

                if (tolgreater)
                    return false;
                else
                    return true;
            }
            else
                return false;
        }

        private static bool onNFaceEdgeTol(Vec3d p, NFace face0, double tolerance)
        {
            // do not use since could be outside too
            bool intersects = false;
            foreach (NEdge edge in face0.edgeList)
            {
                NLine tempLine = new NLine(edge.v, edge.nextNEdge.v);
                bool intersectsBool = RIntersection.PointLineIntersectionTol(p, edge.v, edge.nextNEdge.v, tolerance);
                if (intersectsBool == true)
                    intersects = true;
            }
            return intersects;
        }


    }




}
