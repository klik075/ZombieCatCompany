using UnityEngine;

public static class Define
{
    public enum EScene
    {
        Unknown,
        LoadingScene,
        DevScene,
    }

    public enum EGameState
    {
        None,
        Night,//게임 개발 전
        Dev,//게임 개발 중
        Morning,//아침
        Popup,//팝업이 켜져 있는 상태에서는 게임의 시간 정지.
        Ending,//엔딩
    }
    public enum EEventType
    {
        //=======General=============
        Event,//이벤트
        Dispatch,//파견

        //===========================
        //=======Trigger Events======
        //Game Data Events
        None,
        YearChanged,
        GoldChanged,
        FoodChanged,
        AnnualProfitChanged,
        NewDevTitleChanged,


        //Member Events
        MemberListChanged,
        SelectedMemberChanged,
        EducationCompleted,

        //Lion
        LanguageChanged,

        //LeftPanel
        UI_LeftPanelStateChanged,

        UI_MenuButtonClicked,
        UI_SystemButtonClicked,

         //Popup
        UI_PopupClosed,
        UI_PopupOpened,
        //===========================
    }
    public enum  EEventRewardType
    {
        Item,//아이템
        Ending,//혈청 엔딩
    }
    public enum ESound
    {
        Bgm,
        Effect,

        MaxCount
    }

    public enum ELanguage
    {
        KOR,
        ENG,
    }

	public enum EAnimation
	{
		b_wait,
		b_walk,
		f_wait,
		f_walk
	}

	public enum ECatState
	{
		Idle,
		Move,
		Work
	}
    public enum ERoleType
    {
        Boss,//사장
        Planner,//기획자
        Designer,//디자이너
        SoundWriter,//사운드 작가
        Merchant,//상인
    }
    public enum EMemberStateType
    {
        Full,//배부름
        Hunger1,//약간 배고픔
        Hunger2,//배고픔
        Starvation,//굶주림
        Soon,//곧 죽음

        Dispatch,//파견 중
    }
    public enum EAbilityType
    {
        Programming,//프로그래밍
        Scenario,//시나리오
        Graphics,//그래픽
        Sound,//사운드
        Power//전투력
    }
    public enum EQualityType
    {
        Fun,//재미
        Nyang,//냥력
        Graphics,//그래픽
        Sound,//사운드
        Bug,//버그
    }
    public enum EGameMode
    {
        Purchase, //구매 모드
        Extortion, //강탈 모드
    }
    public enum EEndingType
    {
        Starved,//굶어 죽음
        Detection,//정체 발각
        Serum,//혈청 투여
    }
    public enum ESynergyType
    {
        Good,//걸작
        Normal,//평범
        Bad,//똥게임
    }
    public enum HireMethodType
    {
        Internet,//인터넷
    }
    public enum EMemberSelectionType
    {
        Education,//교육
        Dispatch,//파견
    }
    public enum ESalaryType
    {
        Salary,//연봉
        Food,//식량
        Deposit,//계약금
    }
}
