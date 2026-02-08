using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 경로를 따라 이동하는 전략 (재사용 가능)
/// </summary>
public class PathMovement : IMovementStrategy
{
    private List<Vector2Int> _path;
    private IGridManager _gridManager;
    private int _currentIndex = 0;
    
    public bool IsComplete => _currentIndex >= _path.Count;
    
    public PathMovement(List<Vector2Int> path, IGridManager gridManager)
    {
        _path = path ?? new List<Vector2Int>();
        _gridManager = gridManager;
    }
    
    public void Execute(Cat cat)
    {
        if (IsComplete) 
            return;
        
        Vector2Int nextCell = _path[_currentIndex];
        
        if (_gridManager.CanMoveTo(nextCell))
        {
            cat.MoveTo(nextCell);
            _currentIndex++;
        }
        else
        {
            // 경로 막힘 - 재계산
            OnPathBlocked?.Invoke(cat, nextCell);
            
            // 경로 재계산 시도
            _path = _gridManager.FindPath(cat.CellPosition, _path[_path.Count - 1]);
            _currentIndex = 0;
            
            if (_path.Count == 0)
            {
                OnPathFindingFailed?.Invoke();
            }
        }
    }
    
    public void Reset()
    {
        _currentIndex = 0;
    }
    
    /// <summary>
    /// 디버그용 경로 정보 반환
    /// </summary>
    public List<Vector2Int> GetDebugPath()
    {
        return _path ?? new List<Vector2Int>();
    }
    
    public event System.Action<Cat, Vector2Int> OnPathBlocked;
    public event System.Action OnPathFindingFailed;
}
