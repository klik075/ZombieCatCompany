using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Define;

public class MapManager : Singleton<MapManager>
{
    private Tilemap _tilemap;
    private bool[,] _walkableMap;
    private Dictionary<Vector2Int, Cat> _occupiedPositions = new Dictionary<Vector2Int, Cat>();

    public Tilemap GetTilemap() { return _tilemap; }

    void Awake()
    {
        Init();
    }
    public void InitForScene()
    {
        // 기존 점유 정보 모두 클리어 (새 씬이므로)
        _occupiedPositions.Clear();

        // 새 Tilemap 찾기
        _tilemap = FindFirstObjectByType<Tilemap>();

        if (_tilemap == null)
            return;

        // 타일맵 데이터 다시 로드
        InitializeWalkableMap();
    }
    public void Init()
    {
        _tilemap = FindFirstObjectByType<Tilemap>();
        InitializeWalkableMap();
    }

    private void InitializeWalkableMap()
    {
        BoundsInt bounds = _tilemap.cellBounds;
        int width = bounds.size.x;
        int height = bounds.size.y;
        _walkableMap = new bool[width, height];

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                bool isWalkable = _tilemap.GetTile(position) != null;
                int arrayX = x - bounds.xMin;
                int arrayY = y - bounds.yMin;
                _walkableMap[arrayX, arrayY] = isWalkable;
            }
        }
    }

    public bool CanMove(int x, int y)
    {
        if (_walkableMap == null) 
			return false;

        BoundsInt bounds = _tilemap.cellBounds;
        int arrayX = x - bounds.xMin;
        int arrayY = y - bounds.yMin;
        if (arrayX < 0 || arrayX >= _walkableMap.GetLength(0) || arrayY < 0 || arrayY >= _walkableMap.GetLength(1))
            return false;

        if (!_walkableMap[arrayX, arrayY]) 
			return false;

        Vector2Int pos = new Vector2Int(x, y);
        return !_occupiedPositions.ContainsKey(pos);
    }

    public bool CanMove(Vector2Int position)
    {
        return CanMove(position.x, position.y);
    }

    public bool IsWalkable(int x, int y)
    {
        if (_walkableMap == null) 
            return false;

        BoundsInt bounds = _tilemap.cellBounds;
        int arrayX = x - bounds.xMin;
        int arrayY = y - bounds.yMin;
        if (arrayX < 0 || arrayX >= _walkableMap.GetLength(0) || arrayY < 0 || arrayY >= _walkableMap.GetLength(1))
            return false;

        return _walkableMap[arrayX, arrayY];
    }

    public bool IsWalkable(Vector2Int position)
    {
        return IsWalkable(position.x, position.y);
    }

    // Cat 등록
    public void RegisterCat(Cat cat, Vector2Int position)
    {
        if (!_occupiedPositions.ContainsKey(position))
        {
            _occupiedPositions[position] = cat;
        }
    }

    // Cat 등록 해제
    public void UnregisterCat(Vector2Int position)
    {
        if (_occupiedPositions.ContainsKey(position))
        {
            _occupiedPositions.Remove(position);
        }
    }
    // 스폰 위치 찾기 (주인공 근처 빈 공간)
    public Vector2Int FindNearPosition(Vector2Int startPos)
    {
        List<Vector2Int> checkedPositions = new List<Vector2Int>();
        // 주변 8방향 탐색
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1)
        };

        foreach (var dir in directions)
        {
            Vector2Int checkPos = startPos + dir;
            if (CanMove(checkPos))
            {
                checkedPositions.Add(checkPos);
            }
        }
        if (checkedPositions.Count > 0)
        {
            return checkedPositions[UnityEngine.Random.Range(0, checkedPositions.Count)];
        }

        // 주변에 빈 공간이 없으면 걸을 수 있는 랜덤 위치
        List<Vector2Int> walkableCells = GetWalkableCells();
        if (walkableCells.Count > 0)
        {
            return walkableCells[UnityEngine.Random.Range(0, walkableCells.Count)];
        }

        // 최후의 수단
        return new Vector2Int(0, 2);
    }
    public bool MoveTo(Cat cat, Vector2Int newPosition, bool sync = false)
    {
        if (!CanMove(newPosition.x, newPosition.y)) 
			return false;

        // Remove from old position
        if (_occupiedPositions.ContainsKey(cat.CellPosition))
            _occupiedPositions.Remove(cat.CellPosition);

        // Add to new position
        _occupiedPositions[newPosition] = cat;
        cat.CellPosition = newPosition;

        if (sync)
        {
            cat.transform.position = CellToWorld(newPosition);
        }

        return true;
    }
    public bool MoveToForce(Cat cat, Vector2Int newPosition, bool sync = false)
    {
        // 목표 지점이 walkable한지만 체크 (점유는 무시)
        if (!IsWalkable(newPosition.x, newPosition.y))
        {
            Debug.LogWarning($"Cannot move to seat {newPosition}: position is not walkable");
            return false;
        }

        // 기존 위치 해제
        if (_occupiedPositions.ContainsKey(cat.CellPosition))
            _occupiedPositions.Remove(cat.CellPosition);

        // 목표 지점에 다른 Cat이 있으면 강제로 덮어쓰기 (좌석 이동이므로)
        if (_occupiedPositions.ContainsKey(newPosition))
        {
            Debug.LogWarning($"Seat {newPosition} was occupied, but forcing move for seat assignment");
            _occupiedPositions.Remove(newPosition);
        }

        // 새 위치 점유
        _occupiedPositions[newPosition] = cat;
        cat.CellPosition = newPosition;

        if (sync)
        {
            cat.transform.position = CellToWorld(newPosition);
        }

        return true;
    }

    public Vector3 CellToWorld(Vector2Int cell)
    {
        return _tilemap.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0));
    }

    public Vector2Int WorldToCell(Vector3 pos)
    {
        Vector3Int cell = _tilemap.WorldToCell(pos);
        return new Vector2Int(cell.x, cell.y);
    }

    public List<Vector2Int> GetWalkableCells()
    {
        List<Vector2Int> walkableCells = new List<Vector2Int>();
        BoundsInt bounds = _tilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                if (CanMove(x, y))
                {
                    walkableCells.Add(new Vector2Int(x, y));
                }
            }
        }
        return walkableCells;
    }

    private struct Node : IComparable<Node>
    {
        public Vector2Int position;
        public int f;
        public Node(Vector2Int pos, int f) { this.position = pos; this.f = f; }
        public int CompareTo(Node other) => f.CompareTo(other.f);
    }

    // A* 경로 찾기
    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)//start 미포함
    {
        List<Vector2Int> path = new List<Vector2Int>();
        if (!IsWalkable(start.x, start.y) || !CanMove(goal)) 
			return path;

        Dictionary<Vector2Int, Vector2Int> parent = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, int> G = new Dictionary<Vector2Int, int>();
        Dictionary<Vector2Int, int> F = new Dictionary<Vector2Int, int>();

		Rookiss.PriorityQueue<Node> openSet = new Rookiss.PriorityQueue<Node>();
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();

        openSet.Enqueue(new Node(start, Heuristic(start, goal)));
        G[start] = 0;
        F[start] = Heuristic(start, goal);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.Dequeue();
            Vector2Int current = currentNode.position;

            if (current == goal)
            {
                // Reconstruct path
                while (parent.ContainsKey(current))
                {
                    path.Insert(0, current);
                    current = parent[current];
                }
                return path;
            }

            closedSet.Add(current);

            // Neighbors
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighbor = current + dir;
                if (!CanMove(neighbor) || closedSet.Contains(neighbor)) 
                    continue;

                int tentativeGScore = G[current] + 1; // Cost is 1 for each step
                if (!G.ContainsKey(neighbor) || tentativeGScore < G[neighbor])
                {
                    parent[neighbor] = current;
                    G[neighbor] = tentativeGScore;
                    F[neighbor] = G[neighbor] + Heuristic(neighbor, goal);
                    openSet.Enqueue(new Node(neighbor, F[neighbor]));
                }
            }
        }

        // No path to goal, find closest reachable position
        if (path.Count == 0)
        {
            Vector2Int closest = start;
            int minDist = Heuristic(start, goal);
            foreach (var pos in parent.Keys)
            {
                int dist = Heuristic(pos, goal);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = pos;
                }
            }
            // Reconstruct path to closest
            while (parent.ContainsKey(closest))
            {
                path.Insert(0, closest);
                closest = parent[closest];
            }
        }

        return path;
    }

    private int Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y); // Manhattan distance
    }
}
