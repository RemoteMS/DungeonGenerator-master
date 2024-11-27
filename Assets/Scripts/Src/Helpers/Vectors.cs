using UnityEngine;

namespace Helpers
{
    public static class Vectors
    {
        public static Vector3Int ToVector3Int(this Vector2Int vector, int y = 0)
        {
            return new Vector3Int(vector.x, y, vector.y);
        }

        public static Vector3 ToVector3(this Vector2Int vector, int y = 0)
        {
            return new Vector3(vector.x, y, vector.y);
        }

        public static Vector3 ToVector3(this Vector2 vector, int y = 0)
        {
            return new Vector3(vector.x, y, vector.y);
        }
    }
}