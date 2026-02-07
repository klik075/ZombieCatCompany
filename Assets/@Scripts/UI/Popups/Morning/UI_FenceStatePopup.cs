using System;
using UnityEngine;

public class UI_FenceStatePopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
        //BG
        BG,
    }
    enum Buttons
    {
        //SubBottom
        RepairButton,
        EnhanceButton,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //SubMiddle
        SubMiddleNameText,

        //LeftContent
        EnhanceNameText,
        EnhanceText,

        //RightContent
        AbilityNameText1,
        AbilityNameText2,
        AbilityNameText3,
        AbilityNameText4,
        AbilityNameText5,

        AbilityScoreText1,
        AbilityScoreText2,
        AbilityScoreText3,
        AbilityScoreText4,
        AbilityScoreText5,

        //SubBottom
        RepairButtonText,
        EnhanceButtonText,
    }
    enum Images
    {
        //LeftContent
        FenceImage,
    }

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));
    }
    protected override void Start()
    {
        base.Start();

    }
    public void SetInfo()
    {

    }
    public override void RefreshUI()
    {
        base.RefreshUI();

        //TODO : Localization
    }
}
