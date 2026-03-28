using System;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class UI_GameDevMemberSelectionPopup : UI_UGUI, IUI_Popup, IClickableUI
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
        //LeftContent
        MemberImage,
    }
    
    private int _currentMemberIndex = 0;

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        // 버튼 이벤트 등록
        GetButton((int)Buttons.NextButton).onClick.AddListener(OnClickNextMember);
        GetButton((int)Buttons.PreviousButton).onClick.AddListener(OnClickPreviousMember);
        GetButton((int)Buttons.OkayButton).onClick.AddListener(OnClickOkayButton);
    }
    
    public void SetInfo()
    {
        _currentMemberIndex = 0;
        ValidateCurrentIndex();
        UpdateContent();
    }

    private void ValidateCurrentIndex()
    {
        EGameDevType currentDevType = GameDevManager.Instance.CurrentGameDevType;
        int suitableMemberCount = MemberManager.Instance.GetSuitableMemberCount(currentDevType);
        
        if (_currentMemberIndex >= suitableMemberCount)
        {
            _currentMemberIndex = Mathf.Max(0, suitableMemberCount - 1);
        }
    }
    
    public void UpdateContent()
    {
        UpdateTitleText();
        
        EGameDevType currentDevType = GameDevManager.Instance.CurrentGameDevType;
        int suitableMemberCount = MemberManager.Instance.GetSuitableMemberCount(currentDevType);
        
        if (suitableMemberCount == 0)
            return;
        
        Member currentMember = MemberManager.Instance.GetSuitableMemberByIndex(currentDevType, _currentMemberIndex);
        if (currentMember != null)
        {
            UpdateMemberInfo(currentMember);
            UpdateNavigationButtons(suitableMemberCount);
            UpdateOkayButton(currentMember);
        }
    }

    private void UpdateTitleText()
    {
        EGameDevType currentDevType = GameDevManager.Instance.CurrentGameDevType;
        string stageText = GetGameDevStageText(currentDevType);
        GetText((int)Texts.MainTitleText).text = $"@{stageText} 담당할 고양이";
    }

    private string GetGameDevStageText(EGameDevType devType)
    {
        switch (devType)
        {
            case EGameDevType.Scenario:     
                return "기획을";
            case EGameDevType.Graphics:     
                return "원화를";
            case EGameDevType.Sound:        
                return "사운드를";
            default:                        
                return "너는 누구냐!";
        }
    }

    private void UpdateMemberInfo(Member member)
    {
        MemberData memberData = member.CurrentMemberData;

        Member dispatchMember = MemberManager.Instance.GetDispatchMember();
        bool isDispatchMember = dispatchMember != null && dispatchMember == member;

        // 기본 정보 업데이트
        GetText((int)Texts.SubMiddleNameText).text = memberData.Name;
        GetText((int)Texts.RoleText).text = MemberData.RoleToString(memberData.Role);
        
        // 상태 정보 업데이트
        GetText((int)Texts.StateNameText).text = "@상태";
        GetText((int)Texts.StateText).text = MemberData.StateToString(memberData.State, isDispatchMember);
        
        // 능력치 정보 업데이트
        UpdateAbilityTexts(memberData);
        
        // 멤버 이미지 업데이트
        UpdateMemberImage(memberData);
    }

    private void UpdateAbilityTexts(MemberData memberData)
    {
        GetText((int)Texts.AbilityNameText1).text = MemberData.AbilityToString(EAbilityType.Programming);
        GetText((int)Texts.AbilityNameText2).text = MemberData.AbilityToString(EAbilityType.Scenario);
        GetText((int)Texts.AbilityNameText3).text = MemberData.AbilityToString(EAbilityType.Graphics);
        GetText((int)Texts.AbilityNameText4).text = MemberData.AbilityToString(EAbilityType.Sound);
        GetText((int)Texts.AbilityNameText5).text = MemberData.AbilityToString(EAbilityType.Power);

        GetText((int)Texts.AbilityScoreText1).text = memberData.Programming.ToString();
        GetText((int)Texts.AbilityScoreText2).text = memberData.Scenario.ToString();
        GetText((int)Texts.AbilityScoreText3).text = memberData.Graphics.ToString();
        GetText((int)Texts.AbilityScoreText4).text = memberData.Sound.ToString();
        GetText((int)Texts.AbilityScoreText5).text = memberData.Power.ToString();
    }

    private void UpdateMemberImage(MemberData memberData)
    {
        string imagePath = memberData.ZombieImagePath;
        
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
    }

    private void UpdateNavigationButtons(int suitableMemberCount)
    {
        GetButton((int)Buttons.PreviousButton).interactable = _currentMemberIndex > 0;
        GetButton((int)Buttons.NextButton).interactable = _currentMemberIndex < suitableMemberCount - 1;
    }

    private void UpdateOkayButton(Member member)
    {
        bool isDispatchMember = member.IsDispatched;
        GetButton((int)Buttons.OkayButton).interactable = !isDispatchMember;
        GetText((int)Texts.OkayButtonText).text = "@결정";
    }

    private void OnClickNextMember()
    {
        EGameDevType currentDevType = GameDevManager.Instance.CurrentGameDevType;
        int suitableMemberCount = MemberManager.Instance.GetSuitableMemberCount(currentDevType);
        
        if (_currentMemberIndex < suitableMemberCount - 1)
        {
            _currentMemberIndex++;
            UpdateContent();
        }
    }

    private void OnClickPreviousMember()
    {
        if (_currentMemberIndex > 0)
        {
            _currentMemberIndex--;
            UpdateContent();
        }
    }

    private void OnClickOkayButton()
    {
        EGameDevType currentDevType = GameDevManager.Instance.CurrentGameDevType;
        Member selectedMember = MemberManager.Instance.GetSuitableMemberByIndex(currentDevType, _currentMemberIndex);
        
        if (selectedMember == null)
        {
            Debug.LogWarning("No member selected!");
            return;
        }

        if (selectedMember.IsDispatched)
        {
            Debug.LogWarning("Cannot select dispatched member!");
            return;
        }

        MemberManager.Instance.SelectMember(selectedMember);
        Debug.Log($"Selected member for {currentDevType}: {selectedMember.CurrentMemberData.Name}");
        
        UIManager.Instance.ClosePopupUI();
        UI_GameDevWorkPopup workPopup = UIManager.Instance.ShowPopupUI<UI_GameDevWorkPopup>();
        workPopup.StartOfWork();
    }
    
    public override void RefreshUI()
    {
        base.RefreshUI();
        ValidateCurrentIndex();
        UpdateContent();
    }
}
