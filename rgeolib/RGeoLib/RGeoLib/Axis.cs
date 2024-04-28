using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGeoLib
{
    public class Axis
    {
        public Vec3d v;
        public Vec3d dir;

        public List<NLine> incNLines = new List<NLine>(); // all the lines included in the axis
        
        public List<NEdge> incEdges = new List<NEdge>(); // all the lines included in the axis
        
        public List<int> edgeInt= new List<int>();

        public Vec3d min; // start point of inc lines
        public Vec3d max; // end point of inc lines

        public string property { get; set; }
        public Axis(NEdge inputEdge)
        {
            // project to axis... 
            this.v = RIntersection.ProjectEdgeToNearestAxis(inputEdge);
            this.v.Round2();

            this.dir = inputEdge.Direction;
            this.dir.Norm();
            this.dir.Round2();
        }
        public Axis(NEdge inputEdge, int accuracy)
        {
            // project to axis... 
            this.v = RIntersection.ProjectEdgeToNearestAxis(inputEdge);
            this.v.Round(accuracy);

            this.dir = inputEdge.Direction;
            this.dir.Norm();
            this.dir.Round(accuracy);
        }

        public Axis(Vec3d ptA, Vec3d ptB, int accuracy)
        {
            // project to axis... 
            this.v = RIntersection.ProjectEdgeToNearestAxis(ptA, ptB);
            this.v.Round(accuracy);

            this.dir = ptB - ptA;
            this.dir.Norm();    
            this.dir.Round(accuracy);
        }

        public Axis(Vec3d ptA, Vec3d ptB)
        {
            // project to axis... 
            this.v = RIntersection.ProjectEdgeToNearestAxis(ptA, ptB);
            this.v.Round2();

            this.dir = ptB-ptA;
            this.dir.Norm();
            this.dir.Round2();
        }
        public Axis(NLine inputLine)
        {
            // project to axis... 
            this.v = RIntersection.ProjectEdgeToNearestAxis(inputLine.start, inputLine.end);
            this.v.Round2();

            this.dir = inputLine.end - inputLine.start;
            this.dir.Norm();
            this.dir.Round2();
        }

        public static Axis averageAxisList(List<Axis> inputAxis)
        {
            List<Vec3d> dirVecs = new List<Vec3d>();
            List<Vec3d> vVecs = new List<Vec3d>();

            for (int i=0; i<inputAxis.Count;i++)
            {
                vVecs.Add(inputAxis[i].v);
                dirVecs.Add(inputAxis[i].dir);
            }

            Vec3d averageV = Vec3d.averageVectorList(vVecs);
            Vec3d averageDir = Vec3d.averageVectorList(dirVecs);

            Axis newAxis = new Axis(averageV, averageV + averageDir, 5);

            return newAxis;
        }
        public static Axis averageAxis(Axis axisA, Axis axisB)
        {
            double dot = Vec3d.DotProduct(axisA.dir, axisB.dir);
            if (dot < 0)
                axisB.dir.Reverse();

            Vec3d between = Vec3d.Between(axisA.dir, axisB.dir, 0.5);
            Vec3d mid = Vec3d.Mid(axisA.v, axisB.v);
            //Axis newAxis = new Axis(Vec3d.Mid(axisA.v, axisB.v), Vec3d.Mid(axisA.dir, axisB.dir));
            Axis newAxis = new Axis(mid, mid + between, 5);

            return newAxis;

        }

        public static bool IsAxisSame(Axis axisA, Axis axisB, double maxAngle = 0.1, double maxDist = 0.1)
        {
            bool isSame = false;

            double distance = Vec3d.Distance(axisA.v, axisB.v);

            double angleDiff = Vec3d.Angle(axisA.dir, axisB.dir);

            if (angleDiff>=Math.PI)
                angleDiff -= Math.PI;

            if (distance < maxDist && angleDiff < maxAngle)
                isSame = true;

            return isSame;
        }

        public NLine combineAxisLines()
        {
            List<NPolyLine> inputPoly = NLine.shatterLines(this.incNLines);
            List<NLine> allLines = new List<NLine>();
            for (int i = 0; i < inputPoly.Count; i++)
            {
                List<NLine> uniqueLineList = NLine.getUniqueLines(inputPoly[i].lineList);
                allLines.AddRange(uniqueLineList);
            }
            

            NPolyLine tempMainLine = joinNLinesInAxis(allLines);

            NLine tempEndsLine = new NLine(tempMainLine.start, tempMainLine.end);
            return tempEndsLine;
        }

        public NPolyLine joinNLinesInAxis(List<NLine> inputLines)
        {
            // joins lines at ends if possible
            // use get unique lines first to delete overlaps!
            double joinTolerance = 0.01;

            List<NLine> sortedLines = inputLines.OrderBy(ln => Vec3d.Distance(ln.MidPoint, this.v)).ToList();

            // flip axis
            Vec3d orthoVec = Vec3d.CrossProduct(this.dir, Vec3d.UnitZ);
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
            //NLine combinedOutLine = new NLine(joinedLines[0].start, joinedLines[joinedLines.Count-1].end);
            return combinedLines;
        }




        // For Axis Snap

        public static Tuple<List<Axis>, List<Axis>> sortTwoAxisDirections(List<Axis> inputAxis, Vec3d dirOne, Vec3d dirTwo)
        {
            // points in direction of dirOne
            double tolerance = 0.5;
            List<Axis> dirOneAxisList = new List<Axis>();
            List<Axis> dirTwoAxisList = new List<Axis>();

            for (int i = 0; i < inputAxis.Count; i++)
            {
                double dotOne = Vec3d.DotProduct(inputAxis[i].dir, dirOne);
                double dotTwo = Vec3d.DotProduct(inputAxis[i].dir, dirTwo);

                if ((Math.Abs(dotOne) < Math.Abs(dotTwo)))
                    dirOneAxisList.Add(inputAxis[i]);
                else
                    dirTwoAxisList.Add(inputAxis[i]);
            }
            return new Tuple<List<Axis>, List<Axis>>(dirOneAxisList, dirTwoAxisList);
        }
        public static List<Axis> unifyAxisDirections(List<Axis> inputAxis, Vec3d dirOne, Vec3d dirTwo)
        {
            // points in direction of dirOne
            double tolerance = 0.5;
            for (int i = 0; i < inputAxis.Count; i++)
            {
                double dotOne = Vec3d.DotProduct(inputAxis[i].dir, dirOne);
                double dotTwo = Vec3d.DotProduct(inputAxis[i].dir, dirTwo);

                if ((dotOne < 0 && Math.Abs(dotOne) > 0.25))
                {
                    inputAxis[i].dir.Reverse();
                }
                if ((dotTwo < 0 && Math.Abs(dotTwo) > 0.25))
                {
                    inputAxis[i].dir.Reverse();
                }
            }

            List<Axis> sortedAxis = inputAxis.OrderBy(o => o.v.X).ThenBy(o => o.v.Y).ToList();
            return sortedAxis;
        }

        // Snapping

        public static NMesh snapMeshToAxisList(NMesh inputMesh, List<Axis> inputAxis, double snapTol)
        {
            List<Axis> tempAxis = inputAxis;

            tempAxis = unifyAxisDirections(tempAxis, Vec3d.UnitX, Vec3d.UnitY);
            inputMesh.axisList = tempAxis;
            Tuple<List<Axis>, List<Axis>> axisTuple = sortTwoAxisDirections(tempAxis, Vec3d.UnitX, Vec3d.UnitY);

            inputMesh = snapNMeshToAxisListTol(inputMesh, axisTuple.Item1, snapTol);
            inputMesh = snapNMeshToAxisListTol(inputMesh, axisTuple.Item2, snapTol);

            return inputMesh;
        }
        private static NMesh snapNMeshToAxisListTol(NMesh outMesh, List<Axis> tempAxis, double tol)
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
            outMesh = NMesh.deepCleaner(outMesh, 0.01);

            return outMesh;
        }

        // AXIS
        // vec3d v  is on x or y or z axis! 
        // vec3d dir   NEdge direction
        //
        //   NEdge AB
        // 
        //   x
        //   ^
        //   |
        //   v---A----B--   
        //   |  
        //   |
        //   0------> y 
        //
        public override string ToString()
        {
            return $"AXIS origin[{this.v.X}, {this.v.Y}, {this.v.Z}] dir[{this.dir.X}, {this.dir.Y}, {this.dir.Z}]";
        }


    }
}
