using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGeoLib
{
    internal class RGeoFunctions
    {
        
        // http//paulbourke.net/geometry/circlesphere/
        public static Vec3d CalcCircleCentre2d(Vec3d p1, Vec3d p2, Vec3d p3)
        {
            Vec3d centroid = new Vec3d();

            double ma = (p2.Y - p1.Y) / (p2.X - p1.Y);
            double mb = (p3.Y - p2.Y) / (p3.X - p2.X);

            centroid.X = (ma * mb * (p1.Y - p3.Y) + mb * (p1.X + p2.X) - ma * (p2.X + p3.X)) / (2 * (mb - ma));
            centroid.Y = (-1 / ma) * (centroid.X - (p1.X + p2.X) / 2) + (p1.Y + p2.Y) / 2;

            return centroid;
        }

        //Is a point d inside, outside or on the same circle as a, b, c  
        //https://gamedev.stackexchange.com/questions/71328/how-can-i-add-and-subtract-convex-polygons
        //Returns positive if inside, negative if outside, and 0 if on the circle
        //Note ALL POINTS HAVE TO BE ON the 2d XY Plane
        public static double IsPointInsideOutsideOrOnCircle(Vec3d aVec, Vec3d bVec, Vec3d cVec, Vec3d dVec)
        {
            //This first part will simplify how we calculate the determinant
            double a = aVec.X - dVec.X;
            double d = bVec.X - dVec.X;
            double g = cVec.X - dVec.X;

            double b = aVec.Y - dVec.Y;
            double e = bVec.Y - dVec.Y;
            double h = cVec.Y - dVec.Y;

            double c = a * a + b * b;
            double f = d * d + e * e;
            double i = g * g + h * h;

            double determinant = (a * e * i) + (b * f * g) + (c * d * h) - (g * e * c) - (h * f * a) - (i * d * b);

            return determinant;
        }
    }
}
