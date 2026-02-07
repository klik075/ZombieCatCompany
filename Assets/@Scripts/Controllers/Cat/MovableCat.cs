using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 이동 및 AI 경로찾기 로직을 담당하는 기본 클래스
/// </summary>
public abstract class MovableCat : Cat
{
    protected bool _isMoving = false;
    public bool CanMove => !_isMoving;

    // 이동 관련 변수
    private Vector3 _moveStart;
    private Vector3 _moveEnd;
    private Vector2Int _targetCell;
    private float _moveElapsed = 0f;
    protected float _moveDuration = 0.3f;

    // AI 관련 변수
    public bool AIEnabled = true;
    protected Vector2Int _aiTargetPosition;
    protected List<Vector2Int> _path = new List<Vector2Int>();

    public void MoveTo(Vector2Int targetCell)
    {
        if (_isMoving)
            return;
        if (!MapManager.Instance.CanMove(targetCell))
            return;

        _isMoving = true;
        State = ECatState.Move;
        MapManager.Instance.MoveTo(this, targetCell, false);
        _moveStart = transform.position;
        _moveEnd = MapManager.Instance.CellToWorld(targetCell);
        _targetCell = targetCell;
        _moveElapsed = 0f;

        Vector3 direction = (_moveEnd - _moveStart).normalized;
        IsFacingForward = direction.y <= 0;
        IsFlipped = direction.x < 0;
    }

    public override void Update()
    {
        base.Update();

        if (_isMoving)
        {
            UpdateMovement();
        }
        else if (AIEnabled && State == ECatState.Idle)
        {
            UpdateAI();
        }
    }

    private void UpdateMovement()
    {
        _moveElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_moveElapsed / _moveDuration);
        transform.position = Vector3.Lerp(_moveStart, _moveEnd, t);
        
        if (t >= 1f)
        {
            transform.position = _moveEnd;
            CellPosition = _targetCell;
            _isMoving = false;
            State = ECatState.Idle;
            OnMoveCompleted();

            if (_path.Count > 0)
            {
                _path.RemoveAt(0);
            }
        }
    }

    protected virtual void UpdateAI()
    {
        if (_path.Count > 0)
        {
            if (MapManager.Instance.CanMove(_path[0]))
            {
                MoveTo(_path[0]);
            }
            else
            {
                RecalculatePath();
            }
        }
        else
        {
            OnAIIdle();
        }
    }

    protected void RecalculatePath()
    {
        _path = MapManager.Instance.FindPath(CellPosition, _aiTargetPosition);
        if (_path.Count > 0)
        {
            MoveTo(_path[0]);
        }
    }

    /// <summary>
    /// 이동 완료 시 호출되는 콜백 (상속받은 클래스에서 오버라이드)
    /// </summary>
    protected virtual void OnMoveCompleted() { }

    /// <summary>
    /// AI가 유휴 상태일 때 호출 (상속받은 클래스에서 오버라이드)
    /// </summary>
    protected virtual void OnAIIdle() { }

    public void MoveToPosition(Vector2Int targetPos)
    {
        if (_isMoving)
        {
            _isMoving = false;
            State = ECatState.Idle;
            transform.position = MapManager.Instance.CellToWorld(CellPosition);
        }

        _path.Clear();
        _aiTargetPosition = targetPos;
        _path = MapManager.Instance.FindPath(CellPosition, _aiTargetPosition);

        if (_path.Count > 0)
        {
            MoveTo(_path[0]);
        }
    }

    protected void OnDrawGizmosSelected()
    {
        if (_path == null || _path.Count == 0) return;

        Gizmos.color = Color.green;
        for (int i = 0; i < _path.Count; i++)
        {
            Vector3 worldPos = MapManager.Instance.CellToWorld(_path[i]);
            Gizmos.DrawSphere(worldPos, 0.1f);
            if (i < _path.Count - 1)
            {
                Vector3 nextWorldPos = MapManager.Instance.CellToWorld(_path[i + 1]);
                Gizmos.DrawLine(worldPos, nextWorldPos);
            }
        }
    }
}
