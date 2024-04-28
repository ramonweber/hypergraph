using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGeoLib.BuildingSolver
{
    public class Furniture
    {
        public string name { get; set; }
        public NFace path { get; set; }
        public NFace blocked { get; set; }
        public List<NLine> drawing { get; set; }
        public List<int> placementList { get; set; } // int of path side that is the back

        public double score { get; set; }
        public int importance { get; set; }

        public Furniture(string name, double score)
        {
            // placeholder furniture, e.g. storage addition
            this.name = name;
            this.score = score;

            List<Vec3d> zeroTemp = new List<Vec3d>() { Vec3d.Zero }; 

            this.path = new NFace(zeroTemp);
            this.blocked = new NFace(zeroTemp);
            this.drawing = new List<NLine>();
            this.importance = 0;
            this.placementList = new List<int>();
        }
        public Furniture(string name, NFace path, NFace blocked, List<NLine> drawing, List<int> placementListInput, double score, int importance)
        {
            this.name = name;
            this.path = path;
            this.blocked = blocked;
            this.drawing = drawing;
            this.score = score;
            this.importance = importance;
             
            List<int> placementListTemp = new List<int>();

            for (int i = 0; i < placementListInput.Count; i++)
            {
                if (placementListInput[i] >= path.edgeList.Count)
                {
                    placementListTemp.Add(placementListInput[i] % path.edgeList.Count);
                }
                else
                {
                    placementListTemp.Add(placementListInput[i]);
                }
            }

            this.placementList = new List<int>(placementListTemp);
        }

        public Furniture DeepCopy()
        {
            NFace copyPath = this.path.DeepCopy();
            NFace copyBlocked = this.blocked.DeepCopy();
            string copyName = new string(this.name.ToCharArray());
            List<NLine> copyDrawingList = new List<NLine>();
            List<int> copyPlacement = new List<int>(this.placementList);
            double copyScore = this.score;
            int copyImportance = this.importance;


            foreach (NLine line in this.drawing)
            {
                NLine copyLine = line.DeepCopy();
                copyDrawingList.Add(copyLine);
            }
            Furniture copyFurniture = new Furniture(copyName, copyPath, copyBlocked, copyDrawingList, copyPlacement, copyScore, copyImportance);
            return copyFurniture;
        }

        public static List<Furniture> DeepCopyList(List<Furniture> inputFurnitureList)
        {
            List<Furniture> copyFurniture = new List<Furniture>();

            for (int i = 0; i < inputFurnitureList.Count; i++)
            {
                Furniture tempFurniture = inputFurnitureList[i].DeepCopy();
                copyFurniture.Add(tempFurniture);
            }
            return copyFurniture;
        }

        public void transform(double angle, Vec3d moveVec1, Vec3d moveVec2)
        {
            this.path = NFace.transform(this.path, angle, moveVec1, moveVec2);
            this.blocked = NFace.transform(this.blocked, angle, moveVec1, moveVec2);
            this.drawing = NLine.transform(this.drawing, angle, moveVec1, moveVec2);
        }

        

        
    }
}
