using MapGeneration.Presentation.Enums;
using UnityEngine;

namespace MapGeneration.Presentation.MapInfo
{
    public class HallwayData : IHallway, IPlaceable
    {
        public RectInt Bounds { get; }

        private Vector3Int _hallwayPosition;
        public Cell[,] Cells { get; private set; }
        private readonly Vector2Int[] _globalPoints;

        public HallwayData(Path path, Grid2D<Cell> cells, Grid2D<CellType> grid)
        {
            Bounds = path.GetBounds();

            _globalPoints = new Vector2Int[path.Points.Count];
            for (int i = 0; i < path.Points.Count; i++)
            {
                _globalPoints[i] = path.Points[i];
            }

            CopyCells(cells, grid);

            _name = path.ToString();
        }

        private void CopyCells(Grid2D<Cell> cells, Grid2D<CellType> grid)
        {
            var width = Bounds.size.x;
            var height = Bounds.size.y;

            Cells = new Cell[width, height];

            foreach (var globalPoint in _globalPoints)
            {
                var localPoint = GetLocalPointFromGlobal(globalPoint);

                if (grid[globalPoint] == CellType.Hallway)
                {
                    Cells[localPoint.x, localPoint.y] = cells[globalPoint];
                }
            }
        }


        public Vector2Int GetLocalPointFromGlobal(Vector2Int point)
        {
            return point - Bounds.min;
        }

        public Vector2Int GetGlobalPointFromLocal(Vector2Int point)
        {
            return point + Bounds.min;
        }

        private readonly string _name;

        public override string ToString()
        {
            return _name;
        }
    }
}