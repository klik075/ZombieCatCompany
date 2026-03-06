using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

/// <summary>
/// 연차 이벤트 시스템 (DialogueManager 패턴 사용)
/// </summary>
public class YearEventManager : Singleton<YearEventManager>
{
    [SerializeField] private List<DialogueEventData> yearEventDatas;  // ScriptableObject로 관리
    
    private List<DialogueEvent> yearEventInstances;
    private DialogueEvent currentYearEvent;
    private bool _isSequenceRunning = false;

    #region 초기화

    private void Awake()
    {
        // DialogueEvent 인스턴스 생성
        yearEventInstances = new List<DialogueEvent>();
        
        if (yearEventDatas != null)
        {
            foreach (var eventData in yearEventDatas)
            {
                GameObject eventGO = new GameObject($"YearEvent_{eventData.EventName}");
                eventGO.transform.SetParent(transform);
                DialogueEvent dialogueEvent = eventGO.AddComponent<DialogueEvent>();
                dialogueEvent.Initialize(eventData);
                yearEventInstances.Add(dialogueEvent);
            }
        }
        
        Debug.Log($"[YearEventManager] Initialized {yearEventInstances.Count} year events");
    }

    #endregion

    #region 이벤트 선택 로직

    /// <summary>
    /// 현재 연차에 발생 가능한 이벤트 가져오기
    /// </summary>
    private List<DialogueEvent> GetAvailableYearEvents()
    {
        return yearEventInstances.Where(e => e.CanExecute()).ToList();
    }

    /// <summary>
    /// 가중치 기반 랜덤 이벤트 선택 (DialogueEventData에 Weight 추가 필요)
    /// </summary>
    private DialogueEvent SelectRandomYearEvent()
    {
        List<DialogueEvent> availableEvents = GetAvailableYearEvents();

        if (availableEvents.Count == 0)
        {
            Debug.LogWarning("[YearEventManager] No available year events! Using default.");
            return GetDefaultYearEvent();
        }

        // 1개면 바로 반환
        if (availableEvents.Count == 1)
            return availableEvents[0];

        // 가중치가 있다면 가중치 기반 선택 (추후 확장)
        // 현재는 랜덤 선택
        int randomIndex = Random.Range(0, availableEvents.Count);
        return availableEvents[randomIndex];
    }

    /// <summary>
    /// 기본 연차 이벤트 (발생 가능한 이벤트가 없을 때)
    /// </summary>
    private DialogueEvent GetDefaultYearEvent()
    {
        // 기본 이벤트 반환 (1년차 이벤트 등)
        return yearEventInstances.FirstOrDefault(e => e.Data.EventName == "Year1_Start");
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

        // 연차 이벤트 실행 (Dialogue 시스템 활용)
        yield return CoroutineManager.Instance.Run(CoExecuteYearEvent());

        // 1년차면 식량 배급만
        if (currentYear == 1)
        {
            Debug.Log("[YearEventManager] Year 1 - Skipping dispatch");
            yield return CoroutineManager.Instance.Run(CoShowFoodDistribution());
            _isSequenceRunning = false;
            yield break;
        }

        // 파견 결과 (2년차 이상)
        if (HasDispatchResult())
        {
            yield return CoroutineManager.Instance.Run(CoShowDispatchResult());
        }

        // 파견 보내기
        yield return CoroutineManager.Instance.Run(CoShowDispatchSelection());

        // 식량 배급
        yield return CoroutineManager.Instance.Run(CoShowFoodDistribution());

        _isSequenceRunning = false;
        Debug.Log("[YearEventManager] Night sequence completed");
    }

    /// <summary>
    /// 연차 이벤트 실행 (Dialogue 시스템 활용)
    /// </summary>
    private System.Collections.IEnumerator CoExecuteYearEvent()
    {
        // 조건 만족하는 이벤트 중 랜덤 선택
        DialogueEvent selectedEvent = SelectRandomYearEvent();

        if (selectedEvent == null)
        {
            Debug.LogWarning("[YearEventManager] No event selected!");
            yield break;
        }

        Debug.Log($"[YearEventManager] Executing year event: {selectedEvent.Data.EventName}");

        // DialogueEvent 실행 (자동으로 조건 체크 → 액션 실행 → 다음 이벤트 언락)
        currentYearEvent = selectedEvent;
        selectedEvent.Execute();

        // 이벤트 완료 대기
        while (!selectedEvent.IsFinished())
        {
            yield return null;
        }

        Debug.Log($"[YearEventManager] Year event '{selectedEvent.Data.EventName}' completed");
    }

    #endregion

    #region 파견 시스템 (TODO)

    private bool HasDispatchResult()
    {
        //var dispatchData = GameManager.Instance.MyNightData.DispatchData;
        //return dispatchData.DispatchedMemberIndex >= 0 && 
        //       !dispatchData.IsReturned && 
        //       GameManager.Instance.Year > dispatchData.DispatchYear;

        return true;//Placeholder
    }

    private System.Collections.IEnumerator CoShowDispatchResult()
    {
        Debug.Log("[YearEventManager] Showing dispatch result");
        // TODO: Dialogue 이벤트로 구현
        yield return null;
    }

    private System.Collections.IEnumerator CoShowDispatchSelection()
    {
        Debug.Log("[YearEventManager] Showing dispatch selection");
        // TODO: Dialogue 이벤트로 구현
        yield return null;
    }

    private System.Collections.IEnumerator CoShowFoodDistribution()
    {
        Debug.Log("[YearEventManager] Showing food distribution");
        // TODO: Dialogue 이벤트로 구현
        yield return null;
    }

    #endregion

    #region 공개 메서드

    public bool IsSequenceRunning()
    {
        return _isSequenceRunning;
    }

    /// <summary>
    /// 특정 이벤트를 본 적이 있는지 확인
    /// </summary>
    public bool HasViewedEvent(int eventID)
    {
        // DialogueEvent의 hasBeenExecuted를 활용하거나
        // UserData에 별도 저장
        return false; // TODO: 구현
    }

    #endregion
}
