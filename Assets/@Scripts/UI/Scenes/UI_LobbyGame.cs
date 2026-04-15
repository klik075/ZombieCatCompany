using System;
using UnityEngine;
using static Define;

public class UI_LobbyGame : UI_UGUI, IUI_Scene
{
    enum GameObjects
    {
        //BG
        BG,

    }
    enum Buttons
    {
        NewButton,
        LoadButton,
        EndingRecordButton,
        SettingButton
    }
    enum Texts
    {
        NewButtonText,
        LoadButtonText,
        EndingRecordButtonText,
        SettingButtonText
    }
    enum Images
    {
        TitleImage,

    }

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        GetButton((int)Buttons.NewButton).onClick.AddListener(() => { PlayButtonClickSound(); OnClickNewGame(); });
        GetButton((int)Buttons.LoadButton).onClick.AddListener(() => { PlayButtonClickSound(); OnClickLoadGame(); });
        GetButton((int)Buttons.EndingRecordButton).onClick.AddListener(() => { PlayButtonClickSound(); OnClickEndingRecord(); });
        GetButton((int)Buttons.SettingButton).onClick.AddListener(() => { PlayButtonClickSound(); OnClickSetting(); });

        RefreshUI();
    }
    private void OnClickNewGame()
    {
        if (SaveManager.Instance.HasGameData())
        {
            UI_OverWritePopup popup = UIManager.Instance.ShowPopupUI<UI_OverWritePopup>();
        }
        else
        {
            UI_ModeSelectionPopup modePopup = UIManager.Instance.ShowPopupUI<UI_ModeSelectionPopup>();
        }
    }

    private void OnClickLoadGame()
    {
        if (SaveManager.Instance.HasGameData())
        {
            UI_LoadGamePopup loadPopup = UIManager.Instance.ShowPopupUI<UI_LoadGamePopup>();
        }
        else
        {
            UI_ModeSelectionPopup modePopup = UIManager.Instance.ShowPopupUI<UI_ModeSelectionPopup>();
        }
    }
    private void OnClickEndingRecord()
    {
        UI_EndingRecordPopup endingPopup = UIManager.Instance.ShowPopupUI<UI_EndingRecordPopup>();
    }
    private void OnClickSetting()
    {
        UI_LobbySettingPopup settingPopup = UIManager.Instance.ShowPopupUI<UI_LobbySettingPopup>();
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
