using Graphs.Src.Helpers;
using MapGeneration.Presentation.Enums;
using UnityEngine;

namespace MapGeneration.Presentation.Subsidiary
{
    public class MapElementDrawer
    {
        public static void DrawRoom(Vector2Int location, Vector2Int size)
        {
            GameObject go;
            go = Object.Instantiate(GameResources.cube, new Vector3(location.x, 0, location.y), Quaternion.identity);
            go.GetComponent<Transform>().localScale = new Vector3(size.x, 1, size.y);
            go.GetComponent<MeshRenderer>().material = GameResources.Red;
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