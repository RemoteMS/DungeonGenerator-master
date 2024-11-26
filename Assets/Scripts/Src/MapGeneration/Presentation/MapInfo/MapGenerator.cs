using System;
using System.Collections.Generic;
using System.Linq;
using Graphs;
using Helpers;
using MapGeneration.Helpers;
using MapGeneration.Presentation.Enums;
using MapGeneration.Presentation.Subsidiary;
using MapGeneration.Settings;
using UnityEngine;
using Random = System.Random;

namespace MapGeneration.Presentation.MapInfo
{
    public class MapGenerator : IMapGenerator
    {
        public class Room
        {
            public RectInt Bounds;

            public Room(Vector2Int location, Vector2Int size)
            {
                Bounds = new RectInt(location, size);
            }

            public static bool Intersect(Room a, Room b)
            {
                return !(a.Bounds.position.x >= b.Bounds.position.x + b.Bounds.size.x ||
                         a.Bounds.position.x                        + a.Bounds.size.x <= b.Bounds.position.x ||
                         a.Bounds.position.y >= b.Bounds.position.y + b.Bounds.size.y ||
                         a.Bounds.position.y + a.Bounds.size.y <= b.Bounds.position.y);
            }
        }

        private readonly MapGeneratorSettings _settings;

        public Random Random { get; private set; }
        private Grid2D<CellType> _grid;
        private Grid2D<Cell> _cellGrid;

        private List<Path> _paths;
        private List<HallwayData> _hallways;

        private List<Room> _roomsBases;
        private List<RoomData> _rooms;

        private Delaunay2D _delaunay;
        private HashSet<Prim.Edge> _selectedEdges;

        private BaseRoomPlacer _roomPlacer;


        public MapGenerator(MapGeneratorSettings mapGeneratorSettings)
        {
            _settings = mapGeneratorSettings;
        }

        public MapData Generate()
        {
            Random = new Random(_settings.seed);
            _grid = new Grid2D<CellType>(_settings.size, Vector2Int.zero);
            _cellGrid = new Grid2D<Cell>(_settings.size, Vector2Int.zero);

            PlaceRooms();
            ChangeGridToRooms();

            Triangulate();
            CreateHallways();
            PathfindHallways();

            FillCellsGrid();
            ValidateAndSplitPaths();
            PlaceDoors();
            EnsureUniquePaths();
            PlaceHallwaysWalls();

            ConvertRooms();
            ConvertHallways();

            var mapData = new MapData(_rooms, _hallways);
            return mapData;
        }

        private void ConvertRooms()
        {
            _rooms = new List<RoomData>();

            var id = 0;
            foreach (var room in _roomsBases)
            {
                _rooms.Add(new RoomData(id++, room.Bounds, _cellGrid));
            }

            _roomsBases.Clear();
            _roomsBases = null;
        }

        private void ConvertHallways()
        {
            _hallways = new List<HallwayData>();

            var id = 0;
            foreach (var path in _paths)
            {
                _hallways.Add(new HallwayData(id++, path, _cellGrid, _grid));
            }

            _paths.Clear();
            _paths = null;
        }

        private void PlaceRooms()
        {
            _roomPlacer = new RandomBaseRoomPlacer(this, _settings);
            _roomsBases = _roomPlacer.PlaceRooms();
        }

        private void ChangeGridToRooms()
        {
            foreach (var roomData in _roomsBases)
            {
                foreach (var pos in roomData.Bounds.allPositionsWithin)
                {
                    _grid[pos] = CellType.Room;
                }
            }
        }

        private void Triangulate()
        {
            var vertices = new List<Vertex>();

            foreach (var room in _roomsBases)
            {
                vertices.Add(
                    new Vertex<Room>((Vector2)room.Bounds.position + ((Vector2)room.Bounds.size) / 2, room));
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
                var startRoom = (edge.U as Vertex<Room>).Item;
                var endRoom = (edge.V as Vertex<Room>).Item;

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

            _paths = paths;
        }

        private void FillCellsGrid()
        {
            foreach (var path in _paths)
            {
                foreach (var point in path.Points)
                {
                    _cellGrid[point] = new Cell();
                }
            }

            foreach (var roomData in _roomsBases)
            {
                var width = roomData.Bounds.size.x;
                var height = roomData.Bounds.size.y;

                for (var x = 0; x < width; x++)
                {
                    for (var y = 0; y < height; y++)
                    {
                        _cellGrid[x + roomData.Bounds.xMin, y + roomData.Bounds.yMin] = new Cell();
                    }
                }
            }
        }


        private void PlaceDoors()
        {
            foreach (var path in _paths)
            {
                for (var index = 0; index < path.Points.Count - 1; index++)
                {
                    var point = path.Points[index];
                    var pointNext = path.Points[index + 1];

                    if (_grid[point] == CellType.Room && _grid[pointNext] == CellType.Hallway)
                    {
                        var cell = _cellGrid[point];
                        var direction = GetDirection(pointNext - point);

                        switch (direction)
                        {
                            case Direction.Left:
                                cell.Left = WallType.Door;
                                break;
                            case Direction.Backward:
                                cell.Backward = WallType.Door;
                                break;
                            case Direction.Right:
                                cell.Right = WallType.Door;
                                break;
                            case Direction.Forward:
                                cell.Forward = WallType.Door;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    else if (_grid[pointNext] == CellType.Room && _grid[point] == CellType.Hallway)
                    {
                        var cell = _cellGrid[pointNext];
                        var direction = GetDirection(point - pointNext);

                        switch (direction)
                        {
                            case Direction.Left:
                                cell.Left = WallType.Door;
                                break;
                            case Direction.Backward:
                                cell.Backward = WallType.Door;
                                break;
                            case Direction.Right:
                                cell.Right = WallType.Door;
                                break;
                            case Direction.Forward:
                                cell.Forward = WallType.Door;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }
        }

        private Direction GetDirection(Vector2Int direction)
        {
            if (direction == Vector2Int.left) return Direction.Left;
            if (direction == Vector2Int.right) return Direction.Right;
            if (direction == Vector2Int.up) return Direction.Forward;
            if (direction == Vector2Int.down) return Direction.Backward;

            throw new ArgumentException("Wrong direction");
        }

        private void ValidateAndSplitPaths()
        {
            var resultPaths = new List<Path>();

            foreach (var path in _paths)
            {
                var points = path.Points;
                var currentPathPoints = new List<Vector2Int>();
                var isNewPath = true;

                foreach (var point in points)
                {
                    var cellType = _grid[point.x, point.y];

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

            _paths = resultPaths;
        }

        private void EnsureUniquePaths()
        {
            for (var i = 0; i < _paths.Count; i++)
            {
                for (var j = i + 1; j < _paths.Count; j++)
                {
                    var intersection = _paths[i].Points
                        .Intersect(_paths[j].Points)
                        .Where(pos => _grid[pos.x, pos.y] == CellType.Hallway)
                        .ToList();

                    if (intersection.Any())
                    {
                        _paths[i].Merge(_paths[j]);
                        _paths.RemoveAt(j);
                        j--;
                    }
                }
            }

            _paths.RemoveAll(path => path.Points.All(pos => _grid[pos.x, pos.y] != CellType.Hallway));
        }

        private void PlaceHallwaysWalls()
        {
            foreach (var path in _paths)
            {
                foreach (var point in path.Points)
                {
                    var cell = _cellGrid[point];

                    if (_grid[point] != CellType.Hallway) continue;

                    // todo: probably need add check on array edges
                    // todo: refactor later

                    if (
                        _grid[point.x    - 1, point.y] != CellType.Hallway
                        && _grid[point.x - 1, point.y] != CellType.Room
                    )
                    {
                        if (cell.Left == WallType.None)
                        {
                            cell.Left = WallType.Wall;
                        }
                    }

                    if (
                        _grid[point.x    + 1, point.y] != CellType.Hallway
                        && _grid[point.x + 1, point.y] != CellType.Room
                    )
                    {
                        if (cell.Right == WallType.None)
                        {
                            cell.Right = WallType.Wall;
                        }
                    }

                    if (
                        _grid[point.x, point.y    - 1] != CellType.Hallway
                        && _grid[point.x, point.y - 1] != CellType.Room
                    )
                    {
                        if (cell.Backward is WallType.None)
                        {
                            cell.Backward = WallType.Wall;
                        }
                    }

                    if (
                        _grid[point.x, point.y    + 1] != CellType.Hallway
                        && _grid[point.x, point.y + 1] != CellType.Room
                    )
                    {
                        if (cell.Forward == WallType.None)
                        {
                            cell.Forward = WallType.Wall;
                        }
                    }
                }
            }
        }
    }
}