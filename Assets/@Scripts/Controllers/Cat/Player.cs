using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class Player : Cat
{
    private bool _isMoving = false;
    public bool CanMove => !_isMoving;
    public MemberData CurrentMemberData { get; private set; }
    // 이동 관련 변수
    private Vector3 _moveStart;
    private Vector3 _moveEnd;
    private Vector2Int _targetCell;
    private float _moveElapsed = 0f;
    private float _moveDuration = 0.3f;

    // AI 관련 변수
    public bool AIEnabled = true;
    private Vector2Int _aiTargetPosition;
    private List<Vector2Int> _path = new List<Vector2Int>();
    private bool _isSited = false;

    public override void Init()
    {
        base.Init();
    }
    public void SetMemberData(int employeeId)
    {
        if(DataManager.Instance.MemberDict.TryGetValue(employeeId, out MemberData data))
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
    
    // 저장 데이터 생성
    public PlayerSaveData GetSaveData()
    {
        PlayerSaveData playerSaveData = new PlayerSaveData()
        {
            State = State,
            IsFacingForward = IsFacingForward,
            IsFlipped = IsFlipped,
            CellPosition = CellPosition,
            CurrentMemberData = CurrentMemberData,
            AIEnabled = AIEnabled
        };
        return playerSaveData;
    }
    
    // 저장 데이터에서 로드
    public void LoadFromSaveData(PlayerSaveData saveData)
    {
        if (saveData == null)
        {
            Debug.LogWarning("PlayerSaveData is null!");
            return;
        }

        SetMemberData(saveData.CurrentMemberData);

        // 상태 복원
        State = saveData.State;
        IsFacingForward = saveData.IsFacingForward;
        IsFlipped = saveData.IsFlipped;
        CellPosition = saveData.CellPosition;
        AIEnabled = saveData.AIEnabled;
        
        // 위치 복원
        transform.position = MapManager.Instance.CellToWorld(saveData.CellPosition);
        MapManager.Instance.MoveTo(this, saveData.CellPosition, true);
        
        // 이동 중 상태 초기화
        _isMoving = false;
        _path.Clear();
        _aiTargetPosition = new Vector2Int(int.MinValue, int.MinValue);
    }

    //1셀 단위 이동
    public void MoveTo(Vector2Int targetCell)
    {
        if (_isMoving)
            return;
        if (!MapManager.Instance.CanMove(targetCell))
            return;

        _isMoving = true;
        State = ECatState.Move;
        MapManager.Instance.MoveTo(this, targetCell, false); // 점유만 처리, 위치는 직접 이동
        _moveStart = transform.position;
        _moveEnd = MapManager.Instance.CellToWorld(targetCell);
        _targetCell = targetCell;
        _moveElapsed = 0f;

        // 이동 방향에 따라 Facing 설정
        Vector3 direction = (_moveEnd - _moveStart).normalized;
        IsFacingForward = direction.y <= 0; // y가 늘어나는 쪽이면 false, 아니면 true
        IsFlipped = direction.x < 0; // 왼쪽 방향으로 이동하면 flip
    }

    public override void Update()
    {
        if (_isMoving)
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

                // 경로의 다음 스텝으로 이동
                if (_path.Count > 0)
                {
                    _path.RemoveAt(0); // 현재 위치 제거
                }
            }
        }
        else if (AIEnabled && State == ECatState.Idle)
        {
            if (_path.Count > 0)
            {
                if (MapManager.Instance.CanMove(_path[0]))
                {
                    MoveTo(_path[0]);
                }
                else
                {
                    // 경로 막힘, 다시 계산
                    _path = MapManager.Instance.FindPath(CellPosition, _aiTargetPosition);
                    if (_path.Count > 0)
                    {
                        MoveTo(_path[0]);
                    }
                    else
                    {
                        _path.Clear();
                        _aiTargetPosition = new Vector2Int(int.MinValue, int.MinValue);
                    }
                }
            }
            else
            {
                //작업 자리로 갔는지 확인
                if (_isSited == true)
                {
                    PlayerSeatInfo seatInfo = MemberManager.Instance.GetPlayerSeatInfo(this);
                    IsFlipped = seatInfo.IsFlipped;
                    IsFacingForward = seatInfo.IsFacingForward;
                    _isSited = false;
                }

                if (GameDevManager.Instance.CurrentGameDevType == EGameDevType.None)
                    TryAIMove();
            }
        }
    }

    private void TryAIMove()
    {
        // 새로운 랜덤 목표 선택
        List<Vector2Int> walkableCells = MapManager.Instance.GetWalkableCells();
        if (walkableCells.Count == 0) 
            return;

        Vector2Int randomTarget;
        do
        {
            int randomIndex = UnityEngine.Random.Range(0, walkableCells.Count);
            randomTarget = walkableCells[randomIndex];
        } while (randomTarget == CellPosition); // 현재 위치 제외

        _aiTargetPosition = randomTarget;
        _path = MapManager.Instance.FindPath(CellPosition, _aiTargetPosition);

        if (_path.Count > 0)
        {
            MoveTo(_path[0]);
        }
    }
    public void MoveToSeat(Vector2Int targetPos)
    {
        // 현재 이동 중이라면 이동 상태 초기화 및 즉시 중단
        if (_isMoving)
        {
            _isMoving = false;
            State = ECatState.Idle;
            transform.position = MapManager.Instance.CellToWorld(CellPosition); // 현재 셀 위치로 보정
        }

        // AI 이동 경로와 목표 초기화 (자리 이동이 우선)
        _path.Clear();
        _aiTargetPosition = targetPos;
        CoroutineManager.Instance.StartCoroutine(CoFindPath(CellPosition, _aiTargetPosition));

        if (_path.Count > 0)
        {
            MoveTo(_path[0]);
            _isSited = true;
        }
    }
    private IEnumerator CoFindPath(Vector2Int start, Vector2Int goal)
    {
        int count = 5;
        WaitForSeconds wait = new WaitForSeconds(_moveDuration);
        while (count > 0)
        {
            count--;
            _path = MapManager.Instance.FindPath(start, goal);
            if (_path.Count > 0)
                yield break;

            yield return wait;
        }
    }
    public void DoWork()
    {
        State = ECatState.Work;
    }
    public void FinishWork()
    {
        State = ECatState.Idle;
    }
    private void OnDrawGizmosSelected()
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

    /// <summary>
    /// 멤버의 스프라이트 가져오기 (정상 상태 기준)
    /// </summary>
    /// <returns>멤버 스프라이트 또는 null</returns>
    public Sprite GetMemberSprite(EPlayerImageType playerImageType = EPlayerImageType.Zombie)
    {
        if (CurrentMemberData == null)
            return null;

        string imagePath = playerImageType == EPlayerImageType.Normal ? CurrentMemberData.NormalImagePath : CurrentMemberData.ZombieImagePath;
        
        if (string.IsNullOrEmpty(imagePath))
            return null;

        Sprite memberSprite = ResourceManager.Instance.Get<Sprite>(imagePath);
        
        if (memberSprite == null)
        {
            Debug.LogWarning($"Failed to load sprite at path: {imagePath} for member: {CurrentMemberData.Name}");
        }
        
        return memberSprite;
    }
}
