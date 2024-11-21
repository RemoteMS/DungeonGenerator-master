namespace MapGeneration.Presentation.MapInfo
{
    public abstract class Wall
    {
        public abstract string Type { get; }
    }

    public class SimpleWall : Wall
    {
        public override string Type => "Wall";
    }

    public class Door : Wall
    {
        public override string Type => "Door";
    }

    public class EmptyWall : Wall
    {
        public override string Type => "Empty";
    }

}