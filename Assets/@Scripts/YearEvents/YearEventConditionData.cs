using UnityEngine;
using UnityEditor;

[System.Serializable]
public class YearEventConditionData
{
    public YearConditionType type;
    public int yearValue;       // for YearGreaterThan, YearBetween
    public int yearValueMax;    // for YearBetween
    public int goldValue;       // for GoldGreaterThan, GoldLessThan
    public int foodValue;       // for FoodGreaterThan, FoodLessThan
    public int memberCount;     // for MemberCountGreaterThan, MemberCountLessThan
    public YearEventData requiredEvent;    // for EventViewed, EventNotViewed
}

public class YearEventCondition
{
    public YearEventConditionData Data { get; private set; }

    public void Initialize(YearEventConditionData data)
    {
        Data = data;
    }

    public bool Check()
    {
        switch (Data.type)
        {
            case YearConditionType.YearGreaterThan:
                return GameManager.Instance.Year >= Data.yearValue;

            case YearConditionType.YearBetween:
                return GameManager.Instance.Year >= Data.yearValue && 
                       GameManager.Instance.Year <= Data.yearValueMax;

            case YearConditionType.GoldGreaterThan:
                return GameManager.Instance.Gold >= Data.goldValue;

            case YearConditionType.GoldLessThan:
                return GameManager.Instance.Gold < Data.goldValue;

            case YearConditionType.FoodGreaterThan:
                return GameManager.Instance.Food >= Data.foodValue;

            case YearConditionType.FoodLessThan:
                return GameManager.Instance.Food < Data.foodValue;

            case YearConditionType.MemberCountGreaterThan:
                return MemberManager.Instance.GetAllMembers().Count >= Data.memberCount;

            case YearConditionType.MemberCountLessThan:
                return MemberManager.Instance.GetAllMembers().Count < Data.memberCount;

            case YearConditionType.EventViewed:
                if (Data.requiredEvent == null)
                    return true; // null이면 조건 무시

                return YearEventManager.Instance.HasExecutedEvent(Data.requiredEvent);

            case YearConditionType.EventNotViewed:
                if (Data.requiredEvent == null)
                    return true; // null이면 조건 무시

                return !YearEventManager.Instance.HasExecutedEvent(Data.requiredEvent);

            case YearConditionType.EventIsUnlocked:
                if (Data.requiredEvent == null)
                    return true;

                return YearEventManager.Instance.IsEventUnlocked(Data.requiredEvent);

            case YearConditionType.EventIsLocked:
                if (Data.requiredEvent == null)
                    return true;

                return !YearEventManager.Instance.IsEventUnlocked(Data.requiredEvent);

            default:
                return true;
        }
    }
}

public enum YearConditionType
{
    YearGreaterThan,//연차 이상
    YearBetween, //연차 범위
    GoldGreaterThan, //골드 이상
    GoldLessThan, //골드 미만
    FoodGreaterThan, //음식 이상
    FoodLessThan, //음식 미만
    MemberCountGreaterThan, //멤버 수 이상
    MemberCountLessThan, //멤버 수 미만
    EventViewed, //특정 이벤트를 본 적이 있는지
    EventNotViewed, //특정 이벤트를 본 적이 없는지
    EventIsUnlocked,  // 특정 이벤트가 해금되었는지
    EventIsLocked,    // 특정 이벤트가 잠겨있는지
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(YearEventConditionData))]
public class YearEventConditionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        var typeProp = property.FindPropertyRelative("type");
        position.height = EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(position, typeProp);
        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        YearConditionType type = (YearConditionType)typeProp.enumValueIndex;

        switch (type)
        {
            case YearConditionType.YearGreaterThan:
                var yearValueProp = property.FindPropertyRelative("yearValue");
                EditorGUI.PropertyField(position, yearValueProp, new GUIContent("Min Year"));
                break;

            case YearConditionType.YearBetween:
                var yearMinProp = property.FindPropertyRelative("yearValue");
                EditorGUI.PropertyField(position, yearMinProp, new GUIContent("Min Year"));
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                var yearMaxProp = property.FindPropertyRelative("yearValueMax");
                EditorGUI.PropertyField(position, yearMaxProp, new GUIContent("Max Year"));
                break;

            case YearConditionType.GoldGreaterThan:
                var goldGreaterProp = property.FindPropertyRelative("goldValue");
                EditorGUI.PropertyField(position, goldGreaterProp, new GUIContent("Min Gold"));
                break;

            case YearConditionType.GoldLessThan:
                var goldLessProp = property.FindPropertyRelative("goldValue");
                EditorGUI.PropertyField(position, goldLessProp, new GUIContent("Max Gold"));
                break;

            case YearConditionType.FoodGreaterThan:
                var foodGreaterProp = property.FindPropertyRelative("foodValue");
                EditorGUI.PropertyField(position, foodGreaterProp, new GUIContent("Min Food"));
                break;

            case YearConditionType.FoodLessThan:
                var foodLessProp = property.FindPropertyRelative("foodValue");
                EditorGUI.PropertyField(position, foodLessProp, new GUIContent("Max Food"));
                break;

            case YearConditionType.MemberCountGreaterThan:
                var memberGreaterProp = property.FindPropertyRelative("memberCount");
                EditorGUI.PropertyField(position, memberGreaterProp, new GUIContent("Min Members"));
                break;

            case YearConditionType.MemberCountLessThan:
                var memberLessProp = property.FindPropertyRelative("memberCount");
                EditorGUI.PropertyField(position, memberLessProp, new GUIContent("Max Members"));
                break;

            case YearConditionType.EventViewed:
                var eventViewedProp = property.FindPropertyRelative("requiredEvent");
                EditorGUI.PropertyField(position, eventViewedProp, new GUIContent("Required Event (Viewed)"));
                break;

            case YearConditionType.EventNotViewed:
                var eventNotViewedProp = property.FindPropertyRelative("requiredEvent");
                EditorGUI.PropertyField(position, eventNotViewedProp, new GUIContent("Required Event (Not Viewed)"));
                break;

            case YearConditionType.EventIsUnlocked:
                var eventUnlockedProp = property.FindPropertyRelative("requiredEvent");
                EditorGUI.PropertyField(position, eventUnlockedProp, new GUIContent("Required Event (Unlocked)"));
                break;

            case YearConditionType.EventIsLocked:
                var eventLockedProp = property.FindPropertyRelative("requiredEvent");
                EditorGUI.PropertyField(position, eventLockedProp, new GUIContent("Required Event (Locked)"));
                break;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var typeProp = property.FindPropertyRelative("type");
        YearConditionType type = (YearConditionType)typeProp.enumValueIndex;
        int lines = 1; // type

        switch (type)
        {
            case YearConditionType.YearBetween:
                lines += 2;
                break;
            default:
                lines += 1;
                break;
        }

        return EditorGUIUtility.singleLineHeight * lines + EditorGUIUtility.standardVerticalSpacing * (lines - 1);
    }
}
#endif
