using System;
using System.Text;
using UnityEngine;

public class UI_MessagePopup : UI_UGUI, IUI_Popup, IClickableUI
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
    private Action _onNoCallback;
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

    public void SetInfo(string[] scripts, string[] insertScripts = null, Action okAction = null, Action noAction = null)
    {
        _content = GetContentText(scripts, insertScripts);
        _onOkayCallback = okAction;
        _onNoCallback = noAction;

        UpdateContent();
    }
    public string GetContentText(string[] script, string[] insertScript)
    {
        StringBuilder stringBuilder = new StringBuilder();
        for (int i = 0; i < script.Length; i++)
        {
            stringBuilder.Append(script[i]);
            if (insertScript != null && i < insertScript.Length)
            {
                stringBuilder.Append(insertScript[i]);
            }
        }

        return stringBuilder.ToString();
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
        _onNoCallback?.Invoke();
    }

    public override void RefreshUI()
    {
        base.RefreshUI();
    }
}
