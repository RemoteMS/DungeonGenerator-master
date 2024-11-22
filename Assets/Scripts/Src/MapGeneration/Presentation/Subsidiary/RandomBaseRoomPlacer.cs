using System.Collections.Generic;
using MapGeneration.Presentation.MapInfo;
using MapGeneration.Settings;
using UnityEngine;

namespace MapGeneration.Presentation.Subsidiary
{
    public class RandomBaseRoomPlacer : BaseRoomPlacer
    {
        public RandomBaseRoomPlacer(IMapGenerator mapGenerator, MapGeneratorSettings settings)
            : base(mapGenerator, settings)
        {
        }

        public override List<MapGenerator.Room> PlaceRooms()
        {
            var rooms = new List<MapGenerator.Room>();

            for (var i = 0; i < Settings.roomCount; i++)
            {
                var location = new Vector2Int(
                    MapGenerator.Random.Next(0, Settings.size.x),
                    MapGenerator.Random.Next(0, Settings.size.y)
                );

                var roomSize = new Vector2Int(
                    MapGenerator.Random.Next(Settings.roomMinSize.x, Settings.roomMaxSize.x + 1),
                    MapGenerator.Random.Next(Settings.roomMinSize.y, Settings.roomMaxSize.y + 1)
                );

                var add = true;
                var newRoom = new MapGenerator.Room(location,                         roomSize);
                var buffer = new MapGenerator.Room(location + new Vector2Int(-1, -1), roomSize + new Vector2Int(2, 2));

                foreach (var room in rooms)
                {
                    if (MapInfo.MapGenerator.Room.Intersect(room, buffer))
                    {
                        add = false;
                        break;
                    }
                }

                if (newRoom.Bounds.xMin    < 0
                    || newRoom.Bounds.xMax >= Settings.size.x
                    || newRoom.Bounds.yMin < 0
                    || newRoom.Bounds.yMax >= Settings.size.y
                )
                {
                    add = false;
                }

                if (add)
                {
                    rooms.Add(newRoom);
                }
            }

            return rooms;
        }
    }
}