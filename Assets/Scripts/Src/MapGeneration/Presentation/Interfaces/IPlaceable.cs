using MapGeneration.Presentation.MapInfo;
using UnityEngine;

namespace MapGeneration.Presentation
{
    public interface IPlaceable
    {
        public int Id { get; }
        RectInt Bounds { get; }
        Cell[,] Cells { get; }

        string GetPlaceableType();
    }
}