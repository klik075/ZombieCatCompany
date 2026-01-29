using System;
using UnityEngine;

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
    }
    public void SetInfo()
    {

    }

    public override void RefreshUI()
    {
        base.RefreshUI();

    }

    
}
