using UnityEngine;
using static Define;

public class Member : Cat
{
    public MemberData CurrentMemberData { get; private set; }
    
    private MemberAI _ai;

    [SerializeField] private string _projectilePrefabName = "Projectile";
    [SerializeField] private float _detectionRange = 5f;
    [SerializeField] private float _attackSpeed = 0.5f;

    public bool IsDispatched { get; set; }
    public bool AIEnabled
    {
        get => _ai?.IsEnabled ?? false;
        set 
        { 
            if (_ai != null) 
                _ai.IsEnabled = value; 
        }
    }

    /// <summary>
    /// 현재 공격력 (MemberData.Power 기반)
    /// </summary>
    public int AttackDamage => CurrentMemberData?.Power ?? 0;

    protected override void Awake()
    {
        base.Awake();

        // 초기 공격 전략 설정 (데미지는 나중에 업데이트됨)
        UpdateAttackStrategy();
    }
    
    protected override void Start()
    {
        base.Start();

        _ai = new MemberAI(this, _gridManager);
        _mover.OnMoveCompleted += OnMoveCompleted;
    }
    
    public override void Update()
    {
        base.Update();

        if (!_mover.IsMoving && !_mover.HasStrategy)
            _ai?.Update();
    }
    
    private void OnMoveCompleted() { _ai?.OnMoveCompleted(); }
    
    public void SetMemberData(int employeeId)
    {
        if (DataManager.Instance.MemberDict.TryGetValue(employeeId, out MemberData data))
        {
            CurrentMemberData = data.DeepCopy();
            UpdateAttackStrategy(); // 공격력 업데이트
        }
        else
        {
            CurrentMemberData = null;
        }
    }
    
    public void SetMemberData(MemberData memberData)
    {
        CurrentMemberData = memberData?.DeepCopy();
        UpdateAttackStrategy(); // 공격력 업데이트
    }
    
    /// <summary>
    /// 공격 전략 업데이트 (MemberData.Power 기반)
    /// </summary>
    public void UpdateAttackStrategy()
    {
        if (!string.IsNullOrEmpty(_projectilePrefabName))
        {
            // damage 파라미터 제거
            SetAttackStrategy(new RangedAttackStrategy(this, _attackSpeed, _detectionRange, _projectilePrefabName));
            Debug.Log($"[Member] {CurrentMemberData?.Name ?? "Unknown"} attack strategy updated (Current Power: {AttackDamage})");
        }
    }
    
    /// <summary>
    /// 공격 설정 변경 (공격 속도와 탐지 범위만)
    /// </summary>
    public void SetAttackSettings(float attackSpeed, float detectionRange)
    {
        _attackSpeed = attackSpeed;
        _detectionRange = detectionRange;
        UpdateAttackStrategy();
    }
    
    public void MoveToSeat(Vector2Int seatPosition)
    {
        var seatMovement = MovementPoolManager.Instance.Get<SeatMovement>()
            .Initialize(seatPosition, _gridManager);
        
        seatMovement.OnArrived += () =>
        {
            MemberSeatInfo seatInfo = MemberManager.Instance.GetMemberSeatInfo(this);
            IsFlipped = seatInfo.IsFlipped;
            IsFacingForward = seatInfo.IsFacingForward;
        };
        
        SetMovementStrategy(seatMovement);
    }
    
    public void DoWork() { SetStateWork(); }
    public void FinishWork() { SetStateIdle(); }
    
    public MemberSaveData GetSaveData()
    {
        return new MemberSaveData()
        {
            State = State,
            IsFacingForward = IsFacingForward,
            IsFlipped = IsFlipped,
            CellPosition = CellPosition,
            CurrentMemberData = CurrentMemberData,
            AIEnabled = AIEnabled,
            IsDispatched = IsDispatched
        };
    }
    
    public void LoadFromSaveData(MemberSaveData saveData)
    {
        if (saveData == null) 
            return;

        SetMemberData(saveData.CurrentMemberData);
        SetStateForced(saveData.State);
        IsFacingForward = saveData.IsFacingForward;
        IsFlipped = saveData.IsFlipped;
        CellPosition = saveData.CellPosition;
        AIEnabled = saveData.AIEnabled;
        IsDispatched = saveData.IsDispatched;
        transform.position = MapManager.Instance.CellToWorld(saveData.CellPosition);
        MapManager.Instance.MoveTo(this, saveData.CellPosition, true);
        ClearMovementStrategy();
        UpdateAttackStrategy(); // 공격력 업데이트
    }
    
    public void LoadFromSaveDataNoCellpos(MemberSaveData saveData)
    {
        if (saveData == null) 
            return;

        SetMemberData(saveData.CurrentMemberData);
        SetStateForced(saveData.State);
        IsFacingForward = saveData.IsFacingForward;
        IsFlipped = saveData.IsFlipped;
        AIEnabled = saveData.AIEnabled;
        IsDispatched = saveData.IsDispatched;
        ClearMovementStrategy();
        UpdateAttackStrategy(); // 공격력 업데이트
    }
    
    public Sprite GetMemberSprite(EPlayerImageType playerImageType = EPlayerImageType.Zombie)
    {
        if (CurrentMemberData == null) 
            return null;

        string imagePath = playerImageType == EPlayerImageType.Normal ? CurrentMemberData.NormalImagePath : CurrentMemberData.ZombieImagePath;
        if (string.IsNullOrEmpty(imagePath)) 
            return null;

        Sprite memberSprite = ResourceManager.Instance.Get<Sprite>(imagePath);
        if (memberSprite == null)
            Debug.LogWarning($"Failed to load sprite: {imagePath} for {CurrentMemberData.Name}");
        return memberSprite;
    }
}
