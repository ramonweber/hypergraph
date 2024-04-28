using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGeoLib
{
    public class NEdge
    {
        // Vertex this edge starts at
        public Vec3d v;

        //public int id { get; set; }

        public bool isIntersecting = false;

        // public edge properties
        public string property { get; set; }

        public NEdge(Vec3d v)
        {
            this.v = v;
        }
        public NEdge DeepCopy()
        {
            NEdge deepCopyEdge = new NEdge(this.v.DeepCopy());
            deepCopyEdge.property = this.property;
            return deepCopyEdge;
        }
        public Vec3d Direction
        {
            get
            {
                Vec3d dir = new Vec3d(nextNEdge.v - v);
                return dir;
            }
        }

        public double Length
        {
            get
            {
                Vec3d dir = new Vec3d(nextNEdge.v - v);
                return dir.Mag;
            }
        }

        // Face associated with this nEdge
        public NFace face; 

        // Prev and next edge
        public NEdge prevNEdge;
        public NEdge nextNEdge;

        // temp end
        public Vec3d endv;

        // axis
        public Axis axis;

        //opposite edge
        public NEdge oppositeNEdge;

        public static bool IsNEdgeEqual(NEdge x, NEdge y)
        {
            x.SetTempEnd();
            y.SetTempEnd();
            // Compares two edges and returns true if they are the same
            bool adjacentBool = false;
            double tolDist = Constants.IntersectTolerance;

            double distEndEnd = Vec3d.Distance(x.endv, y.endv);
            double distStartStart = Vec3d.Distance(x.v, y.v);

            double distStartEnd = Vec3d.Distance(x.v, y.endv);
            double distEndStart = Vec3d.Distance(x.endv, y.v);

            if (((distEndEnd < tolDist) && (distStartStart < tolDist)) || ((distStartEnd < tolDist) && (distEndStart < tolDist)))
            {
                adjacentBool = true;
            }

            return adjacentBool;
        }

        public static List<NEdge> subdivideNEdgeByRatio(NEdge inputNEdge, double ratio)
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

            List<NEdge> newEdges = new List<NEdge>();

            newEdges.Add(inputNEdge);
            newEdges.Add(insertEdge);

            return newEdges;
        }
        public static List<NEdge> subdivideNEdgeByRatioList(NEdge inputNEdge, List<double> ratios)
        {
            List<NEdge> outEdges = new List<NEdge>() { inputNEdge };

            NEdge currentEdge = inputNEdge;
            for (int i = 0; i < ratios.Count; i++)
            {
                List<NEdge> currentSplit = NEdge.subdivideNEdgeByRatio(currentEdge, ratios[i]);
                currentEdge = currentSplit[0];
                outEdges.Add(currentSplit[1]);
            }

            return outEdges;
        }
        public void Round(int decimals)
        {
            this.v.Round(decimals);
        }
        
        public void RotateNEdgeAroundAxis(Vec3d axis, double angleDegrees)
        {
            Vec3d tempVec = Vec3d.RotateAroundAxis(this.v, axis, angleDegrees);
            this.v = tempVec;
        }
        public void RotateNEdgeAroundAxisRad(Vec3d axis, double angleRad)
        {
            Vec3d tempVec = Vec3d.RotateAroundAxisRad(this.v, axis, angleRad);
            this.v = tempVec;
        }

        public void TranslateNEdge(Vec3d moveVec)
        {
            this.v += moveVec;
        }
        //public void RotateNEdgeAround2dPoint(Vec3d centerPoint, double radAngle)
        //{
        //    this.v.Rotate2dPoint(centerPoint, radAngle);
        //}
        
        public void SetTempEnd()
        {
            Vec3d endVec = new Vec3d(nextNEdge.v);
            endv = endVec;
        }
        public void SnapToAxis2D()
        {
            // make sure edge connectivity is set in mesh

            Vec3d moveToV = RIntersection.AxisClosestPoint2D(this.v, this.axis);
            this.v = moveToV;

            Vec3d moveToN = RIntersection.AxisClosestPoint2D(this.nextNEdge.v, this.axis);
            this.nextNEdge.v = moveToN;
        }

        public override string ToString()
        {
            return $"NEdge V:[{this.v.X}, {this.v.Y}, {this.v.Z}]";
        }

    }
}
