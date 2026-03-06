using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class FenceSaveData
{
    public int EnhanceLevel;        // 현재 강화 레벨
    public int CurrentHp;           // 현재 HP
    public int CurrentDurability;   // 현재 내구도

    public FenceSaveData()
    {
        EnhanceLevel = 1;
        CurrentHp = 100;
        CurrentDurability = 100;
    }
}

public class Fence : ObjectBase
{
    // 정적 데이터 참조
    private FenceData _fenceData;
    public FenceData FenceData => _fenceData;

    // 런타임 변경되는 수치
    private int _enhanceLevel;
    private int _currentHp;
    private int _currentDurability;

    // UI 참조 (선택사항 - Inspector에서 할당)
    [Header("UI References (Optional)")]
    [SerializeField] private Slider _hpBar;
    [SerializeField] private Slider _durabilityBar;
    [SerializeField] private TMPro.TextMeshProUGUI _enhanceLevelText;

    // 프로퍼티
    public int EnhanceLevel
    {
        get => _enhanceLevel;
        private set
        {
            _enhanceLevel = value;
            UpdateVisuals();
        }
    }

    public int CurrentHp
    {
        get => _currentHp;
        set
        {
            _currentHp = Mathf.Clamp(value, 0, MaxHp);
            UpdateVisuals();
        }
    }

    public int CurrentDurability
    {
        get => _currentDurability;
        set
        {
            _currentDurability = Mathf.Clamp(value, 0, MaxDurability);
            UpdateVisuals();
        }
    }

    public int MaxHp => _fenceData?.MaxHp ?? 100;
    public int MaxDurability => _fenceData?.Durability ?? 100;
    public int Defense => _fenceData?.Defense ?? 1;
    public int Damage => _fenceData?.Damage ?? 1;
    public int RepairCost => _fenceData?.RepairCost ?? int.MaxValue;
    public int EnhanceCost => _fenceData?.EnhanceCost ?? int.MaxValue;
    public float EnhanceProbability => _fenceData?.Probability ?? 0f;
    public int ReducedDurability => _fenceData?.ReducedDurability ?? 5;

    #region Unity Lifecycle

    protected override void Awake()
    {
        base.Awake();
        // 초기화는 FenceManager에서 호출
    }

    #endregion

    #region 생성 및 초기화
    /// <summary>
    /// 특정 레벨로 초기화
    /// </summary>
    public void Initialize(int enhanceLevel = 1)
    {
        _enhanceLevel = enhanceLevel;
        LoadFenceData(_enhanceLevel);
        ResetToMaxStats();
        UpdateVisuals();
    }

    /// <summary>
    /// FenceData 로드
    /// </summary>
    private void LoadFenceData(int enhanceLevel)
    {
        if (DataManager.Instance.FenceDict.TryGetValue(enhanceLevel, out FenceData data))
        {
            _fenceData = data;
        }
        else
        {
            //if(DataManager.Instance.FenceDict.Values.Count > 0)
            Debug.LogError($"FenceData not found for enhance level: {enhanceLevel}");
            // 기본 레벨 1 데이터 로드
            DataManager.Instance.FenceDict.TryGetValue(1, out _fenceData);
        }
    }

    /// <summary>
    /// HP와 내구도를 최대치로 복구
    /// </summary>
    private void ResetToMaxStats()
    {
        if (_fenceData != null)
        {
            _currentHp = _fenceData.MaxHp;
            _currentDurability = _fenceData.Durability;
        }
    }

    #endregion

    #region 수리

    /// <summary>
    /// 수리 가능 여부 확인 (비용 체크 제외)
    /// </summary>
    public bool NeedsRepair()
    {
        return _currentDurability < MaxDurability;
    }

    /// <summary>
    /// 펜스 수리 실행 (내부 상태만 변경)
    /// </summary>
    public void ExecuteRepair()
    {
        CurrentDurability = MaxDurability;
        Debug.Log($"Fence repaired. Durability: {CurrentDurability}/{MaxDurability}");
    }

    #endregion

    #region 강화

    /// <summary>
    /// 강화 가능 여부 확인 (다음 레벨 존재 여부만 체크)
    /// </summary>
    public bool CanEnhanceToNextLevel()
    {
        return DataManager.Instance.FenceDict.ContainsKey(_enhanceLevel + 1);
    }

    /// <summary>
    /// 펜스 강화 성공 처리
    /// </summary>
    public void ExecuteEnhanceSuccess()
    {
        int oldLevel = _enhanceLevel;
        _enhanceLevel++;

        LoadFenceData(_enhanceLevel);
        ResetToMaxStats();

        Debug.Log($"Fence enhanced: {oldLevel} -> {_enhanceLevel}");

        // 강화 성공 이펙트 (선택사항)
        PlayEnhanceSuccessEffect();
    }

    /// <summary>
    /// 펜스 강화 실패 처리
    /// </summary>
    public void ExecuteEnhanceFailed()
    {
        int reducedAmount = ReducedDurability;
        CurrentDurability -= reducedAmount;

        Debug.Log($"Enhancement failed. Durability reduced by {reducedAmount}");

        // 강화 실패 이펙트 (선택사항)
        PlayEnhanceFailedEffect();
    }

    #endregion

    #region 데미지 처리

    /// <summary>
    /// 데미지 받기
    /// </summary>
    public int TakeDamage(int incomingDamage)
    {
        // 방어력 적용
        int actualDamage = Mathf.Max(1, incomingDamage - Defense);

        CurrentHp -= actualDamage;

        Debug.Log($"Fence took {actualDamage} damage. HP: {CurrentHp}/{MaxHp}");

        // 데미지 이펙트 (선택사항)
        PlayDamageEffect();

        return actualDamage;
    }

    /// <summary>
    /// 펜스가 파괴되었는지 확인
    /// </summary>
    public bool IsDestroyed()
    {
        return _currentHp <= 0;
    }

    #endregion

    #region UI 업데이트

    /// <summary>
    /// 비주얼 업데이트
    /// </summary>
    private void UpdateVisuals()
    {
        UpdateHpBar();
        UpdateDurabilityBar();
        UpdateEnhanceLevelText();
    }

    private void UpdateHpBar()
    {
        if (_hpBar != null)
        {
            _hpBar.value = GetHpRatio();
        }
    }

    private void UpdateDurabilityBar()
    {
        if (_durabilityBar != null)
        {
            _durabilityBar.value = GetDurabilityRatio();
        }
    }

    private void UpdateEnhanceLevelText()
    {
        if (_enhanceLevelText != null)
        {
            _enhanceLevelText.text = FenceData.EnhanceToString(_enhanceLevel);
        }
    }

    /// <summary>
    /// HP 비율 (0~1)
    /// </summary>
    public float GetHpRatio()
    {
        if (MaxHp <= 0)
            return 0f;
        return (float)CurrentHp / MaxHp;
    }

    /// <summary>
    /// 내구도 비율 (0~1)
    /// </summary>
    public float GetDurabilityRatio()
    {
        if (MaxDurability <= 0)
            return 0f;
        return (float)CurrentDurability / MaxDurability;
    }

    #endregion

    #region 이펙트 (선택사항)

    private void PlayDamageEffect()
    {
        // TODO: 데미지 이펙트 재생
        // 예: 흔들림, 파티클, 사운드 등
    }

    private void PlayEnhanceSuccessEffect()
    {
        // TODO: 강화 성공 이펙트
        // 예: 반짝임, 파티클, 사운드 등
    }

    private void PlayEnhanceFailedEffect()
    {
        // TODO: 강화 실패 이펙트
    }

    #endregion

    #region 저장/로드

    /// <summary>
    /// 저장 데이터 생성
    /// </summary>
    public FenceSaveData GetSaveData()
    {
        Debug.Log($"저장 펜스 상태 : 레벨 - {_enhanceLevel}, HP - {_currentHp}, 내구도 - {_currentDurability}");
        return new FenceSaveData
        {
            EnhanceLevel = _enhanceLevel,
            CurrentHp = _currentHp,
            CurrentDurability = _currentDurability,
        };
    }

    /// <summary>
    /// 저장 데이터로부터 로드
    /// </summary>
    public void LoadFromSaveData(FenceSaveData saveData)
    {
        if (saveData == null)
        {
            Debug.LogWarning("FenceSaveData is null!");
            Initialize();
            return;
        }

        _enhanceLevel = saveData.EnhanceLevel;

        LoadFenceData(_enhanceLevel);

        _currentHp = saveData.CurrentHp;
        _currentDurability = saveData.CurrentDurability;

        UpdateVisuals();

        Debug.Log($"로드 펜스 상태 : 레벨 - {_enhanceLevel}, HP - {_currentHp}, 내구도 - {_currentDurability}");
    }

    #endregion
}
