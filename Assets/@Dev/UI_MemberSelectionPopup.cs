using UnityEngine;
using static Define;

public class UI_MemberSelectionPopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
        //BG
        BG,
    }
    enum Buttons
    {
        //MainTitle
        NextButton,
        PreviousButton,

        //SubBottom
        OkayButton,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //SubMiddle
        SubMiddleNameText,
        SubMiddleSalaryNameText,
        SubMiddleSalaryText,

        //LeftContent
        RoleText,
        StateNameText,
        StateText,

        //RightContent
        AbilityNameText1,
        AbilityNameText2,
        AbilityNameText3,
        AbilityNameText4,
        AbilityNameText5,

        AbilityScoreText1,
        AbilityScoreText2,
        AbilityScoreText3,
        AbilityScoreText4,
        AbilityScoreText5,

        //SubBottom
        OkayButtonText,
    }
    enum Images
    {
        //BG
        BG,

        //LeftContent
        MemberImage,
    }
    private int currentIndex;
    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        GetButton((int)Buttons.NextButton).onClick.AddListener(() => NextMemberInfoUpdate());
        GetButton((int)Buttons.PreviousButton).onClick.AddListener(() => PreviousMemberInfoUpdate());
    }
    public void SetInfo(Player member)
    {
        UpdateContent(member);
    }
    public void UpdateContent(Player member)
    {
        MemberData memberData = member.CurrentMemberData;
        
        if (memberData == null)
        {
            Debug.LogWarning("MemberData is null!");
            return;
        }

        // 이름 표시
        GetText((int)Texts.SubMiddleNameText).text = memberData.Name;

        // 급여 표시
        GetText((int)Texts.SubMiddleSalaryNameText).text = "식비";
        GetText((int)Texts.SubMiddleSalaryText).text = $"{memberData.Salary}개";

        // 역할 표시
        GetText((int)Texts.RoleText).text = memberData.RoleToString(memberData.Role);

        // 상태 표시
        GetText((int)Texts.StateNameText).text = "상태";
        GetText((int)Texts.StateText).text = memberData.StateToString(memberData.State);

        // 능력치 이름 설정
        GetText((int)Texts.AbilityNameText1).text = memberData.AbilityToString(EAbilityType.Programming);
        GetText((int)Texts.AbilityNameText2).text = memberData.AbilityToString(EAbilityType.Scenario);
        GetText((int)Texts.AbilityNameText3).text = memberData.AbilityToString(EAbilityType.Graphics);
        GetText((int)Texts.AbilityNameText4).text = memberData.AbilityToString(EAbilityType.Sound);
        GetText((int)Texts.AbilityNameText5).text = memberData.AbilityToString(EAbilityType.Power);

        // 능력치 점수 설정
        GetText((int)Texts.AbilityScoreText1).text = memberData.Programming.ToString();
        GetText((int)Texts.AbilityScoreText2).text = memberData.Scenario.ToString();
        GetText((int)Texts.AbilityScoreText3).text = memberData.Graphics.ToString();
        GetText((int)Texts.AbilityScoreText4).text = memberData.Sound.ToString();
        GetText((int)Texts.AbilityScoreText5).text = memberData.Power.ToString();

        // 멤버 이미지 설정 (상태에 따라 normal/zombie 이미지 선택)
        string imagePath = memberData.zombieImagePath;
        
        if (!string.IsNullOrEmpty(imagePath))
        {
            Sprite memberSprite = ResourceManager.Instance.Get<Sprite>(imagePath);
            if (memberSprite != null)
            {
                GetImage((int)Images.MemberImage).sprite = memberSprite;
            }
        }

        // 확인 버튼 텍스트
        GetText((int)Texts.OkayButtonText).text = "교육";
        currentIndex = MemberManager.Instance.GetIndex(member) != -1 ? MemberManager.Instance.GetIndex(member) : 0;
        int order = MemberManager.Instance.GetIndex(member) + 1;
        GetText((int)Texts.MainTitleText).text = $"구성원 선택 {order}/{MemberManager.Instance.MemberCount}";
    }
    public void NextMemberInfoUpdate()
    {
        int memberCount = MemberManager.Instance.MemberCount;
        int nextIndex = (currentIndex + memberCount + 1) % memberCount;
        UpdateContent(MemberManager.Instance.GetMember(nextIndex));
    }
    public void PreviousMemberInfoUpdate()
    {
        int memberCount = MemberManager.Instance.MemberCount;
        int prevIndex = (currentIndex + memberCount - 1) % memberCount;
        UpdateContent(MemberManager.Instance.GetMember(prevIndex));
    }
    public override void RefreshUI()
    {
        base.RefreshUI();

        //TODO : Localization
    }
}
