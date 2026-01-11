using UnityEngine;

public class UI_ResultsReportPopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
        //BG
        BG,
    }
    enum Buttons
    {
        //SubBottom
        OkayButton,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //Content
        ResultNameText1,
        ResultNameText2,
        ResultNameText3,

        ResultText1,
        ResultText2,
        ResultText3,

        //SubBottom
        OkayButtonText,
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
