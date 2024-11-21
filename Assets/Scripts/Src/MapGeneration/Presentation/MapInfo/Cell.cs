using UnityEngine;

namespace MapGeneration.Presentation.MapInfo
{
    public class Cell
    {
        public Wall Right { get; set; } = new EmptyWall();
        public Wall Left { get; set; } = new EmptyWall();
        public Wall Forward { get; set; } = new EmptyWall();
        public Wall Backward { get; set; } = new EmptyWall();


        public Transform Place(int localX, int localZ)
        {
            var cellObject = new GameObject($"Cell_{localX}_{localZ}");

            cellObject.transform.localPosition = new Vector3(localX, 0, localZ);

            var floor = Object.Instantiate(GameResources.cube, cellObject.transform);
            floor.GetComponent<Transform>().localScale = new Vector3(1, 0.1f, 1);
            floor.GetComponent<MeshRenderer>().material = GameResources.Red;

            if (Right is not EmptyWall)
                PlaceWall(nameof(Right), cellObject.transform, Vector3.right * 0.5f);
            if (Left is not EmptyWall)
                PlaceWall(nameof(Left), cellObject.transform, Vector3.left * 0.5f);
            if (Forward is not EmptyWall)
                PlaceWall(nameof(Forward), cellObject.transform, Vector3.forward * 0.5f);
            if (Backward is not EmptyWall)
                PlaceWall(nameof(Backward), cellObject.transform, Vector3.back * 0.5f);

            return cellObject.transform;
        }

        private Transform PlaceWall(string name, Transform parent, Vector3 localOffset)
        {
            var wallObject = Object.Instantiate(GameResources.cube, parent);
            wallObject.transform.localPosition = localOffset + new Vector3(0.5f, 0.5f, 0.5f);
            wallObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            wallObject.GetComponent<MeshRenderer>().material = GameResources.Green;
            wallObject.name = name;

            return wallObject.transform;
        }

    }
}