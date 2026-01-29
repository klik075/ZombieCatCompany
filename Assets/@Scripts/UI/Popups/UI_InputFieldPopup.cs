using System;
using UnityEngine;
using TMPro;
public class UI_InputFieldPopup : UI_UGUI, IUI_Popup, IClickableUI
{
    enum GameObjects
    {
        //BG
        BG,
        InputField
    }
    enum Buttons
    {
        OkayButton,
        NoButton,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //InputField
        Placeholder,

        //SubBottom
        OkayButtonText,
        NoButtonText,
    }
    enum Images
    {

    }

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        GetButton((int)Buttons.OkayButton).onClick.AddListener(() => OnOkayButtonClicked());
        GetButton((int)Buttons.NoButton).onClick.AddListener(() => OnNoButtonClicked());
    }
    public void SetInfo()
    {
        UpdateContent();
    }
    public void UpdateContent()
    {
        GetText((int)Texts.MainTitleText).text = "게임 타이틀";
        GetText((int)Texts.Placeholder).text = "게임 이름 입력";
        GetText((int)Texts.NoButtonText).text = "뒤로";
        GetText((int)Texts.OkayButtonText).text = "결정";
    }
    public void OnOkayButtonClicked()
    {
        string inputText = GetObject((int)GameObjects.InputField).GetComponent<TMP_InputField>().text;
        GameDevManager.Instance.CurrentGameTitle = inputText;
        OnNoButtonClicked();
    }
    public void OnNoButtonClicked()
    {
        UIManager.Instance.ClosePopupUI();
    }
    public override void RefreshUI()
    {
        base.RefreshUI();

    }
}
