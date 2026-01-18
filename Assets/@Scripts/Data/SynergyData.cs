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
