using UnityEngine;
using static Define;

public class UI_MorningLeftPanel : UI_LeftPanelBase
{
    enum Buttons
    {
        // 공통 버튼
        MemberButton,
        SystemButton,
        
        // 낮 전용 버튼
        FenceButton,
        DefenseStartButton,
    }
    
    enum Texts
    {
        // 공통 텍스트
        MemberButtonText,
        SystemButtonText,
        
        // 낮 전용 텍스트
        FenceButtonText,
        DefenseStartButtonText,
    }

    // 바인딩만 수행
    protected override void PerformBinding()
    {
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
    }

    // 공통 버튼 이벤트 등록
    protected override void RegisterCommonButtonEvents()
    {
        GetButton((int)Buttons.MemberButton).onClick.AddListener(OnClickMemberButton);
        GetButton((int)Buttons.SystemButton).onClick.AddListener(OnClickSystemButton);
    }

    // 낮 전용 버튼 이벤트 등록
    protected override void RegisterSpecificButtonEvents()
    {
        GetButton((int)Buttons.FenceButton).onClick.AddListener(OnClickFenceButton);
        GetButton((int)Buttons.DefenseStartButton).onClick.AddListener(OnClickDefenseStartButton);
    }

    // 낮 전용 버튼 처리
    private void OnClickFenceButton()
    {
        UI_FenceStatePopup fenceStatePopup = UIManager.Instance.ShowPopupUI<UI_FenceStatePopup>();
        fenceStatePopup.SetInfo();
        
        IsActive = false;
    }

    private void OnClickDefenseStartButton()
    {
        // 방어 시작 로직
        //TODO: 방어 시작 처리
    }

    public override void RefreshUI()
    {
        base.RefreshUI();
        //TODO: Morning 전용 Localization
    }
}
