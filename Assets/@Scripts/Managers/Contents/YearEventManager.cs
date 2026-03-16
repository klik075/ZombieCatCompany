using Spine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class YearEventManager : Singleton<YearEventManager>
{
    private const string YEAR_EVENT_FOLDER_PATH = "YearEvents";
    private const string DISPATCH_RESULT_FOLDER_PATH = "DispatchResults";

    private Dictionary<YearEventData, YearEvent> _yearEventInstancesDict;
    private Dictionary<YearEventData, YearEvent> _dispatchResultDict;

    public List<string> CurrentDeathMembers { get; private set; } = new List<string>();

    // 마지막으로 실행된 이벤트 정보 (다시보기용)
    public SavedEventInfo LastYearEvent { get; private set; }
    public SavedEventInfo LastDispatchResult { get; private set; }

    private YearEvent _currentYearEvent;
    private YearEvent _currentDispatchResult;
    private bool _isSequenceRunning = false;
    private bool _hasCompletedNightSequence = false; // 현재 밤 시퀀스 완료 여부

    #region 초기화

    protected void Awake()
    {
        InitializeYearEvents();
        InitializeDispatchResults();
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
            if (eventData.category == EventCategory.YearEvent)
            {
                YearEvent yearEvent = new YearEvent(eventData);
                _yearEventInstancesDict[eventData] = yearEvent;
            }
        }

        Debug.Log($"[YearEventManager] Initialized {_yearEventInstancesDict.Count} year events");
    }
    private void InitializeDispatchResults()
    {
        _dispatchResultDict = new Dictionary<YearEventData, YearEvent>();

        YearEventData[] dispatchResultDatas = ResourceManager.Instance.GetAllFromPath<YearEventData>(DISPATCH_RESULT_FOLDER_PATH);

        if (dispatchResultDatas == null || dispatchResultDatas.Length == 0)
        {
            Debug.LogWarning($"[YearEventManager] No dispatch results found in path: {DISPATCH_RESULT_FOLDER_PATH}");
            return;
        }

        foreach (var eventData in dispatchResultDatas)
        {
            if (eventData.category == EventCategory.DispatchResult)
            {
                YearEvent dispatchResult = new YearEvent(eventData);
                _dispatchResultDict[eventData] = dispatchResult;
            }
        }

        Debug.Log($"[YearEventManager] Initialized {_dispatchResultDict.Count} dispatch results");
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
        
        // 밤 시퀀스 완료 여부 저장
        saveData.hasCompletedNightSequence = _hasCompletedNightSequence;

        saveData.lastYearEvent = LastYearEvent;
        saveData.lastDispatchResult = LastDispatchResult;

        foreach (var kvp in _yearEventInstancesDict)
        {
            var eventData = kvp.Key;
            var yearEvent = kvp.Value;

            if (yearEvent.Data.executionType == ExecutionType.Once && yearEvent.IsFinished())
            {
                saveData.executedOnceEvents.Add(eventData.name);
            }

            if (yearEvent.IsUnlocked && !yearEvent.Data.initiallyUnlocked)
            {
                saveData.unlockedEvents.Add(eventData.name);
            }
        }

        foreach (var kvp in _dispatchResultDict)
        {
            var eventData = kvp.Key;
            var dispatchResult = kvp.Value;

            if (dispatchResult.Data.executionType == ExecutionType.Once && dispatchResult.IsFinished())
            {
                saveData.executedOnceEvents.Add(eventData.name);
            }

            if (dispatchResult.IsUnlocked && !dispatchResult.Data.initiallyUnlocked)
            {
                saveData.unlockedEvents.Add(eventData.name);
            }
        }

        Debug.Log($"[YearEventManager] Saved {saveData.executedOnceEvents.Count} executed events, " +
                  $"{saveData.unlockedEvents.Count} unlocked events, " +
                  $"hasCompletedNightSequence: {saveData.hasCompletedNightSequence}");
    }

    public void LoadEventStates(GameData gameData)
    {
        if (gameData.YearEventData == null)
        {
            Debug.Log("[YearEventManager] No saved event data found.");
            _hasCompletedNightSequence = false;
            return;
        }

        var saveData = gameData.YearEventData;
        
        // 밤 시퀀스 완료 여부 로드
        _hasCompletedNightSequence = saveData.hasCompletedNightSequence;

        LastYearEvent = saveData.lastYearEvent;
        LastDispatchResult = saveData.lastDispatchResult;

        // YearEvent 로드
        foreach (var eventName in saveData.executedOnceEvents)
        {
            var eventData = _yearEventInstancesDict.Keys.FirstOrDefault(e => e.name == eventName);
            if (eventData != null && _yearEventInstancesDict.TryGetValue(eventData, out var yearEvent))
            {
                yearEvent.MarkAsExecuted();
                Debug.Log($"[YearEventManager] Restored executed event: {eventData.name}");
            }
        }

        foreach (var eventName in saveData.unlockedEvents)
        {
            var eventData = _yearEventInstancesDict.Keys.FirstOrDefault(e => e.name == eventName);
            if (eventData != null && _yearEventInstancesDict.TryGetValue(eventData, out var yearEvent))
            {
                yearEvent.Unlock();
                Debug.Log($"[YearEventManager] Restored unlocked event: {eventData.name}");
            }
        }

        // DispatchResult 로드
        foreach (var eventName in saveData.executedOnceEvents)
        {
            var eventData = _dispatchResultDict.Keys.FirstOrDefault(e => e.name == eventName);
            if (eventData != null && _dispatchResultDict.TryGetValue(eventData, out var dispatchResult))
            {
                dispatchResult.MarkAsExecuted();
                Debug.Log($"[YearEventManager] Restored executed dispatch result: {eventData.name}");
            }
        }

        foreach (var eventName in saveData.unlockedEvents)
        {
            var eventData = _dispatchResultDict.Keys.FirstOrDefault(e => e.name == eventName);
            if (eventData != null && _dispatchResultDict.TryGetValue(eventData, out var dispatchResult))
            {
                dispatchResult.Unlock();
                Debug.Log($"[YearEventManager] Restored unlocked dispatch result: {eventData.name}");
            }
        }

        Debug.Log($"[YearEventManager] Loaded {saveData.executedOnceEvents.Count} executed events, " +
                  $"{saveData.unlockedEvents.Count} unlocked events, " +
                  $"hasCompletedNightSequence: {_hasCompletedNightSequence}");
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

    #endregion

    #region 파견 결과 선택 로직

    // 사용 가능한 파견 결과 가져오기
    private List<YearEvent> GetAvailableDispatchResults()
    {
        return _dispatchResultDict.Values.Where(e => e.CanExecute()).ToList();
    }

    // 랜덤 파견 결과 선택
    private YearEvent SelectRandomDispatchResult()
    {
        List<YearEvent> availableResults = GetAvailableDispatchResults();

        if (availableResults.Count == 0)
        {
            Debug.LogWarning("[YearEventManager] No available dispatch results");
            return null;
        }

        if (availableResults.Count == 1)
        {
            return availableResults[0];
        }

        return SelectByWeight(availableResults);
    }

    #endregion

    #region 공통 선택 로직

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
        // 이미 완료했으면 생략
        if (_hasCompletedNightSequence)
        {
            Debug.Log("[YearEventManager] Night sequence already completed this cycle. Skipping.");
            return;
        }

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

        ResetRepeatableEventsForNewCycle();

        CurrentDeathMembers = MemberManager.Instance.IncreaseAllMembersHunger();

        yield return CoroutineManager.Instance.Run(CoShowYearEvent()); //연차 이벤트

        if (currentYear > 1)
        {
            if (HasDispatchResult())
            {
                yield return CoroutineManager.Instance.Run(CoShowDispatchResult()); //파견 결과
            }

            yield return CoroutineManager.Instance.Run(CoShowDispatchSelection()); //파견 선택
            yield return CoroutineManager.Instance.Run(CoShowFoodDistribution()); //식량 배급
        }

        _isSequenceRunning = false;
        
        // 시퀀스 완료 플래그 설정
        _hasCompletedNightSequence = true;

        CurrentDeathMembers.Clear();

        // 자동 저장
        SaveManager.Instance.SaveGameData();
        
        Debug.Log("[YearEventManager] Night sequence completed and auto-saved");
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
        
        // ShowEventPopup 액션이 실행될 때 자동으로 저장됨
        yield return CoroutineManager.Instance.Run(selectedEvent.Execute());

        Debug.Log($"[YearEventManager] Year event completed: {selectedEvent.Data.name}");
    }

    #endregion

    #region 파견 시스템

    private bool HasDispatchResult()
    {
        return MemberManager.Instance.HasDispatchedMember();
    }

    private System.Collections.IEnumerator CoShowDispatchResult()
    {
        YearEvent selectedResult = SelectRandomDispatchResult();

        if (selectedResult == null)
        {
            Debug.LogWarning("[YearEventManager] No dispatch result selected!");
            yield break;
        }

        Debug.Log($"[YearEventManager] Executing dispatch result: {selectedResult.Data.name}");

        _currentDispatchResult = selectedResult;
        
        // ShowEventPopup 액션이 실행될 때 자동으로 저장됨
        yield return CoroutineManager.Instance.Run(selectedResult.Execute());

        Debug.Log($"[YearEventManager] Dispatch result completed: {selectedResult.Data.name}");

        MemberManager.Instance.CompleteDispatch();

        while(UIManager.Instance.PopupCount >= 1)
        {
            yield return null;
        }
    }

    private System.Collections.IEnumerator CoShowDispatchSelection()
    {
        while (true)
        {
            if (MemberManager.Instance.HasDispatchedMember())
            {
                Debug.Log("[YearEventManager] Already has dispatched member, skipping dispatch selection");
                yield break;
            }

            if (MemberManager.Instance.MemberCount <= 1)
            {
                Debug.Log("[YearEventManager] No members available for dispatch");
                yield break;
            }

            bool? messageChoice = null;

            UI_MessagePopup messagePopup = UIManager.Instance.ShowPopupUI<UI_MessagePopup>();
            messagePopup.SetInfo(
                MessageManager.Instance.GetMessageScript(EMessageType.SelectDispatch).Contents,
                null,
                okAction: () => { messageChoice = true; },   // Yes
                noAction: () => { messageChoice = false; }   // No
            );

            while (messageChoice == null)
            {
                yield return null;
            }

            // No 선택 시 종료 → CoShowFoodDistribution으로 진행
            if (messageChoice == false)
            {
                Debug.Log("[YearEventManager] User declined dispatch");
                yield break;
            }

            // Step 2: Yes 선택 → MemberSelectionPopup 열기
            bool? selectionResult = null;  // null: 대기, true: 파견 완료, false: 뒤로 버튼

            UI_MemberSelectionPopup selectionPopup = UIManager.Instance.ShowPopupUI<UI_MemberSelectionPopup>();
            selectionPopup.SetInfo(
                EMemberSelectionType.Dispatch,
                onComplete: () =>{ selectionResult = true; },
                onCancel: () => { selectionResult = false; },
                index: 1  // 기본적으로 1번 멤버 선택 (보스 제외)
            );

            // MemberSelectionPopup 결과 대기
            while (selectionResult == null)
            {
                yield return null;
            }

            // 파견 완료 → 루프 탈출하여 다음 코루틴으로 진행
            if (selectionResult == true)
            {
                
                Debug.Log("[YearEventManager] Exiting dispatch selection, proceeding to food distribution");
                yield break;
            }
        }
    }

    private System.Collections.IEnumerator CoShowFoodDistribution()
    {
        bool? messageChoice = null;

        UI_ChatPopup chatPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
        chatPopup.SetInfo(
            MemberManager.Instance.MainCharacter.CurrentMemberData.EmployeeID,
            MessageManager.Instance.GetMessageScript(EMessageType.FoodRationing).Contents,
            null,
            () => { messageChoice = true; });

        while (messageChoice == null)
        {
            yield return null;
        }

        bool? rationCompleted = null;
        UI_FoodRationPopup rationPopup = UIManager.Instance.ShowPopupUI<UI_FoodRationPopup>();
        rationPopup.OnClosed(() => { rationCompleted = true; });

        while (rationCompleted == null)
        {
            yield return null;
        }
    }

    #endregion

    #region 공개 메서드
    public void SetLastYearEvent(SavedEventInfo eventInfo)
    {
        // 죽은 멤버 정보 추가
        if (eventInfo.deathMembers == null)
        {
            eventInfo.deathMembers = new List<string>();
        }

        // 현재 죽은 멤버들을 복사하여 저장
        eventInfo.deathMembers.Clear();
        if (CurrentDeathMembers != null && CurrentDeathMembers.Count > 0)
        {
            eventInfo.deathMembers.AddRange(CurrentDeathMembers);
        }

        LastYearEvent = eventInfo;
        Debug.Log($"[YearEventManager] Saved year event info");
    }

    /// <summary>
    /// 마지막 파견 결과 정보 설정 (ShowEventPopup 실행 시 호출)
    /// </summary>
    public void SetLastDispatchResult(SavedEventInfo eventInfo)
    {
        // 파견 결과는 죽은 멤버 정보가 필요 없음
        if (eventInfo.deathMembers == null)
        {
            eventInfo.deathMembers = new List<string>();
        }

        LastDispatchResult = eventInfo;
        Debug.Log($"[YearEventManager] Saved dispatch result info");
    }
    public bool IsSequenceRunning()
    {
        return _isSequenceRunning;
    }

    /// <summary>
    /// 특정 YearEventData를 언락
    /// </summary>
    public void UnlockEvent(YearEventData eventData)
    {
        // YearEvent 체크
        if (_yearEventInstancesDict.TryGetValue(eventData, out var yearEvent))
        {
            yearEvent.Unlock();
            Debug.Log($"[YearEventManager] Unlocked event: {eventData.name}");
            return;
        }

        // DispatchResult 체크
        if (_dispatchResultDict.TryGetValue(eventData, out var dispatchResult))
        {
            dispatchResult.Unlock();
            Debug.Log($"[YearEventManager] Unlocked dispatch result: {eventData.name}");
            return;
        }

        Debug.LogWarning($"[YearEventManager] Event not found: {eventData.name}");
    }

    /// <summary>
    /// 특정 이벤트를 실행했는지 확인
    /// </summary>
    public bool HasExecutedEvent(YearEventData eventData)
    {
        // YearEvent 체크
        if (_yearEventInstancesDict.TryGetValue(eventData, out var yearEvent))
        {
            return yearEvent.IsFinished();
        }

        // DispatchResult 체크
        if (_dispatchResultDict.TryGetValue(eventData, out var dispatchResult))
        {
            return dispatchResult.IsFinished();
        }

        return false;
    }

    #endregion

    #region 씬 전환 시 초기화

    /// <summary>
    /// 씬 전환 시 Repeated 이벤트 초기화 (밤 시작 시 호출)
    /// </summary>
    public void ResetRepeatableEventsForNewCycle()
    {
        foreach (var yearEvent in _yearEventInstancesDict.Values)
        {
            if (yearEvent.Data.executionType == ExecutionType.Repeated)
            {
                yearEvent.Reset();
            }
        }

        foreach (var dispatchResult in _dispatchResultDict.Values)
        {
            if (dispatchResult.Data.executionType == ExecutionType.Repeated)
            {
                dispatchResult.Reset();
            }
        }

        Debug.Log("[YearEventManager] Reset all repeatable events for new cycle");
    }

    /// <summary>
    /// 씬 전환 시 밤 시퀀스 플래그 초기화
    /// </summary>
    public void ResetNightSequenceFlag()
    {
        _hasCompletedNightSequence = false;
        Debug.Log("[YearEventManager] Night sequence flag reset for new cycle");
    }

    /// <summary>
    /// 밤 시퀀스 완료 여부 확인
    /// </summary>
    public bool HasCompletedNightSequence()
    {
        return _hasCompletedNightSequence;
    }

    #endregion
}
