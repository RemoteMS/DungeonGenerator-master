using MapGeneration.Presentation.MapInfo;

namespace MapGeneration.Presentation.Subsidiary
{
    public class MapDrawer : IMapDrawer
    {
        public void Draw(MapData mapData)
        {
            foreach (var hallway in mapData.Hallways)
            {
                MapElementDrawer.DrawHallwayLocally(hallway);
            }

            foreach (var roomData in mapData.Rooms)
            {
                MapElementDrawer.DrawRoom(roomData);
            }
        }
    }
}