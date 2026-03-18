using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EvaluationScriptData
{
    public int EvaluationScore;
    public string Dialogue;
}
[Serializable]
public class EvaluationScriptDataLoader : IDataLoader<int, List<EvaluationScriptData>>
{
    public List<EvaluationScriptData> datas = new List<EvaluationScriptData>();

    public Dictionary<int, List<EvaluationScriptData>> MakeDict()
    {
        Dictionary<int, List<EvaluationScriptData>> dict = new Dictionary<int, List<EvaluationScriptData>>();
        foreach (var evaluationData in datas)
        {
            if (!dict.ContainsKey(evaluationData.EvaluationScore))
            {
                dict[evaluationData.EvaluationScore] = new List<EvaluationScriptData>();
            }

            dict[evaluationData.EvaluationScore].Add(evaluationData);
        }

        return dict;
    }

    public bool Validate()
    {
        return true;
    }
}
