using UnityEngine;
using static Define;

public class UI_MorningGame : UI_UGUI, IUI_Scene
{
    enum GameObjects
    {

    }
    enum Buttons
    {
        DefenseStartButton
    }
    enum Texts
    {
        DefenseStartButtonText
    }
    private UI_TopPanel _topPanel;
    private UI_MorningLeftPanel _leftPanel;
    private UI_MorningBottomPanel _bottomPanel;
    private UI_DefensePanel _defensePanel;

    protected override void Awake()
    {
        base.Awake();

        _topPanel = Utils.FindChildComponent<UI_TopPanel>(gameObject, recursive: true);
        _leftPanel = Utils.FindChildComponent<UI_MorningLeftPanel>(gameObject, recursive: true);
        _bottomPanel = Utils.FindChildComponent<UI_MorningBottomPanel>(gameObject, recursive: true);
        _defensePanel = Utils.FindChildComponent<UI_DefensePanel>(gameObject, recursive: true);

        _bottomPanel.SetInfo(_leftPanel);// BottomPanel에 LeftPanel 참조 전달

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));

        GetButton((int)Buttons.DefenseStartButton).onClick.AddListener(() => { PlayButtonClickSound(); OnClickedStartButton(); });

        EventManager.Instance.AddEvent(EEventType.UI_LeftPanelStateChanged, UpdateDefenseStartButton);
        EventManager.Instance.AddEvent(EEventType.UI_PopupClosed, UpdateDefenseStartButton);
        EventManager.Instance.AddEvent(EEventType.UI_PopupOpened, UpdateDefenseStartButton);
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        UpdateContent();
    }
    protected void OnDestroy()
    {
        EventManager.Instance.RemoveEvent(EEventType.UI_LeftPanelStateChanged, UpdateDefenseStartButton);
        EventManager.Instance.RemoveEvent(EEventType.UI_PopupClosed, UpdateDefenseStartButton);
        EventManager.Instance.RemoveEvent(EEventType.UI_PopupOpened, UpdateDefenseStartButton);
    }
    private void UpdateContent()
    {
        GetText((int)Texts.DefenseStartButtonText).text = "디펜스 시작";
    }
    public void OnClickedStartButton()
    {
        GetButton((int)Buttons.DefenseStartButton).gameObject.SetActive(false);
        _defensePanel.gameObject.SetActive(true);
        _defensePanel.Init();
    }
    private void UpdateDefenseStartButton()
    {
        bool isActive = false;

        if (!_leftPanel.IsActive && UIManager.Instance.PopupCount == 0)
            isActive = true;

        GetButton((int)Buttons.DefenseStartButton).gameObject.SetActive(isActive);
    }
    private void PlayButtonClickSound()
    {
        SoundManager.Instance.Play2D(ESound.Effect, "Button");
    }
    public override void RefreshUI()
    {
        base.RefreshUI();

        //TODO : Localization
        _topPanel.RefreshUI();
        _bottomPanel.RefreshUI();
        _leftPanel.RefreshUI();
        _defensePanel.RefreshUI();
        UpdateContent();
    }
}
