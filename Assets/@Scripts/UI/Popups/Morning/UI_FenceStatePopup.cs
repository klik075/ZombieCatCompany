using System;
using UnityEngine;

public class UI_FenceStatePopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
        //BG
        BG,
    }
    enum Buttons
    {
        //SubBottom
        RepairButton,
        EnhanceButton,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //SubMiddle
        SubMiddleNameText,

        //LeftContent
        EnhanceNameText,
        EnhanceText,

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
        RepairButtonText,
        EnhanceButtonText,
    }
    enum Images
    {
        //LeftContent
        FenceImage,
    }

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        GetButton((int)Buttons.RepairButton).onClick.AddListener(() => OnClickRepairButton());
        GetButton((int)Buttons.EnhanceButton).onClick.AddListener(() => OnClickEnhanceButton());
    }
    protected override void Start()
    {
        base.Start();

    }
    protected override void OnEnable()
    {
        base.OnEnable();
        UpdateUI();

        EventManager.Instance.AddEvent(Define.EEventType.GoldChanged, UpdateUI);
    }
    protected override void OnDisable()
    {
        base.OnDisable();

        EventManager.Instance.RemoveEvent(Define.EEventType.GoldChanged, UpdateUI);
    }
    private void UpdateUI()
    {
        UpdateButton();
        UpdateContent();
    }
    private void UpdateContent()
    {
        // FenceManager에서 현재 울타리 가져오기
        Fence currentFence = FenceManager.Instance.CurrentFence;
        if (currentFence == null || currentFence.FenceData == null)
        {
            Debug.LogWarning("Current fence or fence data is null");
            return;
        }

        // 메인 제목 설정
        GetText((int)Texts.MainTitleText).text = "펜스 상태";

        // 펜스 이름 설정
        GetText((int)Texts.SubMiddleNameText).text = $"{GameManager.Instance.CompanyName}의 펜스";

        // 강화 정보
        GetText((int)Texts.EnhanceNameText).text = "강화";
        GetText((int)Texts.EnhanceText).text = FenceData.EnhanceToString(currentFence.EnhanceLevel);

        // 울타리 이미지 설정 (필요시)
        // GetImage((int)Images.FenceImage).sprite = ...;

        // 능력치 표시
        GetText((int)Texts.AbilityNameText1).text = "최대 체력";
        GetText((int)Texts.AbilityScoreText1).text = $"{currentFence.MaxHp}";

        GetText((int)Texts.AbilityNameText2).text = "현재 체력";
        GetText((int)Texts.AbilityScoreText2).text = $"{currentFence.CurrentHp}";

        GetText((int)Texts.AbilityNameText3).text = "방어력";
        GetText((int)Texts.AbilityScoreText3).text = $"{currentFence.Defense}";

        GetText((int)Texts.AbilityNameText4).text = "내구도";
        GetText((int)Texts.AbilityScoreText4).text = $"{currentFence.CurrentDurability}";

        GetText((int)Texts.AbilityNameText5).text = "데미지";
        GetText((int)Texts.AbilityScoreText5).text = $"{currentFence.Damage}";

        // 버튼 텍스트 설정
        GetText((int)Texts.RepairButtonText).text = "수리";
        GetText((int)Texts.EnhanceButtonText).text = "강화";
    }
    public void UpdateButton()
    {
        GetButton((int)Buttons.RepairButton).interactable = FenceManager.Instance.CanRepairFence();
        GetButton((int)Buttons.EnhanceButton).interactable = FenceManager.Instance.CanEnhanceFence();
    }
    private void OnClickRepairOkayButton()
    {
        bool success = FenceManager.Instance.RepairFence();

        if (!success)
        {
            Debug.Log("Repair Fail");
            return;
        }

        UI_ChatPopup chatPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
        chatPopup.SetInfo(
            MemberManager.MAIN_CHARACTER_ID,
            MessageManager.Instance.GetMessageScript(Define.EMessageType.RepairSuccess).Contents,
            null,
            OnClickRepairNoButton
        );
    }
    private void OnClickRepairNoButton()
    {
        UIManager.Instance.ShowPopupUI<UI_FenceStatePopup>();
    }
    private void OnClickRepairButton()
    {
        if (!FenceManager.Instance.CanRepairFence())
            return;

        UIManager.Instance.ClosePopupUI();

        Fence fence = FenceManager.Instance.CurrentFence;

        UI_MessagePopup messagePopup = UIManager.Instance.ShowPopupUI<UI_MessagePopup>();
        messagePopup.SetInfo(
            MessageManager.Instance.GetMessageScript(Define.EMessageType.TryRepairFence).Contents,
            new string[] { FenceData.CostToString(fence.RepairCost) },
            OnClickRepairOkayButton,
            OnClickRepairNoButton
        );
    }
    private void OnClickEnhanceButton()
    {
        if (!FenceManager.Instance.CanEnhanceFence())
            return;

        UIManager.Instance.ClosePopupUI();

        UIManager.Instance.ShowPopupUI<UI_FenceEnhancePopup>()
            .OnClosed(() =>
            {
                UIManager.Instance.ShowPopupUI<UI_FenceStatePopup>();
            });
    }
    public override void RefreshUI()
    {
        base.RefreshUI();
        UpdateUI();
        //TODO : Localization
    }
}
