using System;
using UnityEngine;
using static Define;

public class UI_PurchaseFoodPopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
        BG,
    }
    enum Buttons
    {
        InputButton,
        OkayButton
    }
    enum Texts
    {
        MainTitleText,
        SubMiddleNameText,
        
        // LeftContent
        PossessionNameText,
        PossessionText,
        
        // RightContent
        DescriptionText,
        InputFoodNameText,
        InputFoodText,
        CostNameText,
        CostText,
        TotalCostNameText,
        TotalCostText,
        
        // SubBottom
        InputButtonText,
        OkayButtonText,
    }
    enum Images
    {
        FoodImage,
    }
    
    private int _purchaseAmount;  // 구매 희망 개수
    private const int MIN_PURCHASE_AMOUNT = 1;

    protected override void Awake()
    {
        base.Awake();
        
        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        GetButton((int)Buttons.InputButton).onClick.AddListener(() => OnClickInputButton());
        GetButton((int)Buttons.OkayButton).onClick.AddListener(() => OnClickOkayButton());
    }
    
    protected override void OnEnable()
    {
        base.OnEnable();
       
        EventManager.Instance.AddEvent(EEventType.FoodChanged, UpdatePossesionFood);
    }
    
    protected override void OnDisable()
    {
        base.OnDisable();
        
        EventManager.Instance.RemoveEvent(EEventType.FoodChanged, UpdatePossesionFood);
    }
    
    public void SetInfo()
    {
        _purchaseAmount = MIN_PURCHASE_AMOUNT;
        UpdateContent();
    }
    
    /// <summary>
    /// UI 내용 업데이트
    /// </summary>
    private void UpdateContent()
    {
        // 타이틀
        GetText((int)Texts.MainTitleText).text = "통조림 구매";
        GetText((int)Texts.SubMiddleNameText).text = "냥냥 통조림";
        
        // 보유 통조림
        GetText((int)Texts.PossessionNameText).text = "보유";
        GetText((int)Texts.PossessionText).text = $"{GameManager.Instance.Food}";
        
        // 설명
        GetText((int)Texts.DescriptionText).text = "먹을 만한 통조림.";

        UpdateFoodText();

        // 버튼 텍스트
        GetText((int)Texts.InputButtonText).text = "개수 입력";
        GetText((int)Texts.OkayButtonText).text = "구매";
    }
    private void UpdateFoodText()
    {
        // 구매 희망 개수
        GetText((int)Texts.InputFoodNameText).text = "구매 희망";
        GetText((int)Texts.InputFoodText).text = $"{_purchaseAmount}개";

        // 개당 가격
        GetText((int)Texts.CostNameText).text = "개당 가격";
        GetText((int)Texts.CostText).text = $"{GameManager.Instance.FOOD_PRICE_PER_UNIT:N0}G";

        // 총 가격
        int totalCost = _purchaseAmount * GameManager.Instance.FOOD_PRICE_PER_UNIT;
        GetText((int)Texts.TotalCostNameText).text = "총 가격";
        GetText((int)Texts.TotalCostText).text = $"{totalCost:N0}G";
    }
    private void UpdatePossesionFood()
    {
        GetText((int)Texts.PossessionText).text = $"{GameManager.Instance.Food}";
    }

    /// <summary>
    /// 수량 입력 버튼 클릭
    /// </summary>
    private void OnClickInputButton()
    {
        UI_InputFieldPopup inputPopup = UIManager.Instance.ShowPopupUI<UI_InputFieldPopup>();
        inputPopup.SetInfo(EInputFieldType.PurchaseFood, OnInputCompleted);
    }
    
    /// <summary>
    /// 수량 입력 완료
    /// </summary>
    private void OnInputCompleted(string input)
    {
        if (int.TryParse(input, out int amount))
        {
            _purchaseAmount = Mathf.Max(MIN_PURCHASE_AMOUNT, amount);
            UpdateFoodText();
        }
    }
    
    /// <summary>
    /// 구매 버튼 클릭
    /// </summary>
    private void OnClickOkayButton()
    {
        bool success = GameManager.Instance.TryPurchaseFood(_purchaseAmount);

        if (!success)
        {
            UI_ChatPopup chatPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
            chatPopup.SetInfo(
                MemberManager.MAIN_CHARACTER_ID,
                MessageManager.Instance.GetMessageScript(EMessageType.MoneyLow).Contents
            );
        }
        else
        {
            //구매 사운드
        }
    }

    public override void RefreshUI()
    {
        base.RefreshUI();
        UpdateContent();
    }
}
