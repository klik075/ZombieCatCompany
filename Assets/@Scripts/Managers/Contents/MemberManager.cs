using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

[System.Serializable]
public class HireResult
{
    public List<MemberData> MemberDatas;//고용된 멤버 데이터들
    public string[] Messages;
    public HireMethodType HireMethod;//고용 방법
}
public class MemberManager : Singleton<MemberManager>
{
    public const int MAIN_CHARACTER_ID = 100;
    public const int MAX_PLAYERS = 4;
    private Player[] _players = new Player[MAX_PLAYERS];
    
    // Player들의 개인 지정 자리 (MapManager에서 사용하는 좌표)
    private Vector2Int[] _playerSeat = new Vector2Int[MAX_PLAYERS];
    private Vector2Int _doorWay = new Vector2Int(-4, -15);

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
    public HireResult CurrentHireResult { get; private set; }
    public MemberData SelectedHireMemberData { get; private set; }

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
    public void SelectedHireMember(int index)
    {
        if (CurrentHireResult == null || CurrentHireResult.MemberDatas == null || index < 0 || index >= CurrentHireResult.MemberDatas.Count)
        {
            Debug.LogWarning("Cannot select hire member: invalid index or no hire result");
            return;
        }

        SelectedHireMemberData = CurrentHireResult.MemberDatas[index];
    }
    public void EndHire()
    {
        CurrentHireResult = null;
        SelectedHireMemberData = null;
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

        // Player 지정 자리 초기화
        InitializePlayerSeats();

        // 주인공 스폰
        Player mainCharacter = ObjectManager.Instance.SpawnPlayer("Cat");
        mainCharacter.SetMemberData(MAIN_CHARACTER_ID);
        _players[0] = mainCharacter;
        PlayerCount = 1;

        // 주인공을 기본 선택으로 설정
        SelectedPlayerIndex = 0;

        // 주인공을 지정 자리로 이동
        MovePlayerToSeat(0, true);
    }

    //고용 종류에 따라 랜덤한 직원 리스트 생성 후 매개 변수로 받은 액션에게 Result 전달 Invoke
    public HireResult GenerateHireResult(HireMethodType hireMethodType)
    {
        //hireMethodType 마다 다른 로직

        HireResult result = new HireResult();

        result.HireMethod = hireMethodType;
        result.MemberDatas = GenerateMemberDatas();
        result.Messages = new string[] { $"{result.MemberDatas.Count}" };

        return result;
    }
    public IEnumerator CoStartHiringProcess(HireMethodType hireMethodType)
    {
        CurrentHireResult = null;
        // 모집 중 상태로 변경
        GameManager.Instance.IsRecruiting = true;

        // 대기 시간
        float waitTime = GetHireWaitTime(hireMethodType);
        yield return new WaitForSeconds(waitTime);

        // 채용 결과 생성
        CurrentHireResult = GenerateHireResult(hireMethodType);
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

        if (memberData == null)
            return false;

        int hireCost = memberData.SalaryToValue(ESalaryType.Deposit);
        if (GameManager.Instance.Gold < hireCost)
            return false;

        GameManager.Instance.Gold -= hireCost;
        if(CurrentHireResult != null)
            CurrentHireResult.MemberDatas.Remove(memberData);

        // 현재 MemberCount 인덱스에 새 구성원 추가
        Player newPlayer = ObjectManager.Instance.SpawnPlayer("Cat");
        MemberData newMemberData = InfectMemberData(memberData);
        newPlayer.SetMemberData(newMemberData);
        _players[PlayerCount] = newPlayer;
        PlayerCount++;
        MapManager.Instance.MoveTo(newPlayer, FindSpawnPosition(_doorWay), true);

        return true;
    }
    public MemberData InfectMemberData(MemberData memberData)
    {
        MemberData infectedMemberData = memberData.DeepCopy();
        
        // 각 능력치에서 0~현재값 사이의 랜덤한 값을 빼서 전투력으로 전환
        int totalTransferredPower = 0;

        // Programming 능력치 처리
        totalTransferredPower += InfectAbility(ref infectedMemberData.Programming);

        // Scenario 능력치 처리
        totalTransferredPower += InfectAbility(ref infectedMemberData.Scenario);

        // Graphics 능력치 처리
        totalTransferredPower += InfectAbility(ref infectedMemberData.Graphics);

        // Sound 능력치 처리
        totalTransferredPower += InfectAbility(ref infectedMemberData.Sound);

        // 전환된 능력치를 전투력에 추가
        infectedMemberData.Power += totalTransferredPower;

        return infectedMemberData;
    }
    private int InfectAbility(ref int statValue)
    {
        int transferAmount = 0;
        if (statValue > 0)
        {
            transferAmount = UnityEngine.Random.Range(0, statValue + 1);
            statValue -= transferAmount;
        }
        return transferAmount;
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
            
            // 이동된 멤버를 새로운 인덱스에 맞는 지정 자리로 이동
            if (_players[i] != null)
            {
                MovePlayerToSeat(i, true);
            }
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
        
        // Player 지정 자리 초기화
        InitializePlayerSeats();
        
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
    private Vector2Int FindSpawnPosition(Vector2Int startPos)
    {
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
            Vector2Int checkPos = startPos + dir;
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

    #region 게임 개발 관련 멤버 조회

    /// <summary>
    /// 특정 게임 개발 단계에 적합한 멤버들을 가져옵니다
    /// </summary>
    /// <param name="gameDevType">게임 개발 단계</param>
    /// <returns>적합한 멤버들의 리스트</returns>
    public List<Player> GetMembersForGameDevType(EGameDevType gameDevType)
    {
        List<Player> suitableMembers = new List<Player>();
        
        for (int i = 0; i < PlayerCount; i++)
        {
            if (_players[i] != null && IsMemberSuitableForDevType(_players[i], gameDevType))
            {
                suitableMembers.Add(_players[i]);
            }
        }
        
        return suitableMembers;
    }

    /// <summary>
    /// 특정 게임 개발 단계에 적합한 멤버 수를 반환합니다
    /// </summary>
    /// <param name="gameDevType">게임 개발 단계</param>
    /// <returns>적합한 멤버 수</returns>
    public int GetSuitableMemberCount(EGameDevType gameDevType)
    {
        int count = 0;
        
        for (int i = 0; i < PlayerCount; i++)
        {
            if (_players[i] != null && IsMemberSuitableForDevType(_players[i], gameDevType))
            {
                count++;
            }
        }
        
        return count;
    }

    /// <summary>
    /// 특정 게임 개발 단계에 적합한 멤버를 인덱스로 가져옵니다
    /// </summary>
    /// <param name="gameDevType">게임 개발 단계</param>
    /// <param name="index">적합한 멤버들 중의 인덱스 (0부터 시작)</param>
    /// <returns>해당 인덱스의 멤버 또는 null</returns>
    public Player GetSuitableMemberByIndex(EGameDevType gameDevType, int index)
    {
        List<Player> suitableMembers = GetMembersForGameDevType(gameDevType);
        
        if (index >= 0 && index < suitableMembers.Count)
        {
            return suitableMembers[index];
        }
        
        return null;
    }

    /// <summary>
    /// 멤버가 특정 게임 개발 단계에 적합한지 확인합니다
    /// </summary>
    /// <param name="member">확인할 멤버</param>
    /// <param name="gameDevType">게임 개발 단계</param>
    /// <returns>적합하면 true</returns>
    private bool IsMemberSuitableForDevType(Player member, EGameDevType gameDevType)
    {
        if (member?.CurrentMemberData == null)
            return false;

        ERoleType role = member.CurrentMemberData.Role;
        
        // Boss는 항상 포함
        if (role == ERoleType.Boss)
            return true;
            
        // 개발 타입에 따른 적합한 역할 확인
        switch (gameDevType)
        {
            case EGameDevType.Scenario:
                return role == ERoleType.Planner;
            case EGameDevType.Graphics:
                return role == ERoleType.Designer;
            case EGameDevType.Sound:
                return role == ERoleType.SoundWriter;
            case EGameDevType.Debug:
                // 디버그 단계에서는 모든 역할이 참여 가능
                return true;
            default:
                return false;
        }
    }

    #endregion

    #region Player Seat Management

    /// <summary>
    /// Player들의 개인 지정 자리 초기화
    /// _players 배열의 순서대로 지정된 자리 할당
    /// </summary>
    private void InitializePlayerSeats()
    {
        _playerSeat[0] = new Vector2Int(-6, 1);//사장 자리
        _playerSeat[1] = new Vector2Int(0, 1);//직원 1
        _playerSeat[2] = new Vector2Int(-7, -8);//직원 2
        _playerSeat[3] = new Vector2Int(-1, -8);//직원 3
    }

    /// <summary>
    /// 특정 Player의 지정 자리 가져오기
    /// </summary>
    /// <param name="playerIndex">Player의 인덱스</param>
    /// <returns>해당 Player의 지정 자리 좌표</returns>
    public Vector2Int GetPlayerSeat(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= MAX_PLAYERS)
        {
            Debug.LogWarning($"Invalid player index: {playerIndex}");
            return Vector2Int.zero;
        }

        return _playerSeat[playerIndex];
    }

    /// <summary>
    /// 특정 Player 객체의 지정 자리 가져오기
    /// </summary>
    /// <param name="player">Player 객체</param>
    /// <returns>해당 Player의 지정 자리 좌표</returns>
    public Vector2Int GetPlayerSeat(Player player)
    {
        int index = GetIndex(player);
        if (index == -1)
        {
            Debug.LogWarning("Player not found in team");
            return Vector2Int.zero;
        }

        return GetPlayerSeat(index);
    }

    /// <summary>
    /// 특정 Player의 지정 자리 변경
    /// </summary>
    /// <param name="playerIndex">Player의 인덱스</param>
    /// <param name="newSeat">새로운 지정 자리 좌표</param>
    public void SetPlayerSeat(int playerIndex, Vector2Int newSeat)
    {
        if (playerIndex < 0 || playerIndex >= MAX_PLAYERS)
        {
            Debug.LogWarning($"Invalid player index: {playerIndex}");
            return;
        }
        _playerSeat[playerIndex] = newSeat;
    }

    /// <summary>
    /// 특정 Player 객체의 지정 자리 변경
    /// </summary>
    /// <param name="player">Player 객체</param>
    /// <param name="newSeat">새로운 지정 자리 좌표</param>
    public void SetPlayerSeat(Player player, Vector2Int newSeat)
    {
        int index = GetIndex(player);
        if (index == -1)
        {
            Debug.LogWarning("Player not found in team");
            return;
        }

        SetPlayerSeat(index, newSeat);
    }

    /// <summary>
    /// Player를 자신의 지정 자리로 이동시키기
    /// </summary>
    /// <param name="playerIndex">Player의 인덱스</param>
    /// <param name="immediate">즉시 이동 여부</param>
    public void MovePlayerToSeat(int playerIndex, bool immediate = false)
    {
        if (playerIndex < 0 || playerIndex >= PlayerCount || _players[playerIndex] == null)
        {
            Debug.LogWarning($"Cannot move player {playerIndex} to seat: invalid index or null player");
            return;
        }

        Vector2Int seatPosition = GetPlayerSeat(playerIndex);
        Player player = _players[playerIndex];

        Debug.Log($"Moving player {playerIndex} to seat position {seatPosition}");
        MapManager.Instance.MoveTo(player, seatPosition, immediate);
    }

    /// <summary>
    /// 특정 Player 객체를 자신의 지정 자리로 이동시키기
    /// </summary>
    /// <param name="player">Player 객체</param>
    /// <param name="immediate">즉시 이동 여부</param>
    public void MovePlayerToSeat(Player player, bool immediate = false)
    {
        int index = GetIndex(player);
        if (index == -1)
        {
            Debug.LogWarning("Player not found in team");
            return;
        }

        MovePlayerToSeat(index, immediate);
    }

    /// <summary>
    /// 모든 Player들을 각자의 지정 자리로 이동시키기
    /// </summary>
    /// <param name="immediate">즉시 이동 여부</param>
    public void MoveAllPlayersToSeats()
    {
        for (int i = 0; i < PlayerCount; i++)
        {
            if (_players[i] != null)
            {
                _players[i].MoveToSeat(GetPlayerSeat(i));//이동이 완료되면 그때부터 Progress 진행하도록 수정할 것.
            }
        }
        
        Debug.Log("All players moved to their designated seats");
    }

    #endregion
}
