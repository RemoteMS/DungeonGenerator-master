using Helpers;
using MapGeneration.Presentation.MapInfo;
using UnityEngine;

namespace MapGeneration.Presentation.Subsidiary
{
    public class MapElementDrawer
    {
        public static void DrawLocally(IPlaceable placeable, Transform mapObjectTransform, int i = 0,
            float cellSize = 1f)
        {
            var placeablePos = placeable.Bounds.min.ToVector3();

            var placeableGo = new GameObject($"{placeable.GetType()}_[{placeable}]")
            {
                transform =
                {
                    position = new Vector3(placeablePos.x * cellSize, placeablePos.y, placeablePos.z * cellSize) +
                               new Vector3Int(0, i, 0)
                }
            };

            for (var x = 0; x < placeable.Cells.GetLength(0); x++)
            {
                for (var y = 0; y < placeable.Cells.GetLength(1); y++)
                {
                    var cell = placeable.Cells[x, y];

                    cell?.Place(x, y, placeableGo.transform, cellSize: cellSize);
                }
            }

            placeableGo.transform.parent = mapObjectTransform;
            placeableGo.isStatic = true;
        }
    }
}