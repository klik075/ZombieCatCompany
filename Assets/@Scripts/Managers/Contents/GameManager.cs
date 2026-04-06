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
    public bool IsDispatched;
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
    public bool IsRecruiting; //멤버 모집 중인지
    public HireResult HireResult = null;
    public GameDevProjectData GameDevProjectData = new GameDevProjectData();
}
[Serializable]
public class MorningData
{
    public FenceSaveData FenceSaveData = new FenceSaveData();
}
[Serializable]
public class GameData
{
    public EGameMode GameMode;//게임 모드
    public EGameState GameState;//게임 상태

    public CompanyData CompanyData = new CompanyData();
    public NightData NightData = new NightData();
    public MorningData MorningData = new MorningData();
    public YearEventSaveData YearEventData = new YearEventSaveData();
    public EndingData CurrentEndingData = new EndingData(); 
    public GameData()
    {
        GameMode = EGameMode.Purchase;
        GameState = EGameState.None;

        CompanyData.CompanyName = "";
        CompanyData.Year = 1;
        CompanyData.Gold = 0;
        CompanyData.Food = 0;

        NightData.IsRecruiting = false;

        CurrentEndingData = new EndingData();
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
    public EEndingType EndingType;//엔딩 타입 추가
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

    public EndingData()
    {
        EndingType = EEndingType.Starvation;
        CompanyName = "";
        Year = 0;
        TotalGold = 0;
        ConsumedFood = 0;
        DeadMembersCount = 0;
        KilledCatsCount = 0;
        HiredMembersCount = 0;
        EducationCount = 0;
        DispatchedMembersCount = 0;
        TotalEnhancementLevel = 0;
        EnhancementFailCount = 0;
    }
    public static string EndingTypeToString(EEndingType type)
    {
        switch (type)
        {
            case EEndingType.Starvation:
                return "굶어 죽음";
            case EEndingType.Exposed:
                return "정체 발각";
            case EEndingType.Serum:
                return "혈청 투여";
            default:
                return "버그다냥";
        }
    }
}

public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    private UserData _userData;
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

            // EndingData 동기화
            if (_userData.MyGameData.CurrentEndingData != null)
            {
                _userData.MyGameData.CurrentEndingData.Year = _userData.MyGameData.CompanyData.Year;
            }

            EventManager.Instance.TriggerEvent(Define.EEventType.YearChanged);
        }
    }

    public int Gold
    {
        get { return _userData.MyGameData.CompanyData.Gold; }
        set
        {
            int prevGold = _userData.MyGameData.CompanyData.Gold;
            int changeAmount = value - prevGold;
            if (changeAmount > 0)
            {
                EndingManager.Instance.RecordStat(EEndingStatType.TotalGold, changeAmount);
            }
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
            int prevFood = _userData.MyGameData.CompanyData.Food;
            int changeAmount = value - prevFood;
            if (changeAmount < 0)
            {
                EndingManager.Instance.RecordStat(EEndingStatType.ConsumedFood, -changeAmount);
            }
            _userData.MyGameData.CompanyData.Food = value;

            if (_userData.MyGameData.CompanyData.Food < 0)
                _userData.MyGameData.CompanyData.Food = 0;

            EventManager.Instance.TriggerEvent(Define.EEventType.FoodChanged);
        }
    }
    public EGameMode GameMode
    {
        get { return _userData.MyGameData.GameMode; }
        set
        {
            _userData.MyGameData.GameMode = value;

            if (_userData.MyGameData.CurrentEndingData != null)
            {
                _userData.MyGameData.CurrentEndingData.GameMode = value;
            }
        }
    }
    public string CompanyName
    {
        get { return _userData.MyGameData.CompanyData.CompanyName; }
        set
        {
            _userData.MyGameData.CompanyData.CompanyName = value;

            // EndingData 동기화
            if (_userData.MyGameData.CurrentEndingData != null)
            {
                _userData.MyGameData.CurrentEndingData.CompanyName = value;
            }
        }
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
    public bool IsRecruiting
    {
        get { return _userData.MyGameData.NightData.IsRecruiting; }
        set
        {
            _userData.MyGameData.NightData.IsRecruiting = value;
            GameState = value ? EGameState.Recruiting : EGameState.Night;
        }
    }
    public int FOOD_PRICE_PER_UNIT
    {
        get { return DataManager.Instance.GameBalanceConfig.FoodPricePerUnit; }
    }
    private void Awake()
    {
        EventManager.Instance.AddEvent(EEventType.UI_PopupOpened, PauseGame);
        EventManager.Instance.AddEvent(EEventType.UI_PopupClosed, ResumeGame);
        EventManager.Instance.AddEvent(EEventType.UI_LeftPanelOpened, PauseGame);
        EventManager.Instance.AddEvent(EEventType.UI_LeftPanelClosed, ResumeGame);
        EventManager.Instance.AddEvent(EEventType.GameStateChanged, OnChangedGameState);
    }
    private void OnDestroy()
    {
        EventManager.Instance.RemoveEvent(EEventType.UI_PopupOpened, PauseGame);
        EventManager.Instance.RemoveEvent(EEventType.UI_PopupClosed, ResumeGame);
        EventManager.Instance.RemoveEvent(EEventType.UI_LeftPanelOpened, PauseGame);
        EventManager.Instance.RemoveEvent(EEventType.UI_LeftPanelClosed, ResumeGame);
        EventManager.Instance.RemoveEvent(EEventType.GameStateChanged, OnChangedGameState);
    }
    private void OnChangedGameState()
    {
        switch (GameState)
        {
            case EGameState.FoodPurchase:
                SaveManager.Instance.SaveGameData();
                PurchaseManager.Instance.StartPurchase();
                break;
            case EGameState.Morning:
                YearEventManager.Instance.ResetNightSequenceFlag();
                SaveManager.Instance.SaveGameData();
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
        if (SceneManager.Instance.CurrentSceneType == Define.EScene.LobbyScene)
            return;

        Time.timeScale = 0f;
        Debug.Log("게임 정지");
    }

    // 게임 시간 재개
    public void ResumeGame()
    {
        if (SceneManager.Instance.CurrentSceneType == Define.EScene.LobbyScene)
            return;

        if (UIManager.Instance.PopupCount != 0)
            return;

        Time.timeScale = 1f;
        Debug.Log("게임 재개");
    }
}
