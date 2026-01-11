using UnityEngine;

public class UI_ProposalPopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
        //BG - 상호작용 x
        BG,
    }
    enum Buttons
    {
        //Content
        GenreButton,
        ContentButton,

        //SubBottom
        OkayButton,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //SubMiddle
        SubMiddleCostNameText,
        SubMiddleCostText,

        //Content
        GenreNameText,
        GenreText,
        ContentNameText,
        ContentText,

        //SubBottom
        OkayButtonText,
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
