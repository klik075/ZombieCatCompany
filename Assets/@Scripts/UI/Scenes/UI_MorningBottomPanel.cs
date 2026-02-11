using TMPro;
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
        SaveButton,
        MenuButton,
    }
    
    enum Texts
    {
        // 공통
        SaveButtonText,
        MenuButtonText,

        // MorningBottomPanel1
        FenceHpNameText,
        FenceHpText,
        FenceDurabilityNameText,
        FenceDurabilityText,
        CompanyText
    }

    protected override void PerformBinding()
    {
        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
    }

    protected override void RegisterSpecificEvents()
    {
        // 낮 전용 게임 데이터 변경 이벤트 구독
        // 예: 울타리 HP 변경, 내구도 변경 등
    }

    protected override Button GetSaveButton() => GetButton((int)Buttons.SaveButton);
    protected override Button GetMenuButton() => GetButton((int)Buttons.MenuButton);
    protected override TMP_Text GetSaveButtonText() => GetText((int)Texts.SaveButtonText);
    protected override TMP_Text GetMenuButtonText() => GetText((int)Texts.MenuButtonText);

    // 낮 전용 UI 업데이트 메서드들
    //TODO: 울타리 HP, 내구도 업데이트 메서드 추가

    public override void RefreshUI()
    {
        base.RefreshUI();
        //TODO: 낮 전용 UI 업데이트
        //TODO: Localization
    }
}
