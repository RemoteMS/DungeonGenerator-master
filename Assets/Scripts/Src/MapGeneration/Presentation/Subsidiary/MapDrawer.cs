using MapGeneration.Presentation.MapInfo;
using UnityEngine;

namespace MapGeneration.Presentation.Subsidiary
{
    public class MapDrawer : IMapDrawer
    {
        private float _cellSizeInUnits;

        public MapDrawer(float cellSizeInUnits = 1)
        {
            _cellSizeInUnits = cellSizeInUnits;
        }

        public GameObject Draw(MapData mapData)
        {
            var mapObject = new GameObject("mapObject");

            foreach (var hallway in mapData.Hallways)
            {
                MapElementDrawer.DrawLocally(hallway, mapObject.transform, cellSize: _cellSizeInUnits);
            }

            foreach (var roomData in mapData.Rooms)
            {
                MapElementDrawer.DrawLocally(roomData, mapObject.transform, cellSize: _cellSizeInUnits);
            }

            return mapObject;
        }
    }
}