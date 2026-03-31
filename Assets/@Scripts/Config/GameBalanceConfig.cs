using UnityEngine;

[CreateAssetMenu(fileName = "GameBalanceConfig", menuName = "Config/GameBalanceConfig")]
public class GameBalanceConfig : ScriptableObject
{
    [Header("경제 설정")]
    [SerializeField] private int foodPricePerUnit = 50;
    [SerializeField] private int developmentCost = 1000;
    [SerializeField] private float contractDepositRate = 2.0f;
    
    [Header("멤버 월급")]
    [SerializeField] private int salaryIncreasePerEducation = 2;
    
    [Header("수익 공식")]
    [SerializeField] private int baseIncome = 1000;
    [SerializeField] private int baseIncomePerYear = 200;
    [SerializeField] private int earlyGameBonusYears = 5;
    [SerializeField] private int earlyGameBonusPerYear = 500;
    [Range(0f, 1f)]
    [SerializeField] private float guaranteedIncomeRatio = 0.5f;  // 기본 수익 보장 50%

    [Header("품질 보너스")]
    [SerializeField] private float highQualityBonus = 1.3f;  // 평균 8점 이상
    [SerializeField] private float midQualityBonus = 1.15f;   // 평균 6점 이상
    [SerializeField] private float lowQualityBonus = 0.85f;   // 평균 4점 이상
    [SerializeField] private float veryLowQualityPenalty = 0.7f;  // 4점 미만
    
    [Header("디펜스 밸런스")]
    [SerializeField] private int rewardPerCat = 100;
    [SerializeField] private float evaluationMobMultiplierMin = 0.8f;
    [SerializeField] private float evaluationMobMultiplierMax = 1.5f;
    
    // Public Properties
    public int FoodPricePerUnit => foodPricePerUnit;
    public int DevelopmentCost => developmentCost;
    public float ContractDepositRate => contractDepositRate;
    public int SalaryIncreasePerEducation => salaryIncreasePerEducation;
    
    /// <summary>
    /// 년도별 기본 수익 계산
    /// </summary>
    public int CalculateBaseIncome(int year)
    {
        int baseIncome = this.baseIncome + (year * baseIncomePerYear);

        // 초반 보너스 (1~5년)
        if (year <= earlyGameBonusYears)
        {
            int bonus = (earlyGameBonusYears + 1 - year) * earlyGameBonusPerYear;
            baseIncome += bonus;
        }
        
        return baseIncome;
    }
    
    /// <summary>
    /// 평가 점수에 따른 최종 수익 계산
    /// </summary>
    public int CalculateFinalIncome(int year, int totalEvaluationScore)
    {
        int baseIncome = CalculateBaseIncome(year);

        // guaranteedIncomeRatio 안전 범위 클램핑 (0 ~ 1)
        float safeRatio = Mathf.Clamp01(guaranteedIncomeRatio);

        // 기본 수익 보장 + 평가 기반 수익
        float guaranteedIncome = baseIncome * safeRatio;
        float evaluationBasedIncome = baseIncome * (1f - safeRatio);

        // 평가 점수 보정 (40점 만점)
        float evaluationRatio = Mathf.Clamp01(totalEvaluationScore / 40f);
        float adjustedEvaluation = evaluationBasedIncome * evaluationRatio;

        // 품질 보너스
        float qualityBonus = GetQualityBonus(totalEvaluationScore);

        // 최종 수익 = 보장 수익 + (평가 기반 수익 × 품질 보너스)
        float finalIncome = guaranteedIncome + (adjustedEvaluation * qualityBonus);

        return Mathf.RoundToInt(finalIncome);
    }
    
    /// <summary>
    /// 평가 점수에 따른 품질 보너스
    /// </summary>
    private float GetQualityBonus(int totalScore)
    {
        if (totalScore >= 32) return highQualityBonus;      // 평균 8점 이상
        if (totalScore >= 24) return midQualityBonus;       // 평균 6점 이상
        if (totalScore >= 16) return lowQualityBonus;       // 평균 4점 이상
        return veryLowQualityPenalty;                       // 평균 4점 미만
    }
    
    /// <summary>
    /// 평가 점수에 따른 몹 수 보정
    /// </summary>
    public float GetMobMultiplier(int totalEvaluationScore)
    {
        // 점수가 높을수록 몹 수 감소
        float ratio = totalEvaluationScore / 40f;
        return Mathf.Lerp(evaluationMobMultiplierMax, evaluationMobMultiplierMin, ratio);
    }
}
