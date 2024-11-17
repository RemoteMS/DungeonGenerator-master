using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using Graphs;

public class Generator3D : MonoBehaviour
{
    private enum CellType
    {
        None,
        Room,
        Hallway,
        Stairs
    }

    private class Room
    {
        public BoundsInt Bounds;

        public Room(Vector3Int location, Vector3Int size)
        {
            Bounds = new BoundsInt(location, size);
        }

        public static bool Intersect(Room a, Room b)
        {
            return !((a.Bounds.position.x >= (b.Bounds.position.x + b.Bounds.size.x)) ||
                     ((a.Bounds.position.x                        + a.Bounds.size.x) <= b.Bounds.position.x)
                     || (a.Bounds.position.y >= (b.Bounds.position.y + b.Bounds.size.y)) ||
                     ((a.Bounds.position.y + a.Bounds.size.y) <= b.Bounds.position.y)
                     || (a.Bounds.position.z >= (b.Bounds.position.z + b.Bounds.size.z)) ||
                     ((a.Bounds.position.z + a.Bounds.size.z) <= b.Bounds.position.z));
        }
    }

    [SerializeField] private bool randomGenerate;
    [SerializeField] private int seed = 0;
    [SerializeField] private Vector3Int size;
    [SerializeField] private int roomCount;
    [SerializeField] private Vector3Int roomMaxSize;
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private Material redMaterial;
    [SerializeField] private Material blueMaterial;
    [SerializeField] private Material greenMaterial;

    private Random _random;
    private Grid3D<CellType> _grid;
    private List<Room> _rooms;
    private Delaunay3D _delaunay;
    private HashSet<Prim.Edge> _selectedEdges;

    private void Start()
    {
        if (randomGenerate)
        {
            var localTime = DateTimeOffset.Now;
            seed = (int)localTime.ToUnixTimeSeconds();
        }

        _random = new Random(seed);
        _grid = new Grid3D<CellType>(size, Vector3Int.zero);
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
            var location = new Vector3Int(
                _random.Next(0, size.x),
                _random.Next(0, size.y),
                _random.Next(0, size.z)
            );

            var roomSize = new Vector3Int(
                _random.Next(1, roomMaxSize.x + 1),
                _random.Next(1, roomMaxSize.y + 1),
                _random.Next(1, roomMaxSize.z + 1)
            );

            var add = true;
            var newRoom = new Room(location,                            roomSize);
            var buffer = new Room(location + new Vector3Int(-1, 0, -1), roomSize + new Vector3Int(2, 0, 2));

            foreach (var room in _rooms)
            {
                if (Room.Intersect(room, buffer))
                {
                    add = false;
                    break;
                }
            }

            if (newRoom.Bounds.xMin < 0 || newRoom.Bounds.xMax >= size.x
                                        || newRoom.Bounds.yMin < 0 || newRoom.Bounds.yMax >= size.y
                                        || newRoom.Bounds.zMin < 0 || newRoom.Bounds.zMax >= size.z)
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
            vertices.Add(new Vertex<Room>((Vector3)room.Bounds.position + ((Vector3)room.Bounds.size) / 2, room));
        }

        _delaunay = Delaunay3D.Triangulate(vertices);
    }

    private void CreateHallways()
    {
        var edges = new List<Prim.Edge>();

        foreach (var edge in _delaunay.Edges)
        {
            edges.Add(new Prim.Edge(edge.U, edge.V));
        }

        var minimumSpanningTree = Prim.MinimumSpanningTree(edges, edges[0].U);

        _selectedEdges = new HashSet<Prim.Edge>(minimumSpanningTree);
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
        var aStar = new DungeonPathfinder3D(size);

        foreach (var edge in _selectedEdges)
        {
            var startRoom = (edge.U as Vertex<Room>).Item;
            var endRoom = (edge.V as Vertex<Room>).Item;

            var startPosf = startRoom.Bounds.center;
            var endPosf = endRoom.Bounds.center;
            var startPos = new Vector3Int((int)startPosf.x, (int)startPosf.y, (int)startPosf.z);
            var endPos = new Vector3Int((int)endPosf.x,     (int)endPosf.y,   (int)endPosf.z);

            var path = aStar.FindPath(startPos, endPos, (DungeonPathfinder3D.Node a, DungeonPathfinder3D.Node b) =>
            {
                var pathCost = new DungeonPathfinder3D.PathCost();

                var delta = b.Position - a.Position;

                if (delta.y == 0)
                {
                    //flat hallway
                    pathCost.Cost = Vector3Int.Distance(b.Position, endPos); //heuristic

                    if (_grid[b.Position] == CellType.Stairs)
                    {
                        return pathCost;
                    }
                    else if (_grid[b.Position] == CellType.Room)
                    {
                        pathCost.Cost += 5;
                    }
                    else if (_grid[b.Position] == CellType.None)
                    {
                        pathCost.Cost += 1;
                    }

                    pathCost.Traversable = true;
                }
                else
                {
                    //staircase
                    if ((_grid[a.Position]    != CellType.None && _grid[a.Position] != CellType.Hallway)
                        || (_grid[b.Position] != CellType.None && _grid[b.Position] != CellType.Hallway))
                        return pathCost;

                    pathCost.Cost = 100 + Vector3Int.Distance(b.Position, endPos); //base cost + heuristic

                    var xDir = Mathf.Clamp(delta.x, -1, 1);
                    var zDir = Mathf.Clamp(delta.z, -1, 1);
                    var verticalOffset = new Vector3Int(0,      delta.y, 0);
                    var horizontalOffset = new Vector3Int(xDir, 0,       zDir);

                    if (!_grid.InBounds(a.Position    + verticalOffset)
                        || !_grid.InBounds(a.Position + horizontalOffset)
                        || !_grid.InBounds(a.Position + verticalOffset + horizontalOffset))
                    {
                        return pathCost;
                    }

                    if (_grid[a.Position    + horizontalOffset]                      != CellType.None
                        || _grid[a.Position + horizontalOffset * 2]                  != CellType.None
                        || _grid[a.Position + verticalOffset + horizontalOffset]     != CellType.None
                        || _grid[a.Position + verticalOffset + horizontalOffset * 2] != CellType.None)
                    {
                        return pathCost;
                    }

                    pathCost.Traversable = true;
                    pathCost.IsStairs = true;
                }

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

                        if (delta.y != 0)
                        {
                            var xDir = Mathf.Clamp(delta.x, -1, 1);
                            var zDir = Mathf.Clamp(delta.z, -1, 1);
                            var verticalOffset = new Vector3Int(0,      delta.y, 0);
                            var horizontalOffset = new Vector3Int(xDir, 0,       zDir);

                            _grid[prev + horizontalOffset] = CellType.Stairs;
                            _grid[prev + horizontalOffset * 2] = CellType.Stairs;
                            _grid[prev + verticalOffset + horizontalOffset] = CellType.Stairs;
                            _grid[prev + verticalOffset + horizontalOffset * 2] = CellType.Stairs;

                            PlaceStairs(prev + horizontalOffset);
                            PlaceStairs(prev + horizontalOffset * 2);
                            PlaceStairs(prev + verticalOffset + horizontalOffset);
                            PlaceStairs(prev + verticalOffset + horizontalOffset * 2);
                        }

                        Debug.DrawLine(prev + new Vector3(0.5f, 0.5f, 0.5f), current + new Vector3(0.5f, 0.5f, 0.5f),
                            Color.blue, 100, false);
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

    private void PlaceCube(Vector3Int location, Vector3Int size, Material material)
    {
        var go = Instantiate(cubePrefab, location, Quaternion.identity);
        go.GetComponent<Transform>().localScale = size;
        go.GetComponent<MeshRenderer>().material = material;
    }

    private void PlaceRoom(Vector3Int location, Vector3Int size)
    {
        PlaceCube(location, size, redMaterial);
    }

    private void PlaceHallway(Vector3Int location)
    {
        PlaceCube(location, new Vector3Int(1, 1, 1), blueMaterial);
    }

    private void PlaceStairs(Vector3Int location)
    {
        PlaceCube(location, new Vector3Int(1, 1, 1), greenMaterial);
    }
}