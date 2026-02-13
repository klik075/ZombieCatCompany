using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

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

    public GameConfig GameConfig { get; private set; }
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

    public void LoadData()
    {
        GameConfig = LoadScriptableObject<GameConfig>("GameConfig");
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

        Validate();
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
