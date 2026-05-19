using System;
using System.Collections.Generic;
using FloorPlanGeneration.Geometry;
using FloorPlanGeneration.Topology;

namespace FloorPlanGeneration.Schema
{
    public sealed class EngineInput
    {
        public EngineInput()
        {
            Project = new ProjectInfo();
            Floorplate = new FloorplateInput();
            FixedElements = new List<FixedElementInput>();
            Access = new AccessInput();
            Facade = new FacadeInput();
            Program = new ProgramBrief();
            Rules = new RuleSet();
            GenerationSettings = new GenerationSettings();
        }

        public ProjectInfo Project { get; set; }
        public FloorplateInput Floorplate { get; set; }
        public List<FixedElementInput> FixedElements { get; set; }
        public AccessInput Access { get; set; }
        public FacadeInput Facade { get; set; }
        public ProgramBrief Program { get; set; }
        public RuleSet Rules { get; set; }
        public GenerationSettings GenerationSettings { get; set; }
    }

    public sealed class ProjectInfo
    {
        public ProjectInfo()
        {
            Id = "project";
            Name = "Floor Plan Generation Project";
            Units = "m";
            Tolerance = 0.01;
            Seed = 1;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Units { get; set; }
        public double Tolerance { get; set; }
        public int Seed { get; set; }
    }

    public sealed class FloorplateInput
    {
        public FloorplateInput()
        {
            Outer = new PolygonInput();
            Holes = new List<PolygonInput>();
        }

        public PolygonInput Outer { get; set; }
        public List<PolygonInput> Holes { get; set; }
    }

    public sealed class PolygonInput
    {
        public PolygonInput()
        {
            Id = string.Empty;
            Points = new List<Point2>();
        }

        public string Id { get; set; }
        public List<Point2> Points { get; set; }
    }

    public sealed class LineInput
    {
        public LineInput()
        {
            Id = string.Empty;
            Start = new Point2();
            End = new Point2();
        }

        public string Id { get; set; }
        public Point2 Start { get; set; }
        public Point2 End { get; set; }
    }

    public sealed class FixedElementInput
    {
        public FixedElementInput()
        {
            Id = string.Empty;
            Type = "core";
            Polygon = new PolygonInput();
            BlocksGeneration = true;
        }

        public string Id { get; set; }
        public string Type { get; set; }
        public PolygonInput Polygon { get; set; }
        public bool BlocksGeneration { get; set; }
    }

    public sealed class AccessInput
    {
        public AccessInput()
        {
            EntryPoints = new List<Point2>();
            VerticalCoreAccess = new List<Point2>();
            CorridorStartPoints = new List<Point2>();
            CorridorEndPoints = new List<Point2>();
            CorridorCenterlines = new List<LineInput>();
        }

        public List<Point2> EntryPoints { get; set; }
        public List<Point2> VerticalCoreAccess { get; set; }
        public List<Point2> CorridorStartPoints { get; set; }
        public List<Point2> CorridorEndPoints { get; set; }
        public List<LineInput> CorridorCenterlines { get; set; }
    }

    public sealed class FacadeInput
    {
        public FacadeInput()
        {
            Segments = new List<FacadeSegmentInput>();
            DaylightCapableEdges = new List<string>();
            NonDaylightEdges = new List<string>();
        }

        public List<FacadeSegmentInput> Segments { get; set; }
        public List<string> DaylightCapableEdges { get; set; }
        public List<string> NonDaylightEdges { get; set; }
    }

    public sealed class FacadeSegmentInput
    {
        public FacadeSegmentInput()
        {
            Id = string.Empty;
            Start = new Point2();
            End = new Point2();
            DaylightCapable = true;
        }

        public string Id { get; set; }
        public Point2 Start { get; set; }
        public Point2 End { get; set; }
        public bool DaylightCapable { get; set; }
    }

    public sealed class ProgramBrief
    {
        public ProgramBrief()
        {
            TargetUnitTypes = new List<UnitTypeTarget>();
            RoomTypes = new List<RoomTypeRule>();
        }

        public List<UnitTypeTarget> TargetUnitTypes { get; set; }
        public List<RoomTypeRule> RoomTypes { get; set; }
    }

    public sealed class UnitTypeTarget
    {
        public UnitTypeTarget()
        {
            Type = "studio";
            MinArea = 30.0;
            MaxArea = 110.0;
            TargetCount = 0;
            TargetRatio = 0.0;
            Weight = 1.0;
        }

        public string Type { get; set; }
        public double MinArea { get; set; }
        public double MaxArea { get; set; }
        public int TargetCount { get; set; }
        public double TargetRatio { get; set; }
        public double Weight { get; set; }
    }

    public sealed class RoomTypeRule
    {
        public RoomTypeRule()
        {
            Type = string.Empty;
            MinArea = 0.0;
            MaxArea = 0.0;
            MinWidth = 0.0;
            MinDepth = 0.0;
            RequiresDaylight = false;
            IsWet = false;
        }

        public string Type { get; set; }
        public double MinArea { get; set; }
        public double MaxArea { get; set; }
        public double MinWidth { get; set; }
        public double MinDepth { get; set; }
        public bool RequiresDaylight { get; set; }
        public bool IsWet { get; set; }
    }

    public sealed class RuleSet
    {
        public RuleSet()
        {
            MinCorridorWidth = 1.8;
            MinRoomWidth = 2.4;
            MinRoomDepth = 2.4;
            DoorWidth = 0.9;
            WetRoomAdjacencyPreferred = true;
            RequireDaylightForBedrooms = true;
            RequireDaylightForLiving = true;
            MinUnitArea = 25.0;
        }

        public double MinCorridorWidth { get; set; }
        public double MinRoomWidth { get; set; }
        public double MinRoomDepth { get; set; }
        public double DoorWidth { get; set; }
        public bool WetRoomAdjacencyPreferred { get; set; }
        public bool RequireDaylightForBedrooms { get; set; }
        public bool RequireDaylightForLiving { get; set; }
        public double MinUnitArea { get; set; }
    }

    public sealed class GenerationSettings
    {
        public GenerationSettings()
        {
            VariantCount = 8;
            TimeLimitMilliseconds = 1000;
            Strictness = "balanced";
            WeightedVariation = true;
            ScoringWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        }

        public int VariantCount { get; set; }
        public int TimeLimitMilliseconds { get; set; }
        public string Strictness { get; set; }
        public bool WeightedVariation { get; set; }
        public Dictionary<string, double> ScoringWeights { get; set; }
    }

    public sealed class EngineOutput
    {
        public EngineOutput()
        {
            ProjectId = string.Empty;
            Status = "not_started";
            Variants = new List<LayoutVariant>();
            Diagnostics = new List<Diagnostic>();
        }

        public string ProjectId { get; set; }
        public string Status { get; set; }
        public List<LayoutVariant> Variants { get; set; }
        public List<Diagnostic> Diagnostics { get; set; }
    }

    public sealed class LayoutVariant
    {
        public LayoutVariant()
        {
            VariantId = string.Empty;
            Seed = 0;
            Status = "candidate";
            Units = new List<UnitLayout>();
            Rooms = new List<RoomLayout>();
            Corridors = new List<CorridorLayout>();
            Walls = new List<WallLayout>();
            DoorsOpenings = new List<DoorOpening>();
            Labels = new List<LabelLayout>();
            Metrics = new VariantMetrics();
            Validation = new ValidationReport();
            Diagnostics = new List<Diagnostic>();
            Topology = new TopologyGraph();
        }

        public string VariantId { get; set; }
        public int Seed { get; set; }
        public string Status { get; set; }
        public List<UnitLayout> Units { get; set; }
        public List<RoomLayout> Rooms { get; set; }
        public List<CorridorLayout> Corridors { get; set; }
        public List<WallLayout> Walls { get; set; }
        public List<DoorOpening> DoorsOpenings { get; set; }
        public List<LabelLayout> Labels { get; set; }
        public VariantMetrics Metrics { get; set; }
        public ValidationReport Validation { get; set; }
        public List<Diagnostic> Diagnostics { get; set; }
        public TopologyGraph Topology { get; set; }
    }

    public sealed class UnitLayout
    {
        public UnitLayout()
        {
            Id = string.Empty;
            Type = "studio";
            Polygon = new PolygonInput();
            Area = 0.0;
            Rooms = new List<RoomLayout>();
            FacadeLength = 0.0;
            Score = 0.0;
        }

        public string Id { get; set; }
        public string Type { get; set; }
        public PolygonInput Polygon { get; set; }
        public double Area { get; set; }
        public List<RoomLayout> Rooms { get; set; }
        public double FacadeLength { get; set; }
        public double Score { get; set; }
    }

    public sealed class RoomLayout
    {
        public RoomLayout()
        {
            Id = string.Empty;
            UnitId = string.Empty;
            RoomType = "room";
            Polygon = new PolygonInput();
            Area = 0.0;
            Dimensions = new SpaceDimensions();
            Daylight = false;
        }

        public string Id { get; set; }
        public string UnitId { get; set; }
        public string RoomType { get; set; }
        public PolygonInput Polygon { get; set; }
        public double Area { get; set; }
        public SpaceDimensions Dimensions { get; set; }
        public bool Daylight { get; set; }
    }

    public sealed class SpaceDimensions
    {
        public double Width { get; set; }
        public double Depth { get; set; }
    }

    public sealed class CorridorLayout
    {
        public CorridorLayout()
        {
            Id = string.Empty;
            Polygon = new PolygonInput();
            Centerline = new LineInput();
            Width = 0.0;
            Connections = new List<string>();
        }

        public string Id { get; set; }
        public PolygonInput Polygon { get; set; }
        public LineInput Centerline { get; set; }
        public double Width { get; set; }
        public List<string> Connections { get; set; }
    }

    public sealed class WallLayout
    {
        public WallLayout()
        {
            Id = string.Empty;
            Centerline = new LineInput();
            Thickness = 0.15;
            LayerType = "partition";
        }

        public string Id { get; set; }
        public LineInput Centerline { get; set; }
        public double Thickness { get; set; }
        public string LayerType { get; set; }
    }

    public sealed class DoorOpening
    {
        public DoorOpening()
        {
            Id = string.Empty;
            Location = new Point2();
            Width = 0.9;
            HostWall = string.Empty;
            ConnectsSpaces = new List<string>();
        }

        public string Id { get; set; }
        public Point2 Location { get; set; }
        public double Width { get; set; }
        public string HostWall { get; set; }
        public List<string> ConnectsSpaces { get; set; }
    }

    public sealed class LabelLayout
    {
        public LabelLayout()
        {
            Id = string.Empty;
            Text = string.Empty;
            Location = new Point2();
            TargetId = string.Empty;
            Layer = "FP::Generated::Labels";
        }

        public string Id { get; set; }
        public string Text { get; set; }
        public Point2 Location { get; set; }
        public string TargetId { get; set; }
        public string Layer { get; set; }
    }

    public sealed class VariantMetrics
    {
        public VariantMetrics()
        {
            Efficiency = 0.0;
            NetGrossRatio = 0.0;
            SellableArea = 0.0;
            CorridorArea = 0.0;
            UnitMixMatch = 0.0;
            GrossArea = 0.0;
            Score = 0.0;
        }

        public double Efficiency { get; set; }
        public double NetGrossRatio { get; set; }
        public double SellableArea { get; set; }
        public double CorridorArea { get; set; }
        public double UnitMixMatch { get; set; }
        public double GrossArea { get; set; }
        public double Score { get; set; }
    }

    public sealed class ValidationReport
    {
        public ValidationReport()
        {
            Passed = false;
            Checks = new List<ValidationCheck>();
        }

        public bool Passed { get; set; }
        public List<ValidationCheck> Checks { get; set; }
    }

    public sealed class ValidationCheck
    {
        public ValidationCheck()
        {
            Name = string.Empty;
            Passed = false;
            Severity = "error";
            Reason = string.Empty;
            SourceId = string.Empty;
        }

        public string Name { get; set; }
        public bool Passed { get; set; }
        public string Severity { get; set; }
        public string Reason { get; set; }
        public string SourceId { get; set; }
    }

    public sealed class Diagnostic
    {
        public Diagnostic()
        {
            Severity = "info";
            Code = string.Empty;
            Message = string.Empty;
            SourceId = string.Empty;
        }

        public string Severity { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public string SourceId { get; set; }

        public static Diagnostic Info(string code, string message, string sourceId = "")
        {
            return new Diagnostic { Severity = "info", Code = code, Message = message, SourceId = sourceId };
        }

        public static Diagnostic Warning(string code, string message, string sourceId = "")
        {
            return new Diagnostic { Severity = "warning", Code = code, Message = message, SourceId = sourceId };
        }

        public static Diagnostic Error(string code, string message, string sourceId = "")
        {
            return new Diagnostic { Severity = "error", Code = code, Message = message, SourceId = sourceId };
        }
    }
}
