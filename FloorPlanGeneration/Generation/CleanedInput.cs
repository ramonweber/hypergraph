using System.Collections.Generic;
using FloorPlanGeneration.Geometry;
using FloorPlanGeneration.Schema;

namespace FloorPlanGeneration.Generation
{
    internal sealed class CleanedInput
    {
        public CleanedInput()
        {
            Source = new EngineInput();
            Floorplate = new Polygon2();
            Holes = new List<Polygon2>();
            FixedElements = new List<CleanedFixedElement>();
            Diagnostics = new List<Diagnostic>();
            Tolerance = 0.01;
        }

        public EngineInput Source { get; set; }
        public Polygon2 Floorplate { get; set; }
        public List<Polygon2> Holes { get; set; }
        public List<CleanedFixedElement> FixedElements { get; set; }
        public List<Diagnostic> Diagnostics { get; set; }
        public double Tolerance { get; set; }
    }

    internal sealed class CleanedFixedElement
    {
        public CleanedFixedElement()
        {
            Id = string.Empty;
            Type = string.Empty;
            Polygon = new Polygon2();
            BlocksGeneration = true;
        }

        public string Id { get; set; }
        public string Type { get; set; }
        public Polygon2 Polygon { get; set; }
        public bool BlocksGeneration { get; set; }
    }
}
