using System;
using System.Collections.Generic;
using UnityEngine;
using static Define;

[Serializable]
public class SynergyData 
{
    public int GenreId;
    public int ContentId;
    public ESynergyType SynergyType;

    public static string SynergyTypeToString(ESynergyType synergyType)
    {
        switch (synergyType)
        {
            case ESynergyType.Good:
                return "°«°×";
            case ESynergyType.Bad:
                return "¶Ë°×";
            default:
                return "Æò¹ü";
        }
    }
    public static float SynergyTypeToRate(ESynergyType synergyType)
    {
        switch (synergyType)
        {
            case ESynergyType.Good:
                return 1.1f;
            case ESynergyType.Bad:
                return 0.9f;
            default:
                return 1f;
        }
    }
}
[Serializable]
public class SynergyDataLoader : IDataLoader<int, List<SynergyData>>
{
    public List<SynergyData> synergies = new List<SynergyData>();

    public Dictionary<int, List<SynergyData>> MakeDict()
    {
        Dictionary<int, List<SynergyData>> dict = new Dictionary<int, List<SynergyData>>();
        foreach (var synergyData in synergies)
        {
            if (!dict.ContainsKey(synergyData.GenreId))
                dict.Add(synergyData.GenreId, new List<SynergyData>());

            dict[synergyData.GenreId].Add(synergyData);
        }

        return dict;
    }

    public bool Validate()
    {
        return true;
    }
}
