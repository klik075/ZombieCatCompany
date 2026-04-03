using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using static Define;
public interface IValidate
{
    bool Validate();
}

public interface IDataLoader<Key, Value> : IValidate
{
    Dictionary<Key, Value> MakeDict();
}

public class DataManager : Singleton<DataManager>
{
    private HashSet<IValidate> _loaders = new HashSet<IValidate>();
    private bool _isDataLoaded = false;
    public GameConfig GameConfig { get; private set; }
    public GameBalanceConfig GameBalanceConfig { get; private set; }
    public LocalizationConfig LocalizationConfig { get; private set; }
    public AdsConfig AdsConfig { get; private set; }
    public IAPConfig IAPConfig { get; private set; }
    //public GameDevQualityRuleConfig GameDevQualityRuleConfig { get; private set; }

    public Dictionary<int, MemberData> MemberDict { get; private set;  } = new Dictionary<int, MemberData>();
    public Dictionary<int, EducationData> EducationDict { get; private set; } = new Dictionary<int, EducationData>();
    public Dictionary<int, GenreData> GenreDict { get; private set; } = new Dictionary<int, GenreData>();
    public Dictionary<int, ContentData> ContentDict { get; private set; } = new Dictionary<int, ContentData>();
    public Dictionary<int, List<SynergyData>> SynergyDict { get; private set; } = new Dictionary<int, List<SynergyData>>();
    public Dictionary<int, ModeData> ModeDict { get; private set; } = new Dictionary<int, ModeData>();
    public Dictionary<int, FenceData> FenceDict { get; private set; } = new Dictionary<int, FenceData>();
    public Dictionary<int, EventRewardData> RewardDict { get; private set; } = new Dictionary<int, EventRewardData>();
    public Dictionary<int, GameRulePageData> GameRuleDict { get; private set; } = new Dictionary<int, GameRulePageData>();
    public Dictionary<int, List<EvaluationScriptData>> EvaluationScriptDict { get; private set; } = new Dictionary<int, List<EvaluationScriptData>>();

    public void LoadData()
    {
        if (_isDataLoaded)
        {
            Debug.Log("[DataManager] Data already loaded.");
            return;
        }

        GameConfig = LoadScriptableObject<GameConfig>("GameConfig");
        GameBalanceConfig = LoadScriptableObject<GameBalanceConfig>("GameBalanceConfig");
        LocalizationConfig = LoadScriptableObject<LocalizationConfig>("LocalizationConfig");
        AdsConfig = LoadScriptableObject<AdsConfig>("AdsConfig");
        IAPConfig = LoadScriptableObject<IAPConfig>("IAPConfig");
        //GameDevQualityRuleConfig = LoadScriptableObject<GameDevQualityRuleConfig>("GameDevQualityRuleConfig");

        MemberDict = LoadJson<MemberDataLoader, int, MemberData>("MemberData").MakeDict();
        EducationDict = LoadJson<EducationDataLoader, int, EducationData>("EducationData").MakeDict();
        GenreDict = LoadJson<GenreDataLoader, int, GenreData>("GenreData").MakeDict();
        ContentDict = LoadJson<ContentDataLoader, int, ContentData>("ContentData").MakeDict();
        SynergyDict = LoadJson<SynergyDataLoader, int, List<SynergyData>>("SynergyData").MakeDict();
        ModeDict = LoadJson<ModeDataLoader, int, ModeData>("ModeData").MakeDict();
        FenceDict = LoadJson<FenceDataLoader, int, FenceData>("FenceData").MakeDict();
        GameRuleDict = LoadJson<GameRulePageDataLoader, int, GameRulePageData>("GameRulePageData").MakeDict();
        EvaluationScriptDict = LoadJson<EvaluationScriptDataLoader, int, List<EvaluationScriptData>>("EvaluationScriptData").MakeDict();

        RewardDict = LoadRewardDict();

        Validate();

        _isDataLoaded = true;
    }
    
    private T LoadScriptableObject<T>(string path) where T : ScriptableObject
    {
        T asset = ResourceManager.Instance.Get<T>(path);
        if (asset == null)
            Debug.LogError($"Failed to load ScriptableObject at path: {path}");

        return asset;
    }

    private Loader LoadJson<Loader, Key, Value>(string path) where Loader : IDataLoader<Key, Value>
    {
        TextAsset textAsset = ResourceManager.Instance.Get<TextAsset>(path);

        Loader loader = JsonConvert.DeserializeObject<Loader>(textAsset.text);
        _loaders.Add(loader);
        Debug.Log(path);

        return loader;
    }
    private Dictionary<int, EventRewardData> LoadRewardDict()
    {
        var dict = new Dictionary<int, EventRewardData>();
        EventRewardData[] rewards = ResourceManager.Instance.GetAllFromPath<EventRewardData>("RewardData");

        foreach (var reward in rewards)
        {
            if (dict.ContainsKey(reward.rewardId))
            {
                Debug.LogError($"[DataManager] Duplicate rewardId: {reward.rewardId} in EventRewardData");
                continue;
            }
            dict.Add(reward.rewardId, reward);
        }

        Debug.Log($"[DataManager] Loaded {dict.Count} EventRewardData");
        return dict;
    }
    private bool Validate()
    {
        bool success = true;

        foreach (IValidate loader in _loaders)
        {
            if (loader.Validate() == false)
                success = false;
        }

        _loaders.Clear();

        return success;
    }
}
