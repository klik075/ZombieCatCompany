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
    private UI_LeftPanelBase _leftPanel;
    
    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));

        //BottomPanel2
        GetButton((int)Buttons.SaveButton).onClick.AddListener(OnClickSaveButton);
        GetButton((int)Buttons.MenuButton).onClick.AddListener(OnClickMenuButton);

        // UI 관련 이벤트 구독
        EventManager.Instance.AddEvent(EEventType.UI_LeftPanelStateChanged, UpdateUIStates);
        EventManager.Instance.AddEvent(EEventType.UI_PopupClosed, UpdateUIStates);
        EventManager.Instance.AddEvent(EEventType.UI_PopupOpened, UpdateUIStates);
        
        // 게임 데이터 변경 이벤트 구독
        EventManager.Instance.AddEvent(EEventType.GameDevStateChanged, OnGameDevStateChanged);
        EventManager.Instance.AddEvent(EEventType.GameDevProgressChanged, OnGameDevProgressChanged);
        EventManager.Instance.AddEvent(EEventType.QualityChanged, OnQualityChanged);
        EventManager.Instance.AddEvent(EEventType.AnnualProfitChanged, OnAnnualProfitChanged);
        EventManager.Instance.AddEvent(EEventType.NewDevTitleChanged, OnNewDevTitleChanged);
        EventManager.Instance.AddEvent(EEventType.GameStateChanged, OnGameStateChanged);
    }

    // 부모(UI_NightGame)로부터 LeftPanel 참조 받기
    public void SetInfo(UI_LeftPanelBase leftPanel)
    {
        _leftPanel = leftPanel;
    }

    private void OnAnnualProfitChanged()
    {
        UpdateAnnualProfitUI(GameManager.Instance.AnnualProfit);
    }

    private void OnNewDevTitleChanged()
    {
        UpdateDevelopmentStatusUI(GameDevManager.Instance.CurrentGameTitle);
    }

    private void OnGameDevStateChanged()
    {
        UpdateBottomPanelBasedOnDevState();
        UpdateNewWorkText();
    }
    private void OnGameDevProgressChanged()
    {
        UpdateNewWorkText();
    }
    private void OnQualityChanged()
    {
        UpdateQualityText();
    }
    private void OnGameStateChanged()
    {
        UpdateSaveButtonState();
        UpdateBottomPanelBasedOnGameState();
    }
    private void UpdateQualityText()
    {
        if (GetObject((int)GameObjects.GameDevBottomPanel1).activeSelf == false)
            return;

        GetText((int)Texts.QualityText1).text = GameDevManager.Instance.GetQualityScore(EQualityType.Fun).ToString();
        GetText((int)Texts.QualityText2).text = GameDevManager.Instance.GetQualityScore(EQualityType.Nyang).ToString();
        GetText((int)Texts.QualityText3).text = GameDevManager.Instance.GetQualityScore(EQualityType.Graphics).ToString();
        GetText((int)Texts.QualityText4).text = GameDevManager.Instance.GetQualityScore(EQualityType.Sound).ToString();
        GetText((int)Texts.QualityText5).text = GameDevManager.Instance.GetQualityScore(EQualityType.Bug).ToString();
    }
    private void UpdateNewWorkText()
    {
        if (GetObject((int)GameObjects.GameDevBottomPanel1).activeSelf == false)
            return;

        EGameDevType currentDevType = GameDevManager.Instance.CurrentGameDevType;
        switch(currentDevType)
        {
            case EGameDevType.None:
                break;
            case EGameDevType.Scenario:
            case EGameDevType.Graphics:
            case EGameDevType.Sound:
            case EGameDevType.Complete:
                GetText((int)Texts.NewWorkText).text = $"{GameDevManager.Instance.Progress}%";
                break;
            case EGameDevType.Debug:
                GetText((int)Texts.NewWorkText).text = "디버그 중";
                break;
            default:
                break;
        }
    }
    private void UpdateBottomPanelBasedOnDevState()
    {
        EGameDevType currentDevType = GameDevManager.Instance.CurrentGameDevType;
        
        bool isNightPanel = (currentDevType == EGameDevType.None);
        bool isGameDevPanel = !isNightPanel;
        
        // NightBottomPanel1 활성화/비활성화
        GetObject((int)GameObjects.NightBottomPanel1).SetActive(isNightPanel);
        
        // GameDevBottomPanel1 활성화/비활성화
        GetObject((int)GameObjects.GameDevBottomPanel1).SetActive(isGameDevPanel);
    }
    private void UpdateBottomPanelBasedOnGameState()
    {
        EGameState gameState = GameManager.Instance.GameState;
        bool isMorningPanel = (gameState == EGameState.Morning);
        GetObject((int)GameObjects.MorningBottomPanel1).SetActive(isMorningPanel);
        GetObject((int)GameObjects.NightBottomPanel1).SetActive(!isMorningPanel);
    }
    private void UpdateAnnualProfitUI(int annualProfit)
    {
        GetText((int)Texts.AnnualProfitText).text = $"{annualProfit:N0}G";
    }

    private void UpdateDevelopmentStatusUI(string newDevTitle)
    {
        // NewDevTitle이 비어있으면 기본 메시지 표시
        if (string.IsNullOrEmpty(newDevTitle))
        {
            GetText((int)Texts.DevelopmentStatusText).text = "@신규 개발 없음";
        }
        else
        {
            GetText((int)Texts.DevelopmentStatusText).text = $"{newDevTitle}";
        }
    }

    private void OnClickSaveButton()
    {
        SaveManager.Instance.Save();
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
            _leftPanel.IsActive = false;
            UpdateMenuButtonText();
            return;
        }

        // 4순위: 아무것도 열려있지 않으면 메뉴 열기
        EventManager.Instance.TriggerEvent(EEventType.UI_MenuButtonClicked);
    }

    // SaveButton 상태 확인 (Left 패널이나 Popup이 열려있으면 비활성화)
    private bool IsSaveButtonEnabled()
    {
        if (GameManager.Instance.GameState != EGameState.Night && GameManager.Instance.GameState != EGameState.Recruiting)
        {
            return false;
        }

        // 팝업이 열려있는지 확인
        UI_Base lastPopupUI = UIManager.Instance.GetLastPopupUI<UI_Base>();
        if (lastPopupUI != null)
        {
            return false; // 팝업이 있으면 비활성화
        }

        // LeftPanel이나 OptionPanel이 열려있으면 비활성화
        if (_leftPanel.gameObject.activeSelf || _leftPanel.HasActiveOptionPanel())
        {
            return false;
        }

        // 모두 닫혀있으면 활성화
        return true;
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
    // UI 상태 통합 업데이트 (MenuButton과 SaveButton 모두 제어)
    private void UpdateUIStates()
    {
        UpdateMenuButtonText();
        UpdateSaveButtonState();
    }

    private void UpdateMenuButtonText()
    {
        MenuButtonState state = GetMenuButtonState();
        ApplyMenuButtonState(state);
    }

    private void UpdateSaveButtonState()
    {
        bool isEnabled = IsSaveButtonEnabled();
        GetButton((int)Buttons.SaveButton).interactable = isEnabled;
        GetText((int)Texts.SaveButtonText).text = isEnabled ? "@세이브" : "";

        //Debug.Log($"SaveButton state changed: {(isEnabled ? "Enabled" : "Disabled")}");
    }

    private void ApplyMenuButtonState(MenuButtonState state)
    {
        GetButton((int)Buttons.MenuButton).interactable = MenuButtonConfig.IsInteractable(state);
        GetText((int)Texts.MenuButtonText).text = MenuButtonConfig.GetText(state);
    }

    public override void RefreshUI()
    {
        base.RefreshUI();
        UpdateUIStates(); // MenuButton과 SaveButton 상태 모두 업데이트
        UpdateBottomPanelBasedOnDevState(); // GameDev 상태에 따른 패널 전환
        OnAnnualProfitChanged();
        OnNewDevTitleChanged();
        //TODO : Localization
    }
}
