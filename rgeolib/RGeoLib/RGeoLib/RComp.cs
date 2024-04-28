using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGeoLib
{
    public class RComp
    {
        // Class for comparison functions of different shapes

        // RAYCAST difference

        public static Tuple<double, List<NLine>> raycastDifference(NFace inputA, NFace inputB, bool allowShift)
        {
            // check that both faces are in the same orientation
            NFace inputFaceA = inputA.DeepCopy();
            NFace inputFaceB = inputB.DeepCopy();

            if (inputFaceA.IsClockwise == false)
                inputFaceA.flipRH();
            if (inputFaceB.IsClockwise == false)
                inputFaceB.flipRH();

            // outputs raycast difference as total difference length and the difference lines
            double maxRange = 100.0;
            int raycastResolution = 20;
            // if allowshift is set to true, recalculate centroid for each face
            // if allowshift is set to false, set centroid both to centroidA
            Vec3d centroidA = inputFaceA.Centroid;
            Vec3d centroidB = inputFaceB.Centroid;
            if (allowShift == false)
                centroidB = centroidA;

            // do raycast polar array for faceA
            Vec3d directionA = new Vec3d(centroidA + (Vec3d.UnitX) * maxRange);
            NLine tempLineA = new NLine(centroidA, directionA);
            List<NLine> polarListA = NLine.polarNLineArray(centroidA, Vec3d.UnitZ, tempLineA, raycastResolution);
            List<NLine> polarTrimA = RIntersection.trimMultipleLinesWithFace(polarListA, inputFaceA);

            // do raycast polar array for faceB
            Vec3d directionB = new Vec3d(centroidB + (Vec3d.UnitX) * maxRange);
            NLine tempLineB = new NLine(centroidB, directionB);
            List<NLine> polarListB = NLine.polarNLineArray(centroidB, Vec3d.UnitZ, tempLineB, raycastResolution);
            List<NLine> polarTrimB = RIntersection.trimMultipleLinesWithFace(polarListB, inputFaceB);

            // compare length of lines
            List<NLine> outDiffLines = new List<NLine>();
            double combDist = 0;
            for (int i = 0; i < polarTrimA.Count; i++)
            {
                if (allowShift == false)
                {
                    NLine tempEnds = new NLine(polarTrimA[i].end, polarTrimB[i].end);
                    combDist += tempEnds.Length;
                    outDiffLines.Add(tempEnds);
                }
                else
                {
                    double lenNew = polarTrimA[i].Length - polarTrimB[i].Length;
                    Vec3d dirTemp = Vec3d.ScaleToLen(polarTrimA[i].Direction, lenNew);

                    NLine tempEnds = new NLine(polarTrimA[i].start, polarTrimA[i].start + dirTemp);
                    combDist += tempEnds.Length;
                    outDiffLines.Add(tempEnds);
                }
            }
            return new Tuple<double, List<NLine>>(combDist, outDiffLines);
        }
        public static double polarHistComparison(NFace faceA, List<NLine> lineA, NFace faceB, List<NLine> lineB, bool norm)
        {
            Tuple<List<NLine>, List<double>> polarTupleA = polarHist(faceA, lineA, norm, 50);
            Tuple<List<NLine>, List<double>> polarTupleB = polarHist(faceB, lineB, norm, 50);

            //////////----------------------------------------------------------------
            //// Comparison

            double result = 0;
            for (int i = 0; i < polarTupleA.Item2.Count; i++)
            {
                result += Math.Abs(polarTupleA.Item2[i] - polarTupleB.Item2[i]);
            }
            return result;
        }
        public static Tuple<List<NLine>, List<double>> polarHist(NFace faceA, List<NLine> lineA, bool norm, int raycastResolution)
        {
            // norm = outputs only 0 1 otherwise norm false outputs exact lengths.

            // outputs raycast difference as total difference length and the difference lines
            double maxRange = 100.0;

            NFace faceAD = faceA.DeepCopy();
            faceAD.updateEdgeConnectivity();

            NFace faceADC = faceAD.ConvexHullJarvis();
            NLine BBAL = NFace.BoundingBoxDirectionMainLine(faceADC);

            maxRange = BBAL.Length * 10;


            Vec3d centroidA = NFace.centroidInsideFace(faceAD);

            //////////----------------------------------------------------------------
            //// A

            // do raycast polar array for faceA
            Vec3d directionA = new Vec3d(centroidA + (Vec3d.UnitX) * maxRange);
            NLine tempLineA = new NLine(centroidA, directionA);
            List<NLine> polarListA = NLine.polarNLineArray(centroidA, Vec3d.UnitZ, tempLineA, raycastResolution);
            List<NLine> polarTrimA = RIntersection.trimMultipleLinesWithFace(polarListA, faceAD);


            // go through each polarTrim, get length, if end point on line A -negative, if not on lineA positive
            // normalized so either +1 or -1
            List<double> lengthA = new List<double>();
            for (int i = 0; i < polarTrimA.Count; i++)
            {
                double tempLength = 0;
                if (norm == false)
                    tempLength = polarTrimA[i].Length;

                bool touching = false;
                for (int j = 0; j < lineA.Count; j++)
                {
                    bool intersects = RIntersection.PointLineIntersection(polarTrimA[i].end, lineA[j].start, lineA[j].end);
                    if (intersects)
                        touching = true;
                }
                if (touching)
                {
                    tempLength = 1; //-tempLength;
                    if (norm == false)
                        tempLength = -tempLength;
                }

                lengthA.Add(tempLength);
            }

            return new Tuple<List<NLine>, List<double>>(polarTrimA, lengthA);

        }

        public static double faceShapeDifferenceScore(NFace faceA, NFace faceB)
        {
            // Measures difference between two shapes in relationship to the perimeter of the square equivalent

            // 0 = Best
            // 1 = Worst

            double score = (((Math.Sqrt(faceA.Area) * 4) / faceA.Perimeter) / ((Math.Sqrt(faceB.Area) * 4) / faceB.Perimeter));

            double outScore = Math.Abs(1 - score);
            return outScore;
        }

        public static double perimeterDifferenceScore(NFace faceA, double a1=1, double a2=1)
        {
            // a1 = aspect ratio side 1, a2 = aspect ratio side 2
            // Measures difference between perimeter of faceA and a rectangle with the same size and aspect ratio a1 a2, square by default

            // 0 = Best
            // 1 = Worst
            double perimeterA = faceA.Perimeter;

            double areaA = faceA.Area;

            // α = sqrt(A′/ A)
            double areaRect = a1 * a2;

            double alpha = Math.Sqrt(areaA / areaRect);
            //double areaRectComp = areaRatio * alpha;

            double a1comp = a1 * alpha;
            double a2comp = a2 * alpha;

            double perimeterR = a1comp + a1comp + a2comp + a2comp;

            double score = Math.Abs(1 - (perimeterA / perimeterR));

            return score;
        }

        public static double convexityScore(NFace inputFace)
        {
            double areaStart = inputFace.Area;
            NFace tempFace = inputFace.DeepCopy();
            tempFace = tempFace.ConvexHullJarvis();
            double areaEnd = tempFace.Area;

            double ratio = 1 - (areaStart / areaEnd);

            return ratio;
        }

        public static List<double> convexityScore(NMesh inputMesh)
        {
            List<double> convexScore = new List<double>();
            // 0 = best
            // the bigger the worse the more convex

            for (int i = 0; i < inputMesh.faceList.Count; i++)
            {
                double tempScore = convexityScore(inputMesh.faceList[i]);
                convexScore.Add(tempScore);
            }
            return convexScore;

        }
        public static double convexityScoreCombined(NMesh inputMesh)
        {
            double areaStart = inputMesh.Area;
            NMesh tempMesh = inputMesh.DeepCopyWithID();
            NFace bounds = tempMesh.ConvexHullJarvis();
            double areaEnd = bounds.Area;

            double ratio = 1 - (areaStart / areaEnd);

            return ratio;
        }
        // Compare shape with facade lines
        public static int facadeDirectionCompScore(List<NLine> targetFac, List<NLine> sourceFac)
        {
            // compares to set of facades with north, south, east, west direction

            // compare facades
            List<bool> directionListTarget = facadeDirectionFilter(targetFac);
            List<bool> directionListSource = facadeDirectionFilter(sourceFac);

            // comparison score
            int compScore = 0;
            for (int i = 0; i < directionListTarget.Count; i++)
            {
                if (directionListSource[i] == directionListTarget[i])
                    compScore++;
            }
            return compScore;
        }
        public static List<bool> facadeDirectionFilter(List<NLine> targetFac)
        {
            // Filter Facade directions
            bool northFacade = false;
            bool eastFacade = false;
            bool southFacade = false;
            bool westFacade = false;

            Vec3d northVec = new Vec3d(0, -1, 0);
            Vec3d eastVec = new Vec3d(-1, 0, 0);
            Vec3d southVec = new Vec3d(0, 1, 0);
            Vec3d westVec = new Vec3d(1, 0, 0);

            // dot product of each facade + target facade
            for (int i = 0; i < targetFac.Count; i++)
            {
                Vec3d tempTarget = Vec3d.CrossProduct(Vec3d.UnitZ, targetFac[i].Direction);
                if (Vec3d.DotProduct(northVec, tempTarget) > 0.8)
                    northFacade = true;
                if (Vec3d.DotProduct(westVec, tempTarget) > 0.8)
                    westFacade = true;
                if (Vec3d.DotProduct(southVec, tempTarget) > 0.8)
                    southFacade = true;
                if (Vec3d.DotProduct(eastVec, tempTarget) > 0.8)
                    eastFacade = true;
            }
            return new List<bool>() { northFacade, eastFacade, southFacade, westFacade };
        }

        // Orient different shapes with facade lines

        public static Tuple<double, NFace, List<NLine>, List<NLine>, double> raycastOrientationOptimizer(NFace sourceA, List<NLine> sourceCirc, List<NLine>sourceFac, NFace targetB, List<NLine> targetCirc, List<NLine> targetFac)
        {
            Tuple<List<int>, List<NFace>, List<List<NLine>>, List<List<NLine>>, List<double>> orientTuple = orientShapeWithFacadeAndCirc(sourceA, sourceCirc, sourceFac, targetB, targetCirc, targetFac);

            Tuple<int, double, NFace> raycastSimilarityTuple = RComp.raycastMostSimilar(targetB, orientTuple.Item2, false);
                
            double angleOut = orientTuple.Item5[raycastSimilarityTuple.Item1];
            NFace mostSimilarFaceOut = raycastSimilarityTuple.Item3;
            mostSimilarFaceOut.updateEdgeConnectivity();
            List<NLine> facadeLinesOut = orientTuple.Item3[raycastSimilarityTuple.Item1];
            List<NLine> circLinesOut = orientTuple.Item4[raycastSimilarityTuple.Item1];
            double scoreOut = raycastSimilarityTuple.Item2;
            return new Tuple<double, NFace, List<NLine>, List<NLine>, double>(angleOut, mostSimilarFaceOut, facadeLinesOut, circLinesOut, scoreOut);
        }
        public static Tuple<int, double, NFace> raycastMostSimilar(NFace baselineFace, List<NFace> seachSpaceFaces, bool allowshift)
        {
            List<double> raycastScore = new List<double>();
            List<int> raycastInt = new List<int>();
            for (int i = 0; i < seachSpaceFaces.Count; i++)
            {
                NFace tempFaceBaseline = baselineFace.DeepCopy();
                
                Tuple<double, List<NLine>> raycastTuple = raycastDifference(baselineFace, seachSpaceFaces[i], allowshift);
                raycastScore.Add(raycastTuple.Item1);
                raycastInt.Add(i);
            }

            List<int> raycastIntSorted = raycastInt.OrderBy(x => raycastScore[raycastInt.IndexOf(x)]).ToList();

            return new Tuple<int, double, NFace>(raycastIntSorted[0], raycastScore[raycastIntSorted[0]], seachSpaceFaces[raycastIntSorted[0]]);
        }
       

        public static Tuple<List<int>, List<NFace>, List<List<NLine>>, List<List<NLine>>, List<double>> orientShapeWithFacadeAndCirc(NFace sourceA, List<NLine> sourceCirc, List<NLine> sourceFac, NFace targetA, List<NLine> targetCirc, List<NLine> targetFac)
        {

            // iterate through all possible circulation lines

            // output the Face with the highest score
            List<NFace> outSourceFace = new List<NFace>();
            List<List<NLine>> outSourceLines = new List<List<NLine>>();
            List<List<NLine>> outSourceCircLines = new List<List<NLine>>();

            List<int> outSourceScore = new List<int>();
            //List<Vec3d> outSourceMoveVec = new List<Vec3d>();
            List<double> outSourceRotationAngle = new List<double>();
            //List<bool> outSourceFlipped = new List<bool>();

            for (int i = 0; i < targetCirc.Count; i++)
            {
                for (int j = 0; j < sourceCirc.Count; j++)
                {
                    NFace sourceFaceTemp = sourceA.DeepCopy();
                    List<NLine> sourceLinesTemp = NLine.DeepCopyList(sourceFac);
                    List<NLine> sourceCircTemp = NLine.DeepCopyList(sourceCirc);
                    //MOVE
                    Vec3d moveVecCirc = targetCirc[i].MidPoint - sourceCirc[j].MidPoint;
                    sourceFaceTemp.TranslateNFace(moveVecCirc);
                    sourceLinesTemp = NLine.TranslateNLineList(moveVecCirc, sourceLinesTemp);
                    sourceCircTemp = NLine.TranslateNLineList(moveVecCirc, sourceCircTemp);
                    //ROTATE
                    double sourceAngle = Vec3d.Angle2PI_2d(targetCirc[i].Direction, sourceCirc[j].Direction);
                    sourceFaceTemp = NFace.RotateNFace3d(Vec3d.UnitZ, targetCirc[i].MidPoint, -sourceAngle, sourceFaceTemp);
                    sourceLinesTemp = NLine.RotateNLineList3d(Vec3d.UnitZ, targetCirc[i].MidPoint, -sourceAngle, sourceLinesTemp);
                    sourceCircTemp = NLine.RotateNLineList3d(Vec3d.UnitZ, targetCirc[i].MidPoint, -sourceAngle, sourceCircTemp);

                    // compare facades
                    int tempScore = RComp.facadeDirectionCompScore(targetFac, sourceLinesTemp);

                    //Add to List
                    outSourceScore.Add(tempScore);
                    outSourceLines.Add(sourceLinesTemp);
                    outSourceFace.Add(sourceFaceTemp);
                    outSourceCircLines.Add(sourceCircTemp);

                    outSourceRotationAngle.Add(-sourceAngle);
                }
            }

            outSourceFace = outSourceFace.OrderByDescending(x => outSourceScore[outSourceFace.IndexOf(x)]).ToList();
            outSourceLines = outSourceLines.OrderByDescending(x => outSourceScore[outSourceLines.IndexOf(x)]).ToList();
            outSourceCircLines = outSourceCircLines.OrderByDescending(x => outSourceScore[outSourceCircLines.IndexOf(x)]).ToList();
            outSourceRotationAngle = outSourceRotationAngle.OrderByDescending(x => outSourceScore[outSourceRotationAngle.IndexOf(x)]).ToList();

            outSourceScore = outSourceScore.OrderByDescending(x => x).ToList();

            // reverse all lists because the lowest score is best.
            outSourceFace.Reverse();
            outSourceLines.Reverse();
            outSourceCircLines.Reverse();
            outSourceRotationAngle.Reverse();
            outSourceScore.Reverse();

            return new Tuple<List<int>, List<NFace>, List<List<NLine>>, List<List<NLine>>, List<double>>(outSourceScore, outSourceFace, outSourceLines, outSourceCircLines, outSourceRotationAngle); //, outSourceFlipped
        }


        /*
        * public static Tuple<List<int>, List<NFace>, List<List<NLine>>, List<List<NLine>>, List<double>> orientShapeOutputMaxScore(NFace sourceA, List<NLine> sourceCirc, List<NLine> sourceFac, NFace targetA, List<NLine> targetCirc, List<NLine> targetFac)
       {
           // do orient 
           Tuple<List<int>, List<NFace>, List<List<NLine>>, List<List<NLine>>, List<double>> orientTuple = orientShapeWithFacadeAndCirc(sourceA, sourceCirc, sourceFac, targetA, targetCirc, targetFac);
           List<int> outSourceScore = orientTuple.Item1;
           List<NFace> outSourceFace = orientTuple.Item2;
           List<List<NLine>> outSourceLines = orientTuple.Item3;
           List<List<NLine>> outSourceCircLines = orientTuple.Item4;
           List<double> outRotAngle = orientTuple.Item5;
           //List<bool> outFlip = orientTuple.Item5;

           // output by max int
           int maxScore = outSourceScore[0];
           List<int> outScoreInt = new List<int>();
           List<NFace> outSourceFaceMaxList = new List<NFace>();
           List<List<NLine>> outSourceNLineMaxList = new List<List<NLine>>();
           List<List<NLine>> outSourceCircNLineMaxList = new List<List<NLine>>();

           List<double> outSourceRotationAngleMaxList = new List<double>();
           //List<bool> outSourceFlippedMaxList = new List<bool>();

           for (int i = 0; i < outSourceScore.Count; i++)
           {
               if (outSourceScore[i] == maxScore)
               {
                   outScoreInt.Add(outSourceScore[i]);
                   outSourceFaceMaxList.Add(outSourceFace[i]);
                   outSourceNLineMaxList.Add(outSourceLines[i]);
                   outSourceCircNLineMaxList.Add(outSourceCircLines[i]);
                   outSourceRotationAngleMaxList.Add(outRotAngle[i]);

                   //outSourceFlippedMaxList.Add(outFlip[i]);
               }
           }

           return new Tuple<List<int>, List<NFace>, List<List<NLine>>, List<List<NLine>>, List<double>>(outScoreInt, outSourceFaceMaxList, outSourceNLineMaxList, outSourceCircNLineMaxList, outSourceRotationAngleMaxList); //, outSourceFlippedMaxList

       }
       */
    }
}