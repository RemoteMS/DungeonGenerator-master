using System.Collections.Generic;
using Helpers;
using UnityEngine;

namespace MapGeneration
{
    public class Path
    {
        public List<Vector2Int> Points { get; private set; }

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

        public Vector2Int GetMinPoint()
        {
            var minX = int.MaxValue;
            var minY = int.MaxValue;

            foreach (var point in Points)
            {
                if (point.x < minX) minX = point.x;
                if (point.y < minY) minY = point.y;
            }

            return new Vector2Int(minX, minY);
        }

        public override string ToString()
        {
            return $"path_[{string.Join(", ", Points)}]";
        }
    }

}