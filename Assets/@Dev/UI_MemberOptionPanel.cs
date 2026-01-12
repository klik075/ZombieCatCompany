using UnityEngine;
using static Define;

public class UI_MemberOptionPanel : UI_UGUI
{
    enum GameObjects
    {
    }
    enum Buttons
    {
        //밤, 낮 공용 2차 패널
        MemberListButton,//목록
        MemberEducationButton,//교육

        //NightMenu 2차 패널
        MemberHireButton,//고용
        MemberFireButton,//해고
    }
    enum Texts
    {
        //밤, 낮 공용 2차 패널
        MemberListButtonText,
        MemberEducationButtonText,

        //NightMenu 2차 패널
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

        //밤, 낮 공용
        GetButton((int)Buttons.MemberListButton)?.onClick.AddListener(() => OpenPopup(Buttons.MemberListButton));
        GetButton((int)Buttons.MemberEducationButton)?.onClick.AddListener(() => OpenPopup(Buttons.MemberEducationButton));

        //밤
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
                if (GameManager.Instance.IsRecruiting)
                {
                    UI_ChatPopup chatPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
                    chatPopup.SetInfo(MemberManager.MAIN_CHARACTER_ID, "이미 모집 중이다냥.");//대사 테이블이 필요할 듯.
                }
                else
                {
                    UI_MemberHireMethodsPopup memberHireMethodsPopup = UIManager.Instance.ShowPopupUI<UI_MemberHireMethodsPopup>();
                }
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
