using UnityEngine;

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
    }

    public override void RefreshUI()
    {
        base.RefreshUI();

        //TODO : Localization
    }
}
