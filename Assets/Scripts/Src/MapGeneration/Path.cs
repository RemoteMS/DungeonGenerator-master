using System.Collections.Generic;
using Helpers;
using UnityEngine;

namespace MapGeneration
{
    public class Path
    {
        public List<Vector2Int> Points { get; }

        public Path(List<Vector2Int> points)
        {
            Points = points;
        }

        public void Merge(Path other)
        {
            foreach (var point in other.Points)
            {
                if (!Points.Contains(point))
                    Points.Add(point);
            }
        }

        public void DrawPath(int y = 0)
        {
            for (var i = 0; i < Points.Count - 2; i++)
            {
                Debug.DrawLine(Points[i].ToVector3(y), Points[i + 1].ToVector3(y), Color.yellow, 100f);
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

            Debug.Log($"minX - {minX}, minY - {minY}, with - {maxX - minX}, height - {maxY - minY}");
            // todo: check adding "+1" to width and height
            return new RectInt(minX, minY, (maxX - minX) + 1, (maxY - minY) + 1);
        }

        public override string ToString()
        {
            return $"path_[{string.Join(", ", Points)}]";
        }
    }

}