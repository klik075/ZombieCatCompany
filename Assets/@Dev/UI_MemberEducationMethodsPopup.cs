using UnityEngine;

public class UI_MemberEducationMethodsPopup : UI_UGUI, IUI_Popup
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
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //SubMiddle
        SubMiddleNameText,

        //LeftContent
        EducationMethod1,
        EducationMethod2,
        EducationMethod3,
        EducationMethod4,
        EducationMethod5,

        EducationMethodCost1,
        EducationMethodCost2,
        EducationMethodCost3,
        EducationMethodCost4,
        EducationMethodCost5,

        //RightContent
        AbilityScoreText1,
        AbilityScoreText2, 
        AbilityScoreText3,
        AbilityScoreText4,
        AbilityScoreText5,

        //SubBottom
        DescriptionText,
    }
    enum Images
    {
        //RightContent
        AbilityIcon1,
        AbilityIcon2,
        AbilityIcon3,
        AbilityIcon4,
        AbilityIcon5,

        AddIcon1_1,
        AddIcon2_1,
        AddIcon3_1,
        AddIcon4_1,
        AddIcon5_1,

        AddIcon1_2,
        AddIcon2_2,
        AddIcon3_2,
        AddIcon4_2,
        AddIcon5_2,
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
