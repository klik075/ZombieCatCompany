using UnityEngine;

[CreateAssetMenu(fileName = "GameBalanceConfig", menuName = "Config/GameBalanceConfig")]
public class GameBalanceConfig : ScriptableObject
{
    [Header("경제 설정")]
    [SerializeField] private int foodPricePerUnit = 10;
    [SerializeField] private int developmentCost = 1000;
    [SerializeField] private float contractDepositRate = 2.0f;
    [SerializeField] private int gamePrice = 100;
    
    [Header("멤버 월급")]
    [SerializeField] private int salaryIncreasePerEducation = 2;
    
    [Header("수익 공식")]
    [SerializeField] private int baseIncome = 1000;
    [SerializeField] private int baseIncomePerYear = 400;
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
    [SerializeField] private int baseRewardPerCat = 100;  // 기본 보상
    [SerializeField] private int rewardIncreasePerYear = 50;  // 년도당 증가
    [SerializeField] private float evaluationMobMultiplierMin = 0.8f;
    [SerializeField] private float evaluationMobMultiplierMax = 1.5f;

    [Header("몹 소환 공식")]
    [SerializeField] private int baseSpawnCount = 8;  // 1년차 기본 몹 수
    [SerializeField] private float spawnCountIncreasePerYear = 1.1f;  // 년도당 증가 비율
    [SerializeField] private int maxSpawnCount = 30;  // 최대 몹 수

    [Header("몹 스탯 공식")]
    [SerializeField] private int baseHealth = 30;  // 1년차 기본 체력
    [SerializeField] private float healthGrowthRate = 1.05f;  // 체력 성장률
    [SerializeField] private int baseDamage = 8;  // 1년차 기본 공격력
    [SerializeField] private float damageGrowthRate = 1.05f;  // 공격력 성장률

    [Header("강탈 모드 - 식량 드롭")]
    [SerializeField] private int baseFoodDropPerMob = 2;  // 기본 2개
    [SerializeField] private float foodDropIncreasePerYear = 0.4f;  // 년당 +0.4개

    // Public Properties
    public int GamePrice => gamePrice;
    public int FoodPricePerUnit => foodPricePerUnit;
    public int DevelopmentCost => developmentCost;
    public float ContractDepositRate => contractDepositRate;
    public int SalaryIncreasePerEducation => salaryIncreasePerEducation;
    public int BaseRewardPerCat => baseRewardPerCat;
    
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
    /// 년도별 기본 몹 수 계산
    /// </summary>
    /// <param name="year">현재 년도</param>
    /// <returns>기본 몹 수 (평가 보정 전)</returns>
    public int CalculateBaseSpawnCount(int year)
    {
        // 년도에 따라 증가하는 기본 몹 수
        // 예: 1년 = 8, 2년 = 9, 3년 = 10, ... 20년 = 30
        int calculatedCount = baseSpawnCount + Mathf.RoundToInt((year - 1) * spawnCountIncreasePerYear);

        // 최대값 제한
        return Mathf.Min(calculatedCount, maxSpawnCount);
    }

    /// <summary>
    /// 평가 점수에 따른 몹 수 배율 계산
    /// </summary>
    /// <param name="totalEvaluationScore">평가 점수 (0~40)</param>
    /// <returns>몹 수 배율 (0.8~1.5)</returns>
    public float GetMobMultiplier(int totalEvaluationScore)
    {
        // 평가 점수를 0~1 범위로 정규화
        float ratio = Mathf.Clamp01(totalEvaluationScore / 40f);

        // 점수가 높을수록(ratio가 1에 가까울수록) 몹 수 감소
        // ratio 0 (평가 0점) → 1.5배 (많이 소환)
        // ratio 1 (평가 40점) → 0.8배 (적게 소환)
        return Mathf.Lerp(evaluationMobMultiplierMax, evaluationMobMultiplierMin, ratio);
    }

    /// <summary>
    /// 년도와 평가 점수에 따른 최종 몹 수 계산
    /// </summary>
    /// <param name="year">현재 년도</param>
    /// <param name="totalEvaluationScore">평가 점수 (0~40)</param>
    /// <returns>최종 소환할 몹 수</returns>
    public int CalculateFinalMobCount(int year, int totalEvaluationScore)
    {
        // 년도별 기본 몹 수 계산
        int baseCount = CalculateBaseSpawnCount(year);

        // 평가 점수에 따른 배율 계산
        float multiplier = GetMobMultiplier(totalEvaluationScore);

        // 최종 몹 수 = 기본 몹 수 × 평가 배율
        int finalMobCount = Mathf.RoundToInt(baseCount * multiplier);

        // 최소 1마리는 보장
        return Mathf.Clamp(finalMobCount, 1, maxSpawnCount);
    }

    /// <summary>
    /// 년도별 몹 1마리당 보상 계산
    /// </summary>
    public int GetRewardPerCat(int year)
    {
        // 년도가 증가할수록 몹 1마리당 보상 증가
        int reward = baseRewardPerCat + (year * rewardIncreasePerYear);
        return reward;
    }

    /// <summary>
    /// 년도별 디펜스 보상 계산
    /// </summary>
    /// <param name="year">현재 년도</param>
    /// <param name="killedCats">처치한 몹 수</param>
    /// <returns>총 보상 골드</returns>
    public int CalculateDefenseReward(int year, int killedCats)
    {
        // 년도별 몹당 보상
        int rewardPerCat = GetRewardPerCat(year);
        
        // 총 보상 = 몹당 보상 × 처치 수
        int totalReward = rewardPerCat * killedCats;
        
        return totalReward;
    }

    /// <summary>
    /// 연차별 몹당 식량 드롭 수 계산 (강탈 모드)
    /// </summary>
    /// <param name="year">현재 년도</param>
    /// <returns>몹 1마리당 드롭하는 식량 수</returns>
    public int CalculateFoodDropPerMob(int year)
    {
        float drop = baseFoodDropPerMob + (year * foodDropIncreasePerYear);
        return Mathf.Max(1, Mathf.RoundToInt(drop));  // 최소 1개 보장
    }

    #region 디펜스 몹 스탯
    /// <summary>
    /// 년도별 몹 체력 계산
    /// </summary>
    public int CalculateMobHealth(int year)
    {
        // 지수적 성장
        float health = baseHealth * Mathf.Pow(healthGrowthRate, year - 1);
        return Mathf.RoundToInt(health);
    }

    /// <summary>
    /// 년도별 몹 공격력 계산
    /// </summary>
    public int CalculateMobDamage(int year)
    {
        // 지수적 성장
        float damage = baseDamage * Mathf.Pow(damageGrowthRate, year - 1);
        return Mathf.RoundToInt(damage);
    }
    #endregion
}
