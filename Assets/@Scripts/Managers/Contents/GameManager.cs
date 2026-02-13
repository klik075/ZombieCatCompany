using System;
using System.Collections.Generic;
using UnityEngine;
using static Define;

[Serializable]
public class MemberSaveData
{
    public Cat.ECatState State;
    public bool IsFacingForward;
    public bool IsFlipped;
    public Vector2Int CellPosition;

    public MemberData CurrentMemberData;
    public bool AIEnabled;
}
[Serializable]
public class CompanyData
{
    public string CompanyName;//회사 명
    public int Year;//연차
    public int Gold;//자금
    public int Food;//식량

    public List<MemberSaveData> MemberSaveDatas = new List<MemberSaveData>(); // 모든 멤버 상태
}
[Serializable]
public class NightData
{
    public int AnnualProfit;//연간 이익
    public bool IsRecruiting; //멤버 모집 중인지
    public HireResult HireResult = null;
    public GameDevProjectData GameDevProjectData = new GameDevProjectData();
}
[Serializable]
public class MorningData
{
    //펜스 데이터
}
[Serializable]
public class GameData
{
    public EGameMode GameMode;//게임 모드
    public EGameState GameState;//게임 상태

    public CompanyData CompanyData = new CompanyData();
    public NightData NightData = new NightData();
    public MorningData MorningData = new MorningData();
    public GameData()
    {
        GameMode = EGameMode.Purchase;
        GameState = EGameState.None;

        CompanyData.CompanyName = "";
        CompanyData.Year = 0;
        CompanyData.Gold = 0;
        CompanyData.Food = 0;

        NightData.AnnualProfit = 0;
        NightData.IsRecruiting = false;
    }
    public GameData(EGameMode eGameMode, EGameState eGameState, string companyName, int year, int gold, int food, int annualProfit, bool isRecruiting)
    {
        GameMode = eGameMode;
        GameState = eGameState;

        CompanyData.CompanyName = companyName;
        CompanyData.Year = year;
        CompanyData.Gold = gold;
        CompanyData.Food = food;

        NightData.AnnualProfit = annualProfit;
        NightData.IsRecruiting = isRecruiting;
    }
}
[Serializable]
public class UserData
{
    public GameData MyGameData;
    
    public Dictionary<EGameMode, EndingData[]> EndingRecords = new Dictionary<EGameMode, EndingData[]>();//모드 별 엔딩 기록들 1~6

    public UserData()
    {
        EndingRecords[EGameMode.Purchase] = new EndingData[6];
        EndingRecords[EGameMode.Extortion] = new EndingData[6];
    }
}

[Serializable]
public class EndingData
{
    public EGameMode GameMode;//게임 모드
    public string EndingName;//엔딩 이름
    public string CompanyName;//회사 명
    public int Year;//연차
    public int TotalGold;//총 자금
    public int ConsumedFood;//먹은 통조림 수
    public int DeadMembersCount;//죽은 멤버 수
    public int KilledCatsCount;//잡은 고양이 수
    public int HiredMembersCount;//고용한 멤버 수
    public int EducationCount;//교육 횟수
    public int DispatchedMembersCount;//파견 보낸 멤버 수
    public int TotalEnhancementLevel;//강화수치
    public int EnhancementFailCount;//강화 실패 횟수
}

public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    private UserData _userData = new UserData();
    public UserData UserData
    {
        get { return _userData; }
        set { _userData = value; }
    }
    public GameData MyGameData
    {
        get { return _userData.MyGameData; }
        set { _userData.MyGameData = value; }
    }
    public CompanyData MyCompanyData
    {
        get { return _userData.MyGameData.CompanyData; }
        set { _userData.MyGameData.CompanyData = value; }
    }
    public NightData MyNightData
    {
        get { return _userData.MyGameData.NightData; }
        set { _userData.MyGameData.NightData = value; }
    }
    public MorningData MyMorningData
    {
        get { return _userData.MyGameData.MorningData; }
        set { _userData.MyGameData.MorningData = value; }
    }
    public int Year
    {
        get { return _userData.MyGameData.CompanyData.Year; }
        set
        {
            _userData.MyGameData.CompanyData.Year = value;

            if (_userData.MyGameData.CompanyData.Year < 1)
                _userData.MyGameData.CompanyData.Year = 1;

            EventManager.Instance.TriggerEvent(Define.EEventType.YearChanged);
        }
    }

    public int Gold
    {
        get { return _userData.MyGameData.CompanyData.Gold; }
        set
        {
            _userData.MyGameData.CompanyData.Gold = value;

            if (_userData.MyGameData.CompanyData.Gold < 0)
                _userData.MyGameData.CompanyData.Gold = 0;

            EventManager.Instance.TriggerEvent(Define.EEventType.GoldChanged);
        }
    }
    public int Food
    {
        get { return _userData.MyGameData.CompanyData.Food; }
        set
        {
            _userData.MyGameData.CompanyData.Food = value;

            if (_userData.MyGameData.CompanyData.Food < 0)
                _userData.MyGameData.CompanyData.Food = 0;

            EventManager.Instance.TriggerEvent(Define.EEventType.FoodChanged);
        }
    }
    public EGameMode GameMode
    {
        get { return _userData.MyGameData.GameMode; }
        set { _userData.MyGameData.GameMode = value; }
    }
    public string CompanyName
    {
        get { return _userData.MyGameData.CompanyData.CompanyName; }
        set { _userData.MyGameData.CompanyData.CompanyName = value; }
    }
    public EGameState GameState
    {
        get { return _userData.MyGameData.GameState; }
        set 
        { 
            _userData.MyGameData.GameState = value; 
            EventManager.Instance.TriggerEvent(Define.EEventType.GameStateChanged);
        }
    }
    public int AnnualProfit
    {
        get { return _userData.MyGameData.NightData.AnnualProfit; }
        set
        {
            _userData.MyGameData.NightData.AnnualProfit = value;
            EventManager.Instance.TriggerEvent(Define.EEventType.AnnualProfitChanged);
        }
    }
    public bool IsRecruiting
    {
        get { return _userData.MyGameData.NightData.IsRecruiting; }
        set
        {
            _userData.MyGameData.NightData.IsRecruiting = value;
            GameState = value ? EGameState.Recruiting : EGameState.Night;
        }
    }
    public const int FOOD_PRICE_PER_UNIT = 100; // 통조림 1개당 가격
    private void Awake()
    {
        EventManager.Instance.AddEvent(EEventType.UI_PopupOpened, PauseGame);
        EventManager.Instance.AddEvent(EEventType.UI_PopupClosed, ResumeGame);
        EventManager.Instance.AddEvent(EEventType.UI_LeftPanelOpened, PauseGame);
        EventManager.Instance.AddEvent(EEventType.UI_LeftPanelClosed, ResumeGame);
        EventManager.Instance.AddEvent(EEventType.GameStateChanged, OnChangedGameState);
    }
    private void OnChangedGameState()
    {
        switch (GameState)
        {
            case EGameState.Morning:
                SceneManager.Instance.LoadScene(EScene.MorningScene);
                break;
            case EGameState.Defence:
                break;
            case EGameState.Ending:
                break;
            default:
                break;
        }
    }
    public bool TryPurchaseFood(int amount)
    {
        if (amount <= 0)
        {
            return false;
        }

        int totalCost = amount * FOOD_PRICE_PER_UNIT;

        if (Gold < totalCost)
        {
            return false;
        }

        // 구매 성공
        Gold -= totalCost;
        Food += amount;

        Debug.Log($"통조림 구매 성공: {amount}개, 총 비용: {totalCost:N0}원");
        return true;
    }
    public void PauseGame()
    {
        Time.timeScale = 0f;
    }

    // 게임 시간 재개
    public void ResumeGame()
    {
        if (UIManager.Instance.PopupCount != 0)
            return;

        Time.timeScale = 1f;
    }
}
