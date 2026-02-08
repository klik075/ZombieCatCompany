using System.Collections.Generic;
using UnityEngine;
using static Define;

/// <summary>
/// Member의 AI 로직 (프로젝트 특정)
/// </summary>
public class MemberAI
{
    private Member _owner;
    private IGridManager _gridManager;
    private bool _isEnabled = true;
    
    // 행동 확률 설정
    private const float MOVE_PROBABILITY = 0.5f;  // 50% 확률로 이동
    private const float MIN_WAIT_TIME = 1f;       // 최소 대기 시간
    private const float MAX_WAIT_TIME = 2f;       // 최대 대기 시간
    
    public bool IsEnabled
    {
        get => _isEnabled;
        set => _isEnabled = value;
    }
    
    public MemberAI(Member owner, IGridManager gridManager)
    {
        _owner = owner;
        _gridManager = gridManager;
    }
    
    public void Update()
    {
        if (!_isEnabled) 
            return;
        
        // 게임 개발 중이 아닐 때만 동작
        if (GameDevManager.Instance.CurrentGameDevType == EGameDevType.None)
        {
            // 음식 구매 중에는 이동하지 않음
            if (GameManager.Instance.GameState == EGameState.FoodPurchase)
                return;
            
            // 랜덤으로 이동 또는 대기 선택
            if (Random.value < MOVE_PROBABILITY)
            {
                TryRandomMove();
            }
            else
            {
                TryWait();
            }
        }
    }
    
    /// <summary>
    /// 랜덤 위치로 이동 시도
    /// </summary>
    private void TryRandomMove()
    {
        List<Vector2Int> walkableCells = _gridManager.GetWalkableCells();
        if (walkableCells.Count == 0) 
            return;
        
        Vector2Int randomTarget;
        do
        {
            int randomIndex = Random.Range(0, walkableCells.Count);
            randomTarget = walkableCells[randomIndex];
        } while (randomTarget == _owner.CellPosition);
        
        List<Vector2Int> path = _gridManager.FindPath(_owner.CellPosition, randomTarget);
        if (path.Count > 0)
        {
            // 풀에서 가져와서 초기화
            var movement = MovementPoolManager.Instance.Get<PathMovement>()
                .Initialize(path, _gridManager);
            _owner.SetMovementStrategy(movement);
        }
    }
    
    /// <summary>
    /// 현재 위치에서 대기
    /// </summary>
    private void TryWait()
    {
        float waitTime = Random.Range(MIN_WAIT_TIME, MAX_WAIT_TIME);
        
        // 풀에서 가져와서 초기화
        var movement = MovementPoolManager.Instance.Get<WaitMovement>()
            .Initialize(waitTime);
        _owner.SetMovementStrategy(movement);
    }
    
    public void OnMoveCompleted()
    {
        // 이동 완료 시 처리 (필요시 확장)
    }
}
