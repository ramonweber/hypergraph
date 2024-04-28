using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGeoLib
{
    public class RSplit
    {
        // EDGE 
        public static NEdge subdivideNEdgeByRatio(NEdge inputNEdge, double ratio)
        {
            double x1 = inputNEdge.v.X;
            double y1 = inputNEdge.v.Y;
            double z1 = inputNEdge.v.Z;

            double x2 = inputNEdge.nextNEdge.v.X;
            double y2 = inputNEdge.nextNEdge.v.Y;
            double z2 = inputNEdge.nextNEdge.v.Z;

            double xp = x1 + ratio * (x2 - x1);
            double yp = y1 + ratio * (y2 - y1);
            double zp = z1 + ratio * (z2 - z1);

            Vec3d insertVec = new Vec3d(xp, yp, zp);
            NEdge insertEdge = new NEdge(insertVec);

            return insertEdge;
        }
        public static NFace subdivideNFaceEdgeByRatio(NFace inputFace, double ratio)
        {
            int edgeIndex = inputFace.longestEdgeIndex();

            double lengthOfLongest = inputFace.edgeList[edgeIndex].Direction.Mag;
            int numOfDivs = Convert.ToInt32(1 / ratio);
            double tempLength = lengthOfLongest;

            for (int i = 0; i < numOfDivs; i++)
            {
                double currentLength = tempLength;

                double divReal = lengthOfLongest * ratio;

                double splitter = 1 / (currentLength / divReal);

                NEdge insertEdge = subdivideNEdgeByRatio(inputFace.edgeList[edgeIndex], splitter);
                inputFace.edgeList.Insert(edgeIndex + 1, insertEdge);
                inputFace.updateEdgeConnectivity();
                edgeIndex++;
                tempLength -= divReal;
            }

            return inputFace;
        }
        public static NFace subdivideNFaceEdgeByLength(NFace inputFace, double length, int edgeIndex)
        {
            double lengthOfLongest = inputFace.edgeList[edgeIndex].Direction.Mag;
            int numOfDivs = (int)(lengthOfLongest / length);
            double tempLength = lengthOfLongest;
            double ratio = 1 / (lengthOfLongest / length);
            for (int i = 0; i < numOfDivs; i++)
            {

                double currentLength = tempLength;

                double divReal = lengthOfLongest * ratio; 

                double splitter = 1 / (currentLength / divReal);

                NEdge insertEdge = subdivideNEdgeByRatio(inputFace.edgeList[edgeIndex], splitter);
                inputFace.edgeList.Insert(edgeIndex + 1, insertEdge);
                inputFace.updateEdgeConnectivity();
                edgeIndex++;
                tempLength -= divReal;
            }

            return inputFace;
        }


        // QUAD
        public static List<Vec3d> SplitQuad(List<Vec3d> vecs, double splitTop, double splitRatio)
        {
            Vec3d A = vecs[0];
            Vec3d B = vecs[1];
            Vec3d C = vecs[2];
            Vec3d D = vecs[3];

            double area_abc = Vec3d.AreaTri2d(A, B, C);
            double area_acd = Vec3d.AreaTri2d(A, C, D);
            double area_tot = area_abc + area_acd;


            Vec3d M = Vec3d.Between(A, B, splitTop);


            int factorR = 1;

            Vec3d conCheckR = Vec3d.CrossProduct((B - A), (C - B));
            if (conCheckR.Z > 0)
            {
                factorR = -1;
            }

            Vec3d OQo = (C + (B - M)); //(C + (B - M)) - B;
            Vec3d BQo = OQo - B;


            double lenBC = Vec3d.Distance(B, C);
            double alpha1 = Vec3d.Angle((OQo - B), (C - B)); //  BQ0.wall_angle(BC)

            Vec3d conCheckC = Vec3d.CrossProduct((C - B), (C - D));
            double beta1 = 0;
            if (conCheckC.Z > 0)
            {
                beta1 = Vec3d.Angle((C - B), (D - C)); // BC.wall_angle(CD)
            }
            else
            {
                beta1 = Vec3d.Angle((C - B), (C - D));
            }

            double gamma1 = Math.PI - beta1 - (factorR * alpha1);
            double lenb1 = lenBC * (Math.Sin(beta1) / Math.Sin(gamma1));
            Vec3d BQ = Vec3d.ScaleToLen((BQo), lenb1);
            Vec3d Q = B + BQ;
            int factorL = 1;
            Vec3d conCheckL = Vec3d.CrossProduct((B - A), (D - A));
            if (conCheckL.Z > 0)
            {
                factorL = -1;
            }

            Vec3d OPo = A + (D - M);
            Vec3d APo = OPo - A;

            double lenAD = Vec3d.Distance(A, D);
            double alpha2 = Vec3d.Angle((OPo - A), D - A);

            Vec3d conCheckD = Vec3d.CrossProduct((D - A), (D - C));
            double beta2 = 0;
            if (conCheckD.Z > 0)
            {
                beta2 = Vec3d.Angle((D - A), (D - C));
            }
            else
            {
                beta2 = Vec3d.Angle((D - A), (C - D));
            }

            double gamma2 = Math.PI - beta2 - (factorL * alpha2);
            double lenb2 = lenAD * (Math.Sin(beta2) / Math.Sin(gamma2));
            Vec3d AP = Vec3d.ScaleToLen((APo), lenb2);
            Vec3d P = A + AP;

            Vec3d N = Vec3d.Between(P, Q, splitRatio);

            List<Vec3d> resultVecs = new List<Vec3d>() { A, M, N, D, M, B, C, N };

            return resultVecs;
        }
        
        public static Tuple<bool, NFace, NFace> SplitQuadNFaceSimple(NFace face, double splitRatio)
        {
            double splitTop = splitRatio;

            Vec3d A = face.edgeList[0].v;
            Vec3d B = face.edgeList[1].v;
            Vec3d C = face.edgeList[2].v;
            Vec3d D = face.edgeList[3].v;

            double area_abc = Vec3d.AreaTri2d(A, B, C);
            double area_acd = Vec3d.AreaTri2d(A, C, D);
            double area_tot = area_abc + area_acd;


            Vec3d M = Vec3d.Between(A, B, splitTop);

            int factorR = 1;

            Vec3d conCheckR = Vec3d.CrossProduct((B - A), (C - B));
            if (conCheckR.Z > 0)
            {
                factorR = -1;
            }

            Vec3d OQo = (C + (B - M)); 
            Vec3d BQo = OQo - B;


            double lenBC = Vec3d.Distance(B, C);
            double alpha1 = Vec3d.Angle((OQo - B), (C - B)); 

            Vec3d conCheckC = Vec3d.CrossProduct((C - B), (C - D));
            double beta1 = 0;
            if (conCheckC.Z > 0)
            {
                beta1 = Vec3d.Angle((C - B), (D - C)); 
            }
            else
            {
                beta1 = Vec3d.Angle((C - B), (C - D));
            }

            double gamma1 = Math.PI - beta1 - (factorR * alpha1);
            double lenb1 = lenBC * (Math.Sin(beta1) / Math.Sin(gamma1));
            Vec3d BQ = Vec3d.ScaleToLen((BQo), lenb1);
            Vec3d Q = B + BQ;

          
            int factorL = 1;
            Vec3d conCheckL = Vec3d.CrossProduct((B - A), (D - A));
            if (conCheckL.Z > 0)
            {
                factorL = -1;
            }

            Vec3d OPo = A + (D - M);
            Vec3d APo = OPo - A;

            double lenAD = Vec3d.Distance(A, D);
            double alpha2 = Vec3d.Angle((OPo - A), D - A);

            Vec3d conCheckD = Vec3d.CrossProduct((D - A), (D - C));
            double beta2 = 0;
            if (conCheckD.Z > 0)
            {
                beta2 = Vec3d.Angle((D - A), (D - C));
            }
            else
            {
                beta2 = Vec3d.Angle((D - A), (C - D));
            }

            double gamma2 = Math.PI - beta2 - (factorL * alpha2);
            double lenb2 = lenAD * (Math.Sin(beta2) / Math.Sin(gamma2));
            Vec3d AP = Vec3d.ScaleToLen((APo), lenb2);
            Vec3d P = A + AP;

            Vec3d N = Vec3d.Between(P, Q, splitRatio);

          

            NEdge na = new NEdge(A);
            NEdge nm = new NEdge(M);
            NEdge nb = new NEdge(B);
            NEdge nc = new NEdge(C);
            NEdge nn = new NEdge(N);
            NEdge nd = new NEdge(D);

            List<NEdge> FaceLeftEdges = new List<NEdge>();
            List<NEdge> FaceRightEdges = new List<NEdge>();

            FaceLeftEdges.Add(na);
            FaceLeftEdges.Add(nm);
            FaceLeftEdges.Add(nn);
            FaceLeftEdges.Add(nd);

            FaceRightEdges.Add(nm);
            FaceRightEdges.Add(nb);
            FaceRightEdges.Add(nc);
            FaceRightEdges.Add(nn);

            NFace FaceLeft = new NFace(FaceLeftEdges);
            FaceLeft.updateEdgeConnectivity();
            NFace FaceRight = new NFace(FaceRightEdges);
            FaceRight.updateEdgeConnectivity();

            bool ZeroOne = true;
            if ((splitRatio == 0) || (splitRatio == 1))
            {
                ZeroOne = false;
            }

            return new Tuple<bool, NFace, NFace>(ZeroOne, FaceLeft, FaceRight);
        }

        public static Tuple<bool, NFace, NFace> SplitQuadNFace(NFace faceA, double splitRatio)
        {
            bool success = false;
            double tolerance = Constants.SplitQuadTolerance;
                

            string outstring = "CASE 0 - Paral";
            
            NFace faceTop_out = faceA;
            NFace faceBottom_out = faceA;

            Tuple<bool, NFace, NFace> tempMeshTuple = RSplit.SplitQuadNFaceSimple(faceA, splitRatio);

            if (splitRatio > tolerance && splitRatio < 1 - tolerance)
            {
                success = true;
                faceTop_out = tempMeshTuple.Item2;
                faceBottom_out = tempMeshTuple.Item3;
            }

            Vec3d parallelVec = Vec3d.CrossProduct(faceA.edgeList[1].Direction, faceA.edgeList[3].Direction);

            if (Math.Abs(parallelVec.Z) < 0.0001)
            {
                bool wasflipped = false;
                if (faceA.IsClockwise)
                {
                    wasflipped = true;
                    faceA.flipRH();
                    faceA.shiftEdgesByInt(2);
                }


                Vec3d cross02 = Vec3d.CrossProduct(faceA.edgeList[0].Direction, faceA.edgeList[2].Direction);

                if (cross02.Z < -tolerance)
                {
                    outstring = "CASE 1 - converge";

                 
                    splitRatio = 1 - splitRatio;

                    Vec3d planePos = faceA.edgeList[2].v;
                    Vec3d planeNormal = Vec3d.CrossProduct(faceA.edgeList[2].Direction, Vec3d.UnitZ);
                    Vec3d rayStart = faceA.edgeList[1].v;
                    Vec3d rayDir = faceA.edgeList[0].v - faceA.edgeList[1].v;

                    Vec3d top = RIntersection.GetRayPlaneIntersectionCoordinate(planePos, planeNormal, rayStart, rayDir);

                    List<Vec3d> tri = new List<Vec3d>();

                    tri.Add(top);
                    tri.Add(planePos);
                    tri.Add(rayStart);


                    NFace triFace = new NFace(tri);


                    double quadArea = faceA.Area;
                    double ratioConverted = quadArea * splitRatio;

                    double triArea = triFace.Area;
                    double diff = ratioConverted / triArea;
                    double triNewRatio = 1 - diff;



                    Tuple<bool, NFace, NFace> tempMeshTupleCase1 = RSplit.SplitTriNFace(triFace, triNewRatio);
                    success = tempMeshTupleCase1.Item1;

                    NFace tooLarge = tempMeshTupleCase1.Item2;

                    List<Vec3d> quadTop = new List<Vec3d>();

                    quadTop.Add(faceA.edgeList[0].v);
                    quadTop.Add(tooLarge.edgeList[2].v);
                    quadTop.Add(tooLarge.edgeList[1].v);
                    quadTop.Add(faceA.edgeList[3].v);

                    NFace quadTopFace = new NFace(quadTop);
                    NFace quadBottomFace = tempMeshTupleCase1.Item3;

                    // Fix Quad bottom Face
                    quadBottomFace.flipRH();
                    quadBottomFace.shiftEdgesByInt(1);

                    faceTop_out = quadTopFace;
                    faceBottom_out = quadBottomFace;


                    if (wasflipped == true)
                    {
                        faceTop_out.flipRH();
                        faceBottom_out.flipRH();
                        faceTop_out.shiftEdgesByInt(1);
                        faceBottom_out.shiftEdgesByInt(1);
                    }

                }
                else if (cross02.Z > tolerance)
                {
                    outstring = "CASE 2 - converge";

                    Vec3d planePos = faceA.edgeList[3].v; /// !!
                    Vec3d planeNormal = Vec3d.CrossProduct(faceA.edgeList[2].Direction, Vec3d.UnitZ);
                    Vec3d rayStart = faceA.edgeList[0].v;
                    Vec3d rayDir = faceA.edgeList[1].v - faceA.edgeList[0].v;

                    Vec3d top = RIntersection.GetRayPlaneIntersectionCoordinate(planePos, planeNormal, rayStart, rayDir);


                    List<Vec3d> tri = new List<Vec3d>();

                    tri.Add(top);
                    tri.Add(planePos);
                    tri.Add(rayStart);

                    NFace triFace = new NFace(tri);

                    triFace.flipRH();


                    double quadArea = faceA.Area;
                    double ratioConverted = quadArea * splitRatio;

                    double triArea = triFace.Area;
                    double diff = ratioConverted / triArea;
                    double triNewRatio = 1 - diff;


                    Tuple<bool, NFace, NFace> tempMeshTupleCase2 = RSplit.SplitTriNFace(triFace, triNewRatio);
                    success = tempMeshTupleCase2.Item1;
                    NFace tooLarge = tempMeshTupleCase2.Item2;

                    List<Vec3d> quadBottom = new List<Vec3d>();

                    quadBottom.Add(tooLarge.edgeList[1].v);
                    quadBottom.Add(faceA.edgeList[1].v);
                    quadBottom.Add(faceA.edgeList[2].v);
                    quadBottom.Add(tooLarge.edgeList[2].v);

                    NFace quadTopFace = tempMeshTupleCase2.Item3;
                    NFace quadBottomFace = new NFace(quadBottom);


                    quadTopFace.flipRH();
                    quadTopFace.shiftEdgesByInt(3);

                    faceTop_out = quadTopFace;
                    faceBottom_out = quadBottomFace;

                    if (wasflipped == true)
                    {
                        faceTop_out.flipRH();
                        faceBottom_out.flipRH();
                        faceTop_out.shiftEdgesByInt(1);
                        faceBottom_out.shiftEdgesByInt(1);
                    }


                }
            }



            return new Tuple<bool, NFace, NFace>(success, faceTop_out, faceBottom_out);
        }

        // TRI
        public static List<Vec3d> SplitTriangle(List<Vec3d> vecs, double splitRatio)
        {
            

            Vec3d A = vecs[0];
            Vec3d B = vecs[1];
            Vec3d C = vecs[2];
            double area_tot = Vec3d.AreaTri2d(A, B, C);

            double expected_area = area_tot * (splitRatio);
            double expected_area_inverse = area_tot * (1 - splitRatio);
            double scale_factor = Math.Sqrt(expected_area / area_tot);
            double scale_factor_inverse = Math.Sqrt(expected_area_inverse / area_tot);

            Vec3d temp_move = A;
            Vec3d E = B - temp_move;
            Vec3d D = C - temp_move;

            Vec3d CL = Vec3d.Mid(B, C) - Vec3d.Mid(A, B);
            Vec3d cross_p_dir = Vec3d.CrossProduct(CL, (C - B));
            if (cross_p_dir.Z > 0)
            {
                scale_factor = scale_factor_inverse;
            }


            E.Scale(scale_factor);
            D.Scale(scale_factor);

            E += temp_move;
            D += temp_move;

           
            List<Vec3d> resultVecs = new List<Vec3d>() { A, E, D, E, B, C, D };

            return resultVecs;
        }
        public static Tuple<bool, NFace, NFace> SplitTriNFace(NFace face, double splitRatio)
        {
            if (face.IsClockwise == false)
            {
                splitRatio = 1 - splitRatio;
            }

            Vec3d A = face.edgeList[0].v;
            Vec3d B = face.edgeList[1].v;
            Vec3d C = face.edgeList[2].v;
            double area_tot = Vec3d.AreaTri2d(A, B, C);

            double expected_area = area_tot * (splitRatio);
            double expected_area_inverse = area_tot * (1 - splitRatio);
            double scale_factor = Math.Sqrt(expected_area / area_tot);
            double scale_factor_inverse = Math.Sqrt(expected_area_inverse / area_tot);

            Vec3d temp_move = A;
            Vec3d E = B - temp_move;
            Vec3d D = C - temp_move;

            Vec3d CL = Vec3d.Mid(B, C) - Vec3d.Mid(A, B);
            Vec3d cross_p_dir = Vec3d.CrossProduct(CL, (C - B));
            if (cross_p_dir.Z > 0)
            {
                scale_factor = scale_factor_inverse;
            }


            E.Scale(scale_factor);
            D.Scale(scale_factor);

            E += temp_move;
            D += temp_move;

            NEdge na = new NEdge(A);
            NEdge nb = new NEdge(B);
            NEdge nc = new NEdge(C);
            NEdge nd = new NEdge(D);
            NEdge ne = new NEdge(E);

            List<NEdge> FaceLeftEdges = new List<NEdge>();
            List<NEdge> FaceRightEdges = new List<NEdge>();

            FaceLeftEdges.Add(na);
            FaceLeftEdges.Add(ne);
            FaceLeftEdges.Add(nd);

            FaceRightEdges.Add(ne);
            FaceRightEdges.Add(nb);
            FaceRightEdges.Add(nc);
            FaceRightEdges.Add(nd);

            NFace FaceLeft = new NFace(FaceLeftEdges);
            FaceLeft.updateEdgeConnectivity();
            NFace FaceRight = new NFace(FaceRightEdges);
            FaceRight.updateEdgeConnectivity();

            return new Tuple<bool, NFace, NFace>(true, FaceLeft, FaceRight);

        }

        public static Tuple<int, NFace, NFace> SplitNFaceSingle(NFace face, double splitRatio)
        {

            NFace splitFace = face;
            double areaTotGlobal = face.Area;
            List<NFace> facesOut = new List<NFace>();
            int numSplits = 0;

            double currentArea = splitFace.Area;
            double divReal = areaTotGlobal * splitRatio; // currentSplitRatio;
            double splitter = 1 / (currentArea / divReal);

            if (splitFace.edgeList.Count == 3)
            {
               
                if (splitFace.isReverse == false)
                {

                    splitter = 1 - splitter;
                    Tuple<bool, NFace, NFace> triSplit = RSplit.SplitTriNFace(splitFace, splitter);
                    if (triSplit.Item1 == true)
                    {
                        facesOut.Add(triSplit.Item3);
                        facesOut.Add(triSplit.Item2);
                        numSplits++;
                    }
                }
                else
                {

                    Tuple<bool, NFace, NFace> triSplit = RSplit.SplitTriNFace(splitFace, splitter);
                    if (triSplit.Item1 == true)
                    {
                        facesOut.Add(triSplit.Item2);
                        facesOut.Add(triSplit.Item3);
                        numSplits++;
                    }

                }
                
            }
            else 
            {
                Tuple<bool, NFace, NFace> quadSplit = RSplit.SplitQuadNFace(splitFace, splitter);
                if (quadSplit.Item1 == true)
                {
                    facesOut.Add(quadSplit.Item2);
                    facesOut.Add(quadSplit.Item3);
                    numSplits++;
                }
            }
           
            return new Tuple<int, NFace, NFace>(numSplits, facesOut[0], facesOut[1]);
        }
        public static Tuple<int, NMesh> SplitNFaceMultiple(NFace face, List<double> splitList)
        {

            NFace splitFace = face;
            double areaTotGlobal = face.Area;
            List<NFace> facesOut = new List<NFace>();
            int numSplits = 0;
            for (int i = 0; i < splitList.Count; i++)
            {
               

                double currentArea = splitFace.Area;
                double divReal = areaTotGlobal * splitList[i]; 
                double splitter = 1 / (currentArea / divReal);

                if (splitFace.edgeList.Count == 3)
                {
                    Tuple<bool, NFace, NFace> triSplit = RSplit.SplitTriNFace(splitFace, splitter);
                    if (triSplit.Item1 == true)
                    {
                        facesOut.Add(triSplit.Item2);
                        splitFace = triSplit.Item3;
                        numSplits++;
                    }
                }
                else 
                {
                    Tuple<bool, NFace, NFace> quadSplit = RSplit.SplitQuadNFace(splitFace, splitter);
                    if (quadSplit.Item1 == true)
                    {
                        facesOut.Add(quadSplit.Item2);
                        splitFace = quadSplit.Item3;
                        numSplits++;
                    }
                }
            }

            facesOut.Add(splitFace);

            NMesh meshOut = new NMesh(facesOut);
            return new Tuple<int, NMesh>(numSplits, meshOut);
        }
        public static Tuple<int, NFace, NFace> IntersectNFaces(NFace faceA, NFace faceB)
        {

            NFace face0 = faceA.DeepCopy();
            NFace face1 = faceB.DeepCopy();

            if (face0.IsClockwise == false)
            {
                face0.flipRH();
            }

            if (face1.IsClockwise == false)
            {
                face1.flipRH();
            }

            face0.updateEdgeConnectivity();
            face1.updateEdgeConnectivity();
            int numIntersects = 0;

            for (int i = 0; i < face1.edgeList.Count; i++)
            {
                bool success0 = face0.addIntersectVertNFace(face1.edgeList[i].v);
                if (success0 == true)
                { numIntersects++; }

            }
            for (int j = 0; j < face0.edgeList.Count; j++)
            {
                bool success1 = face1.addIntersectVertNFace(face0.edgeList[j].v);
                if (success1 == true)
                { numIntersects++; }
            }

            face0.mergeDuplicateVertex();
            face1.mergeDuplicateVertex();

            if (numIntersects == 0)
            {
                return new Tuple<int, NFace, NFace>(numIntersects, faceA, faceB);
            }
            else
            {
                return new Tuple<int, NFace, NFace>(numIntersects, face0, face1);
            }


        }

        public static Tuple<NMesh, bool> divideNFaceWithNLine(NFace inputFace, NLine inputLine)
        {
            NLine splittingLine = NLine.snapLineToNFace(inputLine, inputFace, 0.05);
            inputLine = splittingLine;

            bool startOnFace = RIntersection.onNFaceEdge(inputLine.start, inputFace);
            bool endOnFace = RIntersection.onNFaceEdge(inputLine.end, inputFace);

            List<NFace> tempList = new List<NFace>();
            bool splitWorked = false;

            if (startOnFace == true && endOnFace == true)
            {
                // set all to intersect false
                double intTolerance = 0.001;
                bool intStart = false;
                bool intEnd = false;

                for (int i = 0; i < inputFace.edgeList.Count; i++)
                {
                    if (Vec3d.Distance(inputFace.edgeList[i].v, inputLine.start) < intTolerance)
                    {
                        intStart = true;
                        inputFace.edgeList[i].isIntersecting = true;
                    }
                    else if (Vec3d.Distance(inputFace.edgeList[i].v, inputLine.end) < intTolerance)
                    {
                        intEnd = true;
                        inputFace.edgeList[i].isIntersecting = true;
                    }
                    else
                    {
                        inputFace.edgeList[i].isIntersecting = false;
                    }
                }

                if (intStart == false)
                    inputFace.addIntersectVertNFace(inputLine.start);
                if (intEnd == false)
                    inputFace.addIntersectVertNFace(inputLine.end);

                bool firstFace = true;
                List<NEdge> AEdges = new List<NEdge>();
                List<NEdge> BEdges = new List<NEdge>();

                for (int i = 0; i < inputFace.edgeList.Count; i++)
                {
                    NEdge tempEdge = inputFace.edgeList[i].DeepCopy();
                    NEdge tempEdge2 = inputFace.edgeList[i].DeepCopy();

                    if (inputFace.edgeList[i].isIntersecting)
                    {
                        firstFace = !firstFace;
                        AEdges.Add(tempEdge);
                        BEdges.Add(tempEdge2);
                    }
                    else if (firstFace)
                    {
                        AEdges.Add(tempEdge);
                    }
                    else if (!firstFace)
                    {
                        BEdges.Add(tempEdge);
                    }
                }

                NFace faceA = new NFace(AEdges);
                NFace faceB = new NFace(BEdges);

                faceA.updateEdgeConnectivity();
                faceB.updateEdgeConnectivity();


                tempList.Add(faceA);
                tempList.Add(faceB);
                splitWorked = true;
            }
            else
            {
                tempList.Add(inputFace);
            }
            NMesh outMesh = new NMesh(tempList);

            return new Tuple<NMesh, bool>(outMesh, splitWorked);

        }
        public static Tuple<NMesh, bool, double, double, double> divideNFaceWithNLineComplex(NFace inputFace, NLine inputLine)
        {
            bool startOnFace = RIntersection.onNFaceEdge(inputLine.start, inputFace);
            bool endOnFace = RIntersection.onNFaceEdge(inputLine.end, inputFace);

            double firstArea = inputFace.Area;
            double secondArea = 0;

            List<NFace> tempList = new List<NFace>();
            bool splitWorked = false;

            if (startOnFace == true && endOnFace == true)
            {
                // set all to intersect false
                double intTolerance = 0.001;
                bool intStart = false;
                bool intEnd = false;

                for (int i = 0; i < inputFace.edgeList.Count; i++)
                {
                    if (Vec3d.Distance(inputFace.edgeList[i].v, inputLine.start) < intTolerance)
                    {
                        intStart = true;
                        inputFace.edgeList[i].isIntersecting = true;
                    }
                    else if (Vec3d.Distance(inputFace.edgeList[i].v, inputLine.end) < intTolerance)
                    {
                        intEnd = true;
                        inputFace.edgeList[i].isIntersecting = true;
                    }
                    else
                    {
                        inputFace.edgeList[i].isIntersecting = false;
                    }
                }

                if (intStart == false)
                    inputFace.addIntersectVertNFace(inputLine.start);
                if (intEnd == false)
                    inputFace.addIntersectVertNFace(inputLine.end);

                bool firstFace = true;
                List<NEdge> AEdges = new List<NEdge>();
                List<NEdge> BEdges = new List<NEdge>();

                // CASE 1 Intersection happens in the middle of thhe face
                for (int i = 0; i < inputFace.edgeList.Count; i++)
                {
                    NEdge tempEdge = inputFace.edgeList[i].DeepCopy();
                    NEdge tempEdge2 = inputFace.edgeList[i].DeepCopy();

                    if (inputFace.edgeList[i].isIntersecting)
                    {
                        firstFace = !firstFace;
                        AEdges.Add(tempEdge);
                        BEdges.Add(tempEdge2);
                    }
                    else if (firstFace)
                    {
                        AEdges.Add(tempEdge);
                    }
                    else if (!firstFace)
                    {
                        BEdges.Add(tempEdge);
                    }
                }

                NFace faceA = new NFace(AEdges);
                NFace faceB = new NFace(BEdges);

                faceA.updateEdgeConnectivity();
                faceB.updateEdgeConnectivity();


                tempList.Add(faceA);
                tempList.Add(faceB);
                firstArea = faceA.Area;
                secondArea = faceB.Area;

                splitWorked = true;
                //A = RhConvert.NFaceToRhPolyline(faceA);
                //B = RhConvert.NFaceToRhPolyline(faceB);
            }
            else
            {
                tempList.Add(inputFace);
            }
            NMesh outMesh = new NMesh(tempList);

            double splitAngle = Vec3d.Angle2PI_2d(inputLine.Direction, Vec3d.UnitX);
            //double splitAngle = Vec3d.Angle(inputLine.Direction, Vec3d.UnitX);

            return new Tuple<NMesh, bool, double, double, double>(outMesh, splitWorked, firstArea, secondArea, splitAngle);

        }

        // Directional Splits

        public static Tuple<bool, NFace, NFace> SplitDirection(NFace face, double radAngle)
        {

            double angleThreshold = 0.00001;
            double areaTolerance = 0.00001;

            List<int> outListInt = new List<int>();
            List<Vec3d> outListVec = new List<Vec3d>();
            string outstring = "";

           


            double areaFace = face.Area;
            double searchRadius = 10000.0;
            Vec3d intersectionPoint = new Vec3d();


            for (int i = 0; i < face.edgeList.Count; i++)
            {
                Vec3d l2_p1 = new Vec3d(face.edgeList[i].v);
                Vec3d searchVec = new Vec3d(searchRadius, 0, 0);
                Vec3d axis = new Vec3d(Vec3d.UnitZ);
                Vec3d intersectionVecEnd = Vec3d.RotateAroundAxisRad(searchVec, axis, radAngle);
                Vec3d l2_p2 = intersectionVecEnd + l2_p1;

                //outstring += "i: " + i;

                for (int j = 0; j < (face.edgeList.Count); j++)
                {
                    int j2 = j + 1;
                    if (j2 >= face.edgeList.Count)
                    {
                        j2 = 0;
                    }

                    //outstring += "j: " + j;
                    //outstring += "j2: " + j2;

                    Vec3d l1_p1 = new Vec3d(face.edgeList[j].v);
                    Vec3d l1_p2 = new Vec3d(face.edgeList[j2].v);




                    bool shouldIncludeEndPoints = false; // false
                    bool intersectTrue = RIntersection.AreLinesIntersecting(l1_p1, l1_p2, l2_p1, l2_p2, shouldIncludeEndPoints);




                    if (intersectTrue == true)
                    {

                        NEdge intersectionEdge = face.edgeList[j];
                        Vec3d intersectionVec = RIntersection.GetLineLineIntersectionPoint(l1_p1, l1_p2, l2_p1, l2_p2);

                        NEdge intersectionRightEdge = new NEdge(face.edgeList[j].v);
                        NEdge intersectionLeftEdge = new NEdge(intersectionVec);
                        NEdge intersectionLineStart = new NEdge(l2_p1);

                        NEdge intersectionLineEnd = new NEdge(intersectionVec);
                        NEdge intersectionLineStart2 = new NEdge(l2_p1);



                        if (i > j)
                        {
                            // CASE A intersect start int (i) > intersect end int (j):
                            outstring += "CASE A intersect start int (i) > intersect end int (j)";
                            
                            List<NEdge> A_edgesFaceLeft = face.edgeList.GetRange(0, j2);
                            A_edgesFaceLeft.Add(intersectionLeftEdge);

                            // A_LEFT : from i to end of count list
                            List<NEdge> A_edgesFaceLeftTemp = face.edgeList.GetRange(i, face.edgeList.Count - i);
                            A_edgesFaceLeft.AddRange(A_edgesFaceLeftTemp);

                            // A_LEFT: Create face from edges
                            NFace A_faceLeft = new NFace(A_edgesFaceLeft);
                            A_faceLeft.updateEdgeConnectivity();

                            // A_RIGHT
                            // A_RIGHT create a face for everything right
                            List<NEdge> A_edgesFaceRight = face.edgeList.GetRange(j2, i - j2);
                            A_edgesFaceRight.Add(face.edgeList[i]);
                            A_edgesFaceRight.Add(intersectionLeftEdge);

                            // A_RIGHT create face from edges
                            NFace A_faceRight = new NFace(A_edgesFaceRight);
                            A_faceRight.updateEdgeConnectivity();

                            // check for self intersections, if both checks hold, return the faces.
                            bool isIntersectingARight = A_faceRight.checkSelfIntersect();
                            if (isIntersectingARight == false)
                            {
                                bool isIntersectingALeft = A_faceLeft.checkSelfIntersect();
                                if (isIntersectingALeft == false)
                                {
                                    // Check if the area of the two subdivided faces combined equal the face area
                                    // if yes, good, subdivision worked
                                    // if bigger, subdivision failed and face outside was created
                                    double areaAL = A_faceLeft.Area;
                                    double areaAR = A_faceRight.Area;
                                    double areaThreshold = 0.000001;
                                    if ((areaAL + areaAR) <= (areaFace + areaThreshold) && (areaAL + areaAR) >= (areaFace - areaThreshold))
                                    {
                                        // Return Left and Right faces
                                        A_faceLeft.mergeDuplicateVertex();
                                        A_faceLeft.checkFor180Angle();

                                        A_faceRight.mergeDuplicateVertex();
                                        A_faceRight.checkFor180Angle();

                                        return new Tuple<bool, NFace, NFace>(true, A_faceLeft, A_faceRight); ////////////////////////////
                                    }

                                }
                            }

                        }
                        else
                        {

                            // CASE B intersect end int (j) > intersect start int (i)
                            outstring += "CASE B intersect end int (j) > intersect start int (i)";
                            // i = intersect start
                            // j = intersect end
                            // j2 = intersecte end end point

                            // B_LEFT
                            // B_LEFT: create face for everything left
                            List<NEdge> B_edgesFaceLeft = new List<NEdge>();
                            B_edgesFaceLeft.AddRange(face.edgeList.GetRange(0, i));


                            B_edgesFaceLeft.Add(intersectionLineStart);
                            B_edgesFaceLeft.Add(intersectionLeftEdge);
                            if (j2 > 0)
                            {
                                List<NEdge> B_edgesFaceLeftTemp = face.edgeList.GetRange(j2, face.edgeList.Count - j2);
                                B_edgesFaceLeft.AddRange(B_edgesFaceLeftTemp);
                            }

                            NFace B_faceLeft = new NFace(B_edgesFaceLeft);
                            B_faceLeft.updateEdgeConnectivity();

                            // B_RIGHT
                            // B_RIGHT: create face for everything right
                            List<NEdge> B_edgesFaceRight = face.edgeList.GetRange(i, j - i);
                            B_edgesFaceRight.Add(face.edgeList[j]);
                            B_edgesFaceRight.Add(intersectionLeftEdge);


                            NFace B_faceRight = new NFace(B_edgesFaceRight);
                            B_faceRight.updateEdgeConnectivity();

                            // check for self intersections, if both checks hold, return the faces.
                            bool isIntersectingBRight = B_faceRight.checkSelfIntersect();
                            if (isIntersectingBRight == false)
                            {
                                bool isIntersectingBLeft = B_faceLeft.checkSelfIntersect();
                                if (isIntersectingBLeft == false)
                                {
                                    // Return Left and Right faces
                                    // Check if the area of the two subdivided faces combined equal the face area
                                    // if yes, good, subdivision worked
                                    // if bigger, subdivision failed and face outside was created
                                    double areaBL = B_faceLeft.Area;
                                    double areaBR = B_faceRight.Area;
                                    double areaThreshold = 0.000001;
                                    if ((areaBL + areaBR) < (areaFace + areaThreshold) && (areaBL + areaBR) > (areaFace - areaThreshold))
                                    {
                                        // Return Left and Right faces
                                        B_faceLeft.mergeDuplicateVertex();
                                        B_faceLeft.checkFor180Angle();
                                        B_faceRight.mergeDuplicateVertex();
                                        B_faceRight.checkFor180Angle();
                                        return new Tuple<bool, NFace, NFace>(true, B_faceLeft, B_faceRight); ////////////////////////////
                                    }


                                }
                            }
                        }
                    }
                }


            }

            for (int startInt = 0; startInt < face.edgeList.Count; startInt++)
            {

                for (int endInt = 0; endInt < face.edgeList.Count; endInt++)
                {

                    // makes sure that the intersection points are more than 2 lines apart from each other (to exclude corners)
                    if (endInt != startInt && Math.Abs(endInt - startInt) > 2 && Math.Abs(endInt - startInt) < face.edgeList.Count - 2)
                    {
                        Vec3d closingLine = new Vec3d(face.edgeList[endInt].v - face.edgeList[startInt].nextNEdge.v);

                        double relevantAngle = Vec3d.Angle2PI_2d(closingLine, Vec3d.UnitX);
                        double relevantAngle2 = Vec3d.Angle2PI_2d(closingLine, Vec3d.UnitZ);

                        double currentAngle = Vec3d.Angle2PI_2d(face.edgeList[startInt].Direction, closingLine);

                        // check if the next curve does not have the same angle
                        double nextAngle = Vec3d.Angle2PI_2d(face.edgeList[startInt].nextNEdge.Direction, closingLine);

                        outstring += "relevantAngle: ";
                        outstring += relevantAngle - radAngle;
                        //outstring += radAngle;
                        outstring += "\n";


                        // checks for angles larger than pi
                        bool flipIf = false;
                        if (Math.Abs(relevantAngle - radAngle) - Math.PI < angleThreshold && Math.Abs(relevantAngle - radAngle) - Math.PI > 0 - angleThreshold)
                        { flipIf = true; }


                        if (Math.Abs(relevantAngle - radAngle) < angleThreshold || flipIf == true)
                        {
                            bool swap360 = false;
                            if (nextAngle > Math.PI - angleThreshold && nextAngle < Math.PI + angleThreshold)
                                nextAngle = 0;
                            if (currentAngle > Math.PI - angleThreshold && currentAngle < Math.PI + angleThreshold)
                            {
                                currentAngle = 0;
                                swap360 = true;
                            }

                            //outstring += " hererer";
                            //outstring += "relevantAngle - radAngle: ";
                            //outstring += (relevantAngle - radAngle);
                            //outstring += "\n";
                            outstring += "currentAngle";
                            outstring += currentAngle;
                            outstring += "\n";
                            outstring += "nextAngle: ";
                            outstring += nextAngle;
                            outstring += "\n";
                            outstring += "radAngle: ";
                            outstring += radAngle;
                            outstring += "\n";

                            outstring += "XX";
                            outstring += Math.Abs(currentAngle - radAngle);
                            outstring += "\n";
                            //if (radAngle > Math.PI)
                            //  radAngle -= Math.PI;

                            if (Math.Abs(currentAngle - radAngle) < angleThreshold || Math.Abs(currentAngle - radAngle) - Math.PI < angleThreshold)
                            {
                                outstring += "currentAngle: ";
                                outstring += currentAngle;
                                outstring += "nextAngle: ";
                                outstring += nextAngle;
                                outstring += "relevantAngle: ";
                                outstring += relevantAngle;

                                int newStart = startInt + 1;
                                int newEnd = endInt + 1;

                                if (newStart >= face.edgeList.Count)
                                {
                                    newStart = 0;
                                }
                                if (newEnd >= face.edgeList.Count)
                                {
                                    newEnd = 0;
                                }

                                if (endInt < startInt)
                                {
                                    outstring += "Case C";

                                    // Left Face
                                    List<NEdge> leftEdges = new List<NEdge>();
                                    leftEdges.AddRange(face.edgeList.GetRange(startInt, face.edgeList.Count - startInt));
                                    leftEdges.RemoveAt(0);

                                    if (swap360)
                                        leftEdges.AddRange(face.edgeList.GetRange(0, endInt + 1));
                                    else
                                        leftEdges.AddRange(face.edgeList.GetRange(0, endInt + 1));

                                    NFace leftFace = new NFace(leftEdges);

                                    // Right Face
                                    List<NEdge> rightEdges = new List<NEdge>();
                                    // rightEdges.AddRange(face.edgeList.GetRange(endInt, startInt - endInt + 2));
                                    //rightEdges.RemoveAt(0);
                                    if (newStart < startInt)
                                    {
                                        rightEdges.AddRange(face.edgeList.GetRange(newEnd, startInt - newEnd));
                                        rightEdges.AddRange(face.edgeList.GetRange(startInt, face.edgeList.Count - startInt));

                                        if (swap360)
                                            rightEdges.AddRange(face.edgeList.GetRange(0, newStart + 0));
                                        else
                                            rightEdges.AddRange(face.edgeList.GetRange(0, newStart + 1));
                                    }
                                    else
                                    {

                                        if (swap360)
                                            rightEdges.AddRange(face.edgeList.GetRange(newEnd - 1, newStart - newEnd + 1));
                                        else
                                            rightEdges.AddRange(face.edgeList.GetRange(newEnd - 0, newStart - newEnd + 1));

                                    }



                                    NFace rightFace = new NFace(rightEdges);

                                    leftFace.mergeDuplicateVertex();
                                    leftFace.checkFor180Angle();

                                    rightFace.mergeDuplicateVertex();
                                    rightFace.checkFor180Angle();

                                    if (leftFace.Area + rightFace.Area < face.Area + areaTolerance && leftFace.Area + rightFace.Area > face.Area - areaTolerance)
                                        return new Tuple<bool, NFace, NFace>(true, leftFace, rightFace);  ////////////////////////////

                                }
                                else
                                {
                                    outstring += "Case D";
                                    // start int smaller than end int

                                    // Left Face
                                    List<NEdge> leftEdges = new List<NEdge>();
                                    if (swap360)
                                        leftEdges.AddRange(face.edgeList.GetRange(startInt + 1, endInt - startInt));
                                    else
                                        leftEdges.AddRange(face.edgeList.GetRange(startInt + 1, endInt - startInt));

                                    NFace leftFace = new NFace(leftEdges);

                                    // Right Face
                                    List<NEdge> rightEdges = new List<NEdge>();
                                    rightEdges.AddRange(face.edgeList.GetRange(endInt, face.edgeList.Count - endInt));

                                    if (swap360)
                                        rightEdges.RemoveAt(0);

                                    rightEdges.AddRange(face.edgeList.GetRange(0, startInt + 2));
                                    NFace rightFace = new NFace(rightEdges);

                                    leftFace.mergeDuplicateVertex();
                                    leftFace.checkFor180Angle();

                                    rightFace.mergeDuplicateVertex();
                                    rightFace.checkFor180Angle();
                                    if (leftFace.Area + rightFace.Area < face.Area + areaTolerance && leftFace.Area + rightFace.Area > face.Area - areaTolerance)
                                        return new Tuple<bool, NFace, NFace>(true, leftFace, rightFace); ////////////////////////////
                                }

                            }

                        }
                    }


                }
            }
            outstring += "Case E";
            face.mergeDuplicateVertex();
            face.checkFor180Angle();
            return new Tuple<bool, NFace, NFace>(false, face, face); ////////////////////////////
        }
        public static Tuple<bool, NFace, NFace, string> SplitDirection_DEBUG(NFace face, double radAngle)
        {
        

            double angleThreshold = 0.00001;



            List<int> outListInt = new List<int>();
            List<Vec3d> outListVec = new List<Vec3d>();
            string outstring = "";


            double areaFace = face.Area;
            double searchRadius = 10000.0;
            Vec3d intersectionPoint = new Vec3d();


            // Iterate through points and find first edge that intersects
            for (int i = 0; i < face.edgeList.Count; i++)
            {
                Vec3d l2_p1 = new Vec3d(face.edgeList[i].v);
                Vec3d searchVec = new Vec3d(searchRadius, 0, 0);
                Vec3d axis = new Vec3d(Vec3d.UnitZ);
                Vec3d intersectionVecEnd = Vec3d.RotateAroundAxisRad(searchVec, axis, radAngle);
                Vec3d l2_p2 = intersectionVecEnd + l2_p1;

                for (int j = 0; j < (face.edgeList.Count); j++)
                {
                    int j2 = j + 1;
                    if (j2 >= face.edgeList.Count)
                    {
                        j2 = 0;
                    }

                    //outstring += "j: " + j;
                    //outstring += "j2: " + j2;

                    Vec3d l1_p1 = new Vec3d(face.edgeList[j].v);
                    Vec3d l1_p2 = new Vec3d(face.edgeList[j2].v);




                    bool shouldIncludeEndPoints = false; // false
                    bool intersectTrue = RIntersection.AreLinesIntersecting(l1_p1, l1_p2, l2_p1, l2_p2, shouldIncludeEndPoints);




                    if (intersectTrue == true)
                    {
                        //outstring = "intersect Inside True";

                        NEdge intersectionEdge = face.edgeList[j];
                        Vec3d intersectionVec = RIntersection.GetLineLineIntersectionPoint(l1_p1, l1_p2, l2_p1, l2_p2);

                        // creates the split edges
                        NEdge intersectionRightEdge = new NEdge(face.edgeList[j].v);
                        NEdge intersectionLeftEdge = new NEdge(intersectionVec);
                        NEdge intersectionLineStart = new NEdge(l2_p1);

                        NEdge intersectionLineEnd = new NEdge(intersectionVec);
                        NEdge intersectionLineStart2 = new NEdge(l2_p1);



                        if (i > j)
                        {
                            // CASE A intersect start int (i) > intersect end int (j):
                            outstring += "CASE A intersect start int (i) > intersect end int (j)";
                            // i = intersect start
                            // j = intersect end
                            // j2 = intersecte end end point

                            // A_LEFT
                            // A_LEFTcreate face for everything left
                            //left1 : from 0 to end of intersection
                            List<NEdge> A_edgesFaceLeft = face.edgeList.GetRange(0, j2);
                            A_edgesFaceLeft.Add(intersectionLeftEdge);

                            // A_LEFT : from i to end of count list
                            List<NEdge> A_edgesFaceLeftTemp = face.edgeList.GetRange(i, face.edgeList.Count - i);
                            A_edgesFaceLeft.AddRange(A_edgesFaceLeftTemp);

                            // A_LEFT: Create face from edges
                            NFace A_faceLeft = new NFace(A_edgesFaceLeft);
                            A_faceLeft.updateEdgeConnectivity();

                            // A_RIGHT
                            // A_RIGHT create a face for everything right
                            List<NEdge> A_edgesFaceRight = face.edgeList.GetRange(j2, i - j2);
                            A_edgesFaceRight.Add(face.edgeList[i]);
                            A_edgesFaceRight.Add(intersectionLeftEdge);

                            // A_RIGHT create face from edges
                            NFace A_faceRight = new NFace(A_edgesFaceRight);
                            A_faceRight.updateEdgeConnectivity();

                            // check for self intersections, if both checks hold, return the faces.
                            bool isIntersectingARight = A_faceRight.checkSelfIntersect();
                            if (isIntersectingARight == false)
                            {
                                bool isIntersectingALeft = A_faceLeft.checkSelfIntersect();
                                if (isIntersectingALeft == false)
                                {
                                    // Check if the area of the two subdivided faces combined equal the face area
                                    // if yes, good, subdivision worked
                                    // if bigger, subdivision failed and face outside was created
                                    double areaAL = A_faceLeft.Area;
                                    double areaAR = A_faceRight.Area;
                                    double areaThreshold = 0.000001;
                                    if ((areaAL + areaAR) <= (areaFace + areaThreshold) && (areaAL + areaAR) >= (areaFace - areaThreshold))
                                    {
                                        // Return Left and Right faces
                                        A_faceLeft.mergeDuplicateVertex();
                                        A_faceLeft.checkFor180Angle();

                                        A_faceRight.mergeDuplicateVertex();
                                        A_faceRight.checkFor180Angle();

                                        return new Tuple<bool, NFace, NFace, string>(true, A_faceLeft, A_faceRight, outstring); ////////////////////////////
                                    }

                                }
                            }

                        }
                        else
                        {

                            // CASE B intersect end int (j) > intersect start int (i)
                            outstring += "CASE B intersect end int (j) > intersect start int (i)";
                            // i = intersect start
                            // j = intersect end
                            // j2 = intersecte end end point

                            // B_LEFT
                            // B_LEFT: create face for everything left
                            List<NEdge> B_edgesFaceLeft = new List<NEdge>();
                            B_edgesFaceLeft.AddRange(face.edgeList.GetRange(0, i));


                            B_edgesFaceLeft.Add(intersectionLineStart);
                            B_edgesFaceLeft.Add(intersectionLeftEdge);
                            if (j2 > 0)
                            {
                                List<NEdge> B_edgesFaceLeftTemp = face.edgeList.GetRange(j2, face.edgeList.Count - j2);
                                B_edgesFaceLeft.AddRange(B_edgesFaceLeftTemp);
                            }

                            NFace B_faceLeft = new NFace(B_edgesFaceLeft);
                            B_faceLeft.updateEdgeConnectivity();

                            // B_RIGHT
                            // B_RIGHT: create face for everything right
                            List<NEdge> B_edgesFaceRight = face.edgeList.GetRange(i, j - i);
                            B_edgesFaceRight.Add(face.edgeList[j]);
                            B_edgesFaceRight.Add(intersectionLeftEdge);


                            NFace B_faceRight = new NFace(B_edgesFaceRight);
                            B_faceRight.updateEdgeConnectivity();

                            // check for self intersections, if both checks hold, return the faces.
                            bool isIntersectingBRight = B_faceRight.checkSelfIntersect();
                            if (isIntersectingBRight == false)
                            {
                                bool isIntersectingBLeft = B_faceLeft.checkSelfIntersect();
                                if (isIntersectingBLeft == false)
                                {
                                    // Return Left and Right faces
                                    // Check if the area of the two subdivided faces combined equal the face area
                                    // if yes, good, subdivision worked
                                    // if bigger, subdivision failed and face outside was created
                                    double areaBL = B_faceLeft.Area;
                                    double areaBR = B_faceRight.Area;
                                    double areaThreshold = 0.000001;
                                    if ((areaBL + areaBR) < (areaFace + areaThreshold) && (areaBL + areaBR) > (areaFace - areaThreshold))
                                    {
                                        // Return Left and Right faces
                                        B_faceLeft.mergeDuplicateVertex();
                                        B_faceLeft.checkFor180Angle();
                                        B_faceRight.mergeDuplicateVertex();
                                        B_faceRight.checkFor180Angle();
                                        return new Tuple<bool, NFace, NFace, string>(true, B_faceLeft, B_faceRight, outstring); ////////////////////////////
                                    }


                                }
                            }
                        }
                    }
                }


            }

            // Iterate through points and find first edge that intersects
            for (int startInt = 0; startInt < face.edgeList.Count; startInt++)
            {

                for (int endInt = 0; endInt < face.edgeList.Count; endInt++)
                {

                    // makes sure that the intersection points are more than 2 lines apart from each other (to exclude corners)
                    if (endInt != startInt && Math.Abs(endInt - startInt) > 2 && Math.Abs(endInt - startInt) < face.edgeList.Count - 2)
                    {
                        Vec3d closingLine = new Vec3d(face.edgeList[endInt].v - face.edgeList[startInt].nextNEdge.v);

                        double relevantAngle = Vec3d.Angle2PI_2d(closingLine, Vec3d.UnitX);
                        double relevantAngle2 = Vec3d.Angle2PI_2d(closingLine, Vec3d.UnitZ);

                        double currentAngle = Vec3d.Angle2PI_2d(face.edgeList[startInt].Direction, closingLine);

                        // check if the next curve does not have the same angle
                        double nextAngle = Vec3d.Angle2PI_2d(face.edgeList[startInt].nextNEdge.Direction, closingLine);

                        outstring += "relevantAngle: ";
                        outstring += relevantAngle - radAngle;
                        //outstring += radAngle;
                        outstring += "\n";


                        // checks for angles larger than pi
                        bool flipIf = false;
                        if (Math.Abs(relevantAngle - radAngle) - Math.PI < angleThreshold && Math.Abs(relevantAngle - radAngle) - Math.PI > 0 - angleThreshold)
                        { flipIf = true; }


                        if (Math.Abs(relevantAngle - radAngle) < angleThreshold || flipIf == true)
                        {
                            bool swap360 = false;
                            if (nextAngle > Math.PI - angleThreshold && nextAngle < Math.PI + angleThreshold)
                                nextAngle = 0;
                            if (currentAngle > Math.PI - angleThreshold && currentAngle < Math.PI + angleThreshold)
                            {
                                currentAngle = 0;
                                swap360 = true;
                            }

                            //outstring += " hererer";
                            //outstring += "relevantAngle - radAngle: ";
                            //outstring += (relevantAngle - radAngle);
                            //outstring += "\n";
                            outstring += "currentAngle";
                            outstring += currentAngle;
                            outstring += "\n";
                            outstring += "nextAngle: ";
                            outstring += nextAngle;
                            outstring += "\n";
                            outstring += "radAngle: ";
                            outstring += radAngle;
                            outstring += "\n";

                            outstring += "XX";
                            outstring += Math.Abs(currentAngle - radAngle);
                            outstring += "\n";
                            //if (radAngle > Math.PI)
                            //  radAngle -= Math.PI;

                            if (Math.Abs(currentAngle - radAngle) < angleThreshold || Math.Abs(currentAngle - radAngle) - Math.PI < angleThreshold)
                            {
                                outstring += "currentAngle: ";
                                outstring += currentAngle;
                                outstring += "nextAngle: ";
                                outstring += nextAngle;
                                outstring += "relevantAngle: ";
                                outstring += relevantAngle;

                                int newStart = startInt + 1;
                                int newEnd = endInt + 1;

                                if (newStart >= face.edgeList.Count)
                                {
                                    newStart = 0;
                                }
                                if (newEnd >= face.edgeList.Count)
                                {
                                    newEnd = 0;
                                }

                                if (endInt < startInt)
                                {
                                    outstring += "Case C";

                                    // Left Face
                                    List<NEdge> leftEdges = new List<NEdge>();
                                    leftEdges.AddRange(face.edgeList.GetRange(startInt, face.edgeList.Count - startInt));
                                    leftEdges.RemoveAt(0);

                                    if (swap360)
                                        leftEdges.AddRange(face.edgeList.GetRange(0, endInt + 1));
                                    else
                                        leftEdges.AddRange(face.edgeList.GetRange(0, endInt + 1));

                                    NFace leftFace = new NFace(leftEdges);

                                    // Right Face
                                    List<NEdge> rightEdges = new List<NEdge>();
                                    // rightEdges.AddRange(face.edgeList.GetRange(endInt, startInt - endInt + 2));
                                    //rightEdges.RemoveAt(0);
                                    if (newStart < startInt)
                                    {
                                        rightEdges.AddRange(face.edgeList.GetRange(newEnd, startInt - newEnd));
                                        rightEdges.AddRange(face.edgeList.GetRange(startInt, face.edgeList.Count - startInt));

                                        if (swap360)
                                            rightEdges.AddRange(face.edgeList.GetRange(0, newStart + 0));
                                        else
                                            rightEdges.AddRange(face.edgeList.GetRange(0, newStart + 1));
                                    }
                                    else
                                    {

                                        if (swap360)
                                            rightEdges.AddRange(face.edgeList.GetRange(newEnd - 1, newStart - newEnd + 1));
                                        else
                                            rightEdges.AddRange(face.edgeList.GetRange(newEnd - 0, newStart - newEnd + 1));

                                    }



                                    NFace rightFace = new NFace(rightEdges);

                                    leftFace.mergeDuplicateVertex();
                                    leftFace.checkFor180Angle();

                                    rightFace.mergeDuplicateVertex();
                                    rightFace.checkFor180Angle();

                                    return new Tuple<bool, NFace, NFace, string>(true, leftFace, rightFace, outstring);  ////////////////////////////

                                }
                                else
                                {
                                    outstring += "Case D";
                                    // start int smaller than end int

                                    // Left Face
                                    List<NEdge> leftEdges = new List<NEdge>();
                                    if (swap360)
                                        leftEdges.AddRange(face.edgeList.GetRange(startInt + 1, endInt - startInt));
                                    else
                                        leftEdges.AddRange(face.edgeList.GetRange(startInt + 1, endInt - startInt));

                                    NFace leftFace = new NFace(leftEdges);

                                    // Right Face
                                    List<NEdge> rightEdges = new List<NEdge>();
                                    rightEdges.AddRange(face.edgeList.GetRange(endInt, face.edgeList.Count - endInt));

                                    if (swap360)
                                        rightEdges.RemoveAt(0);

                                    rightEdges.AddRange(face.edgeList.GetRange(0, startInt + 2));
                                    NFace rightFace = new NFace(rightEdges);

                                    leftFace.mergeDuplicateVertex();
                                    leftFace.checkFor180Angle();

                                    rightFace.mergeDuplicateVertex();
                                    rightFace.checkFor180Angle();
                                    return new Tuple<bool, NFace, NFace, string>(true, leftFace, rightFace, outstring); ////////////////////////////
                                }

                            }

                        }
                    }


                }
            }
            outstring += "Case E";
            face.mergeDuplicateVertex();
            face.checkFor180Angle();
            return new Tuple<bool, NFace, NFace, string>(false, face, face, outstring); ////////////////////////////
        }
        public static Tuple<bool, NFace, NFace> SplitDirectionSingleWithFlip(NFace face, double radAngle)
        {
            // USE this instead of .SplitDirection 
            // tries to split, if no result, flip curve and try again.

            Tuple<bool, NFace, NFace> splitTuple = RSplit.SplitDirection(face, radAngle);

            NFace outFaceLeft = splitTuple.Item2;
            NFace outFaceRight = splitTuple.Item3;
            bool outBool = splitTuple.Item1;

            if (splitTuple.Item1 == false)
            {
                face.flipRH();

                Tuple<bool, NFace, NFace> splitTuple2 = RSplit.SplitDirection(face, radAngle);
                outFaceLeft = splitTuple2.Item2;
                outFaceLeft.flipRH();
                outFaceRight = splitTuple2.Item3;
                outFaceRight.flipRH();
                outBool = splitTuple2.Item1;
            }

            return new Tuple<bool, NFace, NFace>(outBool, outFaceLeft, outFaceRight);
        }
        public static Tuple<int, NMesh, string> SplitDirectionSingleDir(NFace face, double radAngle)
        {
            
            string outstring = "";

            List<NFace> facesOut = new List<NFace>();
            List<NFace> facesToSplit = new List<NFace>();
            facesToSplit.Add(face);

            int emBreak = 0;
            int numSplits = 0;
            while ((facesToSplit.Count > 0) && (emBreak < 10000))
            {
                emBreak++;

                NFace currentFace = facesToSplit[0];
                Tuple<bool, NFace, NFace> subdivCurrentFace = RSplit.SplitDirectionSingleWithFlip(currentFace, radAngle);

                if (subdivCurrentFace.Item1 == true)
                {
                    subdivCurrentFace.Item2.checkForZeroEdgeAngle();
                    subdivCurrentFace.Item3.checkForZeroEdgeAngle();
                    facesToSplit.Add(subdivCurrentFace.Item2);
                    facesToSplit.Add(subdivCurrentFace.Item3);
                    numSplits++;
                }
                else
                {
                    if (currentFace.edgeList.Count > 2)
                        facesOut.Add(currentFace);
                }
                facesToSplit.RemoveAt(0);
            }

            outstring += numSplits;


            NMesh meshOut = new NMesh(facesOut);

            meshOut.sortFacesByAngle(radAngle);
            if (face.IsClockwise == true)
                meshOut.faceList.Reverse();

            return new Tuple<int, NMesh, string>(numSplits, meshOut, outstring);
        }
        public static Tuple<int, NMesh> SplitDirectionNMeshBoth(NFace face, double radAngle)
        {
            
            double areaTolerance = 0.0000001;

            Tuple<int, NMesh, string> SplitMeshDir0 = RSplit.SplitDirectionSingleDir(face, radAngle);
            int numSplits = 0;

            List<NFace> listFaces = new List<NFace>();
            listFaces.AddRange(SplitMeshDir0.Item2.faceList);

            numSplits += SplitMeshDir0.Item1;

            List<NFace> listFacesOpposite = new List<NFace>();

            for (int l = 0; l < listFaces.Count; l++)
            {
                double radAngleOpposite = 0;
                if (radAngle >= Math.PI)
                {
                    radAngleOpposite = radAngle + Math.PI;
                }
                else
                {
                    radAngleOpposite = radAngle - Math.PI;
                }
                Tuple<int, NMesh, string> SplitMeshDir1 = RSplit.SplitDirectionSingleDir(listFaces[l], (radAngleOpposite));
                numSplits += SplitMeshDir1.Item1;

                for (int m = 0; m < SplitMeshDir1.Item2.faceList.Count; m++)
                {

                    // cleansZeroAngle Edges
                    SplitMeshDir1.Item2.faceList[m].checkFor180Angle();

                    SplitMeshDir1.Item2.faceList[m].mergeDuplicateVertex();
                    SplitMeshDir1.Item2.faceList[m].checkForZeroEdgeAngle();
                    SplitMeshDir1.Item2.faceList[m].checkForZeroEdgeAngle();
                    SplitMeshDir1.Item2.faceList[m].flipEdges();
                    SplitMeshDir1.Item2.faceList[m].checkForZeroEdgeAngle();
                    SplitMeshDir1.Item2.faceList[m].checkForZeroEdgeAngle();
                    SplitMeshDir1.Item2.faceList[m].flipEdges();

                    if (SplitMeshDir1.Item2.faceList[m].Area > areaTolerance)
                        listFacesOpposite.Add(SplitMeshDir1.Item2.faceList[m]);
                }
            }


            NMesh meshOut = new NMesh(listFacesOpposite);

          

            return new Tuple<int, NMesh>(numSplits, meshOut);

        }
        public static Tuple<int, NMesh, NMesh> SplitNMesh(NMesh mesh, double splitRatio, double radAngle)
        {

            double areaTotalMesh = 0;
            for (int i = 0; i < mesh.faceList.Count; i++)
            {
                areaTotalMesh += mesh.faceList[i].Area;
            }

            double tolerance = 0.0000001;

           
            int numSplits = 0;
            double currentArea = 0;


            List<NFace> faces = mesh.faceList;
            List<NFace> facesLeft = new List<NFace>();
            List<NFace> facesRight = new List<NFace>();


            bool splitHappened = false;

            if (splitRatio < tolerance)
            {
                facesRight.AddRange(faces);
            }
            else if (splitRatio > 1 - tolerance)
            {
                facesLeft.AddRange(faces);
            }
            else
            {

                for (int i = 0; i < mesh.faceList.Count; i++)
                {
                    double currentFaceArea = mesh.faceList[i].Area;

                    if (currentArea + currentFaceArea >= splitRatio * areaTotalMesh)
                    {
                        if (splitHappened == true)
                        {
                            facesRight.Add(mesh.faceList[i]);
                            currentArea += currentFaceArea;
                        }
                        else
                        {
                            double areaToAchieve = splitRatio * areaTotalMesh - currentArea;
                            double splitter = 1 / (currentFaceArea / areaToAchieve);



                            NFace faceToSplit = mesh.faceList[i];
                            faceToSplit.flipRH();
                            faceToSplit.updateEdgeConnectivity();
                            //Tuple<NFace, List<double>> face2 = new Tuple<NFace, List<double>>;


                            if (faceToSplit.edgeList.Count == 4)
                            {
                                faceToSplit.shiftNQuadToAngle(radAngle);

                                if ((splitter > tolerance) && (splitter < 1 - tolerance))
                                {
                                    Tuple<int, NFace, NFace> splitTuple = RSplit.SplitNFaceSingle(faceToSplit, 1 - splitter);
                                    facesLeft.Add(splitTuple.Item3);
                                    facesRight.Add(splitTuple.Item2);
                                }
                                else if (splitter < tolerance)
                                {
                                    facesRight.Add(faceToSplit);
                                }
                                else if (splitter > 1 - tolerance)
                                {
                                    facesLeft.Add(faceToSplit);//faces.Add(faceToSplit);
                                }

                                numSplits++;
                                currentArea += currentFaceArea;
                                splitHappened = true;
                            }
                            else//if(faceToSplit.edgeList.Count == 3)
                            {
                                faceToSplit.shiftNTriToAngle(radAngle);

                                if ((splitter > tolerance) && (splitter < 1 - tolerance))
                                {
                                    Tuple<int, NFace, NFace> splitTuple = RSplit.SplitNFaceSingle(faceToSplit, splitter);
                                    facesLeft.Add(splitTuple.Item2);
                                    facesRight.Add(splitTuple.Item3);
                                }
                                else if (splitter < tolerance)
                                {
                                    facesRight.Add(faceToSplit);
                                }
                                else if (splitter > 1 - tolerance)
                                {
                                    facesLeft.Add(faceToSplit);
                                }


                                numSplits++;
                                currentArea += currentFaceArea;
                                splitHappened = true;
                            }
                        }
                    }
                    else
                    {
                        facesLeft.Add(mesh.faceList[i]);
                        currentArea += currentFaceArea;
                    }
                }

            }



            //List < NFace > facesPO = new List<NFace>();

            NMesh meshOutLeft = new NMesh(facesLeft);
            NMesh meshOutRight = new NMesh(facesRight);

            return new Tuple<int, NMesh, NMesh>(numSplits, meshOutLeft, meshOutRight);

        }
        
        // Directional Subdivisions *Seems to work** :)
        public static Tuple<NMesh, NMesh> SubdivideNFaceDirection(NFace face1, double splitRatio, double radAngle)
        {
            double areaTolerance = 0.0000001; // smallest possible face
            Tuple<int, NMesh> splitMeshTuple = RSplit.SplitDirectionNMeshBoth(face1, radAngle);

            //clean mesh from split direction both
            List<NFace> cleanFaces = new List<NFace>();
            for (int j = 0; j < splitMeshTuple.Item2.faceList.Count; j++)
            {
                NFace faceToClean = splitMeshTuple.Item2.faceList[j];
                //Tuple<NFace, List<double>> cleanTuple = cleanNFace(faceToClean);
                faceToClean.checkFor180Angle();
                faceToClean.shrinkFace();
                if (faceToClean.Area > areaTolerance)
                    cleanFaces.Add(faceToClean);
            }
            NMesh cleanMesh = new NMesh(cleanFaces);
            List<double> debugList = new List<double>();


            Tuple<int, NMesh, NMesh> tempMeshTuple = RSplit.SplitNMesh(cleanMesh, splitRatio, radAngle);

            tempMeshTuple.Item2.mergeNMeshVert();
            tempMeshTuple.Item3.mergeNMeshVert();

            tempMeshTuple.Item2.deleteFacesWithZeroArea();
            tempMeshTuple.Item3.deleteFacesWithZeroArea();

            NMesh meshLeft = RSplit.CombineNMeshStrip(tempMeshTuple.Item2);
            NMesh meshRight = RSplit.CombineNMeshStrip(tempMeshTuple.Item3);

            //NMesh meshLeft = RSplit.mergeNMesh(tempMeshTuple.Item2);
            //NMesh meshRight = RSplit.mergeNMesh(tempMeshTuple.Item3);

            return new Tuple<NMesh, NMesh>(meshLeft, meshRight);
        }
        public static Tuple<List<double>, NMesh> SubdivideNFaceMultipleDirection(NFace face, List<double> splitRatioList, List<double> splitAngleList)
        {
            

            NFace splitFace = face;
            double areaTotGlobal = face.Area;
            List<NFace> facesOut = new List<NFace>();
            int numSplits = 0;
            List<double> splitterList = new List<double>();

            for (int i = 0; i < splitRatioList.Count; i++)
            {
                // Split put left face into faces out, split right face

                double currentArea = splitFace.Area;
                double divReal = areaTotGlobal * splitRatioList[i]; // currentSplitRatio;
                double splitter = 1 / (currentArea / divReal);

                double currentAngle = splitAngleList[i];

                for (int s = 3; s < splitFace.edgeList.Count; s++)
                    splitFace.checkFor180Angle();

                //splitFace.checkFor180Angle();
                //splitFace.checkFor180Angle();
                //splitFace.checkForZeroEdgeAngle();

                Tuple<NMesh, NMesh> anlgedSplitTuple = SubdivideNFaceDirection(splitFace, splitter, currentAngle);

                NMesh angledSplitMF = anlgedSplitTuple.Item1;
                NMesh angledSplitMR = anlgedSplitTuple.Item2;

                angledSplitMF.deleteFacesWithZeroArea();
                angledSplitMR.deleteFacesWithZeroArea();

                // check all split faces for zeroEdge angles and clockwise directions
                for (int r = 0; r < anlgedSplitTuple.Item1.faceList.Count; r++)
                {
                    if (anlgedSplitTuple.Item1.faceList[r].IsClockwise == false)
                    {
                        anlgedSplitTuple.Item1.faceList[r].flipRH();
                    }
                }

                for (int l = 0; l < anlgedSplitTuple.Item2.faceList.Count; l++)
                {
                    if (anlgedSplitTuple.Item2.faceList[l].IsClockwise == false)
                    {
                        anlgedSplitTuple.Item2.faceList[l].flipRH();
                    }
                }

                NMesh rightM = anlgedSplitTuple.Item1;


                
                if (anlgedSplitTuple.Item2.faceList.Count > 0)
                {
                    NFace leftF = anlgedSplitTuple.Item2.faceList[0];
                    splitterList.Add(leftF.Area);

                    splitFace = leftF;
                }
                
                if (anlgedSplitTuple.Item2.faceList.Count > 1)
                {
                    for (int a = 1; a < anlgedSplitTuple.Item2.faceList.Count; a++)
                    {
                        rightM.faceList.Add(anlgedSplitTuple.Item2.faceList[a]);
                    }
                }
                facesOut.AddRange(rightM.faceList);

                

                numSplits++;
            }

            // Add last face
            facesOut.Add(splitFace);

            // create output mesh
            NMesh meshOut = new NMesh(facesOut);

            return new Tuple<List<double>, NMesh>(splitterList, meshOut);

        }
        public static Tuple<List<double>, NMesh> SubdivideNFaceMultipleDirectionActual(NFace faceInput, List<double> realSplitInput, List<double> angleInput)
        {
            List<double> hierarchy_Ratio = new List<double>();

            double h_Area = faceInput.Area;
            for (int i = 0; i < realSplitInput.Count; i++)
            {
                hierarchy_Ratio.Add(realSplitInput[i] / h_Area);
            }

            List<double> angleGroup_2B = new List<double>() { 0, 0 };
            return RSplit.SubdivideNFaceMultipleDirection(faceInput, hierarchy_Ratio, angleInput);

        }

        // Circular Subdivision
        public static NMesh SubdivideCircularByArea(NFace inputFace, Vec3d centroid, double area)
        {
            NFace rect = inputFace.DeepCopy();
            rect.updateEdgeConnectivity();

            int breaker = 0;

            double areaDesired = area;
            double areaDesiredTemp = area;
            double areaGlobal = rect.Area;

            List<NFace> triangleChunks = new List<NFace>();

            List<Vec3d> testVecs = new List<Vec3d>();

            int currentEdgeInt = 0;
            List<Vec3d> leftOverGlobalVecs = new List<Vec3d>();
            List<NEdge> leftOverExtra = new List<NEdge>();


            while (true)
            {
                if (breaker > 10000)
                    break;

                if (areaGlobal <= areaDesired)
                    break;


                currentEdgeInt = breaker;
                if (currentEdgeInt >= rect.edgeList.Count)
                {
                    currentEdgeInt -= rect.edgeList.Count;
                }

                NEdge choseNEdge = rect.edgeList[currentEdgeInt];


                double areaOfCurrentTriangle = Vec3d.AreaTri2d(centroid, choseNEdge.v, choseNEdge.nextNEdge.v);




                if (areaDesiredTemp < areaOfCurrentTriangle)
                {

                    // insert new vec

                    double ratio = areaDesiredTemp / areaOfCurrentTriangle;
                    double x1 = choseNEdge.v.X;
                    double y1 = choseNEdge.v.Y;
                    double z1 = choseNEdge.v.Z;

                    double x2 = choseNEdge.nextNEdge.v.X;
                    double y2 = choseNEdge.nextNEdge.v.Y;
                    double z2 = choseNEdge.nextNEdge.v.Z;

                    double xp = x1 + ratio * (x2 - x1);
                    double yp = y1 + ratio * (y2 - y1);
                    double zp = z1 + ratio * (z2 - z1);

                    Vec3d insertVec = new Vec3d(xp, yp, zp);

                    rect.addIntersectVertNFace(insertVec);

                    rect.updateEdgeConnectivity();
                    
                    // add currentTriangle to chunk list
                    List<Vec3d> chunkVecs = new List<Vec3d>();

                    Vec3d v1 = choseNEdge.v.DeepCopy();
                    Vec3d v2 = choseNEdge.nextNEdge.v.DeepCopy();
                    leftOverGlobalVecs.Add(v1);
                    leftOverGlobalVecs.Add(v2);
                    leftOverGlobalVecs.Add(centroid);
                    //breaker ++;


                    NFace chunk = new NFace(leftOverGlobalVecs);
                    triangleChunks.Add(chunk);
                    areaGlobal -= chunk.Area;

                    areaDesiredTemp = areaDesired;

                    leftOverGlobalVecs = new List<Vec3d>();

                    leftOverExtra = new List<NEdge>();

                    leftOverExtra.Add(choseNEdge.nextNEdge);
                    leftOverExtra.Add(choseNEdge.nextNEdge.nextNEdge);

                }
                else
                {
                    areaDesiredTemp -= areaOfCurrentTriangle;
                    leftOverGlobalVecs.Add(choseNEdge.v);
                    leftOverGlobalVecs.Add(choseNEdge.nextNEdge.v);


                }

                breaker++;

            }

            var insertEdges = rect.edgeList.GetRange(breaker, rect.edgeList.Count - breaker);

            NFace lastFace = triangleChunks[triangleChunks.Count - 1];
            lastFace.edgeList.InsertRange(triangleChunks[triangleChunks.Count - 1].edgeList.Count - 1, insertEdges);
            lastFace.edgeList.Insert(triangleChunks[triangleChunks.Count - 1].edgeList.Count - 1, rect.edgeList[0]);

            NMesh outMesh = new NMesh(triangleChunks);

            return outMesh;
        }
        public static NMesh SubdivideCircularByInt(NFace inputFace, Vec3d centroid, int numEqualDivs)
        {

            NFace rect = inputFace.DeepCopy();
            rect.updateEdgeConnectivity();

            int breaker = 0;

            // DIVIDE equal

            double areaGlobal = rect.Area;

            double areaDesired = areaGlobal / numEqualDivs;
            double areaDesiredTemp = areaGlobal / numEqualDivs;

            List<NFace> triangleChunks = new List<NFace>();

            List<Vec3d> testVecs = new List<Vec3d>();

            int currentEdgeInt = 0;
            List<Vec3d> leftOverGlobalVecs = new List<Vec3d>();

            while (true)
            {
                if (breaker > 10000)
                    break;

                if (areaGlobal + 0.0001 < areaDesired)
                    break;


                currentEdgeInt = breaker;
                if (currentEdgeInt >= rect.edgeList.Count)
                {
                    currentEdgeInt -= rect.edgeList.Count;
                    //currentEdgeInt = startingInt + breaker;
                }

                NEdge choseNEdge = rect.edgeList[currentEdgeInt];


                double areaOfCurrentTriangle = Vec3d.AreaTri2d(centroid, choseNEdge.v, choseNEdge.nextNEdge.v);

                // if yes, divide NEdge at point to form triangle with desired Area



                if (areaDesiredTemp < areaOfCurrentTriangle)
                {

                    // insert new vec

                    double ratio = areaDesiredTemp / areaOfCurrentTriangle;
                    double x1 = choseNEdge.v.X;
                    double y1 = choseNEdge.v.Y;
                    double z1 = choseNEdge.v.Z;

                    double x2 = choseNEdge.nextNEdge.v.X;
                    double y2 = choseNEdge.nextNEdge.v.Y;
                    double z2 = choseNEdge.nextNEdge.v.Z;

                    double xp = x1 + ratio * (x2 - x1);
                    double yp = y1 + ratio * (y2 - y1);
                    double zp = z1 + ratio * (z2 - z1);

                    Vec3d insertVec = new Vec3d(xp, yp, zp);

                    rect.addIntersectVertNFace(insertVec);


                    //NEdge tempNEdge = RSplit.subdivideNEdgeByRatio(choseNEdge, areaDesired / areaOfCurrentTriangle);
                    //rect.addIntersectVertNFace(tempNEdge.v);
                    rect.updateEdgeConnectivity();
                    // add currentTriangle to chunk list
                    List<Vec3d> chunkVecs = new List<Vec3d>();

                    Vec3d v1 = choseNEdge.v.DeepCopy();
                    Vec3d v2 = choseNEdge.nextNEdge.v.DeepCopy();
                    leftOverGlobalVecs.Add(v1);
                    leftOverGlobalVecs.Add(v2);
                    leftOverGlobalVecs.Add(centroid);
                    //breaker ++;


                    NFace chunk = new NFace(leftOverGlobalVecs);
                    triangleChunks.Add(chunk);
                    areaGlobal -= chunk.Area;

                    areaDesiredTemp = areaDesired;

                    leftOverGlobalVecs = new List<Vec3d>();



                }
                // if no, subtract A from desiredArea
                else
                {
                    areaDesiredTemp -= areaOfCurrentTriangle;
                    //areaGlobal -= areaOfCurrentTriangle;
                    leftOverGlobalVecs.Add(choseNEdge.v);
                    leftOverGlobalVecs.Add(choseNEdge.nextNEdge.v);


                }

                breaker++;

            }


            NMesh outMesh = new NMesh(triangleChunks);

            return outMesh;
        }
        public static NMesh SubdivideCircularByAreaList(NFace inputFace, Vec3d centroid, List<double> areaList)
        {
           
            NFace rect = inputFace.DeepCopy();
            rect.updateEdgeConnectivity();
            string outstring = "";


            int breaker = 0;

            double areaDesired = areaList[0];
            double areaDesiredTemp = areaList[0];
            double areaGlobal = rect.Area;

            List<NFace> triangleChunks = new List<NFace>();

            List<Vec3d> testVecs = new List<Vec3d>();

            int currentEdgeInt = 0;
            List<Vec3d> leftOverGlobalVecs = new List<Vec3d>();
            List<NEdge> leftOverExtra = new List<NEdge>();

            int currentAreaInt = 0;

            while (true)
            {
                if (breaker > 10000)
                    break;

                if (areaGlobal <= areaDesiredTemp)
                    break;




                currentEdgeInt = breaker;
                if (currentEdgeInt >= rect.edgeList.Count)
                {
                    currentEdgeInt -= rect.edgeList.Count;
                    //currentEdgeInt = startingInt + breaker;
                }

                NEdge choseNEdge = rect.edgeList[currentEdgeInt];


                double areaOfCurrentTriangle = Vec3d.AreaTri2d(centroid, choseNEdge.v, choseNEdge.nextNEdge.v);




                if (areaDesiredTemp < areaOfCurrentTriangle)
                {

                    // insert new vec

                    double ratio = areaDesiredTemp / areaOfCurrentTriangle;
                    double x1 = choseNEdge.v.X;
                    double y1 = choseNEdge.v.Y;
                    double z1 = choseNEdge.v.Z;

                    double x2 = choseNEdge.nextNEdge.v.X;
                    double y2 = choseNEdge.nextNEdge.v.Y;
                    double z2 = choseNEdge.nextNEdge.v.Z;

                    double xp = x1 + ratio * (x2 - x1);
                    double yp = y1 + ratio * (y2 - y1);
                    double zp = z1 + ratio * (z2 - z1);

                    Vec3d insertVec = new Vec3d(xp, yp, zp);

                    rect.addIntersectVertNFace(insertVec);


                    //NEdge tempNEdge = RSplit.subdivideNEdgeByRatio(choseNEdge, areaDesired / areaOfCurrentTriangle);
                    //rect.addIntersectVertNFace(tempNEdge.v);
                    rect.updateEdgeConnectivity();
                    // add currentTriangle to chunk list
                    List<Vec3d> chunkVecs = new List<Vec3d>();

                    Vec3d v1 = choseNEdge.v.DeepCopy();
                    Vec3d v2 = choseNEdge.nextNEdge.v.DeepCopy();
                    leftOverGlobalVecs.Add(v1);
                    leftOverGlobalVecs.Add(v2);
                    leftOverGlobalVecs.Add(centroid);
                    //breaker ++;


                    NFace chunk = new NFace(leftOverGlobalVecs);
                    triangleChunks.Add(chunk);
                    areaGlobal -= chunk.Area;

                    currentAreaInt++;
                    if (currentAreaInt >= areaList.Count)
                        currentAreaInt = 0;

                    areaDesiredTemp = areaList[currentAreaInt];

                    leftOverGlobalVecs = new List<Vec3d>();

                    leftOverExtra = new List<NEdge>();

                    leftOverExtra.Add(choseNEdge.nextNEdge);
                    leftOverExtra.Add(choseNEdge.nextNEdge.nextNEdge);
                    //leftOverGlobalVecs.Add(centroid);



                }
                // if no, subtract A from desiredArea
                else
                {
                    //outstring += "Here" + areaGlobal + "\n";
                    areaDesiredTemp -= areaOfCurrentTriangle;
                    //areaGlobal -= areaOfCurrentTriangle;
                    leftOverGlobalVecs.Add(choseNEdge.v);
                    leftOverGlobalVecs.Add(choseNEdge.nextNEdge.v);

                }

                breaker++;

            }

            var insertEdges = rect.edgeList.GetRange(breaker, rect.edgeList.Count - breaker);

            NFace lastFace = triangleChunks[triangleChunks.Count - 1];
            lastFace.edgeList.InsertRange(triangleChunks[triangleChunks.Count - 1].edgeList.Count - 1, insertEdges);
            lastFace.edgeList.Insert(triangleChunks[triangleChunks.Count - 1].edgeList.Count - 1, rect.edgeList[0]);

            NMesh outMesh = new NMesh(triangleChunks);


            return outMesh;
        }
        public static NMesh SubdivideCircularByRatioList(NFace inputFace, Vec3d centroid, List<double> ratioList)
        {
           


            NFace rect = inputFace.DeepCopy();
            double areaGlobal = rect.Area;
            rect.updateEdgeConnectivity();
            List<double> areaList = new List<double>();
            for (int i = 0; i < ratioList.Count; i++)
            {
                double tempRatioArea = ratioList[i] * areaGlobal;
                areaList.Add(tempRatioArea);
            }

            return SubdivideCircularByAreaList(rect, centroid, areaList);
        }

       

        // Zebra Subidvision
        public static Tuple<NMesh, NMesh> subdivideZebraWithRatio(NMesh mesh, double splitRatio)
        {
            double areaTotalMesh = 0;

            List<NFace> emptyList = new List<NFace>();
            NMesh emptyMesh = new NMesh(emptyList);
            

            // check if special cases
            if (splitRatio < 0.0001)
            {
                return new Tuple<NMesh, NMesh>(mesh, emptyMesh);
            }
            if (splitRatio > 0.9999)
            {
                return new Tuple<NMesh, NMesh>(emptyMesh, mesh);
            }




            for (int i = 0; i < mesh.faceList.Count; i++)
            {
                areaTotalMesh += mesh.faceList[i].Area;
            }


            double areaDesired = splitRatio * areaTotalMesh;
            double currentArea = 0;
            double existingArea = 0;
            int chosen = 0;

            List<NFace> facesLeft = new List<NFace>();
            List<NFace> facesRight = new List<NFace>();


            for (int i = 0; i < mesh.faceList.Count; i++)
            {
                double currentFaceArea = mesh.faceList[i].Area;
                currentArea += currentFaceArea;

                if (currentArea > areaDesired)
                {


                    // this face has to be split

                    double areaToAchieve = splitRatio * areaTotalMesh - existingArea;
                    double splitter = 1 / (currentFaceArea / areaToAchieve);
                    splitter = 1 - splitter;
                    NFace splitFace = mesh.faceList[i];

                    NFace splitTemp = splitFace.DeepCopy();

                    splitTemp.checkFor180Angle();
                    splitTemp.mergeVertexWithTol(0.05);


                    NLine direction = splitFace.lineBottom;
                    NLine direction2 = splitFace.lineTop;
                    NLine inbetweenLine = NLine.inbetweenLine(direction2, direction, splitter);

                    Tuple<double, bool> angleTuple = NLine.getSmallestAngle(inbetweenLine, Vec3d.UnitX);
                    double inbetweenAngle = angleTuple.Item1;
                    bool reversed = angleTuple.Item2;

                    splitTemp.checkFor180Angle();
                    splitTemp.updateEdgeConnectivity();


                    List<Vec3d> vecList = new List<Vec3d>();
                    for (int d = 0; d < splitTemp.edgeList.Count; d++)
                    {
                        vecList.Add(splitTemp.edgeList[d].v);
                    }

                    //splitFace.checkFor180Angle();
                    if (reversed == false)
                    {
                        NFace tempFace = NFace.checkInternalDuplicateEdge(splitTemp);
                        tempFace.updateEdgeConnectivity();
                        tempFace.checkFor180Angle();
                        tempFace.checkFor180Angle();

                        /////////////////////////////////////////////////////////////
                        int decimalPlaces = 3;
                        double angleNewRoundDown = Math.Floor(inbetweenAngle * Math.Pow(10, decimalPlaces)) / Math.Pow(10, decimalPlaces);
                        Tuple<int, NMesh> splitMeshTuple = RSplit.SplitDirectionNMeshBoth(tempFace, angleNewRoundDown);

                        //clean mesh from split direction both
                        List<NFace> cleanFaces = new List<NFace>();
                        for (int j = 0; j < splitMeshTuple.Item2.faceList.Count; j++)
                        {
                            NFace faceToClean = splitMeshTuple.Item2.faceList[j];
                            faceToClean.checkFor180Angle();
                            cleanFaces.Add(faceToClean);
                        }
                        NMesh cleanMesh = new NMesh(cleanFaces);
                        List<double> debugList = new List<double>();

                        Tuple<int, NMesh, NMesh> tempMeshTuple = RSplit.SplitNMesh(cleanMesh, splitter, inbetweenAngle);


                        NMesh meshCombinedLeft = RSplit.CombineNMeshStrip(tempMeshTuple.Item2);
                        NMesh meshCombinedRight = RSplit.CombineNMeshStrip(tempMeshTuple.Item3);



                        meshCombinedLeft = RSplit.mergeNMesh(meshCombinedLeft);
                        meshCombinedRight = RSplit.mergeNMesh(meshCombinedRight);

                        ////////////////////////////////////////////////////////////


                        facesLeft.AddRange(meshCombinedRight.faceList);
                        facesRight.AddRange(meshCombinedLeft.faceList);

                    }
                    else
                    {
                        NFace tempFace = NFace.checkInternalDuplicateEdge(splitTemp);

                        /////////////////////////////////////////////////////////////
                        int decimalPlaces = 5;
                        double angleNewRoundDown = Math.Floor(inbetweenAngle * Math.Pow(10, decimalPlaces)) / Math.Pow(10, decimalPlaces);
                        Tuple<int, NMesh> splitMeshTuple = RSplit.SplitDirectionNMeshBoth(tempFace, angleNewRoundDown);

                        //clean mesh from split direction both
                        List<NFace> cleanFaces = new List<NFace>();
                        for (int j = 0; j < splitMeshTuple.Item2.faceList.Count; j++)
                        {
                            NFace faceToClean = splitMeshTuple.Item2.faceList[j];
                            faceToClean.checkFor180Angle();
                            cleanFaces.Add(faceToClean);
                        }
                        NMesh cleanMesh = new NMesh(cleanFaces);
                        List<double> debugList = new List<double>();


                        Tuple<int, NMesh, NMesh> tempMeshTuple = RSplit.SplitNMesh(cleanMesh, 1 - splitter, inbetweenAngle);


                        NMesh meshCombinedLeft = RSplit.CombineNMeshStrip(tempMeshTuple.Item2);
                        NMesh meshCombinedRight = RSplit.CombineNMeshStrip(tempMeshTuple.Item3);



                        meshCombinedLeft = RSplit.mergeNMesh(meshCombinedLeft);
                        meshCombinedRight = RSplit.mergeNMesh(meshCombinedRight);

                        ////////////////////////////////////////////////////////////

                        facesLeft.AddRange(meshCombinedLeft.faceList);
                        facesRight.AddRange(meshCombinedRight.faceList);
                    }
                    chosen = i;
                    break;
                }
                else
                {
                    facesLeft.Add(mesh.faceList[i]);
                    existingArea = currentArea;
                }
            }


            for (int i = chosen + 1; i < mesh.faceList.Count; i++)
            {
                facesRight.Add(mesh.faceList[i]);
            }


            //-----------------------------------------------------------

            NMesh meshRight = new NMesh(facesRight);
            NMesh meshLeft = new NMesh(facesLeft);

            return new Tuple<NMesh, NMesh>(meshRight, meshLeft);
        }
        public static NMesh zebraNMeshFromRails(NPolyLine polyA, NPolyLine polyB, double divInt)
        {
            // Inputs, rail 1 polyA, rail 2 polyB, number of divisions
            // returns zebra mesh

            List<double> splitRatioList = new List<double>();
            double singleDiv = 1 / divInt;
            for (int i = 0; i < divInt; i++)
            {
                splitRatioList.Add(singleDiv);
            }


            List<NPolyLine> splitTupleA = NPolyLine.SplitPolyLineWithList(polyA, splitRatioList);
            List<NPolyLine> splitTupleB = NPolyLine.SplitPolyLineWithList(polyB, splitRatioList);

            List<NFace> tempFaceList = new List<NFace>();

            List<NLine> tempLines = new List<NLine>();

            //List<NLine> startLineList = new List<NLine>();

            for (int i = 0; i < splitTupleA.Count; i++)
            {
                List<Vec3d> tempVecs = new List<Vec3d>();

                List<Vec3d> startList = splitTupleA[i].VecsAll;
                tempVecs.AddRange(startList);

                List<Vec3d> oppositeList = splitTupleB[i].VecsAll;
                oppositeList.Reverse();

                tempVecs.AddRange(oppositeList);

                NFace tempFace = new NFace(tempVecs);

                // orientation = average vec between both A and B start and end
                NLine lineBottom = new NLine(splitTupleB[i].VecsAll[0], splitTupleA[i].VecsAll[0]);

                NLine lineTop = new NLine(splitTupleA[i].VecsAll[splitTupleA[i].VecsAll.Count - 1], splitTupleB[i].VecsAll[splitTupleB[i].VecsAll.Count - 1]);
                //NLine inbetweenLine = NLine.inbetweenLine(lineBottom, lineTop, divisionRatio);

                //startLineList.Add(inbetweenLine);

                tempFace.lineTop = lineTop;
                tempFace.lineBottom = lineBottom;
                tempFaceList.Add(tempFace);
            }

            NMesh zebraMesh = new NMesh(tempFaceList);

            return zebraMesh;

        }

        public static Tuple<NMesh, NMesh> subdivideZebraWithRatio_RAW(NMesh mesh, double splitRatio)
        {
            double areaTotalMesh = 0;

            List<NFace> emptyList = new List<NFace>();
            NMesh emptyMesh = new NMesh(emptyList);


            // check if special cases
            if (splitRatio < 0.0001)
            {
                return new Tuple<NMesh, NMesh>(mesh, emptyMesh);
            }
            if (splitRatio > 0.9999)
            {
                return new Tuple<NMesh, NMesh>(emptyMesh, mesh);
            }




            for (int i = 0; i < mesh.faceList.Count; i++)
            {
                areaTotalMesh += mesh.faceList[i].Area;
            }


            double areaDesired = splitRatio * areaTotalMesh;
            double currentArea = 0;
            double existingArea = 0;
            int chosen = 0;

            List<NFace> facesLeft = new List<NFace>();
            List<NFace> facesRight = new List<NFace>();


            for (int i = 0; i < mesh.faceList.Count; i++)
            {
                double currentFaceArea = mesh.faceList[i].Area;
                currentArea += currentFaceArea;

                if (currentArea > areaDesired)
                {


                    // this face has to be split

                    double areaToAchieve = splitRatio * areaTotalMesh - existingArea;
                    double splitter = 1 / (currentFaceArea / areaToAchieve);
                    splitter = 1 - splitter;
                    NFace splitFace = mesh.faceList[i];

                    NFace splitTemp = splitFace.DeepCopy();

                    splitTemp.checkFor180Angle();
                    splitTemp.mergeVertexWithTol(0.05);


                    NLine direction = splitFace.lineBottom;
                    NLine direction2 = splitFace.lineTop;
                    NLine inbetweenLine = NLine.inbetweenLine(direction2, direction, splitter);

                    Tuple<double, bool> angleTuple = NLine.getSmallestAngle(inbetweenLine, Vec3d.UnitX);
                    double inbetweenAngle = angleTuple.Item1;
                    bool reversed = angleTuple.Item2;

                    splitTemp.checkFor180Angle();
                    splitTemp.updateEdgeConnectivity();


                    List<Vec3d> vecList = new List<Vec3d>();
                    for (int d = 0; d < splitTemp.edgeList.Count; d++)
                    {
                        vecList.Add(splitTemp.edgeList[d].v);
                    }

                    //splitFace.checkFor180Angle();
                    if (reversed == false)
                    {
                        NFace tempFace = NFace.checkInternalDuplicateEdge(splitTemp);
                        tempFace.updateEdgeConnectivity();
                        tempFace.checkFor180Angle();
                        tempFace.checkFor180Angle();

                        /////////////////////////////////////////////////////////////
                        int decimalPlaces = 3;
                        double angleNewRoundDown = Math.Floor(inbetweenAngle * Math.Pow(10, decimalPlaces)) / Math.Pow(10, decimalPlaces);
                        Tuple<int, NMesh> splitMeshTuple = RSplit.SplitDirectionNMeshBoth(tempFace, angleNewRoundDown);

                        //clean mesh from split direction both
                        List<NFace> cleanFaces = new List<NFace>();
                        for (int j = 0; j < splitMeshTuple.Item2.faceList.Count; j++)
                        {
                            NFace faceToClean = splitMeshTuple.Item2.faceList[j];
                            faceToClean.checkFor180Angle();
                            cleanFaces.Add(faceToClean);
                        }
                        NMesh cleanMesh = new NMesh(cleanFaces);
                        List<double> debugList = new List<double>();

                        Tuple<int, NMesh, NMesh> tempMeshTuple = RSplit.SplitNMesh(cleanMesh, splitter, inbetweenAngle);


                        NMesh meshCombinedLeft = RSplit.CombineNMeshStrip(tempMeshTuple.Item2);
                        NMesh meshCombinedRight = RSplit.CombineNMeshStrip(tempMeshTuple.Item3);

                        ////////////////////////////////////////////////////////////
                        // Add bottom and top line of split face to all newly created faces
                        // otherwise the current face cannot be zebra split any further

                        foreach (NFace tempface in meshCombinedRight.faceList)
                        {
                            tempface.lineTop = splitFace.lineTop.DeepCopy();
                            tempface.lineBottom = splitFace.lineBottom.DeepCopy();
                        }

                        foreach (NFace tempface in meshCombinedLeft.faceList)
                        {
                            tempface.lineTop = splitFace.lineTop.DeepCopy();
                            tempface.lineBottom = splitFace.lineBottom.DeepCopy();
                        }

                        ////////////////////////////////////////////////////////////


                        facesLeft.AddRange(meshCombinedRight.faceList);
                        facesRight.AddRange(meshCombinedLeft.faceList);

                    }
                    else
                    {
                        NFace tempFace = NFace.checkInternalDuplicateEdge(splitTemp);

                        /////////////////////////////////////////////////////////////
                        int decimalPlaces = 5;
                        double angleNewRoundDown = Math.Floor(inbetweenAngle * Math.Pow(10, decimalPlaces)) / Math.Pow(10, decimalPlaces);
                        Tuple<int, NMesh> splitMeshTuple = RSplit.SplitDirectionNMeshBoth(tempFace, angleNewRoundDown);

                        //clean mesh from split direction both
                        List<NFace> cleanFaces = new List<NFace>();
                        for (int j = 0; j < splitMeshTuple.Item2.faceList.Count; j++)
                        {
                            NFace faceToClean = splitMeshTuple.Item2.faceList[j];
                            faceToClean.checkFor180Angle();
                            cleanFaces.Add(faceToClean);
                        }
                        NMesh cleanMesh = new NMesh(cleanFaces);
                        List<double> debugList = new List<double>();


                        Tuple<int, NMesh, NMesh> tempMeshTuple = RSplit.SplitNMesh(cleanMesh, 1 - splitter, inbetweenAngle);


                        NMesh meshCombinedLeft = RSplit.CombineNMeshStrip(tempMeshTuple.Item2);
                        NMesh meshCombinedRight = RSplit.CombineNMeshStrip(tempMeshTuple.Item3);



                        ////////////////////////////////////////////////////////////
                        // Add bottom and top line of split face to all newly created faces
                        // otherwise the current face cannot be zebra split any further

                        foreach (NFace tempface in meshCombinedRight.faceList)
                        {
                            tempface.lineTop = splitFace.lineTop.DeepCopy();
                            tempface.lineBottom = splitFace.lineBottom.DeepCopy();
                        }

                        foreach (NFace tempface in meshCombinedLeft.faceList)
                        {
                            tempface.lineTop = splitFace.lineTop.DeepCopy();
                            tempface.lineBottom = splitFace.lineBottom.DeepCopy();
                        }
                        //////////////////////////////////////////////////////////

                        facesLeft.AddRange(meshCombinedLeft.faceList);
                        facesRight.AddRange(meshCombinedRight.faceList);
                    }
                    chosen = i;
                    break;
                }
                else
                {
                    facesLeft.Add(mesh.faceList[i]);
                    existingArea = currentArea;
                }
            }


            for (int i = chosen + 1; i < mesh.faceList.Count; i++)
            {
                facesRight.Add(mesh.faceList[i]);
            }


            //-----------------------------------------------------------

            NMesh meshRight = new NMesh(facesRight);
            NMesh meshLeft = new NMesh(facesLeft);

            return new Tuple<NMesh, NMesh>(meshRight, meshLeft);
        }
        public static NMesh subdivideZebraWithRatioList(NMesh mesh, List<double> splitRatioList)
        {
            double areaTotGlobal = mesh.Area;


            List<NFace> emptyList = new List<NFace>();
            NMesh outMesh = new NMesh(emptyList);

            for (int r = 0; r < splitRatioList.Count; r++)
            {

                double splitRatio = splitRatioList[r];


                double currentArea = mesh.Area;
                double divReal = areaTotGlobal * splitRatioList[r]; // currentSplitRatio;
                double splitter = 1 / (currentArea / divReal);

                Tuple<NMesh, NMesh> zebraTuple = subdivideZebraWithRatio_RAW(mesh, splitter);

                NMesh meshRight = zebraTuple.Item1;
                mesh = meshRight;

                NMesh addToOutMesh = zebraTuple.Item2;
                addToOutMesh = RSplit.mergeNMesh(addToOutMesh);

                outMesh.faceList.AddRange(addToOutMesh.faceList);
            }

            // merge and add final mesh
            NMesh addLastMesh = mesh;
            addLastMesh = RSplit.mergeNMesh(addLastMesh);

            outMesh.faceList.AddRange(addLastMesh.faceList);


            outMesh.faceList.AddRange(mesh.faceList);
            return outMesh;
        }

        // Special Split functions
        public static NMesh SubdivideNFaceLongestSide(NFace inputFace, double length)
        {
            

            int longestEdgeIndex = inputFace.longestEdgeIndex();

            Vec3d direction = new Vec3d(inputFace.edgeList[longestEdgeIndex].Direction);

            //direction.Rotate2dZ(Math.PI / 2);
            Vec3d baseline = new Vec3d(1, 0, 0);
            //double angle = Vec3d.Angle(baseline, direction);
            double angleSpecial = Vec3d.Angle(baseline, direction);
            NFace splitFace = RSplit.subdivideNFaceEdgeByLength(inputFace, length, longestEdgeIndex);
            double rotPos = (Math.PI / 2) - angleSpecial;
            double rotNeg = (Math.PI / 2) + angleSpecial;
            double rotFin = rotPos;

            Vec3d cross = Vec3d.CrossProduct(baseline, direction);
            if (cross.Z > 0)
            {
                rotFin = rotNeg;
            }

            Tuple<int, NMesh> newSplitTuple = RSplit.SplitDirectionNMeshBoth_RAW(splitFace, rotFin);

            return newSplitTuple.Item2;
        }
        public static NMesh SubdivideNFaceShortestSide(NFace inputFace, double length)
        {
           
            int shortestEdgeIndex = inputFace.shortestEdgeIndex();

            Vec3d direction = new Vec3d(inputFace.edgeList[shortestEdgeIndex].Direction);

            //direction.Rotate2dZ(Math.PI / 2);
            Vec3d baseline = new Vec3d(1, 0, 0);
            //double angle = Vec3d.Angle(baseline, direction);
            double angleSpecial = Vec3d.Angle(baseline, direction);
            NFace splitFace = RSplit.subdivideNFaceEdgeByLength(inputFace, length, shortestEdgeIndex);
            double rotPos = (Math.PI / 2) - angleSpecial;
            double rotNeg = (Math.PI / 2) + angleSpecial;
            double rotFin = rotPos;

            Vec3d cross = Vec3d.CrossProduct(baseline, direction);
            if (cross.Z > 0)
            {
                rotFin = rotNeg;
            }

            Tuple<int, NMesh> newSplitTuple = RSplit.SplitDirectionNMeshBoth_RAW(splitFace, rotFin);

            return newSplitTuple.Item2;
        }

        // Special SPLITS RAW       (functions have no autocorrection of 180 angles
        //                           use for manual subdivision tasks)

        public static Tuple<bool, NFace, NFace> SplitDirection_RAW(NFace face, double radAngle)
        {

            List<int> outListInt = new List<int>();
            List<Vec3d> outListVec = new List<Vec3d>();
            string outstring = "";

            double areaFace = face.Area;
            double searchRadius = 1000000.0;
            Vec3d intersectionPoint = new Vec3d();


            // Iterate through points and find first edge that intersects
            for (int i = 0; i < face.edgeList.Count; i++)
            {
                // Create split vector starting at i with rotation radAngle
                Vec3d l2_p1 = new Vec3d(face.edgeList[i].v);
                Vec3d searchVec = new Vec3d(searchRadius, 0, 0);
                Vec3d axis = new Vec3d(Vec3d.UnitZ);
                Vec3d intersectionVecEnd = Vec3d.RotateAroundAxisRad(searchVec, axis, radAngle);
                Vec3d l2_p2 = intersectionVecEnd + l2_p1;

                outstring += "i: " + i;

                for (int j = 0; j < (face.edgeList.Count); j++)
                {
                    
                    int j2 = j + 1;
                    if (j2 >= face.edgeList.Count)
                    {
                        j2 = 0;
                    }

                    outstring += "j: " + j;
                    outstring += "j2: " + j2;

                    Vec3d l1_p1 = new Vec3d(face.edgeList[j].v);
                    Vec3d l1_p2 = new Vec3d(face.edgeList[j2].v);

                  
                    bool shouldIncludeEndPoints = false; // false
                    bool intersectTrue = RIntersection.AreLinesIntersecting(l1_p1, l1_p2, l2_p1, l2_p2, shouldIncludeEndPoints);


                    //outstring = "start";

                    if (intersectTrue == true)
                    {
                        //outstring = "intersect Inside True";

                        NEdge intersectionEdge = face.edgeList[j];
                        Vec3d intersectionVec = RIntersection.GetLineLineIntersectionPoint(l1_p1, l1_p2, l2_p1, l2_p2);

                        // creates the split edges
                        NEdge intersectionRightEdge = new NEdge(face.edgeList[j].v);
                        NEdge intersectionLeftEdge = new NEdge(intersectionVec);
                        NEdge intersectionLineStart = new NEdge(l2_p1);

                        NEdge intersectionLineEnd = new NEdge(intersectionVec);
                        NEdge intersectionLineStart2 = new NEdge(l2_p1);


                        if (i > j)
                        {
                            
                            List<NEdge> A_edgesFaceLeft = face.edgeList.GetRange(0, j2);
                            A_edgesFaceLeft.Add(intersectionLeftEdge);

                            List<NEdge> A_edgesFaceLeftTemp = face.edgeList.GetRange(i, face.edgeList.Count - i);
                            A_edgesFaceLeft.AddRange(A_edgesFaceLeftTemp);

                            NFace A_faceLeft = new NFace(A_edgesFaceLeft);
                            A_faceLeft.updateEdgeConnectivity();

                            List<NEdge> A_edgesFaceRight = face.edgeList.GetRange(j2, i - j2);
                            A_edgesFaceRight.Add(face.edgeList[i]);
                            A_edgesFaceRight.Add(intersectionLeftEdge);

                            NFace A_faceRight = new NFace(A_edgesFaceRight);
                            A_faceRight.updateEdgeConnectivity();

                            bool isIntersectingARight = A_faceRight.checkSelfIntersect();
                            if (isIntersectingARight == false)
                            {
                                bool isIntersectingALeft = A_faceLeft.checkSelfIntersect();
                                if (isIntersectingALeft == false)
                                {
                                    double areaAL = A_faceLeft.Area;
                                    double areaAR = A_faceRight.Area;
                                    double areaThreshold = 0.000001;
                                    if ((areaAL + areaAR) <= (areaFace + areaThreshold) && (areaAL + areaAR) >= (areaFace - areaThreshold))
                                    {
                                        A_faceLeft.mergeDuplicateVertex();

                                        A_faceRight.mergeDuplicateVertex();
                                        return new Tuple<bool, NFace, NFace>(true, A_faceLeft, A_faceRight); ////////////////////////////
                                    }

                                }
                            }

                        }
                        else
                        {

                            List<NEdge> B_edgesFaceLeft = new List<NEdge>();
                            B_edgesFaceLeft.AddRange(face.edgeList.GetRange(0, i));


                            B_edgesFaceLeft.Add(intersectionLineStart);
                            B_edgesFaceLeft.Add(intersectionLeftEdge);
                            if (j2 > 0)
                            {
                                List<NEdge> B_edgesFaceLeftTemp = face.edgeList.GetRange(j2, face.edgeList.Count - j2);
                                B_edgesFaceLeft.AddRange(B_edgesFaceLeftTemp);
                            }

                            NFace B_faceLeft = new NFace(B_edgesFaceLeft);
                            B_faceLeft.updateEdgeConnectivity();

                            // B_RIGHT
                            // B_RIGHT: create face for everything right
                            List<NEdge> B_edgesFaceRight = face.edgeList.GetRange(i, j - i);
                            B_edgesFaceRight.Add(face.edgeList[j]);
                            B_edgesFaceRight.Add(intersectionLeftEdge);


                            NFace B_faceRight = new NFace(B_edgesFaceRight);
                            B_faceRight.updateEdgeConnectivity();

                            // check for self intersections, if both checks hold, return the faces.
                            bool isIntersectingBRight = B_faceRight.checkSelfIntersect();
                            if (isIntersectingBRight == false)
                            {
                                bool isIntersectingBLeft = B_faceLeft.checkSelfIntersect();
                                if (isIntersectingBLeft == false)
                                {
                                   
                                    double areaBL = B_faceLeft.Area;
                                    double areaBR = B_faceRight.Area;
                                    double areaThreshold = 0.000001;
                                    if ((areaBL + areaBR) < (areaFace + areaThreshold) && (areaBL + areaBR) > (areaFace - areaThreshold))
                                    {
                                        // Return Left and Right faces
                                        B_faceLeft.mergeDuplicateVertex();
                                        B_faceRight.mergeDuplicateVertex();
                                        return new Tuple<bool, NFace, NFace>(true, B_faceLeft, B_faceRight); ////////////////////////////
                                    }


                                }
                            }
                        }
                    }
                }


            }

            for (int startInt = 0; startInt < face.edgeList.Count; startInt++)
            {
                for (int endInt = 0; endInt < face.edgeList.Count; endInt++)
                {
                    if (endInt != startInt && Math.Abs(endInt - startInt) > 2 && Math.Abs(endInt - startInt) < face.edgeList.Count - 2)
                    {
                        Vec3d closingLine = new Vec3d(face.edgeList[endInt].v - face.edgeList[startInt].nextNEdge.v);

                        double relevantAngle = Vec3d.Angle2PI_2d(closingLine, Vec3d.UnitX);
                        double relevantAngle2 = Vec3d.Angle2PI_2d(closingLine, Vec3d.UnitZ);

                        double currentAngle = Vec3d.Angle2PI_2d(face.edgeList[startInt].Direction, closingLine);

                        // check if the next curve does not have the same angle
                        double nextAngle = Vec3d.Angle2PI_2d(face.edgeList[startInt].nextNEdge.Direction, closingLine);

                        if (Math.Abs(relevantAngle - radAngle) < Constants.AngleSplitTolerance)
                        {
                            if (Math.Abs(currentAngle - radAngle) < Constants.AngleSplitTolerance && nextAngle > Constants.AngleSplitTolerance)
                            {
                                int newStart = startInt + 1;
                                int newEnd = endInt + 1;

                                if (newStart >= face.edgeList.Count)
                                {
                                    newStart = 0;
                                }
                                if (newEnd >= face.edgeList.Count)
                                {
                                    newEnd = 0;
                                }

                                if (endInt < startInt)
                                {
                                    List<NEdge> leftEdges = new List<NEdge>();
                                    leftEdges.AddRange(face.edgeList.GetRange(startInt, face.edgeList.Count - startInt));
                                    leftEdges.RemoveAt(0);
                                    leftEdges.AddRange(face.edgeList.GetRange(0, endInt + 1));
                                    NFace leftFace = new NFace(leftEdges);

                                    List<NEdge> rightEdges = new List<NEdge>();
                                    if (newStart < startInt)
                                    {
                                        rightEdges.AddRange(face.edgeList.GetRange(newEnd, startInt - newEnd));
                                        rightEdges.AddRange(face.edgeList.GetRange(startInt, face.edgeList.Count - startInt));
                                        rightEdges.AddRange(face.edgeList.GetRange(0, newStart + 1));
                                    }
                                    else
                                    {
                                        rightEdges.AddRange(face.edgeList.GetRange(newEnd, newStart - newEnd + 1));
                                    }
                                    NFace rightFace = new NFace(rightEdges);

                                    leftFace.mergeDuplicateVertex();

                                    rightFace.mergeDuplicateVertex();
                                    return new Tuple<bool, NFace, NFace>(true, leftFace, rightFace);  

                                }
                                else
                                {
                                    // start int smaller than end int

                                    // Left Face
                                    List<NEdge> leftEdges = new List<NEdge>();
                                    leftEdges.AddRange(face.edgeList.GetRange(startInt + 1, endInt - startInt));
                                    NFace leftFace = new NFace(leftEdges);

                                    // Right Face
                                    List<NEdge> rightEdges = new List<NEdge>();
                                    rightEdges.AddRange(face.edgeList.GetRange(endInt, face.edgeList.Count - endInt));
                                    rightEdges.RemoveAt(0);
                                    rightEdges.AddRange(face.edgeList.GetRange(0, startInt + 2));
                                    NFace rightFace = new NFace(rightEdges);

                                    leftFace.mergeDuplicateVertex();

                                    rightFace.mergeDuplicateVertex();
                                    return new Tuple<bool, NFace, NFace>(true, leftFace, rightFace); 
                                }

                            }

                        }
                    }


                }
            }
            face.mergeDuplicateVertex();
            face.checkFor180Angle();
            return new Tuple<bool, NFace, NFace>(false, face, face); ////////////////////////////
        }
        public static Tuple<bool, NFace, NFace> SplitDirectionSingleWithFlip_RAW(NFace face, double radAngle)
        {

            Tuple<bool, NFace, NFace> splitTuple = RSplit.SplitDirection_RAW(face, radAngle);

            NFace outFaceLeft = splitTuple.Item2;
            NFace outFaceRight = splitTuple.Item3;
            bool outBool = splitTuple.Item1;

            if (splitTuple.Item1 == false)
            {
                face.flipRH();

                Tuple<bool, NFace, NFace> splitTuple2 = RSplit.SplitDirection_RAW(face, radAngle);
                outFaceLeft = splitTuple2.Item2;
                outFaceLeft.flipRH();
                outFaceRight = splitTuple2.Item3;
                outFaceRight.flipRH();
                outBool = splitTuple2.Item1;
            }

            return new Tuple<bool, NFace, NFace>(outBool, outFaceLeft, outFaceRight);
        }
        public static Tuple<int, NMesh, string> SplitDirectionSingleDir_RAW(NFace face, double radAngle)
        {
           

            string outstring = "";

            List<NFace> facesOut = new List<NFace>();
            List<NFace> facesToSplit = new List<NFace>();
            facesToSplit.Add(face);

            int emBreak = 0;
            int numSplits = 0;
            while ((facesToSplit.Count > 0) && (emBreak < 10000))
            {
                emBreak++;

                NFace currentFace = facesToSplit[0];
                Tuple<bool, NFace, NFace> subdivCurrentFace = RSplit.SplitDirectionSingleWithFlip_RAW(currentFace, radAngle);

                if (subdivCurrentFace.Item1 == true)
                {
                    facesToSplit.Add(subdivCurrentFace.Item2);
                    facesToSplit.Add(subdivCurrentFace.Item3);
                    numSplits++;
                }
                else
                {
                    facesOut.Add(currentFace);
                }
                facesToSplit.RemoveAt(0);
            }

            outstring += numSplits;


            NMesh meshOut = new NMesh(facesOut);

            return new Tuple<int, NMesh, string>(numSplits, meshOut, outstring);
        }
        public static Tuple<int, NMesh> SplitDirectionNMeshBoth_RAW(NFace face, double radAngle)
        {
          
            Tuple<int, NMesh, string> SplitMeshDir0 = RSplit.SplitDirectionSingleDir_RAW(face, radAngle);
            int numSplits = 0;

            List<NFace> listFaces = new List<NFace>();
            listFaces.AddRange(SplitMeshDir0.Item2.faceList);

            numSplits += SplitMeshDir0.Item1;

            List<NFace> listFacesOpposite = new List<NFace>();

            for (int l = 0; l < listFaces.Count; l++)
            {
                double radAngleOpposite = 0;
                if (radAngle >= Math.PI)
                {
                    radAngleOpposite = radAngle + Math.PI;
                }
                else
                {
                    radAngleOpposite = radAngle - Math.PI;
                }
                Tuple<int, NMesh, string> SplitMeshDir1 = RSplit.SplitDirectionSingleDir_RAW(listFaces[l], (radAngleOpposite));
                numSplits += SplitMeshDir1.Item1;

                for (int m = 0; m < SplitMeshDir1.Item2.faceList.Count; m++)
                {

                    listFacesOpposite.Add(SplitMeshDir1.Item2.faceList[m]);
                }
            }


            // Combine out faces into a new mesh for output
            NMesh meshOut = new NMesh(listFacesOpposite);
            meshOut.sortFacesByAngle(radAngle);
            return new Tuple<int, NMesh>(numSplits, meshOut);

        }
        public static Tuple<NMesh, NMesh> SubdivideNFaceDirection_RAW(NFace face1, double splitRatio, double radAngle)
        {
            // same as subdivide nface direction, but does not discard small faces

            // 01 split according to angle list
            Tuple<int, NMesh> splitMeshTuple = RSplit.SplitDirectionNMeshBoth(face1, radAngle);

            //clean mesh from split direction both
            List<NFace> cleanFaces = new List<NFace>();
            for (int j = 0; j < splitMeshTuple.Item2.faceList.Count; j++)
            {
                NFace faceToClean = splitMeshTuple.Item2.faceList[j];
                faceToClean.checkFor180Angle();

                cleanFaces.Add(faceToClean);
            }
            NMesh cleanMesh = new NMesh(cleanFaces);
            List<double> debugList = new List<double>();


            Tuple<int, NMesh, NMesh> tempMeshTuple = RSplit.SplitNMesh(cleanMesh, splitRatio, radAngle);


            NMesh meshLeft = RSplit.CombineNMeshStrip(tempMeshTuple.Item2);
            NMesh meshRight = RSplit.CombineNMeshStrip(tempMeshTuple.Item3);



            meshLeft = RSplit.mergeNMesh(meshLeft);
            meshRight = RSplit.mergeNMesh(meshRight);


            return new Tuple<NMesh, NMesh>(meshLeft, meshRight);
        }

        // Split with Geometry

        

        // Combine Generic
        public static NMesh mergeNMeshBySameProperty(NMesh inputMesh)
        {

            List<string> mergeStrings = new List<string>();

            // go through each face
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
                string id_old = facesExisting[i].merge_id;
                NFace tempFace = NFace.checkInternalDuplicateEdge(facesExisting[i]);
                tempFace.merge_id = id_old;
                outFaces.Add(tempFace);
            }

            NMesh outMesh = new NMesh(outFaces);

            //NMesh outMesh = new NMesh(facesExisting);

            return outMesh;
        }
        public static NMesh mergeNMeshByPropertyList(NMesh inputMesh, List<string> mergeStringList)
        {
            

            List<NFace> facesExisting = new List<NFace>();
            List<NFace> facesToMerge = new List<NFace>();

            for (int i = 0; i < inputMesh.faceList.Count; i++)
            {
                if (mergeStringList.Contains(inputMesh.faceList[i].merge_id))
                    facesToMerge.Add(inputMesh.faceList[i]);
                else
                    facesExisting.Add(inputMesh.faceList[i]);
            }

            NMesh tempMesh = new NMesh(facesToMerge);

            NMesh mergedTempMesh = RSplit.mergeNMesh(tempMesh);

            // add id to the merged mesh (0 of merge list)
            for (int i = 0; i < mergedTempMesh.faceList.Count; i++)
            {
                mergedTempMesh.faceList[i].merge_id = mergeStringList[0];
            }

            facesExisting.AddRange(mergedTempMesh.faceList);

            List<NFace> outFaces = new List<NFace>();
            for (int i = 0; i < facesExisting.Count; i++)
            {
                string id_old = facesExisting[i].merge_id;
                NFace tempFace = NFace.checkInternalDuplicateEdge(facesExisting[i]);
                tempFace.merge_id = id_old;
                outFaces.Add(tempFace);
            }

            NMesh outMesh = new NMesh(outFaces);

            //NMesh outMesh = new NMesh(facesExisting);

            return outMesh;
        }
        public static NMesh mergeNMeshFaceByList(NMesh inputMesh, List<int> mergeInt)
        {
           

            List<NFace> facesExisting = new List<NFace>();
            List<NFace> facesToMerge = new List<NFace>();

            for (int i = 0; i < inputMesh.faceList.Count; i++)
            {
                if (mergeInt.Contains(i))
                    facesToMerge.Add(inputMesh.faceList[i]);
                else
                    facesExisting.Add(inputMesh.faceList[i]);
            }

            NMesh tempMesh = new NMesh(facesToMerge);

            NMesh mergedTempMesh = RSplit.mergeNMesh(tempMesh);

            facesExisting.AddRange(mergedTempMesh.faceList);

            NMesh outMesh = new NMesh(facesExisting);

            return outMesh;
        }
        public static Tuple<bool, NMesh> mergeAdjacentNFaceByInt(NMesh inputMesh, int faceAInt, int faceBInt)
        {
            
            List<NFace> facesOut = new List<NFace>();
            // string outstring = "";
            // Try combining the two faces
            NFace inputFace0 = inputMesh.faceList[faceAInt];
            NFace inputFace1 = inputMesh.faceList[faceBInt];
            Tuple<bool, NFace> facesCombinedInitialTuple = RSplit.CombineTwoNFaces(inputFace0, inputFace1);

            // if didnt work, check distances of vertices to see if they intersect
            if (facesCombinedInitialTuple.Item1 == false)
            {
                for (int u = 3; u < inputFace0.edgeList.Count; u++)
                {
                    inputFace0.checkFor180Angle();
                }
                for (int u = 3; u < inputFace1.edgeList.Count; u++)
                {
                    inputFace1.checkFor180Angle();
                }

                Tuple<NFace, NFace> facesSplitTuple = RSplit.splitAdjacentNFace(inputFace0, inputFace1);
                inputFace0 = facesSplitTuple.Item1;
                inputFace1 = facesSplitTuple.Item2;
            }

            Tuple<bool, NFace> facesCombinedOutTuple = RSplit.CombineTwoNFaces(inputFace0, inputFace1);

            for (int i = 0; i < inputMesh.faceList.Count; i++)
            {
                if (i == faceAInt)
                {
                    facesOut.Add(facesCombinedOutTuple.Item2);
                    if (facesCombinedOutTuple.Item1 == false)
                        facesOut.Add(inputFace1);
                }
                else if (i != faceBInt)
                {
                    facesOut.Add(inputMesh.faceList[i]);
                }

            }

            NMesh meshOut = new NMesh(facesOut);
            return new Tuple<bool, NMesh>(facesCombinedOutTuple.Item1, meshOut);
        }
        public static NMesh mergeNMesh(NMesh inputMesh)
        {
            //merges all faces in mesh if possible
            //similar to boolean union

            NMesh splitMesh = inputMesh;
            int numInts = inputMesh.faceList.Count;
            //string outstring = "";

            // only does it if mesh has 1 or more faces
            if (numInts > 0)
            {
                List<NFace> remainingFaces = splitMesh.faceList;
                NFace currentFace = remainingFaces[0];
                remainingFaces.RemoveAt(0);

                int emBreak = 0;
                int intFaceB = 0;


                while (remainingFaces.Count > 0 && emBreak < 1000)
                {
                    emBreak++;

                    Tuple<bool, NFace> mergeTuple = mergeNFace(currentFace, remainingFaces[intFaceB]);

                    if (mergeTuple.Item1)
                    {
                        //outstring += "jerer";
                        currentFace = mergeTuple.Item2;
                        remainingFaces.RemoveAt(intFaceB);
                    }

                    intFaceB++;
                    if (intFaceB > remainingFaces.Count - 1)
                        intFaceB = 0;
                }


                List<NFace> outFaces = new List<NFace>();
                currentFace.shrinkFace();
                outFaces.Add(currentFace);
                outFaces.AddRange(remainingFaces);

                NMesh outMesh = new NMesh(outFaces);

                return outMesh;
            }
            else
                return inputMesh;
            
        }
        public static Tuple<bool, NFace> mergeNFace(NFace inputFace0, NFace inputFace1)
        {

            List<NFace> facesOut = new List<NFace>();
            //string outstring = "";
            // Try combining the two faces
            Tuple<bool, NFace> facesCombinedInitialTuple = RSplit.CombineTwoNFaces(inputFace0, inputFace1);

            // if didnt work, check distances of vertices to see if they intersect
            if (facesCombinedInitialTuple.Item1 == false)
            {
                for (int u = 3; u < inputFace0.edgeList.Count; u++)
                {
                    inputFace0.checkFor180Angle();
                }
                for (int u = 3; u < inputFace1.edgeList.Count; u++)
                {
                    inputFace1.checkFor180Angle();
                }

                Tuple<NFace, NFace> facesSplitTuple = splitAdjacentNFace(inputFace0, inputFace1);
                inputFace0 = facesSplitTuple.Item1;
                inputFace1 = facesSplitTuple.Item2;
            }

            Tuple<bool, NFace> facesCombinedOutTuple = RSplit.CombineTwoNFaces(inputFace0, inputFace1);

            if (facesCombinedOutTuple.Item1 == true)
                facesOut.Add(facesCombinedOutTuple.Item2);
            else
            {
                facesOut.Add(inputFace0);
                facesOut.Add(inputFace1);
            }

            //NMesh meshOut = new NMesh(facesOut);
            return new Tuple<bool, NFace>(facesCombinedOutTuple.Item1, facesCombinedOutTuple.Item2);
        }
        public static Tuple<bool, NMesh> mergeAdjacentNFace(NFace inputFace0, NFace inputFace1)
        {
            
            List<NFace> facesOut = new List<NFace>();
            // string outstring = "";
            // Try combining the two faces
            Tuple<bool, NFace> facesCombinedInitialTuple = RSplit.CombineTwoNFaces(inputFace0, inputFace1);

            // if didnt work, check distances of vertices to see if they intersect
            if (facesCombinedInitialTuple.Item1 == false)
            {
                for (int u = 3; u < inputFace0.edgeList.Count; u++)
                {
                    inputFace0.checkFor180Angle();
                }
                for (int u = 3; u < inputFace1.edgeList.Count; u++)
                {
                    inputFace1.checkFor180Angle();
                }

                Tuple<NFace, NFace> facesSplitTuple = splitAdjacentNFace(inputFace0, inputFace1);
                inputFace0 = facesSplitTuple.Item1;
                inputFace1 = facesSplitTuple.Item2;
            }

            Tuple<bool, NFace> facesCombinedOutTuple = RSplit.CombineTwoNFaces(inputFace0, inputFace1);
            
            if (facesCombinedOutTuple.Item1 == true)
                facesOut.Add(facesCombinedOutTuple.Item2);
            else
            {
                facesOut.Add(inputFace0);
                facesOut.Add(inputFace1);
            }

            NMesh meshOut = new NMesh(facesOut);
            return new Tuple<bool, NMesh>(facesCombinedOutTuple.Item1, meshOut);
        }
        public static Tuple<NFace, NFace> splitAdjacentNFace(NFace inputFace0, NFace inputFace1)
        {
           
            string outstring = "";

            // for all vertices of face0 check distance to face1
            for (int i = 0; i < inputFace0.edgeList.Count; i++)
            {
                outstring += inputFace0.edgeList[i].v;
                outstring += "\n";

                inputFace1.addIntersectVertNFace(inputFace0.edgeList[i].v);
            }
            // for all vertices of face1 check distance to face0
            for (int i = 0; i < inputFace1.edgeList.Count; i++)
            {
                outstring += inputFace1.edgeList[i].v;
                outstring += "\n";

                inputFace0.addIntersectVertNFace(inputFace1.edgeList[i].v);
            }

            return new Tuple<NFace, NFace>(inputFace0, inputFace1);

        }
        
        // Strip Combine
        public static Tuple<bool, NFace> CombineTwoNFaces(NFace inputFace0, NFace inputFace1)
        {
            
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

            int exist0 = 0;
            int exist1 = 0;
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
                        exist0 = i;
                        exist1 = j;
                    }
                }
            }

            debugList.Add(exist0);
            debugList.Add(exist1);
            //debugList= existList1;

            List<NEdge> edgesOrdered = new List<NEdge>();
            List<NEdge> edgesRemaining = new List<NEdge>();
            if (overlapBool == true)
            {
                // remove overlap integers:
                //face0.edgeList.RemoveAt(exist0);
                edgesOrdered.AddRange(face0.edgeList);
                face1.shiftEdgesByInt(exist1);
                face1.edgeList.RemoveAt(0);
                //face1.edgeList.RemoveAt(0);
                for (int y = 0; y < face1.edgeList.Count; y++)
                {
                    edgesOrdered.Insert(exist0 + y + 1, face1.edgeList[y]);
                }
                //edgesOrdered.RemoveAt(0);
                NFace faceCombined = new NFace(edgesOrdered);
                faceCombined.updateEdgeConnectivity();
                faceCombined.mergeDuplicateVertex();
                return new Tuple<bool, NFace>(overlapBool, faceCombined);
            }
            // if thisExists
            else
            {
                return new Tuple<bool, NFace>(overlapBool, face0);
            }
        }
        public static NMesh CombineNMeshStrip(NMesh inputMesh)
        {
            // Iteratively goes through all faces of mesh and combines the ones that
            // are next to each other

            int numFaces = inputMesh.faceList.Count;
            NMesh combTuple = CombineNMeshStripOnce(inputMesh);
            for (int i = 0; i < numFaces; i++)
            {
                if (combTuple.faceList.Count > 1)
                {
                    combTuple.shiftFacesLeft();
                    combTuple = CombineNMeshStripOnce(combTuple);
                }
            }
            return combTuple;
        }
        public static NMesh CombineNMeshStripOnce(NMesh inputMesh)
        {
            List<NFace> otherFaceList = new List<NFace>();

            List<int> debugList = new List<int>();

            int iterator = inputMesh.faceList.Count;

            // check if mesh has more than one face
            if (iterator > 0)
            {
                NFace tempFace = inputMesh.faceList[0].DeepCopy();

                for (int i = 1; i < iterator; i++)
                {
                    // Intersect Faces with each other
                    Tuple<int, NFace, NFace> intersectTuple = IntersectNFaces(tempFace, inputMesh.faceList[i]);
                    NFace face0 = intersectTuple.Item2;
                    NFace face1 = intersectTuple.Item3;

                    face0.updateEdgeConnectivity();
                    face1.updateEdgeConnectivity();

                    // combine Faces into one
                    Tuple<bool, NFace> combinedF = CombineTwoNFaces(face0, face1);

                    if (combinedF.Item1 == false)
                    {
                        otherFaceList.Add(face1);
                    }

                    tempFace = combinedF.Item2;

                }
                List<NFace> combinedFaceList = new List<NFace>();
                combinedFaceList.Add(tempFace);
                combinedFaceList.AddRange(otherFaceList);
                NMesh meshOutput = new NMesh(combinedFaceList);

                return meshOutput;
            }
            else
                return inputMesh;
            
        }


        public static Tuple<bool, NFace> CombineFacesBest(NFace inputFace0, NFace inputFace1)
        {
          
            NFace faceOut = inputFace0;

            NFace face_A_N = inputFace0.DeepCopyWithMID();
            NFace face_B_N = inputFace1.DeepCopyWithMID();

            NFace face_A_F = inputFace0.DeepCopyWithMID();
            face_A_F.flipRH();
            NFace face_B_F = inputFace1.DeepCopyWithMID();
            face_B_F.flipRH();

            Tuple<bool, NFace> tupleNorm_N_N = CombineFacesRAW(face_A_N, face_B_N);
            Tuple<bool, NFace> tupleNorm_N_F = CombineFacesRAW(face_A_N, face_B_F);
            Tuple<bool, NFace> tupleNorm_F_N = CombineFacesRAW(face_A_F, face_B_N);
            Tuple<bool, NFace> tupleNorm_F_F = CombineFacesRAW(face_A_F, face_B_F);

            if (tupleNorm_N_N.Item1)
            {
                //outstring += " N_N";
                bool intersecting = NFace.selfIntersectingNEdgesVecs(tupleNorm_N_N.Item2);
                if (intersecting == false)
                {
                    faceOut = tupleNorm_N_N.Item2;
                    //outstring += " + N_N";
                }
            }
            if (tupleNorm_N_F.Item1)
            {
                bool intersecting = NFace.selfIntersectingNEdgesVecs(tupleNorm_N_F.Item2);
                if (intersecting == false)
                {
                    faceOut = tupleNorm_N_F.Item2;
                    //outstring += " + N_F";
                }
            }
            if (tupleNorm_F_N.Item1)
            {
                bool intersecting = NFace.selfIntersectingNEdgesVecs(tupleNorm_F_N.Item2);
                if (intersecting == false)
                {
                    faceOut = tupleNorm_F_N.Item2;
                    //outstring += " + F_N";
                }
            }
            if (tupleNorm_F_F.Item1)
            {
                bool intersecting = NFace.selfIntersectingNEdgesVecs(tupleNorm_F_F.Item2);
                if (intersecting == false)
                {
                    faceOut = tupleNorm_F_F.Item2;
                    //outstring += " + F_F";
                }
            }

            return new Tuple<bool, NFace>(true, faceOut);
        }

        private static Tuple<bool, NFace> CombineFacesRAW(NFace inputFace0, NFace inputFace1)
        {
            // TEMP Do not use

            // update face0 and face1 edge end temp
            NFace face0 = inputFace0.DeepCopy();
            NFace face1 = inputFace1.DeepCopy();

            if (face0.IsClockwise == true)
            {
                //face0.flipRH();
            }
            if (face1.IsClockwise == true)
            {
                //face1.flipRH();
            }


            List<int> debugList = new List<int>();


            face0.updateEdgeConnectivity();
            face1.updateEdgeConnectivity();



            face0.UpdateTempEnd();
            face1.UpdateTempEnd();

            // 2 ------------------------------------------
            // check if adjacent and delete duplicate edges

            int exist0 = 0;
            int exist1 = 0;
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
                        exist0 = i;
                        exist1 = j;
                    }
                }
            }

            debugList.Add(exist0);
            debugList.Add(exist1);
            //debugList= existList1;

            List<NEdge> edgesOrdered = new List<NEdge>();
            List<NEdge> edgesRemaining = new List<NEdge>();
            if (overlapBool == true)
            {
                // remove overlap integers:
                //face0.edgeList.RemoveAt(exist0);
                edgesOrdered.AddRange(face0.edgeList);
                face1.shiftEdgesByInt(exist1);
                face1.edgeList.RemoveAt(0);
                //face1.edgeList.RemoveAt(0);
                for (int y = 0; y < face1.edgeList.Count; y++)
                {
                    edgesOrdered.Insert(exist0 + y + 1, face1.edgeList[y]);
                }
                //edgesOrdered.RemoveAt(0);
                NFace faceCombined = new NFace(edgesOrdered);
                faceCombined.updateEdgeConnectivity();
                faceCombined.mergeDuplicateVertex();
                return new Tuple<bool, NFace>(overlapBool, faceCombined);
            }
            // if thisExists
            else
            {
                return new Tuple<bool, NFace>(overlapBool, face0);
            }
        }

        // Make Ortho
        public static NFace orthoFaceLoop(NFace inputFace, Vec3d dirVec)
        {
            List<NEdge> outList = new List<NEdge>();

            for (int i = 0; i < inputFace.edgeList.Count; i++)
            {
                List<NEdge> tempList = orthoFaceEdgeReturn(inputFace, i, dirVec);
                outList.AddRange(tempList);
            }

            NFace faceOut = new NFace(outList);
            return faceOut;
        }
        public static List<NEdge> orthoFaceEdgeReturn(NFace inputFace, int edgeInt, Vec3d dirVec)
        {
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

            if (Vec3d.Distance(ptB, ptC) < 0.0001)
            {
                return edgesOut;
                //return returnFace;
            }
            else
            {

                NEdge eB = new NEdge(ptB);
                NEdge eC = new NEdge(ptC);

                if (Vec3d.Distance(startVec, ptB) > 0.0001 && Vec3d.Distance(endVec, ptB) > 0.0001)
                    edgesOut.Add(eB);

                if (Vec3d.Distance(startVec, ptC) > 0.0001 && Vec3d.Distance(endVec, ptC) > 0.0001)
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
    }
}
