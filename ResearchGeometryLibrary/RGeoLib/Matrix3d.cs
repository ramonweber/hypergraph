using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGeoLib
{
    public class Matrix3d
    {   
        // 
        // Properties
        // Creates a 3x3 Matrix
        // Entries are referred to as row and column coordinates M00-M22
        // Entries can be reffered to via indexer 0 to 8 where M00=0 M01=1 M02=3 M10=4 ....  

        /// <summary>
        /// Entry at row 0 column 0
        /// </summary>
        public double M00 { get; set; }
        /// <summary>
        /// Entry at row 0 column 1
        /// </summary>
        public double M01 { get; set; }
        /// <summary>
        /// Entry at row 0 column 2
        /// </summary>
        public double M02 { get; set; }
        /// <summary>
        /// Entry at row 1 column 0
        /// </summary>
        public double M10 { get; set; }
        /// <summary>
        /// Entry at row 1 column 1
        /// </summary>
        public double M11 { get; set; }
        /// <summary>
        /// Entry at row 1 column 2
        /// </summary>
        public double M12 { get; set; }
        /// <summary>
        /// Entry at row 2 column 0
        /// </summary>
        public double M20 { get; set; }
        /// <summary>
        /// Entry at row 2 column 1
        /// </summary>
        public double M21 { get; set; }
        /// <summary>
        /// Entry at row 2 column 2
        /// </summary>
        public double M22 { get; set; }

        // Indexer
        public double this[int i]
        {
            get
            {
                if (i == 0)
                {
                    return this.M00;
                }
                else if (i == 1)
                {
                    return this.M01;
                }
                else if (i == 2)
                {
                    return this.M02;
                }
                else if (i == 3)
                {
                    return this.M10;
                }
                else if (i == 4)
                {
                    return this.M11;
                }
                else if (i == 5)
                {
                    return this.M12;
                }
                else if (i == 6)
                {
                    return this.M20;
                }
                else if (i == 7)
                {
                    return this.M21;
                }
                else if (i == 8)
                {
                    return this.M22;
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
                    this.M00 = value;
                }
                else if (i == 1)
                {
                    this.M01 = value;
                }
                else if (i == 2)
                {
                    this.M02 = value;
                }
                else if (i == 3)
                {
                    this.M10 = value;
                }
                else if (i == 4)
                {
                    this.M11 = value;
                }
                else if (i == 5)
                {
                    this.M12 = value;
                }
                else if (i == 6)
                {
                    this.M20 = value;
                }
                else if (i == 7)
                {
                    this.M21 = value;
                }
                else if (i == 8)
                {
                    this.M22 = value;
                }
                else
                {
                    throw new Exception();
                }
            }
        }

        // Constructors
        /// <summary>
        /// Create a 3x3 Matrix from Columns 
        /// </summary>
        /// <param name="column0"></param>
        /// <param name="column1"></param>
        /// <param name="column2"></param>
        public Matrix3d(Vec3d column0, Vec3d column1, Vec3d column2)
        {
            this.M00 = column0.X;
            this.M01 = column1.X;
            this.M02 = column2.X;

            this.M10 = column0.Y;
            this.M11 = column1.Y;
            this.M12 = column2.Y;

            this.M20 = column0.Z;
            this.M21 = column1.Z;
            this.M22 = column2.Z;
        }

        /// <summary>
        /// Create a 3x3 Matrix from individual entries
        /// </summary>
        /// <param name="m00"></param>
        /// <param name="m01"></param>
        /// <param name="m02"></param>
        /// <param name="m10"></param>
        /// <param name="m11"></param>
        /// <param name="m12"></param>
        /// <param name="m20"></param>
        /// <param name="m21"></param>
        /// <param name="m22"></param>
        public Matrix3d(double m00, double m01, double m02, double m10, double m11, double m12, double m20, double m21, double m22)
        {
            this.M00 = m00;
            this.M01 = m01;
            this.M02 = m02;
            this.M10 = m10;
            this.M11 = m11;
            this.M12 = m12;
            this.M20 = m20;
            this.M21 = m21;
            this.M22 = m22;
        }

        /// <summary>
        /// Creates a 3x3 rotation matrix from a quaternion q
        /// </summary>
        /// <param name="q"></param>
        public Matrix3d(Quaternion q)
        {
            
            double q0 = q.Q0;
            double q1 = q.Q1;
            double q2 = q.Q2;
            double q3 = q.Q3;   

            this.M00 = q0 * q0 + q1 * q1 - q2 * q2 - q3 * q3;
            this.M01 = 2 * q1 * q2 - 2 * q0 * q3;
            this.M02 = 2 * q1 * q3 + 2 * q0 * q2;
            this.M10 = 2 * q1 * q2 + 2 * q0 * q3;
            this.M11 = q0 * q0 - q1 * q1 + q2 * q2 - q3 * q3;
            this.M12 = 2 * q2 * q3 - 2 * q0 * q1;
            this.M20 = 2 * q1 * q3 - 2 * q0 * q2;
            this.M21 = 2 * q2 * q3 + 2 * q0 * q1;
            this.M22 = q0 * q0 - q1 * q1 - q2 * q2 + q3 * q3;
        }

        public override string ToString()
        {
            return $"[{this.M00}, {this.M01}, {this.M02} \r\n {this.M10}, {this.M11}, {this.M12}  \r\n {this.M20}, {this.M21}, {this.M22}]";
        }
    }
}
