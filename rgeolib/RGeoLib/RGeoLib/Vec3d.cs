using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RGeoLib
{
    [Serializable]
    public class Vec3d
    {
        // Properties
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        // Indexer
        public double this[int i]
        {
            get
            {
                if (i == 0)
                {
                    return this.X;
                }
                else if (i == 1)
                {
                    return this.Y;
                }
                else if (i == 2)
                {
                    return this.Z;
                }
                else
                {
                    throw new Exception();
                }
            }
            set
            {
                if (i == 0)
                {
                    this.X = value;
                }
                else if (i == 1)
                {
                    this.Y = value;
                }
                else if (i == 2)
                {
                    this.Z = value;
                }
                else
                {
                    throw new Exception();
                }
            }
        }

        // Static properties
        public static Vec3d UnitX
        {
            get => new Vec3d(1, 0, 0);
        }
        public static Vec3d UnitY
        {
            get => new Vec3d(0, 1, 0);
        }
        public static Vec3d UnitZ
        {
            get => new Vec3d(0, 0, 1);
        }

        public static Vec3d Zero
        {
            get => new Vec3d(0, 0, 0);
        }

        public double Mag
        {
            get
            {
                return GetMag();
            }
        }

        // Constructors
        public Vec3d()
        {
            this.X = 0;
            this.Y = 0;
            this.Z = 0;
        }

        public Vec3d(Vec3d other)
        {
            this.X = other.X;
            this.Y = other.Y;
            this.Z = other.Z;
        }
        public Vec3d(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public Vec3d DeepCopy()
        {
            Vec3d deepCopyVec = new Vec3d(this.X, this.Y, this.Z);
            return deepCopyVec;
        }

        // Method Private

        private double GetMag()
        {
            // Vec3d function to return Magnitude of vector 
            double sq1 = this.X * this.X + this.Y * this.Y + this.Z * this.Z;
            double len = Math.Sqrt(sq1);
            return len;
        }

        // Method Public
        public void Abs()
        { 
            this.X = Math.Abs(this.X);
            this.Y = Math.Abs(this.Y);
            this.Z = Math.Abs(this.Z);
        }
        public void Add(Vec3d other)
        {
            this.X += other.X;
            this.Y += other.Y;
            this.Z += other.Z;
        }
        public bool Norm()
        {
            double len = this.Mag;
            if (len <= 0)
            {
                return false;
            }
            this.X /= len;
            this.Y /= len;
            this.Z /= len;

            return true;
        } 
        public Vec3d GetNorm()
        {
            double vx = this.X;
            double vy = this.Y;
            double vz = this.Z;
            Vec3d normVec = new Vec3d(vx, vy, vz); 
            double len = normVec.Mag;
            if (len > 0)
            {
                return new Vec3d(vx/len, vy / len, vz / len);
            }
            return normVec;
        }

        public Vec3d GetReverse()
        {
            double vx = -this.X;
            double vy = -this.Y;
            double vz = -this.Z;

            Vec3d reverseVec = new Vec3d(vx,vy,vz);

            return reverseVec;
        }
        public void Reverse()
        {
            this.X = -this.X;
            this.Y = -this.Y;
            this.Z = -this.Z;
        }
        public void Rotate2dZ(double radAngle)
        {
            // Rotates .this Vec3d around the z axis
            // Counterclockwise with the angle (in radians)

            double radCos = Math.Cos(radAngle);
            double radSin = Math.Sin(radAngle);
            double rot_X = this.X * radCos - this.Y * radSin;
            double rot_Y = this.X * radSin + this.Y * radCos;

            this.X = rot_X;
            this.Y = rot_Y;
        }
        public void Rotate2dPoint(Vec3d centerPoint, double radAngle)
        {
            // rotates .this point around a centerPoint with the angle radAngle
            //
            // Change implementation for multiple points put sin cos outside of main loop
            double radCos = Math.Cos(radAngle);
            double radSin = Math.Cos(radAngle);

            double x0 = this.X;
            double y0 = this.Y;
            double xc = centerPoint.X;
            double yc = centerPoint.Y;

            double rot_X = (x0 - xc)*radCos - (y0 - yc)*radSin + xc;
            double rot_Y = (x0 - xc)*radSin + (y0 - yc)*radCos + yc;
            
            this.X = rot_X;
            this.Y = rot_Y;
        }

        public void Round(int decimals)
        { 
            double tempX = Math.Round(this.X, decimals);
            double tempY = Math.Round(this.Y, decimals);
            double tempZ = Math.Round(this.Z, decimals);

            this.X = tempX;
            this.Y = tempY;
            this.Z = tempZ;
        }
        public void Round2()
        {
            double tempX = Math.Round(this.X, 2);
            double tempY = Math.Round(this.Y, 2);
            double tempZ = Math.Round(this.Z, 2);

            this.X = tempX;
            this.Y = tempY;
            this.Z = tempZ;
        }
        public void Scale(double factor)
        {
            this.X *= factor;
            this.Y *= factor;
            this.Z *= factor;
        }

       
        public void Subtract(Vec3d other)
        {
            this.X -= other.X;
            this.Y -= other.Y;
            this.Z -= other.Z;
        }

        public double GetSquareLength()
        {
            return this.X * this.X + this.Y * this.Y + this.Z * this.Z;
        }

        // Static methods

        public static double Angle(Vec3d v0, Vec3d v1)
        {
            // use Angle2PI_2d
            var d = v0.GetSquareLength() * v1.GetSquareLength();
            return d > 0.0 ? RMath.AcosSafe(Vec3d.DotProduct(v0, v1) / Math.Sqrt(d)) : 0.0;
        }
        public static double AngleDeg(Vec3d a, Vec3d b)
        {
            // returns: Angle in degrees of other vector with .this vector
            // 1rad × 180 / π

            double angle = Vec3d.Angle(a, b);

            double deg = angle * (180 / Math.PI);

            return deg;

        }

        public static double Angle2PI(Vec3d vec0, Vec3d vec1, Vec3d axis)
        {
            // use Angle2PI_2d
            // returns angle in full 0 - 2PI range. 
            // for 2d axis = Vec3d.UnitZ

            double angleRaw = Vec3d.Angle(vec0, vec1);

            double det = det3x3(vec0, vec1, axis);

            if (det < 0)
            {
                return Math.PI * 2 - angleRaw;
            }
            else
            {
                return angleRaw;
            }
        }
        public static double Angle2PI_2d(Vec3d vec0, Vec3d vec1)
        {
            // returns angle in full 0 - 2PI range. 

            Vec3d axis = new Vec3d(Vec3d.UnitZ);

            double angleRaw = Vec3d.Angle(vec0, vec1);

            double det = det3x3(vec0, vec1, axis);

            if (det < 0)
            {
                return Math.PI * 2 - angleRaw;
            }
            else
            {
                return angleRaw;
            }
        }
        public static double det3x3(Vec3d vec0, Vec3d vec1, Vec3d axis)
        {
            //     a b c
            // A = d e f
            //     g h i

            // convert to matrix form

            double a = vec0.X;
            double b = vec1.X;
            double c = axis.X;

            double d = vec0.Y;
            double e = vec1.Y;
            double f = axis.Y;

            double g = vec0.Z;
            double h = vec1.Z;
            double i = axis.Z;
            // |A| = a(ei − fh) − b(di − fg) + c(dh − eg)
            double det = a * (e * i - f * h) - b * (d * i - f * g) + c * (d * h - e * g);
            return det;
        }
        public static double AreaTri2d(Vec3d a, Vec3d b, Vec3d c)
        {
            // def area_triangle(self, corner_B, corner_C):
            //           
            //           B(x2,y2)
            //            / \
            //           / A \                                                
            //          /     \                                                
            //  Self(x3,y3)----C(x1,y1)                                                           
            //                                              
            // Step 0 initialize points

            double x1 = c.X;
            double y1 = c.Y;
            double x2 = b.X;
            double y2 = b.Y;
            double x3 = a.X;
            double y3 = a.Y;

            // Step 1 moving triangle point self to (0,0)
            x1 -= x3;
            y1 -= y3;
            x2 -= x3;
            y2 -= y3;

            // computes the determinant of 2x2 matrix (for 2d vectors)
            double det = Math.Abs(x1 * y2 - x2 * y1);
            double area = 0.5 * det;

            return area;
        }
        public static Vec3d Addition(Vec3d a, Vec3d b)
        {
            double newX = a.X + b.X;
            double newY = a.Y + b.Y;
            double newZ = a.Z + b.Z;

            Vec3d v = new Vec3d(newX, newY, newZ);
            return v;
        }

        public static double Distance(Vec3d a, Vec3d b)
        {
            // returns distance to between a and b

            double new_x = (a.X - b.X) * (a.X - b.X);
            double new_y = (a.Y - b.Y) * (a.Y - b.Y);
            double new_z = (a.Z - b.Z) * (a.Z - b.Z);
            return Math.Sqrt(new_x + new_y + new_z);
        }
        public static double DotProduct(Vec3d a, Vec3d b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }
        public static Vec3d CrossProduct(Vec3d a, Vec3d b)
        {
            double x = a.Y * b.Z - a.Z * b.Y;
            double y = a.Z * b.X - a.X * b.Z;
            double z = a.X * b.Y - a.Y * b.X;
            return new Vec3d(x, y, z);

        }
        
        public static Vec3d Mid(Vec3d a, Vec3d b)
        {
            Vec3d result = (a + b) / 2;
            return result;
        }
        public static Vec3d Between(Vec3d a, Vec3d b, double ratio)
        {
            
            if (ratio > 0.0)
            {
                if (ratio < 1.0)
                {
                    Vec3d between_vec = (b - a) * ratio + a;
                    return between_vec;
                }
                else
                {
                    return b;
                }
            }
            else
            {
                return a;
            }
               
        }

        public static bool isInbetweenAngle(Vec3d origin, Vec3d s, Vec3d e, Vec3d p)
        {

            //    s       e
            //     \     /
            //      \   /                 FALSE
            //       \ /
            //        o--------p
            //
            //
            //    s       e
            //     \  p  /
            //      \ | /                 True
            //       \|/
            //        o


            //
            bool isBetween = true;
            // denote vector to point p as op.

            //  Calculate cross product

            Vec3d os = s - origin;
            Vec3d oe = e - origin;
            Vec3d op = p - origin;
            Vec3d c_se = Vec3d.CrossProduct(os, oe);

            //If c_se>=0(angle in 0..180 range), then you have to check whether

            if (c_se.Z >= 0)
            {
                Vec3d c_sp = Vec3d.CrossProduct(os, op);
                Vec3d c_pe = Vec3d.CrossProduct(op, oe);

                if ((c_sp.Z >= 0) && (c_pe.Z >= 0))
                {
                    isBetween = false;
                }
            }
            else
            {
                // If c_se < 0(angle in 180..360 range), then you have to check whether

                Vec3d c_ep = Vec3d.CrossProduct(oe, op);
                Vec3d c_ps = Vec3d.CrossProduct(op, os);

                //NOT(cross(oe, op) >= 0 AND cross(op, os) >= 0)

                if ((c_ep.Z >= 0) && (c_ps.Z >= 0))
                {
                    isBetween = true;
                }
                else
                {
                    isBetween = false;
                }
            }

            return isBetween;
        }
        public static bool IsVecBetweenVecs(Vec3d a, Vec3d b, Vec3d c)
        {
            // is C between a and b;
            // https//stackoverflow.com/questions/11907947/how-to-check-if-a-point-lies-on-a-line-between-2-other-points
            double THRESHOLD = Constants.DocTolerance;

            double distanceAC = Vec3d.Distance(a, c);
            double distanceBC = Vec3d.Distance(b, c);
            double distanceAB = Vec3d.Distance(a, b);

            return (distanceAC + distanceBC - distanceAB < THRESHOLD);

        }
        public static bool IsCollinear(Vec3d a, Vec3d b, Vec3d c)
        {
            // Check if three points are collinear in 3D space
            // This can be done by checking if the cross product of AB and BC is zero (or very close to zero)
            Vec3d AB = new Vec3d(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
            Vec3d BC = new Vec3d(c.X - b.X, c.Y - b.Y, c.Z - b.Z);
            Vec3d crossProduct = Vec3d.CrossProduct(AB, BC);

            return IsAlmostZero(crossProduct);
        }
        public static bool IsAlmostZero(Vec3d vector)
        {
            double tolerance = 0.00001;  // A small tolerance value, e.g., 0.000001
            return vector.GetSquareLength() < tolerance * tolerance;
        }

        public static bool IsParallelTo(Vec3d vec1, Vec3d vec2, double tolerance)
        {
            // Compute the dot product of the two direction vectors
            double dotProduct = Vec3d.DotProduct(vec1, vec2);
            // Compute the magnitudes of the two direction vectors
            double mag1 = vec1.Mag;
            double mag2 = vec2.Mag;

            // Check if the direction vectors are parallel within the specified tolerance
            if (Math.Abs(dotProduct - mag1 * mag2) <= tolerance)
            {
                return true;
            }
            return false;
        }
        

        public static Vec3d ScaleToLen(Vec3d a, double len)
        {
            Vec3d b = a.GetNorm();
            b.Scale(len);
            return b;
        }

        public static double DistanceToSegment(Vec3d v, Vec3d a, Vec3d b)
        {
            Vec3d ab = b - a;
            Vec3d av = v - a;

            if (Vec3d.DotProduct(av, ab) <= 0.0)           // Point is lagging behind start of the segment, so perpendicular distance is not viable.
                return av.Mag;          // Use distance to start of segment instead.

            Vec3d bv = v - b;

            if (Vec3d.DotProduct(bv, ab) >= 0.0)           // Point is advanced past the end of the segment, so perpendicular distance is not viable.
                return bv.Mag;          // Use distance to end of the segment instead.

            return (Vec3d.CrossProduct(ab, av)).Mag / ab.Mag;       // Perpendicular distance of point to segment.
        }

        /// <summary>
        /// Rotates a point (vec to rotate) around an axis (axis) with angle theta (Degrees)
        /// </summary>
        /// <param name="vecToRotate"></param>
        /// <param name="axis"></param>
        /// <param name="angleDegrees"></param>
        /// <returns></returns>
        
        public static Vec3d RotateAroundAxis(Vec3d vecToRotate, Vec3d axis, double angleDegrees)
        {
            /*
             * Test application for console 
            Vec3d vec1 = new Vec3d(4, 3, 2);
            Vec3d axis2 = new Vec3d(0, 1, 0);
            double angle = 45;
            Vec3d vec2 = Vec3d.RotateAroundAxis(vec1, axis2, angle);
            Console.WriteLine(vec2);
            Console.WriteLine("Debug");
            Console.ReadKey();
            */ 

            // Step 1:  Convert the point to be roated into a quaternion by assigning the points
            //          coordinates as the quaternions imaginary components, and setting the quaternions real component to zero

            //double angleDegrees = RMath.ToDegrees(theta);
            Quaternion qRot = new Quaternion(angleDegrees, axis);

            Quaternion q_inv = Quaternion.Inverse(qRot);

            // Step 2:  Perform the rotation
            Quaternion p = new Quaternion(0, vecToRotate.X, vecToRotate.Y, vecToRotate.Z);
            Quaternion pL = q_inv * p * qRot;

            // Step3: Extract the rotated coordinates from pL
            Vec3d pNew = new Vec3d(pL.Q1, pL.Q2, pL.Q3); 
            return pNew;
        }
        public static Vec3d RotateAroundAxisRad(Vec3d vecToRotate, Vec3d axis, double angleRadians)
        {
            /*
             * Test application for console:
            Vec3d vecA = new Vec3d(4, 3, 2);
            Vec3d axisA = new Vec3d(0, 1, 0);
            double angleA = Math.PI / 4;
            Vec3d vecB = Vec3d.RotateAroundAxisRad(vecA, axisA, angleA);
            Console.WriteLine(vecB);
            Console.ReadKey();
            */

            double angleDegrees = RMath.ToDegrees(angleRadians);
            Vec3d resVec = RotateAroundAxis(vecToRotate, axis, angleDegrees);
            return resVec;
        }
        public static Vec3d ScalarProjection(Vec3d a, Vec3d b)
        {
            // Scalar Projection of a onto b
            // 
            // projab = ( b/|b| )  * ( (a*b)/|b| )    //Note * dot product | = length of vector

            double dot = a * b;
            double lenB = b.Mag;
            Vec3d b_p = new Vec3d(b);

            Vec3d projAonB = new Vec3d((b_p / lenB) * (dot / lenB));
            return projAonB;
        }

        public static Vec3d tripleProd(Vec3d A, Vec3d B, Vec3d C)
        { 
            Vec3d temp1 = Vec3d.CrossProduct(A, B);
            Vec3d temp2 = Vec3d.CrossProduct(temp1, C);
            return temp2;
        }
        public static int OrientationXY(Vec3d p1, Vec3d p2, Vec3d p)
        {
            // Orientatition 2d 
            // Determinant
            double Orin = (p2.X - p1.X) * (p.Y - p1.Y) - (p.X - p1.X) * (p2.Y - p1.Y);

            if (Orin > 0)
                return -1; //   (* Orientation is to the left-hand side  *)
            if (Orin < 0)
                return 1; //    (* Orientation is to the right-hand side *)

            return 0; //        (* Orientation is neutral aka collinear  *)
        }

        // slow comparison functions
        public static List<Vec3d> ITER_RemoveDuplicatesWithTolerance(List<Vec3d> points, double tolerance)
        {
            // This list will hold the unique points
            List<Vec3d> uniquePoints = new List<Vec3d>();

            foreach (var point in points)
            {
                // Check if the point is 'close' to any of the points already in uniquePoints
                bool isDuplicate = uniquePoints.Any(up => Vec3d.Distance(point, up) <= tolerance);

                if (!isDuplicate)
                {
                    uniquePoints.Add(point);
                }
            }

            return uniquePoints;
        }

        public static List<Vec3d> ITER_CullAllDuplicatesWithTolerance(List<Vec3d> points, double tolerance)
        {
            for (int i = 0; i < points.Count; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    if (Vec3d.Distance(points[i], points[j]) <= tolerance)
                    {
                        points.RemoveAt(j);
                        j--;
                    }
                }
            }
            return points;
        }

        // fast comparison functions ... but unclear if they actually work? especially for large tolerances.
        public static List<Vec3d> RemoveDuplicatePoints(List<Vec3d> points, double tolerance)
        {
            HashSet<Vec3d> pointSet = new HashSet<Vec3d>(new Vec3dEqualityComparer(tolerance));
            List<Vec3d> uniquePoints = new List<Vec3d>();

            foreach (Vec3d point in points)
            {
                if (!pointSet.Contains(point))
                {
                    pointSet.Add(point);
                    uniquePoints.Add(point);
                }
            }

            return uniquePoints;
        }
        public static List<Vec3d> deleteDuplicatePointsRKHT(List<Vec3d> inputPoints)
        {
            // deprecated, use remove duplicatePoints

            double dupTol = 0.001;
            // Define the output list and the Rabin-Karp hash table
            List<Vec3d> outPoints = new List<Vec3d>();
            RabinKarpHashTableVec<Vec3d> hashTable = new RabinKarpHashTableVec<Vec3d>(dupTol);
            foreach (Vec3d point in inputPoints)
            {
                // Check if the point is not a duplicate
                if (!hashTable.Contains(point))
                {
                    // Add the point to the output list and the hash table
                    outPoints.Add(point);
                    hashTable.Add(point);
                }
            }
            // Return the output points
            return outPoints;
        }

        public static Vec3d averageVectorList(List<Vec3d> inputVectors)
        {
            Vec3d averageVec = Vec3d.Zero;
            int count = inputVectors.Count;

            if (count > 0)
            {
                double sumX = 0;
                double sumY = 0;
                double sumZ = 0;

                for (int i = 0; i < count; i++)
                {
                    sumX += inputVectors[i].X;
                    sumY += inputVectors[i].Y;
                    sumZ += inputVectors[i].Z;
                }

                averageVec.X = sumX / count;
                averageVec.Y = sumY / count;
                averageVec.Z = sumZ / count;
            }

            return averageVec;
        }

        // Operator overrides
        public static Vec3d operator + (Vec3d a, Vec3d b)
        {
            return new Vec3d(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }
        public static Vec3d operator - (Vec3d a, Vec3d b)
        {
            return new Vec3d(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }
        public static double operator * (Vec3d a, Vec3d b)
        {
            return Vec3d.DotProduct(a, b);
        }
        public static Vec3d operator * (double a, Vec3d b)
        {
            Vec3d v = new Vec3d(b);
            v.Scale(a);
            return v;
        }
        public static Vec3d operator * (Vec3d a, double b)
        {
            Vec3d v = new Vec3d(a);
            v.Scale(b);
            return v;
        }
        public static Vec3d operator / (Vec3d a, double b)
        {
            return new Vec3d(a.X/b, a.Y/b, a.Z/b);
        }


        public static bool operator == (Vec3d a, Vec3d b)
        {
            return (a.X == b.X && a.Y == b.Y && a.Z == b.Z);
        }
        public static bool operator != (Vec3d a, Vec3d b)
        {
            return (a.X != b.X || a.Y != b.Y || a.Z != b.Z);
        }

        // Overrides

        public static string serializeVec(Vec3d inputVec)
        {
            string jsonString = JsonSerializer.Serialize(inputVec);
            return jsonString;
        }
        public static Vec3d deserializeVec(string jsonString)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                IncludeFields = true
            };

            Vec3d reverseNode = JsonSerializer.Deserialize<Vec3d>(jsonString, options);
            return reverseNode;
        }

        public static string serializeVecList(List<Vec3d> inputVecs)
        {
            JFace inputJFace = new JFace(inputVecs);
            string jsonString = JsonSerializer.Serialize(inputJFace);
            return jsonString;
        }
        public static List<Vec3d> deserializeVecList(string jsonString)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                IncludeFields = true
            };

            JFace reverseNode = JsonSerializer.Deserialize<JFace>(jsonString, options);
            List<Vec3d> outVecList = reverseNode.returnVecList();
            return outVecList;
        }
        public override string ToString()
        {
            return $"vec3d[{this.X}, {this.Y}, {this.Z}]";
        }
        public override bool Equals(object obj)
        {
            return this == (Vec3d)obj;
        }
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = (int)2166136261;
                // Suitable nullity checks etc, of course :)
                hash = (hash * 16777619) ^ X.GetHashCode();
                hash = (hash * 16777619) ^ Y.GetHashCode();
                hash = (hash * 16777619) ^ Z.GetHashCode();
                return hash;
            }
        }

    }

    class Vec3dEqualityComparer : IEqualityComparer<Vec3d>
    {
        private double _tolerance;

        public Vec3dEqualityComparer(double tolerance)
        {
            _tolerance = tolerance;
        }

        public bool Equals(Vec3d x, Vec3d y)
        {
            return Vec3d.Distance(x, y) <= _tolerance;
        }

        public int GetHashCode(Vec3d obj)
        {
            return obj.GetHashCode();
        }
    }
    // Use this to compare distances! 
    public class Vec3dTupleEqualityComparer : IEqualityComparer<Tuple<Vec3d, Vec3d>>
    {
        private double _tolerance;

        public Vec3dTupleEqualityComparer(double tolerance)
        {
            _tolerance = tolerance;
        }

        public bool Equals(Tuple<Vec3d, Vec3d> x, Tuple<Vec3d, Vec3d> y)
        {

            double dist1 = Vec3d.Distance(x.Item1, y.Item1);
            double dist2 = Vec3d.Distance(x.Item2, y.Item2);
            double dist3 = Vec3d.Distance(x.Item1, y.Item2);
            double dist4 = Vec3d.Distance(x.Item2, y.Item1);

            return dist1 <= _tolerance && dist2 <= _tolerance || dist3 <= _tolerance && dist4 <= _tolerance;
        }

        public int GetHashCode(Tuple<Vec3d, Vec3d> obj)
        {
            return obj.Item1.GetHashCode() ^ obj.Item2.GetHashCode();
        }
    }
}
