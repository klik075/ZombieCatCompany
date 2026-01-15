using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

[System.Serializable]
public class HireResult
{
    public List<MemberData> MemberDatas;//고용된 멤버 데이터들
    public string Message;
    public HireMethodType HireMethod;//고용 방법
}
public class MemberManager : Singleton<MemberManager>
{
    public const int MAIN_CHARACTER_ID = 100;
    public const int MAX_PLAYERS = 4;
    private Player[] _players = new Player[MAX_PLAYERS];

    // 선택된 플레이어 관리
    private int _selectedPlayerIndex = 0;

    // 현재 선택된 플레이어의 인덱스
    public int SelectedPlayerIndex
    {
        get { return _selectedPlayerIndex; }
        private set 
        {
            _selectedPlayerIndex = value;
            EventManager.Instance.TriggerEvent(EEventType.SelectedMemberChanged);
        }
    }
    // 현재 선택된 플레이어
    public Player SelectedPlayer
    {
        get { return GetMember(_selectedPlayerIndex); }
    }
    // 주인공(첫 번째 구성원)은 해고 불가
    public Player MainCharacter => _players[0];

    private int playerCount = 0;
    // 현재 구성원 수
    public int PlayerCount 
    { 
        get { return playerCount; }
        private set 
        {
            playerCount = value;
            EventManager.Instance.TriggerEvent(EEventType.MemberListChanged); 
        }
    }
    public HireResult HireResult { get; private set; }

    #region Member Selection Management

    /// <summary>
    /// 특정 멤버를 선택합니다
    /// </summary>
    public void SelectMember(Player member)
    {
        int index = GetIndex(member);
        if (index != -1)
        {
            SelectMemberByIndex(index);
        }
        else
        {
            Debug.LogWarning("Cannot select member: member not found in the team");
        }
    }

    /// <summary>
    /// 인덱스로 멤버를 선택합니다
    /// </summary>
    public void SelectMemberByIndex(int index)
    {
        if (index >= 0 && index < PlayerCount && _players[index] != null)
        {
            SelectedPlayerIndex = index;
            //OnSelectedMemberChanged?.Invoke(SelectedPlayer); 선택 트리거 발동
            Debug.Log($"Selected member: {SelectedPlayer.CurrentMemberData?.Name} (Index: {index})");
        }
        else
        {
            Debug.LogWarning($"Cannot select member at index {index}: invalid index or null member");
        }
    }

    /// <summary>
    /// 다음 멤버를 선택합니다 (순환)
    /// </summary>
    public void SelectNextMember()
    {
        if (PlayerCount <= 1) return;
        
        int nextIndex = (SelectedPlayerIndex + 1) % PlayerCount;
        SelectMemberByIndex(nextIndex);
    }

    /// <summary>
    /// 이전 멤버를 선택합니다 (순환)
    /// </summary>
    public void SelectPreviousMember()
    {
        if (PlayerCount <= 1) return;
        
        int prevIndex = (SelectedPlayerIndex - 1 + PlayerCount) % PlayerCount;
        SelectMemberByIndex(prevIndex);
    }

    /// <summary>
    /// 첫 번째 멤버(사장)를 선택합니다
    /// </summary>
    public void SelectMainCharacter()
    {
        SelectMemberByIndex(0);
    }

    #endregion

    // 게임 시작 시 주인공 초기화
    public void InitBoss()
    {
        if (_players[0] != null)
        {
            Debug.LogWarning("Main character already exists!");
            return;
        }

        // 주인공 스폰
        Player mainCharacter = ObjectManager.Instance.SpawnPlayer("Cat");
        mainCharacter.SetMemberData(MAIN_CHARACTER_ID);
        _players[0] = mainCharacter;
        PlayerCount = 1;

        // 주인공을 기본 선택으로 설정
        SelectedPlayerIndex = 0;

        // 초기 위치 설정
        Vector2Int startCell = new Vector2Int(0, 1);
        MapManager.Instance.MoveTo(mainCharacter, startCell, true);

        Debug.Log("Main character initialized and selected");
    }

    //고용 종류에 따라 랜덤한 직원 리스트 생성 후 매개 변수로 받은 액션에게 Result 전달 Invoke
    public HireResult GenerateHireResult(HireMethodType hireMethodType)
    {
        //hireMethodType 마다 다른 로직

        HireResult result = new HireResult();

        result.HireMethod = hireMethodType;
        result.MemberDatas = GenerateMemberDatas();
        result.Message = $"모집했던 결과가 나왔다냥.\r\n고양이 {result.MemberDatas.Count}마리가 지원했다냥.\r\n누구를 채용할까냥?";

        return result;
    }
    public IEnumerator StartHiringProcess(HireMethodType hireMethodType)
    {
        HireResult = null;
        // 모집 중 상태로 변경
        GameManager.Instance.IsRecruiting = true;

        // 대기 시간
        float waitTime = GetHireWaitTime(hireMethodType);
        yield return new WaitForSeconds(waitTime);

        // 채용 결과 생성
        HireResult = GenerateHireResult(hireMethodType);
    }
    private float GetHireWaitTime(HireMethodType hireMethodType)
    {
        switch (hireMethodType)
        {
            case HireMethodType.Internet:
                return 3f; // 추후에 변경
            default:
                return 1f;
        }
    }
    public List<MemberData> GenerateMemberDatas(int count = 4)
    {
        List<MemberData> memberDatas = new List<MemberData>();
        
        for (int i = 0; i < count; i++)
        {
            int randomID = UnityEngine.Random.Range(101, 104);// 101~103 사이의 랜덤한 직원 ID 생성
            MemberData memberData = new MemberData();
            DataManager.Instance.MemberDict.TryGetValue(randomID, out memberData);
            memberDatas.Add(memberData);
        }
        
        return memberDatas;
    }
    public bool HireRandomMember(int start = 101, int end = 104)
    {
        //나중에 범위 체크 할 것

        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond + PlayerCount);

        int employeeId = UnityEngine.Random.Range(start, end);

        if(HireMember(employeeId))
            return true;

        return false;
    }
    public bool HireMember(int employeeId)
    {
        if (DataManager.Instance.MemberDict.TryGetValue(employeeId, out MemberData memberData))
        {
            HireMember(memberData);
            return true;
        }

        return false;
    }
    public bool HireMember(MemberData memberData)
    {
        if (PlayerCount >= MAX_PLAYERS)
        {
            Debug.LogWarning("Cannot hire more members. Team is full!");
            return false;
        }

        // 현재 MemberCount 인덱스에 새 구성원 추가
        Player newPlayer = ObjectManager.Instance.SpawnPlayer("Cat");
        newPlayer.SetMemberData(memberData);
        _players[PlayerCount] = newPlayer;
        PlayerCount++;

        // 초기 위치 설정 (주인공 근처)
        Vector2Int spawnCell = FindSpawnPosition();
        MapManager.Instance.MoveTo(newPlayer, spawnCell, true);

        return true;
    }
    public bool CanFireMember(int memberIndex, bool ignoringStatus = false)
    {
        // 주인공은 해고 불가
        if (memberIndex == 0)
            return false;

        // 유효성 검사
        if (memberIndex < 0 || memberIndex >= PlayerCount)
            return false;

        if (_players[memberIndex] == null)
            return false;

        MemberData memberData = _players[memberIndex].CurrentMemberData;

        if (memberData == null)
            return false;

        if(ignoringStatus == true)
            return true;

        if (memberData.State == EMemberStateType.Dispatch)
            return false;

        return true;
    }
    // 구성원 해고
    public bool FireSelectedMember(bool ignoringStatus = false)
    {
        bool success = FireMember(SelectedPlayerIndex, ignoringStatus);

        if(success)
            SelectedPlayerIndex--;

        return success;
    }
    public bool FireMember(int memberIndex, bool ignoringStatus = false)
    {
        bool canFire = CanFireMember(memberIndex, ignoringStatus);

        if (!canFire)
        {
            Debug.LogWarning($"Cannot fire member at index {memberIndex}");
            return false;
        }

        // 맵에서 위치 해제
        MapManager.Instance.UnregisterCat(_players[memberIndex].CellPosition);

        // 구성원 제거
        ObjectManager.Instance.Despawn(_players[memberIndex]);
        _players[memberIndex] = null;

        // 뒤에 있는 구성원들을 앞으로 한 칸씩 이동
        ShiftMembersForward(memberIndex);
        PlayerCount--;

        Debug.Log($"Member at index {memberIndex} fired and members shifted forward. Current count: {PlayerCount}");
        return true;
    }

    // 구성원들을 앞으로 한 칸씩 이동
    private void ShiftMembersForward(int startIndex)
    {
        for (int i = startIndex; i < PlayerCount - 1; i++)
        {
            _players[i] = _players[i + 1];
        }
        // 마지막 자리는 null로 설정
        _players[PlayerCount - 1] = null;
    }

    // 특정 인덱스의 구성원 가져오기
    public Player GetMember(int memberIndex)
    {
        if (memberIndex < 0 || memberIndex >= PlayerCount)
            return null;
        return _players[memberIndex];
    }

    // Player 객체로 인덱스 찾기
    public int GetIndex(Player player)
    {
        if (player == null)
            return -1;

        for (int i = 0; i < PlayerCount; i++)
        {
            if (_players[i] == player)
                return i;
        }

        return -1; // 찾지 못한 경우
    }

    // 모든 구성원 저장 데이터 생성
    public List<PlayerSaveData> GetSaveData()
    {
        List<PlayerSaveData> saveDatas = new List<PlayerSaveData>();
        
        for (int i = 0; i < PlayerCount; i++)
        {
            if (_players[i] != null)
            {
                saveDatas.Add(_players[i].GetSaveData());
            }
        }
        
        return saveDatas;
    }
    
    // 저장 데이터에서 구성원 복원
    public void LoadFromSaveData(List<PlayerSaveData> saveDatas)
    {
        // 기존 멤버 정리
        ClearAllMembers();
        
        if (saveDatas == null || saveDatas.Count == 0)
        {
            Debug.LogWarning("No member save data to load!");
            return;
        }
        
        for (int i = 0; i < saveDatas.Count && i < MAX_PLAYERS; i++)
        {
            PlayerSaveData saveData = saveDatas[i];
            
            // Player 스폰
            Player player = ObjectManager.Instance.SpawnPlayer("Cat");
            
            // 저장된 데이터 로드
            player.LoadFromSaveData(saveData);
            
            // 멤버 배열에 추가
            _players[i] = player;
        }
        
        PlayerCount = saveDatas.Count;

        // 첫 번째 멤버를 기본 선택으로 설정
        SelectedPlayerIndex = 0;
        
        Debug.Log($"Loaded {PlayerCount} members from save data");
    }
    
    // 모든 구성원 정리
    public void ClearAllMembers()
    {
        for (int i = 0; i < MAX_PLAYERS; i++)
        {
            if (_players[i] != null)
            {
                MapManager.Instance.UnregisterCat(_players[i].CellPosition);
                ObjectManager.Instance.Despawn(_players[i]);
                _players[i] = null;
            }
        }
        PlayerCount = 0;
        SelectedPlayerIndex = 0;
    }

    // 모든 구성원 가져오기 (null 제외)
    public List<Player> GetAllMembers()
    {
        List<Player> players = new List<Player>();
        for (int i = 0; i < PlayerCount; i++)
        {
            if (_players[i] != null)
                players.Add(_players[i]);
        }
        return players;
    }

    // 빈 슬롯 개수 가져오기
    public int GetEmptySlotCount()
    {
        return MAX_PLAYERS - PlayerCount;
    }

    // 팀이 가득 찼는지 확인
    public bool IsTeamFull()
    {
        return PlayerCount >= MAX_PLAYERS;
    }

    // 스폰 위치 찾기 (주인공 근처 빈 공간)
    private Vector2Int FindSpawnPosition()
    {
        if (_players[0] == null)
            return new Vector2Int(0, 0);

        Vector2Int mainCharacterPos = _players[0].CellPosition;

        // 주변 8방향 탐색
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1)
        };

        foreach (var dir in directions)
        {
            Vector2Int checkPos = mainCharacterPos + dir;
            if (MapManager.Instance.CanMove(checkPos))
            {
                return checkPos;
            }
        }

        // 주변에 빈 공간이 없으면 걸을 수 있는 랜덤 위치
        List<Vector2Int> walkableCells = MapManager.Instance.GetWalkableCells();
        if (walkableCells.Count > 0)
        {
            return walkableCells[UnityEngine.Random.Range(0, walkableCells.Count)];
        }

        // 최후의 수단
        return new Vector2Int(0, 2);
    }
}
