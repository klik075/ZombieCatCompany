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

    private bool _isDefenseActive = false;
    protected override void PerformBinding()
    {
        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
    }

    protected override void RegisterSpecificEvents()
    {
        EventManager.Instance.AddEvent(EEventType.FenceStateChanged, UpdateFenceText);
        EventManager.Instance.AddEvent(EEventType.LoadCompleted, UpdateCompanyText);
        EventManager.Instance.AddEvent(EEventType.FenceDamaged, UpdateFenceText);

        EventManager.Instance.AddEvent(EEventType.DefenseStarted, OnDefenseStarted);
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();

        EventManager.Instance.RemoveEvent(EEventType.FenceStateChanged, UpdateFenceText);
        EventManager.Instance.RemoveEvent(EEventType.LoadCompleted, UpdateCompanyText);
        EventManager.Instance.RemoveEvent(EEventType.FenceDamaged, UpdateFenceText);

        EventManager.Instance.RemoveEvent(EEventType.DefenseStarted, OnDefenseStarted);

    }
    private void OnDefenseStarted()
    {
        _isDefenseActive = true;
        UpdateMenuButtonText(); // MenuButton 상태 업데이트
        Debug.Log("[UI_BottomPanel_Morning] Defense started - MenuButton disabled");
    }
    protected override MenuButtonState GetMenuButtonState()
    {
        // 디펜스 활성 시 무조건 비활성화
        if (_isDefenseActive)
        {
            return MenuButtonState.Disabled;
        }

        // 나머지는 부모 클래스 로직 사용
        return base.GetMenuButtonState();
    }
    private void UpdateFenceText()
    {
        if (FenceManager.Instance.CurrentFence == null)
            return;

        // 울타리 HP 업데이트
        GetText((int)Texts.FenceHpText).text = FenceManager.Instance.CurrentFence.CurrentHp.ToString();

        // 울타리 내구도 업데이트
        GetText((int)Texts.FenceDurabilityText).text = FenceManager.Instance.CurrentFence.CurrentDurability.ToString();
    }
    private void UpdateCompanyText()
    {
        GetText((int)Texts.CompanyText).text = GameManager.Instance.CompanyName ?? "";
    }

    protected override Button GetSaveButton() => GetButton((int)Buttons.SaveButton);
    protected override Button GetMenuButton() => GetButton((int)Buttons.MenuButton);
    protected override TMP_Text GetSaveButtonText() => GetText((int)Texts.SaveButtonText);
    protected override TMP_Text GetMenuButtonText() => GetText((int)Texts.MenuButtonText);

    public override void RefreshUI()
    {
        base.RefreshUI();
        //TODO: 낮 전용 UI 업데이트
        //TODO: Localization
    }
}
