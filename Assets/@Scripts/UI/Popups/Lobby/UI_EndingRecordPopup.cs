using System;
using UnityEngine;
using static Define;
public class UI_EndingRecordPopup : UI_UGUI, IUI_Popup, IClickableUI
{
    enum GameObjects
    {
    }
    enum Buttons
    {
        ModeButton1,
        ModeButton2,
        BG,
    }
    enum Texts
    {
        LoadGameText,

        //Content
        ModeButtonText1,
        ModeButtonText2,
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

        GetButton((int)Buttons.ModeButton1).onClick.AddListener(() => OnClickModeButton(EGameMode.Purchase));
        GetButton((int)Buttons.ModeButton2).onClick.AddListener(() => OnClickModeButton(EGameMode.Extortion));
        GetButton((int)Buttons.BG).onClick.AddListener(() => UIManager.Instance.ClosePopupUI());
    }

    private void OnClickModeButton(EGameMode mode)
    {
        UI_EndingRecordDetailsPopup popup = UIManager.Instance.ShowPopupUI<UI_EndingRecordDetailsPopup>();
        switch (mode)
        {
            case EGameMode.Purchase:
                popup.SetInfo(EGameMode.Purchase);
                break;
            case EGameMode.Extortion:
                popup.SetInfo(EGameMode.Extortion);
                break;
            default:
                break;
        }
    }

    public override void RefreshUI()
    {
        base.RefreshUI();

    }
}
