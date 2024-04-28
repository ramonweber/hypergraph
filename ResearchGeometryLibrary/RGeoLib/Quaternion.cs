using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGeoLib
{
    public class Quaternion
    {
        // Implementation of Quaternions for rotation of objects in 3d space
        // https://danceswithcode.net/engineeringnotes/quaternions/quaternions.html

        // Properties
        public double Q0 { get; set; }
        public double Q1 { get; set; }
        public double Q2 { get; set; }
        public double Q3 { get; set; }

        // Constructors
        public Quaternion()
        {
            this.Q0 = 0;
            this.Q1 = 0;
            this.Q2 = 0;
            this.Q3 = 0;
        }

        public Quaternion(double q0, double q1, double q2, double q3)
        {
            this.Q0 = q0;
            this.Q1 = q1;
            this.Q2 = q2;
            this.Q3 = q3;
        }

        /// <summary>
        /// Creates a rotation quaternion from an angle (Degrees) axis (Vec) representation
        /// </summary>
        /// <param name="angleDegrees"></param>
        /// <param name="axis"></param>
        public Quaternion(double angleDegrees, Vec3d rotAxis)
        {
            Vec3d axis = rotAxis.GetNorm();

            double angleRad = RMath.ToRadians(angleDegrees);
            double halfAngle = 0.5 * angleRad;
            double sn = Math.Sin(halfAngle);

            this.Q0 = Math.Cos(halfAngle) ;
            this.Q1 = (axis.X * sn); // RMath.ToDegrees(
            this.Q2 = (axis.Y * sn);
            this.Q3 = (axis.Z * sn);
        }

        /// <summary>
        /// Creates a Quaternion from a rotation Matrix
        /// </summary>
        /// <param name="rotMatrix"></param>
        public Quaternion(Matrix3d rotMatrix)
        {
            // step 1 Magnitude of each quaternion component (sign +- undefined)
            double q0_uS = Math.Abs(Math.Sqrt((1 + rotMatrix.M00 + rotMatrix.M11 + rotMatrix.M22) / 4));
            double q1_uS = Math.Abs(Math.Sqrt((1 + rotMatrix.M00 - rotMatrix.M11 - rotMatrix.M22) / 4));
            double q2_uS = Math.Abs(Math.Sqrt((1 - rotMatrix.M00 + rotMatrix.M11 - rotMatrix.M22) / 4));
            double q3_uS = Math.Abs(Math.Sqrt((1 - rotMatrix.M00 - rotMatrix.M11 + rotMatrix.M22) / 4));

            // step 2 resolve the signs
            if (q0_uS > q1_uS && q0_uS > q2_uS && q0_uS > q3_uS)
            {
                // q0 is the largest
                this.Q0 = q0_uS;
                this.Q1 = (rotMatrix.M21 - rotMatrix.M12) / (4 * q0_uS);
                this.Q2 = (rotMatrix.M02 - rotMatrix.M20) / (4 * q0_uS);
                this.Q3 = (rotMatrix.M10 - rotMatrix.M01) / (4 * q0_uS);

            } else if (q1_uS > q0_uS && q1_uS > q2_uS && q1_uS > q3_uS)
            {
                // q1 is the largest
                this.Q0 = (rotMatrix.M21 - rotMatrix.M12) / (4 * q1_uS);
                this.Q1 = q1_uS;
                this.Q2 = (rotMatrix.M01 - rotMatrix.M10) / (4 * q1_uS);
                this.Q3 = (rotMatrix.M02 - rotMatrix.M20) / (4 * q1_uS);

            } else if (q2_uS > q0_uS && q2_uS > q1_uS && q2_uS > q3_uS)
            {
                // q2 is the largest
                this.Q0 = (rotMatrix.M02 - rotMatrix.M20) / (4 * q2_uS);
                this.Q1 = (rotMatrix.M01 - rotMatrix.M10) / (4 * q2_uS);
                this.Q2 = q2_uS;
                this.Q3 = (rotMatrix.M12 - rotMatrix.M21) / (4 * q2_uS);

            } else
            {
                // q3 is the largest
                this.Q0 = (rotMatrix.M10 - rotMatrix.M01) / (4 * q3_uS);
                this.Q1 = (rotMatrix.M02 - rotMatrix.M20) / (4 * q3_uS);
                this.Q2 = (rotMatrix.M12 - rotMatrix.M21) / (4 * q3_uS);
                this.Q3 = q3_uS;
            }
            
        }

        /*
        public Quaternion(double w, double v, double u)
        {
            // Input Degrees
            // Yaw(z)-Pitch(y)-Roll(x) rotating around the z, y and x axis
            // Right-handed coordinate system with right-handed rotations
            // w = yaw angle
            // v = pitch angle
            // u = roll angle

            this.Q0 = Math.Cos(u / 2) * Math.Cos(v / 2) * Math.Cos(w / 2) + Math.Sin(u / 2) * Math.Sin(v / 2) * Math.Cos(w / 2);
            this.Q1 = Math.Sin(u / 2) * Math.Cos(v / 2) * Math.Cos(w / 2) - Math.Cos(u / 2) * Math.Sin(v / 2) * Math.Sin(w / 2);
            this.Q2 = Math.Cos(u / 2) * Math.Sin(v / 2) * Math.Cos(w / 2) + Math.Sin(u / 2) * Math.Cos(v / 2) * Math.Sin(w / 2);
            this.Q3 = Math.Cos(u / 2) * Math.Cos(v / 2) * Math.Sin(w / 2) - Math.Sin(u / 2) * Math.Sin(v / 2) * Math.Cos(w / 2);

        }
        */
        // Method Public

        public double ReturnAngle()
        {
            double return_theta = 2 * Math.Acos(this.Q0);
            return return_theta;
        }
        public Vec3d ReturnAxis()
        {
            double return_theta = this.ReturnAngle();
            double x_return = (this.Q1 / (Math.Sin(return_theta / 2)));
            double y_return = (this.Q2 / (Math.Sin(return_theta / 2)));
            double z_return = (this.Q3 / (Math.Sin(return_theta / 2)));

            Vec3d return_axis = new Vec3d(x_return,y_return,z_return);
            return return_axis;
        }

        public Vec3d ReturnEulerAngles()
        {
            // Outputs vector with Yaw(z)-Pitch(y)-Roll(x) rotating around the z, y and x axis
            // Right-handed coordinate system with right-handed rotations
            // w = yaw angle
            // v = pitch angle
            // u = roll angle
            double q0 = this.Q0;
            double q1 = this.Q1;
            double q2 = this.Q2;
            double q3 = this.Q3;

            double u;
            double v;
            double w;

            v = Math.Asin(2 * (q0 * q2 + q1 * q3));
            // Gimball lock exception case (if yaw and roll are alligned and infinite solutions are possible)
            if (v == Math.PI/2)
            {
                u = 0;
                w = -2*Math.Atan2(q1, q0);
            }else if( v== -(Math.PI/2))
            {
                u = 0;
                w = 2 * Math.Atan2(q1, q0);
            }
            else
            {
                u = Math.Atan((2 * (q0 * q1 + q2 * q3) / (q0 * q0 - q2 * q2 - q2 * q2 + q3 * q3)));
                w = Math.Atan((2 * (q0 * q3 + q1 * q2) / (q0 * q0 + q2 * q2 - q2 * q2 - q3 * q3)));
            }

            Vec3d eulerVec = new Vec3d(u, v, w);
            return eulerVec; 
            
        }

        public static Quaternion Inverse(Quaternion q_inverse)
        {
            return new Quaternion(q_inverse.Q0, -q_inverse.Q1, -q_inverse.Q2, -q_inverse.Q3);
        }

        // Operator overrides
        public static Quaternion operator * (Quaternion r, Quaternion s)
        {
            // Attention!! Multiplication is associative but not commutative
            // (ab)c = a(bc)   BUT   ab != ba
            //
            // Quaternion Product
            // t = r*s 
            // (t0, t1, t2, t3) = (r0, r1, r2, r3) ✕ (s0, s1, s2, s3)

            double r0 = r.Q0;
            double r1 = r.Q1;
            double r2 = r.Q2;
            double r3 = r.Q3;

            double s0 = s.Q0;
            double s1 = s.Q1;
            double s2 = s.Q2;
            double s3 = s.Q3;


            double t0 = (r0*s0 - r1*s1 - r2*s2 - r3*s3);
            double t1 = (r0*s1 + r1*s0 - r2*s3 + r3*s2);
            double t2 = (r0*s2 + r1*s3 + r2*s0 - r3*s1);
            double t3 = (r0*s3 - r1*s2 + r2*s1 + r3*s0);

            return new Quaternion(t0, t1, t2, t3);
        }

        // Overrides
        public override string ToString()
        {
            return $"quaternion[{this.Q0}, {this.Q1}, {this.Q2}, {this.Q3}]";
        }

    }
}
