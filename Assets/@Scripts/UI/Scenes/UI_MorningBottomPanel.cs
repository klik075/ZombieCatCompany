using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UI_MorningBottomPanel : UI_BottomPanelBase
{
    enum GameObjects
    {
        MorningBottomPanel1,
    }
    
    enum Buttons
    {
        // 공통 버튼 (자식에서 바인딩)
        SaveButton,
        MenuButton,
    }
    
    enum Texts
    {
        // 공통 텍스트
        SaveButtonText,
        MenuButtonText,

        // MorningBottomPanel1
        FenceHpNameText,
        FenceHpText,
        FenceDurabilityNameText,
        FenceDurabilityText,
    }
    protected override void PerformBinding()
    {
        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
    }

    protected override void RegisterButtonEvents()
    {
        // 공통 버튼 사건 (Base 메서드 사용)
        GetButton((int)Buttons.SaveButton)?.onClick.AddListener(OnClickSaveButton);
        GetButton((int)Buttons.MenuButton)?.onClick.AddListener(OnClickMenuButton);

        // 낮 전용 버튼 사건
        // ...
    }

    protected override Button GetMenuButton()
    {
        return GetButton((int)Buttons.MenuButton);
    }

    protected override void OnLeftPanelStateChanged()
    {
        base.OnLeftPanelStateChanged();
        
        // 낮 전용 상태 변경 처리
    }

    public override void RefreshUI()
    {
        base.RefreshUI();
        //TODO: Morning 전용 Localization
    }
}
