using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "YearEvent/Year Event Data")]
public class YearEventData : ScriptableObject
{
    public ExecutionType executionType = ExecutionType.Repeated;
    public bool initiallyUnlocked = false;

    [Range(0, 100)]
    [Tooltip("이벤트 선택 가중치. 높을수록 선택될 확률이 높음")]
    public int weight = 10;

    public List<YearEventData> nextEventDatas = new List<YearEventData>();
    public List<YearEventConditionData> conditionDatas;
    public List<YearEventActionData> actionDatas;
}
public class YearEvent
{
    public YearEventData Data { get; private set; }
    private int currentActionIndex;
    private List<YearEventAction> runtimeActions;
    private List<YearEventCondition> runtimeConditions;
    private bool hasBeenExecuted = false;
    public bool IsUnlocked { get; private set; }

    public YearEvent(YearEventData data)
    {
        Data = data;
        currentActionIndex = 0;
        IsUnlocked = data.initiallyUnlocked;

        // Runtime Actions 생성
        runtimeActions = new List<YearEventAction>();
        if (data.actionDatas != null)
        {
            foreach (var actionData in data.actionDatas)
            {
                runtimeActions.Add(new YearEventAction(actionData));
            }
        }

        // Runtime Conditions 생성
        runtimeConditions = new List<YearEventCondition>();
        if (data.conditionDatas != null)
        {
            foreach (var conditionData in data.conditionDatas)
            {
                var condition = new YearEventCondition();
                condition.Initialize(conditionData);
                runtimeConditions.Add(condition);
            }
        }
    }

    public bool CanExecute()
    {
        return IsUnlocked &&
               (Data.executionType == ExecutionType.Repeated || !hasBeenExecuted) &&
               (runtimeConditions == null || runtimeConditions.All(c => c.Check()));
    }

    public void Unlock()
    {
        IsUnlocked = true;
    }
    public void MarkAsExecuted()
    {
        hasBeenExecuted = true;

        currentActionIndex = runtimeActions.Count;

        foreach (var action in runtimeActions)
        {
            action.SetFinished();
        }
    }
    public bool IsFinished()
    {
        if (hasBeenExecuted)
            return true;

        return currentActionIndex >= runtimeActions.Count || runtimeActions.All(a => a.IsFinished());
    }

    public IEnumerator Execute()
    {
        currentActionIndex = 0;

        while (currentActionIndex < runtimeActions.Count)
        {
            runtimeActions[currentActionIndex].Execute();

            while (!runtimeActions[currentActionIndex].IsFinished())
            {
                yield return null;
            }

            currentActionIndex++;
        }

        hasBeenExecuted = true;

        // Unlock next events
        if (Data.nextEventDatas != null)
        {
            foreach (var nextEvent in Data.nextEventDatas)
            {
                if (nextEvent != null)
                {
                    YearEventManager.Instance.UnlockEvent(nextEvent);
                }
            }
        }
    }

    public void Reset()
    {
        currentActionIndex = 0;
        if (Data.executionType == ExecutionType.Repeated)
        {
            hasBeenExecuted = false;
        }

        // Reset actions
        runtimeActions.Clear();
        if (Data.actionDatas != null)
        {
            foreach (var actionData in Data.actionDatas)
            {
                runtimeActions.Add(new YearEventAction(actionData));
            }
        }
    }
}
[Serializable]
public class YearEventSaveData
{
    public List<string> executedOnceEvents = new List<string>(); // 실행 완료된 Once 타입 이벤트 이름 목록
    public List<string> unlockedEvents = new List<string>(); // 언락된 이벤트 이름 목록 (조건 달성으로 해금된 이벤트)
}
public enum ExecutionType
{
    Once,
    Repeated
}
