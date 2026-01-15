using UnityEngine;

public class UI_GameDevWorkPopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
        //BG
        BG,
    }
    enum Buttons
    {

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

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));
    }

    public override void RefreshUI()
    {
        base.RefreshUI();

    }
}
