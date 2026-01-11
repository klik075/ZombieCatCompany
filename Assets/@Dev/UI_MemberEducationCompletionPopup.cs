using UnityEngine;

public class UI_MemberEducationCompletionPopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
        //BG - 상호작용 x
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

        //SubMiddle
        SubMiddleNameText,

        //RightContent
        AbilityNameText1,
        AbilityNameText2,
        AbilityNameText3,
        AbilityNameText4,
        AbilityNameText5,

        AbilityCurrentScoreText1,
        AbilityCurrentScoreText2,
        AbilityCurrentScoreText3,
        AbilityCurrentScoreText4,
        AbilityCurrentScoreText5,

        AbilityUpgradeScoreText1,
        AbilityUpgradeScoreText2,
        AbilityUpgradeScoreText3,
        AbilityUpgradeScoreText4,
        AbilityUpgradeScoreText5,

        //SubBottom
        OkayButtonText,
    }
    enum Images
    {
        //LeftContent
        MemberImage,

        //RightContent
        AbilityIcon1,
        AbilityIcon2,
        AbilityIcon3,
        AbilityIcon4,
        AbilityIcon5,
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
