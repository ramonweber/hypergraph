namespace FloorPlanGeneration.Generation
{
    internal enum CorridorOrientation
    {
        Horizontal,
        Vertical
    }

    internal sealed class CorridorStrategy
    {
        public CorridorStrategy()
        {
            Id = "corridor-1";
            Orientation = CorridorOrientation.Horizontal;
            MinX = 0.0;
            MinY = 0.0;
            MaxX = 0.0;
            MaxY = 0.0;
            Width = 0.0;
        }

        public string Id { get; set; }
        public CorridorOrientation Orientation { get; set; }
        public double MinX { get; set; }
        public double MinY { get; set; }
        public double MaxX { get; set; }
        public double MaxY { get; set; }
        public double Width { get; set; }
    }
}
