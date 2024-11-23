using MapGeneration.Presentation.MapInfo;

namespace MapGeneration.Presentation
{
    public interface IPlaceable
    {
        Cell[,] Cells { get; }
    }
}