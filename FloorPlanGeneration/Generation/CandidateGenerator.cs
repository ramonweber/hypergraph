using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FloorPlanGeneration.Geometry;
using FloorPlanGeneration.Schema;
using FloorPlanGeneration.Topology;

namespace FloorPlanGeneration.Generation
{
    internal sealed class CandidateGenerator
    {
        private readonly CleanedInput _input;
        private readonly UnitMixPlanner _mixPlanner;
        private readonly RoomTemplateGenerator _roomGenerator;
        private readonly double _tolerance;

        public CandidateGenerator(CleanedInput input)
        {
            _input = input;
            _mixPlanner = new UnitMixPlanner(input.Source.Program);
            _roomGenerator = new RoomTemplateGenerator(input);
            _tolerance = input.Tolerance;
        }

        public UnitMixPlanner MixPlanner
        {
            get { return _mixPlanner; }
        }

        public List<LayoutVariant> Generate()
        {
            int requested = _input.Source.GenerationSettings != null ? _input.Source.GenerationSettings.VariantCount : 8;
            requested = Math.Max(1, Math.Min(20, requested));
            List<LayoutVariant> variants = new List<LayoutVariant>();

            for (int i = 0; i < requested; i++)
            {
                int seed = CombineSeed(_input.Source.Project.Seed, i);
                SeededRandom random = new SeededRandom(seed);
                List<Diagnostic> diagnostics = new List<Diagnostic>();
                CorridorStrategy corridor = TryResolveCorridor(i, random, diagnostics);
                LayoutVariant variant = BuildVariant(i, seed, corridor, random, diagnostics);
                variants.Add(variant);
            }

            return variants;
        }

        private LayoutVariant BuildVariant(int index, int seed, CorridorStrategy corridor, SeededRandom random, List<Diagnostic> diagnostics)
        {
            LayoutVariant variant = new LayoutVariant
            {
                VariantId = "variant-" + (index + 1).ToString("00", CultureInfo.InvariantCulture),
                Seed = seed,
                Status = "candidate"
            };
            variant.Diagnostics.AddRange(diagnostics);

            if (corridor == null)
            {
                variant.Status = "failed";
                variant.Diagnostics.Add(Diagnostic.Error("generation.corridor_missing", "Could not derive a corridor strategy inside the cleaned floorplate."));
                return variant;
            }

            Polygon2 corridorPolygon = CorridorPolygon(corridor);
            variant.Corridors.Add(new CorridorLayout
            {
                Id = corridor.Id,
                Polygon = GeometryCleaner.ToPolygonInput(corridorPolygon),
                Centerline = CorridorCenterline(corridor),
                Width = corridor.Width,
                Connections = CorridorConnections(corridorPolygon)
            });

            Dictionary<string, int> currentCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            List<Polygon2> placedUnits = new List<Polygon2>();

            if (corridor.Orientation == CorridorOrientation.Horizontal)
            {
                AddHorizontalBand(variant, corridor, _input.Floorplate.Bounds().MinY, corridor.MinY, "south", placedUnits, currentCounts, random);
                AddHorizontalBand(variant, corridor, corridor.MaxY, _input.Floorplate.Bounds().MaxY, "north", placedUnits, currentCounts, random);
            }
            else
            {
                AddVerticalBand(variant, corridor, _input.Floorplate.Bounds().MinX, corridor.MinX, "west", placedUnits, currentCounts, random);
                AddVerticalBand(variant, corridor, corridor.MaxX, _input.Floorplate.Bounds().MaxX, "east", placedUnits, currentCounts, random);
            }

            if (variant.Units.Count == 0)
            {
                variant.Status = "failed";
                variant.Diagnostics.Add(Diagnostic.Error("generation.no_units", "No valid unit bays could be placed adjacent to the corridor."));
                variant.Topology = BuildTopology(variant, corridorPolygon);
                return variant;
            }

            int doorIndex = 0;
            foreach (UnitLayout unit in variant.Units)
            {
                UnitTypeTarget target = _mixPlanner.FindTarget(unit.Type);
                _roomGenerator.PopulateUnit(unit, corridor, target);
                variant.Rooms.AddRange(unit.Rooms);
                variant.DoorsOpenings.Add(_roomGenerator.CreateUnitDoor(unit, corridor, doorIndex++));
                variant.Walls.AddRange(_roomGenerator.CreateUnitWalls(unit));
                variant.Labels.AddRange(_roomGenerator.CreateLabels(unit));
                variant.Corridors[0].Connections.Add(unit.Id);
            }

            variant.Topology = BuildTopology(variant, corridorPolygon);
            return variant;
        }

        private void AddHorizontalBand(
            LayoutVariant variant,
            CorridorStrategy corridor,
            double bandMinY,
            double bandMaxY,
            string side,
            List<Polygon2> placedUnits,
            Dictionary<string, int> currentCounts,
            SeededRandom random)
        {
            double depth = bandMaxY - bandMinY;
            if (depth < Math.Max(5.0, _input.Source.Rules.MinRoomDepth * 1.8))
            {
                variant.Diagnostics.Add(Diagnostic.Warning("generation.band_too_shallow", "Skipped " + side + " unit band because usable depth is below room minimums."));
                return;
            }

            double midY = (bandMinY + bandMaxY) * 0.5;
            List<Interval1D> intervals = GeometryPredicates.HorizontalInsideIntervals(_input.Floorplate, midY, _tolerance);
            foreach (Interval1D interval in intervals)
            {
                double start = Math.Max(interval.Start, corridor.MinX);
                double end = Math.Min(interval.End, corridor.MaxX);
                if (end - start <= MinAnyUnitWidth() + _tolerance)
                {
                    continue;
                }

                SplitHorizontalInterval(variant, corridor, start, end, bandMinY, bandMaxY, side, placedUnits, currentCounts, random);
            }
        }

        private void AddVerticalBand(
            LayoutVariant variant,
            CorridorStrategy corridor,
            double bandMinX,
            double bandMaxX,
            string side,
            List<Polygon2> placedUnits,
            Dictionary<string, int> currentCounts,
            SeededRandom random)
        {
            double depth = bandMaxX - bandMinX;
            if (depth < Math.Max(5.0, _input.Source.Rules.MinRoomWidth * 1.8))
            {
                variant.Diagnostics.Add(Diagnostic.Warning("generation.band_too_shallow", "Skipped " + side + " unit band because usable depth is below room minimums."));
                return;
            }

            double midX = (bandMinX + bandMaxX) * 0.5;
            List<Interval1D> intervals = GeometryPredicates.VerticalInsideIntervals(_input.Floorplate, midX, _tolerance);
            foreach (Interval1D interval in intervals)
            {
                double start = Math.Max(interval.Start, corridor.MinY);
                double end = Math.Min(interval.End, corridor.MaxY);
                if (end - start <= MinAnyUnitWidth() + _tolerance)
                {
                    continue;
                }

                SplitVerticalInterval(variant, corridor, start, end, bandMinX, bandMaxX, side, placedUnits, currentCounts, random);
            }
        }

        private void SplitHorizontalInterval(
            LayoutVariant variant,
            CorridorStrategy corridor,
            double minX,
            double maxX,
            double minY,
            double maxY,
            string side,
            List<Polygon2> placedUnits,
            Dictionary<string, int> currentCounts,
            SeededRandom random)
        {
            double x = minX;
            double depth = maxY - minY;
            int safety = 0;
            while (maxX - x > MinAnyUnitWidth() + _tolerance && safety++ < 50)
            {
                double remaining = maxX - x;
                double bayAreaHint = Math.Min(remaining, 8.0 + random.Range(-1.0, 1.0)) * depth;
                string type = _mixPlanner.ChooseUnitType(bayAreaHint, currentCounts, _input.Source.GenerationSettings.WeightedVariation, random);
                UnitTypeTarget target = _mixPlanner.FindTarget(type);
                double desiredWidth = ((target.MinArea + target.MaxArea) * 0.5) / Math.Max(depth, 1.0);
                desiredWidth = Clamp(desiredWidth + random.Range(-0.45, 0.45), MinUnitWidth(type), Math.Min(12.0, remaining));
                if (remaining - desiredWidth < MinAnyUnitWidth() + _tolerance)
                {
                    desiredWidth = remaining;
                }

                Polygon2 unitPolygon = Polygon2.Rectangle("unit-" + side + "-" + (variant.Units.Count + 1).ToString("00", CultureInfo.InvariantCulture), x, minY, x + desiredWidth, maxY);
                if (TryAddUnit(variant, unitPolygon, type, placedUnits, corridor))
                {
                    if (!currentCounts.ContainsKey(type))
                    {
                        currentCounts[type] = 0;
                    }

                    currentCounts[type]++;
                }
                else
                {
                    variant.Diagnostics.Add(Diagnostic.Warning("generation.unit_bay_rejected", "Rejected " + unitPolygon.SourceId + " because it was outside the usable floorplate or conflicted with fixed elements.", unitPolygon.SourceId));
                }

                x += desiredWidth;
            }
        }

        private void SplitVerticalInterval(
            LayoutVariant variant,
            CorridorStrategy corridor,
            double minY,
            double maxY,
            double minX,
            double maxX,
            string side,
            List<Polygon2> placedUnits,
            Dictionary<string, int> currentCounts,
            SeededRandom random)
        {
            double y = minY;
            double depth = maxX - minX;
            int safety = 0;
            while (maxY - y > MinAnyUnitWidth() + _tolerance && safety++ < 50)
            {
                double remaining = maxY - y;
                double bayAreaHint = Math.Min(remaining, 8.0 + random.Range(-1.0, 1.0)) * depth;
                string type = _mixPlanner.ChooseUnitType(bayAreaHint, currentCounts, _input.Source.GenerationSettings.WeightedVariation, random);
                UnitTypeTarget target = _mixPlanner.FindTarget(type);
                double desiredHeight = ((target.MinArea + target.MaxArea) * 0.5) / Math.Max(depth, 1.0);
                desiredHeight = Clamp(desiredHeight + random.Range(-0.45, 0.45), MinUnitWidth(type), Math.Min(12.0, remaining));
                if (remaining - desiredHeight < MinAnyUnitWidth() + _tolerance)
                {
                    desiredHeight = remaining;
                }

                Polygon2 unitPolygon = Polygon2.Rectangle("unit-" + side + "-" + (variant.Units.Count + 1).ToString("00", CultureInfo.InvariantCulture), minX, y, maxX, y + desiredHeight);
                if (TryAddUnit(variant, unitPolygon, type, placedUnits, corridor))
                {
                    if (!currentCounts.ContainsKey(type))
                    {
                        currentCounts[type] = 0;
                    }

                    currentCounts[type]++;
                }
                else
                {
                    variant.Diagnostics.Add(Diagnostic.Warning("generation.unit_bay_rejected", "Rejected " + unitPolygon.SourceId + " because it was outside the usable floorplate or conflicted with fixed elements.", unitPolygon.SourceId));
                }

                y += desiredHeight;
            }
        }

        private bool TryAddUnit(LayoutVariant variant, Polygon2 unitPolygon, string type, List<Polygon2> placedUnits, CorridorStrategy corridor)
        {
            if (!IsUsable(unitPolygon, placedUnits))
            {
                return false;
            }

            Polygon2 corridorPolygon = CorridorPolygon(corridor);
            if (GeometryPredicates.PolygonsOverlapArea(unitPolygon, corridorPolygon, _tolerance))
            {
                return false;
            }

            UnitLayout unit = new UnitLayout
            {
                Id = unitPolygon.SourceId,
                Type = type,
                Polygon = GeometryCleaner.ToPolygonInput(unitPolygon),
                Area = Math.Round(unitPolygon.Area(), 4),
                FacadeLength = Math.Round(FacadeAnalyzer.DaylightFacadeLength(unitPolygon, _input.Floorplate, _input.Source.Facade, _tolerance), 4)
            };
            variant.Units.Add(unit);
            placedUnits.Add(unitPolygon);
            return true;
        }

        private bool IsUsable(Polygon2 polygon, List<Polygon2> existing)
        {
            if (!GeometryPredicates.ContainsPolygon(_input.Floorplate, polygon, _tolerance))
            {
                return false;
            }

            foreach (Polygon2 hole in _input.Holes)
            {
                if (GeometryPredicates.PolygonsOverlapArea(polygon, hole, _tolerance) ||
                    GeometryPredicates.ContainsPoint(hole, polygon.Centroid(), _tolerance, false))
                {
                    return false;
                }
            }

            foreach (CleanedFixedElement fixedElement in _input.FixedElements.Where(f => f.BlocksGeneration))
            {
                if (GeometryPredicates.PolygonsOverlapArea(polygon, fixedElement.Polygon, _tolerance))
                {
                    return false;
                }
            }

            foreach (Polygon2 other in existing)
            {
                if (GeometryPredicates.PolygonsOverlapArea(polygon, other, _tolerance))
                {
                    return false;
                }
            }

            return polygon.Area() >= _input.Source.Rules.MinUnitArea;
        }

        private CorridorStrategy TryResolveCorridor(int variantIndex, SeededRandom random, List<Diagnostic> diagnostics)
        {
            if (_input.Source.Access != null &&
                _input.Source.Access.CorridorCenterlines != null &&
                _input.Source.Access.CorridorCenterlines.Count > 0)
            {
                LineInput line = _input.Source.Access.CorridorCenterlines[variantIndex % _input.Source.Access.CorridorCenterlines.Count];
                CorridorStrategy provided = CorridorFromLine(line, diagnostics);
                if (provided != null)
                {
                    diagnostics.Add(Diagnostic.Info("generation.corridor_from_input", "Used provided corridor centerline.", line.Id));
                    return provided;
                }
            }

            Bounds2 bounds = _input.Floorplate.Bounds();
            double width = Math.Max(_input.Source.Rules.MinCorridorWidth, 1.2);
            CorridorOrientation primary = bounds.Width >= bounds.Height ? CorridorOrientation.Horizontal : CorridorOrientation.Vertical;
            CorridorStrategy strategy = TryCoreAdjacentCorridor(primary, width, diagnostics);
            if (strategy != null)
            {
                return strategy;
            }

            strategy = TryCoreAdjacentCorridor(primary == CorridorOrientation.Horizontal ? CorridorOrientation.Vertical : CorridorOrientation.Horizontal, width, diagnostics);
            if (strategy != null)
            {
                return strategy;
            }

            double[] offsets = new[] { 0.0, -0.08, 0.08, -0.16, 0.16, -0.24, 0.24, -0.32, 0.32 };
            foreach (double offset in offsets)
            {
                double jitter = _input.Source.GenerationSettings.WeightedVariation ? random.Range(-0.025, 0.025) : 0.0;
                double fraction = Clamp(0.5 + offset + jitter, 0.20, 0.80);
                strategy = TryCorridorAtFraction(primary, fraction, width, diagnostics);
                if (strategy != null)
                {
                    diagnostics.Add(Diagnostic.Info("generation.corridor_derived", "Derived central corridor from cleaned floorplate scan."));
                    return strategy;
                }
            }

            CorridorOrientation secondary = primary == CorridorOrientation.Horizontal ? CorridorOrientation.Vertical : CorridorOrientation.Horizontal;
            foreach (double offset in offsets)
            {
                double fraction = Clamp(0.5 + offset, 0.20, 0.80);
                strategy = TryCorridorAtFraction(secondary, fraction, width, diagnostics);
                if (strategy != null)
                {
                    diagnostics.Add(Diagnostic.Warning("generation.corridor_secondary_axis", "Primary corridor axis failed; derived corridor on secondary axis."));
                    return strategy;
                }
            }

            return null;
        }

        private CorridorStrategy CorridorFromLine(LineInput line, List<Diagnostic> diagnostics)
        {
            double width = Math.Max(_input.Source.Rules.MinCorridorWidth, 1.2);
            double dx = Math.Abs(line.End.X - line.Start.X);
            double dy = Math.Abs(line.End.Y - line.Start.Y);
            if (dx >= dy)
            {
                double centerY = (line.Start.Y + line.End.Y) * 0.5;
                CorridorStrategy strategy = new CorridorStrategy
                {
                    Id = string.IsNullOrWhiteSpace(line.Id) ? "corridor-1" : line.Id,
                    Orientation = CorridorOrientation.Horizontal,
                    MinX = Math.Min(line.Start.X, line.End.X),
                    MaxX = Math.Max(line.Start.X, line.End.X),
                    MinY = centerY - (width * 0.5),
                    MaxY = centerY + (width * 0.5),
                    Width = width
                };
                return CorridorIsUsable(strategy) ? strategy : null;
            }

            double centerX = (line.Start.X + line.End.X) * 0.5;
            CorridorStrategy vertical = new CorridorStrategy
            {
                Id = string.IsNullOrWhiteSpace(line.Id) ? "corridor-1" : line.Id,
                Orientation = CorridorOrientation.Vertical,
                MinX = centerX - (width * 0.5),
                MaxX = centerX + (width * 0.5),
                MinY = Math.Min(line.Start.Y, line.End.Y),
                MaxY = Math.Max(line.Start.Y, line.End.Y),
                Width = width
            };
            return CorridorIsUsable(vertical) ? vertical : null;
        }

        private CorridorStrategy TryCoreAdjacentCorridor(CorridorOrientation orientation, double width, List<Diagnostic> diagnostics)
        {
            CleanedFixedElement core = _input.FixedElements.FirstOrDefault(f => f.Type.IndexOf("core", StringComparison.OrdinalIgnoreCase) >= 0);
            if (core == null)
            {
                return null;
            }

            Bounds2 coreBounds = core.Polygon.Bounds();
            if (orientation == CorridorOrientation.Horizontal)
            {
                CorridorStrategy above = TryBuildHorizontalCorridor(coreBounds.MaxY + (width * 0.5), width);
                if (above != null)
                {
                    diagnostics.Add(Diagnostic.Info("generation.corridor_core_adjacent", "Derived corridor adjacent to fixed core.", core.Id));
                    return above;
                }

                CorridorStrategy below = TryBuildHorizontalCorridor(coreBounds.MinY - (width * 0.5), width);
                if (below != null)
                {
                    diagnostics.Add(Diagnostic.Info("generation.corridor_core_adjacent", "Derived corridor adjacent to fixed core.", core.Id));
                    return below;
                }
            }
            else
            {
                CorridorStrategy right = TryBuildVerticalCorridor(coreBounds.MaxX + (width * 0.5), width);
                if (right != null)
                {
                    diagnostics.Add(Diagnostic.Info("generation.corridor_core_adjacent", "Derived corridor adjacent to fixed core.", core.Id));
                    return right;
                }

                CorridorStrategy left = TryBuildVerticalCorridor(coreBounds.MinX - (width * 0.5), width);
                if (left != null)
                {
                    diagnostics.Add(Diagnostic.Info("generation.corridor_core_adjacent", "Derived corridor adjacent to fixed core.", core.Id));
                    return left;
                }
            }

            return null;
        }

        private CorridorStrategy TryCorridorAtFraction(CorridorOrientation orientation, double fraction, double width, List<Diagnostic> diagnostics)
        {
            Bounds2 bounds = _input.Floorplate.Bounds();
            if (orientation == CorridorOrientation.Horizontal)
            {
                return TryBuildHorizontalCorridor(bounds.MinY + (bounds.Height * fraction), width);
            }

            return TryBuildVerticalCorridor(bounds.MinX + (bounds.Width * fraction), width);
        }

        private CorridorStrategy TryBuildHorizontalCorridor(double centerY, double width)
        {
            double minY = centerY - (width * 0.5);
            double maxY = centerY + (width * 0.5);
            if (maxY <= _input.Floorplate.Bounds().MinY + _tolerance || minY >= _input.Floorplate.Bounds().MaxY - _tolerance)
            {
                return null;
            }

            List<Interval1D> lower = GeometryPredicates.HorizontalInsideIntervals(_input.Floorplate, minY + _tolerance, _tolerance);
            List<Interval1D> center = GeometryPredicates.HorizontalInsideIntervals(_input.Floorplate, centerY, _tolerance);
            List<Interval1D> upper = GeometryPredicates.HorizontalInsideIntervals(_input.Floorplate, maxY - _tolerance, _tolerance);
            List<Interval1D> shared = GeometryPredicates.IntersectIntervals(GeometryPredicates.IntersectIntervals(lower, center, _tolerance), upper, _tolerance);
            Interval1D best = shared.OrderByDescending(i => i.Length).FirstOrDefault();
            if (best == null || best.Length < Math.Max(width * 4.0, 7.0))
            {
                return null;
            }

            CorridorStrategy strategy = new CorridorStrategy
            {
                Id = "corridor-1",
                Orientation = CorridorOrientation.Horizontal,
                MinX = best.Start,
                MaxX = best.End,
                MinY = minY,
                MaxY = maxY,
                Width = width
            };
            return CorridorIsUsable(strategy) ? strategy : null;
        }

        private CorridorStrategy TryBuildVerticalCorridor(double centerX, double width)
        {
            double minX = centerX - (width * 0.5);
            double maxX = centerX + (width * 0.5);
            if (maxX <= _input.Floorplate.Bounds().MinX + _tolerance || minX >= _input.Floorplate.Bounds().MaxX - _tolerance)
            {
                return null;
            }

            List<Interval1D> left = GeometryPredicates.VerticalInsideIntervals(_input.Floorplate, minX + _tolerance, _tolerance);
            List<Interval1D> center = GeometryPredicates.VerticalInsideIntervals(_input.Floorplate, centerX, _tolerance);
            List<Interval1D> right = GeometryPredicates.VerticalInsideIntervals(_input.Floorplate, maxX - _tolerance, _tolerance);
            List<Interval1D> shared = GeometryPredicates.IntersectIntervals(GeometryPredicates.IntersectIntervals(left, center, _tolerance), right, _tolerance);
            Interval1D best = shared.OrderByDescending(i => i.Length).FirstOrDefault();
            if (best == null || best.Length < Math.Max(width * 4.0, 7.0))
            {
                return null;
            }

            CorridorStrategy strategy = new CorridorStrategy
            {
                Id = "corridor-1",
                Orientation = CorridorOrientation.Vertical,
                MinX = minX,
                MaxX = maxX,
                MinY = best.Start,
                MaxY = best.End,
                Width = width
            };
            return CorridorIsUsable(strategy) ? strategy : null;
        }

        private bool CorridorIsUsable(CorridorStrategy strategy)
        {
            Polygon2 polygon = CorridorPolygon(strategy);
            if (!GeometryPredicates.ContainsPolygon(_input.Floorplate, polygon, _tolerance))
            {
                return false;
            }

            foreach (Polygon2 hole in _input.Holes)
            {
                if (GeometryPredicates.PolygonsOverlapArea(polygon, hole, _tolerance))
                {
                    return false;
                }
            }

            foreach (CleanedFixedElement fixedElement in _input.FixedElements.Where(f => f.BlocksGeneration))
            {
                if (GeometryPredicates.PolygonsOverlapArea(polygon, fixedElement.Polygon, _tolerance))
                {
                    return false;
                }
            }

            return true;
        }

        private List<string> CorridorConnections(Polygon2 corridorPolygon)
        {
            List<string> connections = new List<string>();
            foreach (CleanedFixedElement fixedElement in _input.FixedElements)
            {
                if ((fixedElement.Type.IndexOf("core", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     fixedElement.Type.IndexOf("stair", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     fixedElement.Type.IndexOf("elevator", StringComparison.OrdinalIgnoreCase) >= 0) &&
                    GeometryPredicates.TouchesOrOverlaps(corridorPolygon, fixedElement.Polygon, _tolerance))
                {
                    connections.Add(fixedElement.Id);
                }
            }

            int accessIndex = 0;
            foreach (Point2 point in _input.Source.Access.VerticalCoreAccess)
            {
                accessIndex++;
                if (GeometryPredicates.ContainsPoint(corridorPolygon, point, _tolerance, true))
                {
                    connections.Add("vertical_access_" + accessIndex.ToString(CultureInfo.InvariantCulture));
                }
            }

            return connections;
        }

        private TopologyGraph BuildTopology(LayoutVariant variant, Polygon2 corridorPolygon)
        {
            TopologyGraph graph = new TopologyGraph();
            graph.AddNode("floorplate", "floorplate", _input.Floorplate.SourceId);

            foreach (CleanedFixedElement fixedElement in _input.FixedElements)
            {
                graph.AddNode(fixedElement.Id, fixedElement.Type, fixedElement.Id, "floorplate");
                graph.AddEdge(fixedElement.Id, "floorplate", "belongs_to", "fixed_element");
                if (GeometryPredicates.TouchesOrOverlaps(corridorPolygon, fixedElement.Polygon, _tolerance))
                {
                    graph.AddEdge(fixedElement.Id, variant.Corridors[0].Id, "connects_to_corridor", "touches corridor boundary");
                }
            }

            foreach (CorridorLayout corridor in variant.Corridors)
            {
                graph.AddNode(corridor.Id, "corridor", corridor.Id, "floorplate");
                graph.AddEdge(corridor.Id, "floorplate", "belongs_to", "circulation");
            }

            foreach (UnitLayout unit in variant.Units)
            {
                graph.AddNode(unit.Id, "unit", unit.Id, "floorplate");
                graph.AddEdge(unit.Id, "floorplate", "belongs_to", "unit containment");
                graph.AddEdge(unit.Id, variant.Corridors[0].Id, "has_door", "unit entry");
                if (unit.FacadeLength > _tolerance)
                {
                    graph.AddEdge(unit.Id, "outside", "has_facade", "daylight facade exposure");
                }

                foreach (RoomLayout room in unit.Rooms)
                {
                    graph.AddNode(room.Id, "room:" + room.RoomType, room.Id, unit.Id);
                    graph.AddEdge(room.Id, unit.Id, "belongs_to", "room containment");
                    if (room.Daylight)
                    {
                        graph.AddEdge(room.Id, "outside", "has_facade", "room daylight exposure");
                    }

                    if (room.RoomType.IndexOf("bedroom", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        room.RoomType.IndexOf("living", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        graph.AddEdge(room.Id, "outside", "constraint_requires_facade", "habitable room daylight requirement");
                    }
                }
            }

            return graph;
        }

        private static Polygon2 CorridorPolygon(CorridorStrategy corridor)
        {
            return Polygon2.Rectangle(corridor.Id, corridor.MinX, corridor.MinY, corridor.MaxX, corridor.MaxY);
        }

        private static LineInput CorridorCenterline(CorridorStrategy corridor)
        {
            if (corridor.Orientation == CorridorOrientation.Horizontal)
            {
                double y = (corridor.MinY + corridor.MaxY) * 0.5;
                return new LineInput
                {
                    Id = corridor.Id + "-centerline",
                    Start = new Point2(corridor.MinX, y),
                    End = new Point2(corridor.MaxX, y)
                };
            }

            double x = (corridor.MinX + corridor.MaxX) * 0.5;
            return new LineInput
            {
                Id = corridor.Id + "-centerline",
                Start = new Point2(x, corridor.MinY),
                End = new Point2(x, corridor.MaxY)
            };
        }

        private double MinAnyUnitWidth()
        {
            return Math.Max(4.0, _input.Source.Rules.MinRoomWidth + 1.2);
        }

        private double MinUnitWidth(string type)
        {
            if (type.IndexOf("two", StringComparison.OrdinalIgnoreCase) >= 0 || type.IndexOf("2", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Math.Max(8.4, _input.Source.Rules.MinRoomWidth * 3.0);
            }

            if (type.IndexOf("one", StringComparison.OrdinalIgnoreCase) >= 0 || type.IndexOf("1", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Math.Max(6.4, _input.Source.Rules.MinRoomWidth * 2.4);
            }

            return Math.Max(4.2, _input.Source.Rules.MinRoomWidth * 1.6);
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

        private static int CombineSeed(int seed, int index)
        {
            unchecked
            {
                int hash = seed == 0 ? 17 : seed;
                hash = (hash * 397) ^ (index + 1);
                return hash;
            }
        }
    }
}
