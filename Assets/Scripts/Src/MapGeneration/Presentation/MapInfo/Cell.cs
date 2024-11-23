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
        }

        private void InstantiateFloor(Transform parent, Material material = null)
        {
            var floor = Object.Instantiate(GameResources.Src.Dungeon.modular_dungeon_kit.Prefabs.PlaneFloor,
                parent
            );

            floor.GetComponent<MeshRenderer>().material = !material ? GameResources.Red : material;

            floor.name = "Floor";
        }

        public Transform Place(Vector3 basePosition, float instanceScale = 1.0f, float positionScale = 1.0f)
        {
            var finalPosition = basePosition * positionScale;

            var cellObject = new GameObject("Cell");
            cellObject.transform.position = finalPosition;

            var floor = Object.Instantiate(GameResources.cube, cellObject.transform);
            floor.transform.localScale = new Vector3(instanceScale, 0.1f * instanceScale, instanceScale);
            floor.GetComponent<MeshRenderer>().material = GameResources.Red;

            if (Right != WallType.None)
                PlaceWall(cellObject.transform, Vector3.right * (instanceScale / 2), Quaternion.Euler(0, 90, 0), Right,
                    instanceScale);

            if (Left != WallType.None)
                PlaceWall(cellObject.transform, Vector3.left * (instanceScale / 2), Quaternion.Euler(0, 90, 0), Left,
                    instanceScale);

            if (Forward != WallType.None)
                PlaceWall(cellObject.transform, Vector3.forward * (instanceScale / 2), Quaternion.identity, Forward,
                    instanceScale);

            if (Backward != WallType.None)
                PlaceWall(cellObject.transform, Vector3.back * (instanceScale / 2), Quaternion.identity, Backward,
                    instanceScale);

            return cellObject.transform;
        }

        private void PlaceWall(Transform parent, Vector3 position, Quaternion rotation, WallType wallType,
            float instanceScale)
        {
            var wall = Object.Instantiate(GameResources.cube, parent);
            wall.transform.localPosition = position;
            wall.transform.localRotation = rotation;

            wall.transform.localScale = new Vector3(0.1f * instanceScale, instanceScale, instanceScale);

            switch (wallType)
            {
                case WallType.Wall:
                    wall.GetComponent<MeshRenderer>().material = GameResources.Green; // Стена
                    break;
                case WallType.Door:
                    wall.GetComponent<MeshRenderer>().material = GameResources.Blue; // Дверь
                    wall.transform.localScale =
                        new Vector3(0.1f * instanceScale, instanceScale / 2, instanceScale / 2); // Размер двери
                    break;
                case WallType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(wallType), wallType, null);
            }

            wall.name = wallType.ToString();
        }
    }
}