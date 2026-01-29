using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using static Define;
public class MagazineManager : Singleton<MagazineManager>
{
    private float mainQualityRate = 1.3f;
    private float evaluatorsRate = 1.5f;
    private float bugRate = 2f;
    public int GetEvaluationScore(EQualityType EvaluatorsType)
    {
        int baseFunScore = GameDevManager.Instance.GetQualityScore(EQualityType.Fun);
        int baseNyangScore = GameDevManager.Instance.GetQualityScore(EQualityType.Nyang);
        int baseGraphicsScore = GameDevManager.Instance.GetQualityScore(EQualityType.Graphics);
        int baseSoundScore = GameDevManager.Instance.GetQualityScore(EQualityType.Sound);
        int baseBugScore = GameDevManager.Instance.GetQualityScore(EQualityType.Bug);

        float funRate = 1f;
        float nyangRate = 1f;
        float graphicsRate = 1f;
        float soundRate = 1f;

        EQualityType mainQualityType = GameDevManager.Instance.CurrentGenreData.MainQuality;
        ApplyQualityRate(mainQualityType, mainQualityRate, ref funRate, ref nyangRate, ref graphicsRate, ref soundRate);
        ApplyQualityRate(EvaluatorsType, evaluatorsRate, ref funRate, ref nyangRate, ref graphicsRate, ref soundRate);

        float synergyRate = SynergyData.SynergyTypeToRate(GameDevManager.Instance.CurrentSynergy);

        float evaluationScore = 
            synergyRate * (
            (baseFunScore * funRate) +
            (baseNyangScore * nyangRate) +
            (baseGraphicsScore * graphicsRate) +
            (baseSoundScore * soundRate) - 
            (baseBugScore * bugRate)
            );


        int finalEvaluationScore = GetFinalScore(evaluationScore);
        Debug.Log($"EvaluationScore : {evaluationScore}, FinalScore : {finalEvaluationScore}");

        return finalEvaluationScore;
    }
    public void ApplyQualityRate(EQualityType qualityType, float rate, ref float funRate, ref float nyangRate, ref float graphicsRate, ref float soundRate)
    {
        switch (qualityType)
        {
            case EQualityType.Fun:
                funRate *= rate;
                break;
            case EQualityType.Nyang:
                nyangRate *= rate;
                break;
            case EQualityType.Graphics:
                graphicsRate *= rate;
                break;
            case EQualityType.Sound:
                soundRate *= rate;
                break;
            default:
                break;
        }
    }
    public int GetFinalScore(float evaluationScore)
    {
        int referenceScore = 30 + (20 * GameManager.Instance.Year);
        float scoreRatio = evaluationScore / referenceScore;
        int finalScore = ConvertToFinalScore(scoreRatio);

        return finalScore;
    }
    public int ConvertToFinalScore(float scoreRatio)
    {
        switch (scoreRatio)
        {
            case float when scoreRatio >= 1.0f:
                return 10;
            case float when scoreRatio >= 0.9f:
                return 9;
            case float when scoreRatio >= 0.8f:
                return 8;
            case float when scoreRatio >= 0.7f:  
                return 7;
            case float when scoreRatio >= 0.6f:
                return 6;   
            case float when scoreRatio >= 0.5f:
                return 5;
            case float when scoreRatio >= 0.4f:
                return 4;
            case float when scoreRatio >= 0.3f:
                return 3;
            case float when scoreRatio >= 0.2f:
                return 2;
            case float when scoreRatio >= 0.1f:
                return 1;
            default:
                return 1;
        }
    }
}
