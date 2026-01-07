using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class ItemData
{
    public int TemplateID;
    public int ItemType;
    public string NameTextID;
    public string DescriptionTextID;
    public string IconImageID;
    public string PrefabNameID;
}

[Serializable]
public class ItemDataLoader : IDataLoader<int, ItemData>
{
    public List<ItemData> items = new List<ItemData>();

    public Dictionary<int, ItemData> MakeDict()
    {
        Dictionary<int, ItemData> dict = new Dictionary<int, ItemData>();
        foreach (var item in items)
            dict.Add(item.TemplateID, item);

        return dict;
    }

    public bool Validate()
    {
        return true;
    }
}