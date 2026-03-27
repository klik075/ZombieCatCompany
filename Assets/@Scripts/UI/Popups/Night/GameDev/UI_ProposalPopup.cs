using UnityEngine;
using static Define;

public class UI_ProposalPopup : UI_UGUI, IUI_Popup
{
    enum GameObjects
    {
        //BG - 상호작용 x
        BG,
    }
    enum Buttons
    {
        //Content
        GenreButton,
        ContentButton,

        //SubBottom
        OkayButton,
    }
    enum Texts
    {
        //MainTitle
        MainTitleText,

        //SubMiddle
        SubMiddleCostNameText,
        SubMiddleCostText,

        //Content
        GenreNameText,
        GenreText,
        ContentNameText,
        ContentText,
        SynergyText,

        //SubBottom
        OkayButtonText,
    }
    
    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));

        GetButton((int)Buttons.GenreButton).onClick.AddListener(() => OnClickSelectionButton(EProposalType.Genre));
        GetButton((int)Buttons.ContentButton).onClick.AddListener(() => OnClickSelectionButton(EProposalType.Content));
        GetButton((int)Buttons.OkayButton).onClick.AddListener(() => OnClickOkayButton());
    }
    protected override void OnEnable()
    {
        base.OnEnable();

        EventManager.Instance.AddEvent(EEventType.ProposalChanged, UpdateContent);

        UpdateContent();
    }
    protected override void OnDisable()
    {
        base.OnDisable();

        EventManager.Instance.RemoveEvent(EEventType.ProposalChanged, UpdateContent);
    }
    public void UpdateContent()
    {
        // GameDevManager에서 현재 선택된 장르와 콘텐츠 정보 가져오기
        GenreData genreData = GameDevManager.Instance.CurrentGenreData;
        ContentData contentData = GameDevManager.Instance.CurrentContentData;

        // 메인 타이틀 설정
        GetText((int)Texts.MainTitleText).text = "@게임 기획";

        // 장르 정보 표시
        GetText((int)Texts.GenreNameText).text = "@장르";
        GetText((int)Texts.GenreText).text = GenreData.GenreToString(genreData.GenreType);

        // 콘텐츠 정보 표시
        GetText((int)Texts.ContentNameText).text = "@내용";
        GetText((int)Texts.ContentText).text = ContentData.ContentToString(contentData.ContentType);

        // 총 개발 비용 계산 및 표시
        int totalCost = GameDevManager.Instance.GetTotalDevelopmentCost();
        GetText((int)Texts.SubMiddleCostNameText).text = "@개발비";
        GetText((int)Texts.SubMiddleCostText).text = $"{totalCost:N0}G";

        // 확인 버튼 텍스트
        GetText((int)Texts.OkayButtonText).text = "@결정";
    }

    public void OnClickSelectionButton(EProposalType proposalType)
    {
        UI_GameSelectionPopup selectedPopup = UIManager.Instance.ShowPopupUI<UI_GameSelectionPopup>();
        selectedPopup.SetInfo(proposalType);
    }
    public void OnClickOkayButton()
    {
        // 개발 비용이 충분한지 확인
        if (!GameDevManager.Instance.CanAffordDevelopment())
        {
            UIManager.Instance.ClosePopupUI();

            // 자금 부족 메시지 표시
            UI_ChatPopup chatPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
            chatPopup.SetInfo(
                MemberManager.MAIN_CHARACTER_ID,
                MessageManager.Instance.GetMessageScript(EMessageType.EndingStarvation).Contents,
                null,
                () => { EndingManager.Instance.TriggerEnding(EEndingType.Starvation);}
                );
            return;
        }

        // 게임 개발 시작
        int totalCost = GameDevManager.Instance.GetTotalDevelopmentCost();

        // 개발 비용 차감
        //GameManager.Instance.Gold -= totalCost;

        // 팝업 닫기
        UIManager.Instance.ClosePopupUI();
        GameDevManager.Instance.StartNewProject();
    }
    public override void RefreshUI()
    {
        base.RefreshUI();

    }
}
