using System;
using System.Collections;
using System.Collections.Generic;
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
    public void SetInfo(EEventPopupType type, SavedEventInfo eventInfo, Action onClose = null)
    {
        _currentType = type;
        _onCloseCallback = onClose;

        SetTitleByType(_currentType);

        bool hasResult = eventInfo.rewards != null && eventInfo.rewards.Count > 0;
        SetEventResultFrame(hasResult);

        // 죽은 멤버 정보 표시 (연차 이벤트일 때만)
        if (_currentType == EEventPopupType.YearEvent)
        {
            SetMemberStateInfo(eventInfo.deathMembers);
        }
        else
        {
            GetText((int)Texts.MemberStateEventText).gameObject.SetActive(false);
        }

        UpdateContentFromSaved(eventInfo.eventText, eventInfo.rewards);

        Canvas canvas = GetComponent<Canvas>();
        CoroutineManager.Instance.StartCoroutine(ForceUpdateLayout(canvas));
    }
    private void SetMemberStateInfo(List<string> deathMembers)
    {
        if (deathMembers == null || deathMembers.Count == 0)
        {
            GetText((int)Texts.MemberStateEventText).gameObject.SetActive(false);
            return;
        }

        GetText((int)Texts.MemberStateEventText).gameObject.SetActive(true);

        // 멤버 이름들을 ", "로 연결
        string memberNames = string.Join(", ", deathMembers);

        // 텍스트 생성
        string stateText = $"{memberNames}의 상태가 몹시 안 좋다.\r\n어쩔 수 없이 해고했다..";

        GetText((int)Texts.MemberStateEventText).text = stateText;
    }

    //private void UpdateContent(string text, EventReward[] rewards)
    //{
    //    GetText((int)Texts.EventText).text = text;

    //    if (GetObject((int)GameObjects.EventResultFrame).activeSelf)
    //    {
    //        // 첫 번째 보상의 스프라이트와 수량 표시
    //        if (rewards != null && rewards.Length > 0 && rewards[0].rewardData != null)
    //        {
    //            GetImage((int)Images.EventResultIcon).sprite = rewards[0].rewardData.sprite;

    //            int amount = rewards[0].amount;
    //            string amountText = amount > 0 ? $"+{amount}" : $"{amount}";
    //            GetText((int)Texts.EventResultText).text = amountText;
    //        }
    //    }
    //}
    
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

    /// <summary>
    /// SavedEventInfo의 보상 정보로 UI 업데이트
    /// </summary>
    private void UpdateContentFromSaved(string text, List<SerializableReward> rewards)
    {
        GetText((int)Texts.EventText).text = text;

        if (GetObject((int)GameObjects.EventResultFrame).activeSelf && rewards != null && rewards.Count > 0)
        {
            var firstReward = rewards[0];

            // DataManager의 RewardDict에서 EventRewardData 가져오기
            if (DataManager.Instance.RewardDict.TryGetValue(firstReward.rewardId, out EventRewardData rewardData))
            {
                GetImage((int)Images.EventResultIcon).sprite = rewardData.sprite;

                int amount = firstReward.amount;
                string amountText = amount > 0 ? $"+{amount}" : $"{amount}";
                GetText((int)Texts.EventResultText).text = amountText;
            }
            else
            {
                Debug.LogError($"[UI_EventPopup] EventRewardData not found for rewardId: {firstReward.rewardId}");
            }
        }
    }
}
