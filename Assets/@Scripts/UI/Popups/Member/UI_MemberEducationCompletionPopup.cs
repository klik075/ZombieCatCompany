using System;
using UnityEngine;
using static Define;

public class UI_MemberEducationCompletionPopup : UI_UGUI, IUI_Popup, IClickableUI
{
    enum GameObjects
    {
        //BG - 상호작용 x
        BG,
    }
    enum Buttons
    {
        //SubBottom
        OkayButton,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //SubMiddle
        SubMiddleNameText,

        //RightContent
        AbilityNameText1,
        AbilityNameText2,
        AbilityNameText3,
        AbilityNameText4,
        AbilityNameText5,

        AbilityCurrentScoreText1,
        AbilityCurrentScoreText2,
        AbilityCurrentScoreText3,
        AbilityCurrentScoreText4,
        AbilityCurrentScoreText5,

        AbilityUpgradeScoreText1,
        AbilityUpgradeScoreText2,
        AbilityUpgradeScoreText3,
        AbilityUpgradeScoreText4,
        AbilityUpgradeScoreText5,

        //SubBottom
        OkayButtonText,
    }
    enum Images
    {
        //LeftContent
        MemberImage,

        //RightContent
        AbilityIcon1,
        AbilityIcon2,
        AbilityIcon3,
        AbilityIcon4,
        AbilityIcon5,
    }
    
    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        GetButton((int)Buttons.OkayButton).onClick.AddListener(OnOkayButtonClicked);
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        SetInfo();
    }

    public void SetInfo()
    {
        ApplyEducation();
        UpdateContent();
    }

    public void UpdateContent()
    {
        Player selectedPlayer = MemberManager.Instance.SelectedPlayer;
        EducationData educationData = EducationManager.Instance.SelectedEducation;

        if (selectedPlayer?.CurrentMemberData == null || educationData == null)
        {
            Debug.LogWarning("Target member or education data is null!");
            return;
        }

        MemberData memberData = selectedPlayer.CurrentMemberData;

        // 제목 설정
        GetText((int)Texts.MainTitleText).text = "@교육 완료";
        
        // 멤버 이름 설정
        GetText((int)Texts.SubMiddleNameText).text = $"@{memberData.Name}";

        // 능력치 이름 설정
        GetText((int)Texts.AbilityNameText1).text = MemberData.AbilityToString(EAbilityType.Programming);
        GetText((int)Texts.AbilityNameText2).text = MemberData.AbilityToString(EAbilityType.Scenario);
        GetText((int)Texts.AbilityNameText3).text = MemberData.AbilityToString(EAbilityType.Graphics);
        GetText((int)Texts.AbilityNameText4).text = MemberData.AbilityToString(EAbilityType.Sound);
        GetText((int)Texts.AbilityNameText5).text = MemberData.AbilityToString(EAbilityType.Power);

        // 현재 능력치 점수 설정 (교육 적용 후)
        GetText((int)Texts.AbilityCurrentScoreText1).text = memberData.Programming.ToString();
        GetText((int)Texts.AbilityCurrentScoreText2).text = memberData.Scenario.ToString();
        GetText((int)Texts.AbilityCurrentScoreText3).text = memberData.Graphics.ToString();
        GetText((int)Texts.AbilityCurrentScoreText4).text = memberData.Sound.ToString();
        GetText((int)Texts.AbilityCurrentScoreText5).text = memberData.Power.ToString();

        // 상승한 능력치 점수 설정 (+ 표시)
        GetText((int)Texts.AbilityUpgradeScoreText1).text = GetUpgradeText(educationData.Programming);
        GetText((int)Texts.AbilityUpgradeScoreText2).text = GetUpgradeText(educationData.Scenario);
        GetText((int)Texts.AbilityUpgradeScoreText3).text = GetUpgradeText(educationData.Graphics);
        GetText((int)Texts.AbilityUpgradeScoreText4).text = GetUpgradeText(educationData.Sound);
        GetText((int)Texts.AbilityUpgradeScoreText5).text = GetUpgradeText(educationData.Power);

        // 멤버 이미지 설정
        SetMemberImage(memberData);

        // 확인 버튼 텍스트 설정
        GetText((int)Texts.OkayButtonText).text = "확인";
    }

    private string GetUpgradeText(int upgradeAmount)
    {
        if (upgradeAmount > 0)
        {
            return $"+{upgradeAmount}";
        }
        else if (upgradeAmount < 0)
        {
            return $"-{upgradeAmount}";
        }
        else
        {
            return "0";
        }
    }

    private void SetMemberImage(MemberData memberData)
    {
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
    }

    private void OnOkayButtonClicked()
    {
        UIManager.Instance.ClosePopupUI();
    }

    private void ApplyEducation()
    {
        Player targetMember = MemberManager.Instance.SelectedPlayer;
        EducationData educationData = EducationManager.Instance.SelectedEducation;

        if (targetMember?.CurrentMemberData == null || educationData == null)
        {
            Debug.LogWarning("Cannot apply education: target member is null!");
            return;
        }

        // 골드 차감
        GameManager.Instance.Gold -= educationData.Cost;

        // 능력치 향상 적용
        MemberData memberData = targetMember.CurrentMemberData;
        memberData.Programming += educationData.Programming;
        memberData.Scenario += educationData.Scenario;
        memberData.Graphics += educationData.Graphics;
        memberData.Sound += educationData.Sound;
        memberData.Power += educationData.Power;

        EventManager.Instance.TriggerEvent(EEventType.EducationCompleted);
        Debug.Log($"{memberData.Name}이(가) {educationData.Name} 교육을 받았습니다!");
    }
    
    public override void RefreshUI()
    {
        base.RefreshUI();
        UpdateContent();
    }
}
