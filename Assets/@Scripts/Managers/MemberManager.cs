using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

[System.Serializable]
public class HireResult
{
    public List<MemberData> MemberDatas;//고용된 멤버 데이터들
    public string message;
    public HireMethodType HireMethod;//고용 방법
}
public class MemberManager : Singleton<MemberManager>
{
    public const int MAIN_CHARACTER_ID = 100;
    private const int MAX_MEMBERS = 4;
    private Player[] _members = new Player[MAX_MEMBERS];

    // 주인공(첫 번째 구성원)은 해고 불가
    public Player MainCharacter => _members[0];
    

    // 현재 구성원 수
    public int MemberCount { get; private set; } = 0;

    // 게임 시작 시 주인공 초기화
    public void InitBoss()
    {
        if (_members[0] != null)
        {
            Debug.LogWarning("Main character already exists!");
            return;
        }

        // 주인공 스폰
        Player mainCharacter = ObjectManager.Instance.SpawnPlayer("Cat");
        mainCharacter.SetMemberData(MAIN_CHARACTER_ID);
        _members[0] = mainCharacter;
        MemberCount = 1;

        // 초기 위치 설정
        Vector2Int startCell = new Vector2Int(0, 1);
        MapManager.Instance.MoveTo(mainCharacter, startCell, true);

        Debug.Log("Main character initialized");
    }

    //고용 종류에 따라 랜덤한 직원 리스트 생성 후 매개 변수로 받은 액션에게 Result 전달 Invoke
    public HireResult GenerateHireResult(HireMethodType hireMethodType)
    {
        //hireMethodType 마다 다른 로직

        HireResult result = new HireResult
        {
            HireMethod = hireMethodType
        };

        result.MemberDatas = GenerateMemberDatas();
        result.message = $"모집했던 결과가 나왔다냥.\r\n고양이 {result.MemberDatas.Count}마리가 지원했다냥.\r\n누구를 채용할까냥?";

        return result;
    }
    public IEnumerator StartHiringProcess(HireMethodType hireMethodType, Action<HireResult> onComplete)
    {
        // 모집 중 상태로 변경
        GameManager.Instance.IsRecruiting = true;

        // 대기 시간
        float waitTime = GetHireWaitTime(hireMethodType);
        yield return new WaitForSeconds(waitTime);

        // 채용 결과 생성
        HireResult result = GenerateHireResult(hireMethodType);

        // 모집 완료
        GameManager.Instance.IsRecruiting = false;

        // 콜백 호출
        onComplete?.Invoke(result);
    }
    private float GetHireWaitTime(HireMethodType hireMethodType)
    {
        switch (hireMethodType)
        {
            case HireMethodType.Internet:
                return 1f; // 추후에 변경
            default:
                return 1f;
        }
    }
    public List<MemberData> GenerateMemberDatas(int count = 4)
    {
        List<MemberData> memberDatas = new List<MemberData>();
        // 101~103 사이의 랜덤한 직원 ID 생성
        for (int i = 0; i < count; i++)
        {
            int randomID = UnityEngine.Random.Range(101, 104);
            MemberData memberData = new MemberData();
            DataManager.Instance.MemberDict.TryGetValue(randomID, out memberData);
            memberDatas.Add(memberData);
        }
        
        return memberDatas;
    }
    public int GetAnnualIncome(MemberData memberData)
    {
        int salary = memberData.Salary * 100;

        return salary;
    }
    public int GetHireCost(MemberData memberData)
    {
        int randomInt = UnityEngine.Random.Range(13, 16);
        float rate = randomInt / 10.0f;
        int hireCost = (int)(GetAnnualIncome(memberData) * rate);//나중에 100을 식량 코스트로 변경해야 함.

        return hireCost;
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

        // 현재 MemberCount 인덱스에 새 구성원 추가
        Player newMember = ObjectManager.Instance.SpawnPlayer("Cat");
        newMember.SetMemberData(memberData);
        _members[MemberCount] = newMember;
        MemberCount++;

        // 초기 위치 설정 (주인공 근처)
        Vector2Int spawnCell = FindSpawnPosition();
        MapManager.Instance.MoveTo(newMember, spawnCell, true);

        return true;
    }

    // 구성원 해고
    public bool FireMember(int memberIndex)
    {
        // 주인공은 해고 불가
        if (memberIndex == 0)
        {
            Debug.LogWarning("Cannot fire the main character!");
            return false;
        }

        // 유효성 검사
        if (memberIndex < 0 || memberIndex >= MemberCount)
        {
            Debug.LogWarning($"Invalid member index: {memberIndex}");
            return false;
        }

        if (_members[memberIndex] == null)
        {
            Debug.LogWarning($"No member at index {memberIndex}");
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
    public Player GetMember(int memberIndex)
    {
        if (memberIndex < 0 || memberIndex >= MemberCount)
            return null;
        return _members[memberIndex];
    }

    // Player 객체로 인덱스 찾기
    public int GetIndex(Player member)
    {
        if (member == null)
            return -1;

        for (int i = 0; i < MemberCount; i++)
        {
            if (_members[i] == member)
                return i;
        }

        return -1; // 찾지 못한 경우
    }

    // 모든 구성원 저장 데이터 생성
    public List<PlayerSaveData> GetSaveData()
    {
        List<PlayerSaveData> saveDatas = new List<PlayerSaveData>();
        
        for (int i = 0; i < MemberCount; i++)
        {
            if (_members[i] != null)
            {
                saveDatas.Add(_members[i].GetSaveData());
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
        
        for (int i = 0; i < saveDatas.Count && i < MAX_MEMBERS; i++)
        {
            PlayerSaveData saveData = saveDatas[i];
            
            // Player 스폰
            Player player = ObjectManager.Instance.SpawnPlayer("Cat");
            
            // 저장된 데이터 로드
            player.LoadFromSaveData(saveData);
            
            // 멤버 배열에 추가
            _members[i] = player;
        }
        
        MemberCount = saveDatas.Count;
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
    }

    // 모든 구성원 가져오기 (null 제외)
    public List<Player> GetAllMembers()
    {
        List<Player> members = new List<Player>();
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

    // 스폰 위치 찾기 (주인공 근처 빈 공간)
    private Vector2Int FindSpawnPosition()
    {
        if (_members[0] == null)
            return new Vector2Int(0, 0);

        Vector2Int mainCharacterPos = _members[0].CellPosition;

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
