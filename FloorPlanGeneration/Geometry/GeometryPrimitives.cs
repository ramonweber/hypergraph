using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FloorPlanGeneration.Geometry
{
    public sealed class Point2
    {
        public Point2()
        {
        }

        public Point2(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; set; }
        public double Y { get; set; }

        public Point2 Clone()
        {
            return new Point2(X, Y);
        }

        public double DistanceTo(Point2 other)
        {
            double dx = X - other.X;
            double dy = Y - other.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public bool EqualsWithin(Point2 other, double tolerance)
        {
            return DistanceTo(other) <= tolerance;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "({0:0.###}, {1:0.###})", X, Y);
        }
    }

    public sealed class LineSegment2
    {
        public LineSegment2(Point2 start, Point2 end)
        {
            Start = start;
            End = end;
        }

        public Point2 Start { get; private set; }
        public Point2 End { get; private set; }

        public double Length
        {
            get { return Start.DistanceTo(End); }
        }

        public bool IsHorizontal(double tolerance)
        {
            return Math.Abs(Start.Y - End.Y) <= tolerance;
        }

        public bool IsVertical(double tolerance)
        {
            return Math.Abs(Start.X - End.X) <= tolerance;
        }

        public Point2 Midpoint()
        {
            return new Point2((Start.X + End.X) * 0.5, (Start.Y + End.Y) * 0.5);
        }
    }

    public sealed class Bounds2
    {
        public Bounds2()
        {
        }

        public Bounds2(double minX, double minY, double maxX, double maxY)
        {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
        }

        public double MinX { get; set; }
        public double MinY { get; set; }
        public double MaxX { get; set; }
        public double MaxY { get; set; }

        public double Width
        {
            get { return MaxX - MinX; }
        }

        public double Height
        {
            get { return MaxY - MinY; }
        }

        public double Area
        {
            get { return Math.Max(0.0, Width) * Math.Max(0.0, Height); }
        }

        public Point2 Center()
        {
            return new Point2((MinX + MaxX) * 0.5, (MinY + MaxY) * 0.5);
        }

        public bool Contains(Point2 point, double tolerance)
        {
            return point.X >= MinX - tolerance && point.X <= MaxX + tolerance &&
                   point.Y >= MinY - tolerance && point.Y <= MaxY + tolerance;
        }

        public bool Intersects(Bounds2 other, double tolerance)
        {
            return !(MaxX < other.MinX + tolerance ||
                     other.MaxX < MinX + tolerance ||
                     MaxY < other.MinY + tolerance ||
                     other.MaxY < MinY + tolerance);
        }

        public static Bounds2 FromPoints(IEnumerable<Point2> points)
        {
            bool any = false;
            double minX = 0.0;
            double minY = 0.0;
            double maxX = 0.0;
            double maxY = 0.0;

            foreach (Point2 point in points)
            {
                if (!any)
                {
                    minX = maxX = point.X;
                    minY = maxY = point.Y;
                    any = true;
                    continue;
                }

                minX = Math.Min(minX, point.X);
                minY = Math.Min(minY, point.Y);
                maxX = Math.Max(maxX, point.X);
                maxY = Math.Max(maxY, point.Y);
            }

            return any ? new Bounds2(minX, minY, maxX, maxY) : new Bounds2();
        }
    }

    public sealed class Polygon2
    {
        public Polygon2()
        {
            SourceId = string.Empty;
            Vertices = new List<Point2>();
        }

        public Polygon2(string sourceId, IEnumerable<Point2> vertices)
        {
            SourceId = sourceId ?? string.Empty;
            Vertices = vertices.Select(p => p.Clone()).ToList();
        }

        public string SourceId { get; set; }
        public List<Point2> Vertices { get; private set; }

        public int Count
        {
            get { return Vertices.Count; }
        }

        public double SignedArea()
        {
            double sum = 0.0;
            for (int i = 0; i < Vertices.Count; i++)
            {
                Point2 a = Vertices[i];
                Point2 b = Vertices[(i + 1) % Vertices.Count];
                sum += (a.X * b.Y) - (b.X * a.Y);
            }

            return sum * 0.5;
        }

        public double Area()
        {
            return Math.Abs(SignedArea());
        }

        public Bounds2 Bounds()
        {
            return Bounds2.FromPoints(Vertices);
        }

        public Point2 Centroid()
        {
            double signedArea = SignedArea();
            if (Math.Abs(signedArea) < 1e-9)
            {
                return Bounds().Center();
            }

            double cx = 0.0;
            double cy = 0.0;
            for (int i = 0; i < Vertices.Count; i++)
            {
                Point2 a = Vertices[i];
                Point2 b = Vertices[(i + 1) % Vertices.Count];
                double cross = (a.X * b.Y) - (b.X * a.Y);
                cx += (a.X + b.X) * cross;
                cy += (a.Y + b.Y) * cross;
            }

            double factor = 1.0 / (6.0 * signedArea);
            return new Point2(cx * factor, cy * factor);
        }

        public IEnumerable<LineSegment2> Edges()
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                yield return new LineSegment2(Vertices[i], Vertices[(i + 1) % Vertices.Count]);
            }
        }

        public List<Point2> ClosedVertices()
        {
            List<Point2> points = Vertices.Select(p => p.Clone()).ToList();
            if (points.Count > 0)
            {
                points.Add(points[0].Clone());
            }

            return points;
        }

        public Polygon2 Clone()
        {
            return new Polygon2(SourceId, Vertices);
        }

        public Polygon2 Reversed()
        {
            List<Point2> reversed = Vertices.Select(p => p.Clone()).Reverse().ToList();
            return new Polygon2(SourceId, reversed);
        }

        public bool IsClockwise()
        {
            return SignedArea() < 0.0;
        }

        public bool IsOrthogonal(double tolerance)
        {
            foreach (LineSegment2 edge in Edges())
            {
                if (!edge.IsHorizontal(tolerance) && !edge.IsVertical(tolerance))
                {
                    return false;
                }
            }

            return true;
        }

        public static Polygon2 Rectangle(string sourceId, double minX, double minY, double maxX, double maxY)
        {
            return new Polygon2(sourceId, new[]
            {
                new Point2(minX, minY),
                new Point2(maxX, minY),
                new Point2(maxX, maxY),
                new Point2(minX, maxY)
            });
        }
    }
}
