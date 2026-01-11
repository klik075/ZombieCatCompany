using UnityEngine;

public class UI_MemberHireMethodsPopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
        //BG - 상호작용 x
        BG,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //SubMiddle
        SubMiddleNameText,
        SubMiddleCostNameText,

        //Content
        HireMethodNameText1,
        HireMethodCostText1,
    }
    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindTexts(typeof(Texts));

    }

    public override void RefreshUI()
    {
        base.RefreshUI();

    }
}
