using UnityEngine;

public class UI_ChatPopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {

    }
    enum Buttons
    {
        //BG
        Click,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //Content
        ContentText,

        //SubBottom
        RoleText,
    }
    enum Images
    {
        //SubBottom
        MemberImage,
    }

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        //BG누를 시 다음 팝업으로 이동
    }

    public override void RefreshUI()
    {
        base.RefreshUI();

    }
}
