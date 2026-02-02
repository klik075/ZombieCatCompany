using System.Collections;
using UnityEngine;
using static Define;

public class UI_MemberHireMethodsPopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
        //BG - 상호작용 x
        BG,
    }
    enum Buttons
    {
        //Content
        HireMethodButton1,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //SubMiddle
        SubMiddleNameText,
        SubMiddleCostNameText,

        //Content
        HireMethodNameText1,
        HireMethodCostText1,
    }
    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));

        GetButton((int)Buttons.HireMethodButton1).onClick.AddListener(() => OnClickMethodButton(Buttons.HireMethodButton1));
    }
    private void OnClickMethodButton(Buttons button)
    {
        UI_ChatPopup chatPopup = null;
        switch (button)
        {
            case Buttons.HireMethodButton1:
                if (GameManager.Instance.Gold < 500)// TODO : 모집 비용으로 변경할 것. MemberManager에서 수행할 것
                {
                    //자금 부족 팝업
                    chatPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
                    chatPopup.SetInfo(MemberManager.MAIN_CHARACTER_ID, MessageManager.Instance.GetMessageScript(EMessageType.MoneyLow).Contents);
                    return;
                }
                else
                {
                    GameManager.Instance.Gold -= 500; // TODO : 모집 비용으로 변경할 것. MemberManager에서 수행할 것
                    MemberManager.Instance.StartHire(EHireMethodType.Internet);
                }
                break;
        }

        UIManager.Instance.ClosePopupUI();

        //모집 시작 팝업
        chatPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
        chatPopup.SetInfo(MemberManager.MAIN_CHARACTER_ID, MessageManager.Instance.GetMessageScript(EMessageType.StartRecruiting).Contents);
    }
    public override void RefreshUI()
    {
        base.RefreshUI();

    }
}
