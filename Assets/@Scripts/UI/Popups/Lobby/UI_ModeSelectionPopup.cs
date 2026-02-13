using System;
using UnityEngine;

public class UI_ModeSelectionPopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
    }
    enum Buttons
    {
        //MainTitle
        NextButton,
        PreviousButton,

        //SubBottom
        OkayButton,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //SubMiddle
        SubMiddleNameText,

        //LeftContent
        RoleText,
        FundsNameText,
        FundsText,

        //RightContent
        DescriptionText,
        FoodNameText,
        FoodText,

        //SubBottom
        OkayButtonText,
    }
    enum Images
    {
        //LeftContent
        MemberImage,
    }

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        GetButton((int)Buttons.NextButton).onClick.AddListener(OnClickNextButton);
        GetButton((int)Buttons.PreviousButton).onClick.AddListener(OnClickPreviousButton);
        GetButton((int)Buttons.OkayButton).onClick.AddListener(OnClickOkayButton);
    }
    private void OnClickNextButton()
    {
        //다음 모드로 내용 업데이트
    }
    private void OnClickPreviousButton()
    {
        //이전 모드로 내용 업데이트
    }
    private void OnClickOkayButton()
    {

    }
    protected override void Start()
    {
        base.Start();

    }
    private void UpdateContent()
    {
        //모드에 맞게 내용 업데이트
    }
    public override void RefreshUI()
    {
        base.RefreshUI();

        //TODO : Localization
    }
}
