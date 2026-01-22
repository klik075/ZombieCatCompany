using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Define;

/// <summary>
/// 작업 애니메이션을 위한 데이터 전달 객체 (DTO)
/// </summary>
public class WorkAnimationData
{
    public List<EQualityType> QualitySequence { get; set; }
    public int TotalCount { get; set; }
    
    public WorkAnimationData(List<EQualityType> qualitySequence)
    {
        QualitySequence = qualitySequence ?? new List<EQualityType>();
        TotalCount = QualitySequence.Count;
    }
}

/// <summary>
/// Quality 이미지 애니메이션을 관리하는 매니저
/// UI와 독립적으로 Quality 생성, 애니메이션, 완료 처리를 담당
/// </summary>
public class QualityManager : Singleton<QualityManager>
{
    // 애니메이션 관련 상수
    private const float ANIMATION_SPEED = 500f;
    private const float MIN_ANIMATION_DURATION = 0.5f;
    private const float ARRIVAL_THRESHOLD = 5f;
    private const float QUALITY_IMAGE_SIZE = 100f;
    private const int ANIMATION_CANVAS_SORT_ORDER = 1000;

    private string _animationCanvasName = "QualityAnimationCanvas";
    private GameObject _animationCanvasObj;
    private Canvas _animationCanvas;
    
    // 애니메이션 데이터
    private List<EQualityType> _qualitySequence;
    private int _totalQualityCount = 0;
    private int _completedQualityCount = 0;

    private Action<EQualityType> _onQualityComplete;
    private Action _onAllQualitiesComplete;

    #region Public API

    /// <summary>
    /// 작업 시작 (GameDevManager에서 데이터 가져와서 애니메이션 시작)
    /// </summary>
    public void StartMainWork(
        Vector2 startPosition,
        Dictionary<EQualityType, Vector2> targetPositions,
        float interval,
        Action<EQualityType> onQualityComplete,
        Action onAllComplete)
    {
        // GameDevManager에서 작업 데이터 가져오기
        WorkAnimationData animationData = GetWorkAnimationData();
        
        if (animationData?.QualitySequence == null || animationData.QualitySequence.Count == 0)
        {
            Debug.LogWarning("No qualities to animate!");
            onAllComplete?.Invoke();
            return;
        }

        
        _totalQualityCount = animationData.TotalCount;
        if (_totalQualityCount == 0)
        {
            onAllComplete?.Invoke();
            return;
        }

        _qualitySequence = animationData.QualitySequence;
        _completedQualityCount = 0;
        _onQualityComplete = onQualityComplete;
        _onAllQualitiesComplete = onAllComplete;

        // 애니메이션 Canvas 생성
        CreateAnimationCanvas();

        // 애니메이션 코루틴 시작
        CoroutineManager.Instance.StartCoroutine(CoAnimateQualitySequence(startPosition, targetPositions, interval));
    }

    /// <summary>
    /// GameDevManager로부터 작업 애니메이션 데이터 가져오기
    /// </summary>
    private WorkAnimationData GetWorkAnimationData()
    {
        Player worker = MemberManager.Instance.SelectedPlayer;
        
        if (worker == null)
        {
            Debug.LogWarning("No suitable member for current stage!");
            return null;
        }

        // Manager에서 WorkResult를 계산하고 DTO로 변환
        var workResult = GameDevManager.Instance.CalculateWorkResult(worker.CurrentMemberData);
        return new WorkAnimationData(workResult.qualitySequence);
    }

    #endregion

    #region Animation Canvas Management

    private void CreateAnimationCanvas()
    {
        CleanupAnimationCanvas();

        _animationCanvasObj = new GameObject(_animationCanvasName);
        _animationCanvas = _animationCanvasObj.AddComponent<Canvas>();
        CanvasScaler animScaler = _animationCanvasObj.AddComponent<CanvasScaler>();

        _animationCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _animationCanvas.overrideSorting = true;
        _animationCanvas.sortingOrder = ANIMATION_CANVAS_SORT_ORDER;

        Canvas mainCanvas = FindMainCanvas();
        if (mainCanvas != null)
        {
            CanvasScaler mainScaler = mainCanvas.GetComponent<CanvasScaler>();
            if (mainScaler != null)
            {
                animScaler.uiScaleMode = mainScaler.uiScaleMode;
                animScaler.referenceResolution = mainScaler.referenceResolution;
                animScaler.screenMatchMode = mainScaler.screenMatchMode;
                animScaler.matchWidthOrHeight = mainScaler.matchWidthOrHeight;
            }
        }
    }
    private Canvas FindMainCanvas()
    {
        Canvas[] allCanvases = GameObject.FindObjectsByType<Canvas>(FindObjectsSortMode.None);

        foreach (var canvas in allCanvases)
        {
            // 애니메이션 Canvas 자신은 제외
            if (canvas.name == _animationCanvasName)
                continue;

            // 첫 번째 Overlay Canvas 반환
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                return canvas;
        }

        return null;
    }
    private void CleanupAnimationCanvas()
    {
        if (_animationCanvasObj != null)
        {
            Destroy(_animationCanvasObj);
            _animationCanvasObj = null;
            _animationCanvas = null;
        }
    }

    #endregion

    #region Quality Animation

    private IEnumerator CoAnimateQualitySequence(
        Vector2 startPosition,
        Dictionary<EQualityType, Vector2> targetPositions,
        float interval)
    {
        if (_qualitySequence == null || _qualitySequence.Count == 0)
        {
            Debug.LogWarning("Quality sequence is empty in animation!");
            yield break;
        }

        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(interval);

        foreach (EQualityType quality in _qualitySequence)
        {
            CreateAndAnimateQuality(quality, startPosition, targetPositions);
            yield return wait;
        }
    }

    private void CreateAndAnimateQuality(EQualityType quality, Vector2 startPos, Dictionary<EQualityType, Vector2> targetPositions)
    {
        GameObject qualityObj = CreateQualityImage(quality);
        if (qualityObj == null) 
            return;

        RectTransform qualityObjRect = qualityObj.GetComponent<RectTransform>();
        qualityObjRect.SetParent(_animationCanvas.transform, false);
        qualityObjRect.anchoredPosition = startPos;
        qualityObjRect.sizeDelta = new Vector2(QUALITY_IMAGE_SIZE, QUALITY_IMAGE_SIZE);

        Vector2 targetPos = targetPositions.GetValueOrDefault(quality, Vector2.zero);
        StartQualityAnimation(qualityObj, targetPos, quality);
    }

    private GameObject CreateQualityImage(EQualityType quality)
    {
        GameObject qualityObj = new GameObject($"Quality_{quality}");
        UnityEngine.UI.Image image = qualityObj.AddComponent<UnityEngine.UI.Image>();

        image.sprite = null;
        image.type = UnityEngine.UI.Image.Type.Simple;
        image.color = GetQualityColor(quality);
        image.raycastTarget = false;

        RectTransform rectTransform = qualityObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(QUALITY_IMAGE_SIZE, QUALITY_IMAGE_SIZE);

        return qualityObj;
    }

    private Color GetQualityColor(EQualityType quality)
    {
        return quality switch
        {
            EQualityType.Fun => Color.blue,
            EQualityType.Nyang => Color.green,
            EQualityType.Graphics => Color.red,
            EQualityType.Sound => Color.yellow,
            _ => Color.white
        };
    }

    private void StartQualityAnimation(GameObject qualityObj, Vector2 targetPos, EQualityType quality)
    {
        RectTransform rectTransform = qualityObj.GetComponent<RectTransform>();
        Vector2 startPos = rectTransform.anchoredPosition;

        float distance = Vector2.Distance(startPos, targetPos);
        float duration = Mathf.Max(MIN_ANIMATION_DURATION, distance / ANIMATION_SPEED);

        CoroutineManager.Instance.StartCoroutine(CoAnimateQuality(rectTransform, targetPos, quality, qualityObj, duration));
    }

    private IEnumerator CoAnimateQuality(RectTransform rectTransform, Vector2 targetPos, EQualityType quality, GameObject qualityObj, float duration)
    {
        if (rectTransform == null || qualityObj == null) 
            yield break;

        Vector2 startPos = rectTransform.anchoredPosition;
        float elapsedTime = 0f;

        while (elapsedTime < duration && qualityObj != null)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            Vector2 currentPos = Vector2.Lerp(startPos, targetPos, t);
            rectTransform.anchoredPosition = currentPos;

            if (Vector2.Distance(currentPos, targetPos) < ARRIVAL_THRESHOLD)
                break;

            yield return null;
        }

        if (qualityObj != null)
        {
            rectTransform.anchoredPosition = targetPos;
            OnQualityAnimationComplete(qualityObj, quality);
        }
    }

    private void OnQualityAnimationComplete(GameObject qualityObj, EQualityType quality)
    {
        if (qualityObj != null)
            Destroy(qualityObj);

        _completedQualityCount++;
        _onQualityComplete?.Invoke(quality);

        if (_completedQualityCount >= _totalQualityCount)
        {
            CleanupAnimationCanvas();
            _onAllQualitiesComplete?.Invoke();
        }
    }

    #endregion
}
