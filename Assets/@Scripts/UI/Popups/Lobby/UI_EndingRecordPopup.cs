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
        LoadEndingText,

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

        GetButton((int)Buttons.ModeButton1).onClick.AddListener(() => { PlayButtonClickSound(); OnClickModeButton(EGameMode.Purchase); });
        GetButton((int)Buttons.ModeButton2).onClick.AddListener(() => { PlayButtonClickSound(); OnClickModeButton(EGameMode.Extortion); });
        GetButton((int)Buttons.BG).onClick.AddListener(() => UIManager.Instance.ClosePopupUI());
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        UpateContent();
    }
    private void UpateContent()
    {
        GetText((int)Texts.LoadEndingText).text = "엔딩 기록";
        GetText((int)Texts.ModeButtonText1).text = "구매 모드";
        GetText((int)Texts.ModeButtonText2).text = "강탈 모드";
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
    private void PlayButtonClickSound()
    {
        SoundManager.Instance.Play2D(ESound.Effect, "Button");
    }
    public override void RefreshUI()
    {
        base.RefreshUI();

    }
}
