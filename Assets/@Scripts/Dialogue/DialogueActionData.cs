using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using static Define;

[System.Serializable]
public class DialogueActionData
{
    public ActionType type;
    public AudioClip clip; // for PlaySound
    public string dialogueText; // Dialogue 텍스트 프로퍼티
    public CharacterId characterId; // for Dialogue
    public CharacterState characterState; // for Dialogue

    public EventReward[] rewards;

    // for ChoiceDialogue
    public string leftChoiceText;
    public string rightChoiceText;
    public DialogueEventData leftChoiceEvent;
    public DialogueEventData rightChoiceEvent;
}

public class DialogueAction
{
    public DialogueActionData Data { get; private set; }
    private bool isDialogueFinished;

    public DialogueAction(DialogueActionData data)
    {
        Data = data;
        isDialogueFinished = false;
    }

    public void Execute()
    {
        switch (Data.type)
        {
            case ActionType.PlaySound:
                AudioSource.PlayClipAtPoint(Data.clip, Vector3.zero);
                break;
            case ActionType.Dialogue:
                isDialogueFinished = false;
                DialogueManager.Instance.ShowDialogue(
                    Data.characterId,
                    Data.characterState,
                    Data.dialogueText);
                break;
            case ActionType.ChoiceDialogue:
                isDialogueFinished = false;
                DialogueManager.Instance.ShowChoiceDialogue(
                    Data.characterId, 
                    Data.characterState, 
                    Data.dialogueText,
                    Data.leftChoiceText,
                    Data.rightChoiceText,
                    Data.leftChoiceEvent,
                    Data.rightChoiceEvent);
                break;
            case ActionType.YearEvent:
                UI_EventPopup yearPopup = UIManager.Instance.ShowPopupUI<UI_EventPopup>();
                yearPopup.SetInfo(EEventPopupType.YearEvent, Data.dialogueText, Data.rewards);
                //보상 제공.
                break;
            case ActionType.DispatchResult:
                UI_EventPopup dispatchPopup = UIManager.Instance.ShowPopupUI<UI_EventPopup>();
                dispatchPopup.SetInfo(EEventPopupType.DispatchResult, Data.dialogueText, Data.rewards);
                //보상 제공.
                break;
            case ActionType.DispatchSelect:

                break;
            case ActionType.ClosePopup:
                UIManager.Instance.ClosePopupUI();
                break;
        }
    }
    private void AddReward()
    {
       
    }

    public void SetDialogueFinished()
    {
        if (Data.type == ActionType.Dialogue || Data.type == ActionType.ChoiceDialogue)
            isDialogueFinished = true;
    }

    public bool IsFinished()
    {
        switch (Data.type)
        {
            case ActionType.Dialogue:
            case ActionType.ChoiceDialogue:
                return isDialogueFinished;
            default:
                return true;
        }
    }
}

public enum ActionType
{
    PlaySound,
    Dialogue,
    ChoiceDialogue,

    YearEvent,
    DispatchResult,
    DispatchSelect,

    ClosePopup,
    //Emoticon,
    //SpawnNpc,
    //MoveNpc,
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(EventReward))]
public class EventRewardDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // RewardData (ScriptableObject)
        var rewardDataProp = property.FindPropertyRelative("rewardData");
        position.height = EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(position, rewardDataProp, new GUIContent("Reward Data"));
        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        // Amount
        var amountProp = property.FindPropertyRelative("amount");
        EditorGUI.PropertyField(position, amountProp, new GUIContent("Amount"));

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
    }
}
[CustomPropertyDrawer(typeof(DialogueActionData))]
public class DialogueActionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        var typeProp = property.FindPropertyRelative("type");
        position.height = EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(position, typeProp);
        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        ActionType type = (ActionType)typeProp.enumValueIndex;

        if (type == ActionType.PlaySound)
        {
            var clipProp = property.FindPropertyRelative("clip");
            EditorGUI.PropertyField(position, clipProp);
        }
        else if (type == ActionType.Dialogue)
        {
            var characterIdProp = property.FindPropertyRelative("characterId");
            EditorGUI.PropertyField(position, characterIdProp);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var characterStateProp = property.FindPropertyRelative("characterState");
            EditorGUI.PropertyField(position, characterStateProp);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var dialogueTextProp = property.FindPropertyRelative("dialogueText");
            EditorGUI.PropertyField(position, dialogueTextProp);
        }
        else if (type == ActionType.ChoiceDialogue)
        {
            var characterIdProp = property.FindPropertyRelative("characterId");
            EditorGUI.PropertyField(position, characterIdProp);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var characterStateProp = property.FindPropertyRelative("characterState");
            EditorGUI.PropertyField(position, characterStateProp);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var dialogueTextProp = property.FindPropertyRelative("dialogueText");
            EditorGUI.PropertyField(position, dialogueTextProp);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var leftChoiceTextProp = property.FindPropertyRelative("leftChoiceText");
            EditorGUI.PropertyField(position, leftChoiceTextProp);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var leftChoiceEventProp = property.FindPropertyRelative("leftChoiceEvent");
            EditorGUI.PropertyField(position, leftChoiceEventProp);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var rightChoiceTextProp = property.FindPropertyRelative("rightChoiceText");
            EditorGUI.PropertyField(position, rightChoiceTextProp);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var rightChoiceEventProp = property.FindPropertyRelative("rightChoiceEvent");
            EditorGUI.PropertyField(position, rightChoiceEventProp);
        }
        else if (type == ActionType.YearEvent || type == ActionType.DispatchResult)
        {
            var dialogueTextProp = property.FindPropertyRelative("dialogueText");
            EditorGUI.PropertyField(position, dialogueTextProp);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // 보상 배열 표시
            var rewardsProp = property.FindPropertyRelative("rewards");
            float rewardsHeight = EditorGUI.GetPropertyHeight(rewardsProp, true);
            position.height = rewardsHeight;
            EditorGUI.PropertyField(position, rewardsProp, new GUIContent("Rewards"), true);
        }
        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var typeProp = property.FindPropertyRelative("type");
        ActionType type = (ActionType)typeProp.enumValueIndex;
        float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        switch (type)
        {
            case ActionType.PlaySound:
                height += EditorGUIUtility.singleLineHeight;
                break;
            case ActionType.Dialogue:
                height += EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing * 2;
                break;
            case ActionType.ChoiceDialogue:
                height += EditorGUIUtility.singleLineHeight * 7 + EditorGUIUtility.standardVerticalSpacing * 6;
                break;
            case ActionType.YearEvent:
            case ActionType.DispatchResult:
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                var rewardsProp = property.FindPropertyRelative("rewards");
                height += EditorGUI.GetPropertyHeight(rewardsProp, true);
                break;
        }

        return height;
    }
}
#endif
