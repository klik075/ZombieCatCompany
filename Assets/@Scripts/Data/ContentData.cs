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