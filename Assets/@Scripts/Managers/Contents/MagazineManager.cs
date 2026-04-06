using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using static Define;
public class MagazineManager : Singleton<MagazineManager>
{
    [Header("평가 가중치")]
    [SerializeField] private float mainQualityRate = 1.3f;
    [SerializeField] private float evaluatorsRate = 1.5f;
    [SerializeField] private float bugRate = 2f;

    [Header("평가 기준 (연차별)")]
    [SerializeField] private int baseReferenceScore = 25;  // 기본 기준 점수
    [SerializeField] private float referenceScorePerYear = 8f;  // 연차당 증가
    [SerializeField] private int earlyGameBonusYears = 5;  // 초반 완화 기간
    [SerializeField] private float earlyGameScoreReduction = 0.7f;  // 초반 70%만 요구
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
    /// <summary>
    /// 연차별 기준 점수 계산 (GameBalanceConfig 스타일)
    /// </summary>
    public float CalculateReferenceScore(int year)
    {
        // 기본 점수 계산
        float referenceScore = baseReferenceScore + (year * referenceScorePerYear);

        // 초반 완화 (1~5년차는 요구 점수 감소)
        if (year <= earlyGameBonusYears)
        {
            referenceScore *= earlyGameScoreReduction;
        }

        return referenceScore;
    }
    /// <summary>
    /// 평가 점수를 최종 점수(1~10)로 변환
    /// </summary>
    public int GetFinalScore(float evaluationScore)
    {
        int year = GameManager.Instance.Year;
        float referenceScore = CalculateReferenceScore(year);
        float scoreRatio = evaluationScore / referenceScore;

        int finalScore = ConvertToFinalScore(scoreRatio);

        Debug.Log($"[MagazineManager] Year {year} | Reference: {referenceScore:F1} | Ratio: {scoreRatio:F2} | Final: {finalScore}/10");

        return finalScore;
    }
    /// <summary>
    /// 점수 비율을 최종 점수로 변환 (더 세밀한 구간)
    /// </summary>
    public int ConvertToFinalScore(float scoreRatio)
    {
        // 점수 구간을 더 촘촘하게 설정
        if (scoreRatio >= 1.5f) return 10;  // 150% 이상 → 만점
        if (scoreRatio >= 1.3f) return 9;   // 130% 이상
        if (scoreRatio >= 1.1f) return 8;   // 110% 이상
        if (scoreRatio >= 0.95f) return 7;  // 95% 이상
        if (scoreRatio >= 0.8f) return 6;   // 80% 이상
        if (scoreRatio >= 0.65f) return 5;  // 65% 이상
        if (scoreRatio >= 0.5f) return 4;   // 50% 이상
        if (scoreRatio >= 0.35f) return 3;  // 35% 이상
        if (scoreRatio >= 0.2f) return 2;   // 20% 이상
        if (scoreRatio >= 0.1f) return 1;   // 10% 이상
        return 1;  // 최소 1점 보장
    }
}
