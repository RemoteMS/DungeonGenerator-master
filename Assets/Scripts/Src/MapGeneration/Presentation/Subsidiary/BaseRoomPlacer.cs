using System.Collections.Generic;
using MapGeneration.Presentation.MapInfo;
using MapGeneration.Settings;

namespace MapGeneration.Presentation.Subsidiary
{
    public abstract class BaseRoomPlacer
    {
        protected MapGeneratorSettings Settings;
        protected IMapGenerator MapGenerator;

        protected BaseRoomPlacer(IMapGenerator mapGenerator, MapGeneratorSettings settings)
        {
            MapGenerator = mapGenerator;
            Settings = settings;
        }

        public abstract List<RoomData> PlaceRooms();
    }
}