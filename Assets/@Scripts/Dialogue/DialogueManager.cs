using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    public UI_Dialogue dialogueUI;
    [SerializeField]
    private List<DialogueEventData> eventDatas;
    [SerializeField]
    private List<CharacterData> characterDataList;

    public DialogueEvent CurrentEvent { get; private set; }
    private List<DialogueEvent> eventInstances;
	private Coroutine coCheckEvent;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Pre-create DialogueEvent instances
        eventInstances = new List<DialogueEvent>();
        foreach (var eventData in eventDatas)
        {
            GameObject eventGO = new GameObject($"DialogueEvent_{eventData.EventName}");
            eventGO.transform.SetParent(transform);
            DialogueEvent dialogueEvent = eventGO.AddComponent<DialogueEvent>();
            dialogueEvent.Initialize(eventData);
            eventInstances.Add(dialogueEvent);
        }
    }

    public void StartDialogueEvent(DialogueEventData dialogueEventData, bool checkCanExecute = true)
    {
        if (dialogueEventData == null)
        {
            Debug.LogWarning("DialogueEventData is null");
            return;
        }

        var dialogueEvent = eventInstances.FirstOrDefault(e => e.Data == dialogueEventData);
        if (dialogueEvent == null)
        {
            Debug.LogWarning($"DialogueEvent instance for '{dialogueEventData.EventName}' not found");
            return;
        }

        StartDialogueEvent(dialogueEvent, checkCanExecute);
    }

    public void StartDialogueEvent(string eventName, bool checkCanExecute = true)
    {
        var eventData = eventDatas.FirstOrDefault(e => e.EventName == eventName);
        if (eventData == null)
        {
            Debug.LogWarning($"DialogueEventData with name '{eventName}' not found");
            return;
        }
        StartDialogueEvent(eventData, checkCanExecute);
    }

    public void StartDialogueEvent(DialogueEvent dialogueEvent, bool checkCanExecute = true)
    {
        if (dialogueEvent == null)
        {
            Debug.LogWarning("DialogueEvent is null");
            return;
        }

        if (checkCanExecute && !dialogueEvent.CanExecute())
        {
            Debug.LogWarning($"DialogueEvent '{dialogueEvent.Data.EventName}' cannot execute - conditions not met");
            return;
        }

        CurrentEvent = dialogueEvent;
        CurrentEvent.Execute();
    }

    public void ShowDialogue(CharacterId characterId, CharacterState characterState, string dialogueText)
    {
        var characterData = GetCharacterData(characterId);
        if (characterData == null)
        {
            Debug.LogWarning($"CharacterData not found for ID: {characterId}");
            return;
        }

        var sprite = GetCharacterSprite(characterData, characterState);
        dialogueUI.SetDialogue(characterData.characterName, dialogueText, sprite);
    }

    public void ShowChoiceDialogue(CharacterId characterId, CharacterState characterState, string dialogueText,
        string leftChoiceText, string rightChoiceText, 
        DialogueEventData leftChoiceEvent, DialogueEventData rightChoiceEvent)
    {
        var characterData = GetCharacterData(characterId);
        if (characterData == null)
        {
            Debug.LogWarning($"CharacterData not found for ID: {characterId}");
            return;
        }

        var sprite = GetCharacterSprite(characterData, characterState);
        dialogueUI.SetChoiceDialogue(characterData.characterName, dialogueText, sprite,
            leftChoiceText, rightChoiceText,
            () => OnChoiceSelected(leftChoiceEvent),
            () => OnChoiceSelected(rightChoiceEvent));
    }

    private void OnChoiceSelected(DialogueEventData choiceEvent)
    {
        if (choiceEvent != null)
        {
            UnlockEvent(choiceEvent);
        }

        CurrentEvent?.SetDialogueActionFinished();
    }

    public void OnDialogueFinished()
    {
        // Notify current dialogue event that dialogue action is finished
        CurrentEvent?.SetDialogueActionFinished();
    }

    private CharacterData GetCharacterData(CharacterId characterId)
    {
        return characterDataList?.FirstOrDefault(data => data.characterId == characterId);
    }

    private Sprite GetCharacterSprite(CharacterData characterData, CharacterState characterState)
    {
        var expression = characterData.expressions?.FirstOrDefault(exp => exp.characterState == characterState);
        return expression?.sprite;
    }

    public bool IsCurrentEventFinished()
    {
        return CurrentEvent == null || CurrentEvent.IsFinished();
    }

    public List<DialogueEvent> GetAvailableEvents()
    {
        return eventInstances.Where(e => e.CanExecute()).ToList();
    }

    public void UnlockEvent(DialogueEventData eventData)
    {
        var dialogueEvent = eventInstances.FirstOrDefault(e => e.Data == eventData);
        if (dialogueEvent != null)
        {
            dialogueEvent.Unlock();
        }
    }
}
