using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace RGeoLib.BuildingSolver
{
    public class ALibrary
    {
        // initialize library from csv
        public static List<Apartment> ReadApartmentsFromJson(string filePath)
        {
             var apartments = new List<Apartment>();

            // Read the JSON file
            string jsonString = File.ReadAllText(filePath);

            try
            {
                // Parse the JSON content into a JsonDocument
                using (JsonDocument document = JsonDocument.Parse(jsonString))
                {
                    JsonElement root = document.RootElement;

                    // Ensure the root element is actually an array
                    if (root.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement element in root.EnumerateArray())
                        {
                                    try
                                    {
                                        // Extract each property using element.GetProperty("property_name").GetString()
                                        var database = element.GetProperty("database").GetString();
                                        var id = element.GetProperty("id").GetString();
                                        var country = element.GetProperty("country").GetString();
                                        var city = element.GetProperty("city").GetString();
                                        string name;
                                        if (element.TryGetProperty("name", out JsonElement nameElement))
                                        {
                                            name = nameElement.ValueKind == JsonValueKind.String
                                                ? nameElement.GetString()
                                                : nameElement.ToString();
                                        }
                                        else
                                        {
                                            // Handle the case where the "name" property is missing
                                            throw new KeyNotFoundException("The 'name' property is missing in the JSON object.");
                                        }
                                        var bounds = element.GetProperty("bounds").GetString();
                                        var facade = element.GetProperty("facade").GetString();
                                        var circulation = element.GetProperty("circulation").GetString();
                                        var split = element.GetProperty("split").GetString();

                                        // Attempt to create a new Apartment object
                                        Apartment apartment = new Apartment(database, id, country, city, name, bounds, facade, circulation, split);

                                        // Prepare Apartment
                                        //apartment.updateMerged();
                                        //apartment.updateDoorsWithAdjacency();
                                        //apartment.updateFacadeEval();
                                        //apartment.updateFacade();
                                        //apartment.updateCirculation();
                                        //apartment.eval.currentBldg = "reference";
                                        //apartment.eval.currentApt = 0;

                                        apartments.Add(apartment);
                                    }
                                    catch (ArgumentOutOfRangeException)
                                    {
                                        // Print the details of the apartment that caused the exception
                                        //Console.WriteLine("Apartment creation failed:");
                                        //Console.WriteLine($"Database: {element.GetProperty("database").GetString()}");
                                        Console.WriteLine($"ID: {element.GetProperty("id").GetString()}");
                                        //Console.WriteLine($"Country: {element.GetProperty("country").GetString()}");
                                        //Console.WriteLine($"City: {element.GetProperty("city").GetString()}");
                                        //Console.WriteLine($"Name: {element.GetProperty("name").GetString()}");
                                        // Continue to the next iteration without adding the apartment
                                        continue;
                                    }

                                }
                    }
                    else
                    {
                        throw new JsonException("JSON root is not an array.");
                    }
                }
            }
            catch (JsonException ex)
            {
                // Handle any JsonException
                Console.WriteLine($"JSON parsing error: {ex.Message}");
            }
            catch (IOException ex)
            {
                // Handle I/O errors
                Console.WriteLine($"I/O error: {ex.Message}");
            }

            return apartments;
        }
        public static void WriteApartmentsToJson(string filePath, List<Apartment> apartments)
        {
            List<ApartmentInfo> aptWriteDetails = new List<ApartmentInfo>();

            for (int i = 0;i < apartments.Count; i++)
            {
                ApartmentInfo currentApt = new ApartmentInfo(apartments[i]);
                aptWriteDetails.Add(currentApt);
            }
            // Convert the updated list back to JSON
            //var options = new JsonSerializerOptions { WriteIndented = true };
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }; 
            
            string jsonString = JsonSerializer.Serialize(aptWriteDetails, options);

            try
            {
                // Write the updated JSON string back to the file
                File.WriteAllText(filePath, jsonString);
            }
            catch (IOException ex)
            {
                // Handle I/O errors
                Console.WriteLine($"I/O error while writing to file: {ex.Message}");
            }
        }
        public static void AddApartmentToJson(string filePath, Apartment newApartment)
        {
            // Read the current apartments from the JSON file
            List<Apartment> apartments = ReadApartmentsFromJson(filePath);

            // Add the new apartment to the list
            apartments.Add(newApartment);

            // Convert to Serializable apt. 

            List<ApartmentInfo> aptWriteDetails = new List<ApartmentInfo>();

            for (int i = 0; i < apartments.Count; i++)
            {
                ApartmentInfo currentApt = new ApartmentInfo(apartments[i]);
                aptWriteDetails.Add(currentApt);
            }

            // Convert the updated list back to JSON
            //var options = new JsonSerializerOptions { WriteIndented = true };
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            string jsonString = JsonSerializer.Serialize(aptWriteDetails, options);

            try
            {
                // Write the updated JSON string back to the file
                File.WriteAllText(filePath, jsonString);
            }
            catch (IOException ex)
            {
                // Handle I/O errors
                Console.WriteLine($"I/O error while writing to file: {ex.Message}");
            }
        }
        public static void AddApartmentToJson(string filePath, List<Apartment> newApartments)
        {
            // Read the current apartments from the JSON file
            List<Apartment> apartments = ReadApartmentsFromJson(filePath);

            // Add the new apartment to the list
            apartments.AddRange(newApartments);

            // Convert to serializable 

            List<ApartmentInfo> aptWriteDetails = new List<ApartmentInfo>();

            for (int i = 0; i < apartments.Count; i++)
            {
                ApartmentInfo currentApt = new ApartmentInfo(apartments[i]);
                aptWriteDetails.Add(currentApt);
            }
            // Convert the updated list back to JSON
            //var options = new JsonSerializerOptions { WriteIndented = true };
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            string jsonString = JsonSerializer.Serialize(aptWriteDetails, options);

            try
            {
                // Write the updated JSON string back to the file
                File.WriteAllText(filePath, jsonString);
            }
            catch (IOException ex)
            {
                // Handle I/O errors
                Console.WriteLine($"I/O error while writing to file: {ex.Message}");
            }
        }
        // get closest fit

        // get 10 closest fits

        // choose random from libary

        // 
    }
}
