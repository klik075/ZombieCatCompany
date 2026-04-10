using System;
using UnityEngine;
using static Define;

public class UI_FenceEnhancePopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
    }
    enum Buttons
    {
        //SubBottom
        EnhanceButton,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //SubMiddle
        CurrentAbilityNameText,
        NextAbilityNameText,

        //LeftContent
        CurrentAbilityNameText1,
        CurrentAbilityNameText2,
        CurrentAbilityNameText3,
        CurrentAbilityNameText4,
        CurrentAbilityNameText5,

        CurrentAbilityScoreText1,
        CurrentAbilityScoreText2,
        CurrentAbilityScoreText3,
        CurrentAbilityScoreText4,
        CurrentAbilityScoreText5,

        //RightContent
        NextAbilityNameText1,
        NextAbilityNameText2,
        NextAbilityNameText3,
        NextAbilityNameText4,
        NextAbilityNameText5,

        NextAbilityScoreText1,
        NextAbilityScoreText2,
        NextAbilityScoreText3,
        NextAbilityScoreText4,
        NextAbilityScoreText5,

        //SubBottom
        SuccessNameText,
        SuccessText,

        EnhanceCostNameText,
        EnhanceCostText,

        EnhanceButtonText,
    }

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));

        GetButton((int)Buttons.EnhanceButton).onClick.AddListener(() => { PlayButtonClickSound(); OnClickEnhanceButton(); });
    }

    protected override void Start()
    {
        base.Start();
    }
    protected override void OnEnable()
    {
        base.OnEnable();

        UpdateUI();
    }
    protected override void OnDisable()
    {
        base.OnDisable();

    }
    private void UpdateUI()
    {
        UpdateContent();
        UpdateButton();
    }
    private void UpdateContent()
    {
        // 현재 펜스 정보 가져오기
        Fence currentFence = FenceManager.Instance.CurrentFence;
        if (currentFence == null)
        {
            Debug.LogWarning("Current fence is null");
            return;
        }

        // 다음 레벨 데이터 가져오기
        int nextLevel = currentFence.EnhanceLevel + 1;
        bool hasNextLevel = DataManager.Instance.FenceDict.TryGetValue(nextLevel, out FenceData nextData);

        // 메인 제목
        GetText((int)Texts.MainTitleText).text = "펜스 강화";

        // 서브 타이틀
        GetText((int)Texts.CurrentAbilityNameText).text = "현재 능력치";
        GetText((int)Texts.NextAbilityNameText).text = "강화 시 능력치";

        if (hasNextLevel)
        {
            // 현재 능력치
            GetText((int)Texts.CurrentAbilityNameText1).text = "최대 체력";
            GetText((int)Texts.CurrentAbilityScoreText1).text = $"{currentFence.MaxHp}";

            GetText((int)Texts.CurrentAbilityNameText2).text = "현재 체력";
            GetText((int)Texts.CurrentAbilityScoreText2).text = $"{currentFence.CurrentHp}";

            GetText((int)Texts.CurrentAbilityNameText3).text = "방어력";
            GetText((int)Texts.CurrentAbilityScoreText3).text = $"{currentFence.Defense}";

            GetText((int)Texts.CurrentAbilityNameText4).text = "내구도";
            GetText((int)Texts.CurrentAbilityScoreText4).text = $"{currentFence.CurrentDurability}";

            GetText((int)Texts.CurrentAbilityNameText5).text = "데미지";
            GetText((int)Texts.CurrentAbilityScoreText5).text = $"{currentFence.Damage}";

            // 다음 레벨 능력치
            GetText((int)Texts.NextAbilityNameText1).text = "최대 체력";
            GetText((int)Texts.NextAbilityScoreText1).text = $"{nextData.MaxHp}";

            GetText((int)Texts.NextAbilityNameText2).text = "다음 체력";
            GetText((int)Texts.NextAbilityScoreText2).text = $"{nextData.MaxHp}";

            GetText((int)Texts.NextAbilityNameText3).text = "방어력";
            GetText((int)Texts.NextAbilityScoreText3).text = $"{nextData.Defense}";

            GetText((int)Texts.NextAbilityNameText4).text = "내구도";
            GetText((int)Texts.NextAbilityScoreText4).text = $"{nextData.Durability}";

            GetText((int)Texts.NextAbilityNameText5).text = "데미지";
            GetText((int)Texts.NextAbilityScoreText5).text = $"{nextData.Damage}";

            // 성공 확률
            GetText((int)Texts.SuccessNameText).text = "성공 확률";
            GetText((int)Texts.SuccessText).text = FenceData.ProbabilityToStringt(currentFence.EnhanceProbability);

            // 강화 비용
            GetText((int)Texts.EnhanceCostNameText).text = "강화 비용";
            GetText((int)Texts.EnhanceCostText).text = FenceData.CostToString(currentFence.EnhanceCost);

            // 강화 버튼
            GetText((int)Texts.EnhanceButtonText).text = "강화";
            
        }
    }
    private void UpdateButton()
    {
        GetButton((int)Buttons.EnhanceButton).interactable = FenceManager.Instance.CanEnhanceFence();
    }
    private void OnClickEnhanceButton()
    {
        if (!FenceManager.Instance.CanEnhanceFence())
            return;

        isTransitioning = true;
        UIManager.Instance.ClosePopupUI();

        Fence fence = FenceManager.Instance.CurrentFence;
        UI_MessagePopup messagePopup = UIManager.Instance.ShowPopupUI<UI_MessagePopup>();
        messagePopup.SetInfo(
            MessageManager.Instance.GetMessageScript(Define.EMessageType.TryEnhanceFence).Contents,
            new string[] { FenceData.ProbabilityToStringt(fence.EnhanceProbability), FenceData.CostToString(fence.EnhanceCost) },
            OnClickOkayButton,
            OnClickNoButton
        );
    }
    private void OnClickOkayButton()
    {
        bool success = FenceManager.Instance.TryEnhanceFence();

        UIManager.Instance.ClosePopupUI();

        UI_ChatPopup chatPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();

         if (success)
        {
            chatPopup.SetInfo(
                MemberManager.MAIN_CHARACTER_ID,
                MessageManager.Instance.GetMessageScript(Define.EMessageType.EnhanceSuccess).Contents,
                null,
                ShowFenceStateUI
                );
        }
        else
        {
            chatPopup.SetInfo(MemberManager.MAIN_CHARACTER_ID,
                MessageManager.Instance.GetMessageScript(Define.EMessageType.EnhanceFail).Contents,
                null,
                ShowFenceStateUI
                );
        }

    }
    private void OnClickNoButton()
    {
        UIManager.Instance.ClosePopupUI();
        UIManager.Instance.ShowPopupUI<UI_FenceEnhancePopup>();
    }
    private void ShowFenceStateUI()
    {
        UI_FenceStatePopup statePopup = UIManager.Instance.ShowPopupUI<UI_FenceStatePopup>();
    }
    private void PlayButtonClickSound()
    {
        SoundManager.Instance.Play2D(ESound.Effect, "Button");
    }
    public override void RefreshUI()
    {
        base.RefreshUI();

        UpdateUI();
    }
}
