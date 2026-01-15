using UnityEngine;

public class UI_NightGame : UI_UGUI, IUI_Scene
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
    private UI_LeftPanel _leftPanel;
    private UI_BottomPanel _bottomPanel;
    
    protected override void Awake()
    {
        base.Awake();

        _topPanel = Utils.FindChildComponent<UI_TopPanel>(gameObject, recursive: true);
        _leftPanel = Utils.FindChildComponent<UI_LeftPanel>(gameObject, recursive: true);
        _bottomPanel = Utils.FindChildComponent<UI_BottomPanel>(gameObject, recursive: true);

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
    }
}
