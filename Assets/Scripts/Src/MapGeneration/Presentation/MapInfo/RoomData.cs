using System;
using Helpers;
using MapGeneration.Helpers;
using UnityEngine;

namespace MapGeneration.Presentation.MapInfo
{
    public class RoomData : IRoom
    {
        public RectInt Bounds { get; private set; }
        public Cell[,] Cells { get; private set; }

        private Vector3Int _position;
        private Vector3Int _size;

        public RoomData(Vector2Int location, Vector2Int size)
        {
            Bounds = new RectInt(location, size);
            _position = location.ToVector3();

            InitializeCells();
            AddBoundaryWalls();
        }

        public static bool Intersect(RoomData a, RoomData b)
        {
            return !(a.Bounds.position.x >= b.Bounds.position.x + b.Bounds.size.x ||
                     a.Bounds.position.x                        + a.Bounds.size.x <= b.Bounds.position.x ||
                     a.Bounds.position.y >= b.Bounds.position.y + b.Bounds.size.y ||
                     a.Bounds.position.y + a.Bounds.size.y <= b.Bounds.position.y);
        }

        private void InitializeCells()
        {
            var width = Bounds.size.x;
            var height = Bounds.size.y;

            Cells = new Cell[width, height];

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    Cells[x, y] = new Cell();
                }
            }
        }

        private void AddBoundaryWalls()
        {
            var width = Bounds.size.x;
            var height = Bounds.size.y;

            for (var x = 0; x < width; x++)
            {
                Cells[x, 0].Backward = new SimpleWall();
                Cells[x, height - 1].Forward = new SimpleWall();
            }

            for (var y = 0; y < height; y++)
            {
                Cells[0, y].Left = new SimpleWall();
                Cells[width - 1, y].Right = new SimpleWall();
            }
        }

        public void SetWall(Vector2Int cellPosition, Direction direction)
        {
            if (!Bounds.Contains(cellPosition))
                return;

            var localPos = cellPosition - Bounds.min;

            switch (direction)
            {
                case Direction.Right:
                    Cells[localPos.x, localPos.y].Right = new SimpleWall();
                    break;
                case Direction.Left:
                    Cells[localPos.x, localPos.y].Left = new SimpleWall();
                    break;
                case Direction.Forward:
                    Cells[localPos.x, localPos.y].Forward = new SimpleWall();
                    break;
                case Direction.Backward:
                    Cells[localPos.x, localPos.y].Backward = new SimpleWall();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public void SetDoor(Vector2Int cellPosition, Direction direction)
        {
            if (!Bounds.Contains(cellPosition))
                return;

            var localPos = cellPosition - Bounds.min;

            switch (direction)
            {
                case Direction.Right:
                    Cells[localPos.x, localPos.y].Right = new Door();
                    break;
                case Direction.Left:
                    Cells[localPos.x, localPos.y].Left = new Door();
                    break;
                case Direction.Forward:
                    Cells[localPos.x, localPos.y].Forward = new Door();
                    break;
                case Direction.Backward:
                    Cells[localPos.x, localPos.y].Backward = new Door();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
    }
}