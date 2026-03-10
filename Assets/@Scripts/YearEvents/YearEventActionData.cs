using UnityEngine;
using UnityEditor;

[System.Serializable]
public class YearEventActionData
{
    public YearActionType type;

    [TextArea(3, 10)]
    public string eventText;        // for ShowEventPopup
    public EventReward[] rewards;   // for ShowEventPopup, GiveRewards

    public string memberTargetType; // for FireMember (Random, Lowest, etc.)
}

public class YearEventAction
{
    public YearEventActionData Data { get; private set; }
    private bool isFinished;

    public YearEventAction(YearEventActionData data)
    {
        Data = data;
        isFinished = false;
    }

    public void Execute()
    {
        switch (Data.type)
        {
            case YearActionType.ShowEventPopup:
                ShowEventPopup();
                break;

            case YearActionType.GiveRewards:
                GiveRewards();
                isFinished = true;
                break;

            case YearActionType.FireMember:
                FireMember();
                isFinished = true;
                break;

            case YearActionType.ClosePopup:
                UIManager.Instance.ClosePopupUI();
                isFinished = true;
                break;
        }
    }

    private void ShowEventPopup()
    {
        UI_EventPopup popup = UIManager.Instance.ShowPopupUI<UI_EventPopup>();
        popup.SetInfo(
            Define.EEventPopupType.YearEvent,
            Data.eventText,
            Data.rewards,
            () => isFinished = true
        );
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
                    Debug.Log($"[YearEventAction] +Member (Not implemented)");
                    break;
            }
        }
    }

    private void FireMember()
    {
        // TODO: 멤버 해고 로직
        Debug.Log($"[YearEventAction] Fire member (Not implemented)");
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
    ShowEventPopup,
    GiveRewards,
    FireMember,
    ClosePopup
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

            case YearActionType.GiveRewards:
                var rewardsProp2 = property.FindPropertyRelative("rewards");
                float rewardsHeight2 = EditorGUI.GetPropertyHeight(rewardsProp2, true);
                position.height = rewardsHeight2;
                EditorGUI.PropertyField(position, rewardsProp2, new GUIContent("Rewards"), true);
                break;

            case YearActionType.FireMember:
                var memberTargetProp = property.FindPropertyRelative("memberTargetType");
                position.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(position, memberTargetProp, new GUIContent("Target Type"));
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

            case YearActionType.GiveRewards:
                var rewardsProp2 = property.FindPropertyRelative("rewards");
                height += EditorGUI.GetPropertyHeight(rewardsProp2, true);
                break;

            case YearActionType.FireMember:
                height += EditorGUIUtility.singleLineHeight;
                break;
        }

        return height;
    }
}
#endif
