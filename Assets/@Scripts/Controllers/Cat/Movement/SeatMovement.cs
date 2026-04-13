using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 좌석으로 이동하는 전략 (교착 상태 방지 개선)
/// </summary>
public class SeatMovement : IMovementStrategy
{
    private Vector2Int _seatPosition;
    private IGridManager _gridManager;
    private List<Vector2Int> _path;
    private int _currentIndex = 0;
    private float _retryTimer = 0f;
    private float _currentRetryDelay = 0f;
    private const float MIN_RETRY_DELAY = 0.3f;
    private const float MAX_RETRY_DELAY = 1.0f;
    private const float DEADLOCK_TIMEOUT = 3f;
    private float _stuckTimer = 0f;
    private bool _pathCalculated = false;
    private Vector2Int _lastPosition;
    
    private float _initialDelay = 0f;
    private float _initialDelayTimer = 0f;
    private int _blockedCounter = 0;
    private const int STEP_ASIDE_THRESHOLD = 5;
    private const int MAX_BLOCKED_ATTEMPTS = 15;
    
    // 비켜서기 시도 쿨다운
    private float _stepAsideCooldown = 0f;
    private const float STEP_ASIDE_COOLDOWN_TIME = 1.0f;
    
    public bool IsComplete { get; private set; }
    
    public SeatMovement()
    {
    }
    
    public SeatMovement(Vector2Int seatPosition, IGridManager gridManager)
    {
        Initialize(seatPosition, gridManager);
    }
    
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
        
        // 초기 경로 계산 랜덤 딜레이
        _initialDelay = Random.Range(0.1f, 1.5f);
        _initialDelayTimer = 0f;
        _blockedCounter = 0;
        _currentRetryDelay = GetRandomRetryDelay();
        _stepAsideCooldown = 0f;
        
        return this;
    }
    
    public void Execute(Cat cat)
    {
        if (cat.CellPosition == _seatPosition)
        {
            IsComplete = true;
            OnArrived?.Invoke();
            return;
        }
        
        // 초기 딜레이 처리
        if (!_pathCalculated && _initialDelayTimer < _initialDelay)
        {
            _initialDelayTimer += Time.deltaTime;
            return;
        }
        
        // 위치 변화 감지
        if (cat.CellPosition != _lastPosition)
        {
            _lastPosition = cat.CellPosition;
            _stuckTimer = 0f;
            _blockedCounter = 0; // 움직였을 때만 리셋
        }
        else
        {
            _stuckTimer += Time.deltaTime;
        }
        
        // 비켜서기 쿨다운 감소
        if (_stepAsideCooldown > 0f)
        {
            _stepAsideCooldown -= Time.deltaTime;
        }
        
        // 경로가 없으면 즉시 재계산
        if (!_pathCalculated)
        {
            _path = _gridManager.FindPath(cat.CellPosition, _seatPosition);
            
            if (_path.Count == 0)
            {
                Debug.LogWarning($"[SeatMovement] Failed to find path to seat {_seatPosition}");
                // 경로를 찾지 못해도 계속 시도
                _retryTimer += Time.deltaTime;
                if (_retryTimer >= _currentRetryDelay)
                {
                    _retryTimer = 0f;
                    _currentRetryDelay = GetRandomRetryDelay();
                    Debug.Log($"[SeatMovement] Retrying to find path...");
                }
                return;
            }

            _pathCalculated = true;
            _currentIndex = 0;
        }
        
        if (_currentIndex >= _path.Count)
        {
            IsComplete = true;
            OnArrived?.Invoke();
            return;
        }
        
        Vector2Int nextCell = _path[_currentIndex];
        bool isLastCell = (_currentIndex == _path.Count - 1);
        
        // 이동 시도
        if (_gridManager.CanMoveTo(nextCell))
        {
            cat.MoveTo(nextCell);
            _currentIndex++;
            _blockedCounter = 0;
            _currentRetryDelay = GetRandomRetryDelay();
        }
        else if (isLastCell)
        {
            _gridManager.Release(nextCell);
            cat.MoveTo(nextCell);
            _currentIndex++;
        }
        else
        {
            HandleBlockedMovement(cat, nextCell);
        }
    }
    
    private void HandleBlockedMovement(Cat cat, Vector2Int nextCell)
    {
        _blockedCounter++;
        
        // 최대 시도 횟수 초과 시 강제 해결
        if (_blockedCounter >= MAX_BLOCKED_ATTEMPTS)
        {
            Debug.LogWarning($"[SeatMovement] Max attempts reached ({MAX_BLOCKED_ATTEMPTS})! Emergency resolution.");
            ForceResolveDeadlock(cat);
            return;
        }
        
        // 교착상태 감지 시 비켜서기 시도 (쿨다운 체크)
        if ((_stuckTimer >= DEADLOCK_TIMEOUT || _blockedCounter >= STEP_ASIDE_THRESHOLD) 
            && _stepAsideCooldown <= 0f)
        {
            Debug.Log($"[SeatMovement] Deadlock suspected (Blocked: {_blockedCounter}, Stuck: {_stuckTimer:F1}s). Attempting to step aside.");
            bool stepped = TryStepAside(cat);
            
            if (stepped)
            {
                _stepAsideCooldown = STEP_ASIDE_COOLDOWN_TIME; // 성공 시 쿨다운
                _stuckTimer = 0f;
                return; // 성공했으면 이번 프레임 종료
            }
            else
            {
                // 실패해도 쿨다운 (연속 시도 방지)
                _stepAsideCooldown = STEP_ASIDE_COOLDOWN_TIME * 0.5f;
                _stuckTimer = 0f;
                // return 하지 않고 아래 경로 재탐색 로직으로 진행!
            }
        }
        
        // 항상 경로 재탐색 시도 (비켜서기 실패해도 실행됨)
        _retryTimer += Time.deltaTime;
        if (_retryTimer >= _currentRetryDelay)
        {
            _retryTimer = 0f;
            _currentRetryDelay = GetRandomRetryDelay();
            
            Debug.Log($"[SeatMovement] Recalculating path (attempt {_blockedCounter})");
            RecalculatePath(cat);
        }
    }
    
    private void RecalculatePath(Cat cat)
    {
        _path = _gridManager.FindPath(cat.CellPosition, _seatPosition);
        _currentIndex = 0;
        
        if (_path.Count == 0)
        {
            Debug.LogWarning($"[SeatMovement] Failed to recalculate path");
            _pathCalculated = false; // 다음 프레임에 다시 시도
        }
        else
        {
            _pathCalculated = true;
        }
    }
    
    /// <summary>
    /// 반환값 추가: 성공 여부
    /// </summary>
    private bool TryStepAside(Cat cat)
    {
        Vector2Int current = cat.CellPosition;
        Vector2Int toGoal = _seatPosition - current;
        Vector2Int[] priorityDirections = GetPriorityDirections(toGoal);
        
        foreach (var dir in priorityDirections)
        {
            Vector2Int sideCell = current + dir;
            
            if (_gridManager.CanMoveTo(sideCell))
            {
                Debug.Log($"[SeatMovement] Stepping aside to {sideCell} to let others pass");
                cat.MoveTo(sideCell);
                
                _pathCalculated = false;
                _retryTimer = 0f;
                _currentRetryDelay = GetRandomRetryDelay();
                _blockedCounter = Mathf.Max(0, _blockedCounter - 2);
                
                return true; // 성공
            }
        }
        
        // 실패 - 경로만 재탐색 예약
        Debug.Log($"[SeatMovement] Cannot step aside, will recalculate path (counter: {_blockedCounter})");
        _pathCalculated = false;
        
        return false; // 실패
    }
    
    private void ForceResolveDeadlock(Cat cat)
    {
        Vector2Int current = cat.CellPosition;
        
        _gridManager.Release(current);
        Debug.Log($"[SeatMovement] Released position {current} temporarily");
        
        _retryTimer = 0f;
        _currentRetryDelay = Random.Range(1.0f, 2.0f);
        _pathCalculated = false;
        _blockedCounter = STEP_ASIDE_THRESHOLD;
        _stepAsideCooldown = 0f; // 쿨다운 리셋
    }
    
    private float GetRandomRetryDelay()
    {
        return Random.Range(MIN_RETRY_DELAY, MAX_RETRY_DELAY);
    }
    
    private Vector2Int[] GetPriorityDirections(Vector2Int toGoal)
    {
        List<Vector2Int> directions = new List<Vector2Int>
        {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0)
        };
        
        directions.Sort((a, b) =>
        {
            int dotA = a.x * toGoal.x + a.y * toGoal.y;
            int dotB = b.x * toGoal.x + b.y * toGoal.y;
            return dotA.CompareTo(dotB);
        });
        
        return directions.ToArray();
    }
    
    public void Reset()
    {
        _currentIndex = 0;
        _pathCalculated = false;
        IsComplete = false;
        _stuckTimer = 0f;
        _retryTimer = 0f;
        _blockedCounter = 0;
        _initialDelayTimer = 0f;
        _currentRetryDelay = GetRandomRetryDelay();
        _stepAsideCooldown = 0f;
    }
    
    public List<Vector2Int> GetDebugPath()
    {
        return _path ?? new List<Vector2Int>();
    }
    
    public string GetDebugStatus()
    {
        return $"Blocked: {_blockedCounter}/{MAX_BLOCKED_ATTEMPTS}, Stuck: {_stuckTimer:F1}s, RetryDelay: {_currentRetryDelay:F1}s, Cooldown: {_stepAsideCooldown:F1}s";
    }
    
    public event System.Action OnArrived;
}
