using System;
using Helpers;
using UnityEngine;

namespace MapGeneration.Presentation.MapInfo
{
    public class RoomData : IRoom, IPlaceable, IEquatable<RoomData>
    {
        public int Id { get; }
        public RectInt Bounds { get; }
        public Cell[,] Cells { get; private set; }

        private Vector3Int _position;
        private Vector3Int _size;

        public RoomData(int id, RectInt bounds, Grid2D<Cell> cells)
        {
            Id = id;
            Bounds = bounds;
            _position = bounds.position.ToVector3();

            CopyCells(cells);
            AddBoundaryWalls();

            _name = $"pos_{Bounds.position}_size_{Bounds.size}";
        }

        private void CopyCells(Grid2D<Cell> cells)
        {
            var width = Bounds.size.x;
            var height = Bounds.size.y;

            Cells = new Cell[width, height];

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    Cells[x, y] = cells[x + Bounds.xMin, y + Bounds.yMin];
                }
            }
        }

        private void AddBoundaryWalls()
        {
            var width = Bounds.size.x;
            var height = Bounds.size.y;

            for (var x = 0; x < width; x++)
            {
                if (Cells[x, 0].Backward == WallType.None)
                    Cells[x, 0].Backward = WallType.Wall;
                if (Cells[x, height - 1].Forward == WallType.None)
                    Cells[x, height - 1].Forward = WallType.Wall;
            }

            for (var y = 0; y < height; y++)
            {
                if (Cells[0, y].Left == WallType.None)
                    Cells[0, y].Left = WallType.Wall;
                if (Cells[width - 1, y].Right == WallType.None)
                    Cells[width - 1, y].Right = WallType.Wall;
            }
        }

        private readonly string _name;

        public override string ToString()
        {
            return _name;
        }

        public bool Equals(RoomData other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((RoomData)obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public string GetPlaceableType()
        {
            return nameof(RoomData);
        }
    }
}