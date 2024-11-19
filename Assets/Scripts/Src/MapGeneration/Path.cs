using System.Collections.Generic;
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
    }

}