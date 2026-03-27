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
        EventResultFrame,
        RewardFrame1,
        RewardFrame2,
        RewardFrame3,
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

        EventResultText1,
        EventResultText2,
        EventResultText3,

        MemberStateEventText,
    }
    enum Images
    {
        //Content
        EventResultIcon1,
        EventResultIcon2,
        EventResultIcon3,
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

        // 모든 RewardFrame 초기화 (비활성화)
        GetObject((int)GameObjects.RewardFrame1).SetActive(false);
        GetObject((int)GameObjects.RewardFrame2).SetActive(false);
        GetObject((int)GameObjects.RewardFrame3).SetActive(false);

        if (GetObject((int)GameObjects.EventResultFrame).activeSelf && rewards != null && rewards.Count > 0)
        {
            // 최대 3개의 보상까지 처리
            int rewardCount = Mathf.Min(rewards.Count, 3);

            for (int i = 0; i < rewardCount; i++)
            {
                var reward = rewards[i];

                // DataManager의 RewardDict에서 EventRewardData 가져오기
                if (DataManager.Instance.RewardDict.TryGetValue(reward.rewardId, out EventRewardData rewardData))
                {
                    // i번째 RewardFrame 활성화
                    GameObject rewardFrame = GetObject((int)GameObjects.RewardFrame1 + i);
                    rewardFrame.SetActive(true);

                    // i번째 아이콘 설정
                    Image icon = GetImage((int)Images.EventResultIcon1 + i);
                    if (icon != null && rewardData.sprite != null)
                    {
                        icon.sprite = rewardData.sprite;
                    }

                    // i번째 텍스트 설정
                    int amount = reward.amount;
                    string formattedAmount = amount > 0 ? $"+{amount}" : $"{amount}";
                    GetText((int)Texts.EventResultText1 + i).text = formattedAmount;
                }
                else
                {
                    Debug.LogError($"[UI_EventPopup] EventRewardData not found for rewardId: {reward.rewardId}");
                }
            }
        }
    }
}
