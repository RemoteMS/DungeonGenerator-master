using UnityEngine;

namespace MapGeneration.Presentation.MapInfo
{
    public abstract class Wall
    {
        public virtual void PlaceWall(string name, Transform parent, Vector3 localOffset)
        {
            var wallObject = Object.Instantiate(GameResources.cube, parent);
            wallObject.transform.localPosition = localOffset + new Vector3(0.5f, 0.5f, 0.5f);
            wallObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            wallObject.GetComponent<MeshRenderer>().material = GetMaterial();
            wallObject.name = name;
        }

        protected abstract Material GetMaterial();
    }

    public class SimpleWall : Wall
    {
        protected override Material GetMaterial()
        {
            return GameResources.Green;
        }
    }

    public class Door : Wall
    {
        protected override Material GetMaterial()
        {
            return GameResources.Yellow;
        }
    }

    public class EmptyWall : Wall
    {
        protected override Material GetMaterial()
        {
            throw new System.NotImplementedException();
        }

        public override void PlaceWall(string name, Transform parent, Vector3 localOffset)
        {
        }
    }

}