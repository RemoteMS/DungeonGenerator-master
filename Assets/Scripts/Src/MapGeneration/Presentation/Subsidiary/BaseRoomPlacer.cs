using System.Collections.Generic;
using MapGeneration.Presentation.MapInfo;
using MapGeneration.Settings;

namespace MapGeneration.Presentation.Subsidiary
{
    public abstract class BaseRoomPlacer
    {
        protected readonly MapGeneratorSettings Settings;
        protected readonly IMapGenerator MapGenerator;

        protected BaseRoomPlacer(IMapGenerator mapGenerator, MapGeneratorSettings settings)
        {
            MapGenerator = mapGenerator;
            Settings = settings;
        }

        public abstract List<MapGenerator.Room> PlaceRooms();
    }
}