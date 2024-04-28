using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace RGeoLib
{

    [Serializable]
    public class JMesh
    {
        // Serializable version of NMesh
        // use only for conversion into JSON

        [JsonPropertyName("faceList")] public List<JFace> faceList { get; set; }
        //[JsonPropertyName("adjacencyMatrix")] public Matrix<double> adjacencyMatrix { get; set; }

        [JsonPropertyName("adjacencyMatrix")]
        public string AdjacencyMatrixJson
        {
            get
            {
                // Serialize the adjacency matrix using the custom method
                return JMatrix.SerializeMatrix(adjacencyMatrix);
                
            }
            set
            {
                // Deserialize the JSON string and set the adjacency matrix
                adjacencyMatrix = JMatrix.DeserializeMatrix(value);
            }
        }

        [JsonIgnore]
        public Matrix<double> adjacencyMatrix { get; set; }

        public JMesh() { }

        public JMesh(NMesh inputMesh)
        {
            List<JFace> tempJFaceList = new List<JFace>();
            for (int i = 0;i<inputMesh.faceList.Count;i++)
            {
                JFace tempJFace = new JFace(inputMesh.faceList[i]);
                tempJFaceList.Add(tempJFace);
            }
            this.faceList = tempJFaceList;
            this.adjacencyMatrix = inputMesh.adjacencyMatrix;
        }

        public NMesh returnNMesh()
        {
            List<NFace> tempNFaceList = new List<NFace>();
            for (int i = 0; i < this.faceList.Count; i++)
            {
                NFace tempNFace = this.faceList[i].returnNFace();
                tempNFaceList.Add(tempNFace);
            }
            NMesh convertedMesh = new NMesh(tempNFaceList);
            convertedMesh.adjacencyMatrix = this.adjacencyMatrix;
            return convertedMesh;
        }

        public static string serializeJMesh(JMesh inputJMesh)
        {
            string jsonString = JsonSerializer.Serialize(inputJMesh);
            return jsonString;
        }
        public static JMesh deserializeJMesh(string jsonString)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                IncludeFields = true
            };

            JMesh reverseNode = JsonSerializer.Deserialize<JMesh>(jsonString, options);
            return reverseNode;
        }
    }
}
