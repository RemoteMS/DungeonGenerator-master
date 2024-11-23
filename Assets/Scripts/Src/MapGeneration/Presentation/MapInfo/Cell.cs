using UnityEngine;

namespace MapGeneration.Presentation.MapInfo
{
    public class Cell
    {
        public Wall Right { get; set; } = new EmptyWall();
        public Wall Left { get; set; } = new EmptyWall();
        public Wall Forward { get; set; } = new EmptyWall();
        public Wall Backward { get; set; } = new EmptyWall();


        public void Place(int localX, int localZ, Transform parent, Material material = null)
        {
            var cellObject = new GameObject($"Cell_{localX}_{localZ}");

            cellObject.transform.parent = parent;
            cellObject.transform.localPosition = new Vector3(localX, 0, localZ);

            var floor = Object.Instantiate(GameResources.cube, cellObject.transform);
            floor.GetComponent<Transform>().localScale = new Vector3(1, 0.1f, 1);

            floor.GetComponent<MeshRenderer>().material = material == null ? GameResources.Red : material;

            floor.name = "Floor";

            Right.PlaceWall(nameof(Right), cellObject.transform, Vector3.right       * 0.5f);
            Left.PlaceWall(nameof(Left), cellObject.transform, Vector3.left          * 0.5f);
            Forward.PlaceWall(nameof(Forward), cellObject.transform, Vector3.forward * 0.5f);
            Backward.PlaceWall(nameof(Backward), cellObject.transform, Vector3.back  * 0.5f);
        }
    }
}