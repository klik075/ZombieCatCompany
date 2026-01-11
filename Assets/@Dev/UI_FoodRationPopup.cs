using UnityEngine;

public class UI_FoodRationPopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
        //BG - 상호작용 x
        BG,

        //Content
        Toggle1,
        Toggle2,
        Toggle3,

        Tooltip1,
        Tooltip2,
        Tooltip3,

        //TooltipBG
        TooltipBG
    }
    enum Buttons
    {
        //MainTitle

        //SubBottom
        OkayButton,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //SubMiddle
        SubMiddleNecessaryFoodNameText,
        SubMiddleNecessaryFoodText,

        //Content
        FoddText1,
        FoddText2,
        FoddText3,

        MemberNameText1,
        MemberNameText2,
        MemberNameText3,

        MemberStateText1,
        MemberStateText2,
        MemberStateText3,

        //SubBottom
        OkayButtonText,
    }
    enum Images
    {
        //Content
        MemberImage1,
        MemberImage2,
        MemberImage3,

        DispatchImage1,
        DispatchImage2,
        DispatchImage3,
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
