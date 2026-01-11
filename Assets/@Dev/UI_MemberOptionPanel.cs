using UnityEngine;
using static Define;

public class UI_MemberOptionPanel : UI_UGUI
{
    enum GameObjects
    {
    }
    enum Buttons
    {
        //¹ã, ³· °ø¿ë 2Â÷ ÆÐ³Î
        MemberListButton,//¸ñ·Ï
        MemberEducationButton,//±³À°

        //NightMenu 2Â÷ ÆÐ³Î
        MemberHireButton,//°í¿ë
        MemberFireButton,//ÇØ°í
    }
    enum Texts
    {
        //¹ã, ³· °ø¿ë 2Â÷ ÆÐ³Î
        MemberListButtonText,
        MemberEducationButtonText,

        //NightMenu 2Â÷ ÆÐ³Î
        MemberHireButtonText,
        MemberFireButtonText,
    }
    private UI_LeftPanel _leftPanel;
    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));

        //¹ã, ³· °ø¿ë
        GetButton((int)Buttons.MemberListButton)?.onClick.AddListener(() => OpenPopup(Buttons.MemberListButton));
        GetButton((int)Buttons.MemberEducationButton)?.onClick.AddListener(() => OpenPopup(Buttons.MemberEducationButton));

        //¹ã
        GetButton((int)Buttons.MemberHireButton)?.onClick.AddListener(() => OpenPopup(Buttons.MemberHireButton));
        GetButton((int)Buttons.MemberFireButton)?.onClick.AddListener(() => OpenPopup(Buttons.MemberFireButton));

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
            case Buttons.MemberListButton:
                UI_MemberListPopup memberListPopup = UIManager.Instance.ShowPopupUI<UI_MemberListPopup>();
                break;
            case Buttons.MemberEducationButton:
                UI_MemberSelectionPopup memberSelectionPopup = UIManager.Instance.ShowPopupUI<UI_MemberSelectionPopup>();
                memberSelectionPopup.SetInfo(MemberManager.Instance.MainCharacter);
                break;
            case Buttons.MemberHireButton:
                UI_MemberHireMethodsPopup memberHireMethodsPopup = UIManager.Instance.ShowPopupUI<UI_MemberHireMethodsPopup>();
                break;
            case Buttons.MemberFireButton:
                UI_MemberFirePopup memberFirePopup = UIManager.Instance.ShowPopupUI<UI_MemberFirePopup>();
                break;
        }

        if (_leftPanel != null)
            _leftPanel.IsActive = false;
    }
    public override void RefreshUI()
    {
        base.RefreshUI();

        //TODO : Localization
    }
}
