using UnityEngine;
using UnityEngine.UI;
using static Define;

public abstract class UI_BottomPanelBase : UI_UGUI
{
    protected UI_LeftPanelBase _leftPanel;

    protected override void Awake()
    {
        base.Awake();

        // 파생 클래스에서 모든 바인딩 수행
        PerformBinding();

        // 파생 클래스에서 이벤트 등록
        RegisterButtonEvents();

        // 이벤트 구독
        EventManager.Instance.AddEvent(EEventType.UI_LeftPanelStateChanged, OnLeftPanelStateChanged);
    }

    // LeftPanel 상태 변경 처리
    protected virtual void OnLeftPanelStateChanged()
    {
        UpdateMenuButtonState();
    }

    // 메뉴 버튼 상태 업데이트 - 파생 클래스에서 호출
    protected void UpdateMenuButtonState()
    {
        if (_leftPanel == null)
            return;

        // 파생 클래스에서 구현한 GetMenuButton() 사용
        Button menuButton = GetMenuButton();
        if (menuButton != null)
        {
            bool shouldDisable = _leftPanel.gameObject.activeSelf || _leftPanel.HasActiveOptionPanel();
            menuButton.interactable = !shouldDisable;
        }
    }

    // 공통 버튼 클릭 메서드들 - 파생 클래스에서 호출
    protected void OnClickSaveButton()
    {
        //TODO: 저장 로직
        Debug.Log("Save Button Clicked");
    }

    protected void OnClickMenuButton()
    {
        EventManager.Instance.TriggerEvent(EEventType.UI_MenuButtonClicked);
    }

    // LeftPanel 설정
    public void SetInfo(UI_LeftPanelBase leftPanel)
    {
        _leftPanel = leftPanel;
    }

    // 추상 메서드
    protected abstract void PerformBinding();
    protected abstract void RegisterButtonEvents();
    protected abstract Button GetMenuButton(); // 메뉴 버튼 반환

    public override void RefreshUI()
    {
        base.RefreshUI();
        UpdateMenuButtonState();
        //TODO: Localization
    }
}
