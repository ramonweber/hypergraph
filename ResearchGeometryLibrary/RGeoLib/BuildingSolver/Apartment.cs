using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MathNet.Numerics.LinearAlgebra;
using RGeoLib.BuildingSolver;

namespace RGeoLib
{
    public class Apartment
    {

        //public string id = "";

        public string database { get; set; }
        public string id { get; set; }
        public string country { get; set; }
        public string city { get; set; }
        public string name { get; set; }

        // Geometry
        public NFace bounds { get; set; }
        public NMesh rooms { get; set; }
        public NMesh roomsMerged { get; set; }
        public List<NLine> facade { get; set; }
        public List<NLine> adiabatic { get; set; }
        public List<NLine> circulation { get; set; }
        public List<NLine> internalWalls { get; set; }
        public List<NLine> doors { get; set; } // only actual doors between rooms
        public List<NLine> doorsAll { get; set; } // doors between rooms could also be open


        public List<Room> roomList { get; set; }

        // Rooms

        //public List<Room> Rooms { get; set; }

        // Connectivity
        public Matrix<double> connectivityMatrix { get; set; }
        public List<List<int>> connectivityList { get; set; }

        public DataNode splitNode { get; set; }

        // evaluation
        public ApartmentResult eval { get; set; }

        public Apartment referenceApartment { get; set; }

        public double referenceRotation { get; set; }
        public double referenceOrientationScore { get; set; }

        public double livingArea
        {
            get
            {
                double totalLivingArea = 0;
                for (int i = 0; i < this.rooms.faceList.Count; i++)
                {
                    if (this.rooms.faceList[i].merge_id == "bed" ||
                        this.rooms.faceList[i].merge_id == "bath" ||
                        this.rooms.faceList[i].merge_id == "living" ||
                        this.rooms.faceList[i].merge_id == "kitchen"
                        )
                        totalLivingArea += this.rooms.faceList[i].Area;
                }
                return totalLivingArea;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////
        ///
        // Constructors!!!

        public Apartment(NFace bounds, NMesh rooms, List<NLine> facade, List<NLine> circulation)
        {
            this.bounds = bounds;
            this.rooms = rooms;
            this.facade = facade;
            this.circulation = circulation;
            this.eval= new ApartmentResult();
        }

        // initialize apartment with all info from json
        public Apartment(string database, string id, string country, string city, string name, string jsonBounds, string jsonFacade, string jsonCirculation, string jsonSplit)
        {
            this.bounds = NFace.deserializeNFace(jsonBounds);
            this.facade = NLine.deserializeNLineList(jsonFacade);
            this.circulation = NLine.deserializeNLineList(jsonCirculation);
            this.eval = new ApartmentResult();

            NFace tempBounds = this.bounds.DeepCopy();
            NMesh inputMesh = new NMesh(tempBounds);
            inputMesh.SnapToAxis(0.01, 0.01);
            inputMesh.facesClockwise();

            /// Split bounds into rooms with split JSON
            string outstring = "";
            Convert.ToString(jsonSplit);
            outstring += jsonSplit.ToString();
            outstring.Trim();


            DataNode reverseNode = DataNode.deserializeDataNode(outstring);
            this.splitNode = reverseNode;

            NMesh outMesh = DataNode.subdivideWithMesh(reverseNode, inputMesh);

            outMesh.SnapToAxis(0.01, 0.01);


            List<string> roomNamesAll = new List<string>();
            List<string> roomIdAll = new List<string>();
            List<string> roomUnique = new List<string>();


            for (int i = 0; i < outMesh.faceList.Count; i++)
            {
                roomNamesAll.Add(outMesh.faceList[i].merge_id);
                roomIdAll.Add(outMesh.faceList[i].unique_id);
                roomUnique.AddRange(outMesh.faceList[i].neighbors_id);
            }


            this.rooms = outMesh;
            this.rooms.UpdateAdjacencyListWithFaceStrings();

            this.database = database;
            this.id = id;
            this.city = city;
            this.country = country;
            this.name = name;

        }

        // initialize apartment from json with split and list
        public Apartment(string jsonBounds, string jsonFacade, string jsonCirculation, string jsonSplit)
        {
            this.bounds = NFace.deserializeNFace(jsonBounds);
            this.facade = NLine.deserializeNLineList(jsonFacade);
            this.circulation = NLine.deserializeNLineList(jsonCirculation);
            this.eval = new ApartmentResult();

            NFace tempBounds = this.bounds.DeepCopy();
            NMesh inputMesh = new NMesh(tempBounds);
            inputMesh.SnapToAxis(0.01, 0.01);
            inputMesh.facesClockwise();

            /// Split bounds into rooms with split JSON
            string outstring = "";
            Convert.ToString(jsonSplit);
            outstring += jsonSplit.ToString();
            outstring.Trim();


            DataNode reverseNode = DataNode.deserializeDataNode(outstring);
            this.splitNode = reverseNode;

            NMesh outMesh = DataNode.subdivideWithMesh(reverseNode, inputMesh);

            outMesh.SnapToAxis(0.01, 0.01);


            List<string> roomNamesAll = new List<string>();
            List<string> roomIdAll = new List<string>();
            List<string> roomUnique = new List<string>();


            for (int i = 0; i < outMesh.faceList.Count; i++)
            {
                roomNamesAll.Add(outMesh.faceList[i].merge_id);
                roomIdAll.Add(outMesh.faceList[i].unique_id);
                roomUnique.AddRange(outMesh.faceList[i].neighbors_id);
            }


            this.rooms = outMesh;
            this.rooms.UpdateAdjacencyListWithFaceStrings();

        }

        public Apartment(NFace bounds, NMesh rooms, List<NLine> facade, List<NLine> circulation, List<Vec3d> connectionVecs)
        {
            this.bounds = bounds;
            //this.rooms = rooms;
            this.facade = facade;
            this.circulation = circulation;
            this.eval = new ApartmentResult();

            NFace tempBounds = this.bounds.DeepCopy();
            NMesh inputMesh = new NMesh(tempBounds);
            inputMesh.SnapToAxis(0.01, 0.01);
            inputMesh.facesClockwise();

            DataNode rootNode = DataNode.dataNodeFromNMeshAndBoundsWithConnectivity(rooms, bounds, connectionVecs);
            NMesh outMesh = DataNode.subdivideWithMesh(rootNode, inputMesh);
            this.splitNode = rootNode;

            outMesh.SnapToAxis(0.01, 0.01);


            List<string> roomNamesAll = new List<string>();
            List<string> roomIdAll = new List<string>();
            List<string> roomUnique = new List<string>();


            for (int i = 0; i < outMesh.faceList.Count; i++)
            {
                roomNamesAll.Add(outMesh.faceList[i].merge_id);
                roomIdAll.Add(outMesh.faceList[i].unique_id);
                roomUnique.AddRange(outMesh.faceList[i].neighbors_id);
            }

            this.rooms = outMesh;
            this.rooms.UpdateAdjacencyListWithFaceStrings();

            this.id = "default";
            this.database = "cloudtest";
            this.country = "USA";
            this.city = "Cambridge";
            this.name = "BTLab";

        }

        // initialize simplified apt without split json (useful for simplified analysis)
        public Apartment(string jsonBounds, string jsonFacade, string jsonCirculation)
        {
            this.bounds = NFace.deserializeNFace(jsonBounds);
            this.facade = NLine.deserializeNLineList(jsonFacade);
            this.circulation = NLine.deserializeNLineList(jsonCirculation);
            this.eval = new ApartmentResult();

            NFace tempBounds = this.bounds.DeepCopy();
            NMesh inputMesh = new NMesh(tempBounds);
            inputMesh.facesClockwise();

            this.rooms = inputMesh;
        }

        // initialize simplified apt without split json (useful for simplified analysis)
        public Apartment(NFace boundsIn, List<NLine> facadeIn, List<NLine> circulationIn)
        {
            this.bounds = boundsIn;
            this.facade = facadeIn;
            this.circulation = circulationIn;
            this.eval = new ApartmentResult();

            NFace tempBounds = this.bounds.DeepCopy();
            NMesh inputMesh = new NMesh(tempBounds);
            inputMesh.facesClockwise();

            this.rooms = inputMesh;
        }

        //////////////////////////////////////////////////////////////////////////////////
        /// POPULATE apartment

        public void populateApartmentWithFurniture(

              List<Furniture> Bed_furnitureBeds,
              List<Furniture> Bed_furnitureStorage,

              List<Furniture> Bath_furnitureToilet,
              List<Furniture> Bath_furnitureSink,
              List<Furniture> Bath_furnitureShower,

              List<Furniture> Living_furnitureSofa,
              List<Furniture> Living_furnitureDining,

              List<Furniture> Kitchen_furnitureKitchen,
              List<Furniture> Kitchen_furnitureDining
              )
            {
                bool extraConnectedX = false;
                List<string> roomNames = this.getRoomNames();

                double wallThickness = -0.075;
                double doorWidth = 0.8;
                double doorOffset = 0.2;
                double doorClearance = 1.5;
                bool leftOrient = true;

                //List<Room> allAptRoomsList = new List<Room>();
                List<NFace> doorClearanceFaceList = new List<NFace>();

                List<Room> roomsAllOutput = new List<Room>();
                List<NLine> validLinesAll = new List<NLine>();

                List<Room> aptRoomsList = new List<Room>();

                List<NLine> doorLines = this.doorsAll;
                List<NLine> circLines = this.circulation;
                doorLines.AddRange(circLines);

                List<NFace> blockedFaces = new List<NFace>();

                for (int i = 0; i < this.rooms.faceList.Count; i++)
                {
                    List<Room> allAptRoomsList = new List<Room>();

                    NFace bounds = this.rooms.faceList[i];
                    string program = this.rooms.faceList[i].merge_id;


                    List<NLine> boundsLines = RhConvert.NFaceToLineList(bounds);
                    List<NLine> doorLinesPerRoom = RIntersection.lineListBooleanIntersectionTol(doorLines, boundsLines, 0.01);

                    Room roomInput = new Room(program, bounds, doorLinesPerRoom, wallThickness, doorWidth, doorOffset, doorClearance, leftOrient);

                    if (roomInput.blocked.faceList != null)
                    {
                        doorClearanceFaceList.AddRange(roomInput.blocked.faceList);
                        blockedFaces.AddRange(roomInput.blocked.faceList);
                    }

                    // Fit Furniture in room

                    List<Room> allRooms = new List<Room>();
                    Room validRoom = roomInput.DeepCopy();

                    if (program == "bed")
                    {
                        // Check if extra is connected
                        NMesh extraMesh = this.rooms.filterNMeshByProperty("extra");
                        bool extraConnected = hasExtraConnected(roomInput.bounds, extraMesh, roomInput.doors);
                        extraConnectedX = extraConnected;

                        validRoom = Room.createBedRoom(validRoom, Bed_furnitureBeds, Bed_furnitureStorage, !extraConnected);
                        aptRoomsList.Add(validRoom);
                    }
                    else if (program == "kitchen")
                    {
                        validRoom = Room.createKitchen(validRoom, Kitchen_furnitureKitchen, Kitchen_furnitureDining);
                        aptRoomsList.Add(validRoom);
                    }
                    else if (program == "living")
                    {
                        validRoom = Room.createLivingRoom(validRoom, Living_furnitureSofa, Living_furnitureDining);
                        aptRoomsList.Add(validRoom);
                    }
                    else if (program == "bath")
                    {
                        validRoom = Room.createBathRoom(validRoom, Bath_furnitureToilet, Bath_furnitureSink, Bath_furnitureShower);
                        aptRoomsList.Add(validRoom);
                    }
                }

                this.roomList= aptRoomsList;

                //return currentApt;

            }

        public void populateApartmentWithFurnitureOLD(

              List<Furniture> Bed_furnitureBeds,
              List<Furniture> Bed_furnitureStorage,
              List<Furniture> Bed_furnitureTable,
              List<Furniture> Bed_furnitureExtra,

              List<Furniture> Bath_furnitureToilet,
              List<Furniture> Bath_furnitureSink,
              List<Furniture> Bath_furnitureShower,
              List<Furniture> Bath_furnitureLaundry,

              List<Furniture> Living_furnitureSofa,
              List<Furniture> Living_furnitureDining,

              List<Furniture> Kitchen_furnitureKitchen,
              List<Furniture> Kitchen_furnitureDining
              )
        {
            // DEPRECATED

            bool extraConnectedX = false;
            List<string> roomNames = this.getRoomNames();

            double wallThickness = -0.075;
            double doorWidth = 0.8;
            double doorOffset = 0.2;
            double doorClearance = 1.5;
            bool leftOrient = true;

            //List<Room> allAptRoomsList = new List<Room>();
            List<NFace> doorClearanceFaceList = new List<NFace>();

            List<Room> roomsAllOutput = new List<Room>();
            List<NLine> validLinesAll = new List<NLine>();

            List<Room> aptRoomsList = new List<Room>();

            List<NLine> doorLines = this.doorsAll;
            List<NLine> circLines = this.circulation;
            doorLines.AddRange(circLines);

            List<NFace> blockedFaces = new List<NFace>();

            for (int i = 0; i < this.rooms.faceList.Count; i++)
            {
                List<Room> allAptRoomsList = new List<Room>();

                NFace bounds = this.rooms.faceList[i];
                string program = this.rooms.faceList[i].merge_id;


                List<NLine> boundsLines = RhConvert.NFaceToLineList(bounds);
                List<NLine> doorLinesPerRoom = RIntersection.lineListBooleanIntersectionTol(doorLines, boundsLines, 0.01);

                Room roomInput = new Room(program, bounds, doorLinesPerRoom, wallThickness, doorWidth, doorOffset, doorClearance, leftOrient);

                if (roomInput.blocked.faceList != null)
                {
                    doorClearanceFaceList.AddRange(roomInput.blocked.faceList);
                    blockedFaces.AddRange(roomInput.blocked.faceList);
                }

                // Fit Furniture in room

                List<Room> allRooms = new List<Room>();
                Room validRoom = roomInput.DeepCopy();

                if (program == "bed")
                {
                    // Check if extra is connected
                    NMesh extraMesh = this.rooms.filterNMeshByProperty("extra");
                    bool extraConnected = hasExtraConnected(roomInput.bounds, extraMesh, roomInput.doors);
                    extraConnectedX = extraConnected;

                    validRoom = Room.createBedRoomOLD(validRoom, Bed_furnitureBeds, Bed_furnitureStorage, Bed_furnitureTable, Bed_furnitureExtra, !extraConnected);
                    aptRoomsList.Add(validRoom);
                }
                else if (program == "kitchen")
                {
                    validRoom = Room.createKitchen(validRoom, Kitchen_furnitureKitchen, Kitchen_furnitureDining);
                    aptRoomsList.Add(validRoom);
                }
                else if (program == "living")
                {
                    validRoom = Room.createLivingRoom(validRoom, Living_furnitureSofa, Living_furnitureDining);
                    aptRoomsList.Add(validRoom);
                }
                else if (program == "bath")
                {
                    validRoom = Room.createBathRoomOLD(validRoom, Bath_furnitureToilet, Bath_furnitureSink, Bath_furnitureShower, Bath_furnitureLaundry);
                    aptRoomsList.Add(validRoom);
                }
            }

            this.roomList = aptRoomsList;

            //return currentApt;

        }

        public static List<List<Room>> populateApartmentsWithFurnitureAllSolutions(Apartment currentApt,

            List<Furniture> Bed_furnitureBeds,
            List<Furniture> Bed_furnitureStorage,
            List<Furniture> Bed_furnitureTable,
            List<Furniture> Bed_furnitureExtra,

            List<Furniture> Bath_furnitureToilet,
            List<Furniture> Bath_furnitureSink,
            List<Furniture> Bath_furnitureShower,
            List<Furniture> Bath_furnitureLaundry,

            List<Furniture> Living_furnitureSofa,
            List<Furniture> Living_furnitureDining,

            List<Furniture> Kitchen_furnitureKitchen,
            List<Furniture> Kitchen_furnitureDining
            )
        {

            List<List<Room>> allAptsList = new List<List<Room>>();

            List<string> roomNames = currentApt.getRoomNames();
            double wallThickness = -0.075;
            double doorWidth = 0.8;
            double doorOffset = 0.2;
            double doorClearance = 1.5;
            bool leftOrient = true;

            int numBedrooms = currentApt.rooms.filterNMeshByProperty("bed").faceList.Count;
            currentApt.eval.numRooms = numBedrooms;

            //List<Room> allAptRoomsList = new List<Room>();
            List<NFace> doorClearanceFaceList = new List<NFace>();
            List<NLine> validLinesAll = new List<NLine>();


            List<NLine> doorLines = currentApt.doorsAll;
            List<NLine> circLines = currentApt.circulation;
            doorLines.AddRange(circLines);

            List<NFace> blockedFaces = new List<NFace>();

            // boolean to see if the bed was placed for studio apt. (otherwise multiple beds if multiple living spaces)
            bool studioBedExists = false;

            for (int i = 0; i < currentApt.rooms.faceList.Count; i++)
            {
                List<Room> roomIterationsList = new List<Room>();

                NFace bounds = currentApt.rooms.faceList[i];
                string program = currentApt.rooms.faceList[i].merge_id;


                List<NLine> boundsLines = RhConvert.NFaceToLineList(bounds);
                List<NLine> doorLinesPerRoom = RIntersection.lineListBooleanIntersectionTol(doorLines, boundsLines, 0.01);

                Room roomInput = new Room(program, bounds, doorLinesPerRoom, wallThickness, doorWidth, doorOffset, doorClearance, leftOrient);

                if (roomInput.blocked.faceList != null)
                {
                    doorClearanceFaceList.AddRange(roomInput.blocked.faceList);
                    blockedFaces.AddRange(roomInput.blocked.faceList);
                }

                // Fit Furniture in room

                List<Room> allRooms = new List<Room>();
                Room validRoom = roomInput.DeepCopy();

                if (program == "bed")
                {
                    // Check if extra is connected
                    NMesh extraMesh = currentApt.rooms.filterNMeshByProperty("extra");
                    bool extraConnected = hasExtraConnected(roomInput.bounds, extraMesh, doorLinesPerRoom);

                    allRooms = Room.createBedRoomAll(roomInput, Bed_furnitureBeds, Bed_furnitureStorage, Bed_furnitureTable, Bed_furnitureExtra, !extraConnected);
                    roomIterationsList.AddRange(allRooms);
                }
                else if (program == "kitchen")
                {
                    allRooms = Room.createKitchenAll(roomInput, Kitchen_furnitureKitchen, Kitchen_furnitureDining);
                    roomIterationsList.AddRange(allRooms);
                }
                else if (program == "living")
                {
                    if (numBedrooms == 0)
                    {
                        // create studio living room
                        Tuple<bool, List<Room>> placementTupleStudio = Room.createStudioLiving(studioBedExists, roomInput, Bed_furnitureBeds, Living_furnitureSofa, Living_furnitureDining);
                        allRooms = placementTupleStudio.Item2;
                        studioBedExists = placementTupleStudio.Item1;
                    }
                    else
                    {
                        // create normal living room
                        allRooms = Room.createLivingRoomAll(roomInput, Living_furnitureSofa, Living_furnitureDining);
                    }
                    roomIterationsList.AddRange(allRooms);
                }
                else if (program == "bath")
                {
                    allRooms = Room.createBathRoomAll(roomInput, Bath_furnitureToilet, Bath_furnitureSink, Bath_furnitureShower, Bath_furnitureLaundry);
                    roomIterationsList.AddRange(allRooms);
                }

                allAptsList.Add(roomIterationsList);
            }

            return allAptsList;

        }

        public static bool furnitureValidation(List<Room> inputRoomList, Apartment inputApartment)
        {
            bool furnitureValid = false;

            NMesh bedrooms = inputApartment.rooms.filterNMeshByProperty("bed");
            int numBedrooms = bedrooms.faceList.Count;
            inputApartment.eval.numRooms = numBedrooms;

            List<Furniture> furnitureList = new List<Furniture>();

            List<double> bathScore = new List<double>();
            List<double> bedScore = new List<double>();


            //////////////////////////////////////////////////////////////////////////////

            // Step: 01 Room based scores
            // it matters where furniture is placed, e.g. bath only counts if in bathroom '

            List<Room> sortedRooms = inputRoomList.OrderBy(o => o.Score).ToList();

            for (int i = 0; i < sortedRooms.Count; i++)
            {
                string program = sortedRooms[i].program;
            
                bool primary = true;
            
                if (program == "bed" && bedScore.Count > 0)
                    primary = false;
                if (program == "bath" && bathScore.Count > 0)
                    primary = false;
            
                double currentScore = Preset.InterpolateProgramScore(sortedRooms[i].Score, sortedRooms[i].program, numBedrooms, primary);
                furnitureList.AddRange(sortedRooms[i].furniture);

                if (program == "bed")
                    bedScore.Add(currentScore);
                else if (program == "bath")
                    bathScore.Add(currentScore);
            }

            // Bed Score
            bool minBed = false;
            bool masterBed = false;

            if (numBedrooms > 0)
            {
                // one has to have score 1 (fits double bed) , the rest can be lower
                minBed = RUtil.largerThan(bedScore, 0.5);
                masterBed = RUtil.checkForOne(bedScore, 1);
                inputApartment.eval.scoreBed = RUtil.average(bedScore);
            }
            else
            {
                // If studio apartment, search through all furniture to find bed.
                double bedTempScore = 0;
                for (int i = 0; i < furnitureList.Count; i++)
                {
                    if (furnitureList[i].name == "Bed")
                        bedTempScore += furnitureList[i].score;
                }

                double tempScore = Preset.InterpolateProgramScore(bedTempScore, "bed", 0);
                if (tempScore >= 1)
                {
                    minBed = true;
                    masterBed = true;
                }
                inputApartment.eval.scoreBed = tempScore;
            }

            // BathScore
            // one has to have score 1 (fits bath tub/toilet), the rest can be lower
            bool minBath = RUtil.largerThan(bathScore, 0.16);
            bool masterBath = RUtil.checkForOne(bathScore, 1);
            inputApartment.eval.scoreBath = RUtil.average(bathScore);

            //////////////////////////////////////////////////////////////////////////////

            // Step: 02 Furniture based scores
            // does not matter in what room the furniture is placed
            // e.g. kitchen room with dining table still counts as dining

            double diningScore = 0;
            double livingScore = 0;
            double kitchenScore = 0;

            double furnitureArea = 0;

            for (int i = 0; i < furnitureList.Count; i++)
            {
                furnitureArea += furnitureList[i].path.Area;
                
                if (furnitureList[i].name == "Dining")
                    diningScore += furnitureList[i].score;
                else if (furnitureList[i].name == "Kitchen")
                    kitchenScore += furnitureList[i].score;
                else if (furnitureList[i].name == "Living")
                    livingScore += furnitureList[i].score;
            }

            // Dining Score
            inputApartment.eval.scoreDining = Preset.InterpolateProgramScore(diningScore, "dining", bedrooms.faceList.Count);
            bool minDining = false;
            if (inputApartment.eval.scoreDining >= 1)
                minDining = true;

            // Living Score
            // one has to have score 1
            inputApartment.eval.scoreLiving = Preset.InterpolateProgramScore(livingScore, "living", bedrooms.faceList.Count);
            bool minLiving = false;
            if (inputApartment.eval.scoreLiving >= 1)
                minLiving = true;

            // Kitchen Score
            inputApartment.eval.scoreKitchen = Preset.InterpolateProgramScore(kitchenScore, "kitchen", bedrooms.faceList.Count);
            bool minKitchen = false;
            if (inputApartment.eval.scoreKitchen >= 1)
                minKitchen = true;


            if (minDining == true && minBed == true && masterBed == true && minLiving == true && minBed == true && masterBath == true && minKitchen == true)
                //if (minDining)
                furnitureValid = true;


            inputApartment.eval.scoreBoolFurniture = furnitureValid;

            

            List<double> scoreList = new List<double>(){
                inputApartment.eval.scoreKitchen,
                inputApartment.eval.scoreLiving,
                inputApartment.eval.scoreDining,
                inputApartment.eval.scoreBath,
                inputApartment.eval.scoreBed
                };

            double programScore = RUtil.average(scoreList);
            double areaMultiplier = Preset.areaMultiplierPreset(inputApartment.eval.area, inputApartment.eval.numRooms);
            inputApartment.eval.scoreProgram = programScore;

            double layoutLoss = Preset.layoutLoss(inputApartment.livingArea, furnitureArea);

            inputApartment.eval.scoreLoss = layoutLoss;
            inputApartment.eval.scoreLayout = programScore * areaMultiplier - layoutLoss;
            inputApartment.eval.scoreAreaMult= areaMultiplier;

            return furnitureValid;
        }

        //////////////////////////////////////////////////////////////////////////////////
        /// Apartment from unknown lines
        /// 

        //public static Apartment ApartmentFromUnknownLines(NFace bounds, List<>)

        //////////////////////////////////////////////////////////////////////////////////
        
        

        /// FIT apartment
        public static List<Apartment> fitApartment(string buildingName, int aptNum, List<Apartment> referenceList, Apartment targetApartment, double areaFilter = 0.15, bool cullAccess=true, bool sortByShape=true)
        {
            List<Apartment> chosenAreaRef = preFilterByArea(referenceList, targetApartment, areaFilter);
            List<Apartment> apartmentOut = createApartmentsFromReference(chosenAreaRef, targetApartment);

            if (cullAccess)
                apartmentOut = preFilterByAccessAndProgram(apartmentOut);

            if (sortByShape)
                apartmentOut = sortAptsByRoomShape(apartmentOut);

            // Add global properties
            for (int i = 0; i < apartmentOut.Count; i++)
            {
                apartmentOut[i].eval.building = buildingName;
                apartmentOut[i].eval.apartmentNum = aptNum;
            }
            return apartmentOut;
        }

        public static List<Apartment> fitApartmentParallel(string buildingName, int aptNum, List<Apartment> referenceList, Apartment targetApartment, bool cullAccess = true, bool sortByShape = true)
        {
            List<Apartment> chosenAreaRef = preFilterByArea(referenceList, targetApartment);
            List<Apartment> apartmentOut = createApartmentsFromReferenceParallel(chosenAreaRef, targetApartment);

            if (cullAccess)
                apartmentOut = preFilterByAccessAndProgram(apartmentOut);

            if (sortByShape)
                apartmentOut = sortAptsByRoomShape(apartmentOut);

            // Add global properties
            for (int i = 0; i < apartmentOut.Count; i++)
            {
                apartmentOut[i].eval.building = buildingName;
                apartmentOut[i].eval.apartmentNum = aptNum;
            }
            return apartmentOut;
        }

        public static List<Apartment> forceFitApartments(string bldg, int aptNum, List<Apartment> referenceApartments, Apartment targetApartment)
        {
            // GLOBAL VALUES
            double alpha = 1.0;

            List<Apartment> referenceList = referenceApartments;
            List<Apartment> outAptList = new List<Apartment>();

            // creates new apartments from reference list, and adds reference details to targetApt
            for (int i = 0; i < referenceApartments.Count; i++)
            {
                //int i = 0;
                Tuple<double, double> outTuple = Apartment.orientationScore(targetApartment, referenceList[i], false);
                double rotationOut = outTuple.Item2;
                double score = outTuple.Item1;

                NMesh inputMeshB = new NMesh(targetApartment.bounds);


                //Tuple<bool,NMesh> outMeshXT = DataNode.TrySubdivideWithMeshSafe(referenceList[i].splitNode, inputMeshB);
                //!!! NMesh outMeshX = DataNode.subdivideWithMesh(referenceList[i].splitNode, inputMeshB);

                //C = RhConvert.NMeshToRhLinePolylineList(outMeshXT.Item2);
                //!!! D = RhConvert.NMeshToRhLinePolylineList(outMeshX);

                // 02 Move target floorplan to Reference
                //Tuple<bool, Apartment> subdivisiontuple = Apartment.createRoomsWithReferenceAndRotSafe(targetApartment, referenceList[i], rotationOut);

                ////////////////////////////////////////////////////////////////


                Apartment targetApt = targetApartment;
                Apartment sourceApt = referenceList[i];
                double rotationInput = rotationOut;

                //////////////////////////////////////////////////////////

                NFace boundsX = targetApt.bounds;
                NFace bounds = boundsX.DeepCopy();
                bounds.updateEdgeConnectivity();

                double rotation = Math.Round(rotationInput, 10); // 5 10?
                double areaOld = bounds.Area;
                //Vec3d centroid = NFace.centroidInsideFace(bounds);
                Vec3d centroid = bounds.Centroid;

                Vec3d reverse = centroid.DeepCopy();
                reverse.Reverse();


                double areaTarget = sourceApt.bounds.Area;
                // get reference area
                double areaBounds = bounds.Area;

                //double areaSource = sourceApt.bounds.Area;

                // 1  rotate bounds to specified
                bounds.TranslateNFace(reverse);
                bounds.RotateNFaceAroundAxisRad(Vec3d.UnitZ, rotation);


                // 2 scale bounds to specified
                double factor = Math.Sqrt(areaTarget / bounds.Area);
                factor = Math.Ceiling(factor * 1000) / 1000;
                bounds.ScaleNFace(factor, Vec3d.Zero);

                // 3 do splitting operation
                bounds.updateEdgeConnectivity();
                NMesh inputMesh = new NMesh(bounds);

                //inputMesh.SnapToAxis(0.01, 0.01);
                DataNode reverseNode = sourceApt.splitNode; // DataNode.deserializeDataNode(outstring);


                //Tuple<bool, NMesh> subdivisionTuple = DataNode.TrySubdivideWithMesh(reverseNode, inputMesh); // reverseNode

                NMesh tempDivisionMesh = DataNode.subdivideWithMesh(referenceList[i].splitNode, inputMesh);

                bool divisionhappened = false;
                if (tempDivisionMesh.faceList.Count > 0)
                    divisionhappened = true;

                Tuple<bool, NMesh> subdivisionTuple = new Tuple<bool, NMesh>(divisionhappened, tempDivisionMesh);

                // Check if subdivision was successful and more than 1 room was created.  //  && subdivisionTuple.Item2.faceList.Count > 1
                if (subdivisionTuple.Item1)
                {
                    bool inbounds = NMesh.NMeshFacesInBounds(subdivisionTuple.Item2, bounds);

                    if (inbounds)
                    {
                        inputMesh.SnapToAxis(0.01, 0.01);

                        // 4 scale all back in reverse
                        inputMesh.scale(1 / factor, Vec3d.Zero);
                        bounds.ScaleNFace(1 / factor, Vec3d.Zero);

                        // 5 rotate all back in reverse
                        inputMesh.rotateAroundAxisRad(Vec3d.UnitZ, -rotation);
                        inputMesh.translate(centroid);
                        inputMesh.facesClockwise();

                        bounds.RotateNFaceAroundAxisRad(Vec3d.UnitZ, -rotation);
                        bounds.TranslateNFace(centroid);

                        //List<NLine> dupF = NLine.DeepCopyList(targetApt.facade);
                        //List<NLine> dupC = NLine.DeepCopyList(targetApt.circulation);

                        Apartment outApt = new Apartment(bounds, subdivisionTuple.Item2, targetApt.facade, targetApt.circulation);
                        outApt.splitNode = reverseNode;


                        outApt.updateMerged();
                        outApt.updateFacadeEval();
                        outApt.updateShapeDiffEval(sourceApt);
                        outApt.updateMerged();
                        outApt.updateDoorsWithOtherAdjacency(sourceApt);
                        outApt.updateFacade();
                        outApt.updateCirculation();

                        //outApt = snapUpdateApartmentLines(outApt, 0.15);
                        outApt.updateDoorsWithOtherAdjacency(sourceApt);
                        outApt.updateFacade();

                        outAptList.Add(outApt);
                    }
                }
            }


            for (int d = 0; d < outAptList.Count; d++)
            {
                outAptList[d].eval.currentBldg = bldg;
                outAptList[d].eval.currentApt = aptNum;
                outAptList[d].database = "reference";
                outAptList[d].id = "reference";
            }

            return outAptList;

        }
        public static List<Apartment> forceFitApartments(string bldg, int aptNum, List<Apartment> referenceApartments, NFace targetBounds, List<NLine> targetCirculation, List<NLine> targetFacade)
        {
            // GLOBAL VALUES

            NFace boundsTarget = targetBounds;
            double targetArea = boundsTarget.Area;
            List<NLine> circTarget = targetCirculation;
            List<NLine> facTarget = targetFacade;

            double alpha = 1.0;
            Apartment targetApartment = new Apartment(boundsTarget, facTarget, circTarget);

            List<Apartment> referenceList = referenceApartments;
            List<Apartment> outAptList = new List<Apartment>();

            // creates new apartments from reference list, and adds reference details to targetApt
            for (int i = 0; i < referenceApartments.Count; i++)
            {
                //int i = 0;
                Tuple<double, double> outTuple = Apartment.orientationScore(targetApartment, referenceList[i], false);
                double rotationOut = outTuple.Item2;
                double score = outTuple.Item1;

                NMesh inputMeshB = new NMesh(targetApartment.bounds);


                //Tuple<bool,NMesh> outMeshXT = DataNode.TrySubdivideWithMeshSafe(referenceList[i].splitNode, inputMeshB);
                //!!! NMesh outMeshX = DataNode.subdivideWithMesh(referenceList[i].splitNode, inputMeshB);

                //C = RhConvert.NMeshToRhLinePolylineList(outMeshXT.Item2);
                //!!! D = RhConvert.NMeshToRhLinePolylineList(outMeshX);

                // 02 Move target floorplan to Reference
                //Tuple<bool, Apartment> subdivisiontuple = Apartment.createRoomsWithReferenceAndRotSafe(targetApartment, referenceList[i], rotationOut);

                ////////////////////////////////////////////////////////////////


                Apartment targetApt = targetApartment;
                Apartment sourceApt = referenceList[i];
                double rotationInput = rotationOut;

                //////////////////////////////////////////////////////////

                NFace boundsX = targetApt.bounds;
                NFace bounds = boundsX.DeepCopy();
                bounds.updateEdgeConnectivity();

                double rotation = Math.Round(rotationInput, 10); // 5 10?
                double areaOld = bounds.Area;
                //Vec3d centroid = NFace.centroidInsideFace(bounds);
                Vec3d centroid = bounds.Centroid;

                Vec3d reverse = centroid.DeepCopy();
                reverse.Reverse();


                double areaTarget = sourceApt.bounds.Area;
                // get reference area
                double areaBounds = bounds.Area;

                //double areaSource = sourceApt.bounds.Area;

                // 1  rotate bounds to specified
                bounds.TranslateNFace(reverse);
                bounds.RotateNFaceAroundAxisRad(Vec3d.UnitZ, rotation);


                // 2 scale bounds to specified
                double factor = Math.Sqrt(areaTarget / bounds.Area);
                factor = Math.Ceiling(factor * 1000) / 1000;
                bounds.ScaleNFace(factor, Vec3d.Zero);

                // 3 do splitting operation
                bounds.updateEdgeConnectivity();
                NMesh inputMesh = new NMesh(bounds);

                //inputMesh.SnapToAxis(0.01, 0.01);
                DataNode reverseNode = sourceApt.splitNode; // DataNode.deserializeDataNode(outstring);


                //Tuple<bool, NMesh> subdivisionTuple = DataNode.TrySubdivideWithMesh(reverseNode, inputMesh); // reverseNode

                NMesh tempDivisionMesh = DataNode.subdivideWithMesh(referenceList[i].splitNode, inputMesh);

                bool divisionhappened = false;
                if (tempDivisionMesh.faceList.Count > 0)
                    divisionhappened = true;

                Tuple<bool, NMesh> subdivisionTuple = new Tuple<bool, NMesh>(divisionhappened, tempDivisionMesh);

                // Check if subdivision was successful and more than 1 room was created.  //  && subdivisionTuple.Item2.faceList.Count > 1
                if (subdivisionTuple.Item1)
                {
                    bool inbounds = NMesh.NMeshFacesInBounds(subdivisionTuple.Item2, bounds);

                    if (inbounds)
                    {
                        inputMesh.SnapToAxis(0.01, 0.01);

                        // 4 scale all back in reverse
                        inputMesh.scale(1 / factor, Vec3d.Zero);
                        bounds.ScaleNFace(1 / factor, Vec3d.Zero);

                        // 5 rotate all back in reverse
                        inputMesh.rotateAroundAxisRad(Vec3d.UnitZ, -rotation);
                        inputMesh.translate(centroid);
                        inputMesh.facesClockwise();

                        bounds.RotateNFaceAroundAxisRad(Vec3d.UnitZ, -rotation);
                        bounds.TranslateNFace(centroid);

                        //List<NLine> dupF = NLine.DeepCopyList(targetApt.facade);
                        //List<NLine> dupC = NLine.DeepCopyList(targetApt.circulation);

                        Apartment outApt = new Apartment(bounds, subdivisionTuple.Item2, targetApt.facade, targetApt.circulation);
                        outApt.splitNode = reverseNode;


                        outApt.updateMerged();
                        outApt.updateFacadeEval();
                        outApt.updateShapeDiffEval(sourceApt);
                        outApt.updateMerged();
                        outApt.updateDoorsWithOtherAdjacency(sourceApt);
                        outApt.updateFacade();
                        outApt.updateCirculation();

                        //outApt = snapUpdateApartmentLines(outApt, 0.15);
                        outApt.updateDoorsWithOtherAdjacency(sourceApt);
                        outApt.updateFacade();

                        outAptList.Add(outApt);
                    }
                }
            }


            for (int d = 0; d < outAptList.Count; d++)
            {
                outAptList[d].eval.currentBldg = bldg;
                outAptList[d].eval.currentApt = aptNum;
                outAptList[d].database = "reference";
                outAptList[d].id = "reference";
            }

            return outAptList;

        }

        public static List<Apartment> dropInPlaceApartment(string buildingName, int aptNum, List<Apartment> referenceList, Apartment targetApartment, List<double> rotationOverride = null)
        {
            // same as fit apartment, but does not take into account facade or circulation, use for something like single family home or outside circulation

            List<Apartment> apartmentOut = new List<Apartment>();

            // Assign default value if rotationOverride is null
            if (rotationOverride == null)
            {
                rotationOverride = new List<double> { 0, 90, 180, 270 };
            }

            for (int d = 0; d < rotationOverride.Count; d++)
            {
                List<Apartment> tempApts = createApartmentsFromReferenceExact(referenceList, targetApartment, rotationOverride[d]);

                apartmentOut.AddRange(tempApts);
            }

            // Add global properties
            for (int i = 0; i < apartmentOut.Count; i++)
            {
                apartmentOut[i].eval.building = buildingName;
                apartmentOut[i].eval.apartmentNum = aptNum;
            }
            return apartmentOut;
        }

        //////////////////////////////////////////////////////////////////////////////////

        public static Apartment snapUpdateApartmentLines(Apartment inputApartment, double snapDistance = 0.05)
        {
            // Input geometry
            NFace boundsFace = inputApartment.bounds;
            NMesh boundsMesh = new NMesh(boundsFace);

            // Snap Boundary and Apartments
            boundsMesh = NMesh.orthoSnapReduceMesh(boundsMesh, snapDistance);
            inputApartment.rooms = NMesh.orthoSnapMeshToAxisList(inputApartment.rooms, boundsMesh.axisList, snapDistance);

            // Snap Line lists to boundary
            List<NLine> boundsReferenceLines = NMesh.GetAllMeshLines(boundsMesh);

            List<NLine> facadeLinesUpdated = RIntersection.shatterWithDistance(boundsReferenceLines, inputApartment.facade);
            List<NLine> circulationLinesUpdated = RIntersection.shatterWithDistance(boundsReferenceLines, inputApartment.circulation);


            // Update Apartment
            inputApartment.bounds = boundsMesh.faceList[0];
            inputApartment.updateFacade(facadeLinesUpdated);
            inputApartment.updateCirculation(circulationLinesUpdated);

            inputApartment.updateMerged();
            inputApartment.updateFacadeEval();
            inputApartment.updateMerged();
            //inputApartment.updateDoorsWithOtherAdjacency(inputApartment);
            inputApartment.updateFacade();
            inputApartment.updateCirculation();

            return inputApartment;
        }

        public static Tuple<NMesh, List<NLine>, List<NLine>> simplifyApartmentBounds(double axisDist, NMesh inputMesh, List<NLine> facadeLineList, List<NLine> circulationLineList)
        {


            for (int i = 0; i < inputMesh.faceList[0].edgeList.Count; i++)
            {
                for (int j = 0; j < facadeLineList.Count; j++)
                {
                    NLine tempLine = new NLine(inputMesh.faceList[0].edgeList[i].v, inputMesh.faceList[0].edgeList[i].nextNEdge.v);
                    bool isEqual = NLine.IsNLineEqualTol(facadeLineList[j], tempLine, 0.01);
                    if (isEqual)
                    {
                        inputMesh.faceList[0].edgeList[i].property = "facade";
                    }
                }
            }

            for (int i = 0; i < inputMesh.faceList[0].edgeList.Count; i++)
            {
                for (int j = 0; j < circulationLineList.Count; j++)
                {
                    NLine tempLine = new NLine(inputMesh.faceList[0].edgeList[i].v, inputMesh.faceList[0].edgeList[i].nextNEdge.v);
                    bool isEqual = NLine.IsNLineEqualTol(circulationLineList[j], tempLine, 0.01);
                    if (isEqual)
                    {
                        inputMesh.faceList[0].edgeList[i].property = "circulation";
                    }
                }
            }


            // Snap Boundary and Apartments
            // Snap Line lists to boundary

            ////////////////////////////////////////////////////////////////////////////////////

            inputMesh.UpdateAxisTol(3, 0.01, 0.01);
            List<Axis> tempAxis = inputMesh.axisList;

            tempAxis = Axis.unifyAxisDirections(tempAxis, Vec3d.UnitX, Vec3d.UnitY);
            tempAxis = RDbscan.DbscanAxis(tempAxis, axisDist);
            inputMesh.axisList = tempAxis;

            Tuple<List<Axis>, List<Axis>> axisTuple = Axis.sortTwoAxisDirections(tempAxis, Vec3d.UnitX, Vec3d.UnitY);

            inputMesh = snapNMeshToAxisListTol(inputMesh, axisTuple.Item1, axisDist);
            inputMesh = snapNMeshToAxisListTol(inputMesh, axisTuple.Item2, axisDist);


            ////////////////////////////////////////////////////////////////////////////////////

            /**/

            //A = inputMesh.faceList[0].edgeList[0].property;

            List<NLine> boundsReferenceLines = NMesh.GetAllMeshLines(inputMesh);
            //boundsUpdated = RhConvert.NMeshToRhLinePolylineList(inputMesh);

            string outstring = "";

            for (int i = 0; i < inputMesh.faceList[0].edgeList.Count; i++)
            {
                outstring += i;
                outstring += " ";
                outstring += inputMesh.faceList[0].edgeList[i].property;
                outstring += "\n";
            }

            //B = outstring;

            List<NLine> facadeLinesOut = new List<NLine>();
            for (int i = 0; i < inputMesh.faceList[0].edgeList.Count; i++)
            {
                if (inputMesh.faceList[0].edgeList[i].property == "facade")
                {
                    NLine tempLine = new NLine(inputMesh.faceList[0].edgeList[i].v, inputMesh.faceList[0].edgeList[i].nextNEdge.v);
                    facadeLinesOut.Add(tempLine);
                }
                outstring += "\n";
            }
            //facadeUpdated = RhConvert.NLineListToRhLineCurveList(facadeLinesOut);

            List<NLine> circLinesOut = new List<NLine>();
            for (int i = 0; i < inputMesh.faceList[0].edgeList.Count; i++)
            {
                if (inputMesh.faceList[0].edgeList[i].property == "circulation")
                {
                    NLine tempLine = new NLine(inputMesh.faceList[0].edgeList[i].v, inputMesh.faceList[0].edgeList[i].nextNEdge.v);
                    circLinesOut.Add(tempLine);
                }
                outstring += "\n";
            }
            //circulationUpdated = RhConvert.NLineListToRhLineCurveList(circLinesOut);


            return new Tuple<NMesh, List<NLine>, List<NLine>>(inputMesh, facadeLinesOut, circLinesOut);
        }

        private static NMesh snapNMeshToAxisListTol(NMesh outMesh, List<Axis> tempAxis, double tol)
        {
            for (int i = 0; i < outMesh.faceList.Count; i++)
            {
                outMesh.faceList[i].updateEdgeConnectivity();

                // snap edges to closest axis
                for (int j = 0; j < outMesh.faceList[i].edgeList.Count; j++)
                {
                    //int j = iter;

                    Vec3d testVec = outMesh.faceList[i].edgeList[j].v;
                    List<Vec3d> closestVecs = new List<Vec3d>();
                    for (int k = 0; k < tempAxis.Count; k++)
                    {
                        Vec3d outVec = RIntersection.AxisClosestPoint2D(testVec, tempAxis[k]);
                        closestVecs.Add(outVec);
                    }
                    List<Vec3d> vecsSorted = closestVecs.OrderBy(o => Vec3d.Distance(o, testVec)).ToList();

                    if (Vec3d.Distance(vecsSorted[0], outMesh.faceList[i].edgeList[j].v) < tol)
                        outMesh.faceList[i].edgeList[j].v = vecsSorted[0]; // RIntersection.AxisClosestPoint2D(outMesh.faceList[i].edgeList[j].v, averagedAxisListSorted[0]);
                }
            }
            //outMesh = NMesh.deepCleaner(outMesh, 0.01);
            outMesh = NMesh.deepCleaner(outMesh, 0.01);

            return outMesh;
        }
        /////////////////////////////////////////////////////////////////////////////////


        public Apartment DeepCopyApartment()
        {
            NFace boundsCopy = this.bounds.DeepCopy();
            Matrix<double> adjacencyMatrixTemp = null;

            if (this.rooms.adjacencyMatrix != null)
            {
                adjacencyMatrixTemp = this.rooms.adjacencyMatrix.Clone();
            }
            else
            {
                // If the adjacency matrix is null, initialize it by updating with face strings
                this.rooms.UpdateAdjacencyListWithFaceStrings();
                this.rooms.UpdateAdjacencyMatrix();
                adjacencyMatrixTemp = this.rooms.adjacencyMatrix.Clone();
            }

            NMesh dupRooms = this.rooms.DeepCopyWithID();
            List<NLine> facadeCopy = NLine.DeepCopyList(this.facade);
            List<NLine> circulationCopy = NLine.DeepCopyList(this.circulation);

            Apartment deepCopyApt = new Apartment(boundsCopy, dupRooms, facadeCopy, circulationCopy);

            deepCopyApt.rooms.adjacencyMatrix = adjacencyMatrixTemp;

            deepCopyApt.rooms.UpdateAdjacencyListFromMatrix();
            deepCopyApt.database = this.database;
            deepCopyApt.id = this.id;
            deepCopyApt.country = this.country;
            deepCopyApt.city = this.city;
            deepCopyApt.name = this.name;

            return deepCopyApt;
        }

        /*
        public Apartment DeepCopyApartment()
        {
            NFace boundsCopy = this.bounds.DeepCopy();

            Matrix<double> adjacencyMatrixTemp = this.rooms.adjacencyMatrix.Clone();

            NMesh dupRooms = this.rooms.DeepCopyWithID();
            
            List<NLine> facadeCopy = NLine.DeepCopyList(this.facade);
            List<NLine> circulationCopy = NLine.DeepCopyList(this.circulation);

            Apartment deepCopyApt = new Apartment(boundsCopy, dupRooms, facadeCopy, circulationCopy);

            deepCopyApt.rooms.adjacencyMatrix= adjacencyMatrixTemp;

            deepCopyApt.rooms.UpdateAdjacencyListFromMatrix();
            deepCopyApt.database = this.database;
            deepCopyApt.id = this.id;
            deepCopyApt.country = this.country;
            deepCopyApt.city = this.city;   
            deepCopyApt.name = this.name;

            return deepCopyApt;
        }
        */

        public Apartment DeepCopyBoundsFacadeCirculation()
        {
            NFace boundsCopy = this.bounds.DeepCopy();

            List<NLine> facadeCopy = NLine.DeepCopyList(this.facade);
            List<NLine> circulationCopy = NLine.DeepCopyList(this.circulation);

            Apartment deepCopyApt = new Apartment(boundsCopy, facadeCopy, circulationCopy);
            return deepCopyApt;
        }

        //////////////////////////////////////////////////////////////////////////////////
        // Apartment Creation

        public static List<Apartment> createApartmentsFromReferenceExact(List<Apartment> referenceList, Apartment targetApartment, double rotationOut)
        {
            // creates new apartments from reference list, and adds reference details to targetApt

            List<Apartment> chosenApt = new List<Apartment>();

            for (int i = 0; i < referenceList.Count; i++)
            {

                // 02 Move target floorplan to Reference
                Tuple<bool, Apartment> subdivisiontuple = createRoomsWithReferenceAndRotSafe(targetApartment, referenceList[i], rotationOut);

                if (subdivisiontuple.Item1)
                {

                    subdivisiontuple.Item2.database = referenceList[i].database;
                    subdivisiontuple.Item2.id = referenceList[i].id;
                    subdivisiontuple.Item2.country = referenceList[i].country;
                    subdivisiontuple.Item2.city = referenceList[i].city;
                    subdivisiontuple.Item2.name = referenceList[i].name;
                    subdivisiontuple.Item2.referenceApartment = referenceList[i];
                    subdivisiontuple.Item2.referenceRotation = rotationOut;
                    subdivisiontuple.Item2.referenceOrientationScore = 0;
                    //Console.WriteLine(i + " | apt fitted..");

                    chosenApt.Add(subdivisiontuple.Item2);

                }
                else
                    Console.WriteLine(i + " | ERROR");

            }

            return chosenApt;
        }
        public static List<Apartment> createApartmentsFromReference(List<Apartment> referenceList, Apartment targetApartment)
        {
            // creates new apartments from reference list, and adds reference details to targetApt

            List<Apartment> chosenApt = new List<Apartment>();

            for (int i = 0; i < referenceList.Count; i++)
            {
                Tuple<double, double> outTuple = orientationScore(targetApartment, referenceList[i], false);
                double rotationOut = outTuple.Item2;
                double score = outTuple.Item1;

                // 02 Move target floorplan to Reference
                Tuple<bool, Apartment> subdivisiontuple = createRoomsWithReferenceAndRotSafe(targetApartment, referenceList[i], rotationOut);

                if (subdivisiontuple.Item1)
                {

                    subdivisiontuple.Item2.database = referenceList[i].database;
                    subdivisiontuple.Item2.id = referenceList[i].id;
                    subdivisiontuple.Item2.country = referenceList[i].country;
                    subdivisiontuple.Item2.city = referenceList[i].city;
                    subdivisiontuple.Item2.name = referenceList[i].name;
                    subdivisiontuple.Item2.referenceApartment = referenceList[i];
                    subdivisiontuple.Item2.referenceRotation = rotationOut;
                    subdivisiontuple.Item2.referenceOrientationScore = score;
                    Console.WriteLine(i + " | apt fitted..");

                    chosenApt.Add(subdivisiontuple.Item2);

                }
                else 
                    Console.WriteLine(i + " | ERROR");

            }

            return chosenApt;
        }

        public static Tuple<List<Apartment>, List<Apartment>, List<double>, List<double>> createApartmentsFromReferenceTupleOut(List<Apartment> referenceList, Apartment targetApartment)
        {
            // creates new apartments from reference list, and adds reference details to targetApt

            List<Apartment> chosenApt = new List<Apartment>();
            List<Apartment> referenceApt = new List<Apartment>();
            List<double> rotList = new List<double>();
            List<double> scoreList = new List<double>();

            for (int i = 0; i < referenceList.Count; i++)
            {
                Tuple<double, double> outTuple = Apartment.orientationScore(targetApartment, referenceList[i], false);
                double rotationOut = outTuple.Item2;
                double score = outTuple.Item1;

                // 02 Move target floorplan to Reference
                Tuple<bool, Apartment> subdivisiontuple = Apartment.createRoomsWithReferenceAndRotSafe(targetApartment, referenceList[i], rotationOut);

                if (subdivisiontuple.Item1)
                {
                    if (subdivisiontuple.Item2.eval.facadeAccess == true && subdivisiontuple.Item2.eval.circAccess == true && subdivisiontuple.Item2.eval.roomAccess == true)
                    {
                        subdivisiontuple.Item2.database = referenceList[i].database;
                        subdivisiontuple.Item2.id = referenceList[i].id;
                        subdivisiontuple.Item2.country = referenceList[i].country;
                        subdivisiontuple.Item2.city = referenceList[i].city;
                        subdivisiontuple.Item2.name = referenceList[i].name;

                        chosenApt.Add(subdivisiontuple.Item2);
                        referenceApt.Add(referenceList[i]);
                        rotList.Add(rotationOut);
                        scoreList.Add(score);
                    }
                }
            }

            return new Tuple<List<Apartment>, List<Apartment>, List<double>, List<double>>(chosenApt, referenceApt, rotList, scoreList);
        }


        public static List<Apartment> createApartmentsFromReferenceParallel(List<Apartment> referenceList, Apartment targetApartment)
        {
            List<Apartment> chosenApt = new List<Apartment>();

            // Use Parallel.For to parallelize the for loop
            Parallel.For(0, referenceList.Count, i =>
            {
                Tuple<double, double> outTuple = orientationScore(targetApartment, referenceList[i], false);
                double rotationOut = outTuple.Item2;
                double score = outTuple.Item1;

                Tuple<bool, Apartment> subdivisiontuple = createRoomsWithReferenceAndRotSafe(targetApartment, referenceList[i], rotationOut);

                if (subdivisiontuple.Item1)
                {
                    subdivisiontuple.Item2.database = referenceList[i].database;
                    subdivisiontuple.Item2.id = referenceList[i].id;
                    subdivisiontuple.Item2.country = referenceList[i].country;
                    subdivisiontuple.Item2.city = referenceList[i].city;
                    subdivisiontuple.Item2.name = referenceList[i].name;

                    Console.WriteLine(i + " | apt fitted..");

                    lock (chosenApt)
                    {
                        chosenApt.Add(subdivisiontuple.Item2);
                    }
                }
                else
                {
                    Console.WriteLine(i + " | ERROR");
                }
            });

            return chosenApt;
        }
        public static List<Apartment> sortAptsByRoomShape(List<Apartment> inputApts)
        {
            List<Apartment> aptOut = new List<Apartment>();

            List<double> apartmentRatioList = new List<double>();
            for (int i = 0; i < inputApts.Count; i++)
            {
                apartmentRatioList.Add(inputApts[i].eval.roomShapeDiffMax);
            }

            List<Apartment> apartementsSorted = RUtil.SortListByOtherList(inputApts, apartmentRatioList);

            return apartementsSorted;
        }

        // ROOM Creation
        public static Tuple<bool, Apartment> createRoomsWithReferenceAndRotSafe(Apartment targetApt, Apartment sourceApt, double rotationRadians)
        {

            NFace boundsX = targetApt.bounds;
            NFace bounds = boundsX.DeepCopy();
            bounds.updateEdgeConnectivity();

            double rotation = Math.Round(rotationRadians, 10); // 5 10?
            double areaOld = bounds.Area;
            //Vec3d centroid = NFace.centroidInsideFace(bounds);
            Vec3d centroid = bounds.Centroid;

            Vec3d reverse = centroid.DeepCopy();
            reverse.Reverse();


            double areaTarget = sourceApt.bounds.Area;
            // get reference area
            double areaBounds = bounds.Area;

            //double areaSource = sourceApt.bounds.Area;

            // 1  rotate bounds to specified
            bounds.TranslateNFace(reverse);
            bounds.RotateNFaceAroundAxisRad(Vec3d.UnitZ, rotation);


            // 2 scale bounds to specified
            double factor = Math.Sqrt(areaTarget / bounds.Area);
            factor = Math.Ceiling(factor * 1000) / 1000;
            bounds.ScaleNFace(factor, Vec3d.Zero);

            // 3 do splitting operation
            bounds.updateEdgeConnectivity();
            NMesh inputMesh = new NMesh(bounds);

            inputMesh.SnapToAxis(0.01, 0.01);
            DataNode reverseNode = sourceApt.splitNode; // DataNode.deserializeDataNode(outstring);


            Tuple<bool, NMesh> subdivisionTuple = DataNode.TrySubdivideWithMeshSafe(reverseNode, inputMesh); // reverseNode


            // Check if subdivision was successful and more than 1 room was created.  //  && subdivisionTuple.Item2.faceList.Count > 1
            if (subdivisionTuple.Item1)
            {
                bool inbounds = NMesh.NMeshFacesInBounds(subdivisionTuple.Item2, bounds);

                if (inbounds)
                {
                    inputMesh.SnapToAxis(0.01, 0.01);

                    // 4 scale all back in reverse
                    inputMesh.scale(1 / factor, Vec3d.Zero);
                    bounds.ScaleNFace(1 / factor, Vec3d.Zero);

                    // 5 rotate all back in reverse
                    inputMesh.rotateAroundAxisRad(Vec3d.UnitZ, -rotation);
                    inputMesh.translate(centroid);
                    inputMesh.facesClockwise();

                    bounds.RotateNFaceAroundAxisRad(Vec3d.UnitZ, -rotation);
                    bounds.TranslateNFace(centroid);

                    //List<NLine> dupF = NLine.DeepCopyList(targetApt.facade);
                    //List<NLine> dupC = NLine.DeepCopyList(targetApt.circulation);

                    Apartment outApt = new Apartment(bounds, subdivisionTuple.Item2, targetApt.facade, targetApt.circulation);
                    outApt.splitNode = reverseNode;

                    outApt.updateMerged();
                    outApt.updateFacadeEval();
                    outApt.updateShapeDiffEval(sourceApt);
                    outApt.updateMerged();
                    outApt.updateDoorsWithOtherAdjacency(sourceApt);
                    outApt.updateFacade();
                    outApt.updateCirculation();

                    return new Tuple<bool, Apartment>(true, outApt);
                }
            }

            return new Tuple<bool, Apartment>(false, targetApt);
            
        }

        // Reference
        public static NMesh createRoomsFromAptAndRot(NFace bounds, double rotationInput, Apartment sourceApt)
        {
            //Apartment outApt = new Apartment(bounds, facadeIn, circulationIn);
            bool isClockwise = false;
            if (bounds.IsClockwise == true)
                isClockwise = true;

            //Apartment aptRot = new Apartment(bounds, facadeIn, circulationIn, rotationInput, sourceApt);
            //Apartment aptRot = placement(bounds, facadeIn, circulationIn, rotationInput, sourceApt);
            double rotation = Math.Round(rotationInput, 5);
            double areaOld = bounds.Area;
            Vec3d centroid = NFace.centroidInsideFace(bounds);
            Vec3d reverse = centroid.DeepCopy();
            reverse.Reverse();
            //NMesh inputMesh = new NMesh(bounds);


            //string outstring = "";
            //Convert.ToString(jsonSplit);
            //outstring += jsonSplit.ToString();
            //outstring.Trim();
            //inputMesh.SnapToAxis(0.01, 0.01);


            double areaTarget = sourceApt.bounds.Area;

            // get reference area
            double areaBounds = bounds.Area;
            //double areaSource = sourceApt.bounds.Area;

            // 1  rotate bounds to specified

            bounds.TranslateNFace(reverse);
            bounds.RotateNFaceAroundAxisRad(Vec3d.UnitZ, rotation);


            // 2 scale bounds to specified
            double factor = Math.Sqrt(areaTarget / bounds.Area);
            factor = Math.Ceiling(factor * 1000) / 1000;
            bounds.ScaleNFace(factor, Vec3d.Zero);

            // 3 do splitting operation
            bounds.updateEdgeConnectivity();
            NMesh inputMesh = new NMesh(bounds);

            inputMesh.SnapToAxis(0.01, 0.01);
            DataNode reverseNode = sourceApt.splitNode; // DataNode.deserializeDataNode(outstring);


            NMesh outMesh = DataNode.subdivideWithMesh(reverseNode, inputMesh); // reverseNode
            inputMesh.SnapToAxis(0.01, 0.01);

            // 4 scale all back in reverse
            inputMesh.scale(1 / factor, Vec3d.Zero);
            bounds.ScaleNFace(1 / factor, Vec3d.Zero);

            // 5 rotate all back in reverse
            inputMesh.rotateAroundAxisRad(Vec3d.UnitZ, -rotation);
            inputMesh.translate(centroid);
            inputMesh.facesClockwise();

            bounds.RotateNFaceAroundAxisRad(Vec3d.UnitZ, -rotation);
            bounds.TranslateNFace(centroid);

            if (isClockwise == true)
                bounds.makeClockwise();
            else 
                bounds.makeCounterClockwise();

            ////// Construct apt
            //outApt.roomNMesh = inputMesh;
            return inputMesh;
        }
        public static Apartment createRoomsWithReferenceAndRot(Apartment targetApt, Apartment sourceApt, double rotationInput)
        {

            NFace boundsX = targetApt.bounds;
            NFace bounds = boundsX.DeepCopy();
            bounds.updateEdgeConnectivity();

            double rotation = Math.Round(rotationInput, 10); // 5 10?
            double areaOld = bounds.Area;
            //Vec3d centroid = NFace.centroidInsideFace(bounds);
            Vec3d centroid = bounds.Centroid;

            Vec3d reverse = centroid.DeepCopy();
            reverse.Reverse();


            double areaTarget = sourceApt.bounds.Area;
            // get reference area
            double areaBounds = bounds.Area;

            //double areaSource = sourceApt.bounds.Area;

            // 1  rotate bounds to specified
            bounds.TranslateNFace(reverse);
            bounds.RotateNFaceAroundAxisRad(Vec3d.UnitZ, rotation);


            // 2 scale bounds to specified
            double factor = Math.Sqrt(areaTarget / bounds.Area);
            factor = Math.Ceiling(factor * 1000) / 1000;
            bounds.ScaleNFace(factor, Vec3d.Zero);

            // 3 do splitting operation
            bounds.updateEdgeConnectivity();
            NMesh inputMesh = new NMesh(bounds);

            inputMesh.SnapToAxis(0.01, 0.01);
            DataNode reverseNode = sourceApt.splitNode; // DataNode.deserializeDataNode(outstring);


            NMesh outMesh = DataNode.subdivideWithMesh(reverseNode, inputMesh); // reverseNode
            inputMesh.SnapToAxis(0.01, 0.01);

            // 4 scale all back in reverse
            inputMesh.scale(1 / factor, Vec3d.Zero);
            bounds.ScaleNFace(1 / factor, Vec3d.Zero);

            // 5 rotate all back in reverse
            inputMesh.rotateAroundAxisRad(Vec3d.UnitZ, -rotation);
            inputMesh.translate(centroid);
            inputMesh.facesClockwise();

            bounds.RotateNFaceAroundAxisRad(Vec3d.UnitZ, -rotation);
            bounds.TranslateNFace(centroid);

            List<NLine> dupF = NLine.DeepCopyList(targetApt.facade);
            List<NLine> dupC = NLine.DeepCopyList(targetApt.circulation);

            Apartment outApt = new Apartment(bounds, outMesh, targetApt.facade, targetApt.circulation);
            outApt.splitNode = reverseNode;

            outApt.updateMerged();

            return outApt;
        }


        // Update functions
        public void updateRoomNMesh(NMesh inputMesh)
        {
            this.rooms = inputMesh;
        }
        public void updateFacade(List<NLine> facadeUpdated)
        {
            this.facade = facadeUpdated;
        }
        public void updateFacade()
        {
            // Update Facade based on current Facade and merged rooms

            for (int i = 0; i < this.roomsMerged.faceList.Count; i++)
            {
                string mergeIdTemp = this.roomsMerged.faceList[i].merge_id;

                this.roomsMerged.faceList[i] = NFace.deleteInternalLines(this.roomsMerged.faceList[i]);
                this.roomsMerged.faceList[i].merge_id = mergeIdTemp;

            }

            List<NLine> roomLinesListForFacade = new List<NLine>();

            for (int i = 0; i < this.roomsMerged.faceList.Count; i++)
            {
                List<NLine> currentRoomLines = RhConvert.NFaceToLineList(this.roomsMerged.faceList[i]);
                roomLinesListForFacade.AddRange(currentRoomLines);
            }
            List<NLine> intersectionFacTemp = RIntersection.lowTolBoolIntersection(roomLinesListForFacade, this.facade, 0.1, 0.05);

            this.facade = intersectionFacTemp;
            this.updateFacadeEval();
        }
        public void updateCirculation(List<NLine> circulationUpdated)
        {
            this.circulation = circulationUpdated;
            this.updateCircEval();
        }
        public void updateCirculation()
        {

            List<NFace> roomsWithCircAccess = new List<NFace>();

            List<NLine> roomLinesList = new List<NLine>();
            for (int i = 0; i < this.roomsMerged.faceList.Count; i++)
            {
                if (this.roomsMerged.faceList[i].merge_id == "living" || this.roomsMerged.faceList[i].merge_id == "foyer" || this.roomsMerged.faceList[i].merge_id == "kitchen")
                {
                    roomsWithCircAccess.Add(this.roomsMerged.faceList[i]);
                }
            }
            NMesh circRoomsMesh = new NMesh(roomsWithCircAccess);
            List<NLine> roomMergedLineList = NMesh.GetAllMeshLines(circRoomsMesh);
            List<NLine> intersectionCircTemp = RIntersection.lowTolBoolIntersection(roomMergedLineList, this.circulation, 0.05);
            this.circulation = intersectionCircTemp;


            /*
            for (int i = 0; i < this.roomsMerged.faceList.Count; i++)
            {
                string mergeIdTemp = this.roomsMerged.faceList[i].merge_id;

                this.roomsMerged.faceList[i] = NFace.deleteInternalLines(this.roomsMerged.faceList[i]);
                this.roomsMerged.faceList[i].merge_id = mergeIdTemp;

            }
            // Extraction STEP 3: Circulation

            // shattered circulation lines from all rooms --> for access
            // intersect circulation mesh with lines that go to merged mesh, living, foyer.
            List<NLine> roomLinesList = new List<NLine>();
            for (int i = 0; i < this.roomsMerged.faceList.Count; i++)
            {
                if (this.roomsMerged.faceList[i].merge_id == "living" || this.roomsMerged.faceList[i].merge_id == "foyer" || this.roomsMerged.faceList[i].merge_id == "kitchen")
                {
                    List<NLine> currentRoomLines = RhConvert.NFaceToLineList(this.roomsMerged.faceList[i]);
                    roomLinesList.AddRange(currentRoomLines);
                }
            }

            List<NLine> intersectionCircTemp = RIntersection.lowTolBoolIntersection(roomLinesList, this.circulation, 0.15);
            this.circulation=intersectionCircTemp;
            */


            // Update Eval Function
            this.updateCircEval();
        }
        public void updateInternalWalls(List<NLine> internalWallsUpdated)
        {
            this.internalWalls = internalWallsUpdated;
        }
        public void updateAdiabatic(List<NLine> adiabaticUpdated)
        {
            this.adiabatic = adiabaticUpdated;
        }  
        public void updateDoors(List<NLine> doorsUpdated)
        {
            this.doors = doorsUpdated;
        }
        public void updateDoorsWithAdjacency()
        {
            // Update doors?
            this.rooms.UpdateAdjacencyMatrixWithFaceStrings();

            List<NLine> doorLines = new List<NLine>();
            double minWidth = 1.0;


            List<bool> adjSuccess = new List<bool>();
            for (int i = 0; i < this.rooms.adjacencyList.Count; i++)
            {
                List<NLine> tempList = RhConvert.NFaceToLineList(this.rooms.faceList[i]);
                // get face start
                for (int j = 0; j < this.rooms.adjacencyList[i].Count; j++)
                {
                    // get face that should be adjacent
                    List<NLine> currentList = RhConvert.NFaceToLineList(this.rooms.faceList[this.rooms.adjacencyList[i][j]]);

                    // get intersection lines
                    List<NLine> intersectionLine = RIntersection.lineListBooleanIntersectionTol(tempList, currentList, 0.1);
                    doorLines.AddRange(intersectionLine);

                    bool minLengthBool = NLine.minLength(intersectionLine, minWidth);
                    adjSuccess.Add(minLengthBool);
                }
            }
            doorLines = NLine.deleteDuplicateLines(doorLines);
            this.doorsAll = doorLines;

            // 5d intersect door list with merged mesh to return only doors to rooms
            this.updateMerged();

            NMesh currentMerged = this.roomsMerged;
            List<NLine> linesMerged = NMesh.GetUniqueMeshLines(currentMerged, 0.01);
            List<NLine> intersectionDoorsMerged = RIntersection.lineListBooleanIntersectionTol(doorLines, linesMerged, 0.01);


            this.doors = intersectionDoorsMerged;
        }
        public void updateDoorsWithOtherAdjacency(Apartment referenceApartment)
        {
            double minWidth = 1.1;

            // 5a get adjacency from reference floorplan
            referenceApartment.rooms.UpdateAdjacencyMatrixWithFaceStrings();
            this.rooms.UpdateAdjacencyMatrix();
            // 5b check if ref connectivity is still valid
            bool connectivitySameAsRef = RMath.CheckMatrixCondition(referenceApartment.rooms.adjacencyMatrix, this.rooms.adjacencyMatrix);
            this.eval.roomAccess = connectivitySameAsRef;

            // 5c construct all door lines
            List<NLine> doorLines = new List<NLine>();

            List<bool> adjSuccess = new List<bool>();
            for (int i = 0; i < referenceApartment.rooms.adjacencyList.Count; i++)
            {
                // get face start
                List<NLine> tempList = RhConvert.NFaceToLineList(this.rooms.faceList[i]);
                for (int j = 0; j < referenceApartment.rooms.adjacencyList[i].Count; j++)
                {
                    // get face that should be adjacent
                    List<NLine> currentList = RhConvert.NFaceToLineList(this.rooms.faceList[referenceApartment.rooms.adjacencyList[i][j]]);

                    // get intersection lines
                    List<NLine> intersectionLine = RIntersection.lineListBooleanIntersectionTol(tempList, currentList, 0.1);
                    doorLines.AddRange(intersectionLine);

                    bool minLengthBool = NLine.minLength(intersectionLine, minWidth);
                    adjSuccess.Add(minLengthBool);
                }
            }
            doorLines = NLine.deleteDuplicateLines(doorLines);
            this.doorsAll = doorLines;

            // 5d intersect door list with merged mesh to return only doors to rooms
            NMesh currentMerged = this.roomsMerged;
            List<NLine> linesMerged = NMesh.GetUniqueMeshLines(currentMerged, 0.01);
            List<NLine> intersectionDoorsMerged = RIntersection.lineListBooleanIntersectionTol(doorLines, linesMerged, 0.01);

            this.doors = intersectionDoorsMerged;
        }
        public void updateJsonSplit(string jsonSplit)
        {
            /// Split bounds into rooms with split JSON
            string outstring = "";
            Convert.ToString(jsonSplit);
            outstring += jsonSplit.ToString();
            outstring.Trim();

            NFace tempBounds = this.bounds.DeepCopy();
            NMesh tempMesh = new NMesh(tempBounds);

            tempMesh.SnapToAxis(0.01, 0.01);


            DataNode reverseNode = DataNode.deserializeDataNode(outstring);
            NMesh outMesh = DataNode.subdivideWithMesh(reverseNode, tempMesh);

            outMesh.SnapToAxis(0.01, 0.01);


            List<string> roomNamesAll = new List<string>();
            List<string> roomIdAll = new List<string>();
            List<string> roomUnique = new List<string>();


            for (int i = 0; i < outMesh.faceList.Count; i++)
            {
                roomNamesAll.Add(outMesh.faceList[i].merge_id);
                roomIdAll.Add(outMesh.faceList[i].unique_id);
                roomUnique.AddRange(outMesh.faceList[i].neighbors_id);
            }

            this.rooms = outMesh;
            this.rooms.UpdateAdjacencyListWithFaceStrings();
        }

        // Filter

        public static List<Apartment> preFilterByArea(List<Apartment> apartmentList, Apartment targetApartment, double areaDiff=0.15)
        {
            // Start
            double targetArea = targetApartment.bounds.Area;
            //double areaDiff = 0.15;


            // B EVALUATE APT LIST
            List<Apartment> chosenApt = new List<Apartment>();

            List<bool> chosenBool = new List<bool>();

            for (int i = 0; i < apartmentList.Count; i++)
            {
                double refArea = apartmentList[i].bounds.Area;

                if (targetArea * (1 + areaDiff) > refArea && targetArea * (1 - areaDiff) < refArea)
                    chosenApt.Add(apartmentList[i]);
            }


            return chosenApt;
        }
        public static List<Apartment> preFilterByGeometry(List<Apartment> apartmentList, Apartment targetApartment)
        {
            double ratioCutoff = 0.2;

            List<Apartment> chosenApt = new List<Apartment>();

            List<bool> chosenBool = new List<bool>();
            List<double> ratioSort = new List<double>();



            for (int i = 0; i < apartmentList.Count; i++)
            {
                Tuple<double, double> outTuple = Apartment.orientationScore(targetApartment, apartmentList[i], false);
                double rotationOut = outTuple.Item2;
                double score = outTuple.Item1;

                List<string> names = apartmentList[i].getRoomNames();
                List<string> mergedNames = apartmentList[i].getRoomMergedNames();



                // Check if reference is valid
                if (NMesh.checkNanNMesh(apartmentList[i].rooms))
                {
                    // 02 Move target floorplan to Reference
                    Tuple<bool, Apartment> outAptTuple = createRoomsWithReferenceAndRotSafe(targetApartment, apartmentList[i], rotationOut);

                    if (outAptTuple.Item1)
                    {
                        Apartment outApt = outAptTuple.Item2;

                        chosenApt.Add(apartmentList[i]);
                        apartmentList[i].updateMerged();
                        outApt.updateDoorsWithOtherAdjacency(apartmentList[i]);
                        outApt.updateFacade();
                        outApt.updateCirculation();

                        // Filter 1 ratio cutoff
                        bool ratioFilterBool = true;
                        if (outApt.eval.roomShapeDiffMax < ratioCutoff)
                        {
                            ratioFilterBool = true;
                        }
                        else
                        {
                            ratioFilterBool = false;
                        }

                        // Filter 1 ratio cutoff && Filter 2 facade access && Filter 3 circulation access
                        if (ratioFilterBool && outApt.eval.circAccess && outApt.eval.facadeAccess)
                        {
                            chosenApt.Add(apartmentList[i]);
                        }
                    }

                }


            }

            return chosenApt;
        }
        public static List<Apartment> preFilterByAccessAndProgram(List<Apartment> inputApts)
        {
            List<Apartment> aptOut = new List<Apartment>();

            for (int i = 0; i < inputApts.Count; i++)
            {
                if (inputApts[i].eval.facadeAccess == true && inputApts[i].eval.circAccess == true)
                {
                    bool progValid = programValid(inputApts[i]);
                    if (progValid)
                    {
                        inputApts[i].eval.minSize = true;
                        aptOut.Add(inputApts[i]);
                    }
                }
            }
            return aptOut;
        }

        public static bool programValid(Apartment currentApt)
        {
            NMesh mergedRoomsCopy = currentApt.roomsMerged.DeepCopyWithID();
            NMesh allRoomsCopy = currentApt.rooms.DeepCopyWithID();

            //////////////////////////////////////////////////////////////////////////////////////
            // 1 BEDROOM

            NMesh bedMesh = mergedRoomsCopy.filterNMeshByProperty("bed");
            bool bedValid = false;

            if (bedMesh.faceList.Count == 0)
            {
                // studio, so get living area
                bedMesh = allRoomsCopy.filterNMeshByProperty("living");
                List<NFace> bedRects = NMesh.returnInsideRectListFromNMesh(bedMesh);
                bedValid = NFace.facesMinSize_MinOne(bedRects, 7.45, 2.45);

                if (bedValid == false)
                {
                    bedMesh = mergedRoomsCopy.filterNMeshByProperty("living");
                    bedRects = NMesh.returnInsideRectListFromNMesh(bedMesh);
                    bedValid = NFace.facesMinSize_MinOne(bedRects, 7.45, 2.45);
                }
            }
            else
            {
                List<NFace> bedRects = NMesh.returnInsideRectsFromNMesh(bedMesh);


                bedValid = NFace.facesMinSize_ALL(bedRects, 7.45, 2.2);

                // for less than 3 bedrooms
                if (bedMesh.faceList.Count == 1)
                {
                    bedValid = NFace.facesMinSize_ALL(bedRects, 7.45, 2.45);
                }
                else if (bedMesh.faceList.Count == 2)
                {
                    bedValid = NFace.facesMinSize_ALL(bedRects, 7.45, 2.45);
                }
                else
                {
                    bool allMust = NFace.facesMinSize_ALL(bedRects, 7.45, 2.15);
                    bool oneMust = NFace.facesMinSize_MinOne(bedRects, 7.45, 2.45);

                    if (allMust && oneMust)
                        bedValid = true;
                }
            }



            //////////////////////////////////////////////////////////////////////////////////////
            // 2 Bathroom

            NMesh bathMesh = mergedRoomsCopy.filterNMeshByProperty("bath");

            List<NFace> bathRects = NMesh.returnInsideRectsFromNMesh(bathMesh);
            bool bathValid = false;

            bathValid = NFace.facesMinSize_ALL(bathRects, 1.1, 1);

            // for less than 3 bedrooms
            if (bedMesh.faceList.Count == 1)
            {
                bool min0 = NFace.facesMinSize_ALL(bathRects, 3.19, 1.45);
                bool min1 = NFace.facesMinSize_ALL(bathRects, 3.39, 1.65);
                bool min2 = NFace.facesMinSize_ALL(bathRects, 3.53, 1.5);
                if (min0 || min1 || min2)
                    bathValid = true;
            }
            else if (bedMesh.faceList.Count > 1)
            {
                bool min0 = NFace.facesMinSize_MinOne(bathRects, 3.19, 1.45);
                bool min1 = NFace.facesMinSize_MinOne(bathRects, 3.39, 1.65);
                bool min2 = NFace.facesMinSize_MinOne(bathRects, 3.53, 1.5);
                if (min0 || min1 || min2)
                    bathValid = true;
            }


            if (bedValid && bathValid)
                return true;
            else
                return false;
        }

        // Evaluation Functions
        public void updateShapeDiffEval(Apartment referenceApartment)
        {
            Tuple<bool, double, double> ratioTuple = Apartment.ratioScore(this, referenceApartment);

            this.eval.roomShapeDiffAvg = ratioTuple.Item2;
            this.eval.roomShapeDiffMax = ratioTuple.Item3;
        }
        public void updateFacadeEval()
        {
            bool accessToFacade = true;

            // go through all merged rooms
            for (int i = 0; i < this.roomsMerged.faceList.Count; i++)
            {
                // if room is living, bed, check if access to facade
                if (this.roomsMerged.faceList[i].merge_id == "living" || this.roomsMerged.faceList[i].merge_id == "bed")
                {
                    List<NLine> currentRoomLines = RhConvert.NFaceToLineList(this.roomsMerged.faceList[i]);
                    List<NLine> boolFacList = RIntersection.lowTolBoolIntersection(currentRoomLines, this.facade, 0.5, 0.01);
                    if (boolFacList.Count <= 0)
                        accessToFacade = false;
                }
            }
            this.eval.facadeAccess = accessToFacade;
        }
        private void updateCircEval()
        {
            double minLength = 1.0;
            bool hasCircAccess = false;
            for (int i = 0; i < this.circulation.Count; i++)
            {
                if (this.circulation[i].Length >= minLength)
                    hasCircAccess = true;
            }
            this.eval.circAccess = hasCircAccess;
        }
        public void updateCircLossScore()
        { 
        
            
        }

        // Bool checker
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

        /// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Scoring
        public static Tuple<double, double> orientationScore(Apartment aptAIn, Apartment aptBIn, bool norm)
        {
            Apartment aptA = aptAIn.DeepCopyBoundsFacadeCirculation();
            Apartment aptB = aptBIn.DeepCopyBoundsFacadeCirculation();

            Tuple<List<int>, List<NFace>, List<List<NLine>>, List<List<NLine>>, List<double>> orientTuple = RComp.orientShapeWithFacadeAndCirc(aptA.bounds, aptA.circulation, aptA.facade, aptB.bounds, aptB.circulation, aptB.facade);

            List<NFace> boundsList = orientTuple.Item2;
            List<double> rotationList = orientTuple.Item5;

            List<List<NLine>> facList = orientTuple.Item3;
            List<List<NLine>> circList = orientTuple.Item4;

            // for each result, create a polar comparison based on facade orientation
            //bool norm = false;
            List<double> polarScoreList = new List<double>();


            List<int> facadeScoreList = new List<int>();

            for (int i = 0; i < facList.Count; i++)
            {
                double polarScore = RComp.polarHistComparison(aptB.bounds, aptB.facade, boundsList[i], facList[i], norm);
                int facadeCompScore = RComp.facadeDirectionCompScore(aptB.circulation, facList[i]);
                facadeScoreList.Add(facadeCompScore);
                polarScoreList.Add(polarScore);
            }

            List<double> scoreList = polarScoreList;
            List<int> sortedFacScore = RUtil.SortListByOtherList(facadeScoreList, scoreList);
            List<double> sortedRotation = RUtil.SortListByOtherList(rotationList, scoreList);
            //List<List<NLine>> sortedFacade = RUtil.SortListByOtherList(facList, scoreList);
            //List<NFace> sortedBounds = RUtil.SortListByOtherList(boundsList, scoreList);
            //List<List<NLine>> sortedCirc = RUtil.SortListByOtherList(circList, scoreList);

            List<double> SortedScores = polarScoreList.OrderBy(x => x).ToList();

            // return the item with the lowest score;
            double score = SortedScores[0];
            double rotation = sortedRotation[0];
            //double facadeScore = sortedFacScore[0];

            return new Tuple<double, double>(score, rotation);
        }

        public static Tuple<bool, double, double> ratioScore(Apartment aptA, Apartment aptRef)
        {
            NMesh meshCurrent = aptA.rooms;
            NMesh meshRef = aptRef.rooms;

            bool success = false;
            double avgScore = 0;
            double maxScore = 0;
            if (meshCurrent.faceList.Count == meshRef.faceList.Count)
            {
                List<double> allScores = new List<double>();

                for (int i = 0; i < meshCurrent.faceList.Count; i++)
                {
                    double outScore = RComp.faceShapeDifferenceScore(meshCurrent.faceList[i], meshRef.faceList[i]);
                    allScores.Add(outScore);
                }
                allScores.Sort();
                allScores.Reverse();
                maxScore = allScores[0];
                double scoreTotal = 0;
                for (int j = 0; j < allScores.Count; j++)
                {
                    scoreTotal += allScores[j];
                }

                success = true;
                avgScore = scoreTotal / allScores.Count;

            }


            return new Tuple<bool, double, double>(success, avgScore, maxScore);
        }

        public static List<double> ratioScorePerRoom(Apartment aptA, Apartment aptRef)
        {
            NMesh meshCurrent = aptA.rooms;
            NMesh meshRef = aptRef.rooms;
            List<double> allScores = new List<double>();

            if (meshCurrent.faceList.Count == meshRef.faceList.Count)
            {
                for (int i = 0; i < meshCurrent.faceList.Count; i++)
                {
                    double outScore = RComp.faceShapeDifferenceScore(meshCurrent.faceList[i], meshRef.faceList[i]);
                    allScores.Add(outScore);
                }
            }

            return allScores;
        }

        
        public void updateAreaExcessScore()
        {
            //  A positive A_e indicates excess area, a value of 0 or less than 0 indicates the excess area is below the 20 % area threshold.
            // A_e is the excess area (in m2) derived from subtracting the sum of all circulation areas F_n of all furniture objects inside
            // the apartment (extra rooms count as furniture, foyer rooms do not) from the total apartment area A_(apt )with a 20 % buffer. 

            double furnitureTotalArea = 0;
            for (int i = 0; i < this.roomList.Count; i++)
            {
                for (int j = 0; j < this.roomList[i].furniture.Count; j++)
                {
                    furnitureTotalArea += this.roomList[i].furniture[j].path.Area;
                }
            }

            for (int i = 0; i < this.rooms.faceList.Count; i++)
            {
                if (this.rooms.faceList[i].merge_id == "extra")
                    furnitureTotalArea += this.roomList[i].bounds.Area;
            }

            double apartmentArea = this.rooms.Area;

            double excess = apartmentArea * 0.8 - furnitureTotalArea;

            this.eval.excessArea = excess;
        }



        // Output
        public List<string> getRoomNames()
        {
            List<string> roomNamesAll = new List<string>();

            for (int i = 0; i < this.rooms.faceList.Count; i++)
            {
                roomNamesAll.Add(this.rooms.faceList[i].merge_id);
            }

            return roomNamesAll;
        }
        public List<string> getRoomMergedNames()
        {
            List<string> roomNamesAll = new List<string>();

            for (int i = 0; i < this.roomsMerged.faceList.Count; i++)
            {
                roomNamesAll.Add(this.roomsMerged.faceList[i].merge_id);
            }

            return roomNamesAll;
        }

        public void updateMerged()
        {
            NMesh tempRooms = this.rooms.DeepCopyWithID();

            //List<string> stringList = new List<string>() { "living", "kitchen" };
            //NMesh secondTry = RSplit.mergeNMeshByPropertyList(tempRooms, stringList); //this.rooms
            //List<string> stringList2 = new List<string>() { "foyer" };
            //secondTry = RSplit.mergeNMeshByPropertyList(secondTry, stringList2);
            //List<string> stringList3 = new List<string>() { "living", "foyer" };
            //secondTry = RSplit.mergeNMeshByPropertyList(secondTry, stringList3);

            List<string> stringList = new List<string>() { "living" };
            NMesh secondTry = RSplit.mergeNMeshByPropertyList(tempRooms, stringList); //this.rooms
            List<string> stringList2 = new List<string>() { "foyer" };
            secondTry = RSplit.mergeNMeshByPropertyList(secondTry, stringList2);
            List<string> stringList3 = new List<string>() { "kitchen" };
            secondTry = RSplit.mergeNMeshByPropertyList(secondTry, stringList3);
            

            List<string> stringList4 = new List<string>() { "living", "kitchen" };
            secondTry = RSplit.mergeNMeshByPropertyList(secondTry, stringList4); //this.rooms
            List<string> stringList5 = new List<string>() { "living", "foyer" };
            secondTry = RSplit.mergeNMeshByPropertyList(secondTry, stringList5);


            for (int i = 0; i < secondTry.faceList.Count; i++)
            {
                secondTry.faceList[i].shrinkFace();
                secondTry.faceList[i].shrinkFace();
                secondTry.faceList[i].shrinkFace();
            }

            for (int i = 0; i < secondTry.faceList.Count; i++)
            {
                bool counter = secondTry.faceList[i].IsClockwise;
                if (counter == false)
                    secondTry.faceList[i].flipRH();
            }

            this.roomsMerged = secondTry;
           
        }
        public Tuple<NMesh, List<string>> getMergedTuple()
        {
            List<string> stringList = new List<string>() { "living", "kitchen" };
            NMesh secondTry = RSplit.mergeNMeshByPropertyList(this.rooms, stringList);
            List<string> stringList2 = new List<string>() { "foyer" };
            secondTry = RSplit.mergeNMeshByPropertyList(secondTry, stringList2);
            List<string> stringList3 = new List<string>() { "living", "foyer" };
            secondTry = RSplit.mergeNMeshByPropertyList(secondTry, stringList3);

            for (int i = 0; i < secondTry.faceList.Count; i++)
            {
                secondTry.faceList[i].shrinkFace();
                secondTry.faceList[i].shrinkFace();
                secondTry.faceList[i].shrinkFace();
            }

            for (int i = 0; i < secondTry.faceList.Count; i++)
            {
                bool counter = secondTry.faceList[i].IsClockwise;
                if (counter == false)
                    secondTry.faceList[i].flipRH();
            }

            List<string> roomNames = new List<string>();

            for (int i = 0; i < secondTry.faceList.Count; i++)
            {
                roomNames.Add(secondTry.faceList[i].merge_id);
            }

            return new Tuple<NMesh, List<string>>(secondTry, roomNames);
        }
    }
}
