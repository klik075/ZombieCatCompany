using Newtonsoft.Json;
using System.Collections;
using System.IO;
using UnityEngine;

public class SaveManager : Singleton<SaveManager>
{
    private const string SAVE_FILE_NAME = "GameData.json";
    public static string SavePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

    private const float AUTO_SAVE_INTERVAL = 10f;
    private Coroutine _coAutoSave;

    #region AutoSave
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
            Save();
            yield return wait;
        }
    }
    #endregion

    public void Save()
    {
        GameData gameData = GameManager.Instance.GameData;
        if (gameData == null)
        {
            Debug.Log("SaveManager: GameData is null, cannot save.");
            return;
        }

        // MemberManager 데이터 저장
        gameData.PlayerSaveDatas = MemberManager.Instance.GetSaveData();

        string json = JsonConvert.SerializeObject(gameData, Formatting.Indented);
        File.WriteAllText(SavePath, json);
        Debug.Log($"SaveManager: Game saved to {SavePath}");
    }

    public void Load()
    {
        if (File.Exists(SavePath) == false)
        {
            Debug.Log("SaveManager: No save file found. Starting with default data.");
            Reset();
            return;
        }

        string json = File.ReadAllText(SavePath);
        GameData gameData = JsonConvert.DeserializeObject<GameData>(json);
        GameManager.Instance.GameData = gameData;
        
        // MemberManager 데이터 로드
        if (gameData.PlayerSaveDatas != null && gameData.PlayerSaveDatas.Count > 0)
        {
            MemberManager.Instance.LoadFromSaveData(gameData.PlayerSaveDatas);
        }
        
        Debug.Log($"SaveManager: Game loaded from {SavePath}");
    }

    public void Reset()
    {
        GameData gameData = new GameData()
        {
            Gold = DataManager.Instance.GameConfig.InitialGold,
            Year = DataManager.Instance.GameConfig.InitialYear,
            Food = DataManager.Instance.GameConfig.InitialFood,
            AnnualProfit = DataManager.Instance.GameConfig.InitialAnnualProfit,
            GameMode = DataManager.Instance.GameConfig.InitialGameMode,
            CompanyName = DataManager.Instance.GameConfig.InitialCompanyName,
            GameState = DataManager.Instance.GameConfig.InitialGameState,
            NewDevTitle = DataManager.Instance.GameConfig.InitialNewDevTitle,
            IsRecruiting = DataManager.Instance.GameConfig.InitialIsRecruiting,
            PlayerSaveDatas = null,
        };

        MemberManager.Instance.InitBoss();
        GameManager.Instance.GameData = gameData;
        Save();
    }

    public void Delete()
    {
        if (File.Exists(SavePath) == false)
        {
            Debug.LogWarning("SaveManager: No save file to delete.");
            return;
        }

        File.Delete(SavePath);
        Debug.Log("SaveManager: Save file deleted.");
    }
}
