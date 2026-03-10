using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;

[CreateAssetMenu(menuName = "VN/Dialogue Event")]
public class DialogueEventData : ScriptableObject
{
    public string EventName;
    public ExecutionType executionType = ExecutionType.Repeated;
    public bool initiallyUnlocked = false;

    [Range(0, 100)]
    [Tooltip("이벤트 선택 가중치. 높을수록 선택될 확률이 높음")]
    public int weight = 10;

    public List<DialogueEventData> nextEventDatas = new List<DialogueEventData>();
    public List<DialogueConditionData> conditionDatas;
    public List<DialogueActionData> actionDatas;
}

public class DialogueEvent : MonoBehaviour
{
    public DialogueEventData Data { get; private set; }
    private int currentActionIndex;
    private List<DialogueAction> runtimeActions;
    private List<DialogueCondition> runtimeConditions;
    private Coroutine currentCoroutine;
    private bool hasBeenExecuted = false;
    public bool IsUnlocked { get; private set; }

    public void Initialize(DialogueEventData data)
    {
        Data = data;
        currentActionIndex = 0;
        IsUnlocked = data.initiallyUnlocked;
        runtimeActions = new List<DialogueAction>();
        if (data.actionDatas != null)
        {
            foreach (var actionData in data.actionDatas)
            {
                runtimeActions.Add(new DialogueAction(actionData));
            }
        }
        runtimeConditions = new List<DialogueCondition>();
        if (data.conditionDatas != null)
        {
            foreach (var conditionData in data.conditionDatas)
            {
                var condition = new DialogueCondition();
                condition.Initialize(conditionData);
                runtimeConditions.Add(condition);
            }
        }
    }

    public bool CanExecute()
    {
        return IsUnlocked && (Data.executionType == ExecutionType.Repeated || !hasBeenExecuted) && (runtimeConditions == null || runtimeConditions.All(c => c.Check()));
    }

    public void Unlock()
    {
        IsUnlocked = true;
    }

    public bool IsFinished()
    {
        return currentActionIndex >= runtimeActions.Count || runtimeActions.All(a => a.IsFinished());
    }

    public void Execute()
    {
        // Stop any existing coroutine before starting a new one
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
        
        // Open UI_Dialogue when event starts
        //DialogueManager.Instance.dialogueUI.gameObject.SetActive(true);

        currentActionIndex = 0;
        currentCoroutine = StartCoroutine(CoExecuteAllActions());
    }

    private IEnumerator CoExecuteAllActions()
    {
        while (currentActionIndex < runtimeActions.Count)
        {
            runtimeActions[currentActionIndex].Execute();
            
            // Wait for the action to finish if it's not instantaneous
            while (!runtimeActions[currentActionIndex].IsFinished())
            {
                yield return null;
            }
            
            currentActionIndex++;
        }
        
        // Clear the coroutine reference when finished
        currentCoroutine = null;

        // Mark as executed
        hasBeenExecuted = true;

        // Unlock next events if exists
        if (Data.nextEventDatas != null)
        {
            foreach (var nextEvent in Data.nextEventDatas)
            {
                if (nextEvent != null)
                {
                    DialogueManager.Instance.UnlockEvent(nextEvent);
                }
            }
        }

        // Close UI_Dialogue when all actions are finished
        //DialogueManager.Instance.dialogueUI.gameObject.SetActive(false);
    }

    public void Reset()
    {
        // Stop any running coroutine
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
        
        currentActionIndex = 0;
        // Reset execution flag only for Repeated type
        if (Data.executionType == ExecutionType.Repeated)
        {
            hasBeenExecuted = false;
        }
        // Recreate runtime actions to reset state
        runtimeActions.Clear();
        if (Data.actionDatas != null)
        {
            foreach (var actionData in Data.actionDatas)
            {
                runtimeActions.Add(new DialogueAction(actionData));
            }
        }
        // Reset conditions
        runtimeConditions.Clear();
        if (Data.conditionDatas != null)
        {
            foreach (var conditionData in Data.conditionDatas)
            {
                var condition = new DialogueCondition();
                condition.Initialize(conditionData);
                runtimeConditions.Add(condition);
            }
        }
    }

    public void SetDialogueActionFinished()
    {
        if (currentActionIndex < runtimeActions.Count)
        {
            runtimeActions[currentActionIndex].SetDialogueFinished();
        }

        foreach (var cond in runtimeConditions)
        {
            if (cond != null && cond.Data != null && cond.Data.type == ConditionType.CoolTime)
            {
                cond.RecordExecutionTime();
            }
        }
    }
}
