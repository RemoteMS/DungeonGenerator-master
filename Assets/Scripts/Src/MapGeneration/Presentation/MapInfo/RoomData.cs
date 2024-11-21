using System.Collections.Generic;
using UnityEngine;

namespace MapGeneration.Presentation.MapInfo
{
    public class RoomData : IRoom
    {
        public RectInt Bounds { get; private set; }

        private List<Vector3> _doors;
        private Vector3Int _position;
        private Vector3Int _size;

        public RoomData(Vector2Int location, Vector2Int size)
        {
            Bounds = new RectInt(location, size);
            _doors = new List<Vector3>();
        }

        public static bool Intersect(RoomData a, RoomData b)
        {
            return !(a.Bounds.position.x >= b.Bounds.position.x + b.Bounds.size.x ||
                     a.Bounds.position.x                        + a.Bounds.size.x <= b.Bounds.position.x ||
                     a.Bounds.position.y >= b.Bounds.position.y + b.Bounds.size.y ||
                     a.Bounds.position.y + a.Bounds.size.y <= b.Bounds.position.y);
        }
    }
}