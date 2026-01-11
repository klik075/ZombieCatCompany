using UnityEngine;

public class UI_MagazineReviewPopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
    }
    enum Buttons
    {
        Click,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //SubMiddle
        SubMiddleNameText,

        //Content
        EvaluationText1,
        EvaluationText2,
        EvaluationText3,
        EvaluationText4,

        EvaluationScoreText1,
        EvaluationScoreText2,
        EvaluationScoreText3,
        EvaluationScoreText4,

        //SubBottom
        SubBottomText,
    }
    enum Images
    {
        //Content
        EvaluatorsImage1,
        EvaluatorsImage2,
        EvaluatorsImage3,
        EvaluatorsImage4,
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
