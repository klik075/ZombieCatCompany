using UnityEngine;

public class UI_EndingRecordPopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
        //BG
        BG,
    }
    enum Buttons
    {
        ModeButton1,
        ModeButton2,
    }
    enum Texts
    {
        LoadGameText,

        //Content
        ModeButtonText1,
        ModeButtonText2,
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
