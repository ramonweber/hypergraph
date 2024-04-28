using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RGeoLib
{
    [Serializable]
    public class JFace
    {
        // Serializable version of NFace
        // use only for conversion into JSON

        [JsonPropertyName("corners")] public List<Vec3d> corners { get; set; }

        [JsonPropertyName("id")] public string id { get; set; }

        public JFace() { }

        public JFace(List<Vec3d> inputCorners) 
        {
            this.corners = inputCorners;
        }

        public JFace(NFace inputFace)
        { 
            List<Vec3d> cornerList= new List<Vec3d>();

            for (int i=0;i< inputFace.edgeList.Count;i++) 
            {
                cornerList.Add(inputFace.edgeList[i].v);
            }
            this.corners = cornerList;
            this.id = inputFace.merge_id;
        }

        public NFace returnNFace()
        {
            NFace tempFace = new NFace(this.corners);
            tempFace.merge_id = this.id;
            return tempFace;
        }

        public List<Vec3d> returnVecList()
        {
            return this.corners;
        }

        public static string serializeJFace(JFace inputJFace)
        {
            string jsonString = JsonSerializer.Serialize(inputJFace);
            return jsonString;
        }
        public static JFace deserializeJFace(string jsonString)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                IncludeFields = true
            };

            JFace reverseNode = JsonSerializer.Deserialize<JFace>(jsonString, options);
            return reverseNode;
        }
    }
}
