using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class TextData
{
    public string TemplateID; // HELLO
    public string KOR; // 안녕하세요
    public string ENG; // Hello
}

[Serializable]
public class TextDataLoader : IDataLoader<string, TextData>
{
    public List<TextData> texts = new List<TextData>();

    public Dictionary<string, TextData> MakeDict()
    {
        Dictionary<string, TextData> dict = new Dictionary<string, TextData>();
        foreach (var text in texts)
            dict.Add(text.TemplateID, text);

        return dict;
    }

    public bool Validate()
    {
        return true;
    }
}