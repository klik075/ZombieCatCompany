using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EducationData
{
    public int EducationId;
    public string Name;
    public int Cost;
    public int Programming;
    public int Scenario;
    public int Graphics;
    public int Sound;
    public int Power;


}

[Serializable]
public class EducationDataLoader : IDataLoader<int, EducationData>
{
    public List<EducationData> educations = new List<EducationData>();

    public Dictionary<int, EducationData> MakeDict()
    {
        Dictionary<int, EducationData> dict = new Dictionary<int, EducationData>();
        foreach (var education in educations)
            dict.Add(education.EducationId, education);

        return dict;
    }

    public bool Validate()
    {
        return true;
    }
}
