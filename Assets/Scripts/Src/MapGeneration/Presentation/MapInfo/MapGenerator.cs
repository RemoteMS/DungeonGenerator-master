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
        private readonly MapGeneratorSettings _settings;

        public Random Random { get; private set; }
        private Grid2D<CellType> _grid;
        private Grid2D<Cell> _cellGrid;

        private List<Path> _paths;
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

            PlaceCells();

            DrawHallways();
            DrawRooms();

            var mapData = new MapData();
            return mapData;
        }

        private void PlaceRooms()
        {
            _roomPlacer = new RandomBaseRoomPlacer(this, _settings);
            _rooms = _roomPlacer.PlaceRooms();
        }

        private void ChangeGridToRooms()
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
                MapElementDrawer.DrawRoom(roomData, roomData.Bounds.size);
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

            foreach (var roomData in _rooms)
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
                                cell.Left = new Door();
                                break;
                            case Direction.Backward:
                                cell.Backward = new Door();
                                break;
                            case Direction.Right:
                                cell.Right = new Door();
                                break;
                            case Direction.Forward:
                                cell.Forward = new Door();
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
                                cell.Left = new Door();
                                break;
                            case Direction.Backward:
                                cell.Backward = new Door();
                                break;
                            case Direction.Right:
                                cell.Right = new Door();
                                break;
                            case Direction.Forward:
                                cell.Forward = new Door();
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