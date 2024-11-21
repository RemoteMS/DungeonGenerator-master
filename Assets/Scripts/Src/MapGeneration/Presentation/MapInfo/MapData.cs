using System.Collections.Generic;

namespace MapGeneration.Presentation.MapInfo
{
    public class MapData : IMap
    {
        private List<RoomData> _rooms;
        private List<HallwayData> _corridors;
    }
}