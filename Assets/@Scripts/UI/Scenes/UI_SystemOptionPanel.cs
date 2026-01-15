using System;
using UnityEngine;

public class UI_SystemOptionPanel : UI_UGUI
{
    enum GameObjects
    {
    }
    enum Buttons
    {
        //¹ã, ³· °ø¿ë
        SystemSettingButton,
        SystemHowToPlayButton,
    }
    enum Texts
    {
        //¹ã, ³· °ø¿ë
        SystemSettingButtonText,
        SystemHowToPlayButtonText,
    }
    private UI_LeftPanel _leftPanel;
    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));

        GetButton((int)Buttons.SystemSettingButton)?.onClick.AddListener(() => OpenPopup(Buttons.SystemSettingButton));
        GetButton((int)Buttons.SystemHowToPlayButton)?.onClick.AddListener(() => OpenPopup(Buttons.SystemHowToPlayButton));

        gameObject.SetActive(false);
    }
    public void SetInfo(UI_LeftPanel leftPanel)
    {
        _leftPanel = leftPanel;
    }
    private void OpenPopup(Buttons buttonType)
    {
        switch (buttonType)
        {
            case Buttons.SystemSettingButton:
                UI_SettingPopup settingPopup = UIManager.Instance.ShowPopupUI<UI_SettingPopup>();
                break;
            case Buttons.SystemHowToPlayButton:
                UI_GameRulesPopup gameRulesPopup = UIManager.Instance.ShowPopupUI<UI_GameRulesPopup>();
                break;
        }

        if (_leftPanel != null)
            _leftPanel.IsActive = false;
    }

    protected override void Start()
    {
        base.Start();
        
    }
    public override void RefreshUI()
    {
        base.RefreshUI();

        //TODO : Localization
    }
}
