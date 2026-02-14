using System;
using UnityEngine;

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
    }
    enum Texts
    {
        NewButtonText,
        LoadButtonText,
        EndingRecordButtonText,
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

        GetButton((int)Buttons.NewButton).onClick.AddListener(OnClickNewGame);
        GetButton((int)Buttons.LoadButton).onClick.AddListener(OnClickLoadGame);
        GetButton((int)Buttons.EndingRecordButton).onClick.AddListener(OnClickEndingRecord);

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

    public override void RefreshUI()
    {
        base.RefreshUI();

    }
}
