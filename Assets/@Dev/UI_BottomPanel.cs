using System.ComponentModel.Design;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
    private enum MenuButtonState
    {
        Menu,           // "메뉴" 표시
        Back,           // "뒤로" 표시
        Disabled        // 비활성화 (텍스트 없음)
    }
    private static class MenuButtonConfig
    {
        public static string GetText(MenuButtonState state)
        {
            switch (state)
            {
                case MenuButtonState.Menu:
                    return "@메뉴"; // 나중에 LocalizationManager로 대체 가능
                case MenuButtonState.Back:
                    return "@뒤로";
                case MenuButtonState.Disabled:
                    return "";
                default:
                    return "";
            }
        }

        public static bool IsInteractable(MenuButtonState state)
        {
            return state != MenuButtonState.Disabled;
        }
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

        // 2순위: LeftPanel의 OptionPanel이 열려있는 상태이면 닫기
        if (_leftPanel.HasActiveOptionPanel())
        {
            _leftPanel.CloseCurrentOptionPanel();
            return;
        }

        // 3순위: LeftPanel이 열려있는 상태이면 닫기
        if (_leftPanel.gameObject.activeSelf)
        {
            _leftPanel.gameObject.SetActive(false);
            UpdateMenuButtonText();
            return;
        }

        // 4순위: 아무것도 열려있지 않으면 메뉴 열기
        EventManager.Instance.TriggerEvent(EEventType.UI_MenuButtonClicked);
    }
    private MenuButtonState GetMenuButtonState()
    {
        // 팝업이 열려있는지 확인
        UI_Base lastPopupUI = UIManager.Instance.GetLastPopupUI<UI_Base>();
        if (lastPopupUI != null)
        {
            // IClickableUI 팝업이면 비활성화
            if (lastPopupUI is IClickableUI)
            {
                return MenuButtonState.Disabled;
            }
            // 일반 팝업이면 "뒤로"
            return MenuButtonState.Back;
        }

        // LeftPanel이나 OptionPanel이 열려있으면 "뒤로"
        if (_leftPanel.gameObject.activeSelf || _leftPanel.HasActiveOptionPanel())
        {
            return MenuButtonState.Back;
        }

        // 모두 닫혀있으면 "메뉴"
        return MenuButtonState.Menu;
    }
    private void UpdateMenuButtonText()
    {
        MenuButtonState state = GetMenuButtonState();
        ApplyMenuButtonState(state);
    }

    private void ApplyMenuButtonState(MenuButtonState state)
    {
        GetButton((int)Buttons.MenuButton).interactable = MenuButtonConfig.IsInteractable(state);
        GetText((int)Texts.MenuButtonText).text = MenuButtonConfig.GetText(state);
    }

    public override void RefreshUI()
    {
        base.RefreshUI();
        UpdateMenuButtonText();
        //TODO : Localization
    }
}
