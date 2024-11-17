using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using Graphs;
using Graphs.Src.Helpers;

public class Generator2D : MonoBehaviour
{
    private enum CellType
    {
        None,
        Room,
        Hallway
    }

    private class Room
    {
        public RectInt Bounds;

        public Room(Vector2Int location, Vector2Int size)
        {
            Bounds = new RectInt(location, size);
        }

        public static bool Intersect(Room a, Room b)
        {
            return !((a.Bounds.position.x >= (b.Bounds.position.x + b.Bounds.size.x)) ||
                     ((a.Bounds.position.x                        + a.Bounds.size.x) <= b.Bounds.position.x)
                     || (a.Bounds.position.y >= (b.Bounds.position.y + b.Bounds.size.y)) ||
                     ((a.Bounds.position.y + a.Bounds.size.y) <= b.Bounds.position.y));
        }
    }


    [SerializeField] private bool randomGenerate;
    [SerializeField] private int seed = 0;
    [SerializeField] private Vector2Int size;
    [SerializeField] private int roomCount;
    [SerializeField] private Vector2Int roomMinSize;
    [SerializeField] private Vector2Int roomMaxSize;
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private Material redMaterial;
    [SerializeField] private Material blueMaterial;

    private Random _random;
    private Grid2D<CellType> _grid;
    private List<Room> _rooms;
    private Delaunay2D _delaunay;
    private HashSet<Prim.Edge> _selectedEdges;

    private void Start()
    {
        Generate();
    }

    private void Generate()
    {
        if (randomGenerate)
        {
            var localTime = DateTimeOffset.Now;
            seed = (int)localTime.ToUnixTimeSeconds();
        }

        _random = new Random(seed);
        _grid = new Grid2D<CellType>(size, Vector2Int.zero);
        _rooms = new List<Room>();

        PlaceRooms();
        Triangulate();
        CreateHallways();
        PathfindHallways();
    }

    private void PlaceRooms()
    {
        for (var i = 0; i < roomCount; i++)
        {
            var location = new Vector2Int(
                _random.Next(0, size.x),
                _random.Next(0, size.y)
            );

            var roomSize = new Vector2Int(
                _random.Next(roomMinSize.x, roomMaxSize.x + 1),
                _random.Next(roomMinSize.y, roomMaxSize.y + 1)
            );

            var add = true;
            var newRoom = new Room(location,                         roomSize);
            var buffer = new Room(location + new Vector2Int(-1, -1), roomSize + new Vector2Int(2, 2));

            foreach (var room in _rooms)
            {
                if (Room.Intersect(room, buffer))
                {
                    add = false;
                    break;
                }
            }

            if (newRoom.Bounds.xMin < 0 || newRoom.Bounds.xMax >= size.x
                                        || newRoom.Bounds.yMin < 0 || newRoom.Bounds.yMax >= size.y)
            {
                add = false;
            }

            if (add)
            {
                _rooms.Add(newRoom);
                PlaceRoom(newRoom.Bounds.position, newRoom.Bounds.size);

                foreach (var pos in newRoom.Bounds.allPositionsWithin)
                {
                    _grid[pos] = CellType.Room;
                }
            }
        }
    }

    private void Triangulate()
    {
        var vertices = new List<Vertex>();

        foreach (var room in _rooms)
        {
            vertices.Add(new Vertex<Room>((Vector2)room.Bounds.position + ((Vector2)room.Bounds.size) / 2, room));
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
            if (_random.NextDouble() < 0.125)
            {
                _selectedEdges.Add(edge);
            }
        }
    }

    private void PathfindHallways()
    {
        var aStar = new DungeonPathfinder2D(size);

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

                foreach (var pos in path)
                {
                    if (_grid[pos] == CellType.Hallway)
                    {
                        PlaceHallway(pos);
                    }
                }
            }
        }
    }

    private void PlaceCube(Vector2Int location, Vector2Int size, Material material)
    {
        var go = Instantiate(cubePrefab, new Vector3(location.x, 0, location.y), Quaternion.identity);
        go.GetComponent<Transform>().localScale = new Vector3(size.x, 1, size.y);
        go.GetComponent<MeshRenderer>().material = material;
    }

    private void PlaceRoom(Vector2Int location, Vector2Int size)
    {
        PlaceCube(location, size, redMaterial);
    }

    private void PlaceHallway(Vector2Int location)
    {
        PlaceCube(location, new Vector2Int(1, 1), blueMaterial);
    }
}