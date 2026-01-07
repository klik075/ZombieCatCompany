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

        string json = JsonConvert.SerializeObject(gameData);
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
        GameManager.Instance.GameData = JsonConvert.DeserializeObject<GameData>(json);
        Debug.Log($"SaveManager: Game loaded from {SavePath}");
    }

    public void Reset()
    {
        GameData gameData = new GameData()
        {
            Gold = DataManager.Instance.GameConfig.InitialGold,
            Level = DataManager.Instance.GameConfig.InitialLevel
        };

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
