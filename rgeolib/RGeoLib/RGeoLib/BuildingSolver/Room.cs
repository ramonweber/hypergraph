using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Tls.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGeoLib.BuildingSolver
{
    public class Room
    {
        public string program { get; set; }
        public NFace bounds { get; set; }
        public List<NLine> wall { get; set; }
        public List<NLine> valid { get; set; }
        public NMesh blocked { get; set; }
        public List<Furniture> furniture { get; set; }
        public List<NLine> doors { get; set; }

        public double Score
        {
            get
            {
                return this.returnScore();
            }
        }

        public Room(string program, NFace bounds, List<NLine> wall)
        {
            this.program = program;
            this.bounds = bounds;
            this.wall = wall;
            this.valid = NLine.DeepCopyList(wall);
            this.blocked = new NMesh();
            this.furniture = new List<Furniture>();
            this.doors = new List<NLine>();
        }

        public Room(string program, NFace bounds, List<NLine> wall, List<NLine> valid, NMesh blocked, List<NLine> doors)
        {
            this.program = program;
            this.bounds = bounds;
            this.wall = wall;
            this.valid = valid;
            this.blocked = blocked;
            this.doors = doors;
            this.furniture = new List<Furniture>();
        }

        public Room(string program, NFace bounds, List<NLine> wall, List<NLine> valid, NMesh blocked, List<Furniture> furnitureList)
        {
            this.program = program;
            this.bounds = bounds;
            this.wall = wall;
            this.valid = valid;
            this.blocked = blocked;
            this.furniture = furnitureList;
            this.doors = new List<NLine>();
        }

        public Room(string program, NFace bounds, List<NLine> wall, List<NLine> valid, NMesh blocked, List<Furniture> furnitureList, List<NLine> doors)
        {
            this.program = program;
            this.bounds = bounds;
            this.wall = wall;
            this.valid = valid;
            this.blocked = blocked;
            this.furniture = furnitureList;
            this.doors = doors;
        }

        public Room(string program, NFace bounds, List<NLine> doorLinesPerRoom, double wallThickness, double doorWidth, double doorOffset, double doorClearanceDepth, bool leftOrient = true)
        {

            NMesh outMesh = RClipper.clipperInflatePathsMitterPolygon(bounds, wallThickness);
            List<NLine> wallLines = RhConvert.NFaceToLineList(bounds);

            // Check if doors were input
            if (doorLinesPerRoom.Count > 0)
            {
                List<NLine> doorBlockedLine = new List<NLine>();
                List<NLine> doorActual = new List<NLine>();

                // get longest door line
                //List<NLine> sortedDoorLines = doorLinesPerRoom.OrderBy(o => o.Length).ToList();
                // get first 1m from left
                //sortedDoorLines.Reverse();

                for (int i = 0; i < doorLinesPerRoom.Count; i++)
                {
                    NLine selectedLine = doorLinesPerRoom[i];

                    Vec3d dirVecBlocked = Vec3d.ScaleToLen(selectedLine.Direction, doorOffset + doorWidth + doorOffset);
                    Vec3d dirVecOffset = Vec3d.ScaleToLen(selectedLine.Direction, doorOffset);
                    Vec3d dirVecDoor = Vec3d.ScaleToLen(selectedLine.Direction, doorWidth);

                    /////////////////////////////////////////////////////////////////////////////
                    // Check orientation and do left one or right one

                    if (leftOrient)
                    {
                        NLine tempDoor = new NLine(selectedLine.start, selectedLine.start + dirVecBlocked);

                        if (tempDoor.Length < selectedLine.Length)
                        {
                            // DOOR valid if smaller length than right door
                            doorBlockedLine.Add(tempDoor);
                            NLine tempActual = new NLine(selectedLine.start + dirVecOffset, selectedLine.start + dirVecOffset + dirVecDoor);
                            doorActual.Add(tempActual);
                        }
                    }
                    else
                    {
                        NLine tempDoor = new NLine(selectedLine.end - dirVecBlocked, selectedLine.end);

                        if (tempDoor.Length < selectedLine.Length)
                        {
                            // DOOR valid if smaller length than right door
                            doorBlockedLine.Add(tempDoor);
                            NLine tempActual = new NLine(selectedLine.end - dirVecOffset - dirVecDoor, selectedLine.end - dirVecOffset);
                            doorActual.Add(tempActual);
                        }
                    }
                }

                List<NFace> blockedFacesTemp = new List<NFace>();

                for (int i = 0; i < doorBlockedLine.Count; i++)
                {
                    List<NLine> tempBlockedSingle = new List<NLine>(){ doorBlockedLine[i] };
                    NPolyLine doorPolyLine = new NPolyLine(tempBlockedSingle);
                    NMesh tempBlocked = RClipper.clipperInflatePolylineMiterButtMesh(doorPolyLine, doorClearanceDepth);
                    blockedFacesTemp.AddRange(tempBlocked.faceList);
                }

                NMesh blockedMesh = new NMesh(blockedFacesTemp);

                List<NLine> linesDoor = NMesh.GetAllMeshLines(blockedMesh);
                List<NLine> linesRoom = NMesh.GetAllMeshLines(outMesh);

                NMesh intersectedMesh = outMesh.DeepCopyWithID();
                if (blockedMesh.faceList.Count > 0)
                    intersectedMesh = RClipper.clipperDifference(outMesh.faceList[0], blockedMesh.faceList[0]);

                //NMesh intersectedMesh = RClipper.clipperDifference(outMesh.faceList[0], blockedMesh.faceList[0]); // .lineListBooleanDifferenceTol(linesDoor, linesRoom, 0.01);

                List<NLine> linesIntMesh = NMesh.GetAllMeshLines(intersectedMesh);

                List<NLine> linesIntersection = RIntersection.lineListBooleanIntersectionTol(linesRoom, linesIntMesh, 0.05);

                ////////////////////////////////////////////////////////////////////////////////
                // Create room

                //Room tempRoom = new Room(program, bounds, wallLines, linesIntersection, blockedMesh, doorActual);

                this.program = program;
                this.bounds = bounds;
                this.wall = wallLines;
                this.valid = linesIntersection;
                this.blocked = blockedMesh;
                this.doors = doorActual;
                this.furniture = new List<Furniture>();
            } else
            {
                this.program = program;
                this.bounds = bounds;
                this.wall = wallLines;
                this.valid = NMesh.GetAllMeshLines(outMesh); // NLine.DeepCopyList(wallLines);
                this.blocked = new NMesh();
                this.doors = new List<NLine>();
                this.furniture = new List<Furniture>();
            }
            
        }

        public Room DeepCopy()
        {
            string copyProgram = new string(this.program.ToCharArray());
            NFace copyBounds = this.bounds.DeepCopyWithMID();
            List<NLine> copyWall = NLine.DeepCopyList(this.wall);
            List<NLine> copyValid = NLine.DeepCopyList(this.valid);

            NMesh copyBlocked = new NMesh();
            if (this.blocked.faceList != null)
                copyBlocked = this.blocked.DeepCopyWithID();

            List<Furniture> copyFurnitureList = Furniture.DeepCopyList(this.furniture);
            List<NLine> doors = NLine.DeepCopyList(this.doors);

            Room copyRoom = new Room(copyProgram, copyBounds, copyWall, copyValid, copyBlocked, copyFurnitureList, doors);
            //Room copyRoom = new Room(copyProgram, copyBounds, copyWall);
            return copyRoom;
        }


        // Drawing Functions
        /*
        public List<NLine> drawFurniture()
        {
            List<NLine> tempLines = new List<NLine>();

            for (int i = 0; i < this.furniture.Count; i++)
            {
                tempLines.AddRange(this.furniture[i].drawing);
            }
            return tempLines;
        }
        public NMesh drawBlocked()
        {
            List<NFace> blockedList = new List<NFace>();

            for (int i = 0; i < this.furniture.Count; i++)
            {
                blockedList.Add(this.furniture[i].blocked);
            }
            return new NMesh(blockedList);
        }
        public NMesh drawPath()
        {
            List<NFace> pathList = new List<NFace>();

            for (int i = 0; i < this.furniture.Count; i++)
            {
                pathList.Add(this.furniture[i].path); 
            }
            return new NMesh(pathList);
        }
        public static List<NLine> drawFurniture(Room inputRooms)
        {
            return inputRooms.drawFurniture();
        }
        public static List<NLine> drawBlocked(Room inputRooms)
        {
            return inputRooms.drawBlocked();
        }
        public static List<NLine> drawPath(Room inputRooms)
        {
            return inputRooms.drawPath();
        }
        public static List<NLine> drawFurniture(List<Room> inputRooms)
        {
            List<NLine> tempLines = new List<NLine>();

            for (int i = 0; i < inputRooms.Count; i++)
            {
                List<NLine> temp = inputRooms[i].drawFurniture();
                tempLines.AddRange(temp);
            }
            return tempLines;
        }
        public static List<NLine> drawBlocked(List<Room> inputRooms)
        {
            List<NLine> tempLines = new List<NLine>();

            for (int i = 0; i < inputRooms.Count; i++)
            {
                List<NLine> temp = inputRooms[i].drawBlocked();
                tempLines.AddRange(temp);
            }
            return tempLines;
        }
        public static List<NLine> drawPath(List<Room> inputRooms)
        {
            List<NLine> tempLines = new List<NLine>();

            for (int i = 0; i < inputRooms.Count; i++)
            {
                List<NLine> temp = inputRooms[i].drawPath();
                tempLines.AddRange(temp);
            }
            return tempLines;
        }
        */

        // Placement Functions
        
        public static Tuple<bool, List<Room>> placeFurnitureItemsHierarchicallyBool(Room roomInput, List<Furniture> furnitureList, bool insidePlacement = false)
        {
            // same as placeFurnitureItemsHierarchiaclly but sets bool to false if no furniture was placed


            bool placementSuccess = true;
            // Tries to place furniture item first, if not possible try to place second,
            List<Room> outRooms = new List<Room>();

            for (int i = 0; i < furnitureList.Count; i++)
            {
                List<Room> validRooms = Room.placeFurnitureItem(roomInput, furnitureList[i], insidePlacement);
                outRooms.AddRange(validRooms);
                if (outRooms.Count > 0)
                    break;
            }

            // if no furniture was placed, return original room, without placed furniture
            if (outRooms.Count == 0)
            {
                outRooms.Add(roomInput);
                placementSuccess = false;
            }

            List<Room> sortedOutput = outRooms.OrderBy(o => o.Score).ToList();
            sortedOutput.Reverse();
            return new Tuple<bool, List<Room>>(placementSuccess, sortedOutput);

            //return outRooms;
        }

        public static List<Room> placeFurnitureItemsHierarchically(Room roomInput, List<Furniture> furnitureList, bool insidePlacement = false)
        {
            // Tries to place furniture item first, if not possible try to place second,
            List<Room> outRooms = new List<Room>();

            for (int i = 0; i < furnitureList.Count; i++)
            {
                List<Room> validRooms = Room.placeFurnitureItem(roomInput, furnitureList[i], insidePlacement);
                outRooms.AddRange(validRooms);
                if (outRooms.Count > 0)
                    break;
            }

            // if no furniture was placed, return original room, without placed furniture
            if (outRooms.Count == 0)
                outRooms.Add(roomInput);

            List<Room> sortedOutput = outRooms.OrderBy(o => o.Score).ToList();
            sortedOutput.Reverse();
            return sortedOutput;

            //return outRooms;
        }
        public static List<Room> placeFurnitureItemsHierarchically(List<Room> roomListInput, List<Furniture> furnitureList, bool insidePlacement = false)
        {
            // Tries to place furniture item first, if not possible try to place second,
            List<Room> outRooms = new List<Room>();

            for (int i = 0; i < roomListInput.Count; i++)
            {
                outRooms.AddRange(placeFurnitureItemsHierarchically(roomListInput[i], furnitureList, insidePlacement));
            }

            List<Room> sortedOutput = outRooms.OrderBy(o => o.Score).ToList();
            sortedOutput.Reverse();
            return sortedOutput;
            //return outRooms;
        }
        public static List<Room> placeFurnitureItemsHierarchyWithScoreFill(List<Room> roomListInput, List<Furniture> furnitureList, double targetScore)
        {
            // Tries to place furniture item first, if not possible try to place second,
            List<Room> outRooms = new List<Room>();

            for (int i = 0; i < roomListInput.Count; i++)
            {
                outRooms.AddRange(placeFurnitureItemsHierarchyWithScoreFill(roomListInput[i], furnitureList, targetScore));
            }

            List<Room> sortedOutput = outRooms.OrderBy(o => o.Score).ToList();
            sortedOutput.Reverse();
            return sortedOutput;

            //return outRooms;
        }
        public static List<Room> placeFurnitureItemsHierarchyWithScoreFill(Room roomInput, List<Furniture> furnitureList, double targetScore)
        {
            // Tries to place furniture item first, if not possible try to place second,
            // Use for furniture like storage, that can be in different parts of the room.
            // Uses last item in furniture list as filler furniture

            List<Room> outRooms = new List<Room>();
            double startingScore = returnScore(roomInput);
            Furniture fillerFurniture = furnitureList[furnitureList.Count - 1];

            for (int i = 0; i < furnitureList.Count; i++)
            {
                List<Room> validRooms = Room.placeFurnitureItem(roomInput, furnitureList[i]);
                outRooms.AddRange(validRooms);
                if (outRooms.Count > 0)
                    break;
            }

            List<Room> filteredRooms = new List<Room>();
            // check score
            for (int i = 0; i < outRooms.Count; i++)
            {
                double currentScore = returnScore(outRooms[i]);
                double scoreDifference = currentScore - startingScore;

                // check if target score has been reached, otherwise try infill more.
                if (scoreDifference < targetScore)
                {
                    List<Room> validRooms = Room.placeFurnitureItem(outRooms[i], fillerFurniture);
                    filteredRooms.AddRange(validRooms);
                }
                else
                {
                    filteredRooms.Add(outRooms[i]);
                }
            }

            // If no furniture placed, add fill room
            /// ------------- if didnt work

            if (filteredRooms.Count == 0)
            {
                List<Room> tempRooms = Room.placeFurnitureItem(roomInput, fillerFurniture);
                filteredRooms = tempRooms;
            }

            List<Room> filteredRooms2 = new List<Room>();

            for (int i = 0; i < filteredRooms.Count; i++)
            {
                double currentScore = returnScore(filteredRooms[i]);
                double scoreDifference = currentScore - startingScore;

                // check if target score has been reached, otherwise try infill more.
                if (scoreDifference < targetScore)
                {
                    List<Room> validRooms = Room.placeFurnitureItem(filteredRooms[i], fillerFurniture);
                    filteredRooms2.AddRange(validRooms);
                }
                else
                {
                    filteredRooms2.Add(filteredRooms[i]);
                }
            }

            // if no furniture was placed, return original room, without placed furniture
            if (filteredRooms2.Count == 0)
                filteredRooms2.Add(roomInput);

            List<Room> sortedOutput = filteredRooms2.OrderBy(o => o.Score).ToList();
            sortedOutput.Reverse();
            return sortedOutput;
            //return filteredRooms2;
        }
        

        public static List<Room> placeFurnitureItem(Room roomInput, Furniture furnitureItem, bool insidePlacement = false)
        {
            List<Room> validRoomsLeft = Room.placeFurnitureItemSingleSide(roomInput, furnitureItem, false, insidePlacement);
            List<Room> validRoomsRight = Room.placeFurnitureItemSingleSide(roomInput, furnitureItem, true, insidePlacement);
            validRoomsLeft.AddRange(validRoomsRight);
            return validRoomsLeft;
        }
        public static List<Room> placeFurnitureItemSingleSide(Room roomInput, Furniture furnitureItem, bool leftAnchor = true, bool insidePlacement = false)
        {
            List<Room> validRooms = new List<Room>();

            for (int i = 0; i < roomInput.valid.Count; i++)
            {
                for (int j = 0; j < furnitureItem.placementList.Count; j++)
                {
                    Room currentRoom = roomInput.DeepCopy();
                    double sourceLength = furnitureItem.path.edgeList[furnitureItem.placementList[j]].Length; // length of furniture rectangle
                    double lineLength = currentRoom.valid[i].Length;

                    if (sourceLength <= lineLength)
                    {
                        Furniture copyFurniture = furnitureItem.DeepCopy();
                        Tuple<NFace, NLine, double, Vec3d, Vec3d> placementTuple = NFace.moveNFaceToLineWithHistory(copyFurniture.path, currentRoom.valid[i], furnitureItem.placementList[j], leftAnchor);
                        NFace blockedPlaced = NFace.transform(copyFurniture.blocked, placementTuple.Item3, placementTuple.Item4, placementTuple.Item5);

                        copyFurniture.transform(placementTuple.Item3, placementTuple.Item4, placementTuple.Item5);

                        NFace pathPlaced = placementTuple.Item1;
                        currentRoom.valid[i] = placementTuple.Item2;

                        //////////////////////////////////////////////////////////////////////////////////
                        // Inside valid (adds circulation edges to valid so that e.g dining table can be placed inside the room)
                        if (insidePlacement)
                        {
                            List<NLine> boundariesPlacedBlock = RhConvert.NFaceToLineList(pathPlaced);
                            currentRoom.valid.AddRange(boundariesPlacedBlock);
                        }


                        //////////////////////////////////////////////////////////////////////////////////


                        // check does overlap bounds of room ///////////////////////////////////////////////////////////
                        bool doesoverlap = !RIntersection.insideBounds(pathPlaced, currentRoom.bounds);

                        // check blocked does not overlap other blocked items
                        if (currentRoom.blocked.faceList != null)
                        {
                            for (int e = 0; e < currentRoom.blocked.faceList.Count; e++)
                            {
                                bool thisoverlaps = RIntersection.doNFacesIntersectMinkowski(blockedPlaced, currentRoom.blocked.faceList[e]);  //blockedPlaced
                                if (thisoverlaps == true)
                                {
                                    doesoverlap = true;
                                }
                            }
                        }

                        // Check if placed paths overlaps existing blocked
                        for (int f = 0; f < currentRoom.furniture.Count; f++)
                        {
                            bool placedBlocked = RIntersection.doNFacesIntersectMinkowski(pathPlaced, currentRoom.furniture[f].blocked);  //blockedPlaced
                            if (placedBlocked == true)
                            {
                                doesoverlap = true;
                            }
                        }

                        // Check if placed blocked overlaps existing paths
                        for (int f=0; f< currentRoom.furniture.Count; f++)
                        {
                            bool placedBlocked = RIntersection.doNFacesIntersectMinkowski(blockedPlaced, currentRoom.furniture[f].path);  //blockedPlaced
                            if (placedBlocked == true)
                            {
                                doesoverlap = true;
                            }
                        }
                       

                        if (doesoverlap == false)
                        {

                            //if (currentRoom.blocked.faceList != null)
                            //{
                            //    currentRoom.blocked.faceList.Add(copyFurniture.blocked);
                            //}
                            //else
                            //{
                            //    currentRoom.blocked = new NMesh(copyFurniture.blocked);
                            //}
                            currentRoom.furniture.Add(copyFurniture);
                            validRooms.Add(currentRoom);
                        }
                    }
                }
            }
            return validRooms;
        }

        // Connected Query

        public static bool hasExtraConnected(NFace roomInput, NMesh extraMesh, List<NLine> doorsEval)
        {

            bool hasExtraConnected = false;

            // Check if intersects with door,
            for (int j = 0; j < doorsEval.Count; j++)
            {
                bool doesIntersectWithDoor = RIntersection.LineFaceIntersectionBool(doorsEval[j], roomInput);
                if (doesIntersectWithDoor)
                {
                    // Go through all extra, check if intersect with door
                    for (int k = 0; k < extraMesh.faceList.Count; k++)
                    {
                        bool doesExtraIntersectWithDoor = RIntersection.LineFaceIntersectionBool(doorsEval[j], extraMesh.faceList[k]);

                        if (doesExtraIntersectWithDoor == true && doesIntersectWithDoor)
                            hasExtraConnected = true;
                    }
                }
            }

            return hasExtraConnected;
        }


        // SCORING
        public double returnScore()
        {
            // Tries to place furniture item first, if not possible try to place second,
            double cummulativeScore = 0;

            for (int i = 0; i < this.furniture.Count; i++)
            {
                cummulativeScore += this.furniture[i].score;
            }

            return cummulativeScore;
        }
        public static double returnScore(Room roomInput)
        {
            // Tries to place furniture item first, if not possible try to place second,
            double cummulativeScore = 0;

            for (int i = 0; i < roomInput.furniture.Count; i++)
            {
                cummulativeScore += roomInput.furniture[i].score;
            }

            return cummulativeScore;
        }

        public bool hasStringFurniture(string searchString)
        {
            bool hasStringFurniture = false;

            for (int i=0;i<this.furniture.Count; i++)
            {
                if (this.furniture[i].name == searchString)
                {
                    hasStringFurniture = true;
                    break;
                }
            }
            return hasStringFurniture;
        }

        // PROGRAM INFILL

        public static Tuple<bool, List<Room>> createStudioLiving(bool bedExists, Room roomInput, List<Furniture> furnitureBeds, List<Furniture> furnitureSofa, List<Furniture> furnitureDining)
        {
            // checks if bed should be placed
            // if yes, place bed, if no skip
            if (bedExists==false)
            {
                // Bedroom
                // 1 Bed - Hierarchy fill
                Tuple<bool, List<Room>> betPlacementTuple = placeFurnitureItemsHierarchicallyBool(roomInput, furnitureBeds, true);

                bool bedPlaced = betPlacementTuple.Item1;
                List<Room> validWithBed = betPlacementTuple.Item2;

                // 2 Sofa - Hierarchy Fill
                List<Room> validWithTable = placeFurnitureItemsHierarchically(validWithBed, furnitureSofa, true);

                // 3 Dining - HierarchyFill
                List<Room> validWithExtra = placeFurnitureItemsHierarchically(validWithTable, furnitureDining);

                // go through room list, check if has dining room furniture 
                return new Tuple<bool, List<Room>>(bedPlaced, validWithExtra);
            }
            else 
            {
                // Bedroom
                bool bedPlaced = true;

                // 2 Sofa - Hierarchy Fill
                List<Room> validWithTable = placeFurnitureItemsHierarchically(roomInput, furnitureSofa, true);

                // 3 Dining - HierarchyFill
                List<Room> validWithExtra = placeFurnitureItemsHierarchically(validWithTable, furnitureDining);

                return new Tuple<bool, List<Room>>(bedPlaced, validWithExtra);
            }
           
        }

        public static Room createBedRoomOLD(Room roomInput, List<Furniture> furnitureBeds, List<Furniture> furnitureStorage, List<Furniture> furnitureTable, List<Furniture> furnitureExtra, bool placeStorage = true)
        {
            // Bedroom
            // 1 Bed - Hierarchy fill
            List<Room> validWithBed = placeFurnitureItemsHierarchically(roomInput, furnitureBeds);

            if (placeStorage == true)
            {
                // 2 Storage - Flood Fill
                List<Room> validWithStorage = placeFurnitureItemsHierarchyWithScoreFill(validWithBed[0], furnitureStorage, 3);



                // 3 Table - Hierarchy Fill
                List<Room> validWithTable = placeFurnitureItemsHierarchically(validWithStorage[0], furnitureTable);

                // 4 Extra - HierarchyFill
                List<Room> validWithExtra = placeFurnitureItemsHierarchically(validWithTable[0], furnitureExtra);

                return validWithExtra[0];
            }
            else
            {
                // 3 Table - Hierarchy Fill
                List<Room> validWithTable = placeFurnitureItemsHierarchically(validWithBed[0], furnitureTable);

                // 4 Extra - HierarchyFill
                List<Room> validWithExtra = placeFurnitureItemsHierarchically(validWithTable[0], furnitureExtra);

                Furniture placeholderStorage = new Furniture("StoragePlaceholder", 3);



                return validWithExtra[0];
            }

        }
        public static Room createBedRoom(Room roomInput, List<Furniture> furnitureBeds, List<Furniture> furnitureStorage, bool placeStorage = true)
        {
            // Bedroom
            // 1 Bed - Hierarchy fill
            List<Room> validWithBed = placeFurnitureItemsHierarchically(roomInput, furnitureBeds);

            if (placeStorage == true)
            {
                // 2 Storage - Flood Fill
                List<Room> validWithStorage = placeFurnitureItemsHierarchyWithScoreFill(validWithBed[0], furnitureStorage, 3);
                return validWithStorage[0];
            }
            else
            {
                return validWithBed[0];
            }
            
        }
        public static List<Room> createBedRoomAll(Room roomInput, List<Furniture> furnitureBeds, List<Furniture> furnitureStorage, List<Furniture> furnitureTable, List<Furniture> furnitureExtra, bool placeStorage = true)
        {
            // Bedroom
            // 1 Bed - Hierarchy fill
            List<Room> validWithBed = placeFurnitureItemsHierarchically(roomInput, furnitureBeds);

            if (placeStorage == true)
            {
                // 2 Storage - Flood Fill
                List<Room> validWithStorage = placeFurnitureItemsHierarchyWithScoreFill(validWithBed, furnitureStorage, 0.75);

                // 3 Table - Hierarchy Fill
                List<Room> validWithTable = placeFurnitureItemsHierarchically(validWithStorage, furnitureTable);

                // 4 Extra - HierarchyFill
                List<Room> validWithExtra = placeFurnitureItemsHierarchically(validWithTable, furnitureExtra);

                return validWithExtra;
            }
            else
            {

                // 3 Table - Hierarchy Fill
                List<Room> validWithTable = placeFurnitureItemsHierarchically(validWithBed, furnitureTable);

                // 4 Extra - HierarchyFill
                List<Room> validWithExtra = placeFurnitureItemsHierarchically(validWithTable, furnitureExtra);

                Furniture placeholderStorage = new Furniture("StoragePlaceholder", 3);

                for (int i=0;i<validWithExtra.Count; i++)
                {
                    validWithExtra[i].furniture.Add(placeholderStorage);
                }

                return validWithExtra;
            }
        }

        public static Room createKitchen(Room roomInput, List<Furniture> furnitureKitchen, List<Furniture> furnitureDining)
        {
            // Bedroom
            // 1 Sofa - Hierarchy fill
            List<Room> validWithKitchen = placeFurnitureItemsHierarchically(roomInput, furnitureKitchen, true);

            // 2 Seating - Hierarchy Fill
            List<Room> validWithSeating = placeFurnitureItemsHierarchically(validWithKitchen[0], furnitureDining);

            return validWithSeating[0];
        }
        public static List<Room> createKitchenAll(Room roomInput, List<Furniture> furnitureKitchen, List<Furniture> furnitureDining)
        {
            // Bedroom
            // 1 Sofa - Hierarchy fill
            List<Room> validWithKitchen = placeFurnitureItemsHierarchically(roomInput, furnitureKitchen, true);

            // 2 Seating - Hierarchy Fill
            List<Room> validWithSeating = placeFurnitureItemsHierarchically(validWithKitchen, furnitureDining);

            return validWithSeating;
        }


        public static Room createLivingRoom(Room roomInput, List<Furniture> furnitureSofa, List<Furniture> furnitureDining)
        {
            // Bedroom
            // 1 Sofa - Hierarchy fill
            List<Room> validWithSofa = placeFurnitureItemsHierarchically(roomInput, furnitureSofa, true);

            // 2 Seating - Hierarchy Fill
            List<Room> validWithSeating = placeFurnitureItemsHierarchically(validWithSofa[0], furnitureDining);

            return validWithSeating[0];
        }
        public static List<Room> createLivingRoomAll(Room roomInput, List<Furniture> furnitureSofa, List<Furniture> furnitureDining)
        {
            // Bedroom
            // 1 Sofa - Hierarchy fill
            List<Room> validWithSofa = placeFurnitureItemsHierarchically(roomInput, furnitureSofa, true);

            // 2 Seating - Hierarchy Fill
            List<Room> validWithSeating = placeFurnitureItemsHierarchically(validWithSofa, furnitureDining);

            return validWithSeating;
        }

        public static Room createBathRoomOLD(Room roomInput, List<Furniture> furnitureToilet, List<Furniture> furnitureSink, List<Furniture> furnitureShower, List<Furniture> furnitureLaundry)
        {
            // Bedroom
            // 1 Bed - Hierarchy fill
            List<Room> validWithToilet = placeFurnitureItemsHierarchically(roomInput, furnitureToilet);

            // 2 Storage - Flood Fill
            List<Room> validWithSink = placeFurnitureItemsHierarchically(validWithToilet[0], furnitureSink);

            // 3 Table - Hierarchy Fill
            List<Room> validWithShower = placeFurnitureItemsHierarchically(validWithSink[0], furnitureShower);

            // 4 Extra - HierarchyFill
            List<Room> validWithLaundry = placeFurnitureItemsHierarchically(validWithShower[0], furnitureLaundry);

            return validWithLaundry[0];
        }
        public static Room createBathRoom(Room roomInput, List<Furniture> furnitureToilet, List<Furniture> furnitureSink, List<Furniture> furnitureShower)
        {
            // Bedroom
            // 1 Bed - Hierarchy fill
            List<Room> validWithToilet = placeFurnitureItemsHierarchically(roomInput, furnitureToilet);

            // 2 Storage - Flood Fill
            List<Room> validWithSink = placeFurnitureItemsHierarchically(validWithToilet[0], furnitureSink);

            // 3 Table - Hierarchy Fill
            List<Room> validWithShower = placeFurnitureItemsHierarchically(validWithSink[0], furnitureShower);

            // 4 Extra - HierarchyFill
            //List<Room> validWithLaundry = placeFurnitureItemsHierarchically(validWithShower[0], furnitureLaundry);

            return validWithShower[0];
        }
        public static List<Room> createBathRoomAll(Room roomInput, List<Furniture> furnitureToilet, List<Furniture> furnitureSink, List<Furniture> furnitureShower, List<Furniture> furnitureLaundry)
        {
            // Bedroom
            // 1 Bed - Hierarchy fill
            List<Room> validWithToilet = placeFurnitureItemsHierarchically(roomInput, furnitureToilet);

            // 2 Storage - Flood Fill
            List<Room> validWithSink = placeFurnitureItemsHierarchically(validWithToilet, furnitureSink);

            // 3 Table - Hierarchy Fill
            List<Room> validWithShower = placeFurnitureItemsHierarchically(validWithSink, furnitureShower);

            // 4 Extra - HierarchyFill
            List<Room> validWithLaundry = placeFurnitureItemsHierarchically(validWithShower, furnitureLaundry);

            return validWithLaundry;
        }
    }
}
