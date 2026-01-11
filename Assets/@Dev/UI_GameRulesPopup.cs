using UnityEngine;

public class UI_GameRulesPopup : UI_UGUI, IUI_Popup
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
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //Content
        ContentText,
    }
    enum Images
    {

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
