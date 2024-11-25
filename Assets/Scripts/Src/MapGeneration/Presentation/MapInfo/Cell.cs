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


        public void Place(float localX, float localZ, Transform parent, Material material = null, float cellSize = 1)
        {
            var x = localX * cellSize;
            var z = localZ * cellSize;

            var cellObject = new GameObject($"Cell_{localX}_{localZ}, {x}_{z}");

            cellObject.transform.parent = parent;
            cellObject.transform.localPosition = new Vector3(x, 0, z);

            InstantiateFloor(cellObject.transform, cellSize: cellSize);

            if (Right != WallType.None)
                PlaceWall(
                    cellObject.transform,
                    (Vector3.right + Vector3.forward) * cellSize,
                    Quaternion.Euler(0, 180, 0),
                    Right,
                    nameof(Right)
                );

            if (Left != WallType.None)
                PlaceWall(
                    cellObject.transform,
                    Vector3.zero * cellSize,
                    Quaternion.Euler(0, 0, 0),
                    Left,
                    nameof(Left)
                );

            if (Forward != WallType.None)
                PlaceWall(cellObject.transform,
                    Vector3.forward * cellSize,
                    Quaternion.Euler(0, 90, 0),
                    Forward,
                    nameof(Forward)
                );

            if (Backward != WallType.None)
                PlaceWall(
                    cellObject.transform,
                    Vector3.right * cellSize,
                    Quaternion.Euler(0, -90, 0),
                    Backward,
                    nameof(Backward)
                );

            cellObject.isStatic = true;
        }

        private void InstantiateFloor(Transform parent, Material material = null, float cellSize = 1f)
        {
            var floor = Object.Instantiate(
                GameResources.Prefabs.FloorContainer,
                parent,
                false
            );

            floor.isStatic = true;
            // floor.transform.localPosition += new Vector3(cellSize / 2, 0, cellSize / 2);

            floor.name = "Floor";
        }

        private void PlaceWall(Transform parent, Vector3 position, Quaternion rotation, WallType wallType, string name)
        {
            GameObject wall;

            switch (wallType)
            {
                case WallType.Wall:
                    wall = Object.Instantiate(
                        GameResources.Prefabs.WallContainer,
                        parent
                    );
                    break;
                case WallType.Door:
                    wall = Object.Instantiate(
                        GameResources.Prefabs.DoorContainer,
                        parent
                    );
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(wallType), wallType, null);
            }

            wall.transform.localPosition = position;
            wall.transform.localRotation = rotation;
            wall.name = name;
            wall.isStatic = true;
        }
    }
}