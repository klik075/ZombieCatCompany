using System;
using System.Collections.Generic;
using System.Text;
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

    public int GetAbilityValue(EAbilityType eAbilityType)
    {
        switch(eAbilityType)
        {
            case EAbilityType.Programming:
                return Programming;
            case EAbilityType.Scenario:
                return Scenario;
            case EAbilityType.Graphics:
                return Graphics;
            case EAbilityType.Sound:
                return Sound;
            case EAbilityType.Power:
                return Power;
            default:
                return 0;
        }
    }
    public string SalaryToString(ESalaryType eSalaryType)
    {
        switch(eSalaryType)
        {
            case ESalaryType.Food:
                return $"{SalaryToValue(eSalaryType)}개";
            case ESalaryType.Salary:
            case ESalaryType.Deposit:
                return $"{SalaryToValue(eSalaryType)}G";
            default:
                return "";
        }
    }
    public int SalaryToValue(ESalaryType eSalaryType)
    {
        switch(eSalaryType)
        {
            case ESalaryType.Food:
                return Salary;
            case ESalaryType.Salary:
                return (Salary * 100);
            case ESalaryType.Deposit:
                return ((int)(Salary * 100 * 1.3));
            default:
                return 0;
        }
    }
    public static string RoleToString(ERoleType eRoleType)
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
    public static string AbilityToString(EAbilityType eAbilityType)
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
    public static string StateToString(EMemberStateType eMemberState)
    {
        return StateToString(eMemberState, false);
    }
    public static string StateToString(EMemberStateType eMemberState, bool isDispatch)
    {
        StringBuilder sb = new StringBuilder();

        switch (eMemberState)
        {
            case EMemberStateType.Full:
                sb.Append("배부름");
                break;
            case EMemberStateType.Hunger1:
            case EMemberStateType.Hunger2:
                sb.Append("배고픔");
                break;
            case EMemberStateType.Starvation:
                sb.Append("굶주림");
                break;
            case EMemberStateType.Soon:
                sb.Append("곧꼬닥");
                break;
            case EMemberStateType.Death:
                sb.Append("사망");
                break;
            default:
                sb.Append("누구냐 넌");
                break;
        }

        if (isDispatch)
        {
            sb.Append("(파견 중)");
        }

        return sb.ToString();
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
