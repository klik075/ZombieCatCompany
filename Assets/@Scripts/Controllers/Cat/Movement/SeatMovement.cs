using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 좌석으로 이동하는 전략 (마지막 셀 점유 무시)
/// </summary>
public class SeatMovement : IMovementStrategy
{
    private Vector2Int _seatPosition;
    private IGridManager _gridManager;
    private List<Vector2Int> _path;
    private int _currentIndex = 0;
    private float _retryTimer = 0f;
    private const float RETRY_INTERVAL = 0.5f;
    private const float DEADLOCK_TIMEOUT = 3f; // 교착 상태 판단 시간
    private float _stuckTimer = 0f; // 막힌 시간 측정
    private bool _pathCalculated = false;
    private Vector2Int _lastPosition; // 이전 위치 추적
    
    public bool IsComplete { get; private set; }
    
    // 기본 생성자 추가
    public SeatMovement()
    {
    }
    
    public SeatMovement(Vector2Int seatPosition, IGridManager gridManager)
    {
        Initialize(seatPosition, gridManager);
    }
    
    /// <summary>
    /// 재사용을 위한 재초기화
    /// </summary>
    public SeatMovement Initialize(Vector2Int seatPosition, IGridManager gridManager)
    {
        _seatPosition = seatPosition;
        _gridManager = gridManager;
        _path = new List<Vector2Int>();
        _currentIndex = 0;
        _retryTimer = 0f;
        _stuckTimer = 0f;
        _pathCalculated = false;
        _lastPosition = new Vector2Int(int.MinValue, int.MinValue);
        IsComplete = false;
        return this;
    }
    
    public void Execute(Cat cat)
    {
        // 이미 도착했으면 완료
        if (cat.CellPosition == _seatPosition)
        {
            IsComplete = true;
            OnArrived?.Invoke();
            return;
        }
        
        // 위치 변화 감지 (움직이고 있으면 타이머 리셋)
        if (cat.CellPosition != _lastPosition)
        {
            _lastPosition = cat.CellPosition;
            _stuckTimer = 0f;
        }
        else
        {
            _stuckTimer += Time.deltaTime;
        }
        
        // 첫 실행 시 경로 계산
        if (!_pathCalculated)
        {
            _path = _gridManager.FindPath(cat.CellPosition, _seatPosition);
            
            if (_path.Count == 0)
            {
                Debug.LogWarning($"Failed to find path to seat {_seatPosition}");
                return;
            }

            _pathCalculated = true;
        }
        
        if (_currentIndex >= _path.Count)
        {
            IsComplete = true;
            OnArrived?.Invoke();
            return;
        }
        
        Vector2Int nextCell = _path[_currentIndex];
        bool isLastCell = (_currentIndex == _path.Count - 1);
        
        if (_gridManager.CanMoveTo(nextCell))
        {
            cat.MoveTo(nextCell);
            _currentIndex++;
        }
        else if (isLastCell)
        {
            // 마지막 셀(좌석)이면 강제로 점유 해제 후 이동
            _gridManager.Release(nextCell);
            cat.MoveTo(nextCell);
            _currentIndex++;
        }
        else
        {
            // 교착 상태 감지 - 3초 이상 같은 위치에 있으면
            if (_stuckTimer >= DEADLOCK_TIMEOUT)
            {
                Debug.Log($"Deadlock detected! Member will step aside.");
                TryStepAside(cat);
                _stuckTimer = 0f; // 타이머 리셋
                return;
            }
            
            // 중간 경로 막힘 - 재계산 시도
            _retryTimer += Time.deltaTime;
            if (_retryTimer >= RETRY_INTERVAL)
            {
                _retryTimer = 0f;
                _path = _gridManager.FindPath(cat.CellPosition, _seatPosition);
                _currentIndex = 0;
                
                if (_path.Count == 0)
                {
                    Debug.LogWarning($"Failed to recalculate path to seat {_seatPosition}");
                    _pathCalculated = false;
                }
            }
        }
    }
    
    /// <summary>
    /// 교착 상태 해결: 옆으로 비켜나기
    /// </summary>
    private void TryStepAside(Cat cat)
    {
        Vector2Int current = cat.CellPosition;
        
        // 상하좌우 중 비어있는 곳 찾기
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),   // 위
            new Vector2Int(0, -1),  // 아래
            new Vector2Int(1, 0),   // 오른쪽
            new Vector2Int(-1, 0)   // 왼쪽
        };
        
        foreach (var dir in directions)
        {
            Vector2Int sideCell = current + dir;
            
            if (_gridManager.CanMoveTo(sideCell))
            {
                // 비어있는 곳으로 잠시 이동 (양보)
                Debug.Log($"Stepping aside to {sideCell}");
                cat.MoveTo(sideCell);
                
                // 경로 재계산 예약
                _pathCalculated = false;
                _retryTimer = RETRY_INTERVAL; // 즉시 재계산
                return;
            }
        }
        
        // 옆으로 비켜날 곳이 없으면 현재 위치 점유 해제 (다른 Member가 지나갈 수 있게)
        Debug.Log($"Cannot step aside, releasing position temporarily");
        _gridManager.Release(current);
        
        // 잠시 후 다시 점유 시도
        _retryTimer = RETRY_INTERVAL;
    }
    
    public void Reset()
    {
        _currentIndex = 0;
        _pathCalculated = false;
        IsComplete = false;
        _stuckTimer = 0f;
        _retryTimer = 0f;
    }
    
    /// <summary>
    /// 디버그용 경로 정보 반환
    /// </summary>
    public List<Vector2Int> GetDebugPath()
    {
        return _path ?? new List<Vector2Int>();
    }
    
    public event System.Action OnArrived;
}
