using UnityEngine;
using static Define;

public class UI_LeftPanel : UI_UGUI
{
    enum GameObjects
    {

    }
    enum Buttons
    {
        //밤, 낮 공용 1차 패널
        MemberButton,
        SystemButton,

        //NightMenu - 1차 패널
        GameDevButton,
        DiaryButton,
        DispatchResultButton,

        //MorningMenu - 1차 패널
        FenceButton,

        //Morning 0차 버튼
        DefenseStartButton,
    }
    enum Texts
    {
        //밤, 낮 공용 1차 패널
        MemberButtonText,
        SystemButtonText,

        //NightMenu - 1차 패널
        GameDevButtonText,
        DiaryButtonText,
        DispatchResultButtonText,

        //MorningMenu - 1차 패널
        FenceButtonText,

        //Morning 0차 버튼
        DefenseStartButtonText,
    }

    //2차 패널들 (LeftPanel 내부)
    private UI_MemberOptionPanel _memberOptionPanel;
    private UI_SystemOptionPanel _systemOptionPanel;

    // 현재 활성화된 2차 패널 - 프로퍼티로 자동 이벤트 발생
    private GameObject _currentActiveOptionPanel;
    private GameObject CurrentActiveOptionPanel
    {
        get => _currentActiveOptionPanel;
        set
        {
            if (_currentActiveOptionPanel != value)
            {
                // 기존 패널 닫기
                if (_currentActiveOptionPanel != null)
                    _currentActiveOptionPanel.SetActive(false);
                
                _currentActiveOptionPanel = value;
                NotifyStateChanged();
            }
        }
    }

    // LeftPanel 활성화 상태 - 프로퍼티로 자동 이벤트 발생
    public bool IsActive
    {
        get => gameObject.activeSelf;
        set
        {
            if (gameObject.activeSelf != value)
            {
                gameObject.SetActive(value);

                if (_currentActiveOptionPanel != null)
                    _currentActiveOptionPanel.SetActive(false);

                _currentActiveOptionPanel = null;
                NotifyStateChanged();
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));

        _memberOptionPanel = Utils.FindChildComponent<UI_MemberOptionPanel>(gameObject, recursive: true);
        if (_memberOptionPanel != null)
            _memberOptionPanel.SetInfo(this);

        _systemOptionPanel = Utils.FindChildComponent<UI_SystemOptionPanel>(gameObject, recursive: true);
        if (_systemOptionPanel != null)
            _systemOptionPanel.SetInfo(this);

        // 버튼 이벤트 등록 - 공통
        GetButton((int)Buttons.MemberButton)?.onClick.AddListener(() => OnClickMenuButton(Buttons.MemberButton));
        GetButton((int)Buttons.SystemButton)?.onClick.AddListener(() => OnClickMenuButton(Buttons.SystemButton));

        // 버튼 이벤트 등록 - 밤 메뉴
        GetButton((int)Buttons.GameDevButton)?.onClick.AddListener(() => OnClickMenuButton(Buttons.GameDevButton));
        GetButton((int)Buttons.DiaryButton)?.onClick.AddListener(() => OnClickMenuButton(Buttons.DiaryButton));
        GetButton((int)Buttons.DispatchResultButton)?.onClick.AddListener(() => OnClickMenuButton(Buttons.DispatchResultButton));

        // 버튼 이벤트 등록 - 낮 메뉴
        GetButton((int)Buttons.FenceButton)?.onClick.AddListener(() => OnClickMenuButton(Buttons.FenceButton));

        

        EventManager.Instance.AddEvent(EEventType.UI_MenuButtonClicked, OnMenuButtonClicked);

        gameObject.SetActive(false);
    }

    // 통합된 메뉴 버튼 클릭 처리
    private void OnClickMenuButton(Buttons buttonType)
    {
        switch (buttonType)
        {
            case Buttons.GameDevButton:
                OpenPopup(buttonType);
                break;
            case Buttons.MemberButton:
                OpenOptionPanel(_memberOptionPanel.gameObject);
                break;
            case Buttons.DiaryButton:
                OpenPopup(buttonType);
                break;
            case Buttons.DispatchResultButton:
                OpenPopup(buttonType);
                break;
            case Buttons.SystemButton:
                OpenOptionPanel(_systemOptionPanel.gameObject);
                break;
            case Buttons.FenceButton:
                OpenPopup(buttonType);
                break;
        }
    }

    // 옵션 패널 열기 (Member, System 등)
    private void OpenOptionPanel(GameObject optionPanel)
    {
        if (optionPanel == null)
            return;
        
        if (_currentActiveOptionPanel == optionPanel)
            return;

        // 프로퍼티 setter에서 자동으로 기존 패널 닫고 새 패널 설정
        optionPanel.SetActive(true);
        CurrentActiveOptionPanel = optionPanel;
    }

    public void OnMenuButtonClicked()//메뉴 버튼 클릭 시만, 뒤로는 x
    {
        bool wasActive = gameObject.activeSelf;
        bool newState = !wasActive;
        
        // LeftPanel을 열 때 + 실제로 옵션 패널이 열려있을 때만 닫기
        if (newState && HasActiveOptionPanel())
        {
            CurrentActiveOptionPanel = null; // 프로퍼티 사용 - 자동으로 패널 닫힘
        }
        
        IsActive = newState;
    }

    // 팝업 UI 열기 전 현재 상태 저장 및 LeftPanel 숨김
    private void OpenPopup(Buttons buttonType)
    {
        switch (buttonType)
        {
            case Buttons.GameDevButton:
                if (GameManager.Instance.GameState == EGameState.Night)
                {
                    UI_ProposalPopup memberListPopup = UIManager.Instance.ShowPopupUI<UI_ProposalPopup>();
                }
                else
                {
                    UI_ChatPopup chatPopup;

                    if (GameManager.Instance.GameState == EGameState.Dev)
                    {
                        chatPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
                        chatPopup.SetInfo(MemberManager.MAIN_CHARACTER_ID, MessageManager.Instance.GetMessageScript(EMessageType.NoDev).Contents);
                        break;
                    }

                    chatPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
                    chatPopup.SetInfo(MemberManager.MAIN_CHARACTER_ID, MessageManager.Instance.GetMessageScript(EMessageType.NoDev).Contents);
                }
                break;
            case Buttons.DiaryButton:
                UI_EventPopup diaryPopup = UIManager.Instance.ShowPopupUI<UI_EventPopup>();
                diaryPopup.SetInfo();
                break;
            case Buttons.DispatchResultButton:
                //TODO: 파견 결과 팝업 열기, 파견을 보내지 않았으면 UI_ChatPopup을 열고 "파견을 보내지 않았다냥." text 설정
                //UI_EventPopup은 일기장과 파견 결과가 동일한 팝업을 사용하고 내용은 Diary인지, DispatchResult인지에 따라 다르게 설정
                UI_EventPopup dispatchResultPopup = UIManager.Instance.ShowPopupUI<UI_EventPopup>();
                dispatchResultPopup.SetInfo();
                break;
            case Buttons.FenceButton:
                UI_FenceStatePopup fenceStatePopup = UIManager.Instance.ShowPopupUI<UI_FenceStatePopup>();
                fenceStatePopup.SetInfo();
                break;
        }
        // LeftPanel 숨김
        IsActive = false;
    }

    public bool HasActiveOptionPanel()//현재 옵션 패널이 열려있는지 여부 반환
    {
        return _currentActiveOptionPanel != null && _currentActiveOptionPanel.activeSelf;
    }

    public void CloseCurrentOptionPanel()
    {
        CurrentActiveOptionPanel = null; // 프로퍼티 사용 - 자동으로 패널 닫힘
    }

    private void NotifyStateChanged()
    {
        if(IsActive == false)
            EventManager.Instance.TriggerEvent(EEventType.UI_LeftPanelClosed);
        else
            EventManager.Instance.TriggerEvent(EEventType.UI_LeftPanelOpened);

        EventManager.Instance.TriggerEvent(EEventType.UI_LeftPanelStateChanged);//BottomPanel에서 감지
    }

    public override void RefreshUI()
    {
        base.RefreshUI();

        //TODO : Localization
    }
}
