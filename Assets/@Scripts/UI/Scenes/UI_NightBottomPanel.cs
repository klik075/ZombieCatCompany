using TMPro;
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
        SaveButton,
        MenuButton,
    }
    
    enum Texts
    {
        // 공통
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
        CompanyNameText,
        CompanyText,
    }

    enum Images
    {
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

    protected override void RegisterSpecificEvents()
    {
        // 밤 전용 게임 데이터 변경 이벤트 구독
        EventManager.Instance.AddEvent(EEventType.GameDevStateChanged, OnGameDevStateChanged);
        EventManager.Instance.AddEvent(EEventType.GameDevProgressChanged, OnGameDevProgressChanged);
        EventManager.Instance.AddEvent(EEventType.QualityChanged, OnQualityChanged);
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        // 이벤트 구독 해제
        EventManager.Instance.RemoveEvent(EEventType.GameDevStateChanged, OnGameDevStateChanged);
        EventManager.Instance.RemoveEvent(EEventType.GameDevProgressChanged, OnGameDevProgressChanged);
        EventManager.Instance.RemoveEvent(EEventType.QualityChanged, OnQualityChanged);
    }
    protected override Button GetSaveButton() => GetButton((int)Buttons.SaveButton);
    protected override Button GetMenuButton() => GetButton((int)Buttons.MenuButton);
    protected override TMP_Text GetSaveButtonText() => GetText((int)Texts.SaveButtonText);
    protected override TMP_Text GetMenuButtonText() => GetText((int)Texts.MenuButtonText);

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

    protected override void OnGameStateChanged()
    {
        base.OnGameStateChanged();
        UpdateBottomPanelBasedOnDevState();
    }

    // 밤 전용 UI 업데이트 메서드들
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
        switch (currentDevType)
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

    /// <summary>
    /// 회사 이름 업데이트
    /// </summary>
    private void UpdateCompanyText()
    {
        if (GetObject((int)GameObjects.NightBottomPanel1).activeSelf == false)
            return;

        GetText((int)Texts.CompanyText).text = GameManager.Instance.CompanyName;
    }

    private void UpdateBottomPanelBasedOnDevState()
    {
        EGameDevType currentDevType = GameDevManager.Instance.CurrentGameDevType;

        bool isNightPanel = (currentDevType == EGameDevType.None);
        bool isGameDevPanel = !isNightPanel;

        GetObject((int)GameObjects.NightBottomPanel1).SetActive(isNightPanel);
        GetObject((int)GameObjects.GameDevBottomPanel1).SetActive(isGameDevPanel);

        // NightPanel이 활성화될 때 회사 이름 업데이트
        if (isNightPanel)
        {
            UpdateCompanyText();
        }
    }

    public override void RefreshUI()
    {
        base.RefreshUI();
        UpdateBottomPanelBasedOnDevState();
        UpdateCompanyText();
        //TODO: Localization
    }
}
