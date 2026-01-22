using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Define;

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
    private const float QUALITY_SPAWN_INTERVAL = 0.3f;
    
    private int _currentWorkCount = 0;
    
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
        _currentWorkCount = 0;
        UpdateContent();
        
        CoroutineManager.Instance.StartCoroutine(CoWorkProcess());
    }

    private IEnumerator CoWorkProcess()
    {
        // 메인 Canvas 준비
        Canvas mainCanvas = GetComponentInParent<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("Main Canvas not found!");
            SetInteractable(true);
            yield break;
        }

        // 레이아웃 강제 업데이트 후 위치 계산
        yield return ForceUpdateLayout(mainCanvas);
        
        RectTransform canvasRect = mainCanvas.GetComponent<RectTransform>();
        Camera uiCamera = mainCanvas.worldCamera ?? Camera.main;
        
        Vector2 startPos = GetUIPosition(GetImage((int)Images.MemberImage).rectTransform, canvasRect, uiCamera);
        Dictionary<EQualityType, Vector2> targetPositions = GetQualityPositions(canvasRect, uiCamera);

        QualityManager.Instance.StartMainWork(
            startPos,
            targetPositions,
            QUALITY_SPAWN_INTERVAL,
            OnQualityComplete,
            OnAllQualitiesComplete
        );
    }

    // 레이아웃 강제 업데이트
    private IEnumerator ForceUpdateLayout(Canvas mainCanvas)
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(mainCanvas.GetComponent<RectTransform>());
        yield return null;
    }

    // 품질 이미지들의 위치 계산
    private Dictionary<EQualityType, Vector2> GetQualityPositions(RectTransform animCanvas, Camera uiCamera)
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
            positions[type] = GetUIPosition(GetImage((int)image).rectTransform, animCanvas, uiCamera);
        }

        return positions;
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

    // QualityManager 콜백
    private void OnQualityComplete(EQualityType quality)
    {
        _currentWorkCount++;
        GameDevManager.Instance.AddQualityScore(quality, 1);
    }

    private void OnAllQualitiesComplete()
    {
        SetInteractable(true);
        EventManager.Instance.TriggerEvent(EEventType.WorkCompleted);
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
                GetImage((int)Images.MemberImage).sprite = memberSprite;
        }

        GetText((int)Texts.DialogueText).text = "우어어...워어어어..";
        UpdateQuality();
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
