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
    private UI_LeftPanelBase _leftPanel;
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
    public void SetInfo(UI_LeftPanelBase leftPanel)
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
                memberSelectionPopup.SetInfo(EMemberSelectionType.Education); // ±³À° Å¸ÀÔÀ¸·Î ¼³Á¤
                break;
            case Buttons.MemberHireButton:
                if (GameManager.Instance.GameState == EGameState.Recruiting)
                {
                    //ÀÌ¹Ì ¸ðÁý Áß ÆË¾÷
                    UI_ChatPopup chatPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
                    chatPopup.SetInfo(MemberManager.MAIN_CHARACTER_ID, MessageManager.Instance.GetMessageScript(EMessageType.AlreadyRecruiting).Contents);
                }
                else
                {
                    if (GameManager.Instance.GameState == EGameState.Night)
                    {
                        UI_MemberHireMethodsPopup memberHireMethodsPopup = UIManager.Instance.ShowPopupUI<UI_MemberHireMethodsPopup>();
                        break;
                    }

                    UI_ChatPopup chatPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
                    chatPopup.SetInfo(MemberManager.MAIN_CHARACTER_ID, MessageManager.Instance.GetMessageScript(EMessageType.NoRecruiting).Contents);
                }
                break;
            case Buttons.MemberFireButton:
                if (GameManager.Instance.GameState == EGameState.Night)
                {
                    UI_MemberFirePopup memberFirePopup = UIManager.Instance.ShowPopupUI<UI_MemberFirePopup>();
                    memberFirePopup.SetInfo(EFireType.Normal);
                }
                else
                {
                    UI_ChatPopup chatPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
                    chatPopup.SetInfo(MemberManager.MAIN_CHARACTER_ID, MessageManager.Instance.GetMessageScript(EMessageType.NoFire).Contents);
                }
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
