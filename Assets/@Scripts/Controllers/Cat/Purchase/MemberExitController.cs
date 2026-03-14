using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Member 퇴장 관리 (door 도착 시 Disable)
/// </summary>
public class MemberExitController
{
    private List<Member> _members;
    private List<Member> _exitingMembers;
    private Vector2Int _exitPosition;
    private Action _onAllMembersExited;
    private int _exitedCount = 0;

    public MemberExitController(List<Member> members, Vector2Int exitPosition)
    {
        _members = members;
        _exitPosition = exitPosition;
    }

    /// <summary>
    /// 모든 Member를 door로 이동 시작
    /// </summary>
    public void StartExit(Action onAllMembersExited)
    {
        _onAllMembersExited = onAllMembersExited;
        _exitedCount = 0;

        _exitingMembers = _members.Where(m => m != null && !m.IsDispatched).ToList();

        if (_exitingMembers.Count == 0)
        {
            Debug.Log("[MemberExitController] No members to exit (all dispatched or null)");
            _onAllMembersExited?.Invoke();
            return;
        }

        foreach (Member member in _exitingMembers)
        {
            // SeatMovement로 door 이동
            var doorMovement = MovementPoolManager.Instance.Get<SeatMovement>()
                .Initialize(_exitPosition, new GridManagerAdapter());

            // 각 Member의 도착 이벤트 구독
            doorMovement.OnArrived += () => OnMemberArrived(member);

            member.AIEnabled = false; // AI 비활성화
            member.SetMovementStrategy(doorMovement);
        }
    }

    /// <summary>
    /// Member가 door에 도착했을 때
    /// </summary>
    private void OnMemberArrived(Member member)
    {
        // Member Disable 처리
        member.gameObject.SetActive(false);
        _exitedCount++;

        Debug.Log($"Member exited: {_exitedCount}/{_members.Count}");

        // 모두 퇴장했으면 콜백 호출
        if (_exitedCount >= _exitingMembers.Count)
        {
            _onAllMembersExited?.Invoke();
        }
    }

    /// <summary>
    /// 정리
    /// </summary>
    public void Cancel()
    {
        _members = null;
        _exitingMembers = null;
        _onAllMembersExited = null;
    }
}
