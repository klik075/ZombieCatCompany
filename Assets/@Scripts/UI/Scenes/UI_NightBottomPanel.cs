using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UI_NightBottomPanel : UI_BottomPanelBase
{
    enum GameObjects
    {
        GameDevBottomPanel1,
        NightBottomPanel1,
    }
    
    enum Buttons
    {
        // 공통 버튼 (자식에서 바인딩)
        SaveButton,
        MenuButton,
    }
    
    enum Texts
    {
        // 공통 텍스트
        SaveButtonText,
        MenuButtonText,

        // GameDevBottomPanel1
        NewWorkNameText,
        NewWorkText,

        QualityText1,
        QualityText2,
        QualityText3,
        QualityText4,
        QualityText5,

        // NightBottomPanel1
        AnnualProfitNameText,
        AnnualProfitText,
        DevelopmentStatusText,
    }

    enum Images
    {
        // GameDevBottomPanel1 이미지들
        QualityImage1,
        QualityImage2,
        QualityImage3,
        QualityImage4,
        QualityImage5,
    }

    protected override void PerformBinding()
    {
        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));
    }

    protected override void RegisterButtonEvents()
    {
        // 공통 버튼 이벤트 (Base 메서드 사용)
        GetButton((int)Buttons.SaveButton)?.onClick.AddListener(OnClickSaveButton);
        GetButton((int)Buttons.MenuButton)?.onClick.AddListener(OnClickMenuButton);

        // 밤 전용 버튼 이벤트
        // ...
    }

    protected override Button GetMenuButton()
    {
        return GetButton((int)Buttons.MenuButton);
    }

    protected override void OnLeftPanelStateChanged()
    {
        base.OnLeftPanelStateChanged();
        
        // 밤 전용 상태 변경 처리
        UpdateNightPanelVisibility();
    }

    private void UpdateNightPanelVisibility()
    {
        EGameState currentState = GameManager.Instance.GameState;
        
        GameObject gameDevPanel = GetObject((int)GameObjects.GameDevBottomPanel1);
        GameObject nightPanel = GetObject((int)GameObjects.NightBottomPanel1);

        if (gameDevPanel != null)
            gameDevPanel.SetActive(currentState == EGameState.Dev);
            
        if (nightPanel != null)
            nightPanel.SetActive(currentState == EGameState.Night);
    }

    public override void RefreshUI()
    {
        base.RefreshUI();
        UpdateNightPanelVisibility();
        //TODO: Night 전용 Localization
    }
}
