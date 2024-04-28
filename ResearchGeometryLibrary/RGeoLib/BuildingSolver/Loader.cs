using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace RGeoLib.BuildingSolver
{
    public class Loader
    {
        public static List<Apartment> ApartmentListFromExcel(string path)
        {
            XSSFWorkbook hssfwb;
            using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                hssfwb = new XSSFWorkbook(file);
            }

            ISheet sheet = hssfwb.GetSheetAt(0);
            List<Apartment> apartments = new List<Apartment>();

            for (int row = 1; row <= sheet.LastRowNum; row++) // Skip header row (row = 0)
            {
                if (sheet.GetRow(row) != null) // Null check for empty row
                {
                    string bounds = sheet.GetRow(row).GetCell(8).ToString(); // 8th column is 'bounds'
                    string facade = sheet.GetRow(row).GetCell(9).ToString(); // 9th column is 'facade'
                    string circulation = sheet.GetRow(row).GetCell(10).ToString(); // 10th column is 'circulation'
                    //string split = sheet.GetRow(row).GetCell(11).ToString(); // 11th column is 'split'

                    string split = sheet.GetRow(row).GetCell(11) != null ? sheet.GetRow(row).GetCell(11).ToString() : null; // 11th column is 'split'

                    Apartment newApartment = split != null ? new Apartment(bounds, facade, circulation, split)
                                                           : new Apartment(bounds, facade, circulation);

                    apartments.Add(newApartment);

                }
            }
            return apartments;
        }

        public static void SaveApartmentListToExcel(List<Apartment> apartments, string path)
        {
            XSSFWorkbook hssfwb = new XSSFWorkbook();
            ISheet sheet = hssfwb.CreateSheet("Apartment Data");
            IRow headerRow = sheet.CreateRow(0);

            // Set headers
            string[] headers = new string[] { "database", "id", "country", "city", "name", "area", "bedrooms", "bathrooms", "bounds", "facade", "circulation", "split" };
            for (int i = 0; i < headers.Length; i++)
            {
                headerRow.CreateCell(i).SetCellValue(headers[i]);
            }

            // Fill data
            for (int i = 0; i < apartments.Count; i++)
            {
                IRow row = sheet.CreateRow(i + 1); // Start from second row
                Apartment apartment = apartments[i];

                row.CreateCell(0).SetCellValue(apartment.eval.database ?? "default");
                row.CreateCell(1).SetCellValue(apartment.eval.id ?? "default");
                row.CreateCell(2).SetCellValue(apartment.eval.country ?? "default");
                row.CreateCell(3).SetCellValue(apartment.eval.city ?? "default");
                row.CreateCell(4).SetCellValue(apartment.eval.name ?? "default");
                row.CreateCell(5).SetCellValue(apartment.eval.area.ToString() ?? "default");
                row.CreateCell(6).SetCellValue(apartment.eval.numRooms.ToString() ?? "default");
                row.CreateCell(7).SetCellValue(apartment.eval.numRooms.ToString() ?? "default");
                row.CreateCell(8).SetCellValue(NFace.serializeNFace(apartment.bounds) ?? "default");
                row.CreateCell(9).SetCellValue(NLine.serializeNLineList(apartment.facade) ?? "default");
                row.CreateCell(10).SetCellValue(NLine.serializeNLineList(apartment.circulation) ?? "default");
                row.CreateCell(11).SetCellValue(DataNode.serializeDataNode(apartment.splitNode) ?? "default");

                List<NLine> roomLines = NMesh.GetAllMeshLines(apartment.rooms);
                string roomjson = NLine.serializeNLineList(roomLines);
                row.CreateCell(12).SetCellValue(roomjson ?? "default");
            }

            // Save the file
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                hssfwb.Write(fs);
            }
        }
    }
}
