using UnityEngine;
using static Define;
public class UI_BottomPanel : UI_UGUI
{
    enum GameObjects
    {
        //BottomPanel1
        MorningBottomPanel1,
        GameDevBottomPanel1,
        NightBottomPanel1,
    }
    enum Buttons
    {
        //BottomPanel2
        SaveButton,
        MenuButton,
    }
    enum Texts
    {
        //MorningBottomPanel1
        FenceHpNameText,
        FenceHpText,
        FenceDurabilityNameText,
        FenceDurabilityText,

        //GameDevBottomPanel1
        NewWorkNameText,
        NewWorkText,

        QualityText1,
        QualityText2,
        QualityText3,
        QualityText4,
        QualityText5,

        //NightBottomPanel1
        AnnualProfitNameText,
        AnnualProfitText,
        DevelopmentStatusText,

        //BottomPanel2
        SaveButtonText,
        MenuButtonText,
    }
    enum Images
    {
        //GameDevBottomPanel1
        QualityImage1,
        QualityImage2,
        QualityImage3,
        QualityImage4,
        QualityImage5,
    }

    private UI_LeftPanel _leftPanel;
    
    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));

        //BottomPanel2
        GetButton((int)Buttons.SaveButton).onClick.AddListener(OnClickSaveButton);
        GetButton((int)Buttons.MenuButton).onClick.AddListener(OnClickMenuButton);

        EventManager.Instance.AddEvent(EEventType.UI_LeftPanelStateChanged, UpdateMenuButtonText);
        EventManager.Instance.AddEvent(EEventType.UI_PopupClosed, UpdateMenuButtonText);
        EventManager.Instance.AddEvent(EEventType.UI_PopupOpened, UpdateMenuButtonText);
    }

    // 부모(UI_NightGame)로부터 LeftPanel 참조 받기
    public void SetInfo(UI_LeftPanel leftPanel)
    {
        _leftPanel = leftPanel;
    }

    private void OnClickSaveButton()
    {
        Debug.Log("SaveButton Clicked");
    }

    private void OnClickMenuButton()
    {
        // 1순위: 팝업이 열려있으면 팝업 닫기
        if (UIManager.Instance.GetLastPopupUI<UI_Base>() != null)
        {
            UIManager.Instance.ClosePopupUI();
            return;
        }

        if (_leftPanel == null)
        {
            EventManager.Instance.TriggerEvent(EEventType.UI_MenuButtonClicked);
            return;
        }

        // 2순위: LeftPanel의 OptionPanel이 열려있는 상태이면 닫기
        if (_leftPanel.HasActiveOptionPanel())
        {
            _leftPanel.CloseCurrentOptionPanel();
        }
        // 3순위: LeftPanel이 열려있는 상태이면 닫기
        else if (_leftPanel.gameObject.activeSelf)
        {
            _leftPanel.gameObject.SetActive(false);
            UpdateMenuButtonText();
        }
        // 4순위: 아무것도 열려있지 않으면 메뉴 열기
        else
        {
            EventManager.Instance.TriggerEvent(EEventType.UI_MenuButtonClicked);
        }
    }

    private void UpdateMenuButtonText()// LeftPanel 상태에 따라 메뉴 버튼 텍스트 변경
    {
        GetButton((int)Buttons.MenuButton).interactable = true;
        // 팝업이 열려있으면 "뒤로"
        UI_Base lastPopupUI = UIManager.Instance.GetLastPopupUI<UI_Base>();
        if (lastPopupUI != null)
        {
            if (lastPopupUI is IClickableUI)
            {
                GetText((int)Texts.MenuButtonText).text = "";
                GetButton((int)Buttons.MenuButton).interactable = false;
                return;
            }

            GetText((int)Texts.MenuButtonText).text = "@뒤로";
            return;
        }

        if (_leftPanel == null) 
            return;

        if (_leftPanel.gameObject.activeSelf || _leftPanel.HasActiveOptionPanel())
        {
            GetText((int)Texts.MenuButtonText).text = "@뒤로";
        }
        else
        {
            GetText((int)Texts.MenuButtonText).text = "@메뉴";
        }
    }

    public override void RefreshUI()
    {
        base.RefreshUI();

        //TODO : Localization
    }
}
