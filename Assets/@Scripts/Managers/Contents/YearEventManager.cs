using Spine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class YearEventManager : Singleton<YearEventManager>
{
    private const string YEAR_EVENT_FOLDER_PATH = "YearEvents";

    private Dictionary<YearEventData, YearEvent> _yearEventInstancesDict;
    private YearEvent _currentYearEvent;
    private bool _isSequenceRunning = false;

    #region 초기화

    protected void Awake()
    {
        InitializeYearEvents();
    }

    private void InitializeYearEvents()
    {
        _yearEventInstancesDict = new Dictionary<YearEventData, YearEvent>();

        YearEventData[] yearEventDatas = ResourceManager.Instance.GetAllFromPath<YearEventData>(YEAR_EVENT_FOLDER_PATH);

        if (yearEventDatas == null || yearEventDatas.Length == 0)
        {
            Debug.LogWarning($"[YearEventManager] No year events found in path: {YEAR_EVENT_FOLDER_PATH}");
            return;
        }

        foreach (var eventData in yearEventDatas)
        {
            YearEvent yearEvent = new YearEvent(eventData);
            _yearEventInstancesDict[eventData] = yearEvent;
        }

        Debug.Log($"[YearEventManager] Initialized {_yearEventInstancesDict.Count} year events");
    }

    #endregion

    #region 저장/로드
    public void SaveEventStates(GameData gameData)
    {
        if (gameData.YearEventData == null)
        {
            gameData.YearEventData = new YearEventSaveData();
        }

        var saveData = gameData.YearEventData;
        saveData.executedOnceEvents.Clear();
        saveData.unlockedEvents.Clear();

        foreach (var kvp in _yearEventInstancesDict)
        {
            var eventData = kvp.Key;
            var yearEvent = kvp.Value;

            // Once 타입 이벤트 중 실행된 것 저장
            if (yearEvent.Data.executionType == ExecutionType.Once && yearEvent.IsFinished())
            {
                saveData.executedOnceEvents.Add(eventData.name);
            }

            // 언락된 이벤트 저장 (initiallyUnlocked가 아닌데 언락된 경우)
            if (yearEvent.IsUnlocked && !yearEvent.Data.initiallyUnlocked)
            {
                saveData.unlockedEvents.Add(eventData.name);
            }
        }

        Debug.Log($"[YearEventManager] Saved {saveData.executedOnceEvents.Count} executed events, " +
                  $"{saveData.unlockedEvents.Count} unlocked events");
    }

    public void LoadEventStates(GameData gameData)
    {
        if (gameData.YearEventData == null)
        {
            Debug.Log("[YearEventManager] No saved event data found.");
            return;
        }

        var saveData = gameData.YearEventData;

        // 실행된 Once 이벤트 복원
        foreach (var eventName in saveData.executedOnceEvents)
        {
            var eventData = _yearEventInstancesDict.Keys.FirstOrDefault(e => e.name == eventName);
            if (eventData != null && _yearEventInstancesDict.TryGetValue(eventData, out var yearEvent))
            {
                yearEvent.MarkAsExecuted();
                Debug.Log($"[YearEventManager] Restored executed event: {eventData.name}");
            }
        }

        // 언락된 이벤트 복원
        foreach (var eventName in saveData.unlockedEvents)
        {
            var eventData = _yearEventInstancesDict.Keys.FirstOrDefault(e => e.name == eventName);
            if (eventData != null && _yearEventInstancesDict.TryGetValue(eventData, out var yearEvent))
            {
                yearEvent.Unlock();
                Debug.Log($"[YearEventManager] Restored unlocked event: {eventData.name}");
            }
        }

        Debug.Log($"[YearEventManager] Loaded {saveData.executedOnceEvents.Count} executed events, " +
                  $"{saveData.unlockedEvents.Count} unlocked events");
    }

    #endregion

    #region 이벤트 선택 로직

    private List<YearEvent> GetAvailableYearEvents()
    {
        return _yearEventInstancesDict.Values.Where(e => e.CanExecute()).ToList();
    }

    private YearEvent SelectRandomYearEvent()
    {
        List<YearEvent> availableEvents = GetAvailableYearEvents();

        if (availableEvents.Count == 0)
        {
            Debug.LogWarning("[YearEventManager] No available events");
            return null;
        }

        if (availableEvents.Count == 1)
            return availableEvents[0];

        return SelectByWeight(availableEvents);
    }

    private YearEvent SelectByWeight(List<YearEvent> events)
    {
        int totalWeight = 0;
        foreach (var evt in events)
        {
            totalWeight += Mathf.Max(1, evt.Data.weight);
        }

        int randomValue = Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        for (int i = 0; i < events.Count; i++)
        {
            int weight = Mathf.Max(1, events[i].Data.weight);
            cumulativeWeight += weight;

            if (randomValue < cumulativeWeight)
            {
                Debug.Log($"[YearEventManager] Selected '{events[i].Data.name}' " +
                         $"(weight: {weight}/{totalWeight}, roll: {randomValue})");
                return events[i];
            }
        }

        return events[events.Count - 1];
    }

    #endregion

    #region 밤 시작 시퀀스

    public void StartNightSequence()
    {
        if (_isSequenceRunning)
        {
            Debug.LogWarning("[YearEventManager] Sequence already running!");
            return;
        }

        CoroutineManager.Instance.Run(CoNightSequence());
    }

    private System.Collections.IEnumerator CoNightSequence()
    {
        _isSequenceRunning = true;
        int currentYear = GameManager.Instance.Year;

        Debug.Log($"[YearEventManager] Starting night sequence for Year {currentYear}");

        yield return CoroutineManager.Instance.Run(CoShowYearEvent());

        if (currentYear > 1)
        {
            if (HasDispatchResult())
            {
                yield return CoroutineManager.Instance.Run(CoShowDispatchResult());
            }

            yield return CoroutineManager.Instance.Run(CoShowDispatchSelection());
            yield return CoroutineManager.Instance.Run(CoShowFoodDistribution());
        }

        _isSequenceRunning = false;
        Debug.Log("[YearEventManager] Night sequence completed");
    }

    private System.Collections.IEnumerator CoShowYearEvent()
    {
        YearEvent selectedEvent = SelectRandomYearEvent();

        if (selectedEvent == null)
        {
            Debug.LogWarning("[YearEventManager] No event selected!");
            yield break;
        }

        Debug.Log($"[YearEventManager] Executing year event: {selectedEvent.Data.name}");

        _currentYearEvent = selectedEvent;
        
        // YearEvent.Execute()는 코루틴이므로 직접 실행
        yield return CoroutineManager.Instance.Run(selectedEvent.Execute());

        Debug.Log($"[YearEventManager] Year event completed: {selectedEvent.Data.name}");
    }

    #endregion

    #region 파견 시스템

    private bool HasDispatchResult()
    {
        return false;
    }

    private System.Collections.IEnumerator CoShowDispatchResult()
    {
        Debug.Log("[YearEventManager] Showing dispatch result");
        yield return null;
    }

    private System.Collections.IEnumerator CoShowDispatchSelection()
    {
        Debug.Log("[YearEventManager] Showing dispatch selection");
        yield return null;
    }

    private System.Collections.IEnumerator CoShowFoodDistribution()
    {
        Debug.Log("[YearEventManager] Showing food distribution");
        yield return null;
    }

    #endregion

    #region 공개 메서드

    public bool IsSequenceRunning()
    {
        return _isSequenceRunning;
    }

    /// <summary>
    /// 특정 YearEventData를 언락
    /// </summary>
    public void UnlockEvent(YearEventData eventData)
    {
        if (_yearEventInstancesDict.TryGetValue(eventData, out var yearEvent))
        {
            yearEvent.Unlock();
            Debug.Log($"[YearEventManager] Unlocked event: {eventData.name}");
        }
        else
        {
            Debug.LogWarning($"[YearEventManager] Event not found: {eventData.name}");
        }
    }

    /// <summary>
    /// 모든 Repeatable 이벤트 초기화
    /// </summary>
    public void ResetRepeatableEvents()
    {
        foreach (var yearEvent in _yearEventInstancesDict.Values)
        {
            yearEvent.Reset();
        }
        Debug.Log("[YearEventManager] Reset all repeatable events");
    }

    /// <summary>
    /// 특정 이벤트를 실행했는지 확인
    /// </summary>
    public bool HasExecutedEvent(YearEventData eventData)
    {
        if (_yearEventInstancesDict.TryGetValue(eventData, out var yearEvent))
        {
            return yearEvent.IsFinished();
        }
        return false;
    }

    #endregion
}
