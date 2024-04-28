using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json;
using System.Text.Json.Serialization;


namespace RGeoLib.BuildingSolver
{
    public class BuildingResult
    {
        // Properties of DataNode
        [JsonPropertyName("area")] public double area { get; set; }
        [JsonPropertyName("numApts")] public int numApts { get; set; }
        [JsonPropertyName("sizeApts")] public List<double> sizeApts { get; set; }
        [JsonPropertyName("conApts")] public List<double> conApts { get; set; }

        public BuildingResult()
        {

        }

        public BuildingResult(Building inputBuilding)
        {
            
            List<double> sizes = new List<double>();

            for (int i = 0; i<inputBuilding.apartmentBounds.faceList.Count; i++)
            {
                sizes.Add(inputBuilding.apartmentBounds.faceList[i].Area);
            }

            this.sizeApts = sizes;
            this.numApts = inputBuilding.apartmentBounds.faceList.Count;
            this.area = inputBuilding.bounds.Area;
            this.conApts = RComp.convexityScore(inputBuilding.apartmentBounds);
        }


        //JSON conversion
        public static string serialize(BuildingResult inputResult)
        {
            var serializeOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            string jsonString = JsonSerializer.Serialize(inputResult, serializeOptions);
            return jsonString;
        }
        public static BuildingResult deserialize(string jsonString)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                IncludeFields = true
            };

            BuildingResult reverseNode = JsonSerializer.Deserialize<BuildingResult>(jsonString, options);
            return reverseNode;
        }
    }
}
