using UnityEngine;

public class UI_MemberHirePopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
        //BG - 상호작용 x
        BG,
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
        SubMiddleSalaryNameText,
        SubMiddleSalaryText,

        //LeftContent
        RoleText,
        PaymentNameText,
        PaymentText,

        //RightContent
        AbilityNameText1,
        AbilityScoreText1,
        AbilityNameText2,
        AbilityScoreText2,
        AbilityNameText3,
        AbilityScoreText3,
        AbilityNameText4,
        AbilityScoreText4,

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

        //OkayButton은 다음으로 진행
    }

    public override void RefreshUI()
    {
        base.RefreshUI();

    }
}
