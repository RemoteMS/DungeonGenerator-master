using UnityEngine;

namespace MapGeneration.Presentation.MapInfo
{
    public abstract class Wall
    {
        public virtual void PlaceWall(string name, Transform parent, Vector3 localOffset)
        {
            var wallObject = Object.Instantiate(GetWallObject(), parent);
            wallObject.transform.localPosition = localOffset + new Vector3(0.5f, 0.5f, 0.5f);
            wallObject.GetComponent<MeshRenderer>().material = GetMaterial();
            wallObject.name = name;
        }

        protected abstract GameObject GetWallObject();

        protected abstract Material GetMaterial();
    }

    public class SimpleWall : Wall
    {
        protected override GameObject GetWallObject()
        {
            return GameResources.Src.Dungeon.modular_dungeon_kit.Prefabs.Wall;
        }

        protected override Material GetMaterial()
        {
            return GameResources.Green;
        }
    }

    public class Door : Wall
    {
        protected override GameObject GetWallObject()
        {
            return GameResources.Src.Dungeon.modular_dungeon_kit.Prefabs.Door;
        }

        protected override Material GetMaterial()
        {
            return GameResources.Yellow;
        }
    }

    public class EmptyWall : Wall
    {
        protected override GameObject GetWallObject()
        {
            throw new System.NotImplementedException();
        }

        protected override Material GetMaterial()
        {
            throw new System.NotImplementedException();
        }

        public override void PlaceWall(string name, Transform parent, Vector3 localOffset)
        {
        }
    }

}