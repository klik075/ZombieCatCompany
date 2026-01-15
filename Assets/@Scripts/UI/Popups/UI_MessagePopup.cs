using System;
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

    private Action _onOkayCallback;
    private string _content;

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));

        GetButton((int)Buttons.OkayButton).onClick.AddListener(OnOkayButtonClicked);
        GetButton((int)Buttons.NoButton).onClick.AddListener(OnNoButtonClicked);
    }

    public void SetInfo(string content, Action onOkayCallback = null)
    {
        _content = content;
        _onOkayCallback = onOkayCallback;
        
        UpdateContent();
    }

    private void UpdateContent()
    {
        GetText((int)Texts.MainTitleText).text = "메시지";
        GetText((int)Texts.ContentText).text = _content != null ? _content : "";
        GetText((int)Texts.OkayButtonText).text = "네";
        GetText((int)Texts.NoButtonText).text = "아니오";
    }

    private void OnOkayButtonClicked()
    {
        UIManager.Instance.ClosePopupUI();
        _onOkayCallback?.Invoke();
    }

    private void OnNoButtonClicked()
    {
        UIManager.Instance.ClosePopupUI();
    }

    public override void RefreshUI()
    {
        base.RefreshUI();
    }
}
