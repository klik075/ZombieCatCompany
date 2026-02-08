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
public class GameData
{
    public int Year;//연차
    public int Gold;//자금
    public int Food;//식량
    public EGameMode GameMode;//게임 모드
    public string CompanyName;//회사 명

    public EGameState GameState;//게임 상태

    //Night
    public int AnnualProfit;//연간 이익

    public HireResult HireResult;
    //Dev
    public GameDevProjectData GameDevProjectData = new GameDevProjectData();

    //Morning

    //etc
    public bool IsRecruiting; //멤버 모집 중인지

    // 멤버 관련 데이터
    public List<MemberSaveData> MemberSaveDatas = new List<MemberSaveData>(); // 모든 멤버 상태
    
    public Dictionary<EGameMode, EndingData[]> EndingRecords = new Dictionary<EGameMode, EndingData[]>();//모드 별 엔딩 기록들 1~6
    
    // 맵 상태 (필요시 추가)
    // public MapSaveData MapData;
}

[Serializable]
public class EndingData
{
    public string EndingName;//엔딩 이름
    public EGameMode GameMode;//게임 모드
}

public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    private GameData _gameData = new GameData();
    public GameData GameData
    {
        get { return _gameData; }
        set
        {
            _gameData = value;
        }
    }
    public int Year
    {
        get { return _gameData.Year; }
        set
        {
            _gameData.Year = value;

            if (_gameData.Year < 1)
                _gameData.Year = 1;

            EventManager.Instance.TriggerEvent(Define.EEventType.YearChanged);
        }
    }

    public int Gold
    {
        get { return _gameData.Gold; }
        set
        {
            _gameData.Gold = value;

            if (_gameData.Gold < 0)
                _gameData.Gold = 0;

            EventManager.Instance.TriggerEvent(Define.EEventType.GoldChanged);
        }
    }
    public int Food
    {
        get { return _gameData.Food; }
        set
        {
            _gameData.Food = value;

            if (_gameData.Food < 0)
                _gameData.Food = 0;

            EventManager.Instance.TriggerEvent(Define.EEventType.FoodChanged);
        }
    }
    public EGameMode GameMode
    {
        get { return _gameData.GameMode; }
        set { _gameData.GameMode = value; }
    }
    public string CompanyName
    {
        get { return _gameData.CompanyName; }
        set { _gameData.CompanyName = value; }
    }
    public EGameState GameState
    {
        get { return _gameData.GameState; }
        set 
        { 
            _gameData.GameState = value; 
            EventManager.Instance.TriggerEvent(Define.EEventType.GameStateChanged);
        }
    }
    public int AnnualProfit
    {
        get { return _gameData.AnnualProfit; }
        set 
        { 
            _gameData.AnnualProfit = value;
            EventManager.Instance.TriggerEvent(Define.EEventType.AnnualProfitChanged);
        }
    }
    public bool IsRecruiting
    {
        get { return _gameData.IsRecruiting; }
        set 
        { 
            _gameData.IsRecruiting = value;
            GameState = value ? EGameState.Recruiting : EGameState.Night;
        }
    }
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

                break;
            case EGameState.Defence:
                break;
            case EGameState.Ending:
                break;
            default:
                break;
        }
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
