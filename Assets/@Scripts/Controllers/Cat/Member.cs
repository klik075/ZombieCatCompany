using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class Member : MovableCat
{
    public MemberData CurrentMemberData { get; private set; }
    private bool _isSited = false;
    public override void Init()
    {
        base.Init();
    }

    public void SetMemberData(int employeeId)
    {
        if (DataManager.Instance.MemberDict.TryGetValue(employeeId, out MemberData data))
        {
            CurrentMemberData = data.DeepCopy();
        }
        else
        {
            CurrentMemberData = null;
        }
    }

    public void SetMemberData(MemberData memberData)
    {
        CurrentMemberData = memberData.DeepCopy();
    }

    // 한 칸 이동이 완료된 후 호출되는 메서드
    protected override void OnMoveCompleted()
    {
        
    }

    // 모든 이동이 끝났을 때 호출되는 메서드
    protected override void OnAIIdle()
    {
        if (_isSited)
        {
            MemberSeatInfo seatInfo = MemberManager.Instance.GetMemberSeatInfo(this);
            IsFlipped = seatInfo.IsFlipped;
            IsFacingForward = seatInfo.IsFacingForward;
            _isSited = false;
        }

        if (GameDevManager.Instance.CurrentGameDevType == EGameDevType.None)
        {
            TryRandomMove();
        }
        else if(GameManager.Instance.GameState == EGameState.FoodPurchase)
        {

        }
    }

    private void TryRandomMove()
    {
        var walkableCells = MapManager.Instance.GetWalkableCells();
        if (walkableCells.Count == 0) return;

        Vector2Int randomTarget;
        do
        {
            int randomIndex = UnityEngine.Random.Range(0, walkableCells.Count);
            randomTarget = walkableCells[randomIndex];
        } while (randomTarget == CellPosition);

        _aiTargetPosition = randomTarget;
        _path = MapManager.Instance.FindPath(CellPosition, _aiTargetPosition);

        if (_path.Count > 0)
        {
            MoveTo(_path[0]);
        }
    }

    public void MoveToSeat(Vector2Int targetPos)
    {
        MoveToPosition(targetPos);
        _isSited = true;
    }

    public void DoWork()
    {
        State = ECatState.Work;
    }

    public void FinishWork()
    {
        State = ECatState.Idle;
    }

    // 저장/로드 로직은 Player만 필요하므로 여기 유지
    public PlayerSaveData GetSaveData()
    {
        return new PlayerSaveData()
        {
            State = State,
            IsFacingForward = IsFacingForward,
            IsFlipped = IsFlipped,
            CellPosition = CellPosition,
            CurrentMemberData = CurrentMemberData,
            AIEnabled = AIEnabled
        };
    }

    public void LoadFromSaveData(PlayerSaveData saveData)
    {
        if (saveData == null) return;

        SetMemberData(saveData.CurrentMemberData);
        State = saveData.State;
        IsFacingForward = saveData.IsFacingForward;
        IsFlipped = saveData.IsFlipped;
        CellPosition = saveData.CellPosition;
        AIEnabled = saveData.AIEnabled;
        
        transform.position = MapManager.Instance.CellToWorld(saveData.CellPosition);
        MapManager.Instance.MoveTo(this, saveData.CellPosition, true);
        
        _isMoving = false;
        _path.Clear();
        _aiTargetPosition = new Vector2Int(int.MinValue, int.MinValue);
    }

    public Sprite GetMemberSprite(EPlayerImageType playerImageType = EPlayerImageType.Zombie)
    {
        if (CurrentMemberData == null) return null;

        string imagePath = playerImageType == EPlayerImageType.Normal 
            ? CurrentMemberData.NormalImagePath 
            : CurrentMemberData.ZombieImagePath;
        
        if (string.IsNullOrEmpty(imagePath)) return null;

        Sprite memberSprite = ResourceManager.Instance.Get<Sprite>(imagePath);
        
        if (memberSprite == null)
        {
            Debug.LogWarning($"Failed to load sprite: {imagePath} for {CurrentMemberData.Name}");
        }
        
        return memberSprite;
    }
}
