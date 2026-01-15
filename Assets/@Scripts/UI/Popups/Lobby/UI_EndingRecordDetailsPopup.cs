using UnityEngine;

public class UI_EndingRecordDetailsPopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
        //BG
        BG,
    }
    enum Buttons
    {
        //MainTitle
        NextButton,
        PreviousButton,

        //SubBottom
        OkayButton,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //SubMiddle
        EndingNameText,
        ModeNameText,

        //LeftContent
        LeftContentLeftText1,
        LeftContentLeftText2,
        LeftContentLeftText3,
        LeftContentLeftText4,
        LeftContentLeftText5,
        LeftContentLeftText6,

        LeftContentRightText1,
        LeftContentRightText2,
        LeftContentRightText3,
        LeftContentRightText4,
        LeftContentRightText5,
        LeftContentRightText6,

        SurvivalRecordNameText,
        //RightContent
        RightContentLeftText1,
        RightContentLeftText2,
        RightContentLeftText3,
        RightContentLeftText4,
        RightContentLeftText5,
        RightContentLeftText6,

        RightContentRightText1,
        RightContentRightText2,
        RightContentRightText3,
        RightContentRightText4,
        RightContentRightText5,
        RightContentRightText6,

        //NoRecord
        NoRecordText,

        //SubBottom
        OkayButtonText,
    }
    enum Images
    {
        NoRecordImage,
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
