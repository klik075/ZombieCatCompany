using UnityEngine;
using static Define;

public class Member : Cat
{
    public MemberData CurrentMemberData { get; private set; }
    
    private MemberAI _ai;

    [SerializeField] private string _projectilePrefabName = "Projectile";
    [SerializeField] private float _detectionRange = 5f;
    [SerializeField] private int _attackDamage = 3;
    [SerializeField] private float _attackSpeed = 0.5f;
    
    public bool AIEnabled
    {
        get => _ai?.IsEnabled ?? false;
        set 
        { 
            if (_ai != null) 
                _ai.IsEnabled = value; 
        }
    }

    protected override void Awake()
    {
        base.Awake();

        // 원거리 공격 전략 설정
        if (!string.IsNullOrEmpty(_projectilePrefabName))
        {
            SetAttackStrategy(new RangedAttackStrategy(this, _attackDamage, _attackSpeed, _detectionRange, _projectilePrefabName));
        }
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
            CurrentMemberData = data.DeepCopy();
        else
            CurrentMemberData = null;
    }
    
    public void SetMemberData(MemberData memberData)
    {
        CurrentMemberData = memberData?.DeepCopy();
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

    /// <summary>
    /// 발사체 프리팹 이름 설정
    /// </summary>
    public void SetProjectilePrefabName(string prefabName)
    {
        _projectilePrefabName = prefabName;

        if (!string.IsNullOrEmpty(_projectilePrefabName))
        {
            SetAttackStrategy(new RangedAttackStrategy(this, _attackDamage, _attackSpeed, _detectionRange, _projectilePrefabName));
        }
    }

    /// <summary>
    /// 공격 설정 변경
    /// </summary>
    public void SetAttackSettings(int damage, float attackSpeed, float detectionRange)
    {
        _attackDamage = damage;
        _attackSpeed = attackSpeed;
        _detectionRange = detectionRange;

        if (!string.IsNullOrEmpty(_projectilePrefabName))
        {
            SetAttackStrategy(new RangedAttackStrategy(this, _attackDamage, _attackSpeed, _detectionRange, _projectilePrefabName));
        }
    }
    
    public MemberSaveData GetSaveData()
    {
        return new MemberSaveData()
        {
            State = State,
            IsFacingForward = IsFacingForward,
            IsFlipped = IsFlipped,
            CellPosition = CellPosition,
            CurrentMemberData = CurrentMemberData,
            AIEnabled = AIEnabled
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
        transform.position = MapManager.Instance.CellToWorld(saveData.CellPosition);
        MapManager.Instance.MoveTo(this, saveData.CellPosition, true);
        ClearMovementStrategy();
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
        ClearMovementStrategy();
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
