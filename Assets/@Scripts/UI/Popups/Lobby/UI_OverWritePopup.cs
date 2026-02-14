using UnityEngine;

public class UI_OverWritePopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
        //BG
        BG,
    }
    enum Buttons
    {
        OkayButton,
        NoButton,
    }
    enum Texts
    {
        OverWriteText,

        //Buttons
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

        GetButton((int)Buttons.OkayButton).onClick.AddListener(OnClickOkayButton);
        GetButton((int)Buttons.NoButton).onClick.AddListener(OnClickNoButton);
    }
    private void OnClickOkayButton()
    {
        OnClickNoButton();
        UI_ModeSelectionPopup popup = UIManager.Instance.ShowPopupUI<UI_ModeSelectionPopup>();
    }
    private void OnClickNoButton()
    {
        UIManager.Instance.ClosePopupUI();
    }

    public override void RefreshUI()
    {
        base.RefreshUI();

    }
}
