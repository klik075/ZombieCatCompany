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

        RefreshUI();
    }

    private void OnClickNewGame()
    {
        if (SaveManager.Instance.HasGameData())
        {
            UI_OverWritePopup popup = UIManager.Instance.ShowPopupUI<UI_OverWritePopup>();
        }
    }

    public override void RefreshUI()
    {
        base.RefreshUI();

    }
}
