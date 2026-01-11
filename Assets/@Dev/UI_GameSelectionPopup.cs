using UnityEngine;

public class UI_GameSelectionPopup : UI_UGUI, IUI_Popup
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

        //Content
        ContentButton1,
        ContentButton2,
        ContentButton3,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //SubMiddle
        SubMiddleContentNameText,
        SubMiddleCostNameText,

        //Content
        ContentText1,
        ContentText2,
        ContentText3,

        CostText1,
        CostText2,
        CostText3,

        //SubBottom
        SynergyNameText,
        SynergyText,
    }

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
    }

    public override void RefreshUI()
    {
        base.RefreshUI();

    }
}
