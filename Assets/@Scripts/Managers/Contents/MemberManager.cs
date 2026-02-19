using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Cat;
using static Define;

[System.Serializable]
public class HireResult
{
    public List<MemberData> MemberDatas;//고용된 멤버 데이터들
    public string[] Messages;
    public EHireMethodType HireMethod;//고용 방법

    public HireResult DeepCopy()
    {
        HireResult copy = new HireResult();
        copy.HireMethod = this.HireMethod;
        copy.Messages = (string[])this.Messages.Clone();
        copy.MemberDatas = new List<MemberData>();
        foreach (var memberData in this.MemberDatas)
        {
            copy.MemberDatas.Add(memberData.DeepCopy());
        }
        return copy;
    }
}
[System.Serializable]
public struct MemberSeatInfo
{
    public Vector2Int SeatPosition;
    public bool IsFlipped;
    public bool IsFacingForward;

    // 생성자 추가 (선택사항, 초기화 편의)
    public MemberSeatInfo(Vector2Int position, bool isFlipped = false, bool isFacingForward = true)
    {
        SeatPosition = position;
        IsFlipped = isFlipped;
        IsFacingForward = isFacingForward;
    }
}
public class MemberManager : Singleton<MemberManager>
{
    public const int MAIN_CHARACTER_ID = 100;
    public const int MAX_MEMBERS = 4;
    private Member[] _members = new Member[MAX_MEMBERS];
    public Vector2Int DoorWay = new Vector2Int(-3, -8);

    // Player들의 개인 지정 자리 (MapManager에서 사용하는 좌표)
    private MemberSeatInfo[] _memberNightSeat = new MemberSeatInfo[MAX_MEMBERS];
    private MemberSeatInfo[] _memberMorningSeat = new MemberSeatInfo[MAX_MEMBERS];
    // 선택된 플레이어 관리
    private int _selectedMemberIndex = 0;

    // 현재 선택된 플레이어의 인덱스
    public int SelectedMemberIndex
    {
        get { return _selectedMemberIndex; }
        private set 
        {
            _selectedMemberIndex = value;
            EventManager.Instance.TriggerEvent(EEventType.SelectedMemberChanged);
        }
    }
    // 현재 선택된 플레이어
    public Member SelectedMember
    {
        get { return GetMember(_selectedMemberIndex); }
    }
    // 주인공(첫 번째 구성원)은 해고 불가
    public Member MainCharacter => _members[0];

    private int memberCount = 0;
    // 현재 구성원 수
    public int MemberCount 
    { 
        get { return memberCount; }
        private set 
        {
            memberCount = value;
            EventManager.Instance.TriggerEvent(EEventType.MemberListChanged); 
        }
    }
    public HireResult CurrentHireResult { get; private set; }
    public MemberData SelectedHireMemberData { get; private set; }

    #region Member Selection Management

    /// <summary>
    /// 특정 멤버를 선택합니다
    /// </summary>
    public void SelectMember(Member member)
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
        if (index >= 0 && index < MemberCount && _members[index] != null)
        {
            SelectedMemberIndex = index;
            //OnSelectedMemberChanged?.Invoke(SelectedPlayer); 선택 트리거 발동
            Debug.Log($"Selected member: {SelectedMember.CurrentMemberData?.Name} (Index: {index})");
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
        if (MemberCount <= 1) return;
        
        int nextIndex = (SelectedMemberIndex + 1) % MemberCount;
        SelectMemberByIndex(nextIndex);
    }

    /// <summary>
    /// 이전 멤버를 선택합니다 (순환)
    /// </summary>
    public void SelectPreviousMember()
    {
        if (MemberCount <= 1) return;
        
        int prevIndex = (SelectedMemberIndex - 1 + MemberCount) % MemberCount;
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
        if (_members[0] != null)
        {
            Debug.LogWarning("Main character already exists!");
            return;
        }

        // Player 지정 자리 초기화
        InitializeMemberSeats();

        // 주인공 스폰
        Member mainCharacter = ObjectManager.Instance.SpawnPlayer("CatBlackZombie");
        mainCharacter.SetMemberData(MAIN_CHARACTER_ID);
        _members[0] = mainCharacter;
        MemberCount = 1;

        // 주인공을 기본 선택으로 설정
        SelectedMemberIndex = 0;

        // 주인공을 지정 자리로 이동
        MoveMemberToSeat(0, true);
    }

    //고용 종류에 따라 랜덤한 직원 리스트 생성 후 매개 변수로 받은 액션에게 Result 전달 Invoke
    public HireResult GenerateHireResult(EHireMethodType hireMethodType)
    {
        //hireMethodType 마다 다른 로직

        HireResult result = new HireResult();

        result.HireMethod = hireMethodType;
        result.MemberDatas = GenerateMemberDatas();
        result.Messages = new string[] { $"{result.MemberDatas.Count}" };

        return result;
    }
    public void StartHire(EHireMethodType hireMethodType, bool isLoad = false)
    {
        CoroutineManager.Instance.Run(CoHireProcess(hireMethodType, isLoad));
    }
    private IEnumerator CoHireProcess(EHireMethodType hireMethodType, bool isLoad)
    {
        yield return CoroutineManager.Instance.Run(MemberManager.Instance.CoStartHiringProcess(hireMethodType, isLoad));

        // 팝업이 모두 닫힐 때까지 대기
        while (UIManager.Instance.PopupCount > 0)
        {
            yield return null;
        }

        // 모집 완료 팝업
        UI_ChatPopup chatPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
        chatPopup.SetInfo(MemberManager.MAIN_CHARACTER_ID, MessageManager.Instance.GetMessageScript(EMessageType.CompleteRecruiting).Contents, MemberManager.Instance.CurrentHireResult.Messages, action: OpenMemberHirePopup);

        // 모집 완료
        GameManager.Instance.IsRecruiting = false;
    }
    public IEnumerator CoStartHiringProcess(EHireMethodType hireMethodType, bool isLoad)
    {
        HireResult temp = null;
        if (isLoad)
            temp = CurrentHireResult.DeepCopy();

        CurrentHireResult = null;
        GameManager.Instance.IsRecruiting = true;

        if(isLoad == false)
            CurrentHireResult = GenerateHireResult(hireMethodType);
        else
            CurrentHireResult = temp;

        // 대기 시간
        float waitTime = GetHireWaitTime(hireMethodType);
        yield return new WaitForSeconds(waitTime);
    }
    private void OpenMemberHirePopup()
    {
        UIManager.Instance.ShowPopupUI<UI_MemberHirePopup>();
    }
    private float GetHireWaitTime(EHireMethodType hireMethodType)
    {
        switch (hireMethodType)
        {
            case EHireMethodType.Internet:
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

        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond + MemberCount);

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
        if (MemberCount >= MAX_MEMBERS)
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
        Member newPlayer = ObjectManager.Instance.SpawnPlayer(GetMemberPrefabName(memberData.EmployeeID));
        MemberData newMemberData = InfectMemberData(memberData);
        newPlayer.SetMemberData(newMemberData);
        _members[MemberCount] = newPlayer;
        MemberCount++;
        MapManager.Instance.MoveTo(newPlayer, MapManager.Instance.FindNearPosition(DoorWay), true);

        return true;
    }
    public string GetMemberPrefabName(int employeeId)
    {
        string name = "";
        switch (employeeId)
        {
            case 100:
                name = "CatBlackZombie";
                break;
            case 101:
                name = "CatGrayZombie";
                break;
            case 102:
                name = "CatBrownZombie";
                break;
            case 103:
                name = "CatWhiteZombie";
                break;
            default:
                name = "CatBlackZombie";
                break;
        }
        return name;
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
        if (memberIndex < 0 || memberIndex >= MemberCount)
            return false;

        if (_members[memberIndex] == null)
            return false;

        MemberData memberData = _members[memberIndex].CurrentMemberData;

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
        bool success = FireMember(SelectedMemberIndex, ignoringStatus);

        if(success)
            SelectedMemberIndex--;

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
        MapManager.Instance.UnregisterCat(_members[memberIndex].CellPosition);

        // 구성원 제거
        ObjectManager.Instance.Despawn(_members[memberIndex]);
        _members[memberIndex] = null;

        // 뒤에 있는 구성원들을 앞으로 한 칸씩 이동
        ShiftMembersForward(memberIndex);
        MemberCount--;

        Debug.Log($"Member at index {memberIndex} fired and members shifted forward. Current count: {MemberCount}");
        return true;
    }

    // 구성원들을 앞으로 한 칸씩 이동
    private void ShiftMembersForward(int startIndex)
    {
        for (int i = startIndex; i < MemberCount - 1; i++)
        {
            _members[i] = _members[i + 1];
        }
        // 마지막 자리는 null로 설정
        _members[MemberCount - 1] = null;
    }

    // 특정 인덱스의 구성원 가져오기
    public Member GetMember(int memberIndex)
    {
        if (memberIndex < 0 || memberIndex >= MemberCount)
            return null;
        return _members[memberIndex];
    }

    // Player 객체로 인덱스 찾기
    public int GetIndex(Member player)
    {
        if (player == null)
            return -1;

        for (int i = 0; i < MemberCount; i++)
        {
            if (_members[i] == player)
                return i;
        }

        return -1; // 찾지 못한 경우
    }

    // 모든 구성원 저장 데이터 생성
    public List<MemberSaveData> GetPlayerSaveData()
    {
        List<MemberSaveData> saveDatas = new List<MemberSaveData>();
        
        for (int i = 0; i < MemberCount; i++)
        {
            if (_members[i] != null)
            {
                saveDatas.Add(_members[i].GetSaveData());
            }
        }
        
        return saveDatas;
    }
    public HireResult GetHireResult()
    {
        HireResult hireResult = CurrentHireResult.DeepCopy();
        return hireResult;
    }
    public void LoadHireResult(HireResult hireResult)
    {
        CurrentHireResult = hireResult.DeepCopy();
    }
    // 저장 데이터에서 구성원 복원
    public void LoadFromSaveData()
    {
        MapManager.Instance.InitForScene();

        // 기존 멤버 정리
        ClearAllMembers();
        InitializeMemberSeats();

        bool isNight = SceneManager.Instance.CurrentSceneType == EScene.NightScene ? true : false;

        GameData gameData = SaveManager.Instance.GetGameData();
        List<MemberSaveData> saveDatas = gameData.CompanyData.MemberSaveDatas;

        for (int i = 0; i < saveDatas.Count && i < MAX_MEMBERS; i++)
        {
            MemberSaveData saveData = saveDatas[i];
            if(saveData == null)
                continue;

            Member member = ObjectManager.Instance.SpawnPlayer(GetMemberPrefabName(saveData.CurrentMemberData.EmployeeID));

            _members[i] = member;

            // 저장된 데이터 로드
            if (isNight)
            {
                member.LoadFromSaveData(saveData);
                member.AIEnabled = true;

                //임시 - 자기 자리로 가기
                MemberSeatInfo seatInfo = GetMemberSeatInfo(i, isNight: true);
                member.MoveToSeat(seatInfo.SeatPosition);
            }
            else
            {
                member.LoadFromSaveDataNoCellpos(saveData);
                member.AIEnabled = false;

                MemberSeatInfo seatInfo = GetMemberSeatInfo(i, isNight: false);
                MapManager.Instance.MoveTo(member, seatInfo.SeatPosition, true);

                member.IsFlipped = seatInfo.IsFlipped;
                member.IsFacingForward = seatInfo.IsFacingForward;
                member.State = ECatState.Idle;
            }
        }
        
        MemberCount = saveDatas.Count;

        // 첫 번째 멤버를 기본 선택으로 설정
        SelectedMemberIndex = 0;
        
        Debug.Log($"Loaded {MemberCount} members from save data");
    }
    
    // 모든 구성원 정리
    public void ClearAllMembers()
    {
        for (int i = 0; i < MAX_MEMBERS; i++)
        {
            if (_members[i] != null)
            {
                MapManager.Instance.UnregisterCat(_members[i].CellPosition);
                ObjectManager.Instance.Despawn(_members[i]);
                _members[i] = null;
            }
        }
        MemberCount = 0;
        SelectedMemberIndex = 0;
    }

    // 모든 구성원 가져오기 (null 제외)
    public List<Member> GetAllMembers()
    {
        List<Member> members = new List<Member>();
        for (int i = 0; i < MemberCount; i++)
        {
            if (_members[i] != null)
                members.Add(_members[i]);
        }
        return members;
    }

    // 빈 슬롯 개수 가져오기
    public int GetEmptySlotCount()
    {
        return MAX_MEMBERS - MemberCount;
    }

    // 팀이 가득 찼는지 확인
    public bool IsTeamFull()
    {
        return MemberCount >= MAX_MEMBERS;
    }

    #region 게임 개발 관련 멤버 조회

    /// <summary>
    /// 특정 게임 개발 단계에 적합한 멤버들을 가져옵니다
    /// </summary>
    /// <param name="gameDevType">게임 개발 단계</param>
    /// <returns>적합한 멤버들의 리스트</returns>
    public List<Member> GetMembersForGameDevType(EGameDevType gameDevType)
    {
        List<Member> suitableMembers = new List<Member>();
        
        for (int i = 0; i < MemberCount; i++)
        {
            if (_members[i] != null && IsMemberSuitableForDevType(_members[i], gameDevType))
            {
                suitableMembers.Add(_members[i]);
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
        
        for (int i = 0; i < MemberCount; i++)
        {
            if (_members[i] != null && IsMemberSuitableForDevType(_members[i], gameDevType))
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
    public Member GetSuitableMemberByIndex(EGameDevType gameDevType, int index)
    {
        List<Member> suitableMembers = GetMembersForGameDevType(gameDevType);
        
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
    private bool IsMemberSuitableForDevType(Member member, EGameDevType gameDevType)
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

    // 기존 class를 struct로 변경
    

    /// <summary>
    /// Player들의 개인 지정 자리 초기화
    /// _players 배열의 순서대로 지정된 자리 할당
    /// </summary>
    private void InitializeMemberSeats()
    {
        _memberNightSeat[0] = new MemberSeatInfo(new Vector2Int(-3, 0), false, true);//사장 자리
        _memberNightSeat[1] = new MemberSeatInfo(new Vector2Int(0, 0), false, true);//직원 1
        _memberNightSeat[2] = new MemberSeatInfo(new Vector2Int(-4, -4), true, false);//직원 2
        _memberNightSeat[3] = new MemberSeatInfo(new Vector2Int(-1, -4), true, false);//직원 3

        _memberMorningSeat[0] = new MemberSeatInfo(new Vector2Int(-4, -2), false, true);//사장 자리
        _memberMorningSeat[1] = new MemberSeatInfo(new Vector2Int(-3, -2), false, true);//직원 1
        _memberMorningSeat[2] = new MemberSeatInfo(new Vector2Int(-2, -2), false, true);//직원 2
        _memberMorningSeat[3] = new MemberSeatInfo(new Vector2Int(-1, -2), false, true);//직원 3
    }

    public MemberSeatInfo GetMemberSeatInfo(Member member, bool isNight = true)
    {
        int index = GetIndex(member);
        return GetMemberSeatInfo(index, isNight);
    }
    public MemberSeatInfo GetMemberSeatInfo(int memberIndex, bool isNight = true)
    {
        if (memberIndex < 0 || memberIndex >= MAX_MEMBERS)
        {
            Debug.LogWarning($"Invalid player index: {memberIndex}");
            return default;
        }
        return isNight ? _memberNightSeat[memberIndex] : _memberMorningSeat[memberIndex];
    }
    /// <summary>
    /// 특정 Player의 지정 자리 가져오기
    /// </summary>
    /// <param name="memberIndex">Player의 인덱스</param>
    /// <returns>해당 Player의 지정 자리 좌표</returns>
    public Vector2Int GetMemberSeat(int memberIndex, bool isNight = true)
    {
        return GetMemberSeatInfo(memberIndex, isNight).SeatPosition;
    }

    /// <summary>
    /// 특정 Player 객체의 지정 자리 가져오기
    /// </summary>
    /// <param name="member">Player 객체</param>
    /// <returns>해당 Player의 지정 자리 좌표</returns>
    public Vector2Int GetMemberSeat(Member member, bool isNight = true)
    {
        int index = GetIndex(member);
        if (index == -1)
        {
            Debug.LogWarning("Player not found in team");
            return Vector2Int.zero;
        }

        return GetMemberSeat(index, isNight);
    }

    /// <summary>
    /// 특정 Player의 지정 자리 변경
    /// </summary>
    /// <param name="memberIndex">Player의 인덱스</param>
    /// <param name="newSeat">새로운 지정 자리 좌표</param>
    public void SetMemberSeat(int memberIndex, Vector2Int newSeat, bool isNight = true)
    {
        if (memberIndex < 0 || memberIndex >= MAX_MEMBERS)
        {
            Debug.LogWarning($"Invalid player index: {memberIndex}");
            return;
        }

        if (isNight)
        {
            _memberNightSeat[memberIndex].SeatPosition = newSeat;
        }
        else
        {
            _memberMorningSeat[memberIndex].SeatPosition = newSeat;
        }
    }

    /// <summary>
    /// 특정 Player 객체의 지정 자리 변경
    /// </summary>
    /// <param name="player">Player 객체</param>
    /// <param name="newSeat">새로운 지정 자리 좌표</param>
    public void SetMemberSeat(Member player, Vector2Int newSeat, bool isNight = true)
    {
        int index = GetIndex(player);
        if (index == -1)
        {
            Debug.LogWarning("Player not found in team");
            return;
        }

        SetMemberSeat(index, newSeat, isNight);
    }
    /// <summary>
    /// Player를 자신의 지정 자리로 이동시키기
    /// </summary>
    /// <param name="memberIndex">Player의 인덱스</param>
    /// <param name="immediate">즉시 이동 여부</param>
    public void MoveMemberToSeat(int memberIndex, bool immediate = false)
    {
        if (memberIndex < 0 || memberIndex >= MemberCount || _members[memberIndex] == null)
        {
            Debug.LogWarning($"Cannot move player {memberIndex} to seat: invalid index or null player");
            return;
        }

        Vector2Int seatPosition = GetMemberSeat(memberIndex);
        Member player = _members[memberIndex];

        Debug.Log($"Moving player {memberIndex} to seat position {seatPosition}");
        MapManager.Instance.MoveTo(player, seatPosition, immediate);
    }

    /// <summary>
    /// 특정 Player 객체를 자신의 지정 자리로 이동시키기
    /// </summary>
    /// <param name="member">Player 객체</param>
    /// <param name="immediate">즉시 이동 여부</param>
    public void MoveMemberToSeat(Member member, bool immediate = false)
    {
        int index = GetIndex(member);
        if (index == -1)
        {
            Debug.LogWarning("Player not found in team");
            return;
        }

        MoveMemberToSeat(index, immediate);
    }

    /// <summary>
    /// 모든 Player들을 각자의 지정 자리로 이동시키기
    /// </summary>
    /// <param name="immediate">즉시 이동 여부</param>
    public void MoveAllMembersToSeats()
    {
        for (int i = 0; i < MemberCount; i++)
        {
            if (_members[i] != null)
            {
                _members[i].MoveToSeat(GetMemberSeat(i));
            }
        }
        
        Debug.Log("All players moved to their designated seats");
    }
    public int HowManyMemberSitting()
    {
        int count = 0;
        for (int i = 0; i < MemberCount; i++)
        {
            if (_members[i] != null)
            {
                Vector2Int seatPos = GetMemberSeat(i);
                if (_members[i].CellPosition == seatPos)
                    count++;
            }
        }
        return count;
    }
    public bool IsAnyMemberAtSeat()
    {
        return HowManyMemberSitting() > 0;
    }
    #endregion
}
