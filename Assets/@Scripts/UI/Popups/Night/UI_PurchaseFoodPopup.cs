using System;
using UnityEngine;

public class UI_PurchaseFoodPopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
        //BG
        BG,
    }
    enum Buttons
    {
        //SubBottom
        InputButton,
        OkayButton
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //SubMiddle
        SubMiddleNameText,

        //LeftContent
        PossessionNameText,
        PossessionText,

        //RightContent
        DescriptionText,

        InputFoodNameText,
        InputFoodText,

        CostNameText,
        CostText,

        TotalCostNameText,
        TotalCostText,

        //SubBottom
        InputButtonText,
        OkayButtonText,
    }
    enum Images
    {
        //LeftContent
        FoodImage,

    }

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));
    }
    public void SetInfo()
    {
        UpdateContent();
    }
    private void UpdateContent()
    {

    }

    public override void RefreshUI()
    {
        base.RefreshUI();

    }

    
}
