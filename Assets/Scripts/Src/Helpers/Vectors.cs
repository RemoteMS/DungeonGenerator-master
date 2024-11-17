using UnityEngine;

namespace Graphs.Src.Helpers
{
    public static class Vectors
    {
        public static Vector3Int ToVector3(this Vector2Int vector, int y = 0)
        {
            return new Vector3Int(vector.x, y, vector.y);
        }
    }
}