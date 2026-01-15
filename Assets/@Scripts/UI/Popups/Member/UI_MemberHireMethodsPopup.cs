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
        switch (button)
        {
            case Buttons.HireMethodButton1:
                CoroutineManager.Instance.Run(HireProcessCoroutine(HireMethodType.Internet));
                break;
        }

        UIManager.Instance.ClosePopupUI();
    }
    private IEnumerator HireProcessCoroutine(HireMethodType hireMethodType)
    {
        yield return CoroutineManager.Instance.Run(MemberManager.Instance.StartHiringProcess(hireMethodType));

        // 팝업이 모두 닫힐 때까지 대기
        while (UIManager.Instance.PopupCount > 0)
        {
            yield return null;
        }

        UI_ChatPopup chatPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
        chatPopup.SetInfo(MemberManager.MAIN_CHARACTER_ID, MemberManager.Instance.HireResult.Message, OpenMemberHirePopup);

        // 모집 완료
        GameManager.Instance.IsRecruiting = false;
    }
    private void OpenMemberHirePopup()
    {
        UI_MemberHirePopup memberHirePopup = UIManager.Instance.ShowPopupUI<UI_MemberHirePopup>();
    }
    public override void RefreshUI()
    {
        base.RefreshUI();

    }
}
