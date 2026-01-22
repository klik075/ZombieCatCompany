using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Define;

// 작업 애니메이션을 위한 데이터 전달 객체 (DTO)
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

public class UI_GameDevWorkPopup : UI_UGUI, IUI_Popup, IClickableUI
{
    enum GameObjects
    {
        //BG
        BG,
    }
    enum Buttons
    {
        Click,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //SubMiddle
        SubMiddleNameText,

        //RightContent
        WorkText,

        ScoreText1,
        ScoreText2,
        ScoreText3,
        ScoreText4,

        //SubBottom
        DialogueText,
    }
    enum Images
    {
        //LeftContent
        MemberImage,

        //RightContent
        QualityImage1,
        QualityImage2,
        QualityImage3,
        QualityImage4,
    }
    // 애니메이션 관련 상수
    private const float ANIMATION_SPEED = 500f; // 픽셀/초
    private const float MIN_ANIMATION_DURATION = 0.5f; // 최소 애니메이션 시간
    private const float ARRIVAL_THRESHOLD = 5f; // 도착 판정 임계값 (픽셀)
    private const float QUALITY_IMAGE_SIZE = 100f; // 품질 이미지 크기
    private const int ANIMATION_CANVAS_SORT_ORDER = 1000; // 애니메이션 Canvas 정렬 순서
    
    private float interval = 1.0f;
    private List<EQualityType> _qualitySequence; // DTO에서 추출한 품질 시퀀스
    private int _currentWorkCount = 0;
    private int _totalQualityCount = 0;
    private int _completedQualityCount = 0;
    private GameObject _animationCanvasObj;
    
    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        GetButton((int)Buttons.Click).onClick.AddListener(OnClickButton);

        EventManager.Instance.AddEvent(Define.EEventType.QualityChanged, UpdateQuality);
    }
    private void SetInteractable(bool interactable)
    {
        GetButton((int)Buttons.Click).interactable = interactable;
    }
    public void StartOfWork()
    {
        SetInteractable(false);

        // DTO를 통해 필요한 애니메이션 데이터만 받기
        WorkAnimationData animationData = GetWorkAnimationData();
        
        if (animationData?.QualitySequence == null || animationData.QualitySequence.Count == 0)
        {
            Debug.LogWarning("No qualities to animate!");
            SetInteractable(true);
            return;
        }

        // DTO 데이터를 로컬 변수에 저장
        _qualitySequence = animationData.QualitySequence;
        _currentWorkCount = 0;
        _totalQualityCount = animationData.TotalCount;
        _completedQualityCount = 0;
        
        UpdateContent();
        CoroutineManager.Instance.StartCoroutine(CoWorkProcess());
    }

    // GameDevManager로부터 애니메이션 데이터 가져오기
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

    private IEnumerator CoWorkProcess()
    {
        if (_qualitySequence == null || _qualitySequence.Count == 0)
        {
            Debug.LogError("Quality sequence is empty!");
            yield break;
        }

        Debug.Log($"Starting work process with {_qualitySequence.Count} qualities");

        // 메인 Canvas 및 애니메이션 Canvas 준비
        Canvas mainCanvas = GetComponentInParent<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("Main Canvas not found!");
            yield break;
        }

        Canvas animCanvas = CreateAnimationCanvas(mainCanvas);
        Camera uiCamera = GetUICamera(mainCanvas);

        // 레이아웃 강제 업데이트 후 위치 계산
        yield return ForceUpdateLayout(mainCanvas);
        
        Vector2 memberAnimPos = GetUIPosition(GetImage((int)Images.MemberImage).rectTransform, animCanvas.GetComponent<RectTransform>(), uiCamera);
        Dictionary<EQualityType, Vector2> qualityPositions = GetQualityPositions(animCanvas.GetComponent<RectTransform>(), uiCamera);

        // 각 품질 이미지 애니메이션 실행
        foreach (EQualityType quality in _qualitySequence)
        {
            CreateAndAnimateQuality(quality, animCanvas, memberAnimPos, qualityPositions);
            yield return new WaitForSecondsRealtime(interval);
        }
        
        Debug.Log("Work process completed");
    }

    // 애니메이션 Canvas 생성
    private Canvas CreateAnimationCanvas(Canvas mainCanvas)
    {
        _animationCanvasObj = new GameObject("AnimationCanvas");
        Canvas animCanvas = _animationCanvasObj.AddComponent<Canvas>();
        CanvasScaler animScaler = _animationCanvasObj.AddComponent<CanvasScaler>();
        
        // 메인 Canvas와 동일한 설정 적용
        animCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        animCanvas.overrideSorting = true;
        animCanvas.sortingOrder = ANIMATION_CANVAS_SORT_ORDER;
        
        // CanvasScaler 설정 복사
        CanvasScaler mainScaler = mainCanvas.GetComponent<CanvasScaler>();
        if (mainScaler != null)
        {
            animScaler.uiScaleMode = mainScaler.uiScaleMode;
            animScaler.referenceResolution = mainScaler.referenceResolution;
            animScaler.screenMatchMode = mainScaler.screenMatchMode;
            animScaler.matchWidthOrHeight = mainScaler.matchWidthOrHeight;
        }

        return animCanvas;
    }

    // UI 카메라 가져오기
    private Camera GetUICamera(Canvas canvas)
    {
        Camera uiCamera = canvas.worldCamera;
        return uiCamera ?? Camera.main;
    }

    // 레이아웃 강제 업데이트
    private IEnumerator ForceUpdateLayout(Canvas mainCanvas)
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(mainCanvas.GetComponent<RectTransform>());
        yield return null;
    }

    // 품질 이미지들의 위치 계산
    private Dictionary<EQualityType, Vector2> GetQualityPositions(RectTransform animCanvasRect, Camera uiCamera)
    {
        var qualityImages = new (EQualityType type, Images image)[]
        {
            (EQualityType.Fun, Images.QualityImage1),
            (EQualityType.Nyang, Images.QualityImage2),
            (EQualityType.Graphics, Images.QualityImage3),
            (EQualityType.Sound, Images.QualityImage4)
        };

        var positions = new Dictionary<EQualityType, Vector2>();
        foreach (var (type, image) in qualityImages)
        {
            positions[type] = GetUIPosition(GetImage((int)image).rectTransform, animCanvasRect, uiCamera);
        }

        return positions;
    }

    // 품질 이미지 생성 및 애니메이션 시작
    private void CreateAndAnimateQuality(EQualityType quality, Canvas animCanvas, Vector2 startPos, Dictionary<EQualityType, Vector2> qualityPositions)
    {
        GameObject qualityObj = CreateQualityImage(quality);
        if (qualityObj == null) return;

        // Canvas에 추가 및 위치 설정
        RectTransform qualityObjRect = qualityObj.GetComponent<RectTransform>();
        qualityObjRect.SetParent(animCanvas.transform, false);
        qualityObjRect.anchoredPosition = startPos;
        qualityObjRect.sizeDelta = new Vector2(QUALITY_IMAGE_SIZE, QUALITY_IMAGE_SIZE);

        // 애니메이션 시작
        Vector2 targetPos = qualityPositions.GetValueOrDefault(quality, Vector2.zero);
        StartQualityAnimation(qualityObj, targetPos, quality);
    }

    // UI 좌표 변환
    private Vector2 GetUIPosition(RectTransform target, RectTransform animCanvas, Camera uiCamera)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, target.position);
        
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(animCanvas, screenPoint, uiCamera, out Vector2 localPoint))
        {
            return localPoint;
        }

        Debug.LogWarning($"Failed to convert screen point for {target.name}");
        return Vector2.zero;
    }

    private void StartQualityAnimation(GameObject qualityObj, Vector2 targetPos, EQualityType quality)
    {
        RectTransform rectTransform = qualityObj.GetComponent<RectTransform>();
        Vector2 startPos = rectTransform.anchoredPosition;
        
        float distance = Vector2.Distance(startPos, targetPos);
        float duration = Mathf.Max(MIN_ANIMATION_DURATION, distance / ANIMATION_SPEED);
        
        StartCoroutine(CoAnimateQuality(rectTransform, targetPos, quality, qualityObj, duration));
    }
    
    private IEnumerator CoAnimateQuality(RectTransform rectTransform, Vector2 targetPos, EQualityType quality, GameObject qualityObj, float duration)
    {
        if (rectTransform == null || qualityObj == null)
        {
            yield break;
        }

        Vector2 startPos = rectTransform.anchoredPosition;
        float elapsedTime = 0f;

        while (elapsedTime < duration && qualityObj != null)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            Vector2 currentPos = Vector2.Lerp(startPos, targetPos, t);
            rectTransform.anchoredPosition = currentPos;

            // 목표 지점 도달 확인
            if (Vector2.Distance(currentPos, targetPos) < ARRIVAL_THRESHOLD)
            {
                break;
            }

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
        // 이미지 제거 및 점수 처리
        if (qualityObj != null)
        {
            Destroy(qualityObj);
        }
        
        GameDevManager.Instance.AddQualityScore(quality, 1);
        _currentWorkCount++;
        _completedQualityCount++;
        
        // 모든 품질 완료 시 정리
        if (_completedQualityCount >= _totalQualityCount)
        {
            SetInteractable(true);
            CleanupAnimationCanvas();
            EventManager.Instance.TriggerEvent(EEventType.WorkCompleted);
            Debug.Log("All qualities completed! Work finished.");
        }
    }

    private void CleanupAnimationCanvas()
    {
        if (_animationCanvasObj != null)
        {
            Destroy(_animationCanvasObj);
            _animationCanvasObj = null;
        }
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

    private void UpdateContent()
    {
        Player worker = MemberManager.Instance.SelectedPlayer;
        if (worker != null)
        {
            GetText((int)Texts.MainTitleText).text = worker.CurrentMemberData.Name;
            GetText((int)Texts.SubMiddleNameText).text = GetGameDevStageText(GameDevManager.Instance.CurrentGameDevType);
            
            Sprite memberSprite = worker.GetMemberSprite();
            if (memberSprite != null)
            {
                GetImage((int)Images.MemberImage).sprite = memberSprite;
            }
        }
        
        GetText((int)Texts.WorkText).text = $"{_currentWorkCount}번 작업";
        GetText((int)Texts.DialogueText).text = "우어어...워어어어..";
    }

    private void UpdateQuality()
    {
        var project = GameDevManager.Instance.CurrentProject;
        if (project != null)
        {
            GetText((int)Texts.ScoreText1).text = project.funScore.ToString();
            GetText((int)Texts.ScoreText2).text = project.nyangScore.ToString(); 
            GetText((int)Texts.ScoreText3).text = project.graphicsScore.ToString();
            GetText((int)Texts.ScoreText4).text = project.soundScore.ToString();
        }

        GetText((int)Texts.WorkText).text = $"{_currentWorkCount}번 작업";
    }
    private string GetGameDevStageText(EGameDevType devType)
    {
        switch (devType)
        {
            case EGameDevType.Scenario:
                return "기획을 정리 중";
            case EGameDevType.Graphics:
                return "원화 디자인 중";
            case EGameDevType.Sound:
                return "사운드 제작 중";
            default:
                return "너는 누구냐!";
        }
    }
    public void OnClickButton()
    {
        UIManager.Instance.ClosePopupUI();
    }
    public override void RefreshUI()
    {
        base.RefreshUI();

    }
}
