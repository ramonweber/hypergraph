using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json;
using System.Text.Json.Serialization;
using Clipper2Lib;

namespace RGeoLib.BuildingSolver
{
    public class ApartmentResult
    {
        // Properties of DataNode

        // REFERENCE PROPERTIES
        [JsonPropertyName("database")] public string database { get; set; }
        [JsonPropertyName("id")] public string id { get; set; } // id of reference building
        [JsonPropertyName("country")] public string country { get; set; }
        [JsonPropertyName("city")] public string city { get; set; }
        [JsonPropertyName("name")] public string name { get; set; } // 

        // CURRENT NODE PROPERTIES
        [JsonPropertyName("building")] public string building { get; set; } // identifier of current building of reference apt
        [JsonPropertyName("apartmentNum")] public int apartmentNum { get; set; } // identifier of current apartment of reference apt

        [JsonPropertyName("currentBldg")] public string currentBldg { get; set; } // identifier of current apartment of reference apt
        [JsonPropertyName("currentApt")] public int currentApt { get; set; } // identifier of current apartment of reference apt

        [JsonPropertyName("area")] public double area { get; set; }

        [JsonPropertyName("daylitArea")] public double daylitArea { get; set; } // daylit area
        
        [JsonPropertyName("areaBed")] public double areaBed { get; set; } // area of bedrooms 
        [JsonPropertyName("areaFoyer")] public double areaFoyer { get; set; } // area of foyer
        [JsonPropertyName("areaKitchen")] public double areaKitchen { get; set; } // area of kitchen
        [JsonPropertyName("areaBath")] public double areaBath { get; set; } // area of bath
        [JsonPropertyName("areaLiving")] public double areaLiving { get; set; } // area of living


        [JsonPropertyName("sDACS")] public double sDACS { get; set; } // climate studio sDA
        [JsonPropertyName("sDA")] public double sDA { get; set; } // area normalized sDA

        [JsonPropertyName("EUI")] public double EUI { get; set; }  // [kWh/m2/a] STANDARD EUI
        [JsonPropertyName("EUI_HighPerformance")] public double EUI_HighPerformance { get; set; }  // [kWh/m2/a] HIGH Performance EUI
        [JsonPropertyName("view")] public double view { get; set; }  // View factor EN: High, factor of LIVING room with high level view

        [JsonPropertyName("gridCarbon")] public double gridCarbon { get; set; }  // [kgCO2e/kWh/a] Structural Material Quantity from Karamba Simulation

        // Structural
        [JsonPropertyName("SMQ")] public double SMQ { get; set; }  // [kg/m2] Structural Material Quantity from Karamba Simulation
        [JsonPropertyName("EC_S")] public double EC_S { get; set; }  // [kg/m2] Embodied carbon from Structure in Karamba Simulation
        [JsonPropertyName("EC_E")] public double EC_E { get; set; }  // [kg/m2] Embodied carbon from Eplus wall/ceiling estimate Solemma/Eplus
        [JsonPropertyName("maxSpan")] public double maxSpan { get; set; }

        // Spatial
        [JsonPropertyName("facadeAccess")] public bool facadeAccess { get; set; } // do all the rooms requiring daylight have access to facade ? 
        [JsonPropertyName("circLoss")] public double circLoss { get; set; }  // circulation Loss score (0 = best, no extra circulation 1=worst lot of foyer compared to living space
        [JsonPropertyName("roomShapeDiffAvg")] public double roomShapeDiffAvg { get; set; }  // average shape difference to reference floorplan
        [JsonPropertyName("roomShapeDiffMax")] public double roomShapeDiffMax { get; set; }   // max shape difference of room to reference floorplan
        [JsonPropertyName("circAccess")] public bool circAccess { get; set; }
        [JsonPropertyName("roomAccess")] public bool roomAccess { get; set; }
        [JsonPropertyName("minSize")] public bool minSize { get; set; }
        [JsonPropertyName("occupancy")] public double occupancy { get; set; }
        [JsonPropertyName("numRooms")] public int numRooms { get; set; }  // Number of bedrooms, 0 = Studio --> 1 = 1 bedroom , 2 = 2 bedroom etc...
        [JsonPropertyName("numBath")] public int numBath { get; set; }  // Number of bathrooms


        // Scoring 2
        [JsonPropertyName("furnitureAreaActual")] public double furnitureAreaActual { get; set; } // furnitureAreaActual
        [JsonPropertyName("furnitureAreaCapped")] public double furnitureAreaCapped { get; set; } // furnitureAreaActual if larger than minfurnitureArea (for number of rooms) otherwise area from mimimum size list 
        // z.B. if furnitureAreaActual = 15m2 and the apartment is a studio that requires minimum of 21.4m2 the furnitureAreaCapped will be 21.4m2, and the minfurnitureBool = false
        // z.B. if furnitureAreaActual = 35m2 and the apartment is a 1Bedroom  that requires a minimum of 33.4m2, the furnitureAreaCapped will be 35m2, and the minfurnitureBool = true
        [JsonPropertyName("minfurnitureArea")] public double minfurnitureArea { get; set; } // minimum furnishing area for specific program:
                                                                                            // Studio - 21.4m2
                                                                                            // 1Bed - 33.4m2
                                                                                            // 2Bed - 45.4m2
                                                                                            // 3Bed - 58.2m2
                                                                                            // 4Bed - 66.2m2
                                                                                            // 5Bed - 74.2m2
        [JsonPropertyName("minfurnitureBool")] public bool minfurnitureBool { get; set; } // if furnitureAreaActual < furnitureAreaCapped
        // to determine if the floorplan is valid. if the actual area is smaller than the capped area, it wasnt able to fit all required furniture items. 
        [JsonPropertyName("excessArea")] public double excessArea { get; set; } // A_e  ///// ApartmentArea - (furnitureAreaCapped * 1.8)

        [JsonPropertyName("excessCarbonS")] public double excessCarbonS { get; set; } // 
        // A_e*EUI*cc = C_e is the excess carbon emitted from an apartment (kgCO2e/a), where A_e is the
        // excess area (m2) (Equation 10), EUI the Energy Use Intensity STANDARD (kWh/m2) derived
        // from the energy simulation of the apartment and cc the local grid carbon content (kgCO2e/kWh)

        [JsonPropertyName("emissionDelta")] public double emissionDelta { get; set; } // 
        // excessCarbonS - (EUIs - EUIhp) 

        [JsonPropertyName("excessCarbonH")] public double excessCarbonH { get; set; } // 
        // A_e*EUI*cc = C_e is the excess carbon emitted from an apartment (kgCO2e/a), where A_e is the
        // excess area (m2) (Equation 10), EUI the Energy Use Intensity HIGH PERFORMANCE (kWh/m2) derived
        // from the energy simulation of the apartment and cc the local grid carbon content (kgCO2e/kWh)

        [JsonPropertyName("totalCarbonS")] public double totalCarbonS { get; set; } // total carbon emissions in kgCO2e/a for standard facade insulation
        [JsonPropertyName("totalCarbonH")] public double totalCarbonH { get; set; } // total carbon emissions in kgCO2e/a for a high performance building envelope                                                                            

        // Scoring
        [JsonPropertyName("scoreLayout")] public double scoreLayout { get; set; } // scoreProgram * area multiplier
        [JsonPropertyName("scoreLoss")] public double scoreLoss { get; set; } // layout efficiency loss, average of all layout losses for each room
        [JsonPropertyName("scoreProgram")] public double scoreProgram { get; set; } //              
        [JsonPropertyName("scoreAreaMult")] public double scoreAreaMult { get; set; } // 

        [JsonPropertyName("scoreBoolFurniture")] public bool scoreBoolFurniture { get; set; }
        [JsonPropertyName("scoreBed")] public double scoreBed { get; set; }
        [JsonPropertyName("scoreBath")] public double scoreBath { get; set; }
        [JsonPropertyName("scoreLiving")] public double scoreLiving { get; set; }
        [JsonPropertyName("scoreKitchen")] public double scoreKitchen { get; set; }
        [JsonPropertyName("scoreDining")] public double scoreDining { get; set; }



        // set to true if room, to false if hierarchical split

        public ApartmentResult()
        {
            
        }

        public ApartmentResult(string database, string id, string country, string city, string name, string building, int apartmentNum,  double area, double sDA, bool facadeAccess, bool circAccess, bool roomAccess, int numRooms, double roomShapeDiffAvg, double roomShapeDiffMax)
        {
            this.database = database;     
            this.id = id;
            this.country = country;
            this.city = city;
            this.name = name;

            this.building = building;
            this.apartmentNum = apartmentNum;

            this.area = area;             
            this.sDA = sDA;
            this.facadeAccess = circAccess;
            this.circAccess = circAccess; 
            this.roomAccess = roomAccess; 
            this.numRooms = numRooms;     
            this.roomShapeDiffAvg = roomShapeDiffAvg;     
            this.roomShapeDiffMax = roomShapeDiffMax;      
        }

        // Update Eval Functions

        //JSON conversion

        public static string serializeDataNode(ApartmentResult inputResult)
        {
            var serializeOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            string jsonString = JsonSerializer.Serialize(inputResult, serializeOptions);
            return jsonString;
        }
        public static ApartmentResult deserializeDataNode(string jsonString)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                IncludeFields = true
            };

            ApartmentResult reverseNode = JsonSerializer.Deserialize<ApartmentResult>(jsonString, options);
            return reverseNode;
        }

    }
}
