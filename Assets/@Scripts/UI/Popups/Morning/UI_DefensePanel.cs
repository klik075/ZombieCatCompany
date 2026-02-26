using UnityEngine;

public class UI_DefensePanel : UI_UGUI
{
    enum GameObjects
    {
        Slider,
    }
    enum Buttons
    {
        SpeedButton,

    }
    enum Texts
    {
        SpeedButtonText,

        //Slider
        RemainingPercentageText,

        //UserCat
        UserCatText
    }

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));

        gameObject.SetActive(false);
    }
    public void Init()
    {
        //물량 설정.
        DefenseManager.Instance.StartDefense();
    }
    protected override void Start()
    {
        base.Start();
    }
    public override void RefreshUI()
    {
        base.RefreshUI();

        //TODO : Localization
    }
}
