using MapGeneration.Presentation.MapInfo;
using UnityEngine;

namespace MapGeneration.Presentation
{
    public interface IMapDrawer
    {
        GameObject Draw(MapData mapData);
    }
}