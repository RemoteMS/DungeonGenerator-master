using UnityEngine;

namespace MapGeneration.Settings
{
    [System.Serializable]
    public class MapGeneratorSettings
    {
        public int seed = 0;
        public Vector2Int size;
        public int roomCount;
        public Vector2Int roomMinSize;
        public Vector2Int roomMaxSize;
    }
}