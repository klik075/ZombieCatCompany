using UnityEngine;
using Spine.Unity;

[RequireComponent(typeof(SkeletonAnimation))]
public class Cat : ObjectBase
{
    public enum EAnimation
    {
        b_wait,
        b_walk,
        b_work,
        b_attack,
        f_wait,
        f_walk,
        f_work,
        f_attack,
    }

    public enum ECatState
    {
        Idle,
        Move,
        Work,
        Attack
    }

    //애니메이션 및 상태
    private SkeletonAnimation _skeletonAnimation;
    private ECatState _state;
    private bool _isFacingForward = true;
    private bool _isFlipped = false;

    public Vector2Int CellPosition { get; set; }

    // 이동 시스템 (전략패턴)
    protected CatMover _mover;
    protected IGridManager _gridManager;
    public IGridManager GridManager { get { return _gridManager; } }

    // 공격 시스템 (전략패턴)
    protected IAttackStrategy _attackStrategy;
    
    public ECatState State
    {
        get { return _state; }
        private set { _state = value; UpdateAnimation(); } // private set으로 변경
    }

    public bool IsFacingForward
    {
        get { return _isFacingForward; }
        set { _isFacingForward = value; UpdateAnimation(); }
    }

    public bool IsFlipped
    {
        get { return _isFlipped; }
        set 
        { 
            _isFlipped = value;

            if (_skeletonAnimation == null)
                return;

            _skeletonAnimation.skeleton.ScaleX = _isFlipped ? -1 : 1; 
        }
    }

    public bool IsMoving => _mover != null && _mover.IsMoving;
    public bool IsAttacking => _attackStrategy != null && _attackStrategy.IsAttacking;

    protected override void Awake()
    {
        _skeletonAnimation = GetComponent<SkeletonAnimation>();
        _gridManager = new GridManagerAdapter();
        _mover = new CatMover(this, _gridManager);
        State = ECatState.Idle;
    }

    protected virtual void Start()
    {

    }
    protected virtual void OnDestroy()
    {
        if (_gridManager != null)
        {
            _gridManager.Release(CellPosition);
        }
    }
    public virtual void Update()
    {
        // Isometric sorting for spine renderer
        if (_skeletonAnimation != null)
        {
            Renderer renderer = _skeletonAnimation.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100f);
            }
        }

        // 공격이 활성화되어 있으면 공격만 처리 (이동 중지)
        if (IsAttacking)
        {
            _attackStrategy?.Update();
        }
        // 공격 중이 아닐 때만 이동 처리
        else if (!IsAttacking)
        {
            _mover?.Update();
        }
    }

    #region 상태 관리 (내부 전용)
    /// <summary>
    /// 상태를 Move로 전환 (CatMover 전용)
    /// </summary>
    internal void SetStateMove()
    {
        if (!IsAttacking) // 공격 중이 아닐 때만
        {
            State = ECatState.Move;
        }
    }

    /// <summary>
    /// 상태를 Idle로 전환 (CatMover 전용)
    /// </summary>
    internal void SetStateIdle()
    {
        if (!IsAttacking) // 공격 중이 아닐 때만
        {
            State = ECatState.Idle;
        }
    }

    /// <summary>
    /// 상태를 Attack으로 전환 (IAttackStrategy 전용)
    /// </summary>
    internal void SetStateAttack()
    {
        State = ECatState.Attack;
    }

    /// <summary>
    /// 상태를 Work로 전환
    /// </summary>
    internal void SetStateWork()
    {
        State = ECatState.Work;
    }

    /// <summary>
    /// 강제로 상태 설정 (세이브 로드용)
    /// </summary>
    internal void SetStateForced(ECatState state)
    {
        State = state;
    }
    #endregion

    #region 이동 처리
    /// <summary>
    /// 특정 셀로 이동 (외부 컨트롤 기본 메서드)
    /// </summary>
    public void MoveTo(Vector2Int targetCell)
    {
        // 공격 중이면 이동 불가
        if (IsAttacking)
        {
            Debug.LogWarning($"[Cat] {name} is attacking, cannot move");
            return;
        }

        _mover?.MoveTo(targetCell);
    }

    /// <summary>
    /// 이동 전략 설정
    /// </summary>
    public void SetMovementStrategy(IMovementStrategy strategy)
    {
        // 공격 중이면 이동 전략 설정 불가
        if (IsAttacking)
        {
            Debug.LogWarning($"[Cat] {name} is attacking, cannot set movement strategy");
            return;
        }

        _mover?.SetMovementStrategy(strategy);
    }

    /// <summary>
    /// 이동 전략 해제
    /// </summary>
    public void ClearMovementStrategy()
    {
        _mover?.ClearStrategy();
    }
    #endregion

    #region 애니메이션 처리
    private void UpdateAnimation()
    {
        EAnimation animation;
        bool loop = true; // 기본값: 반복 재생

        switch (_state)
        {
            case ECatState.Idle:
                animation = _isFacingForward ? EAnimation.f_wait : EAnimation.b_wait;
                break;
            case ECatState.Move:
                animation = _isFacingForward ? EAnimation.f_walk : EAnimation.b_walk;
                break;
            case ECatState.Work:
                animation = _isFacingForward ? EAnimation.f_work : EAnimation.b_work;
                break;
            case ECatState.Attack:
                animation = _isFacingForward ? EAnimation.f_attack : EAnimation.b_attack;
                loop = false; // Attack은 반복 안 함
                break;
            default:
                animation = EAnimation.f_wait;
                break;
        }

        PlayAnimation(animation, loop);
    }
    
    public void PlayAnimation(EAnimation animation, bool loop = true)
    {
        if (_skeletonAnimation != null)
        {
            _skeletonAnimation.AnimationState.SetAnimation(0, animation.ToString(), loop);
        }
    }

    /// <summary>
    /// 애니메이션 속도 설정
    /// </summary>
    public void SetAnimationSpeed(float speed)
    {
        if (_skeletonAnimation != null)
        {
            _skeletonAnimation.AnimationState.TimeScale = speed;
        }
    }

    /// <summary>
    /// 현재 애니메이션 길이 가져오기
    /// </summary>
    public float GetCurrentAnimationDuration()
    {
        if (_skeletonAnimation != null && _skeletonAnimation.AnimationState.GetCurrent(0) != null)
        {
            return _skeletonAnimation.AnimationState.GetCurrent(0).Animation.Duration;
        }
        return 0f;
    }
    #endregion

    #region 공격 처리
    /// <summary>
    /// 공격 전략 설정
    /// </summary>
    public void SetAttackStrategy(IAttackStrategy strategy)
    {
        _attackStrategy?.StopAttack();
        _attackStrategy = strategy;
    }
    
    /// <summary>
    /// 공격 시작
    /// </summary>
    public void StartAttack()
    {
        // 이동 중이면 이동 중지
        if (IsMoving)
        {
            _mover?.ClearStrategy();
            Debug.Log($"[Cat] {name} stopped moving to start attack");
        }

        _attackStrategy?.StartAttack();
    }
    
    /// <summary>
    /// 공격 중지
    /// </summary>
    public void StopAttack()
    {
        _attackStrategy?.StopAttack();
    }
    #endregion

    #region 경로 시각화 (기즈모)
    /// <summary>
    /// 에디터에서 선택했을 때 경로 시각화 (기즈모)
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (_mover == null) 
            return;
        
        // 현재 이동 전략에서 경로 정보 가져오기
        var strategy = GetCurrentStrategy();
        if (strategy == null) 
            return;
        
        var path = strategy.GetDebugPath();
        if (path == null || path.Count == 0) 
            return;
        
        // 경로 그리기
        Gizmos.color = Color.green;
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 worldPos = _gridManager.CellToWorld(path[i]);
            
            // 경로 점 그리기
            Gizmos.DrawSphere(worldPos, 0.1f);
            
            // 경로 선 그리기
            if (i < path.Count - 1)
            {
                Vector3 nextWorldPos = _gridManager.CellToWorld(path[i + 1]);
                Gizmos.DrawLine(worldPos, nextWorldPos);
            }
        }
        
        // 시작점 표시 (빨간색)
        if (path.Count > 0)
        {
            Gizmos.color = Color.red;
            Vector3 startPos = _gridManager.CellToWorld(path[0]);
            Gizmos.DrawWireSphere(startPos, 0.15f);
        }
        
        // 목표점 표시 (파란색)
        if (path.Count > 0)
        {
            Gizmos.color = Color.blue;
            Vector3 endPos = _gridManager.CellToWorld(path[path.Count - 1]);
            Gizmos.DrawWireSphere(endPos, 0.15f);
        }
    }
    
    /// <summary>
    /// 현재 이동 전략 가져오기 (리플렉션 사용)
    /// </summary>
    private IMovementStrategy GetCurrentStrategy()
    {
        if (_mover == null) 
            return null;
        
        // CatMover의 private 필드에 접근
        var field = typeof(CatMover).GetField("_currentStrategy", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        return field?.GetValue(_mover) as IMovementStrategy;
    }
    #endregion
}

