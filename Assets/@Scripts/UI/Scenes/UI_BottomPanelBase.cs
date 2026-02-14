using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Define;

public abstract class UI_BottomPanelBase : UI_UGUI
{
    protected enum MenuButtonState
    {
        Menu,           // "메뉴" 표시
        Back,           // "뒤로" 표시
        Disabled        // 비활성화 (텍스트 없음)
    }

    // 공통 static class
    protected static class MenuButtonConfig
    {
        public static string GetText(MenuButtonState state)
        {
            switch (state)
            {
                case MenuButtonState.Menu:
                    return "@메뉴"; // 나중에 LocalizationManager로 대체 가능
                case MenuButtonState.Back:
                    return "@뒤로";
                case MenuButtonState.Disabled:
                    return "";
                default:
                    return "";
            }
        }

        public static bool IsInteractable(MenuButtonState state)
        {
            return state != MenuButtonState.Disabled;
        }
    }

    protected UI_LeftPanelBase _leftPanel;

    protected override void Awake()
    {
        base.Awake();

        // 파생 클래스에서 모든 바인딩 수행
        PerformBinding();

        // 공통 버튼 이벤트 등록 (부모에서 자동 처리)
        RegisterCommonButtonEvents();

        // 공통 UI 이벤트 구독
        EventManager.Instance.AddEvent(EEventType.UI_LeftPanelStateChanged, UpdateUIStates);
        EventManager.Instance.AddEvent(EEventType.UI_PopupClosed, UpdateUIStates);
        EventManager.Instance.AddEvent(EEventType.UI_PopupOpened, UpdateUIStates);
        EventManager.Instance.AddEvent(EEventType.GameStateChanged, OnGameStateChanged);

        // 파생 클래스의 고유 이벤트 구독
        RegisterSpecificEvents();

        UpdateUIStates();
    }

    // 공통 버튼 이벤트 등록 (Save, Menu 버튼)
    private void RegisterCommonButtonEvents()
    {
        Button saveButton = GetSaveButton();
        if (saveButton != null)
            saveButton.onClick.AddListener(OnClickSaveButton);

        Button menuButton = GetMenuButton();
        if (menuButton != null)
            menuButton.onClick.AddListener(OnClickMenuButton);
    }

    // LeftPanel 설정
    public void SetInfo(UI_LeftPanelBase leftPanel)
    {
        _leftPanel = leftPanel;
    }

    // 공통 버튼 클릭 처리
    protected void OnClickSaveButton()
    {
        SaveManager.Instance.SaveGameData();
        Debug.Log("SaveButton Clicked");
    }

    protected void OnClickMenuButton()
    {
        // 1순위: 팝업이 열려있으면 팝업 닫기
        if (UIManager.Instance.GetLastPopupUI<UI_Base>() != null)
        {
            UIManager.Instance.ClosePopupUI();
            return;
        }

        // 2순위: LeftPanel의 OptionPanel이 열려있는 상태이면 닫기
        if (_leftPanel.HasActiveOptionPanel())
        {
            _leftPanel.CloseCurrentOptionPanel();
            return;
        }

        // 3순위: LeftPanel이 열려있는 상태이면 닫기
        if (_leftPanel.gameObject.activeSelf)
        {
            _leftPanel.IsActive = false;
            UpdateMenuButtonText();
            return;
        }

        // 4순위: 아무것도 열려있지 않으면 메뉴 열기
        EventManager.Instance.TriggerEvent(EEventType.UI_MenuButtonClicked);
    }

    // SaveButton 상태 확인 (Left 패널이나 Popup이 열려있으면 비활성화)
    protected bool IsSaveButtonEnabled()
    {
        if (GameManager.Instance.GameState != EGameState.Night && GameManager.Instance.GameState != EGameState.Recruiting)
        {
            return false;
        }

        // 팝업이 열려있는지 확인
        UI_Base lastPopupUI = UIManager.Instance.GetLastPopupUI<UI_Base>();
        if (lastPopupUI != null)
        {
            return false; // 팝업이 있으면 비활성화
        }

        // LeftPanel이나 OptionPanel이 열려있으면 비활성화
        if (_leftPanel.gameObject.activeSelf || _leftPanel.HasActiveOptionPanel())
        {
            return false;
        }

        // 모두 닫혀있으면 활성화
        return true;
    }

    protected MenuButtonState GetMenuButtonState()
    {
        // 팝업이 열려있는지 확인
        UI_Base lastPopupUI = UIManager.Instance.GetLastPopupUI<UI_Base>();
        if (lastPopupUI != null)
        {
            // IClickableUI 팝업이면 비활성화
            if (lastPopupUI is IClickableUI)
            {
                return MenuButtonState.Disabled;
            }
            // 일반 팝업이면 "뒤로"
            return MenuButtonState.Back;
        }

        // LeftPanel이나 OptionPanel이 열려있으면 "뒤로"
        if (_leftPanel.gameObject.activeSelf || _leftPanel.HasActiveOptionPanel())
        {
            return MenuButtonState.Back;
        }

        // 모두 닫혀있으면 "메뉴"
        return MenuButtonState.Menu;
    }

    // UI 상태 통합 업데이트 (MenuButton과 SaveButton 모두 제어)
    protected void UpdateUIStates()
    {
        UpdateMenuButtonText();
        UpdateSaveButtonState();
    }

    protected void UpdateMenuButtonText()
    {
        MenuButtonState state = GetMenuButtonState();
        ApplyMenuButtonState(state);
    }

    protected void UpdateSaveButtonState()
    {
        bool isEnabled = IsSaveButtonEnabled();
        Button saveButton = GetSaveButton();
        TMP_Text saveButtonText = GetSaveButtonText();

        if (saveButton != null)
            saveButton.interactable = isEnabled;

        if (saveButtonText != null)
            saveButtonText.text = isEnabled ? "@세이브" : "";
    }

    protected void ApplyMenuButtonState(MenuButtonState state)
    {
        Button menuButton = GetMenuButton();
        TMP_Text menuButtonText = GetMenuButtonText();

        if (menuButton != null)
            menuButton.interactable = MenuButtonConfig.IsInteractable(state);

        if (menuButtonText != null)
            menuButtonText.text = MenuButtonConfig.GetText(state);
    }

    // GameState 변경 처리
    protected virtual void OnGameStateChanged()
    {
        UpdateSaveButtonState();
    }

    // 추상 메서드 - 파생 클래스에서 구현
    protected abstract void PerformBinding();
    protected abstract void RegisterSpecificEvents();
    protected abstract Button GetSaveButton();
    protected abstract Button GetMenuButton();
    protected abstract TMP_Text GetSaveButtonText();
    protected abstract TMP_Text GetMenuButtonText();

    public override void RefreshUI()
    {
        base.RefreshUI();
        UpdateUIStates(); // MenuButton과 SaveButton 상태 모두 업데이트
        //TODO: Localization
    }
}
