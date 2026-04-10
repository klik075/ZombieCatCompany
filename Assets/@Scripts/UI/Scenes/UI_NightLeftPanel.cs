using UnityEngine;
using static Define;

public class UI_NightLeftPanel : UI_LeftPanelBase
{
    enum Buttons
    {
        // 공통 버튼
        MemberButton,
        SystemButton,
        
        // 밤 전용 버튼
        GameDevButton,
        DiaryButton,
        DispatchResultButton,
    }
    
    enum Texts
    {
        // 공통 텍스트
        MemberButtonText,
        SystemButtonText,
        
        // 밤 전용 텍스트
        GameDevButtonText,
        DiaryButtonText,
        DispatchResultButtonText,
    }

    // 바인딩만 수행
    protected override void PerformBinding()
    {
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
    }

    // 공통 버튼 반환 (부모에서 자동 등록에 사용)
    protected override UnityEngine.UI.Button GetMemberButton() => GetButton((int)Buttons.MemberButton);
    protected override UnityEngine.UI.Button GetSystemButton() => GetButton((int)Buttons.SystemButton);

    // 밤 전용 버튼 이벤트 등록
    protected override void RegisterSpecificEvents()
    {
        GetButton((int)Buttons.GameDevButton).onClick.AddListener(() => { PlayButtonClickSound(); OnClickGameDevButton(); });
        GetButton((int)Buttons.DiaryButton).onClick.AddListener(() => { PlayButtonClickSound(); OnClickDiaryButton(); });
        GetButton((int)Buttons.DispatchResultButton).onClick.AddListener(() => { PlayButtonClickSound(); OnClickDispatchResultButton(); });
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        UpdateContent();
    }

    private void UpdateContent()
    {
        GetText((int)Texts.MemberButtonText).text = "구성원";
        GetText((int)Texts.SystemButtonText).text = "시스템";

        GetText((int)Texts.GameDevButtonText).text = "게임 개발";
        GetText((int)Texts.DiaryButtonText).text = "일기장";
        GetText((int)Texts.DispatchResultButtonText).text = "파견 결과";
    }
    // 밤 전용 버튼 처리
    private void OnClickGameDevButton()
    {
        if (GameManager.Instance.GameState == EGameState.Night)
        {
            UI_ProposalPopup memberListPopup = UIManager.Instance.ShowPopupUI<UI_ProposalPopup>();
        }
        else
        {
            UI_ChatPopup chatPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
            chatPopup.SetInfo(
                MemberManager.MAIN_CHARACTER_ID, 
                MessageManager.Instance.GetMessageScript(EMessageType.NoDev).Contents
                );
        }
        
        IsActive = false;
    }

    private void OnClickDiaryButton()
    {
        SavedEventInfo eventInfo = YearEventManager.Instance.LastYearEvent;

        if (eventInfo == null || string.IsNullOrEmpty(eventInfo.eventText))
        {
            // 아직 이벤트를 보지 않았을 때
            UI_ChatPopup chatPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
            chatPopup.SetInfo(
                MemberManager.MAIN_CHARACTER_ID,
                new string[] { "버그다냥.." }
            );
        }
        else
        {
            // 이전 이벤트 다시 보기 (보상 정보는 표시하되 지급은 안함)
            UI_EventPopup diaryPopup = UIManager.Instance.ShowPopupUI<UI_EventPopup>();
            diaryPopup.SetInfo(
                EEventPopupType.YearEvent,
                eventInfo
            );
        }

        IsActive = false;
    }

    private void OnClickDispatchResultButton()
    {
        SavedEventInfo resultInfo = YearEventManager.Instance.LastDispatchResult;

        if (resultInfo == null || string.IsNullOrEmpty(resultInfo.eventText))
        {
            // 파견을 보내지 않았거나 결과가 없을 때
            UI_ChatPopup chatPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
            chatPopup.SetInfo(
                MemberManager.MAIN_CHARACTER_ID,
                MessageManager.Instance.GetMessageScript(EMessageType.DispatchNotSent).Contents
            );
        }
        else
        {
            // 이전 파견 결과 다시 보기 (보상 정보는 표시하되 지급은 안함)
            UI_EventPopup dispatchResultPopup = UIManager.Instance.ShowPopupUI<UI_EventPopup>();
            dispatchResultPopup.SetInfo(
                EEventPopupType.DispatchResult,
                resultInfo
            );
        }

        IsActive = false;
    }
    public override void RefreshUI()
    {
        base.RefreshUI();
        UpdateContent();
        //TODO: Night 전용 Localization
    }
}
