using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapGeneration.Presentation.MapInfo
{
    public class CorridorData : ICorridor
    {
        private HashSet<Vector2Int> _corridorPoints;

        public void Generate()
        {
        }

        public static void EnsureUniquePaths(List<Path> paths, Grid2D<Generator2D.CellType> grid)
        {
            for (var i = 0; i < paths.Count; i++)
            {
                for (var j = i + 1; j < paths.Count; j++)
                {
                    var intersection = paths[i].Points
                        .Intersect(paths[j].Points)
                        .Where(pos => grid[pos.x, pos.y] == Generator2D.CellType.Hallway)
                        .ToList();

                    if (intersection.Any())
                    {
                        paths[i].Merge(paths[j]);

                        paths.RemoveAt(j);
                        j--;
                    }
                }
            }
        }
    }
}