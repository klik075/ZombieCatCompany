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
    private UI_LeftPanelBase _leftPanel;
    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));

        //밤, 낮 공용
        GetButton((int)Buttons.MemberListButton)?.onClick.AddListener(() => { PlayButtonClickSound(); OpenPopup(Buttons.MemberListButton); });
        GetButton((int)Buttons.MemberEducationButton)?.onClick.AddListener(() => { PlayButtonClickSound(); OpenPopup(Buttons.MemberEducationButton); });

        //밤
        GetButton((int)Buttons.MemberHireButton)?.onClick.AddListener(() => { PlayButtonClickSound(); OpenPopup(Buttons.MemberHireButton); });
        GetButton((int)Buttons.MemberFireButton)?.onClick.AddListener(() => { PlayButtonClickSound(); OpenPopup(Buttons.MemberFireButton); });

        gameObject.SetActive(false);
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        UpdateContent();
    }
    private void UpdateContent()
    {
        GetText((int)Texts.MemberListButtonText).text = "목록";
        GetText((int)Texts.MemberEducationButtonText).text = "교육";

        if (GetText((int)Texts.MemberHireButtonText) != null)
            GetText((int)Texts.MemberHireButtonText).text = "고용";
        if (GetText((int)Texts.MemberFireButtonText) != null)
            GetText((int)Texts.MemberFireButtonText).text = "해고";
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
                memberSelectionPopup.SetInfo(EMemberSelectionType.Education); // 교육 타입으로 설정
                break;
            case Buttons.MemberHireButton:
                if (GameManager.Instance.GameState == EGameState.Recruiting)
                {
                    //이미 모집 중 팝업
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
    private void PlayButtonClickSound()
    {
        SoundManager.Instance.Play2D(ESound.Effect, "Button");
    }
    public override void RefreshUI()
    {
        base.RefreshUI();
        UpdateContent();
        //TODO : Localization
    }
}
