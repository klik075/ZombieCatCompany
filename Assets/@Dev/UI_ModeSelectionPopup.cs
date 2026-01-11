using UnityEngine;

public class UI_ModeSelectionPopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
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
        SubMiddleNameText,

        //LeftContent
        RoleText,
        FundsNameText,
        FundsText,

        //RightContent
        DescriptionText,
        FoodNameText,
        FoodText,

        //SubBottom
        OkayButtonText,
    }
    enum Images
    {
        //LeftContent
        MemberImage,
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
    public override void RefreshUI()
    {
        base.RefreshUI();

        //TODO : Localization
    }
}
