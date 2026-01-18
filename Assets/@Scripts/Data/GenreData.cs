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