using MapGeneration.Presentation.MapInfo;
using UnityEngine;

namespace MapGeneration.Presentation
{
    public interface IPlaceable
    {
        RectInt Bounds { get; }
        Cell[,] Cells { get; }
    }
}