using UnityEngine;
using UnityEngine.UI;

public class UI_EventPopup : UI_UGUI, IUI_Popup, IClickableUI
{
    enum GameObjects
    {

    }
    enum Buttons
    {
        //BG
        ClickButton,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //Content
        EventText,
        EventResultText,
        MemberStateEventText,
    }
    enum Images
    {
        //Content
        EventResultIcon,
    }
    private RectTransform _rectTransform;
    protected override void Awake()
    {
        base.Awake();

        _rectTransform = gameObject.GetComponent<RectTransform>();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        GetButton((int)Buttons.ClickButton).onClick.AddListener(() => OnClickButton());
    }
    public void SetInfo()
    {
        
    }
    public void OnClickButton()
    {
        UIManager.Instance.ClosePopupUI();
    }
    public override void RefreshUI()
    {
        base.RefreshUI();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);

    }
}
