using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FloorPlanGeneration.Geometry;
using FloorPlanGeneration.Schema;

namespace FloorPlanGeneration.Generation
{
    internal sealed class RoomTemplateGenerator
    {
        private readonly CleanedInput _input;
        private readonly double _tolerance;

        public RoomTemplateGenerator(CleanedInput input)
        {
            _input = input;
            _tolerance = input.Tolerance;
        }

        public void PopulateUnit(UnitLayout unit, CorridorStrategy corridor, UnitTypeTarget target)
        {
            Polygon2 unitPolygon = ToPolygon(unit.Polygon);
            Bounds2 bounds = unitPolygon.Bounds();
            if (corridor.Orientation == CorridorOrientation.Horizontal)
            {
                PopulateHorizontalUnit(unit, corridor, target, bounds);
            }
            else
            {
                PopulateVerticalUnit(unit, corridor, target, bounds);
            }

            unit.Rooms = unit.Rooms.OrderBy(r => r.Id, StringComparer.OrdinalIgnoreCase).ToList();
            unit.Score = Math.Round(UnitRoomScore(unit, target), 4);
        }

        public DoorOpening CreateUnitDoor(UnitLayout unit, CorridorStrategy corridor, int index)
        {
            Polygon2 polygon = ToPolygon(unit.Polygon);
            Bounds2 bounds = polygon.Bounds();
            Point2 location;
            if (corridor.Orientation == CorridorOrientation.Horizontal)
            {
                double y = Math.Abs(bounds.MinY - corridor.MaxY) <= Math.Abs(bounds.MaxY - corridor.MinY)
                    ? bounds.MinY
                    : bounds.MaxY;
                location = new Point2((bounds.MinX + bounds.MaxX) * 0.5, y);
            }
            else
            {
                double x = Math.Abs(bounds.MinX - corridor.MaxX) <= Math.Abs(bounds.MaxX - corridor.MinX)
                    ? bounds.MinX
                    : bounds.MaxX;
                location = new Point2(x, (bounds.MinY + bounds.MaxY) * 0.5);
            }

            return new DoorOpening
            {
                Id = "door-" + unit.Id,
                Location = location,
                Width = _input.Source.Rules.DoorWidth,
                HostWall = "wall-entry-" + unit.Id,
                ConnectsSpaces = new List<string> { unit.Id, corridor.Id }
            };
        }

        public IEnumerable<WallLayout> CreateUnitWalls(UnitLayout unit)
        {
            Polygon2 polygon = ToPolygon(unit.Polygon);
            int index = 0;
            foreach (LineSegment2 edge in polygon.Edges())
            {
                index++;
                yield return new WallLayout
                {
                    Id = index == 1 ? "wall-entry-" + unit.Id : "wall-" + unit.Id + "-" + index.ToString(CultureInfo.InvariantCulture),
                    Centerline = new LineInput { Id = "wall-" + unit.Id + "-" + index.ToString(CultureInfo.InvariantCulture), Start = edge.Start.Clone(), End = edge.End.Clone() },
                    Thickness = 0.18,
                    LayerType = "unit_demising"
                };
            }

            foreach (RoomLayout room in unit.Rooms)
            {
                Polygon2 roomPolygon = ToPolygon(room.Polygon);
                int roomEdgeIndex = 0;
                foreach (LineSegment2 edge in roomPolygon.Edges())
                {
                    roomEdgeIndex++;
                    if (GeometryPredicates.SharedSegmentLength(edge, polygon.Edges().First(), _tolerance) > _tolerance)
                    {
                        continue;
                    }

                    yield return new WallLayout
                    {
                        Id = "wall-" + room.Id + "-" + roomEdgeIndex.ToString(CultureInfo.InvariantCulture),
                        Centerline = new LineInput { Id = "wall-" + room.Id + "-" + roomEdgeIndex.ToString(CultureInfo.InvariantCulture), Start = edge.Start.Clone(), End = edge.End.Clone() },
                        Thickness = 0.10,
                        LayerType = "room_partition"
                    };
                }
            }
        }

        public IEnumerable<LabelLayout> CreateLabels(UnitLayout unit)
        {
            Polygon2 unitPolygon = ToPolygon(unit.Polygon);
            yield return new LabelLayout
            {
                Id = "label-" + unit.Id,
                TargetId = unit.Id,
                Text = unit.Id + " " + unit.Type + " " + Math.Round(unit.Area, 1).ToString(CultureInfo.InvariantCulture) + " m2",
                Location = unitPolygon.Centroid(),
                Layer = "FP::Generated::Labels"
            };

            foreach (RoomLayout room in unit.Rooms)
            {
                Polygon2 roomPolygon = ToPolygon(room.Polygon);
                yield return new LabelLayout
                {
                    Id = "label-" + room.Id,
                    TargetId = room.Id,
                    Text = room.RoomType + " " + Math.Round(room.Area, 1).ToString(CultureInfo.InvariantCulture) + " m2",
                    Location = roomPolygon.Centroid(),
                    Layer = "FP::Generated::Labels"
                };
            }
        }

        private void PopulateHorizontalUnit(UnitLayout unit, CorridorStrategy corridor, UnitTypeTarget target, Bounds2 b)
        {
            double width = b.Width;
            double depth = b.Height;
            double wetDepth = WetDepth(depth);
            bool aboveCorridor = b.MinY >= corridor.MaxY - _tolerance;
            double wetMinY = aboveCorridor ? b.MinY : b.MaxY - wetDepth;
            double wetMaxY = aboveCorridor ? b.MinY + wetDepth : b.MaxY;
            double facadeMinY = aboveCorridor ? wetMaxY : b.MinY;
            double facadeMaxY = aboveCorridor ? b.MaxY : wetMinY;

            string type = NormalizeUnitType(unit.Type);
            if (type == "studio")
            {
                double bathWidth = Clamp(width * 0.36, _input.Source.Rules.MinRoomWidth, Math.Min(3.4, width * 0.55));
                AddRoom(unit, "bathroom", b.MinX, wetMinY, b.MinX + bathWidth, wetMaxY, false);
                AddRoom(unit, "kitchen", b.MinX + bathWidth, wetMinY, b.MaxX, wetMaxY, false);
                AddRoom(unit, "living_sleeping", b.MinX, facadeMinY, b.MaxX, facadeMaxY, true);
                return;
            }

            if (type == "two_bed")
            {
                double bedWidth = Math.Max(_input.Source.Rules.MinRoomWidth, width * 0.29);
                double livingStart = b.MinX + (bedWidth * 2.0);
                if (b.MaxX - livingStart < _input.Source.Rules.MinRoomWidth)
                {
                    bedWidth = width / 3.0;
                    livingStart = b.MinX + (bedWidth * 2.0);
                }

                AddRoom(unit, "bathroom", b.MinX, wetMinY, b.MinX + Math.Min(3.2, width * 0.32), wetMaxY, false);
                AddRoom(unit, "kitchen", b.MinX + Math.Min(3.2, width * 0.32), wetMinY, b.MaxX, wetMaxY, false);
                AddRoom(unit, "bedroom", b.MinX, facadeMinY, b.MinX + bedWidth, facadeMaxY, true);
                AddRoom(unit, "bedroom", b.MinX + bedWidth, facadeMinY, livingStart, facadeMaxY, true);
                AddRoom(unit, "living", livingStart, facadeMinY, b.MaxX, facadeMaxY, true);
                return;
            }

            double bedroomWidth = Clamp(width * 0.42, _input.Source.Rules.MinRoomWidth + 0.4, Math.Max(_input.Source.Rules.MinRoomWidth, width - _input.Source.Rules.MinRoomWidth));
            AddRoom(unit, "bathroom", b.MinX, wetMinY, b.MinX + Math.Min(3.2, width * 0.38), wetMaxY, false);
            AddRoom(unit, "kitchen", b.MinX + Math.Min(3.2, width * 0.38), wetMinY, b.MaxX, wetMaxY, false);
            AddRoom(unit, "bedroom", b.MinX, facadeMinY, b.MinX + bedroomWidth, facadeMaxY, true);
            AddRoom(unit, "living", b.MinX + bedroomWidth, facadeMinY, b.MaxX, facadeMaxY, true);
        }

        private void PopulateVerticalUnit(UnitLayout unit, CorridorStrategy corridor, UnitTypeTarget target, Bounds2 b)
        {
            double width = b.Width;
            double depth = b.Height;
            double wetDepth = WetDepth(width);
            bool rightOfCorridor = b.MinX >= corridor.MaxX - _tolerance;
            double wetMinX = rightOfCorridor ? b.MinX : b.MaxX - wetDepth;
            double wetMaxX = rightOfCorridor ? b.MinX + wetDepth : b.MaxX;
            double facadeMinX = rightOfCorridor ? wetMaxX : b.MinX;
            double facadeMaxX = rightOfCorridor ? b.MaxX : wetMinX;

            string type = NormalizeUnitType(unit.Type);
            if (type == "studio")
            {
                double bathDepth = Clamp(depth * 0.36, _input.Source.Rules.MinRoomDepth, Math.Min(3.4, depth * 0.55));
                AddRoom(unit, "bathroom", wetMinX, b.MinY, wetMaxX, b.MinY + bathDepth, false);
                AddRoom(unit, "kitchen", wetMinX, b.MinY + bathDepth, wetMaxX, b.MaxY, false);
                AddRoom(unit, "living_sleeping", facadeMinX, b.MinY, facadeMaxX, b.MaxY, true);
                return;
            }

            if (type == "two_bed")
            {
                double bedDepth = Math.Max(_input.Source.Rules.MinRoomDepth, depth * 0.29);
                double livingStart = b.MinY + (bedDepth * 2.0);
                if (b.MaxY - livingStart < _input.Source.Rules.MinRoomDepth)
                {
                    bedDepth = depth / 3.0;
                    livingStart = b.MinY + (bedDepth * 2.0);
                }

                AddRoom(unit, "bathroom", wetMinX, b.MinY, wetMaxX, b.MinY + Math.Min(3.2, depth * 0.32), false);
                AddRoom(unit, "kitchen", wetMinX, b.MinY + Math.Min(3.2, depth * 0.32), wetMaxX, b.MaxY, false);
                AddRoom(unit, "bedroom", facadeMinX, b.MinY, facadeMaxX, b.MinY + bedDepth, true);
                AddRoom(unit, "bedroom", facadeMinX, b.MinY + bedDepth, facadeMaxX, livingStart, true);
                AddRoom(unit, "living", facadeMinX, livingStart, facadeMaxX, b.MaxY, true);
                return;
            }

            double bedroomDepth = Clamp(depth * 0.42, _input.Source.Rules.MinRoomDepth + 0.4, Math.Max(_input.Source.Rules.MinRoomDepth, depth - _input.Source.Rules.MinRoomDepth));
            AddRoom(unit, "bathroom", wetMinX, b.MinY, wetMaxX, b.MinY + Math.Min(3.2, depth * 0.38), false);
            AddRoom(unit, "kitchen", wetMinX, b.MinY + Math.Min(3.2, depth * 0.38), wetMaxX, b.MaxY, false);
            AddRoom(unit, "bedroom", facadeMinX, b.MinY, facadeMaxX, b.MinY + bedroomDepth, true);
            AddRoom(unit, "living", facadeMinX, b.MinY + bedroomDepth, facadeMaxX, b.MaxY, true);
        }

        private void AddRoom(UnitLayout unit, string roomType, double minX, double minY, double maxX, double maxY, bool expectsDaylight)
        {
            Polygon2 polygon = Polygon2.Rectangle(unit.Id + "-" + roomType + "-" + (unit.Rooms.Count + 1).ToString(CultureInfo.InvariantCulture), minX, minY, maxX, maxY);
            bool daylight = expectsDaylight && FacadeAnalyzer.HasDaylightExposure(polygon, _input.Floorplate, _input.Source.Facade, _tolerance);
            Bounds2 bounds = polygon.Bounds();
            RoomLayout room = new RoomLayout
            {
                Id = polygon.SourceId,
                UnitId = unit.Id,
                RoomType = roomType,
                Polygon = GeometryCleaner.ToPolygonInput(polygon),
                Area = Math.Round(polygon.Area(), 4),
                Dimensions = new SpaceDimensions { Width = Math.Round(bounds.Width, 4), Depth = Math.Round(bounds.Height, 4) },
                Daylight = daylight
            };
            unit.Rooms.Add(room);
        }

        private double WetDepth(double totalDepth)
        {
            double minDepth = Math.Max(2.2, _input.Source.Rules.MinRoomDepth);
            double wetDepth = Math.Min(3.2, Math.Max(minDepth, totalDepth * 0.35));
            if (totalDepth - wetDepth < _input.Source.Rules.MinRoomDepth)
            {
                wetDepth = Math.Max(1.8, totalDepth - _input.Source.Rules.MinRoomDepth);
            }

            return wetDepth;
        }

        private static double UnitRoomScore(UnitLayout unit, UnitTypeTarget target)
        {
            if (unit.Area <= 0.0)
            {
                return 0.0;
            }

            double desired = (target.MinArea + target.MaxArea) * 0.5;
            double areaFit = 1.0 - Math.Min(1.0, Math.Abs(unit.Area - desired) / Math.Max(desired, 1.0));
            double roomCoverage = Math.Min(1.0, unit.Rooms.Sum(r => r.Area) / unit.Area);
            return Math.Max(0.0, Math.Min(1.0, (areaFit * 0.45) + (roomCoverage * 0.55)));
        }

        private static string NormalizeUnitType(string type)
        {
            if (string.Equals(type, "2-bed", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(type, "2_bed", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(type, "two-bedroom", StringComparison.OrdinalIgnoreCase))
            {
                return "two_bed";
            }

            if (string.Equals(type, "1-bed", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(type, "1_bed", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(type, "one-bedroom", StringComparison.OrdinalIgnoreCase))
            {
                return "one_bed";
            }

            if (string.Equals(type, "studio", StringComparison.OrdinalIgnoreCase))
            {
                return "studio";
            }

            return "one_bed";
        }

        private static double Clamp(double value, double min, double max)
        {
            if (max < min)
            {
                return min;
            }

            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private static Polygon2 ToPolygon(PolygonInput input)
        {
            List<Point2> points = input.Points.Select(p => p.Clone()).ToList();
            if (points.Count > 1 && points[0].EqualsWithin(points[points.Count - 1], 1e-9))
            {
                points.RemoveAt(points.Count - 1);
            }

            return new Polygon2(input.Id, points);
        }
    }
}
