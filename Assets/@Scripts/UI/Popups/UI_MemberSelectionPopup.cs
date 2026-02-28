using System;
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

    private EMemberSelectionType _selectionType;

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        GetButton((int)Buttons.NextButton).onClick.AddListener(() => MemberManager.Instance.SelectNextMember());
        GetButton((int)Buttons.PreviousButton).onClick.AddListener(() => MemberManager.Instance.SelectPreviousMember());
        GetButton((int)Buttons.OkayButton).onClick.AddListener(() => OnOkayButtonClicked());
    }
    protected override void OnEnable()
    {
        base.OnEnable();

        EventManager.Instance.AddEvent(EEventType.SelectedMemberChanged, UpdateContent);
        EventManager.Instance.AddEvent(EEventType.EducationCompleted, UpdateContent);
    }
    protected override void OnDisable()
    {
        base.OnDisable();

        EventManager.Instance.RemoveEvent(EEventType.SelectedMemberChanged, UpdateContent);
        EventManager.Instance.RemoveEvent(EEventType.EducationCompleted, UpdateContent);
    }
    public void SetInfo(EMemberSelectionType selectionType, int index = 0)
    {
        _selectionType = selectionType;
        MemberManager.Instance.SelectMemberByIndex(index);//현재 선택된 멤버 설정
    }

    public void UpdateContent()
    {
        Member selectedPlayer = MemberManager.Instance.SelectedMember;
        
        if (selectedPlayer == null || selectedPlayer?.CurrentMemberData == null)
        {
            Debug.LogWarning("No selected member or MemberData is null!");
            return;
        }

        MemberData memberData = selectedPlayer.CurrentMemberData;

        // 제목 표시 (현재 선택된 멤버의 순서)
        int currentOrder = MemberManager.Instance.SelectedMemberIndex + 1;
        int totalMembers = MemberManager.Instance.MemberCount;
        string mainTitlePrefix = _selectionType == EMemberSelectionType.Education ? "@구성원 선택" : "@파견 선택";
        GetText((int)Texts.MainTitleText).text = $"{mainTitlePrefix} {currentOrder}/{totalMembers}";

        // 이름 표시
        GetText((int)Texts.SubMiddleNameText).text = memberData.Name;

        // 급여 표시
        GetText((int)Texts.SubMiddleSalaryNameText).text = "@식비";
        GetText((int)Texts.SubMiddleSalaryText).text = $"{memberData.SalaryToString(ESalaryType.Food)}";

        // 역할 표시
        GetText((int)Texts.RoleText).text = MemberData.RoleToString(memberData.Role);

        // 상태 표시
        GetText((int)Texts.StateNameText).text = "@상태";
        GetText((int)Texts.StateText).text = MemberData.StateToString(memberData.State);

        // 능력치 이름 설정
        GetText((int)Texts.AbilityNameText1).text = MemberData.AbilityToString(EAbilityType.Programming);
        GetText((int)Texts.AbilityNameText2).text = MemberData.AbilityToString(EAbilityType.Scenario);
        GetText((int)Texts.AbilityNameText3).text = MemberData.AbilityToString(EAbilityType.Graphics);
        GetText((int)Texts.AbilityNameText4).text = MemberData.AbilityToString(EAbilityType.Sound);
        GetText((int)Texts.AbilityNameText5).text = MemberData.AbilityToString(EAbilityType.Power);

        // 능력치 점수 설정
        GetText((int)Texts.AbilityScoreText1).text = memberData.Programming.ToString();
        GetText((int)Texts.AbilityScoreText2).text = memberData.Scenario.ToString();
        GetText((int)Texts.AbilityScoreText3).text = memberData.Graphics.ToString();
        GetText((int)Texts.AbilityScoreText4).text = memberData.Sound.ToString();
        GetText((int)Texts.AbilityScoreText5).text = memberData.Power.ToString();

        // 멤버 이미지 설정 (상태에 따라 normal/zombie 이미지 선택)
        string imagePath = memberData.ZombieImagePath;
        
        if (!string.IsNullOrEmpty(imagePath))
        {
            Sprite memberSprite = ResourceManager.Instance.Get<Sprite>(imagePath);
            if (memberSprite != null)
            {
                GetImage((int)Images.MemberImage).sprite = memberSprite;
            }
        }

        // 확인 버튼 텍스트
        GetText((int)Texts.OkayButtonText).text = _selectionType == EMemberSelectionType.Education ? "@교육" : "@파견";
    }

    private void OnOkayButtonClicked()
    {
        switch(_selectionType)
        {
            case EMemberSelectionType.Education:
                HandleEducation();
                break;
            case EMemberSelectionType.Dispatch:
                HandleDispatch();
                break;
            default:
                Debug.LogWarning("Unknown member selection type.");
                break;
        }
    }

    private void HandleEducation()
    {
        // 교육 가능 여부 체크
        if (!EducationManager.Instance.CanReceiveEducation())
        {
            Debug.LogWarning($"현재 교육을 받을 수 없는 상태입니다.");
            return;
        }

        UI_MemberEducationMethodsPopup educationPopup = UIManager.Instance.ShowPopupUI<UI_MemberEducationMethodsPopup>();
    }

    private void HandleDispatch()
    {

    }

    public override void RefreshUI()
    {
        base.RefreshUI();
        UpdateContent();
        //TODO : Localization
    }
}
