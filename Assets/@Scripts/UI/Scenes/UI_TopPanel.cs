using UnityEngine;
using static Define;

public class UI_TopPanel : UI_UGUI
{
    enum GameObjects
    {
    }
    enum Buttons
    {
    }
    enum Texts
    {
        //TopPanel
        SurvivalYearText,
        GoldText,
        FoodText,
    }
    
    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));

        // 게임 데이터 변경 이벤트 구독
        EventManager.Instance.AddEvent(EEventType.YearChanged, OnYearChanged);
        EventManager.Instance.AddEvent(EEventType.GoldChanged, OnGoldChanged);
        EventManager.Instance.AddEvent(EEventType.FoodChanged, OnFoodChanged);
    }
    protected virtual void OnDestroy()
    {
        // 게임 데이터 변경 이벤트 구독 해제
        EventManager.Instance.RemoveEvent(EEventType.YearChanged, OnYearChanged);
        EventManager.Instance.RemoveEvent(EEventType.GoldChanged, OnGoldChanged);
        EventManager.Instance.RemoveEvent(EEventType.FoodChanged, OnFoodChanged);
    }
    private void OnYearChanged()
    {
        UpdateSurvivalYearUI(GameManager.Instance.Year);
    }

    private void OnGoldChanged()
    {
        UpdateGoldUI(GameManager.Instance.Gold);
    }

    private void OnFoodChanged()
    {
        UpdateFoodUI(GameManager.Instance.Food);
    }

    private void UpdateSurvivalYearUI(int year)
    {
        GetText((int)Texts.SurvivalYearText).text = $"{year}년차";
    }

    private void UpdateGoldUI(int gold)
    {
        GetText((int)Texts.GoldText).text = $"보유 자금 : {gold:N0}G";
    }

    private void UpdateFoodUI(int food)
    {
        GetText((int)Texts.FoodText).text = $"보유 통조림 :{food}개";
    }

    public override void RefreshUI()
    {
        base.RefreshUI();
        
        // RefreshUI 호출 시 모든 데이터 다시 업데이트
        OnYearChanged();
        OnGoldChanged();
        OnFoodChanged();
        //TODO : Localization
    }
}
