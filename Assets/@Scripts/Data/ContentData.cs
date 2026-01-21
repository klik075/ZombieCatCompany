using System;
using System.Collections.Generic;
using UnityEngine;
using static Define;

[Serializable]
public class ContentData
{
    public int ContentId;
    public EContentType ContentType;
    public int Cost;

    public static string ContentToString(EContentType contentType)
    {
        switch (contentType)
        {
            case EContentType.Box:
                return "상자";
            case EContentType.Tuna:
                return "참치";
            case EContentType.Snack:
                return "간식";
            case EContentType.Laser:
                return "레이저";
            case EContentType.Thread:
                return "실타래";
            case EContentType.Nap:
                return "낮잠";
            case EContentType.CatTower:
                return "캣타워";
            case EContentType.Claw:
                return "발톱";
            case EContentType.Jumping:
                return "점프";
            case EContentType.Stealing:
                return "도둑질";
            case EContentType.Rat:
                return "쥐";
            case EContentType.Cushion:
                return "쿠션";
            case EContentType.Smell:
                return "냄새";
            case EContentType.Climbing:
                return "등반";
            case EContentType.Hiding:
                return "숨기기";
            case EContentType.Bell:
                return "방울";
            case EContentType.Shadow:
                return "그림자";
            case EContentType.Water:
                return "물";
            case EContentType.Sand:
                return "모래";
            case EContentType.Milk:
                return "우유";
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
public class ContentDataLoader : IDataLoader<int, ContentData>
{
    public List<ContentData> contents = new List<ContentData>();

    public Dictionary<int, ContentData> MakeDict()
    {
        Dictionary<int, ContentData> dict = new Dictionary<int, ContentData>();
        foreach (var contentData in contents)
            dict.Add(contentData.ContentId, contentData);

        return dict;
    }

    public bool Validate()
    {
        return true;
    }
}