using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RGeoLib.BuildingSolver;
using Rhino.Geometry;

namespace RGeoLib
{
    public class RhConvert
    {
        ///////////////////////////////////////////////////////////////////////////
        // RGEOLIB INTERNAL
        public static List<Vec3d> NLineListToVec3dList(List<NLine> inputLines)
        {
            List<Vec3d> vecList = new List<Vec3d>();
            for (int i = 0; i < inputLines.Count; i++)
            {
                vecList.Add(inputLines[i].start);
            }
            return vecList;
        }

        public static List<NLine> NFaceToLineList(NFace f)
        {

            List<NLine> rh_crv_out = new List<NLine>();

            for (int j = 0; j < (f.edgeList.Count); j++)
            {
                int j2 = j + 1;
                if (j2 >= f.edgeList.Count)
                {
                    j2 = 0;
                }
                Point3d pointStart = new Point3d(f.edgeList[j].v.X, f.edgeList[j].v.Y, f.edgeList[j].v.Z);
                Point3d pointEnd = new Point3d(f.edgeList[j2].v.X, f.edgeList[j2].v.Y, f.edgeList[j2].v.Z);
                rh_crv_out.Add(new NLine(f.edgeList[j].v, f.edgeList[j2].v));
            }
            return rh_crv_out;
            //return lineList;
        }


        // Functions to convert between RGeoLib and Rhino
        // sorted by input


        ///////////////////////////////////////////////////////////////////////////
        // RGEOLIB

        // Vec3d
        public static List<Point3d> Vec3dListToRhPoint3dList(List<Vec3d> vecs)
        {
            List<Point3d> rh_pt_out = new List<Point3d>();

            for (int i = 0; i < vecs.Count; i++)
            {
                Point3d point = new Point3d(vecs[i].X, vecs[i].Y, vecs[i].Z);
                rh_pt_out.Add(point);
            }

            return rh_pt_out;
        }
        public static Point3d Vec3dToRhPoint(Vec3d vecs)
        {
            Point3d point = new Point3d(vecs.X, vecs.Y, vecs.Z);
            return point;
        }

        // NEdges
        public static List<Point3d> NEdgeListToRhPointList(List<NEdge> edges)
        {
            List<Point3d> rh_pt_out = new List<Point3d>();

            for (int j = 0; j < edges.Count; j++)
            {
                Point3d pointStart = new Point3d(edges[j].v.X, edges[j].v.Y, edges[j].v.Z);
                rh_pt_out.Add(pointStart);
            }

            return rh_pt_out;
        }

        // Axis

        public static List<LineCurve> AxisToRhCurve(Axis inputAxis, double length)
        {
            List<LineCurve> rh_crv_out = new List<LineCurve>();

            Vec3d startPt = inputAxis.v + length * inputAxis.dir;
            Vec3d endPt = inputAxis.v - length * inputAxis.dir;

            Point3d pointStart = new Point3d(startPt.X, startPt.Y, startPt.Z);
            Point3d pointMid = new Point3d(inputAxis.v.X, inputAxis.v.Y, inputAxis.v.Z);
            Point3d pointEnd = new Point3d(endPt.X, endPt.Y, endPt.Z);
            LineCurve rh_crv_1 = new LineCurve(pointStart, pointMid);
            LineCurve rh_crv_2 = new LineCurve(pointMid, pointEnd);

            rh_crv_out.Add(rh_crv_1);
            rh_crv_out.Add(rh_crv_2);

            return rh_crv_out;
        }
        public static List<LineCurve> AxisListToRhCurve(List<Axis> inputAxis, double length)
        {
            List<LineCurve> rh_crv_out = new List<LineCurve>();

            for (int i = 0; i < (inputAxis.Count); i++)
            {
                //Vec3d endPt = inputAxis[j].v + length * inputAxis[j].dir;
                //Point3d pointStart = new Point3d(inputAxis[j].v.X, inputAxis[j].v.Y, inputAxis[j].v.Z);
                //Point3d pointEnd = new Point3d(endPt.X, endPt.Y, endPt.Z);
                //rh_crv_out.Add(new LineCurve(pointStart, pointEnd));


                Vec3d startPt = inputAxis[i].v + length * inputAxis[i].dir;
                Vec3d endPt = inputAxis[i].v - length * inputAxis[i].dir;

                Point3d pointStart = new Point3d(startPt.X, startPt.Y, startPt.Z);
                Point3d pointMid = new Point3d(inputAxis[i].v.X, inputAxis[i].v.Y, inputAxis[i].v.Z);
                Point3d pointEnd = new Point3d(endPt.X, endPt.Y, endPt.Z);
                LineCurve rh_crv_1 = new LineCurve(pointStart, pointMid);
                LineCurve rh_crv_2 = new LineCurve(pointMid, pointEnd);

                rh_crv_out.Add(rh_crv_1);
                rh_crv_out.Add(rh_crv_2);

            }
            return rh_crv_out;
        }


        // NFace
        public static List<LineCurve> NFaceToRhLineCurveList(NFace f)
        {
            List<LineCurve> rh_crv_out = new List<LineCurve>();

            for (int j = 0; j < (f.edgeList.Count); j++)
            {
                int j2 = j + 1;
                if (j2 >= f.edgeList.Count)
                {
                    j2 = 0;
                }
                Point3d pointStart = new Point3d(f.edgeList[j].v.X, f.edgeList[j].v.Y, f.edgeList[j].v.Z);
                Point3d pointEnd = new Point3d(f.edgeList[j2].v.X, f.edgeList[j2].v.Y, f.edgeList[j2].v.Z);
                rh_crv_out.Add(new LineCurve(pointStart, pointEnd));
            }
            return rh_crv_out;
        }
        public static List<LineCurve> NFaceListToRhLineCurveList(List<NFace> faces)
        {
            List<LineCurve> rh_crv_out = new List<LineCurve>();

            for (int i = 0; i < faces.Count; i++)
            {
                NFace f = faces[i];
                f.updateEdgeConnectivity();
                for (int j = 0; j < f.edgeList.Count; j++)
                {
                    Point3d pointStart = new Point3d(f.edgeList[j].v.X, f.edgeList[j].v.Y, f.edgeList[j].v.Z);
                    Point3d pointEnd = new Point3d(f.edgeList[j].nextNEdge.v.X, f.edgeList[j].nextNEdge.v.Y, f.edgeList[j].nextNEdge.v.Z);
                    rh_crv_out.Add(new LineCurve(pointStart, pointEnd));
                }
            }

            return rh_crv_out;
        }
        public static Polyline NFaceToRhPolyline(NFace inputFace)
        {
            List<Point3d> tempPoint = NEdgeListToRhPointList(inputFace.edgeList);
            Point3d point0 = new Point3d(inputFace.edgeList[0].v.X, inputFace.edgeList[0].v.Y, inputFace.edgeList[0].v.Z);
            tempPoint.Add(point0);

            Polyline polyTemp = new Polyline(tempPoint);
            return polyTemp;

        }

        // NMesh
        public static List<Polyline> NMeshToRhLinePolylineList(NMesh inputMesh)
        {
            List<Polyline> linesPoly = new List<Polyline>();

            for (int m = 0; m < inputMesh.faceList.Count; m++)
            {
                Polyline polyTemp = NFaceToRhPolyline(inputMesh.faceList[m]);
                linesPoly.Add(polyTemp);
            }
            return linesPoly;
        }

        // NLine

        public static LineCurve NLineToRhLineCurve(NLine inputLine)
        {
            Point3d pointStart = new Point3d(inputLine.start.X, inputLine.start.Y, inputLine.start.Z);
            Point3d pointEnd = new Point3d(inputLine.end.X, inputLine.end.Y, inputLine.end.Z);

            return new LineCurve(pointStart, pointEnd);
        }
        public static List<LineCurve> NLineListToRhLineCurveList(List<NLine> lines)
        {
            List<LineCurve> rh_crv_out = new List<LineCurve>();

            for (int i = 0; i < lines.Count; i++)
            {
                Point3d pointStart = new Point3d(lines[i].start.X, lines[i].start.Y, lines[i].start.Z);
                Point3d pointEnd = new Point3d(lines[i].end.X, lines[i].end.Y, lines[i].end.Z);
                rh_crv_out.Add(new LineCurve(pointStart, pointEnd));
            }

            return rh_crv_out;
        }

        ///////////////////////////////////////////////////////////////////////////
        // Rhino

        // Rhino Point3d
        public static Vec3d RhPointToVec3d(Point3d rhPoint)
        {
            Vec3d vec = new Vec3d(rhPoint.X, rhPoint.Y, rhPoint.Z);
            return vec;
        }
        public static List<Vec3d> RhPointListToVec3dList(List<Point3d> rhPoints)
        {
            List<Vec3d> vecs = new List<Vec3d>();

            for (int i = 0; i < rhPoints.Count; i++)
            {
                Vec3d vec = new Vec3d(rhPoints[i].X, rhPoints[i].Y, rhPoints[i].Z);
                vecs.Add(vec);
            }

            return vecs;
        }
        public static NFace RhPointListToNFace(List<Point3d> RHpointList)
        {
            List<NEdge> edges = new List<NEdge>();

            foreach (Point3d point in RHpointList)
            {
                Vec3d tempVec = new Vec3d(point.X, point.Y, point.Z);
                NEdge tempEdge = new NEdge(tempVec);
                edges.Add(tempEdge);
            }

            NFace face = new NFace(edges);
            face.updateEdgeConnectivity();

            return face;
        }

        // NPolyLine
        public static Polyline NPolyLineToRhPolyline(NPolyLine inputPoly)
        {
            List<Vec3d> vecList = new List<Vec3d>();
            for (int i = 0; i < inputPoly.lineList.Count; i++)
            {
                vecList.Add(inputPoly.lineList[i].start);
            }
            vecList.Add(inputPoly.lineList[inputPoly.lineList.Count-1].end);
            
            List<Point3d> ptList =  Vec3dListToRhPoint3dList(vecList); 

            Polyline polyTemp = new Polyline(ptList);
            return polyTemp;

        }
        public static List<Polyline> NPolyLineListToRhPolylineList(List<NPolyLine> inputPolyList)
        {
            List<Polyline> polyList = new List<Polyline>();
            
            for (int i = 0; i < inputPolyList.Count; i++)
            {
                polyList.Add(NPolyLineToRhPolyline(inputPolyList[i]));
            }
            return polyList;
        }
       
        // Rhino Polyline
        public static List<NLine> RhPolylineToNLineList(Polyline inputRHPoly)
        {
            List<NLine> tempLinesOut = new List<NLine>();

            for (int j = 0; j < inputRHPoly.Count - 1; j++)
            {
                Vec3d tempStart = new Vec3d(inputRHPoly[j].X, inputRHPoly[j].Y, inputRHPoly[j].Z);
                Vec3d tempEnd = new Vec3d(inputRHPoly[j + 1].X, inputRHPoly[j + 1].Y, inputRHPoly[j + 1].Z);
                NLine tempLine = new NLine(tempStart, tempEnd);
                tempLinesOut.Add(tempLine);

            }

            return tempLinesOut;
        }
        public static NPolyLine RhPolylineToNPolyLine(Polyline inputRHPoly)
        {
            List<NLine> tempLinesOut = new List<NLine>();

            for (int j = 0; j < inputRHPoly.Count - 1; j++)
            {
                Vec3d tempStart = new Vec3d(inputRHPoly[j].X, inputRHPoly[j].Y, inputRHPoly[j].Z);
                Vec3d tempEnd = new Vec3d(inputRHPoly[j + 1].X, inputRHPoly[j + 1].Y, inputRHPoly[j + 1].Z);
                NLine tempLine = new NLine(tempStart, tempEnd);
                tempLinesOut.Add(tempLine);

            }

            NPolyLine polyTemp = new NPolyLine(tempLinesOut);
            return polyTemp;
        }
        public static List<NLine> RhPolylineListToNLineList(List<Polyline> inputRHPoly)
        {
            List<NLine> tempLinesOut = new List<NLine>();

            for (int i = 0; i < inputRHPoly.Count; i++)
            {
                for (int j = 0; j < inputRHPoly[i].Count - 1; j++)
                {
                    Vec3d tempStart = new Vec3d(inputRHPoly[i][j].X, inputRHPoly[i][j].Y, inputRHPoly[i][j].Z);
                    Vec3d tempEnd = new Vec3d(inputRHPoly[i][j + 1].X, inputRHPoly[i][j + 1].Y, inputRHPoly[i][j + 1].Z);
                    NLine tempLine = new NLine(tempStart, tempEnd);
                    tempLinesOut.Add(tempLine);

                }
            }

            return tempLinesOut;
        }

        public static List<NPolyLine> RhPolylineListToNPolyLineList(List<Polyline> inputRHPoly)
        {
            List<NPolyLine> polyLists = new List<NPolyLine>();

            for (int i = 0; i < inputRHPoly.Count; i++)
            {
                List<NLine> tempLinesOut = new List<NLine>();
                for (int j = 0; j < inputRHPoly[i].Count - 1; j++)
                {
                    Vec3d tempStart = new Vec3d(inputRHPoly[i][j].X, inputRHPoly[i][j].Y, inputRHPoly[i][j].Z);
                    Vec3d tempEnd = new Vec3d(inputRHPoly[i][j + 1].X, inputRHPoly[i][j + 1].Y, inputRHPoly[i][j + 1].Z);
                    NLine tempLine = new NLine(tempStart, tempEnd);
                    tempLinesOut.Add(tempLine);

                }
                NPolyLine tempPolyLine = new NPolyLine(tempLinesOut);
                polyLists.Add(tempPolyLine);
            }

            return polyLists;
        }
        public static NFace RhPolylineToNFace(Polyline inputPoly)
        {
            List<NEdge> tempEdges = new List<NEdge>();

            for (int j = 0; j < inputPoly.Count - 1; j++)
            {
                Vec3d tempVec = new Vec3d(inputPoly[j].X, inputPoly[j].Y, inputPoly[j].Z);
                NEdge tempEdge = new NEdge(tempVec);
                tempEdges.Add(tempEdge);
            }
            NFace tempFace = new NFace(tempEdges);
            tempFace.updateEdgeConnectivity();
            return tempFace;
        }
        public static List<NFace> RhPolylineListToNFacesList(List<Polyline> inputPolys)
        {
            List<NFace> allFaces = new List<NFace>();
            for (int i = 0; i < inputPolys.Count; i++)
            {
                List<NEdge> tempEdges = new List<NEdge>();

                for (int j = 0; j < inputPolys[i].Count - 1; j++)
                {
                    Vec3d tempVec = new Vec3d(inputPolys[i][j].X, inputPolys[i][j].Y, inputPolys[i][j].Z);
                    NEdge tempEdge = new NEdge(tempVec);
                    tempEdges.Add(tempEdge);
                }
                NFace tempFace = new NFace(tempEdges);
                tempFace.updateEdgeConnectivity();
                allFaces.Add(tempFace);
            }
            return allFaces;
        }
        public static NMesh RhPolylineListToNMesh(List<Polyline> inputPolys)
        {
            List<NFace> tempFaceList = RhPolylineListToNFacesList(inputPolys);
            return new NMesh(tempFaceList);
        }

        public static List<NFace> RhPolylineListToNFacesListWithNames(List<Polyline> inputPolys, List<string> inputNames)
        {
            List<NFace> allFaces = new List<NFace>();
            for (int i = 0; i < inputPolys.Count; i++)
            {
                List<NEdge> tempEdges = new List<NEdge>();

                for (int j = 0; j < inputPolys[i].Count - 1; j++)
                {
                    Vec3d tempVec = new Vec3d(inputPolys[i][j].X, inputPolys[i][j].Y, inputPolys[i][j].Z);
                    NEdge tempEdge = new NEdge(tempVec);
                    tempEdges.Add(tempEdge);
                }
                NFace tempFace = new NFace(tempEdges);
                tempFace.merge_id = inputNames[i];
                tempFace.updateEdgeConnectivity();
                allFaces.Add(tempFace);
            }
            return allFaces;
        }
        public static NMesh RhPolylineListToNMeshWithNames(List<Polyline> inputPolys, List<string> inputNames)
        {
            List<NFace> tempFaceList = RhPolylineListToNFacesListWithNames(inputPolys, inputNames);
            return new NMesh(tempFaceList);
        }

        // Rooms 
        public static List<LineCurve> RoomDrawFurniture(Room inputRoom)
        {
            List<NLine> tempLines = new List<NLine>();

            for (int i = 0; i < inputRoom.furniture.Count; i++)
            {
                tempLines.AddRange(inputRoom.furniture[i].drawing);
            }
            return NLineListToRhLineCurveList(tempLines);
        }
        public static List<LineCurve> RoomDrawFurniture(List<Room> inputRooms)
        {
            List<LineCurve> tempLines = new List<LineCurve>();

            for (int i = 0; i < inputRooms.Count; i++)
            {
                List<LineCurve> lines = RoomDrawFurniture(inputRooms[i]);
                tempLines.AddRange(lines);
            }
            return tempLines;
        }
        public static List<Polyline> RoomDrawPaths(Room inputRoom)
        {
            List<NFace> pathList = new List<NFace>();

            for (int i = 0; i < inputRoom.furniture.Count; i++)
            {
                pathList.Add(inputRoom.furniture[i].path);
            }
            NMesh pathMesh = new NMesh(pathList);

            return NMeshToRhLinePolylineList(pathMesh);
        }
        public static List<Polyline> RoomDrawPaths(List<Room> inputRooms)
        {
            List<Polyline> tempLines = new List<Polyline>();

            for (int i = 0; i < inputRooms.Count; i++)
            {
                List<Polyline> lines = RoomDrawPaths(inputRooms[i]);
                tempLines.AddRange(lines);
            }
            return tempLines;
        }
        public static List<Polyline> RoomDrawBlocked(Room inputRoom)
        {
            List<NFace> blockedList = new List<NFace>();

            for (int i = 0; i < inputRoom.furniture.Count; i++)
            {
                blockedList.Add(inputRoom.furniture[i].blocked);
            }
            NMesh blockedMesh = new NMesh(blockedList);

            return NMeshToRhLinePolylineList(inputRoom.blocked);
        }
        public static List<Polyline> RoomDrawBlocked(List<Room> inputRooms)
        {
            List<Polyline> tempLines = new List<Polyline>();

            for (int i = 0; i < inputRooms.Count; i++)
            {
                List<Polyline> lines = RoomDrawBlocked(inputRooms[i]);
                tempLines.AddRange(lines);
            }
            return tempLines;
        }

        // LIST TO TREE

        /*
        public static DataTree<T> ListOfListsToTree(List<List<T>> list)
        {
          DataTree<T> tree = new DataTree<T>();
          int i = 0;
          foreach(List<T> innerList in list)
          {
              tree.AddRange(innerList, new GH_Path(new int[]{0,i}));
              i++;
          }
          return tree;
        }
        */

    }
}
