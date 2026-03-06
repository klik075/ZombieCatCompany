using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

[System.Serializable]
public class DialogueActionData
{
    public ActionType type;
    public AudioClip clip; // for PlaySound
    public string dialogueText; // Dialogue 텍스트 프로퍼티
    public CharacterId characterId; // for Dialogue
    public CharacterState characterState; // for Dialogue

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
                DialogueManager.Instance.ShowDialogue(Data.characterId, Data.characterState, Data.dialogueText);
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
        }
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

    Emoticon,
    SpawnNpc,
    MoveNpc,

    ShowYearEventResult,
    ShowDispatchResult,

    StartYearEvent,
    StartDispatch,
    StartFoodRation,
}

#if UNITY_EDITOR
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
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var typeProp = property.FindPropertyRelative("type");
        ActionType type = (ActionType)typeProp.enumValueIndex;
        int lines = 1; // only type
        switch (type)
        {
            case ActionType.PlaySound:
                lines += 1;
                break;
            case ActionType.Dialogue:
                lines += 3;
                break;
            case ActionType.ChoiceDialogue:
                lines += 7;
                break;
        }
        return EditorGUIUtility.singleLineHeight * lines + EditorGUIUtility.standardVerticalSpacing * (lines - 1);
    }
}
#endif
