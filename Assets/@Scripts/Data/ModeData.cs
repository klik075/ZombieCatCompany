using System;
using System.Collections.Generic;
using UnityEngine;
using static Define;

[Serializable]
public class ModeData
{
    public EGameMode GameMode;
    public string Description;
    public int Gold;
    public int Food;

    public static string GetModeName(EGameMode gameMode)
    {
        switch (gameMode)
        {
            case EGameMode.Purchase:
                return "구매모드(보통)";
            case EGameMode.Extortion:
                return "강탈모드(어려움)";
            default:
                return "알 수 없음";
        }
    }
}

[Serializable]
public class ModeDataLoader : IDataLoader<int, ModeData>
{
    public List<ModeData> mods = new List<ModeData>();

    public Dictionary<int, ModeData> MakeDict()
    {
        Dictionary<int, ModeData> dict = new Dictionary<int, ModeData>();
        foreach (var mode in mods)
            dict.Add((int)mode.GameMode, mode);

        return dict;
    }

    public bool Validate()
    {
        return true;
    }
}
