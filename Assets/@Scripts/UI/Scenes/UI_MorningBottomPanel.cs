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
        EventManager.Instance.AddEvent(EEventType.FenceStateChanged, UpdateFenceText);
        EventManager.Instance.AddEvent(EEventType.LoadCompleted, UpdateCompanyText);
        EventManager.Instance.AddEvent(EEventType.FenceDamaged, UpdateFenceText);
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventManager.Instance.RemoveEvent(EEventType.FenceStateChanged, UpdateFenceText);
        EventManager.Instance.RemoveEvent(EEventType.LoadCompleted, UpdateCompanyText);
        EventManager.Instance.RemoveEvent(EEventType.FenceDamaged, UpdateFenceText);

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
