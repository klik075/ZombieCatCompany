using System;
using UnityEngine;

public static class Define
{
    public enum EScene
    {
        Unknown,
        LoadingScene,
        LobbyScene,
        NightScene,
        MorningScene,
    }

    public enum EGameState
    {
        None,

        Event,//이벤트
        DispatchResult,//파견 결과
        Dispatch,//파견 선택
        FoodRationing,//식량 배급

        Night,//게임 개발 전
        Recruiting,//고용 중
        Dev,//게임 개발 중

        FoodPurchase,//식량 구매

        Morning,//아침
        Defence,//방어

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
        GameStateChanged,
        ProposalChanged,
        GameDevStateChanged,
        GameDevProgressChanged,
        NewDevTitleChanged,

        QualityChanged,
        WorkCompleted,

        //Purchase Events
        MerchantStateChanged,

        //Member Events
        MemberListChanged,
        SelectedMemberChanged,
        EducationCompleted,
        MemberSwapped,

        //Fence Events
        FenceStateChanged,
        FenceDamaged,

        //Defense Events
        DefenseStarted,
        DefenseProgressChanged,
        DefenseStopped,

        //Lion
        LanguageChanged,

        //LeftPanel
        UI_LeftPanelStateChanged,
        UI_LeftPanelOpened,
        UI_LeftPanelClosed,

        UI_MenuButtonClicked,
        UI_BackButtonClicked,

        //Popup
        UI_PopupClosed,
        UI_PopupOpened,

        //Load
        LoadCompleted,
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
        Death//죽음
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
        GameStart,  //게임 시작
        Starvation, //굶어 죽음
        Exposed, //정체 발각
        Serum, //혈청 투여
    }
    public enum ERecordType
    {
        Shortest = 0,        // 최단 기록
        Longest = 1          // 최장 기록
    }
    public enum EEndingStatType
    {
        TotalGold,           // 총 자금 (누적)
        ConsumedFood,        // 먹은 통조림
        DeadMembers,         // 죽은 멤버
        KilledCats,          // 잡은 고양이
        HiredMembers,        // 고용한 멤버
        Education,           // 교육 횟수
        DispatchedMembers,   // 파견 보낸 멤버
        EnhancementLevel,    // 강화 레벨
        EnhancementFail      // 강화 실패
    }
    public enum EHireMethodType
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
        Food,//식비
        Deposit,//계약금
    }
    
    public enum EMessageType
    {
        // 파견 관련
        SelectDispatch,       // 파견 선택
        DispatchMemberSelected, // 파견 멤버 선택됨
        DispatchNotSent,       // 파견 안보냄

        // 식량 관련
        FoodRationing,       // 식량 배급
        FoodShortage,        // 식량 부족

        // 재정 관련
        MoneyLow,          // 자금 부족

        // 고용 관련
        AlreadyRecruiting,     // 이미 모집 중
        StartRecruiting,       // 모집 시작
        CompleteRecruiting,    // 모집 완료
        MembersFull,          // 멤버가 가득 참
        MemberFired,           // 멤버 해고
        MemberSwapped,          // 멤버 해고 후 고용
        MemberHired,          // 멤버 고용
        MemberFiredConfirm,   //멤버 해고 확인
        NoRecruiting,         //모집 불가
        NoFire,              //해고 불가

        // 개발 관련
        NoDev,              // 개발 불가. 모집 중
        AlreadyDev,         // 이미 개발 중
        SelectPlanner,       // 기획자 선택
        SelectDesigner,      // 원화 선택
        SelectSoundWriter,   // 사운드 작업 선택
        StartDebugging,     // 디버깅 시작
        CompleteGameDev,    // 게임 개발 완료

        // 통조림 구매 관련
        MerchantHello,        // 암상인 인사
        PurchaseMessage,      // 통조림 구매 메시지
        MerchantBye,          // 구매 완료

        // 팬스 관련
        TryRepairFence,       // 수리 시도
        RepairSuccess,       // 수리 성공

        TryEnhanceFence,      // 강화 시도
        EnhanceSuccess,       // 강화 성공
        EnhanceFail,          // 강화 실패

        //엔딩 관련
        EndingStarvation,     // 굶어 죽음 엔딩
        EndingExposed,        // 정체 발각 엔딩
        EndingSerum,          // 혈청 엔딩
    }

    public enum EChatType
    {
        // 멤버 간 대화
        MemberToMember,    // 멤버끼리 대화
        MemberToBoss,      // 멤버가 사장에게
        BossToMember,      // 사장이 멤버에게
        
        // 업무 관련
        WorkRequest,       // 업무 요청
        WorkComplete,      // 업무 완료
        WorkProgress,      // 업무 진행 상황
        
        // 상태 관련
        StatusReport,      // 상태 보고
        ComplaintHungry,   // 배고픔 불만
        ComplaintTired,    // 피로 불만
        
        // 감정 표현
        Happy,             // 기쁨
        Sad,               // 슬픔
        Angry,             // 화남
        Surprised,         // 놀람
        
        // 시스템
        System,            // 시스템 메시지
        Narration,         // 나레이션
        
        // 이벤트
        EventMessage,      // 이벤트 발생 시
        
        // 랜덤 대화
        RandomChat,        // 일상 대화
    }
    public enum EFireType
    {
        Normal,//일반 해고
        Swap,//교체 해고
    }
    public enum EGenreType
    {
        ActionGame,
        Adventure,
        RPG,
        Simulation,
        PuzzleGame,
        StrategyGame,
        Sports,
        Racing,
        HealingGame,
        TowerDefense,
        RhythmGame,
        HorrorGame,
        CardGame,
        Sandbox,
        Education,
    }
    public enum EContentType
    {
        Box,
        Tuna,
        Snack,
        Laser,
        Thread,
        Nap,
        CatTower,
        Claw,
        Jumping,
        Stealing,
        Rat,
        Cushion,
        Smell,
        Climbing,
        Hiding,
        Bell,
        Shadow,
        Water,
        Sand,
        Milk,
    }
    public enum ESynergyType
    {
        Good,//걸작
        Normal,//평범
        Bad,//똥게임
    }
    public enum EProposalType
    {
        Genre,
        Content,
    }
    public enum EGameDevType
    {
        None,
        Scenario,
        Graphics,
        Sound,
        Debug,
        Complete,
        EndDev,
    }
    public enum EPlayerImageType
    {
        Normal,
        Zombie,
    }
    public enum EInputFieldType
    {
        None,
        CompanyName,        // 회사 이름
        ChangeGameTitle,    // 게임 타이틀 변경
        PurchaseFood        // 통조림 구매
    }
    public enum EResultsReportType
    {
        None,
        GameSales,          // 게임 판매 결과
        Defense             // 디펜스 결과
    }
    public enum EEventPopupType
    {
        YearEvent,
        DispatchResult,
    }
}
