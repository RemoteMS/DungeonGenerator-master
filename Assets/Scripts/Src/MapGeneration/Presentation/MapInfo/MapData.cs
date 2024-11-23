using System.Collections.Generic;

namespace MapGeneration.Presentation.MapInfo
{
    public class MapData : IMap
    {
        public List<RoomData> Rooms { get; }
        public List<HallwayData> Hallways { get; }

        public MapData(List<RoomData> rooms, List<HallwayData> hallways)
        {
            Rooms = rooms;
            Hallways = hallways;
        }
    }
}