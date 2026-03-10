using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UI_EventPopup : UI_UGUI, IUI_Popup, IClickableUI
{
    enum GameObjects
    {
        EventResultFrame
    }
    enum Buttons
    {
        //BG
        ClickButton,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //Content
        EventText,
        EventResultText,
        MemberStateEventText,
    }
    enum Images
    {
        //Content
        EventResultIcon,
    }
    private EEventPopupType _currentType;
    private Action _onCloseCallback;  // 추가

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        GetButton((int)Buttons.ClickButton).onClick.AddListener(() => OnClickButton());
    }
    
    /// <summary>
    /// 이벤트 팝업 정보 설정
    /// </summary>
    public void SetInfo(EEventPopupType type, string text, EventReward[] rewards = null, Action onClose = null)
    {
        _currentType = type;
        _onCloseCallback = onClose;

        SetTitleByType(_currentType);

        bool hasResult = rewards != null && rewards.Length > 0 && rewards[0].rewardData != null;
        SetEventResultFrame(hasResult);

        UpdateContent(text, rewards);

        Canvas canvas = GetComponent<Canvas>();
        CoroutineManager.Instance.StartCoroutine(ForceUpdateLayout(canvas));
    }
    
    private void UpdateContent(string text, EventReward[] rewards)
    {
        GetText((int)Texts.EventText).text = text;

        if (GetObject((int)GameObjects.EventResultFrame).activeSelf)
        {
            // 첫 번째 보상의 스프라이트와 수량 표시
            if (rewards != null && rewards.Length > 0 && rewards[0].rewardData != null)
            {
                GetImage((int)Images.EventResultIcon).sprite = rewards[0].rewardData.sprite;

                int amount = rewards[0].amount;
                string amountText = amount > 0 ? $"+{amount}" : $"{amount}";
                GetText((int)Texts.EventResultText).text = amountText;
            }
        }
    }
    
    private void SetEventResultFrame(bool isActive)
    {
        GetObject((int)GameObjects.EventResultFrame).SetActive(isActive);
    }
    
    private void SetTitleByType(EEventPopupType type)
    {
        string title = type switch
        {
            EEventPopupType.YearEvent => $"{GameManager.Instance.Year}년차",
            EEventPopupType.DispatchResult => "파견 결과",
            _ => ""
        };

        GetText((int)Texts.MainTitleText).text = title;
    }
    
    private IEnumerator ForceUpdateLayout(Canvas mainCanvas)
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(mainCanvas.GetComponent<RectTransform>());
    }
    
    public void OnClickButton()
    {
        UIManager.Instance.ClosePopupUI();
        _onCloseCallback?.Invoke();
    }
    
    public override void RefreshUI()
    {
        base.RefreshUI();
        //SetInfo();
    }
}
