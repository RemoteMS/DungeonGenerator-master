using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MapGeneration.Presentation.MapInfo
{
    public class Cell
    {
        public WallType Right { get; set; } = WallType.None;
        public WallType Left { get; set; } = WallType.None;
        public WallType Forward { get; set; } = WallType.None;
        public WallType Backward { get; set; } = WallType.None;


        public void Place(int localX, int localZ, Transform parent, Material material = null)
        {
            var x = localX;
            var z = localZ;

            var cellObject = new GameObject($"Cell_{x}_{z}");

            cellObject.transform.parent = parent;
            cellObject.transform.localPosition = new Vector3(x, 0, z);

            InstantiateFloor(cellObject.transform);

            if (Right != WallType.None)
                PlaceWall(cellObject.transform, Vector3.right + new Vector3(0, 0, 0.5f), Quaternion.Euler(0, 180, 0),
                    Right);

            if (Left != WallType.None)
                PlaceWall(cellObject.transform, Vector3.zero + new Vector3(0, 0, 0.5f) /*Vector3.left*/,
                    Quaternion.Euler(0, 0, 0),  Left);

            if (Forward != WallType.None)
                PlaceWall(cellObject.transform, Vector3.forward + new Vector3(0.5f, 0, 0), Quaternion.Euler(0, 90, 0),
                    Forward);

            if (Backward != WallType.None)
                PlaceWall(cellObject.transform,  Vector3.zero + new Vector3(0.5f, 0, 0) /* Vector3.back*/,
                    Quaternion.Euler(0, -90, 0), Backward);
        }

        private void InstantiateFloor(Transform parent, Material material = null)
        {
            var floor = Object.Instantiate(GameResources.Src.Dungeon.modular_dungeon_kit.Prefabs.PlaneFloor,
                parent
            );

            floor.GetComponent<MeshRenderer>().material = !material ? GameResources.Red : material;

            floor.name = "Floor";
        }

        private void PlaceWall(Transform parent, Vector3 position, Quaternion rotation, WallType wallType)
        {
            GameObject wall;

            switch (wallType)
            {
                case WallType.Wall:
                    wall = Object.Instantiate(
                        GameResources.Src.Dungeon.modular_dungeon_kit.Prefabs.Wall_1,
                        parent
                    );
                    wall.GetComponent<MeshRenderer>().material = GameResources.Green; // Стена
                    break;
                case WallType.Door:
                    wall = Object.Instantiate(
                        GameResources.Src.Dungeon.modular_dungeon_kit.Prefabs.Door_1,
                        parent
                    );
                    wall.GetComponent<MeshRenderer>().material = GameResources.Blue; // Дверь
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(wallType), wallType, null);
            }

            wall.transform.localPosition = position;
            wall.transform.localRotation = rotation;
            wall.name = wallType.ToString();
        }
    }
}