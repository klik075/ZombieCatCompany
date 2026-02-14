using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static Define;
public class SaveManager : Singleton<SaveManager>
{
    private const string USER_DATA_FILE_NAME = "UserData.json";
    private const string GAME_DATA_FILE_NAME = "GameData.json";
    public static string UserDataPath => Path.Combine(Application.persistentDataPath, USER_DATA_FILE_NAME);
    public static string GameDataPath => Path.Combine(Application.persistentDataPath, GAME_DATA_FILE_NAME);

    private const float AUTO_SAVE_INTERVAL = 10f;
    private Coroutine _coAutoSave;

    #region Auto Save
    public void StartAutoSave()
    {
        StopAutoSave();

        if (_coAutoSave == null)
        {
            _coAutoSave = StartCoroutine(CoAutoSave());
            Debug.Log($"SaveManager: Auto-save started (interval: {AUTO_SAVE_INTERVAL}s)");
        }
    }

    public void StopAutoSave()
    {
        if (_coAutoSave != null)
        {
            StopCoroutine(_coAutoSave);
            _coAutoSave = null;
            Debug.Log("SaveManager: Auto-save stopped");
        }
    }

    private IEnumerator CoAutoSave()
    {
        WaitForSeconds wait = new WaitForSeconds(AUTO_SAVE_INTERVAL);

        while (true)
        {
            SaveGameData();
            yield return wait;
        }
    }
    #endregion

    #region SaveFile Check

    /// <summary>
    /// 유저 데이터 파일이 존재하는지 확인
    /// </summary>
    public bool HasUserData()
    {
        return File.Exists(UserDataPath);
    }

    /// <summary>
    /// 현재 플레이 세이브 파일이 존재하는지 확인
    /// </summary>
    public bool HasGameData()
    {
        return File.Exists(GameDataPath);
    }

    #endregion

    #region GameData Save/Load
    public void SaveGameData()
    {
        GameData gameData = GameManager.Instance.MyGameData;
        if (gameData == null)
        {
            Debug.Log("SaveManager: GameData is null, cannot save.");
            return;
        }

        // MemberManager 데이터 저장
        gameData.CompanyData.MemberSaveDatas = MemberManager.Instance.GetPlayerSaveData();
        gameData.NightData.HireResult = GameManager.Instance.IsRecruiting ? MemberManager.Instance.GetHireResult() : null;
        gameData.NightData.GameDevProjectData = GameDevManager.Instance.GetGameDevProjectData();

        string json = JsonConvert.SerializeObject(gameData, Formatting.Indented);
        File.WriteAllText(GameDataPath, json);

        Debug.Log($"SaveManager: Game saved to {GameDataPath}");
    }

    public void LoadGameData()
    {
        if (GameManager.Instance.UserData == null)
        {
            Debug.LogWarning("SaveManager: UserData Instance is null. loading UserData first.");
            LoadUserData();
        }

        if(!HasGameData())
        {
            Debug.LogWarning("SaveManager: No GameData File found.");
            NewGame();
        }

        string json = File.ReadAllText(GameDataPath);
        GameData gameData = JsonConvert.DeserializeObject<GameData>(json);
        GameManager.Instance.MyGameData = gameData;

        if (gameData.CompanyData.MemberSaveDatas != null && gameData.CompanyData.MemberSaveDatas.Count == 0)
        {
            MemberManager.Instance.InitBoss();
        }

        if (gameData.CompanyData.MemberSaveDatas != null && gameData.CompanyData.MemberSaveDatas.Count > 0)
        {
            MemberManager.Instance.LoadFromSaveData();
        }

        if (gameData.NightData.HireResult != null)
        {
            MemberManager.Instance.LoadHireResult(gameData.NightData.HireResult);
            MemberManager.Instance.StartHire(gameData.NightData.HireResult.HireMethod, true);
        }

        if(gameData.NightData.GameDevProjectData != null)
        {
            GameDevManager.Instance.LoadFromSaveData(gameData.NightData.GameDevProjectData);
        }
        Debug.Log($"SaveManager: Game loaded from {GameDataPath}");
    }
    public GameData GetGameData()
    {
        if (!HasGameData())
        {
            Debug.Log("SaveManager: No save file found.");
            return null;
        }

        string json = File.ReadAllText(GameDataPath);
        GameData gameData = JsonConvert.DeserializeObject<GameData>(json);
        return gameData;
    }
    #endregion

    #region UserData Save/Load

    /// <summary>
    /// 유저 데이터 저장 (UserData - 엔딩 기록, 설정 등)
    /// </summary>
    public void SaveUserData()
    {
        UserData userData = GameManager.Instance.UserData;
        if (userData == null)
        {
            Debug.LogWarning("SaveManager: UserData is null, cannot save UserData.");
            return;
        }

        string json = JsonConvert.SerializeObject(userData, Formatting.Indented);
        File.WriteAllText(UserDataPath, json);

        Debug.Log($"SaveManager: UserData saved to {UserDataPath}");
    }

    /// <summary>
    /// 유저 데이터 불러오기 (UserData)
    /// </summary>
    public void LoadUserData()
    {
        if (!HasUserData())
        {
            Debug.LogWarning("SaveManager: No UserData found. Creating new UserData.");
            GameManager.Instance.UserData = new UserData();
            SaveUserData();
            return;
        }

        string json = File.ReadAllText(UserDataPath);
        UserData userData = JsonConvert.DeserializeObject<UserData>(json);
        GameManager.Instance.UserData = userData;

        Debug.Log($"SaveManager: UserData loaded from {UserDataPath}");
    }

    #endregion

    #region New Game / Reset

    /// <summary>
    /// 새 게임 시작 (기존 세이브 삭제 + 초기화)
    /// </summary>
    public void NewGame()
    {
        // 기존 세이브 파일 삭제
        DeleteGameData();

        // 새 GameData 생성
        ResetGameData();

        //// 멤버 초기화
        CreateBossData();

        GameDevManager.Instance.InitNewProject();

        // 즉시 저장
        SaveGameData();
        SaveUserData();

        Debug.Log("SaveManager: New game started.");
    }

    /// <summary>
    /// GameData 초기화 (설정값 기반)
    /// </summary>
    private void ResetGameData()
    {
        GameData gameData = new GameData()
        {
            GameMode = DataManager.Instance.GameConfig.InitialGameMode,
            GameState = DataManager.Instance.GameConfig.InitialGameState,
        };

        gameData.CompanyData.CompanyName = DataManager.Instance.GameConfig.InitialCompanyName;
        gameData.CompanyData.Year = DataManager.Instance.GameConfig.InitialYear;
        gameData.CompanyData.Gold = DataManager.Instance.GameConfig.InitialGold;
        gameData.CompanyData.Food = DataManager.Instance.GameConfig.InitialFood;

        gameData.NightData.AnnualProfit = DataManager.Instance.GameConfig.InitialAnnualProfit;
        gameData.NightData.IsRecruiting = DataManager.Instance.GameConfig.InitialIsRecruiting;

        GameManager.Instance.MyGameData = gameData;
    }

    #endregion

    #region Create Boss

    private void CreateBossData()
    {
        if (GameManager.Instance.MyGameData == null)
        {
            Debug.LogError("GameData is null!");
            return;
        }

        // Boss 멤버 데이터 생성
        MemberData bossData = DataManager.Instance.MemberDict[MemberManager.MAIN_CHARACTER_ID];
        if (bossData == null)
        {
            Debug.LogError("Boss MemberData not found in DataManager!");
            return;
        }

        // Boss 저장 데이터 생성
        MemberSaveData bossSaveData = new MemberSaveData
        {
            State = Cat.ECatState.Idle,
            IsFacingForward = true,
            IsFlipped = false,
            CellPosition = new Vector2Int(0, 2), // 기본 스폰 위치
            CurrentMemberData = bossData,
            AIEnabled = false // Boss는 AI 비활성화
        };

        // GameData에 추가
        GameManager.Instance.MyCompanyData.MemberSaveDatas.Add(bossSaveData);

        Debug.Log("Boss data created and added to GameData");
    }

    #endregion

    #region Delete

    public void DeleteGameData()
    {
        if (!HasGameData())
        {
            Debug.LogWarning("SaveManager: No Game Data to delete.");
            return;
        }

        File.Delete(GameDataPath);
        Debug.Log("SaveManager: Game Data deleted.");
    }
    public void DeleteAllData()
    {
        if (HasGameData())
        {
            File.Delete(GameDataPath);
            Debug.Log("SaveManager: Game Data deleted.");
        }
        if (HasUserData())
        {
            File.Delete(UserDataPath);
            Debug.Log("SaveManager: User Data deleted.");
        }
    }

    #endregion
}
