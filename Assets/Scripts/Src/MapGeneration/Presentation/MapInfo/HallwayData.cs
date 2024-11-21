using System.Collections.Generic;
using UnityEngine;

namespace MapGeneration.Presentation.MapInfo
{
    public class HallwayData : IHallway
    {
        private HashSet<Vector2Int> _corridorPoints;
    }
}