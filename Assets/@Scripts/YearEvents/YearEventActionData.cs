using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using static Define;

[System.Serializable]
public class YearEventActionData
{
    public YearActionType type;

    [TextArea(3, 10)]
    public string eventText;        // for ShowEventPopup
    public EventReward[] rewards;   // for ShowEventPopup, GiveRewards

    public int memberIndex;         // for ChangeHungerState (멤버 인덱스)
    public int hungerChange;        // for ChangeHungerState (배고픔 변화량: 양수=배고파짐, 음수=배불러짐)

    [TextArea(3, 10)]
    public string choiceEventText;  // for ShowChoicePopup (선택지 텍스트)
    public YearEventData yesNextEvent;  // Yes 선택 시 해금할 이벤트
    public YearEventData noNextEvent;   // No 선택 시 해금할 이벤트
    public bool executeImmediately = false;  // 체인 실행 옵션

    public EEndingType endingType;  // for TriggerEnding (엔딩 타입)

    public string soundClipName;    // for PlaySound (사운드 클립 이름)
}

public class YearEventAction
{
    public YearEventActionData Data { get; private set; }
    private bool isFinished;
    private EventCategory _category;

    public YearEventAction(YearEventActionData data, EventCategory category)
    {
        Data = data;
        _category = category;
        isFinished = false;
    }
    public void Reset()
    {
        isFinished = false;
    }
    public void Execute()
    {
        switch (Data.type)
        {
            case YearActionType.ShowEventPopup:
                ShowEventPopup();
                break;

            case YearActionType.ShowChoicePopup:
                ShowChoicePopup();
                break;

            case YearActionType.GiveRewards:
                GiveRewards();
                isFinished = true;
                break;

            case YearActionType.FireMember:
                FireMember();
                isFinished = true;
                break;

            case YearActionType.ChangeHungerState:
                ChangeHungerState();
                isFinished = true;
                break;

            case YearActionType.TriggerEnding:
                TriggerEnding();
                isFinished = true;
                break;

            case YearActionType.PlaySound:
                PlaySound();
                isFinished = true;
                break;
        }
    }

    private void ShowEventPopup()
    {
        UI_EventPopup popup = UIManager.Instance.ShowPopupUI<UI_EventPopup>();

        Define.EEventPopupType popupType = _category == EventCategory.YearEvent
            ? Define.EEventPopupType.YearEvent
            : Define.EEventPopupType.DispatchResult;

        SavedEventInfo eventInfo = new SavedEventInfo
        {
            eventText = Data.eventText,
            rewards = ConvertToSerializableRewards(Data.rewards),
            deathMembers = new List<string>()
        };

        if (_category == EventCategory.YearEvent)
        {
            YearEventManager.Instance.SetLastYearEvent(eventInfo);
        }
        else if (_category == EventCategory.DispatchResult)
        {
            YearEventManager.Instance.SetLastDispatchResult(eventInfo);
        }

        popup.SetInfo(
            popupType,
            eventInfo,
            () => isFinished = true
        );
    }
    private void ShowChoicePopup()
    {
        UI_EventChoicePopup popup = UIManager.Instance.ShowPopupUI<UI_EventChoicePopup>();

        Define.EEventPopupType popupType = _category == EventCategory.YearEvent
            ? Define.EEventPopupType.YearEvent
            : Define.EEventPopupType.DispatchResult;

        popup.SetInfo(
            popupType,
            Data.choiceEventText,
            () => OnChoiceSelected(true),   // Yes
            () => OnChoiceSelected(false)   // No
        );
    }
    private void OnChoiceSelected(bool isYes)
    {
        YearEventData nextEventData = isYes ? Data.yesNextEvent : Data.noNextEvent;

        if (nextEventData != null)
        {
            if (Data.executeImmediately)
            {
                CoroutineManager.Instance.Run(CoExecuteNextEventChain(nextEventData));
            }
            else
            {
                // 해금만 (다음 기회에 실행)
                YearEventManager.Instance.UnlockEvent(nextEventData);
                isFinished = true;
            }
        }
        else
        {
            isFinished = true;
        }
    }
    private System.Collections.IEnumerator CoExecuteNextEventChain(YearEventData nextEvent)
    {
        Debug.Log($"[YearEventAction] Executing next event chain: {nextEvent.name}");

        // 다음 이벤트 해금 및 실행
        yield return CoroutineManager.Instance.Run(
            YearEventManager.Instance.UnlockAndExecuteEvent(nextEvent)
        );

        Debug.Log($"[YearEventAction] Next event chain completed: {nextEvent.name}");

        // 체인 실행 완료 후 현재 액션 완료
        isFinished = true;
    }
    private void GiveRewards()
    {
        if (Data.rewards == null || Data.rewards.Length == 0)
            return;

        foreach (var reward in Data.rewards)
        {
            if (reward.rewardData == null) continue;

            switch (reward.rewardData.rewardType)
            {
                case RewardType.Gold:
                    GameManager.Instance.Gold += reward.amount;
                    Debug.Log($"[YearEventAction] +{reward.amount} gold");
                    break;

                case RewardType.Food:
                    GameManager.Instance.Food += reward.amount;
                    Debug.Log($"[YearEventAction] +{reward.amount} food");
                    break;

                case RewardType.Member:
                    MemberManager.Instance.HireMember(reward.rewardData.rewardId);
                    break;
            }
        }
    }

    private List<SerializableReward> ConvertToSerializableRewards(EventReward[] rewards)
    {
        List<SerializableReward> serializableRewards = new List<SerializableReward>();

        if (rewards == null || rewards.Length == 0)
        {
            return serializableRewards;
        }

        foreach (var reward in rewards)
        {
            if (reward.rewardData == null)
                continue;

            serializableRewards.Add(new SerializableReward
            {
                rewardType = reward.rewardData.rewardType,
                amount = reward.amount,
                rewardId = reward.rewardData.rewardId
            });
        }

        return serializableRewards;
    }
    private void FireMember()
    {
        // TODO: 멤버 해고 로직
        Debug.Log($"[YearEventAction] Fire member (Not implemented)");
    }
    private void ChangeHungerState()
    {
        List<Member> eligibleMembers = new List<Member>();
        List<Member> activeMembers = MemberManager.Instance.GetActiveMembers(); // 파견 제외

        foreach (var member in activeMembers)
        {
            int index = MemberManager.Instance.GetIndex(member);
            if (index > 0) // 보스(인덱스 0) 제외
            {
                eligibleMembers.Add(member);
            }
        }

        // 선택 가능한 멤버가 없으면 종료
        if (eligibleMembers.Count == 0)
        {
            Debug.LogWarning($"[YearEventAction] No eligible members for hunger change (Boss and dispatched excluded)");
            return;
        }

        // 랜덤으로 한 명 선택
        int randomListIndex = Random.Range(0, eligibleMembers.Count);
        Member selectedMember = eligibleMembers[randomListIndex];
        int memberIndex = MemberManager.Instance.GetIndex(selectedMember);

        if (selectedMember == null || selectedMember.CurrentMemberData == null)
        {
            Debug.LogWarning($"[YearEventAction] Member not found at index: {Data.memberIndex}");
            return;
        }

        if (Data.hungerChange > 0)
        {
            // 양수: 배고픔 증가
            MemberManager.Instance.IncreaseHungerState(Data.memberIndex, Data.hungerChange);
        }
        else if (Data.hungerChange < 0)
        {
            // 음수: 배고픔 감소 (절댓값 사용)
            MemberManager.Instance.DecreaseHungerState(Data.memberIndex, Mathf.Abs(Data.hungerChange));
        }
    }
    /// <summary>
    /// 엔딩 트리거 (게임 종료)
    /// </summary>
    private void TriggerEnding()
    {
        EndingManager.Instance.TriggerEnding(Data.endingType);
    }
    /// <summary>
    /// 사운드 재생
    /// </summary>
    private void PlaySound()
    {
        if (string.IsNullOrEmpty(Data.soundClipName))
        {
            Debug.LogWarning("[YearEventAction] Sound clip name is empty");
            return;
        }

        SoundManager.Instance.Play2D(ESound.Effect, Data.soundClipName);
        Debug.Log($"[YearEventAction] Playing sound: {Data.soundClipName}");
    }
    public bool IsFinished()
    {
        return isFinished;
    }

    public void SetFinished()
    {
        isFinished = true;
    }
}

public enum YearActionType
{
    ShowEventPopup,     // 이벤트 팝업 표시 (텍스트 + 보상)
    ShowChoicePopup,    // 선택지 팝업 표시 (Yes/No 선택)
    GiveRewards,        // 보상 지급 (골드, 음식, 멤버)
    FireMember,         // 멤버 해고
    ChangeHungerState,  // 배고픔 상태 변경 (양수: 배고픔 증가 / 음수: 배고픔 감소)
    TriggerEnding,      // 엔딩 트리거 (게임 종료)
    PlaySound           // 사운드 재생
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(YearEventActionData))]
public class YearEventActionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        var typeProp = property.FindPropertyRelative("type");
        position.height = EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(position, typeProp);
        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        YearActionType type = (YearActionType)typeProp.enumValueIndex;

        switch (type)
        {
            case YearActionType.ShowEventPopup:
                // Event Text (TextArea 지원)
                var eventTextProp = property.FindPropertyRelative("eventText");
                float eventTextHeight = EditorGUI.GetPropertyHeight(eventTextProp, true);
                position.height = eventTextHeight;
                EditorGUI.PropertyField(position, eventTextProp, new GUIContent("Event Text"), true);
                position.y += eventTextHeight + EditorGUIUtility.standardVerticalSpacing;

                // Rewards
                var rewardsProp = property.FindPropertyRelative("rewards");
                float rewardsHeight = EditorGUI.GetPropertyHeight(rewardsProp, true);
                position.height = rewardsHeight;
                EditorGUI.PropertyField(position, rewardsProp, new GUIContent("Rewards"), true);
                break;

            case YearActionType.ShowChoicePopup:
                // Event Text
                var choiceTextProp = property.FindPropertyRelative("choiceEventText");
                float choiceTextHeight = EditorGUI.GetPropertyHeight(choiceTextProp, true);
                position.height = choiceTextHeight;
                EditorGUI.PropertyField(position, choiceTextProp, new GUIContent("Choice Text"), true);
                position.y += choiceTextHeight + EditorGUIUtility.standardVerticalSpacing;

                // Yes Next Event
                var yesNextProp = property.FindPropertyRelative("yesNextEvent");
                position.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(position, yesNextProp, new GUIContent("Yes Unlock Event"));
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                // No Next Event
                var noNextProp = property.FindPropertyRelative("noNextEvent");
                position.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(position, noNextProp, new GUIContent("No Unlock Event"));
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                // Execute Immediately
                var executeImmediatelyProp = property.FindPropertyRelative("executeImmediately");
                position.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(position, executeImmediatelyProp, new GUIContent("Execute Immediately"));
                break;

            case YearActionType.GiveRewards:
                var rewardsProp2 = property.FindPropertyRelative("rewards");
                float rewardsHeight2 = EditorGUI.GetPropertyHeight(rewardsProp2, true);
                position.height = rewardsHeight2;
                EditorGUI.PropertyField(position, rewardsProp2, new GUIContent("Rewards"), true);
                break;

            case YearActionType.FireMember:
                var memberIndexProp = property.FindPropertyRelative("memberIndex");
                position.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(position, memberIndexProp, new GUIContent("Member Index"));
                break;

            case YearActionType.ChangeHungerState:
                // Hunger Change
                var hungerChangeProp = property.FindPropertyRelative("hungerChange");
                position.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(position, hungerChangeProp, new GUIContent("Hunger Change (+ 배고픔 / - 배부름)"));
                break;

            case YearActionType.TriggerEnding:
                // Ending Type
                var endingTypeProp = property.FindPropertyRelative("endingType");
                position.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(position, endingTypeProp, new GUIContent("Ending Type"));
                break;

            case YearActionType.PlaySound:
                // Sound Clip Name
                var soundClipNameProp = property.FindPropertyRelative("soundClipName");
                position.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(position, soundClipNameProp, new GUIContent("Sound Clip Name"));
                break;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var typeProp = property.FindPropertyRelative("type");
        YearActionType type = (YearActionType)typeProp.enumValueIndex;
        float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        switch (type)
        {
            case YearActionType.ShowEventPopup:
                var eventTextProp = property.FindPropertyRelative("eventText");
                height += EditorGUI.GetPropertyHeight(eventTextProp, true) + EditorGUIUtility.standardVerticalSpacing;
                var rewardsProp = property.FindPropertyRelative("rewards");
                height += EditorGUI.GetPropertyHeight(rewardsProp, true);
                break;

            case YearActionType.ShowChoicePopup:
                var choiceTextProp = property.FindPropertyRelative("choiceEventText");
                height += EditorGUI.GetPropertyHeight(choiceTextProp, true) + EditorGUIUtility.standardVerticalSpacing;
                height += EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing * 3; // Yes, No fields
                break;

            case YearActionType.GiveRewards:
                var rewardsProp2 = property.FindPropertyRelative("rewards");
                height += EditorGUI.GetPropertyHeight(rewardsProp2, true);
                break;

            case YearActionType.FireMember:
            case YearActionType.ChangeHungerState:
            case YearActionType.TriggerEnding:
            case YearActionType.PlaySound:
                height += EditorGUIUtility.singleLineHeight;
                break;
        }

        return height;
    }
}
#endif
