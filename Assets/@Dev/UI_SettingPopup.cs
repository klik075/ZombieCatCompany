using UnityEngine;

public class UI_SettingPopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
        //BG
        BG,

        //Content   
        Scrollbar2,
        Scrollbar3,
    }
    enum Buttons
    {
        //Content
        SettingLeftButton1,
        SettingRightButton1,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //Content
        SettingNameText1,
        SettingNameText2,
        SettingNameText3,

        SettingLeftButtonText1,
        SettingRightButtonText1,
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
