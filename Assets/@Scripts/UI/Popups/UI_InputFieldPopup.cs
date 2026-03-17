using System;
using TMPro;
using UnityEngine;
using static Define;
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

    private EInputFieldType _currentType;
    private Action<string> _onConfirm;

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
    public void SetInfo(EInputFieldType type, Action<string> onConfirm)
    {
        _currentType = type;
        _onConfirm = onConfirm;

        UpdateContent();
    }
    public void UpdateContent()
    {
        UpdateUIByType();
        GetText((int)Texts.NoButtonText).text = "뒤로";
        GetText((int)Texts.OkayButtonText).text = "결정";
    }
    private void UpdateUIByType()
    {
        string mainTitle = "";
        string placeholder = "";

        switch (_currentType)
        {
            case EInputFieldType.ChangeGameTitle:
                mainTitle = "게임 타이틀";
                placeholder = "게임 이름 입력";
                break;
            case EInputFieldType.PurchaseFood:
                mainTitle = "통조림 개수";
                placeholder = "통조림 개수 입력";
                break;
            case EInputFieldType.CompanyName:
                mainTitle = "회사 명";
                placeholder = "회사 이름 입력";
                break;
            default:
                break;
        }

        GetText((int)Texts.MainTitleText).text = mainTitle;
        GetText((int)Texts.Placeholder).text = placeholder;
    }
    public void OnOkayButtonClicked()
    {
        string inputText = GetObject((int)GameObjects.InputField).GetComponent<TMP_InputField>().text;

        OnNoButtonClicked();
        _onConfirm?.Invoke(inputText);
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
