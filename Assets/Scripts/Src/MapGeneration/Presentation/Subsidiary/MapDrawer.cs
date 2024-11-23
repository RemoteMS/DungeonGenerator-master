using MapGeneration.Presentation.MapInfo;
using UnityEngine;

namespace MapGeneration.Presentation.Subsidiary
{
    public class MapDrawer : IMapDrawer
    {
        public GameObject Draw(MapData mapData)
        {
            var mapObject = new GameObject("mapObject");

            foreach (var hallway in mapData.Hallways)
            {
                MapElementDrawer.DrawLocally(hallway, mapObject.transform);
            }

            foreach (var roomData in mapData.Rooms)
            {
                MapElementDrawer.DrawLocally(roomData, mapObject.transform);
            }

            return mapObject;
        }
    }
}