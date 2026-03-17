using UnityEngine;

public class UI_GameRulesPopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
        //BG
        BG,
    }
    enum Buttons
    {
        //MainTitle
        NextButton,
        PreviousButton,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //Content
        ContentText,
    }
    enum Images
    {

    }

    private int _currentPageIndex = 0;
    private int _totalPages = 0;

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        GetButton((int)Buttons.NextButton).onClick.AddListener(OnClickNextButton);
        GetButton((int)Buttons.PreviousButton).onClick.AddListener(OnClickPreviousButton);
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        SetInfo();
    }
    public void SetInfo()
    {
        _currentPageIndex = 0;
        _totalPages = DataManager.Instance.GameRuleDict.Count;

        if (_totalPages == 0)
        {
            Debug.LogError("[UI_GameRulesPopup] No game rule pages found!");
            UIManager.Instance.ClosePopupUI();
            return;
        }

        UpdateContent();
    }
    private void UpdateContent()
    {
        if (DataManager.Instance.GameRuleDict.TryGetValue(_currentPageIndex, out GameRulePageData page))
        {
            // 페이지 번호 표시 (예: "1 / 5")
            GetText((int)Texts.MainTitleText).text = $"플레이 방법 {_currentPageIndex + 1} / {_totalPages}";

            // 내용 설정
            GetText((int)Texts.ContentText).text = page.Content;
        }
        else
        {
            Debug.LogError($"[UI_GameRulesPopup] Page not found: {_currentPageIndex}");
        }
    }

    private void OnClickNextButton()
    {
        // 순환 구조: 마지막 페이지에서 Next를 누르면 첫 페이지로
        _currentPageIndex = (_currentPageIndex + 1) % _totalPages;
        UpdateContent();
    }

    private void OnClickPreviousButton()
    {
        // 순환 구조: 첫 페이지에서 Prev를 누르면 마지막 페이지로
        _currentPageIndex = (_currentPageIndex - 1 + _totalPages) % _totalPages;
        UpdateContent();
    }
    public override void RefreshUI()
    {
        base.RefreshUI();
        UpdateContent();
    }
}
