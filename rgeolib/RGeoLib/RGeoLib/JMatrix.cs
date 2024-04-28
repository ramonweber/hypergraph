using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RGeoLib
{
    public class JMatrix
    {

        public static string SerializeMatrix(Matrix<double> adjacencyMatrix)
        {
            // Convert the Matrix<double> to a two-dimensional array of doubles
            double[,] matrixData = new double[adjacencyMatrix.RowCount, adjacencyMatrix.ColumnCount];
            for (int i = 0; i < adjacencyMatrix.RowCount; i++)
            {
                for (int j = 0; j < adjacencyMatrix.ColumnCount; j++)
                {
                    matrixData[i, j] = adjacencyMatrix[i, j];
                }
            }

            // Serialize the two-dimensional array to JSON
            JsonArray jsonArray = new JsonArray();
            for (int i = 0; i < adjacencyMatrix.RowCount; i++)
            {
                JsonArray rowArray = new JsonArray();
                for (int j = 0; j < adjacencyMatrix.ColumnCount; j++)
                {
                    rowArray.Add(matrixData[i, j]);
                }
                jsonArray.Add(rowArray);
            }

            return jsonArray.ToString();
        }


        public static Matrix<double> DeserializeMatrix(string jsonString)
        {
            // Deserialize the JSON string to a JsonNode
            JsonNode jsonArray = JsonNode.Parse(jsonString);

            // Determine the dimensions of the matrix
            JsonArray tempArray = jsonArray.AsArray();
            JsonArray tempArrayRow = jsonArray[0].AsArray();

            int rowCount = tempArrayRow.Count;
            int columnCount = tempArray.Count;


            Matrix<double> newMatrix = Matrix<double>.Build.Dense(rowCount, columnCount);


            // Fill the matrix with data from the JsonArray
            for (int i = 0; i < rowCount; i++)
            {
                JsonArray rowArray = jsonArray[i].AsArray();
                for (int j = 0; j < columnCount; j++)
                {
                    newMatrix[i, j] = (double)rowArray[j];
                }
            }

            return newMatrix;

        }

    }
}
