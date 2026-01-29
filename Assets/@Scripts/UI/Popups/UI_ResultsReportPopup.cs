using System;
using UnityEngine;

public class UI_ResultsReportPopup : UI_UGUI, IUI_Popup, IClickableUI
{
    enum GameObjects
    {
        //BG
        BG,
    }
    enum Buttons
    {
        //SubBottom
        OkayButton,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //Content
        ResultNameText1,
        ResultNameText2,
        ResultNameText3,

        ResultText1,
        ResultText2,
        ResultText3,

        //SubBottom
        OkayButtonText,
    }
    enum Images
    {

    }

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        GetButton((int)Buttons.OkayButton).onClick.AddListener(OnClickOkayButton);
    }
    public void SetInfo()
    {
        UpdateContent();
        GetReward();
    }
    private void UpdateContent()
    {
        GetText((int)Texts.ResultNameText1).text = "게임 이름";
        GetText((int)Texts.ResultText1).text = $"{GameDevManager.Instance.CurrentGameTitle}";

        GetText((int)Texts.ResultNameText2).text = "판매량";
        GetText((int)Texts.ResultText2).text = $"{100}";

        GetText((int)Texts.ResultNameText3).text = "영업 수익";
        GetText((int)Texts.ResultText3).text = $"{100000}";
    }
    private void GetReward()
    {
        GameManager.Instance.Gold += 100000;
        //연간 이익 = 영업 수익 - 개발비용
    }
    private void OnClickOkayButton()
    {
        UIManager.Instance.ClosePopupUI();
    }
    public override void RefreshUI()
    {
        base.RefreshUI();

    }
}
