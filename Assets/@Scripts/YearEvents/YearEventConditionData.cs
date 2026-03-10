using UnityEngine;
using UnityEditor;

[System.Serializable]
public class YearEventConditionData
{
    public YearConditionType type;
    public int yearValue;       // for YearGreaterThan, YearBetween
    public int yearValueMax;    // for YearBetween
    public int goldValue;       // for GoldGreaterThan
    public int foodValue;       // for FoodGreaterThan
    public int memberCount;     // for MemberCountGreaterThan
    public YearEventData requiredEvent;    // for EventViewed
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

            case YearConditionType.FoodGreaterThan:
                return GameManager.Instance.Food >= Data.foodValue;

            case YearConditionType.MemberCountGreaterThan:
                return MemberManager.Instance.GetAllMembers().Count >= Data.memberCount;

            case YearConditionType.EventViewed:
                if (Data.requiredEvent == null)
                    return true; // null이면 조건 무시

                return YearEventManager.Instance.HasExecutedEvent(Data.requiredEvent);

            default:
                return true;
        }
    }
}

public enum YearConditionType
{
    YearGreaterThan,
    YearBetween,
    GoldGreaterThan,
    FoodGreaterThan,
    MemberCountGreaterThan,
    EventViewed
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
                var goldProp = property.FindPropertyRelative("goldValue");
                EditorGUI.PropertyField(position, goldProp, new GUIContent("Min Gold"));
                break;

            case YearConditionType.FoodGreaterThan:
                var foodProp = property.FindPropertyRelative("foodValue");
                EditorGUI.PropertyField(position, foodProp, new GUIContent("Min Food"));
                break;

            case YearConditionType.MemberCountGreaterThan:
                var memberProp = property.FindPropertyRelative("memberCount");
                EditorGUI.PropertyField(position, memberProp, new GUIContent("Min Members"));
                break;

            case YearConditionType.EventViewed:
                var eventNameProp = property.FindPropertyRelative("requiredEvent");
                EditorGUI.PropertyField(position, eventNameProp, new GUIContent("RequiredEvent"));
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
