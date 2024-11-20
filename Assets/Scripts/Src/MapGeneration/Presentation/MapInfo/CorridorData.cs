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

        public static List<Path> ValidateAndSplitPaths(List<Path> paths, Grid2D<Generator2D.CellType> grid)
        {
            var resultPaths = new List<Path>();

            foreach (var path in paths)
            {
                var points = path.Points;
                var currentPathPoints = new List<Vector2Int>();
                var isNewPath = true;

                foreach (var point in points)
                {
                    var cellType = grid[point.x, point.y];

                    if (cellType == Generator2D.CellType.Room)
                    {
                        if (isNewPath)
                        {
                            currentPathPoints = new List<Vector2Int> { point };
                            isNewPath = false;
                        }
                        else
                        {
                            currentPathPoints.Add(point);
                            resultPaths.Add(new Path(new List<Vector2Int>(currentPathPoints)));

                            currentPathPoints = new List<Vector2Int> { point };
                        }
                    }
                    else if (cellType == Generator2D.CellType.Hallway)
                    {
                        if (!isNewPath)
                        {
                            currentPathPoints.Add(point);
                        }
                    }
                }

                if (currentPathPoints.Count > 1)
                {
                    resultPaths.Add(new Path(currentPathPoints));
                }
            }

            return resultPaths;
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