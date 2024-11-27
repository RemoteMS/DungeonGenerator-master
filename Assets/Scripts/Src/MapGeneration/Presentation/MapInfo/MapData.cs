namespace MapGeneration.Presentation.MapInfo
{
    public class MapData : IMap
    {
        public RoomData[] Rooms { get; }
        public HallwayData[] Hallways { get; }

        public Graph Graph;

        public MapData(RoomData[] rooms, HallwayData[] hallways)
        {
            Rooms = rooms;
            Hallways = hallways;
        }
    }
}