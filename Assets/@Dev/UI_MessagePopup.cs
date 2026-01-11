using UnityEngine;

public class UI_MessagePopup : UI_UGUI, IUI_Popup
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
        NoButton,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //Content
        ContentText,

        //SubBottom
        OkayButtonText,
        NoButtonText,
    }

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));

        //OkayButton은 다음으로 진행
        //NoButton은 뒤로 가기
    }

    public override void RefreshUI()
    {
        base.RefreshUI();

    }
}
