using System;
using System.Collections.Generic;
using UnityEngine;
using static Define;

[Serializable]
public class MemberData
{
    public int EmployeeID;
    public string NormalImagePath;
    public string ZombieImagePath;
    public string Name;
    public ERoleType Role;
    public int Salary;
    public EMemberStateType State;
    public int Programming;
    public int Scenario;
    public int Graphics;
    public int Sound;
    public int Power;

    public string RoleToString(ERoleType eRoleType)
    {
        switch(eRoleType)
        {
            case ERoleType.Boss:
                return "사장";
            case ERoleType.Planner:
                return "기획자";
            case ERoleType.Designer:
                return "디자이너";
            case ERoleType.SoundWriter:
                return "사운드 작가";
            case ERoleType.Merchant:
                return "상인";
            default:
                return "누구냐 넌";
        }
    }
    public string AbilityToString(EAbilityType eAbilityType)
    {
        switch(eAbilityType)
        {
            case EAbilityType.Programming:
                return "프로그래밍";
            case EAbilityType.Scenario:
                return "시나리오";
            case EAbilityType.Graphics:
                return "그래픽";
            case EAbilityType.Sound:
                return "사운드";
            case EAbilityType.Power:
                return "전투력";
            default:
                return "누구냐 넌";
        }
    }
    public string StateToString(EMemberStateType eMemberState)
    {
        switch(eMemberState)
        {
            case EMemberStateType.Full:
                return "배부름";
            case EMemberStateType.Hunger1:
            case EMemberStateType.Hunger2:
                return "배고픔";
            case EMemberStateType.Starvation:
                return "굶주림";
            case EMemberStateType.Soon:
                return "곧꼬닥";
            default:
                return "누구냐 넌";
        }
    }

    // 깊은 복사 메서드
    public MemberData DeepCopy()
    {
        return new MemberData
        {
            EmployeeID = this.EmployeeID,
            NormalImagePath = this.NormalImagePath,
            ZombieImagePath = this.ZombieImagePath,
            Name = this.Name,
            Role = this.Role,
            Salary = this.Salary,
            State = this.State,
            Programming = this.Programming,
            Scenario = this.Scenario,
            Graphics = this.Graphics,
            Sound = this.Sound,
            Power = this.Power
        };
    }
}
[Serializable]
public class MemberDataLoader : IDataLoader<int, MemberData>
{
    public List<MemberData> members = new List<MemberData>();

    public Dictionary<int, MemberData> MakeDict()
    {
        Dictionary<int, MemberData> dict = new Dictionary<int, MemberData>();
        foreach (var member in members)
            dict.Add(member.EmployeeID, member);

        return dict;
    }

    public bool Validate()
    {
        return true;
    }
}