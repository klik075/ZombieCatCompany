using UnityEngine;
using static Define;

public abstract class UI_LeftPanelBase : UI_UGUI
{
    // 공통 2차 패널들
    protected UI_MemberOptionPanel _memberOptionPanel;
    protected UI_SystemOptionPanel _systemOptionPanel;

    // 현재 활성화된 2차 패널
    private GameObject _currentActiveOptionPanel;
    protected GameObject CurrentActiveOptionPanel
    {
        get => _currentActiveOptionPanel;
        set
        {
            if (_currentActiveOptionPanel != value)
            {
                if (_currentActiveOptionPanel != null)
                    _currentActiveOptionPanel.SetActive(false);
                
                _currentActiveOptionPanel = value;
                NotifyStateChanged();
            }
        }
    }

    // LeftPanel 활성화 상태
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

        // 바인딩은 파생 클래스에서 수행
        PerformBinding();

        // 공통 옵션 패널 초기화
        InitializeCommonPanels();

        // 공통 버튼 이벤트 등록 (부모에서 자동 처리)
        RegisterCommonButtonEvents();

        // 파생 클래스의 고유 이벤트 등록
        RegisterSpecificEvents();

        // 이벤트 등록
        EventManager.Instance.AddEvent(EEventType.UI_MenuButtonClicked, OnMenuButtonClicked);

        gameObject.SetActive(false);
    }

    // 공통 버튼 이벤트 등록 (Member, System 버튼)
    private void RegisterCommonButtonEvents()
    {
        UnityEngine.UI.Button memberButton = GetMemberButton();
        if (memberButton != null)
            memberButton.onClick.AddListener(OnClickMemberButton);

        UnityEngine.UI.Button systemButton = GetSystemButton();
        if (systemButton != null)
            systemButton.onClick.AddListener(OnClickSystemButton);
    }

    // 추상 메서드 - 파생 클래스에서 구현
    protected abstract void PerformBinding(); // 바인딩만 수행
    protected abstract void RegisterSpecificEvents(); // 고유 이벤트 등록
    protected abstract UnityEngine.UI.Button GetMemberButton(); // Member 버튼 반환
    protected abstract UnityEngine.UI.Button GetSystemButton(); // System 버튼 반환

    // 공통 옵션 패널 초기화
    private void InitializeCommonPanels()
    {
        _memberOptionPanel = Utils.FindChildComponent<UI_MemberOptionPanel>(gameObject, recursive: true);
        if (_memberOptionPanel != null)
            _memberOptionPanel.SetInfo(this);

        _systemOptionPanel = Utils.FindChildComponent<UI_SystemOptionPanel>(gameObject, recursive: true);
        if (_systemOptionPanel != null)
            _systemOptionPanel.SetInfo(this);
    }

    // 공통 버튼 클릭 처리
    protected void OnClickMemberButton()
    {
        OpenOptionPanel(_memberOptionPanel.gameObject);
    }

    protected void OnClickSystemButton()
    {
        OpenOptionPanel(_systemOptionPanel.gameObject);
    }

    // 옵션 패널 열기 (공통)
    protected void OpenOptionPanel(GameObject optionPanel)
    {
        if (optionPanel == null)
            return;
        
        if (_currentActiveOptionPanel == optionPanel)
            return;

        optionPanel.SetActive(true);
        CurrentActiveOptionPanel = optionPanel;
    }

    // 메뉴 버튼 클릭 (공통)
    public void OnMenuButtonClicked()
    {
        bool wasActive = gameObject.activeSelf;
        bool newState = !wasActive;
        
        if (newState && HasActiveOptionPanel())
        {
            CurrentActiveOptionPanel = null;
        }
        
        IsActive = newState;
    }

    // 옵션 패널 활성 여부 확인 (공통)
    public bool HasActiveOptionPanel()
    {
        return _currentActiveOptionPanel != null && _currentActiveOptionPanel.activeSelf;
    }

    // 현재 옵션 패널 닫기 (공통)
    public void CloseCurrentOptionPanel()
    {
        CurrentActiveOptionPanel = null;
    }

    // 상태 변경 알림 (공통)
    protected void NotifyStateChanged()
    {
        if(IsActive == false)
            EventManager.Instance.TriggerEvent(EEventType.UI_LeftPanelClosed);
        else
            EventManager.Instance.TriggerEvent(EEventType.UI_LeftPanelOpened);

        EventManager.Instance.TriggerEvent(EEventType.UI_LeftPanelStateChanged);
    }

    public override void RefreshUI()
    {
        base.RefreshUI();
        //TODO: Localization
    }
}
