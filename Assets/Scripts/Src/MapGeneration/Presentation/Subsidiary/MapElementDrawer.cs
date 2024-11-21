using Helpers;
using MapGeneration.Presentation.Enums;
using MapGeneration.Presentation.MapInfo;
using UnityEngine;

namespace MapGeneration.Presentation.Subsidiary
{
    public class MapElementDrawer
    {
        public static void DrawRoom(RoomData roomData, Vector2Int size)
        {
            var roomObject = new GameObject($"Room_{roomData.Bounds.position.x}_{roomData.Bounds.position.y}");

            for (var x = 0; x < roomData.Cells.GetLength(0); x++)
            {
                for (var y = 0; y < roomData.Cells.GetLength(1); y++)
                {
                    var cell = roomData.Cells[x, y];

                    var cellTransform = cell.Place(x, y);
                    cellTransform.parent = roomObject.transform;
                }
            }

            roomObject.transform.position = roomData.Bounds.position.ToVector3();
        }

        public static void DrawHallwayLocally(Path path, Grid2D<CellType> grid, int i = 0)
        {
            var pathPos = path.GetMinPoint().ToVector3();

            var p = new GameObject(path.ToString())
            {
                transform =
                {
                    position = pathPos + new Vector3Int(0, i, 0)
                }
            };

            foreach (var point in path.Points)
            {
                if (grid[point] != CellType.Hallway) continue;

                var localPosition = new Vector3(point.x, 0, point.y) - pathPos;

                GameObject go;
                go = Object.Instantiate(GameResources.cube, p.transform);
                go.transform.localPosition = localPosition;
                go.transform.localScale = new Vector3(1, 1, 1);

                go.GetComponent<MeshRenderer>().material = GameResources.Blue;
            }
        }

        public void DrawElement()
        {
        }
    }
}