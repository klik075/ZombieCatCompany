using System.Collections.Generic;
using UnityEngine;

public class MemberManager : Singleton<MemberManager>
{
    private const int MAX_MEMBERS = 4;
    private Player[] _members = new Player[MAX_MEMBERS];

    // 주인공(첫 번째 구성원)은 해고 불가
    public Player MainCharacter => _members[0];

    // 현재 구성원 수
    public int MemberCount { get; private set; } = 0;

    // 게임 시작 시 주인공 초기화
    public void InitializeMainCharacter()
    {
        if (_members[0] != null)
        {
            Debug.LogWarning("Main character already exists!");
            return;
        }

        // 주인공 스폰
        Player mainCharacter = ObjectManager.Instance.SpawnPlayer("Cat");
        mainCharacter.SetMemberData(100);
        _members[0] = mainCharacter;
        MemberCount = 1;

        // 초기 위치 설정
        Vector2Int startCell = new Vector2Int(0, 1);
        MapManager.Instance.MoveTo(mainCharacter, startCell, true);

        Debug.Log("Main character initialized");
    }

    // 구성원 고용 (빈 자리에 추가)
    public bool HireMember(out int memberIndex)
    {
        Random.InitState(System.DateTime.Now.Millisecond + MemberCount);
        memberIndex = -1;

        // 팀이 가득 찬 경우
        if (MemberCount >= MAX_MEMBERS)
        {
            Debug.LogWarning("Cannot hire more members. Team is full!");
            return false;
        }

        // 현재 MemberCount 인덱스에 새 구성원 추가
        Player newMember = ObjectManager.Instance.SpawnPlayer("Cat");
        newMember.SetMemberData(Random.Range(101,104));
        _members[MemberCount] = newMember;
        memberIndex = MemberCount;
        MemberCount++;

        // 초기 위치 설정 (주인공 근처)
        Vector2Int spawnCell = FindSpawnPosition();
        MapManager.Instance.MoveTo(newMember, spawnCell, true);

        Debug.Log($"Member hired at index {memberIndex}");
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
            return walkableCells[Random.Range(0, walkableCells.Count)];
        }

        // 최후의 수단
        return new Vector2Int(0, 2);
    }

    //// 모든 구성원 정리
    //public void Clear()
    //{
    //    for (int i = 0; i < MemberCount; i++)
    //    {
    //        if (_members[i] != null)
    //        {
    //            MapManager.Instance.UnregisterCat(_members[i].CellPosition);
    //            ObjectManager.Instance.Despawn(_members[i]);
    //            _members[i] = null;
    //        }
    //    }
    //    MemberCount = 0;
    //}
}
