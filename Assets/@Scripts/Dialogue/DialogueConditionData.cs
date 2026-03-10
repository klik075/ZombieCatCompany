using UnityEditor;
using UnityEngine;

[System.Serializable]
public class DialogueConditionData
{
    public ConditionType type;
    public float timeValue;
    public int moneyValue;
    public float coolTimeValue;

    public int yearValue;
    public int minYearValue;
    public int maxYearValue;
    public int eventIDValue;        // 선행 이벤트 ID
    public int memberCountValue;
    public int fenceLevelValue;
}

public enum ConditionType
{
    TimeElapsed,            //
    MoneyGreaterThan,
    CoolTime,

    YearGreaterThan,        // 특정 연차 이상
    YearBetween,            // 연차 범위
    EventViewed,            // 특정 이벤트를 봤는지
    MemberCountGreaterThan, // 멤버 수
    FenceLevelGreaterThan,  // 펜스 레벨
    HasDispatchResult,      // 파견 결과 있음
}

public class DialogueCondition
{
    public DialogueConditionData Data { get; private set; }
    private float lastCheckedTime = -99999f;

    public void Initialize(DialogueConditionData data)
    {
        Data = data;
        lastCheckedTime = -99999f;
    }

    public bool Check()
    {
        switch (Data.type)
        {
            case ConditionType.TimeElapsed:
                return Time.time >= Data.timeValue;
            case ConditionType.MoneyGreaterThan:
                return GameManager.Instance.Gold > Data.moneyValue;
            case ConditionType.CoolTime:
                if (Time.time - lastCheckedTime >= Data.coolTimeValue)
                    return true;
                return false;
            case ConditionType.YearGreaterThan:
                return GameManager.Instance.Year >= Data.yearValue;

            case ConditionType.YearBetween:
                int year = GameManager.Instance.Year;
                return year >= Data.minYearValue && (Data.maxYearValue == 0 || year <= Data.maxYearValue);

            case ConditionType.EventViewed:
                //return YearEventManager.Instance.HasViewedEvent(Data.eventIDValue);
                return false;//placeholder
            case ConditionType.MemberCountGreaterThan:
                return MemberManager.Instance.MemberCount >= Data.memberCountValue;

            case ConditionType.FenceLevelGreaterThan:
                return FenceManager.Instance.CurrentFence != null &&
                       FenceManager.Instance.CurrentFence.EnhanceLevel >= Data.fenceLevelValue;

            case ConditionType.HasDispatchResult:
                //var dispatchData = GameManager.Instance.MyNightData.DispatchData;
                //return dispatchData.DispatchedMemberIndex >= 0 &&
                //       !dispatchData.IsReturned &&
                //       GameManager.Instance.Year > dispatchData.DispatchYear;

            default:
                return true;
        }
    }

    public void RecordExecutionTime()
    {
        lastCheckedTime = Time.time;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(DialogueConditionData))]
public class DialogueConditionDataDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        var typeProp = property.FindPropertyRelative("type");
        position.height = EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(position, typeProp);
        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        ConditionType type = (ConditionType)typeProp.enumValueIndex;
        switch (type)
        {
            case ConditionType.TimeElapsed:
                var timeProp = property.FindPropertyRelative("timeValue");
                EditorGUI.PropertyField(position, timeProp, new GUIContent("Required Time"));
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                break;
            case ConditionType.MoneyGreaterThan:
                var moneyProp = property.FindPropertyRelative("moneyValue");
                EditorGUI.PropertyField(position, moneyProp, new GUIContent("Required Money"));
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                break;
            case ConditionType.CoolTime:
                var coolTimeProp = property.FindPropertyRelative("coolTimeValue");
                EditorGUI.PropertyField(position, coolTimeProp, new GUIContent("Cool Time (sec)"));
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                break;
        }
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var typeProp = property.FindPropertyRelative("type");
        ConditionType type = (ConditionType)typeProp.enumValueIndex;
        int lines = 1; // type line
        switch (type)
        {
            case ConditionType.TimeElapsed:
            case ConditionType.MoneyGreaterThan:
            case ConditionType.CoolTime:
                lines += 1; // value line
                break;
        }
        return EditorGUIUtility.singleLineHeight * lines + EditorGUIUtility.standardVerticalSpacing * (lines - 1);
    }
}
#endif
