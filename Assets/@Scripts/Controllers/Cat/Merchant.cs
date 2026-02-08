using System;
using UnityEngine;
using static Define;

/// <summary>
/// Merchant NPC (이동/애니메이션만 담당)
/// </summary>
public class Merchant : Cat
{
    private Vector2Int _doorWay;
    private Vector2Int _bossNearPosition;

    // 이벤트로 외부와 통신
    public event Action OnArrivedAtBoss;
    public event Action OnArrivedAtDoor;

    protected override void Awake()
    {
        base.Awake();

        _doorWay = MemberManager.Instance.DoorWay;
        _bossNearPosition = MapManager.Instance.FindNearPosition(
            MemberManager.Instance.GetMemberSeat(0)
        );
    }

    protected override void Start()
    {
        base.Start();
        _mover.OnMoveCompleted += OnMoveCompleted;
    }

    public override void Update()
    {
        base.Update();
    }

    /// <summary>
    /// 이동 완료 시 이벤트 발생 (이동 책임만)
    /// </summary>
    private void OnMoveCompleted()
    {
        if (CellPosition == _bossNearPosition)
        {
            OnArrivedAtBoss?.Invoke();
        }
        else if (CellPosition == _doorWay)
        {
            OnArrivedAtDoor?.Invoke();
        }
    }

    /// <summary>
    /// 보스 근처로 이동 (이동 책임만)
    /// </summary>
    public void MoveToBossNearPosition()
    {
        var movement = MovementPoolManager.Instance.Get<PathMovement>()
            .Initialize(_gridManager.FindPath(CellPosition, _bossNearPosition), _gridManager);
        SetMovementStrategy(movement);
    }

    /// <summary>
    /// 문으로 이동 (이동 책임만)
    /// </summary>
    public void MoveToDoorWay()
    {
        var movement = MovementPoolManager.Instance.Get<PathMovement>()
            .Initialize(_gridManager.FindPath(CellPosition, _doorWay), _gridManager);
        SetMovementStrategy(movement);
    }
}