using Helpers;
using MapGeneration.Presentation.MapInfo;
using UnityEngine;

namespace MapGeneration.Presentation.Subsidiary
{
    public class MapElementDrawer
    {
        public static void DrawLocally(IPlaceable placeable, Transform mapObjectTransform, int i = 0)
        {
            var placeablePos = placeable.Bounds.min.ToVector3();

            var placeableGo = new GameObject($"{placeable.GetType()}_[{placeable}]")
            {
                transform =
                {
                    position = placeablePos + new Vector3Int(0, i, 0)
                }
            };

            for (var x = 0; x < placeable.Cells.GetLength(0); x++)
            {
                for (var y = 0; y < placeable.Cells.GetLength(1); y++)
                {
                    var cell = placeable.Cells[x, y];

                    cell?.Place(x, y, placeableGo.transform, GameResources.Blue);
                }
            }

            placeableGo.transform.parent = mapObjectTransform;
        }
    }
}