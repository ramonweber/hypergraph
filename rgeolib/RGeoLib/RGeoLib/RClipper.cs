using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clipper2Lib;
using NPOI.SS.Formula.Functions;
using static Rhino.Runtime.ViewCaptureWriter;


namespace RGeoLib
{
    public class RClipper
    {
        // Interface with Clipper Library, intersectionPrepper adds vertices to avoid rounding errors....

        public static NFace intersectionPrepper(NFace faceToCheck, List<NFace> others)
        {
            // adds control points wherever there is an intersection with another face
            // this avoids rounding errors
            double tol = 0.00001;
            // get all points from faces
            List<Vec3d> vecsToCheck = new List<Vec3d>();
            for (int i = 0; i < others.Count; i++)
            {
                List<Vec3d> tempVecs = others[i].getPoints();
                vecsToCheck.AddRange(tempVecs);
            }

            // go through all points, check if distance to face > 0.0001
            for (int i = 0;i < vecsToCheck.Count;i++)
            {
                bool intersects = faceToCheck.addIntersectVertNFace(vecsToCheck[i]);
            }
            //public bool addIntersectVertNFace(Vec3d vecToAdd)
            return faceToCheck;
        }
        public static NFace intersectionPrepper(NFace faceToCheck, NFace other)
        {
            // adds control points wherever there is an intersection with another face
            // this avoids rounding errors
            double tol = 0.00001;
            // get all points from faces
            List<Vec3d> vecsToCheck = other.getPoints(); ;
 
            // go through all points, check if distance to face > 0.0001
            for (int i = 0; i < vecsToCheck.Count; i++)
            {
                bool intersects = faceToCheck.addIntersectVertNFace(vecsToCheck[i]);
            }
            //public bool addIntersectVertNFace(Vec3d vecToAdd)
            return faceToCheck;
        }

        // converts a NFAce to a clipper PathD
        public static PathD NFaceToClipperPathD(NFace inputFace)
        {
            List<PointD> tempPointList = new List<PointD>();
            for (int i = 0; i < inputFace.edgeList.Count; i++)
            {
                PointD tempPD = new PointD(inputFace.edgeList[i].v.X, inputFace.edgeList[i].v.Y);
                tempPointList.Add(tempPD);
            }
            PathD tempPath = new PathD(tempPointList);
            return tempPath;
        }
        public static PathD NLineListToClipperPathD(List<NLine> inputLine)
        {
            List<PointD> tempPointList = new List<PointD>();
            for (int i = 0; i < inputLine.Count; i++)
            {
                PointD tempPD = new PointD(inputLine[i].start.X, inputLine[i].start.Y);
                tempPointList.Add(tempPD);
            }
            PointD tempPD2 = new PointD(inputLine[inputLine.Count - 1].end.X, inputLine[inputLine.Count - 1].end.Y);
            tempPointList.Add(tempPD2);

            PathD tempPath = new PathD(tempPointList);
            return tempPath;
        }
        public static PathD NPolyLineToClipperPathD(NPolyLine inputLine)
        {
            List<PointD> tempPointList = new List<PointD>();
            for (int i = 0; i < inputLine.VecsAll.Count; i++)
            {
                PointD tempPD = new PointD(inputLine.VecsAll[i].X, inputLine.VecsAll[i].Y);
                tempPointList.Add(tempPD);
            }

            PathD tempPath = new PathD(tempPointList);
            return tempPath;
        }
        public static PathD NLineToClipperPathD(NLine inputLine)
        {
            List<PointD> tempPointList = new List<PointD>();

            PointD tempPD = new PointD(inputLine.start.X, inputLine.start.Y);
            tempPointList.Add(tempPD);
            
            PointD tempPD2 = new PointD(inputLine.end.X, inputLine.end.Y);
            tempPointList.Add(tempPD2);

            PathD tempPath = new PathD(tempPointList);
            return tempPath;
        }
        public static NFace ClipperPathDToNFace(PathD inputPath)
        {
            List<PointD> tempPointList = new List<PointD>();
            List<Vec3d> tempVecs = new List<Vec3d>();

            for (int i = 0; i < inputPath.Count; i++)
            {
                Vec3d tempVec = new Vec3d(inputPath[i].x, inputPath[i].y,0);
                tempVecs.Add(tempVec);
            }
            
            NFace tempFace = new NFace(tempVecs);
            return tempFace;
        }
        public static NMesh ClipperPathsDToNMesh(PathsD inputPaths)
        {
            List<NFace> faces = new List<NFace>();

            for(int i = 0;i < inputPaths.Count;i++)
            {
                faces.Add(ClipperPathDToNFace(inputPaths[i]));
            }

            NMesh tempMesh = new NMesh(faces);
            return tempMesh;
        }
        public static NPolyLine ClipperPathDToNPolyLine(PathD inputPath)
        {
            List<PointD> tempPointList = new List<PointD>();
            List<Vec3d> tempVecs = new List<Vec3d>();

            for (int i = 0; i < inputPath.Count; i++)
            {
                Vec3d tempVec = new Vec3d(inputPath[i].x, inputPath[i].y, 0);
                tempVecs.Add(tempVec);
            }

            NPolyLine tempPL = new NPolyLine(tempVecs);
            return tempPL;
        }
        public static List<NPolyLine> ClipperPathsDToNPolyLine(PathsD inputPaths)
        {
            List<NPolyLine> plList = new List<NPolyLine>();

            for (int i = 0; i < inputPaths.Count; i++)
            {
                plList.Add(ClipperPathDToNPolyLine(inputPaths[i]));
            }
            return plList;

        }

       
        // test method for clipper2
        public static NMesh clipperIntersection(NFace inputFaceA, NFace inputFaceB)
        {
            PathsD subj = new PathsD();
            PathsD clip = new PathsD();

            inputFaceA = intersectionPrepper(inputFaceA,inputFaceB);
            inputFaceB = intersectionPrepper(inputFaceB, inputFaceA);

            subj.Add(NFaceToClipperPathD(inputFaceA));
            clip.Add(NFaceToClipperPathD(inputFaceB));

            PathsD solution = Clipper.Intersect(subj, clip, FillRule.NonZero,8);

            NMesh outMesh = ClipperPathsDToNMesh(solution); 

            return outMesh;
        }
        public static NMesh clipperIntersection(NMesh inputMesh, NFace inputBounds)
        {
            List<NFace> facePrep = new List<NFace>();
            for (int i = 0; i < inputMesh.faceList.Count; i++)
            {
                NMesh intersectionMesh = RClipper.clipperIntersection(inputBounds, inputMesh.faceList[i]);
                facePrep.AddRange(intersectionMesh.faceList);
            }

            NMesh fout = new NMesh(facePrep);
            return fout;
        }
        public static NMesh clipperUnion(NFace inputFaceA, NFace inputFaceB)
        {
            PathsD subj = new PathsD();
            PathsD clip = new PathsD();

            inputFaceA = intersectionPrepper(inputFaceA, inputFaceB);
            inputFaceB = intersectionPrepper(inputFaceB, inputFaceA);

            subj.Add(NFaceToClipperPathD(inputFaceA));
            clip.Add(NFaceToClipperPathD(inputFaceB));

            PathsD solution = Clipper.Union(subj, clip, FillRule.NonZero,8);

            NMesh outMesh = ClipperPathsDToNMesh(solution);

            return outMesh;
        }
        public static NMesh clipperUnion(List<NFace> inputFaceA)
        {
            NMesh outMesh = new NMesh(inputFaceA);

            PathsD subj = new PathsD();
            PathsD clip = new PathsD();

            for (int i = 0; i < inputFaceA.Count; i++)
            {
                var others = inputFaceA.Where((v, x) => x != i).ToList();

                inputFaceA[i] = intersectionPrepper(inputFaceA[i], others);
            }

            clip.Add(NFaceToClipperPathD(inputFaceA[0]));

            if (inputFaceA.Count > 1)
            {
                for (int i = 1; i < inputFaceA.Count; i++)
                {
                    subj.Add(NFaceToClipperPathD(inputFaceA[i]));
                }

                PathsD solution = Clipper.Union(clip, subj, FillRule.NonZero, 8);


                outMesh = ClipperPathsDToNMesh(solution);
            }
            return outMesh;
        }

        public static NMesh clipperDifference(NFace inputFaceA, NFace inputFaceB)
        {
            PathsD subj = new PathsD();
            PathsD clip = new PathsD();

            subj.Add(NFaceToClipperPathD(inputFaceA));
            clip.Add(NFaceToClipperPathD(inputFaceB));

            inputFaceA = intersectionPrepper(inputFaceA, inputFaceB);
            inputFaceB = intersectionPrepper(inputFaceB, inputFaceA);

            PathsD solution = Clipper.Difference(subj, clip, FillRule.NonZero, 8);

            NMesh outMesh = ClipperPathsDToNMesh(solution);

            return outMesh;
        }
        public static NMesh clipperDifference(NMesh inputMesh, NFace inputBounds)
        {
            List<NFace> facePrep = new List<NFace>();
            for (int i = 0; i < inputMesh.faceList.Count; i++)
            {
                NMesh intersectionMesh = RClipper.clipperDifference(inputMesh.faceList[i], inputBounds);
                facePrep.AddRange(intersectionMesh.faceList);
            }

            NMesh fout = new NMesh(facePrep);
            return fout;
        }

        public static NMesh clipperUnionLowTol(List<NFace> inputFaceA)
        {
            NMesh outMesh = new NMesh(inputFaceA);

            PathsD subj = new PathsD();
            PathsD clip = new PathsD();

            for (int i = 0; i < inputFaceA.Count; i++)
            {
                var others = inputFaceA.Where((v, x) => x != i).ToList();

                inputFaceA[i] = intersectionPrepper(inputFaceA[i], others);
            }

            clip.Add(NFaceToClipperPathD(inputFaceA[0]));

            if (inputFaceA.Count > 1)
            {
                for (int i = 1; i < inputFaceA.Count; i++)
                {
                    subj.Add(NFaceToClipperPathD(inputFaceA[i]));
                }

                PathsD solution = Clipper.Union(clip, subj, FillRule.NonZero, 2);


                outMesh = ClipperPathsDToNMesh(solution);
            }
            return outMesh;
        }

        public static List<NPolyLine> clipperIntersectionLine(NLine inputLine, NFace inputFaceA)
        {
            // DOES NOT WORK... DONT USE....

            PathsD subj = new PathsD();
            PathsD clip = new PathsD();

            //inputFaceA = intersectionPrepper(inputFaceA, inputFaceB);
            //inputFaceB = intersectionPrepper(inputFaceB, inputFaceA);

            clip.Add(NLineToClipperPathD(inputLine));
            subj.Add(NFaceToClipperPathD(inputFaceA));

            PathsD solution = Clipper.Intersect(subj, clip, FillRule.EvenOdd, 8);

            List<NPolyLine> outPList = ClipperPathsDToNPolyLine(solution);

            return outPList;
        }


        // test method for offsetting
        public static NMesh clipperInflatePathsMitterPolygon(NFace inputFace, double deltaIn)
        {
            //PathD singlePath = NPolyLineToClipperPathD(inputPoly);
            PathD singlePath = NFaceToClipperPathD(inputFace);

            PathsD paths = new PathsD();
            paths.Add(singlePath);

            double delta = deltaIn; // the offset distance
            JoinType joinType = JoinType.Miter; //http //www angusj com/clipper2/Docs/Units/Clipper/Types/JoinType.htm
            EndType endType = EndType.Polygon; // http //www angusj com/clipper2/Docs/Units/Clipper/Types/EndType.htm
            PathsD inflatedPaths = Clipper.InflatePaths(paths, delta, joinType, endType);

            //List<NPolyLine> polyOut = ClipperPathsDToNPolyLine(inflatedPaths);
            NMesh meshOut = ClipperPathsDToNMesh(inflatedPaths);
            return meshOut;
        }
        public static List<NPolyLine> clipperInflatePathsMitterPolygon(NPolyLine inputPoly, double deltaIn)
        {
            PathD singlePath = NPolyLineToClipperPathD(inputPoly);

            PathsD paths = new PathsD();
            paths.Add(singlePath);

            double delta = deltaIn; // the offset distance
            JoinType joinType = JoinType.Miter; //http //www angusj com/clipper2/Docs/Units/Clipper/Types/JoinType.htm
            EndType endType = EndType.Polygon; // http //www angusj com/clipper2/Docs/Units/Clipper/Types/EndType.htm
            PathsD inflatedPaths = Clipper.InflatePaths(paths, delta, joinType, endType);

            List<NPolyLine> polyOut = ClipperPathsDToNPolyLine(inflatedPaths);
            return polyOut;
        }

        public static List<NPolyLine> clipperInflatePathsSquarePolygon(NPolyLine inputPoly, double deltaIn)
        {
            PathD singlePath = NPolyLineToClipperPathD(inputPoly);

            PathsD paths = new PathsD();
            paths.Add(singlePath);

            double delta = deltaIn; // the offset distance
            JoinType joinType = JoinType.Square; //http //www angusj com/clipper2/Docs/Units/Clipper/Types/JoinType.htm
            EndType endType = EndType.Polygon; // http //www angusj com/clipper2/Docs/Units/Clipper/Types/EndType.htm
            PathsD inflatedPaths = Clipper.InflatePaths(paths, delta, joinType, endType);

            List<NPolyLine> polyOut = ClipperPathsDToNPolyLine(inflatedPaths);
            return polyOut;
        }
        public static NMesh clipperInflatePathsSquarePolygon(NFace inputFace, double deltaIn)
        {
            //PathD singlePath = NPolyLineToClipperPathD(inputPoly);
            PathD singlePath = NFaceToClipperPathD(inputFace);

            PathsD paths = new PathsD();
            paths.Add(singlePath);

            double delta = deltaIn; // the offset distance
            JoinType joinType = JoinType.Square; //http //www angusj com/clipper2/Docs/Units/Clipper/Types/JoinType.htm
            EndType endType = EndType.Polygon; // http //www angusj com/clipper2/Docs/Units/Clipper/Types/EndType.htm
            PathsD inflatedPaths = Clipper.InflatePaths(paths, delta, joinType, endType);

            //List<NPolyLine> polyOut = ClipperPathsDToNPolyLine(inflatedPaths);
            //return polyOut;
            NMesh meshOut = ClipperPathsDToNMesh(inflatedPaths);
            return meshOut;
        }


        public static NMesh clipperInflatePathsSquareButtMesh(NPolyLine inputPoly, double deltaIn)
        {
            PathD singlePath = NPolyLineToClipperPathD(inputPoly);

            PathsD paths = new PathsD();
            paths.Add(singlePath);

            double delta = deltaIn; // the offset distance
            JoinType joinType = JoinType.Square; //http //www angusj com/clipper2/Docs/Units/Clipper/Types/JoinType.htm
            EndType endType = EndType.Butt; // http //www angusj com/clipper2/Docs/Units/Clipper/Types/EndType.htm
            PathsD inflatedPaths = Clipper.InflatePaths(paths, delta, joinType, endType);

            NMesh meshOut = ClipperPathsDToNMesh(inflatedPaths);
            return meshOut;
        }
        public static List<NPolyLine> clipperInflatePathsSquareButt(NPolyLine inputPoly, double deltaIn)
        {
            PathD singlePath = NPolyLineToClipperPathD(inputPoly);

            PathsD paths = new PathsD();
            paths.Add(singlePath);

            double delta = deltaIn; // the offset distance
            JoinType joinType = JoinType.Square; //http //www angusj com/clipper2/Docs/Units/Clipper/Types/JoinType.htm
            EndType endType = EndType.Butt; // http //www angusj com/clipper2/Docs/Units/Clipper/Types/EndType.htm
            PathsD inflatedPaths = Clipper.InflatePaths(paths, delta, joinType, endType);

            List<NPolyLine> polyOut = ClipperPathsDToNPolyLine(inflatedPaths);
            return polyOut;
        }
        

        public static List<NPolyLine> clipperInflatePaths(NPolyLine inputPoly, double deltaIn)
        {
            PathD singlePath = NPolyLineToClipperPathD(inputPoly);
            
            PathsD paths = new PathsD();
            paths.Add(singlePath);

            double delta = deltaIn; // the offset distance
            JoinType joinType = JoinType.Square; //http //www angusj com/clipper2/Docs/Units/Clipper/Types/JoinType.htm
            EndType endType = EndType.Butt; // http //www angusj com/clipper2/Docs/Units/Clipper/Types/EndType.htm
            PathsD inflatedPaths = Clipper.InflatePaths(paths,delta, joinType, endType);

            List<NPolyLine> polyOut = ClipperPathsDToNPolyLine(inflatedPaths); 
            return polyOut;
        }


        public static List<NPolyLine> clipperInflatePolylineSquareSquare(NPolyLine inputPoly, double deltaIn)
        {
            PathD singlePath = NPolyLineToClipperPathD(inputPoly);

            PathsD paths = new PathsD();
            paths.Add(singlePath);

            double delta = deltaIn; // the offset distance
            JoinType joinType = JoinType.Square; //http //www angusj com/clipper2/Docs/Units/Clipper/Types/JoinType.htm
            EndType endType = EndType.Square; // http //www angusj com/clipper2/Docs/Units/Clipper/Types/EndType.htm
            PathsD inflatedPaths = Clipper.InflatePaths(paths, delta, joinType, endType);

            List<NPolyLine> polyOut = ClipperPathsDToNPolyLine(inflatedPaths);
            return polyOut;
        }
        public static NMesh clipperInflatePolylineSquareSquareMesh(NPolyLine inputPoly, double deltaIn)
        {
            PathD singlePath = NPolyLineToClipperPathD(inputPoly);

            PathsD paths = new PathsD();
            paths.Add(singlePath);

            double delta = deltaIn; // the offset distance
            JoinType joinType = JoinType.Square; //http //www angusj com/clipper2/Docs/Units/Clipper/Types/JoinType.htm
            EndType endType = EndType.Square; // http //www angusj com/clipper2/Docs/Units/Clipper/Types/EndType.htm
            PathsD inflatedPaths = Clipper.InflatePaths(paths, delta, joinType, endType);

            NMesh meshOut = ClipperPathsDToNMesh(inflatedPaths);
            return meshOut;
        }


        public static List<NPolyLine> clipperInflatePolylineMiterSquare(NPolyLine inputPoly, double deltaIn)
        {
            PathD singlePath = NPolyLineToClipperPathD(inputPoly);

            PathsD paths = new PathsD();
            paths.Add(singlePath);

            double delta = deltaIn; // the offset distance
            JoinType joinType = JoinType.Miter; //http //www angusj com/clipper2/Docs/Units/Clipper/Types/JoinType.htm
            EndType endType = EndType.Square; // http //www angusj com/clipper2/Docs/Units/Clipper/Types/EndType.htm
            PathsD inflatedPaths = Clipper.InflatePaths(paths, delta, joinType, endType);

            List<NPolyLine> polyOut = ClipperPathsDToNPolyLine(inflatedPaths);
            return polyOut;
        }
        public static NMesh clipperInflatePolylineMiterSquareMesh(NPolyLine inputPoly, double deltaIn)
        {
            PathD singlePath = NPolyLineToClipperPathD(inputPoly);

            PathsD paths = new PathsD();
            paths.Add(singlePath);

            double delta = deltaIn; // the offset distance
            JoinType joinType = JoinType.Miter; //http //www angusj com/clipper2/Docs/Units/Clipper/Types/JoinType.htm
            EndType endType = EndType.Square; // http //www angusj com/clipper2/Docs/Units/Clipper/Types/EndType.htm
            PathsD inflatedPaths = Clipper.InflatePaths(paths, delta, joinType, endType);

            NMesh meshOut = ClipperPathsDToNMesh(inflatedPaths);
            return meshOut;
        }


        public static List<NPolyLine> clipperInflatePolylineMiterButt(NPolyLine inputPoly, double deltaIn)
        {
            PathD singlePath = NPolyLineToClipperPathD(inputPoly);

            PathsD paths = new PathsD();
            paths.Add(singlePath);

            double delta = deltaIn; // the offset distance
            JoinType joinType = JoinType.Miter; //http //www angusj com/clipper2/Docs/Units/Clipper/Types/JoinType.htm
            EndType endType = EndType.Butt; // http //www angusj com/clipper2/Docs/Units/Clipper/Types/EndType.htm
            PathsD inflatedPaths = Clipper.InflatePaths(paths, delta, joinType, endType);

            List<NPolyLine> polyOut = ClipperPathsDToNPolyLine(inflatedPaths);
            return polyOut;
        }
        public static NMesh clipperInflatePolylineMiterButtMesh(NPolyLine inputPoly, double deltaIn)
        {
            PathD singlePath = NPolyLineToClipperPathD(inputPoly);

            PathsD paths = new PathsD();
            paths.Add(singlePath);

            double delta = deltaIn; // the offset distance
            JoinType joinType = JoinType.Miter; //http //www angusj com/clipper2/Docs/Units/Clipper/Types/JoinType.htm
            EndType endType = EndType.Butt; // http //www angusj com/clipper2/Docs/Units/Clipper/Types/EndType.htm
            PathsD inflatedPaths = Clipper.InflatePaths(paths, delta, joinType, endType);

            NMesh meshOut = ClipperPathsDToNMesh(inflatedPaths);
            return meshOut;
        }


        // Offset no sides

        public static List<NPolyLine> clipperOffsetBothSides(NPolyLine linesIn, double deltaIn)
        {
            // offsets with no ends 

            Vec3d startV = linesIn.start;
            Vec3d endV = linesIn.end;
            //NMesh outMesh = RClipper.clipperInflatePolylineMiterButtMesh(linesIn, delta);

            int lineCount = linesIn.lineList.Count;

            NMesh outMesh = RClipper.clipperInflatePolylineMiterButtMesh(linesIn, deltaIn);
            List<NLine> inputLines = NMesh.GetAllMeshLines(outMesh);


            List<NLine> outLines = new List<NLine>();

            for (int i = 0; i < inputLines.Count; i++)
            {
                // check if intersecting with start,
                bool intersectStart = RIntersection.PointLineIntersectionTol(startV, inputLines[i].start, inputLines[i].end, 0.01);

                // check if intersecting with end,
                bool intersectEnd = RIntersection.PointLineIntersectionTol(endV, inputLines[i].start, inputLines[i].end, 0.01);

                if (intersectStart == false && intersectEnd == false)
                    outLines.Add(new NLine(inputLines[i].start, inputLines[i].end));
            }

            List<NPolyLine> outPolys = NPolyLine.mergeLines(outLines);

            return outPolys;
        }



       

    }


}
