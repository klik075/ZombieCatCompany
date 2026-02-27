using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 울타리 타겟 이동 전략 (DefenseManager용)
/// </summary>
public class FenceTargetMovementStrategy : IMovementStrategy
{
    private readonly NormalCat _owner;
    private readonly IGridManager _gridManager;
    private readonly Vector2Int _targetPosition;
    
    private List<Vector2Int> _path;
    private int _currentPathIndex;
    private bool _isComplete;
    private bool _callbackInvoked;
    private Action _onArrived;
    
    // 경로 재계산 타이머
    private float _lastPathCalculateTime;
    private const float PATH_RECALCULATE_INTERVAL = 0.5f; // 0.5초마다 재시도
    private bool _waitingForPath; // 경로를 찾지 못해 대기 중인 상태

    public bool IsComplete => _isComplete;

    public FenceTargetMovementStrategy(NormalCat owner, IGridManager gridManager, Vector2Int targetPosition)
    {
        _owner = owner;
        _gridManager = gridManager;
        _targetPosition = targetPosition;
        _isComplete = false;
        _callbackInvoked = false;
        _waitingForPath = false;
        _lastPathCalculateTime = Time.time;
        
        CalculatePath();
    }

    public FenceTargetMovementStrategy SetOnArrived(Action onArrived)
    {
        _onArrived = onArrived;
        return this;
    }

    private void CalculatePath()
    {
        _path = MapManager.Instance.FindPath(_owner.CellPosition, _targetPosition);
        _currentPathIndex = 0;
        _lastPathCalculateTime = Time.time;

        if (_path == null || _path.Count == 0)
        {
            Debug.LogWarning($"[FenceTargetMovement] No path found from {_owner.CellPosition} to {_targetPosition}, will retry in {PATH_RECALCULATE_INTERVAL}s");
            _waitingForPath = true;
        }
        else
        {
            Debug.Log($"[FenceTargetMovement] Path calculated: {_path.Count} steps from {_owner.CellPosition} to {_targetPosition}");
            _waitingForPath = false;
        }
    }

    public void Execute(Cat owner)
    {
        // 이미 완료되었으면 더 이상 실행하지 않음
        if (_isComplete)
            return;

        // 이미 목표 위치에 도착했는지 확인
        if (_owner.CellPosition == _targetPosition)
        {
            Debug.Log($"[FenceTargetMovement] Already at target position {_targetPosition}");
            CompleteMovement();
            return;
        }

        // 경로가 없거나 대기 중이면 일정 간격으로 재시도
        if (_waitingForPath || _path == null || _path.Count == 0)
        {
            if (Time.time - _lastPathCalculateTime >= PATH_RECALCULATE_INTERVAL)
            {
                Debug.Log($"[FenceTargetMovement] Retrying path calculation...");
                CalculatePath();
            }
            return;
        }

        // 경로의 모든 스텝을 완료했는지 확인
        if (_currentPathIndex >= _path.Count)
        {
            // 실제로 목표 위치에 있는지 재확인
            if (_owner.CellPosition == _targetPosition)
            {
                CompleteMovement();
            }
            else
            {
                // 목표 위치에 없으면 경로 재계산
                Debug.LogWarning($"[FenceTargetMovement] Path completed but not at target. Current: {_owner.CellPosition}, Target: {_targetPosition}");
                _waitingForPath = true;
                _lastPathCalculateTime = Time.time;
            }
            return;
        }

        Vector2Int nextCell = _path[_currentPathIndex];

        // 다음 셀이 이동 불가능하면 경로 재계산 대기
        if (!_gridManager.CanMoveTo(nextCell))
        {
            Debug.LogWarning($"[FenceTargetMovement] Path blocked at {nextCell}, will recalculate...");
            _waitingForPath = true;
            _lastPathCalculateTime = Time.time;
            return;
        }

        // 이동 실행
        owner.MoveTo(nextCell);
        _currentPathIndex++;
    }

    private void CompleteMovement()
    {
        if (_callbackInvoked)
            return;

        _isComplete = true;
        _callbackInvoked = true;
        
        Debug.Log($"[FenceTargetMovement] Reached target {_targetPosition} (current position: {_owner.CellPosition}), invoking callback");
        
        // 콜백 호출
        _onArrived?.Invoke();
    }

    public List<Vector2Int> GetDebugPath()
    {
        return _path;
    }

    public void Reset()
    {
        _currentPathIndex = 0;
        _isComplete = false;
        _callbackInvoked = false;
        _waitingForPath = false;
        _lastPathCalculateTime = Time.time;
    }
}
