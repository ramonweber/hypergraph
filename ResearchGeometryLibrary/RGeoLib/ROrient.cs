using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGeoLib
{
    public class ROrient
    {
        public static Tuple<NFace, NLine, List<NLine>> moveNFaceToLineAlignmentWithPoint(NFace insertFace, NLine selectLine, Vec3d targetPoint, int alignment, int edgeNum = 0)
        {
            //alignment 0 = left
            //alignment 1 = middle
            //alignment 2 = right

            // aligns to closest point on line

            Vec3d pointOnCurve = RIntersection.LineClosestPoint2D(targetPoint, selectLine.start, selectLine.end);

            ////////////////////////////////////////////////////////////////////
            // 1 Positioning

            int numEdges = insertFace.edgeList.Count;

            if (edgeNum >= numEdges)
            {
                edgeNum = edgeNum % insertFace.edgeList.Count;
            }

            int edgeNumEnd = edgeNum + 1;
            if (edgeNumEnd >= numEdges)
                edgeNumEnd = 0;

            Vec3d bottomLine = selectLine.end - selectLine.start;
            Vec3d rootLine = selectLine.end;

            // get bottom corner
            Vec3d bottomLeftV = insertFace.edgeList[edgeNum].v;
            Vec3d bottomRightV = insertFace.edgeList[edgeNumEnd].v;

            Vec3d bottomVec = bottomRightV - bottomLeftV;


            // rotate box to same angle

            double angleRad = Vec3d.Angle2PI_2d(bottomVec, bottomLine);
            double angleDeg = RMath.ToDegrees(angleRad);

            Vec3d axis = new Vec3d(0, 0, 1);

            Vec3d translationVec = rootLine;

            // move to 0,0,0
            Vec3d moveTo0 = Vec3d.Zero - bottomLeftV;
            insertFace.TranslateNFace(moveTo0);


            // at 0,0,0 translate to target
            insertFace.RotateNFaceAroundAxisRad(axis, angleRad + Math.PI);
            insertFace.TranslateNFace(translationVec);


            ////////////////////////////////////////////////////////////////////
            // 2. Alignment

            List<NLine> faceLines = RhConvert.NFaceToLineList(insertFace);
            NLine locLine = faceLines[edgeNum];

            // Alignment left
            if (alignment == 0)
            {
                Vec3d moveToLeft = selectLine.end - locLine.start;
                //moveToLeft = moveToLeft - pointOnCurve;  // selectLine.end - ; //locLine.start;
                insertFace.TranslateNFace(moveToLeft);

                Vec3d alignWithPoint = pointOnCurve - selectLine.end;
                insertFace.TranslateNFace(alignWithPoint);

            }

            // Alignment centroid
            if (alignment == 1)
            {
                Vec3d moveToCentroid = selectLine.MidPoint - locLine.MidPoint;
                insertFace.TranslateNFace(moveToCentroid);
                //Vec3d moveToCentroid = selectLine.MidPoint  - pointOnCurve; //  - locLine.MidPoint;

                Vec3d alignWithPoint = pointOnCurve - selectLine.MidPoint;
                insertFace.TranslateNFace(alignWithPoint);
            }

            // Alignment Right
            if (alignment == 2)
            {
                Vec3d moveToRight = selectLine.start - locLine.end;
                //Vec3d moveToRight = selectLine.start  - pointOnCurve; // - locLine.end;
                insertFace.TranslateNFace(moveToRight);

                Vec3d alignWithPoint = pointOnCurve - selectLine.start;
                insertFace.TranslateNFace(alignWithPoint);
            }

            List<NLine> faceLinesTemp = RhConvert.NFaceToLineList(insertFace);
            NLine tempLine = faceLinesTemp[edgeNum];
            Tuple<string, List<NLine>> remainingBottomLineTuple = RIntersection.curveBooleanDifference(selectLine.start, selectLine.end, tempLine.start, tempLine.end);

            double remainingLineLength = selectLine.Length - bottomVec.Mag;
            Vec3d newBottomLine = Vec3d.ScaleToLen(bottomLine, remainingLineLength);
            NLine restLine = new NLine(selectLine.start, selectLine.start + newBottomLine);
            // new NLine(selectLine.start, selectLine.start - bottomLine);

            return new Tuple<NFace, NLine, List<NLine>>(insertFace, tempLine, remainingBottomLineTuple.Item2);
        }
        public static Tuple<NFace, NLine, List<NLine>> moveNFaceToLineAlignment(NFace insertFace, NLine selectLine, int alignment, int edgeNum = 0)
        {
            //alignment 0 = left
            //alignment 1 = middle
            //alignment 2 = right

            // moves nface at edge[ edgenum ] to line

            ////////////////////////////////////////////////////////////////////
            // 1 Positioning

            int numEdges = insertFace.edgeList.Count;

            if (edgeNum >= numEdges)
            {
                edgeNum = edgeNum % insertFace.edgeList.Count;
            }

            int edgeNumEnd = edgeNum + 1;
            if (edgeNumEnd >= numEdges)
                edgeNumEnd = 0;

            Vec3d bottomLine = selectLine.end - selectLine.start;
            Vec3d rootLine = selectLine.end;

            // get bottom corner
            Vec3d bottomLeftV = insertFace.edgeList[edgeNum].v;
            Vec3d bottomRightV = insertFace.edgeList[edgeNumEnd].v;

            Vec3d bottomVec = bottomRightV - bottomLeftV;

            // rotate box to same angle

            double angleRad = Vec3d.Angle2PI_2d(bottomVec, bottomLine);
            double angleDeg = RMath.ToDegrees(angleRad);

            Vec3d axis = new Vec3d(0, 0, 1);

            Vec3d translationVec = rootLine;

            // move to 0,0,0
            Vec3d moveTo0 = Vec3d.Zero - bottomLeftV;
            insertFace.TranslateNFace(moveTo0);


            // at 0,0,0 translate to target
            insertFace.RotateNFaceAroundAxisRad(axis, angleRad + Math.PI);
            insertFace.TranslateNFace(translationVec);


            ////////////////////////////////////////////////////////////////////
            // 2. Alignment

            List<NLine> faceLines = RhConvert.NFaceToLineList(insertFace);
            NLine locLine = faceLines[edgeNum];

            // Alignment left
            if (alignment == 0)
            {
                Vec3d moveToLeft = selectLine.end - locLine.start;
                insertFace.TranslateNFace(moveToLeft);
            }

            // Alignment centroid
            if (alignment == 1)
            {
                Vec3d moveToCentroid = selectLine.MidPoint - locLine.MidPoint;
                insertFace.TranslateNFace(moveToCentroid);
            }

            // Alignment Right
            if (alignment == 2)
            {
                Vec3d moveToRight = selectLine.start - locLine.end;
                insertFace.TranslateNFace(moveToRight);
            }

            List<NLine> faceLinesTemp = RhConvert.NFaceToLineList(insertFace);
            NLine tempLine = faceLinesTemp[edgeNum];
            Tuple<string, List<NLine>> remainingBottomLineTuple = RIntersection.curveBooleanDifference(selectLine.start, selectLine.end, tempLine.start, tempLine.end);

            return new Tuple<NFace, NLine, List<NLine>>(insertFace, tempLine, remainingBottomLineTuple.Item2);
        }
    }
}
