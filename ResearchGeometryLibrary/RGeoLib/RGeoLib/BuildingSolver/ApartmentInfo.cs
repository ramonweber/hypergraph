using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RGeoLib.BuildingSolver
{
    public class ApartmentInfo
    {
        // use for serialziation
        [JsonPropertyName("database")] public string database { get; set; }
        [JsonPropertyName("id")] public string id { get; set; }
        [JsonPropertyName("country")] public string country { get; set; }
        [JsonPropertyName("city")] public string city { get; set; }
        [JsonPropertyName("name")] public string name { get; set; }
        [JsonPropertyName("area")] public double area { get; set; }
        [JsonPropertyName("bedrooms")] public int bedrooms { get; set; }
        [JsonPropertyName("bathrooms")] public int bathrooms { get; set; }
        [JsonPropertyName("bounds")] public string bounds { get; set; }
        [JsonPropertyName("facade")] public string facade { get; set; }
        [JsonPropertyName("circulation")] public string circulation { get; set; }
        [JsonPropertyName("split")] public string split { get; set; }

        public ApartmentInfo(Apartment inputApartment)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };

            this.database = inputApartment.database;
            this.id = inputApartment.id;
            this.country = inputApartment.country;
            this.city = inputApartment.city;
            this.name = inputApartment.name;
            this.area = inputApartment.rooms.Area;

            this.bedrooms = inputApartment.rooms.filterNMeshByProperty("bed").faceList.Count;
            this.bathrooms = inputApartment.rooms.filterNMeshByProperty("bath").faceList.Count;

            this.split = JsonSerializer.Serialize(inputApartment.splitNode);
            this.bounds = NFace.serializeNFace(inputApartment.bounds);
            this.facade = NLine.serializeNLineList(inputApartment.facade);
            this.circulation = NLine.serializeNLineList(inputApartment.circulation);
        }
    }
}
