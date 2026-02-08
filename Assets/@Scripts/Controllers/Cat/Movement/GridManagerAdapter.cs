using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MapManager를 IGridManager 인터페이스로 변환하는 어댑터
/// </summary>
public class GridManagerAdapter : IGridManager
{
    public bool CanMoveTo(Vector2Int position)
    {
        return MapManager.Instance.CanMove(position);
    }
    
    public bool IsWalkable(Vector2Int position)
    {
        return MapManager.Instance.IsWalkable(position);
    }
    
    public void Occupy(Vector2Int position, Cat occupant)
    {
        MapManager.Instance.RegisterCat(occupant, position);
    }
    
    public void Release(Vector2Int position)
    {
        MapManager.Instance.UnregisterCat(position);
    }
    
    public Vector3 CellToWorld(Vector2Int cell)
    {
        return MapManager.Instance.CellToWorld(cell);
    }
    
    public Vector2Int WorldToCell(Vector3 pos)
    {
        return MapManager.Instance.WorldToCell(pos);
    }
    
    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
    {
        return MapManager.Instance.FindPath(start, goal);
    }
    
    public List<Vector2Int> GetWalkableCells()
    {
        return MapManager.Instance.GetWalkableCells();
    }
}
