using UnityEngine;
using static Define;

public class UI_DeleteDataPopup : UI_UGUI, IUI_Popup
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
        DeleteText,

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

        GetButton((int)Buttons.OkayButton).onClick.AddListener(() => { PlayButtonClickSound(); OnClickOkayButton(); });
        GetButton((int)Buttons.NoButton).onClick.AddListener(() => { PlayButtonClickSound(); OnClickNoButton(); });
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        UpateContent();
    }
    private void UpateContent()
    {
        GetText((int)Texts.DeleteText).text = "현재 유저 데이터를 삭제합니다.";
        GetText((int)Texts.OkayButtonText).text = "네";
        GetText((int)Texts.NoButtonText).text = "아니오";
    }
    private void OnClickOkayButton()
    {
        OnClickNoButton();
        SaveManager.Instance.DeleteAllData();
    }
    private void OnClickNoButton()
    {
        UIManager.Instance.ClosePopupUI();
    }
    private void PlayButtonClickSound()
    {
        SoundManager.Instance.Play2D(ESound.Effect, "Button");
    }
    public override void RefreshUI()
    {
        base.RefreshUI();
        UpateContent();
    }
}
