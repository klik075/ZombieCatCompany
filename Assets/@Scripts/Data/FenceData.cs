using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FenceData
{
    public int Enhance;
    public int MaxHp;
    public int Defense;
    public int Durability;
    public int Damage;
    public int RepairCost;
    public int EnhanceCost;
    public float Probability;
    public int ReducedDurability;

    public static string EnhanceToString(int enhance)
    {
        switch (enhance)
        {
            case 1: 
                return "+1";
            case 2:     
                return "+2";
            case 3:     
                return "+3";
            case 4:     
                return "+4";
            case 5:
                return "+5";
            case 6:
                return "+6";
            case 7:
                return "+7";
            case 8:
                return "+8";
            case 9:
                return "+9";
            case 10:
                return "+10(Max)";
            default:
                return "???";
        }
    }
    public static string ProbabilityToStringt(float probability)
    {
        return $"{Mathf.RoundToInt(probability * 100)}%";
    }
    public static string CostToString(int cost)
    {
        return $"{cost}G";
    }
}

[Serializable]
public class FenceDataLoader : IDataLoader<int, FenceData>
{
    public List<FenceData> datas = new List<FenceData>();

    public Dictionary<int, FenceData> MakeDict()
    {
        Dictionary<int, FenceData> dict = new Dictionary<int, FenceData>();
        foreach (var fenceData in datas)
            dict.Add(fenceData.Enhance, fenceData);

        return dict;
    }

    public bool Validate()
    {
        return true;
    }
}
