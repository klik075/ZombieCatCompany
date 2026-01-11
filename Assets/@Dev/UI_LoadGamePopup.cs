using UnityEngine;

public class UI_LoadGamePopup : UI_UGUI, IUI_Popup
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
        LoadGameText,

        //Top
        ModeText,
        CompanyText,

        //Bottom
        YearText,
        FundsText
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
