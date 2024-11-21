using Random = System.Random;

namespace MapGeneration.Presentation
{
    public interface IMapGenerator
    {
        Random Random { get; }

        void Generate();
    }
}