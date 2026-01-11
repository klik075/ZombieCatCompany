using UnityEngine;

public class UI_InputFieldPopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
        //BG
        BG,
        InputField
    }
    enum Buttons
    {
        OkayButton,
        NoButton,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        OkayButtonText,
        NoButtonText,
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
