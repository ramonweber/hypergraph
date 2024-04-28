using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
namespace RGeoLib
{
    public class RMath
    {
        /// <summary>
        /// Converts Radians to Degree
        /// </summary>
        /// <param name="radians"></param>
        /// <returns></returns>
        public static double ToDegrees(double radians)
        {
            const double toDeg = 180.0 / Math.PI;
            return radians * toDeg;
        }

        /// <summary>
        /// Converts Degree to Radians
        /// </summary>
        /// <param name="radians"></param>
        /// <returns></returns>
        public static double ToRadians(double degrees)
        {
            const double toRad = Math.PI / 180.0;
            return degrees * toRad;
        }

        //Clamp list indices
        public static int ClampListIndex(int index, int listSize)
        {
            index = ((index % listSize) + listSize) % listSize;

            return index;
        }

        public static double angle_1to360(double angle)
        {
            double angleNew = ((int)angle % 360) + (angle - Math.Truncate(angle));
            if (angle > 0.0)
                return angle;
            else
                return angle + 360.0;
        }
                    
        public static double Clamp(double t, double min, double max)
        {
            return (t < min) ? min : (t > max) ? max : t;
        }

        public static double AcosSafe(double d)
        {
            return Math.Acos(Clamp(d, -1.0, 1.0));
        }

        public static List<double> ratioToArea(List<double> ratioList, double areaGlobal)
        {
            List<double> areaList = new List<double>();
            for (int i = 0; i < ratioList.Count; i++)
            {
                double tempRatioArea = ratioList[i] * areaGlobal;
                areaList.Add(tempRatioArea);
            }
            return areaList;
        }

        // Matrix Helpers
        public static Matrix<double> CalculateLaplacian(Matrix<double> connectivityMatrix)
        {
            int n = connectivityMatrix.RowCount;
            var degreeMatrix = Matrix<double>.Build.DenseIdentity(n, n);

            for (int i = 0; i < n; i++)
            {
                double degree = connectivityMatrix.Row(i).Sum();
                degreeMatrix[i, i] = degree;
            }

            var laplacianMatrix = degreeMatrix - connectivityMatrix;

            return laplacianMatrix;
        }
        public static Matrix<double> CreateDegreeMatrix(Matrix<double> connectivityMatrix)
        {
            int n = connectivityMatrix.RowCount;
            var degreeMatrix = Matrix<double>.Build.DenseIdentity(n, n);

            for (int i = 0; i < n; i++)
            {
                double degree = connectivityMatrix.Row(i).Sum();
                degreeMatrix[i, i] = degree;
            }

            return degreeMatrix;
        }
        
        // Helper function to convert jagged array to rectangular array
        public static double[,] JaggedToRectangular(double[][] jaggedArray)
        {
            int rowCount = jaggedArray.Length;
            int colCount = jaggedArray[0].Length;
            double[,] rectangularArray = new double[rowCount, colCount];

            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    rectangularArray[i, j] = jaggedArray[i][j];
                }
            }

            return rectangularArray;
        }
        public static double[] MatrixToColumnWiseArray(Matrix<double> matrix)
        {
            int numRows = matrix.RowCount;
            int numCols = matrix.ColumnCount;
            double[] array = new double[numRows * numCols];

            int index = 0;
            for (int col = 0; col < numCols; col++)
            {
                for (int row = 0; row < numRows; row++)
                {
                    array[index++] = matrix[row, col];
                }
            }

            return array;
        }
        public static bool CheckMatrixCondition(Matrix<double> matrixA, Matrix<double> matrixB)
        {
            for (int i = 0; i < matrixA.RowCount; i++)
            {
                for (int j = 0; j < matrixA.ColumnCount; j++)
                {
                    if (matrixA[i, j] == 1 && matrixB[i, j] == 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
