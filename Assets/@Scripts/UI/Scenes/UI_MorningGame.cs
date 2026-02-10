using UnityEngine;

public class UI_MorningGame : UI_UGUI, IUI_Scene
{
    enum GameObjects
    {

    }
    enum Buttons
    {

    }
    enum Texts
    {

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
    }

    public override void RefreshUI()
    {
        base.RefreshUI();

        //TODO : Localization
        _topPanel.RefreshUI();
        _bottomPanel.RefreshUI();
        _leftPanel.RefreshUI();
        _defensePanel.RefreshUI();
    }
}
