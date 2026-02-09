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

    // 공통 버튼 이벤트 등록
    protected override void RegisterCommonButtonEvents()
    {
        GetButton((int)Buttons.MemberButton)?.onClick.AddListener(OnClickMemberButton);
        GetButton((int)Buttons.SystemButton)?.onClick.AddListener(OnClickSystemButton);
    }

    // 밤 전용 버튼 이벤트 등록
    protected override void RegisterSpecificButtonEvents()
    {
        GetButton((int)Buttons.GameDevButton)?.onClick.AddListener(OnClickGameDevButton);
        GetButton((int)Buttons.DiaryButton)?.onClick.AddListener(OnClickDiaryButton);
        GetButton((int)Buttons.DispatchResultButton)?.onClick.AddListener(OnClickDispatchResultButton);
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
            chatPopup.SetInfo(MemberManager.MAIN_CHARACTER_ID, 
                MessageManager.Instance.GetMessageScript(EMessageType.NoDev).Contents);
        }
        
        IsActive = false;
    }

    private void OnClickDiaryButton()
    {
        UI_EventPopup diaryPopup = UIManager.Instance.ShowPopupUI<UI_EventPopup>();
        diaryPopup.SetInfo();
        
        IsActive = false;
    }

    private void OnClickDispatchResultButton()
    {
        //TODO: 파견 결과 팝업 열기, 파견을 보내지 않았으면 UI_ChatPopup을 열고 "파견을 보내지 않았다냥." text 설정
        UI_EventPopup dispatchResultPopup = UIManager.Instance.ShowPopupUI<UI_EventPopup>();
        dispatchResultPopup.SetInfo();
        
        IsActive = false;
    }

    public override void RefreshUI()
    {
        base.RefreshUI();
        //TODO: Night 전용 Localization
    }
}
