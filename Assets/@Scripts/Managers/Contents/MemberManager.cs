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
        Member mainCharacter = ObjectManager.Instance.SpawnMember("CatBlackZombie");
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
        Member newPlayer = ObjectManager.Instance.SpawnMember(GetMemberPrefabName(memberData.EmployeeID));
        MemberData newMemberData = InfectMemberData(memberData);
        newPlayer.SetMemberData(newMemberData);
        _members[MemberCount] = newPlayer;
        MemberCount++;

        // 개선 1: AI 비활성화 (자동 이동 방지)
        newPlayer.AIEnabled = false;

        // 개선 2: DoorWay 근처에 배치
        Vector2Int spawnPosition = MapManager.Instance.FindNearPosition(DoorWay);
        MapManager.Instance.MoveTo(newPlayer, spawnPosition, true);

        // 개선 3: Idle 상태로 고정
        newPlayer.SetStateIdle();

        EndingManager.Instance.RecordStat(Define.EEndingStatType.HiredMembers, 1);

        Debug.Log($"[MemberManager] Hired {newMemberData.Name} at {spawnPosition} (AI disabled)");
        return true;
    }

    /// <summary>
    /// 고용된 모든 멤버의 AI 활성화 (고용 프로세스 완료 후 호출)
    /// </summary>
    public void ActivateHiredMembersAI()
    {
        int activatedCount = 0;
        
        for (int i = 0; i < MemberCount; i++)
        {
            Member member = GetMember(i);
            
            if (member != null && !member.AIEnabled)
            {
                member.AIEnabled = true;
                activatedCount++;
                Debug.Log($"[MemberManager] Activated AI for {member.CurrentMemberData?.Name}");
            }
        }
        
        if (activatedCount > 0)
        {
            Debug.Log($"[MemberManager] Activated AI for {activatedCount} newly hired members");
        }
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

        //MemberData memberData = _members[memberIndex].CurrentMemberData;

        //if (memberData == null)
        //    return false;

        if(ignoringStatus == true)
            return true;

        if (_members[memberIndex].IsDispatched)
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
    /// <summary>
    /// 멤버의 인덱스 가져오기
    /// </summary>
    public int GetIndex(Member member)
    {
        for (int i = 0; i < MAX_MEMBERS; i++)
        {
            if (_members[i] == member)
            {
                return i;
            }
        }
        
        return -1;
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

        int actualMemberCount = 0;

        for (int i = 0; i < saveDatas.Count && i < MAX_MEMBERS; i++)
        {
            MemberSaveData saveData = saveDatas[i];
            if(saveData == null)
                continue;

            Member member = ObjectManager.Instance.SpawnMember(GetMemberPrefabName(saveData.CurrentMemberData.EmployeeID));
            _members[i] = member;

            if (saveData.IsDispatched)
            {
                Debug.Log($"[MemberManager] Skipping dispatched member at index {i} (MorningScene)");
                member.LoadFromSaveDataNoCellpos(saveData);
                member.IsDispatched = true;
                member.AIEnabled = false;
                member.gameObject.SetActive(false);
                actualMemberCount++;
                continue;
            }

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
                member.SetStateIdle(); // State 직접 설정 대신 내부 메서드 사용
            }

            actualMemberCount++;
        }

        MemberCount = actualMemberCount;

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
    public List<Member> GetAllMembers(bool excludeDispatched = false)
    {
        List<Member> members = new List<Member>();
        for (int i = 0; i < MemberCount; i++)
        {
            if (_members[i] != null)
            {
                // 파견 중인 멤버 제외 옵션
                if (excludeDispatched && _members[i].IsDispatched)
                    continue;

                members.Add(_members[i]);
            }
        }
        return members;
    }
    public List<Member> GetActiveMembers()
    {
        return GetAllMembers(excludeDispatched: true);
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

    #region 파견 시스템
    /// <summary>
    /// 현재 선택된 멤버를 파견 보내기
    /// </summary>
    public bool DispatchSelectedMember()
    {
        return DispatchMember(SelectedMemberIndex);
    }

    /// <summary>
    /// 특정 멤버를 파견 보내기
    /// </summary>
    public bool DispatchMember(int memberIndex)
    {
        if (!CanDispatchMember(memberIndex))
        {
            Debug.LogWarning($"[MemberManager] Cannot dispatch member at index {memberIndex}");
            return false;
        }

        Member member = GetMember(memberIndex);

        // 파견 상태로 설정
        member.IsDispatched = true;

        MapManager.Instance.UnregisterCat(member.CellPosition);
        member.gameObject.SetActive(false);

        Debug.Log($"[MemberManager] Member {member.CurrentMemberData.Name} dispatched");
        return true;
    }
    /// <summary>
    /// 파견 중인 멤버 찾아서 복귀시키기
    /// </summary>
    public bool CompleteDispatch()
    {
        for (int i = 0; i < MemberCount; i++)
        {
            Member member = GetMember(i);
            if (member != null && member.IsDispatched)
            {
                member.IsDispatched = false;

                member.gameObject.SetActive(true);

                bool isNight = SceneManager.Instance.CurrentSceneType == EScene.NightScene;
                MemberSeatInfo seatInfo = GetMemberSeatInfo(i, isNight);

                member.MoveToSeat(seatInfo.SeatPosition);

                Debug.Log($"[MemberManager] Member {member.CurrentMemberData.Name} returned from dispatch");
                return true;
            }
        }

        Debug.LogWarning("[MemberManager] No dispatched member found");
        return false;
    }
    /// <summary>
    /// 파견 가능 여부 체크
    /// </summary>
    public bool CanDispatchMember()
    {
        return CanDispatchMember(SelectedMemberIndex);
    }
    public bool CanDispatchMember(int memberIndex)
    {
        // 주인공(보스)는 파견 불가
        if (memberIndex == 0)
            return false;

        // 유효성 검사
        if (memberIndex < 0 || memberIndex >= MemberCount)
            return false;

        Member member = GetMember(memberIndex);
        if (member == null)
            return false;

        // 이미 파견 중이면 불가
        if (member.IsDispatched)
            return false;

        // 이미 다른 멤버가 파견 중이면 불가 (한 번에 한 명만)
        if (HasDispatchedMember())
            return false;

        return true;
    }
    /// <summary>
    /// 파견 중인 멤버가 있는지 확인
    /// </summary>
    public bool HasDispatchedMember()
    {
        bool hasDispatched = GetDispatchMember() != null;
        return hasDispatched;
    }
    public Member GetDispatchMember()
    {
        for (int i = 0; i < MemberCount; i++)
        {
            Member member = GetMember(i);
            if (member != null && member.IsDispatched)
            {
                return member;
            }
        }
        return null;
    }
    #endregion

    #region 좌석 관리

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
    public int HowManyMemberSitting(bool excludeDispatched = false)
    {
        int count = 0;
        for (int i = 0; i < MemberCount; i++)
        {
            if (_members[i] != null)
            {
                if (excludeDispatched && _members[i].IsDispatched)
                    continue;

                Vector2Int seatPos = GetMemberSeat(i);
                if (_members[i].CellPosition == seatPos)
                    count++;
            }
        }
        return count;
    }
    public int HowManyActiveMemberSitting()
    {
        return HowManyMemberSitting(excludeDispatched: true);
    }
    public bool IsAnyMemberAtSeat()
    {
        return HowManyActiveMemberSitting() > 0;
    }
   
    /// <summary>
    /// 모든 멤버를 지정된 씬의 자리로 초기화
    /// </summary>
    /// <param name="isNight">NightScene 자리인지 여부</param>
    public void ResetMembersToSeats(bool isNight)
    {
        List<Member> members = GetAllMembers();
        
        Debug.Log($"[MemberManager] Resetting {members.Count} members to {(isNight ? "NightScene" : "MorningScene")} seats...");
        
        foreach (var member in members)
        {
            if (member == null)
                continue;
            
            // 멤버의 인덱스 가져오기
            int memberIndex = GetIndex(member);
            
            if (memberIndex == -1)
            {
                Debug.LogWarning($"[MemberManager] Member {member.name} not found in member array");
                continue;
            }
            
            // 멤버의 씬별 자리 정보 가져오기
            MemberSeatInfo seatInfo = GetMemberSeatInfo(memberIndex, isNight);
            
            // CellPosition을 자리로 설정
            member.CellPosition = seatInfo.SeatPosition;
            
            // 이동 전략 초기화
            member.ClearMovementStrategy();
            
            // 공격 중지
            member.StopAttack();
            
            // 상태를 Idle로 변경
            member.SetStateForced(Cat.ECatState.Idle);
        }
        
        Debug.Log($"[MemberManager] All members reset to their seats");
    }
    #endregion

    #region 배고픔 시스템

    /// <summary>
    /// 모든 멤버의 배고픔 상태를 1단계 증가시키고, 사망한 멤버를 처리
    /// </summary>
    /// <returns>사망한 멤버 수</returns>
    public List<string> IncreaseAllMembersHunger()
    {
        List<string> deathNames = new List<string>();

        // 뒤에서부터 순회 (해고 시 인덱스 변경 방지)
        for (int i = MemberCount - 1; i >= 0; i--)
        {
            Member member = GetMember(i);

            if (member == null || member.CurrentMemberData == null)
                continue;

            MemberData memberData = member.CurrentMemberData;
            EMemberStateType currentState = memberData.State;

            // 이미 Death 상태면 스킵
            if (currentState == EMemberStateType.Death)
                continue;

            // 배고픔 상태 1단계 증가
            EMemberStateType nextState = GetNextHungerState(currentState);
            memberData.State = nextState;

            Debug.Log($"[MemberManager] {memberData.Name}: {currentState} → {nextState}");

            // Death 상태가 되면 해고
            if (nextState == EMemberStateType.Death)
            {
                if (memberData.Role == ERoleType.Boss)
                {
                    EndingManager.Instance.TriggerEnding(EEndingType.Starvation);
                    return deathNames;
                }

                string memberName = memberData.Name;
                Debug.Log($"[MemberManager] {memberName} died from starvation. Firing member...");

                // ignoringStatus = true로 상태 무시하고 강제 해고
                bool fired = FireMember(i, ignoringStatus: true);

                if (fired)
                {
                    deathNames.Add(memberName);
                }
            }
        }

        EndingManager.Instance.RecordStat(EEndingStatType.DeadMembers, deathNames.Count);

        return deathNames;
    }
    public void IncreaseHungerState(int memberIndex, int stages = 1)//이벤트 용
    {
        Member member = GetMember(memberIndex);

        if (member == null || member.CurrentMemberData == null)
        {
            Debug.LogWarning($"[MemberManager] Cannot increase hunger: Invalid member at index {memberIndex}");
            return;
        }

        MemberData memberData = member.CurrentMemberData;
        EMemberStateType newState = memberData.State;

        if (memberData.State == EMemberStateType.Soon)
            return;

        // stages만큼 배고픔 증가
        for (int i = 0; i < stages; i++)
        {
            newState = GetNextHungerState(newState);
        }

        memberData.State = newState;

        // Death 상태가 되면 엔딩 처리
        if (newState == EMemberStateType.Death)
        {
            memberData.State = EMemberStateType.Soon; 
        }
    }
    /// <summary>
    /// 특정 멤버의 배고픔 상태를 감소시킴 (식량 배급 시 사용)
    /// </summary>
    public void DecreaseHungerState(int memberIndex, int stages = 2)
    {
        Member member = GetMember(memberIndex);

        if (member == null || member.CurrentMemberData == null)
            return;

        MemberData memberData = member.CurrentMemberData;
        EMemberStateType currentState = memberData.State;

        // stages만큼 배고픔 감소
        for (int i = 0; i < stages; i++)
        {
            currentState = GetPreviousHungerState(currentState);
        }

        memberData.State = currentState;
        Debug.Log($"[MemberManager] {memberData.Name}: Hunger decreased to {currentState}");
    }

    /// <summary>
    /// 다음 배고픔 상태 반환
    /// </summary>
    private EMemberStateType GetNextHungerState(EMemberStateType currentState)
    {
        return currentState switch
        {
            EMemberStateType.Full => EMemberStateType.Hunger1,
            EMemberStateType.Hunger1 => EMemberStateType.Hunger2,
            EMemberStateType.Hunger2 => EMemberStateType.Starvation,
            EMemberStateType.Starvation => EMemberStateType.Soon,
            EMemberStateType.Soon => EMemberStateType.Death,
            EMemberStateType.Death => EMemberStateType.Death, // 이미 죽음
            _ => EMemberStateType.Hunger1
        };
    }

    /// <summary>
    /// 이전 배고픔 상태 반환 (식량 배급 시)
    /// </summary>
    private EMemberStateType GetPreviousHungerState(EMemberStateType currentState)
    {
        return currentState switch
        {
            EMemberStateType.Death => EMemberStateType.Soon,
            EMemberStateType.Soon => EMemberStateType.Starvation,
            EMemberStateType.Starvation => EMemberStateType.Hunger2,
            EMemberStateType.Hunger2 => EMemberStateType.Hunger1,
            EMemberStateType.Hunger1 => EMemberStateType.Full,
            EMemberStateType.Full => EMemberStateType.Full, // 이미 배부름
            _ => EMemberStateType.Full
        };
    }

    #endregion

    #region 이동, 공격 정지
    /// <summary>
    /// 모든 멤버의 이동 및 공격 즉시 정지 (긴급 상황용 - 엔딩, 씬 전환 등)
    /// </summary>
    public void StopAllMembersImmediately()
    {
        Debug.Log("[MemberManager] Stopping all members immediately...");

        for (int i = 0; i < MemberCount; i++)
        {
            Member member = GetMember(i);

            if (member != null)
            {
                // 공격 전략 제거 (코루틴 정지)
                member.StopAttack();

                // 이동 전략 제거
                member.ClearMovementStrategy();

                // AI 비활성화
                member.AIEnabled = false;

                // 상태 강제 변경
                member.SetStateForced(Cat.ECatState.Idle);
            }
        }

        Debug.Log("[MemberManager] All members stopped");
    }
    #endregion
}
