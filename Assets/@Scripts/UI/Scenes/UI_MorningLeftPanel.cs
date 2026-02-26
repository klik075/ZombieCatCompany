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
    }
    
    enum Texts
    {
        // 공통 텍스트
        MemberButtonText,
        SystemButtonText,
        
        // 낮 전용 텍스트
        FenceButtonText,
    }

    // 바인딩만 수행
    protected override void PerformBinding()
    {
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();

    }
    // 공통 버튼 반환 (부모에서 자동 등록에 사용)
    protected override UnityEngine.UI.Button GetMemberButton() => GetButton((int)Buttons.MemberButton);
    protected override UnityEngine.UI.Button GetSystemButton() => GetButton((int)Buttons.SystemButton);

    // 낮 전용 버튼 이벤트 등록
    protected override void RegisterSpecificEvents()
    {
        GetButton((int)Buttons.FenceButton).onClick.AddListener(OnClickFenceButton);
    }

    // 낮 전용 버튼 처리
    private void OnClickFenceButton()
    {
        UI_FenceStatePopup fenceStatePopup = UIManager.Instance.ShowPopupUI<UI_FenceStatePopup>();
        
        IsActive = false;
    }
    public override void RefreshUI()
    {
        base.RefreshUI();
        //TODO: Morning 전용 Localization
    }
}
