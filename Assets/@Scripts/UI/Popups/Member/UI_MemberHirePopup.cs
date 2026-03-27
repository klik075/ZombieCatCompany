using JetBrains.Annotations;
using System;
using UnityEngine;
using static Define;

public class UI_MemberHirePopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
        //BG - 상호작용 x
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
        PaymentNameText,
        PaymentText,

        //RightContent
        AbilityNameText1,
        AbilityNameText2,
        AbilityNameText3,
        AbilityNameText4,

        AbilityScoreText1,
        AbilityScoreText2,
        AbilityScoreText3,
        AbilityScoreText4,

        //SubBottom
        OkayButtonText,
    }
    enum Images
    {
        //LeftContent
        MemberImage,
    }
    private int _currentIndex = 0;
    
    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        // 버튼 이벤트 등록
        GetButton((int)Buttons.NextButton).onClick.AddListener(() => NextMemberInfoUpdate());
        GetButton((int)Buttons.PreviousButton).onClick.AddListener(() => PreviousMemberInfoUpdate());
        GetButton((int)Buttons.OkayButton).onClick.AddListener(() => OnOkayButtonClicked());
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        EventManager.Instance.AddEvent(EEventType.MemberSwapped, CheckUpdate);

        SetInfo();
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        EventManager.Instance.RemoveEvent(EEventType.MemberSwapped, CheckUpdate);
    }
    public void SetInfo()
    {
        UpdateContent(0);
    }
    
    public void UpdateContent(int index)
    {
        HireResult hireResult = MemberManager.Instance.CurrentHireResult;

        if (hireResult == null)
        {
            Debug.LogWarning("HireResult is null!");
            return;
        }

        if (hireResult.MemberDatas == null || hireResult.MemberDatas.Count == 0)
        {
            Debug.LogWarning("No member data available!");
            return;
        }

        if (index < 0 || index >= hireResult.MemberDatas.Count)
        {
            Debug.LogWarning($"Invalid index: {index}");
            return;
        }

        MemberManager.Instance.SelectedHireMember(index);
        MemberData memberData = MemberManager.Instance.SelectedHireMemberData;


        if (memberData == null)
        {
            Debug.LogWarning($"MemberData at index {index} is null!");
            return;
        }

        _currentIndex = index;

        // 타이틀 텍스트 (현재 멤버 / 전체 멤버 수)
        GetText((int)Texts.MainTitleText).text = $"@신규 고용 {_currentIndex + 1}/{hireResult.MemberDatas.Count}";

        // 이름 표시
        GetText((int)Texts.SubMiddleNameText).text = memberData.Name;

        // 급여 표시
        GetText((int)Texts.SubMiddleSalaryNameText).text = "@연봉";
        GetText((int)Texts.SubMiddleSalaryText).text = $"{memberData.SalaryToString(ESalaryType.Salary)}";

        // 역할 표시
        GetText((int)Texts.RoleText).text = MemberData.RoleToString(memberData.Role);

        // 지불 금액 표시
        GetText((int)Texts.PaymentNameText).text = "@계약금";
        GetText((int)Texts.PaymentText).text = $"{memberData.SalaryToString(ESalaryType.Deposit)}";

        // 능력치 이름 설정
        GetText((int)Texts.AbilityNameText1).text = MemberData.AbilityToString(EAbilityType.Programming);
        GetText((int)Texts.AbilityNameText2).text = MemberData.AbilityToString(EAbilityType.Scenario);
        GetText((int)Texts.AbilityNameText3).text = MemberData.AbilityToString(EAbilityType.Graphics);
        GetText((int)Texts.AbilityNameText4).text = MemberData.AbilityToString(EAbilityType.Sound);

        // 능력치 점수 설정
        GetText((int)Texts.AbilityScoreText1).text = memberData.Programming.ToString();
        GetText((int)Texts.AbilityScoreText2).text = memberData.Scenario.ToString();
        GetText((int)Texts.AbilityScoreText3).text = memberData.Graphics.ToString();
        GetText((int)Texts.AbilityScoreText4).text = memberData.Sound.ToString();

        // 멤버 이미지 설정
        string imagePath = memberData.NormalImagePath;
        
        if (!string.IsNullOrEmpty(imagePath))
        {
            Sprite memberSprite = ResourceManager.Instance.Get<Sprite>(imagePath);
            if (memberSprite != null)
            {
                GetImage((int)Images.MemberImage).sprite = memberSprite;
            }
            else
            {
                Debug.LogWarning($"Failed to load sprite at path: {imagePath}");
            }
        }

        // 확인 버튼 텍스트
        GetText((int)Texts.OkayButtonText).text = "@고용";
    }
    
    private void NextMemberInfoUpdate()
    {
        HireResult hireResult = MemberManager.Instance.CurrentHireResult;

        if (hireResult == null || hireResult.MemberDatas == null || hireResult.MemberDatas.Count == 0)
            return;
        
        int nextIndex = (_currentIndex + 1) % hireResult.MemberDatas.Count;
        UpdateContent(nextIndex);
    }
    
    private void PreviousMemberInfoUpdate()
    {
        HireResult hireResult = MemberManager.Instance.CurrentHireResult;

        if (hireResult == null || hireResult.MemberDatas == null || hireResult.MemberDatas.Count == 0)
            return;
        
        int prevIndex = (_currentIndex - 1 + hireResult.MemberDatas.Count) % hireResult.MemberDatas.Count;
        UpdateContent(prevIndex);
    }
    
    private void OnOkayButtonClicked()
    {
        MemberData selectedHireMember = MemberManager.Instance.SelectedHireMemberData;

        if (selectedHireMember == null)
        {
            Debug.LogWarning("Cannot hire: Selected member is null!");
            return;
        }

        // 자금이 충분한지 확인
        int hireCost = selectedHireMember.SalaryToValue(ESalaryType.Deposit);
        if (GameManager.Instance.Gold < hireCost)
        {
            //자금 부족 팝업
            UI_ChatPopup chatPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
            chatPopup.SetInfo(MemberManager.MAIN_CHARACTER_ID, MessageManager.Instance.GetMessageScript(EMessageType.MoneyLow).Contents);
            return;
        }

        // 팀이 가득 찼는지 확인
        if (MemberManager.Instance.IsTeamFull())
        {
            //멤버 꽉참 팝업
            UI_ChatPopup chatPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
            chatPopup.SetInfo(MemberManager.MAIN_CHARACTER_ID, MessageManager.Instance.GetMessageScript(EMessageType.MembersFull).Contents, action : OnClickMembersFullChatPopup);
            return;
        }

        // 실제 멤버 고용
        if (MemberManager.Instance.HireMember(selectedHireMember))
        {
            //고용 팝업
            UI_ChatPopup chatPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
            chatPopup.SetInfo(MemberManager.MAIN_CHARACTER_ID, MessageManager.Instance.GetMessageScript(EMessageType.MemberHired).Contents, new string[] { selectedHireMember.Name }, action : OnClickMemberHiredChatPopup);
        }
        else
        {
            Debug.LogError("Failed to hire member!");
        }
    }
    public void OnClickMembersFullChatPopup()
    {
        UI_MemberFirePopup memberFirePopup = UIManager.Instance.ShowPopupUI<UI_MemberFirePopup>();
        memberFirePopup.SetInfo(EFireType.Swap);
    }
    public void OnClickMemberHiredChatPopup()
    {
        CheckUpdate();
    }
    public void CheckUpdate()
    {
        HireResult hireResult = MemberManager.Instance.CurrentHireResult;
        if (hireResult.MemberDatas.Count > 0)
            SetInfo();
        else
        {
            MemberManager.Instance.ActivateHiredMembersAI();

            MemberManager.Instance.EndHire();
            UIManager.Instance.ClosePopupUI();
        }
    }
    public override void RefreshUI()
    {
        base.RefreshUI();
        SetInfo();
        // TODO: Localization
    }
}
