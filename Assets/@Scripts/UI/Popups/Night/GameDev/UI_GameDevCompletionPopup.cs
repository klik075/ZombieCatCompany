using System;
using UnityEngine;
using static Define;

public class UI_GameDevCompletionPopup : UI_UGUI, IUI_Popup, IClickableUI
{
    enum GameObjects
    {
        //BG
        BG,
    }
    enum Buttons
    {
        //SubBottom
        GameTitleChangeButton,
        OkayButton,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //SubMiddle
        SubMiddleNameText,

        //LeftContent
        GenreText,
        ContentText,

        //RightContent
        QualityNameText1,
        QualityNameText2,
        QualityNameText3,
        QualityNameText4,
        QualityNameText5,

        QualityScoreText1,
        QualityScoreText2,
        QualityScoreText3,
        QualityScoreText4,
        QualityScoreText5,

        //SubBottom
        GameTitleChangeButtonText,
        OkayButtonText,
    }
    enum Images
    {
        //LeftContent
        PcImage,
    }

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));
        
        GetButton((int)Buttons.GameTitleChangeButton).onClick.AddListener(() => OpenInputFieldPopup());
        GetButton((int)Buttons.OkayButton).onClick.AddListener(() => OpenMagazineReviewPopup());
    }
    protected override void OnEnable()
    {
        base.OnEnable();

        EventManager.Instance.AddEvent(EEventType.NewDevTitleChanged, UpdateGameTitle);
    }
    protected override void OnDisable()
    {
        base.OnDisable();

        EventManager.Instance.RemoveEvent(EEventType.NewDevTitleChanged, UpdateGameTitle);
    }
    public void SetInfo()
    {
        UpdateContent();
    }
    private void UpdateContent()
    {
        GetText((int)Texts.MainTitleText).text = "게임 개발 완성";

        UpdateGameTitle();

        GetText((int)Texts.GameTitleChangeButtonText).text = "타이틀 변경";
        GetText((int)Texts.OkayButtonText).text = "출하한다";

        GetText((int)Texts.GenreText).text = GenreData.GenreToString(GameDevManager.Instance.CurrentGenreData.GenreType);
        GetText((int)Texts.ContentText).text = ContentData.ContentToString(GameDevManager.Instance.CurrentContentData.ContentType);
        
        GetText((int)Texts.QualityNameText1).text = QualityData.QualityToString(Define.EQualityType.Fun);
        GetText((int)Texts.QualityNameText2).text = QualityData.QualityToString(Define.EQualityType.Nyang);
        GetText((int)Texts.QualityNameText3).text = QualityData.QualityToString(Define.EQualityType.Graphics);
        GetText((int)Texts.QualityNameText4).text = QualityData.QualityToString(Define.EQualityType.Sound);
        GetText((int)Texts.QualityNameText5).text = QualityData.QualityToString(Define.EQualityType.Bug);

        GetText((int)Texts.QualityScoreText1).text = GameDevManager.Instance.CurrentProject.funScore.ToString();
        GetText((int)Texts.QualityScoreText2).text = GameDevManager.Instance.CurrentProject.nyangScore.ToString();
        GetText((int)Texts.QualityScoreText3).text = GameDevManager.Instance.CurrentProject.graphicsScore.ToString();
        GetText((int)Texts.QualityScoreText4).text = GameDevManager.Instance.CurrentProject.soundScore.ToString();
        GetText((int)Texts.QualityScoreText5).text = GameDevManager.Instance.CurrentProject.bugScore.ToString();
    }
    private void UpdateGameTitle()
    {
        GetText((int)Texts.SubMiddleNameText).text = GameDevManager.Instance.CurrentGameTitle;
    }
    private void OpenInputFieldPopup()
    {
        UI_InputFieldPopup InputPopup = UIManager.Instance.ShowPopupUI<UI_InputFieldPopup>();
        InputPopup.SetInfo(EInputFieldType.ChangeGameTitle, OnInputCompleted);
    }
    private void OnInputCompleted(string input)
    {
        GameDevManager.Instance.CurrentGameTitle = input;
    }
    private void OpenMagazineReviewPopup()
    {
        UIManager.Instance.ClosePopupUI();

        UI_MagazineReviewPopup ReviewPopup = UIManager.Instance.ShowPopupUI<UI_MagazineReviewPopup>();
        ReviewPopup.SetInfo();
    }
    public override void RefreshUI()
    {
        base.RefreshUI();

    }
}
