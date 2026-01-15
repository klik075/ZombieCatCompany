using UnityEngine;

public class UI_FenceEnhancePopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
    }
    enum Buttons
    {
        //SubBottom
        EnhanceButton,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //SubMiddle
        CurrentAbilityNameText,
        NextAbilityNameText,

        //LeftContent
        CurrentAbilityNameText1,
        CurrentAbilityNameText2,
        CurrentAbilityNameText3,
        CurrentAbilityNameText4,
        CurrentAbilityNameText5,

        CurrentAbilityScoreText1,
        CurrentAbilityScoreText2,
        CurrentAbilityScoreText3,
        CurrentAbilityScoreText4,
        CurrentAbilityScoreText5,

        //RightContent
        NextAbilityNameText1,
        NextAbilityNameText2,
        NextAbilityNameText3,
        NextAbilityNameText4,
        NextAbilityNameText5,

        NextAbilityScoreText1,
        NextAbilityScoreText2,
        NextAbilityScoreText3,
        NextAbilityScoreText4,
        NextAbilityScoreText5,

        //SubBottom
        SuccessNameText,
        SuccessText,

        EnhanceCostNameText,
        EnhanceCostText,

        EnhanceButtonText,
    }

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
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
