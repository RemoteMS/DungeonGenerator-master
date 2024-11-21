using System.Collections.Generic;
using System.Linq;
using Graphs;
using Graphs.Src.Helpers;
using MapGeneration.Presentation.Enums;
using MapGeneration.Presentation.Subsidiary;
using MapGeneration.Settings;
using UnityEngine;
using Random = System.Random;

namespace MapGeneration.Presentation.MapInfo
{
    public class MapGenerator : IMapGenerator
    {
        private readonly MapGeneratorSettings _settings;

        public Random Random { get; private set; }
        private Grid2D<CellType> _grid;

        private List<Path> _paths;
        private List<RoomData> _rooms;
        private Delaunay2D _delaunay;
        private HashSet<Prim.Edge> _selectedEdges;

        private BaseRoomPlacer _roomPlacer;


        public MapGenerator(MapGeneratorSettings mapGeneratorSettings)
        {
            _settings = mapGeneratorSettings;
        }

        public void Generate()
        {
            Random = new Random(_settings.seed);
            _grid = new Grid2D<CellType>(_settings.size, Vector2Int.zero);

            PlaceRooms();
            ChangeGrid();
            DrawRooms();
            Triangulate();
            CreateHallways();
            PathfindHallways();
            DrawHallways();
        }


        private void PlaceRooms()
        {
            _roomPlacer = new RandomBaseRoomPlacer(this, _settings);
            _rooms = _roomPlacer.PlaceRooms();
        }

        private void ChangeGrid()
        {
            foreach (var roomData in _rooms)
            {
                foreach (var pos in roomData.Bounds.allPositionsWithin)
                {
                    _grid[pos] = CellType.Room;
                }
            }
        }

        private void DrawRooms()
        {
            foreach (var roomData in _rooms)
            {
                MapElementDrawer.DrawRoom(roomData.Bounds.position, roomData.Bounds.size);
            }
        }


        private void Triangulate()
        {
            var vertices = new List<Vertex>();

            foreach (var room in _rooms)
            {
                vertices.Add(
                    new Vertex<RoomData>((Vector2)room.Bounds.position + ((Vector2)room.Bounds.size) / 2, room));
            }

            _delaunay = Delaunay2D.Triangulate(vertices);
        }

        private void CreateHallways()
        {
            var edges = new List<Prim.Edge>();

            foreach (var edge in _delaunay.Edges)
            {
                edges.Add(new Prim.Edge(edge.U, edge.V));
            }

            var mst = Prim.MinimumSpanningTree(edges, edges[0].U);

            _selectedEdges = new HashSet<Prim.Edge>(mst);
            var remainingEdges = new HashSet<Prim.Edge>(edges);
            remainingEdges.ExceptWith(_selectedEdges);

            foreach (var edge in remainingEdges)
            {
                if (Random.NextDouble() < 0.125)
                {
                    _selectedEdges.Add(edge);
                }
            }
        }

        private void PathfindHallways()
        {
            var aStar = new DungeonPathfinder2D(_settings.size);

            var paths = new List<Path>();

            foreach (var edge in _selectedEdges)
            {
                var startRoom = (edge.U as Vertex<RoomData>).Item;
                var endRoom = (edge.V as Vertex<RoomData>).Item;

                var startPosf = startRoom.Bounds.center;
                var endPosf = endRoom.Bounds.center;
                var startPos = new Vector2Int((int)startPosf.x, (int)startPosf.y);
                var endPos = new Vector2Int((int)endPosf.x,     (int)endPosf.y);

                var path = aStar.FindPath(startPos, endPos, (DungeonPathfinder2D.Node a, DungeonPathfinder2D.Node b) =>
                {
                    var pathCost = new DungeonPathfinder2D.PathCost();

                    pathCost.Cost = Vector2Int.Distance(b.Position, endPos); //heuristic

                    if (_grid[b.Position] == CellType.Room)
                    {
                        pathCost.Cost += 10;
                    }
                    else if (_grid[b.Position] == CellType.None)
                    {
                        pathCost.Cost += 5;
                    }
                    else if (_grid[b.Position] == CellType.Hallway)
                    {
                        pathCost.Cost += 1;
                    }

                    pathCost.Traversable = true;

                    return pathCost;
                });

                if (path != null)
                {
                    for (var i = 0; i < path.Count; i++)
                    {
                        var current = path[i];

                        if (_grid[current] == CellType.None)
                        {
                            _grid[current] = CellType.Hallway;
                        }

                        if (i > 0)
                        {
                            var prev = path[i - 1];

                            var delta = current - prev;

                            Debug.DrawLine(prev.ToVector3() + new Vector3(0.5f, 0.5f, 0.5f),
                                current.ToVector3()         + new Vector3(0.5f, 0.5f, 0.5f),
                                Color.magenta, 100, false);
                        }
                    }

                    paths.Add(new Path(path));
                }
            }


            paths = ValidateAndSplitPaths(paths, _grid);
            EnsureUniquePaths(paths, _grid);

            _paths = paths;
        }


        private static List<Path> ValidateAndSplitPaths(List<Path> paths, Grid2D<CellType> grid)
        {
            var resultPaths = new List<Path>();

            foreach (var path in paths)
            {
                var points = path.Points;
                var currentPathPoints = new List<Vector2Int>();
                var isNewPath = true;

                foreach (var point in points)
                {
                    var cellType = grid[point.x, point.y];

                    if (cellType == CellType.Room)
                    {
                        if (isNewPath)
                        {
                            currentPathPoints = new List<Vector2Int> { point };
                            isNewPath = false;
                        }
                        else
                        {
                            currentPathPoints.Add(point);
                            resultPaths.Add(new Path(new List<Vector2Int>(currentPathPoints)));

                            currentPathPoints = new List<Vector2Int> { point };
                        }
                    }
                    else if (cellType == CellType.Hallway)
                    {
                        if (!isNewPath)
                        {
                            currentPathPoints.Add(point);
                        }
                    }
                }

                if (currentPathPoints.Count > 1)
                {
                    resultPaths.Add(new Path(currentPathPoints));
                }
            }

            return resultPaths;
        }

        private void EnsureUniquePaths(List<Path> paths, Grid2D<CellType> grid)
        {
            for (var i = 0; i < paths.Count; i++)
            {
                for (var j = i + 1; j < paths.Count; j++)
                {
                    var intersection = paths[i].Points
                        .Intersect(paths[j].Points)
                        .Where(pos => grid[pos.x, pos.y] == CellType.Hallway)
                        .ToList();

                    if (intersection.Any())
                    {
                        paths[i].Merge(paths[j]);
                        paths.RemoveAt(j);
                        j--;
                    }
                }
            }

            paths.RemoveAll(path => path.Points.All(pos => grid[pos.x, pos.y] != CellType.Hallway));
        }

        private void DrawHallways()
        {
            foreach (var path in _paths)
            {
                path.DrawPath();
                MapElementDrawer.DrawHallwayLocally(path, _grid);
            }
        }
    }
}