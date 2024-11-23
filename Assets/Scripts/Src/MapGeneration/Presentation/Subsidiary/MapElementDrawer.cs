using Helpers;
using MapGeneration.Presentation.Enums;
using MapGeneration.Presentation.MapInfo;
using UnityEngine;

namespace MapGeneration.Presentation.Subsidiary
{
    public class MapElementDrawer
    {
        public static void DrawRoom(RoomData roomData)
        {
            var roomObject = new GameObject($"Room_{roomData.Bounds.position.x}_{roomData.Bounds.position.y}");
            roomObject.transform.position = roomData.Bounds.position.ToVector3();

            for (var x = 0; x < roomData.Cells.GetLength(0); x++)
            {
                for (var y = 0; y < roomData.Cells.GetLength(1); y++)
                {
                    var cell = roomData.Cells[x, y];

                    cell.Place(x, y, roomObject.transform);
                }
            }
        }

        public static void DrawHallwayLocally(HallwayData hallway, int i = 0)
        {
            var hallwayPos = hallway.Bounds.min.ToVector3();

            var hallwayGo = new GameObject($"Hallway_[{hallway.ToString()}]")
            {
                transform =
                {
                    position = hallwayPos + new Vector3Int(0, i, 0)
                }
            };

            for (var x = 0; x < hallway.Cells.GetLength(0); x++)
            {
                for (var y = 0; y < hallway.Cells.GetLength(1); y++)
                {
                    var cell = hallway.Cells[x, y];

                    if (cell == null) continue;

                    cell.Place(x, y, hallwayGo.transform, GameResources.Blue);
                }
            }
        }

        public void DrawElement()
        {
        }
    }
}