using UnityEngine;

/// <summary>
/// 캐릭터 이동을 담당하는 재사용 가능한 컴포넌트
/// </summary>
public class CatMover
{
    private Cat _owner;
    private IGridManager _gridManager;
    
    // 이동 상태
    private bool _isMoving = false;
    private Vector3 _moveStart;
    private Vector3 _moveEnd;
    private Vector2Int _nextCell;
    private float _moveElapsed = 0f;
    private float _moveDuration = 0.3f;
    
    // 현재 이동 전략
    private IMovementStrategy _currentStrategy;
    
    public bool IsMoving => _isMoving;
    public bool HasStrategy => _currentStrategy != null && !_currentStrategy.IsComplete;
    
    public CatMover(Cat owner, IGridManager gridManager)
    {
        _owner = owner;
        _gridManager = gridManager;
    }
    
    /// <summary>
    /// 단일 셀로 즉시 이동
    /// </summary>
    public void MoveTo(Vector2Int nextCell)
    {
        if (_isMoving) 
            return;

        if (!_gridManager.CanMoveTo(nextCell)) 
            return;
        
        StartMove(nextCell);
    }
    
    /// <summary>
    /// 이동 전략 설정 (경로 이동, AI 이동 등)
    /// </summary>
    public void SetMovementStrategy(IMovementStrategy strategy)
    {
        _currentStrategy = strategy;
    }
    
    /// <summary>
    /// 현재 전략 제거
    /// </summary>
    public void ClearStrategy()
    {
        _currentStrategy = null;
    }
    
    public void Update()
    {
        if (_isMoving)
        {
            UpdateMovement();
        }
        else if (_currentStrategy != null && !_currentStrategy.IsComplete)//경로 남음
        {
            _currentStrategy.Execute(_owner);
        }
    }
    
    private void StartMove(Vector2Int nextCell)
    {
        _isMoving = true;
        _owner.State = Cat.ECatState.Move;
        
        // 기존 위치 해제
        if (_gridManager.CanMoveTo(_owner.CellPosition) || _owner.CellPosition != nextCell)
        {
            _gridManager.Release(_owner.CellPosition);
        }
        
        _gridManager.Occupy(nextCell, _owner);
        _moveStart = _owner.transform.position;
        _moveEnd = _gridManager.CellToWorld(nextCell);
        _nextCell = nextCell;
        _moveElapsed = 0f;
        
        // 방향 설정
        Vector3 direction = (_moveEnd - _moveStart).normalized;
        _owner.IsFacingForward = direction.y <= 0;
        _owner.IsFlipped = direction.x < 0;
    }
    
    private void UpdateMovement()
    {
        _moveElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_moveElapsed / _moveDuration);
        _owner.transform.position = Vector3.Lerp(_moveStart, _moveEnd, t);
        
        if (t >= 1f)
        {
            _owner.transform.position = _moveEnd;
            _owner.CellPosition = _nextCell;
            _isMoving = false;
            _owner.State = Cat.ECatState.Idle;
            
            OnMoveCompleted?.Invoke();
        }
    }
    
    public event System.Action OnMoveCompleted;
}
