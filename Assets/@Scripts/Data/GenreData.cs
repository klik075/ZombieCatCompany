using System;
using System.Collections.Generic;
using UnityEngine;
using static Define;

[Serializable]
public class GenreData
{
    public int GenreId;
    public EGenreType GenreType;
    public int Cost;
    public EQualityType MainQuality;

    public static string GenreToString(EGenreType genreType)
    {
        switch (genreType)
        {
            case EGenreType.ActionGame:
                return "액션게임";
            case EGenreType.Adventure:
                return "어드벤처";
            case EGenreType.RPG:
                return "RPG";
            case EGenreType.Simulation:
                return "시뮬레이션";
            case EGenreType.PuzzleGame:
                return "퍼즐게임";
            case EGenreType.StrategyGame:
                return "전략게임";
            case EGenreType.Sports:
                return "스포츠";
            case EGenreType.Racing:
                return "레이싱";
            case EGenreType.HealingGame:
                return "힐링게임";
            case EGenreType.TowerDefense:
                return "타워디펜스";
            case EGenreType.RhythmGame:
                return "리듬게임";
            case EGenreType.HorrorGame:
                return "공포게임";
            case EGenreType.CardGame:
                return "카드게임";
            case EGenreType.Sandbox:
                return "샌드박스";
            case EGenreType.Education:
                return "교육";
            default:
                return "누구냐 넌!";
        }
    }
    public string CostToString()
    {
        return $"{Cost:N0}G";
    }
}

[Serializable]
public class GenreDataLoader : IDataLoader<int, GenreData>
{
    public List<GenreData> genres = new List<GenreData>();

    public Dictionary<int, GenreData> MakeDict()
    {
        Dictionary<int, GenreData> dict = new Dictionary<int, GenreData>();
        foreach (var genreData in genres)
            dict.Add(genreData.GenreId, genreData);

        return dict;
    }

    public bool Validate()
    {
        return true;
    }
}