using System.Collections.Generic;
using MapGeneration.Presentation.MapInfo;
using UnityEngine;

namespace MapGeneration
{
    public class Path
    {
        public MapGenerator.Room From { get; }
        public MapGenerator.Room To { get; }

        public List<Vector2Int> Points { get; }

        public Path(List<Vector2Int> points, MapGenerator.Room from, MapGenerator.Room to)
        {
            Points = points;
            From = from;
            To = to;
        }

        public void Merge(Path other)
        {
            foreach (var point in other.Points)
            {
                if (!Points.Contains(point))
                    Points.Add(point);
            }
        }

        public RectInt GetBounds()
        {
            var minX = int.MaxValue;
            var minY = int.MaxValue;

            var maxX = int.MinValue;
            var maxY = int.MinValue;

            foreach (var point in Points)
            {
                if (point.x < minX) minX = point.x;
                if (point.y < minY) minY = point.y;

                if (point.x > maxX) maxX = point.x;
                if (point.y > maxY) maxY = point.y;
            }

            // todo: check adding "+1" to width and height
            return new RectInt(minX, minY, (maxX - minX) + 1, (maxY - minY) + 1);
        }

        public override string ToString()
        {
            return $"path_[{string.Join(", ", Points)}]";
        }
    }

}