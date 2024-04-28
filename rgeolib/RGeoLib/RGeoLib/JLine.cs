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
    public class JLine
    {
        // Serializable version of NLINE
        // use only for conversion into JSON
        [JsonPropertyName("start")] public Vec3d start { get; set; }
        [JsonPropertyName("end")] public Vec3d end { get; set; }

        public JLine()
        {
        }
        public JLine(Vec3d v1, Vec3d v2)
        {
            this.start = v1;
            this.end = v2;
        }

        public JLine(NLine inputLine)
        {
            this.start = inputLine.start;
            this.end = inputLine.end;
        }

        public NLine returnNLine()
        {
            NLine templine = new NLine(this.start, this.end);
            return templine;
        }

        public override string ToString()
        {
            return $"NLine[Start {this.start},End {this.end}]";
        }

        // Serialization JLINE
        public static string serializeJLine(JLine inputJLine)
        {
            string jsonString = JsonSerializer.Serialize(inputJLine);
            return jsonString;
        }
        public static JLine deserializeJLine(string jsonString)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                IncludeFields = true
            };

            JLine reverseNode = JsonSerializer.Deserialize<JLine>(jsonString, options);
            return reverseNode;
        }
    }
}
