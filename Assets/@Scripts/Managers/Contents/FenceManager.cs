using UnityEngine;
using static Define;

public class FenceManager : Singleton<FenceManager>
{
    private Fence _currentFence;
    public Fence CurrentFence => _currentFence;

    #region 초기화

    /// <summary>
    /// 씬의 Fence 찾기 및 초기화
    /// </summary>
    private void InitForScene()
    {
        // 씬에서 Fence 컴포넌트 찾기
        _currentFence = FindFirstObjectByType<Fence>();

        if (_currentFence == null)
        {
            Debug.LogWarning("No Fence found in scene!");
            _currentFence = ObjectManager.Instance.SpawnFence();
            //위치 지정 필요
            return;
        }

        Debug.Log("Fence found in scene and initialized");
    }

    #endregion

    #region 수리

    /// <summary>
    /// 펜스를 수리할 수 있는지 확인
    /// </summary>
    public bool CanRepairFence()
    {
        if (_currentFence == null)
            return false;

        // 수리가 필요한지 확인
        if (!_currentFence.NeedsRepair())
            return false;

        // 골드가 충분한지 확인
        if (GameManager.Instance.Gold < _currentFence.RepairCost)
            return false;

        return true;
    }

    /// <summary>
    /// 펜스 수리
    /// </summary>
    public bool RepairFence()
    {
        if (!CanRepairFence())
        {
            Debug.LogWarning("Cannot repair fence");
            return false;
        }

        // 비용 차감
        GameManager.Instance.Gold -= _currentFence.RepairCost;

        // 펜스 수리 실행
        _currentFence.ExecuteRepair();

        //EventManager.Instance.TriggerEvent(EEventType.FenceRepaired);

        return true;
    }

    #endregion

    #region 강화

    /// <summary>
    /// 펜스를 강화할 수 있는지 확인
    /// </summary>
    public bool CanEnhanceFence()
    {
        if (_currentFence == null)
            return false;

        // 다음 레벨이 존재하는지 확인
        if (!_currentFence.CanEnhanceToNextLevel())
        {
            Debug.Log("Fence is already at max level");
            return false;
        }

        // 골드가 충분한지 확인
        if (GameManager.Instance.Gold < _currentFence.EnhanceCost)
            return false;

        return true;
    }

    /// <summary>
    /// 펜스 강화 시도 (확률 기반)
    /// </summary>
    public bool TryEnhanceFence()
    {
        if (!CanEnhanceFence())
        {
            Debug.LogWarning("Cannot enhance fence");
            return false;
        }

        // 비용 차감 (실패해도 차감)
        int cost = _currentFence.EnhanceCost;
        GameManager.Instance.Gold -= cost;

        // 확률 체크
        float randomValue = UnityEngine.Random.value;
        bool success = randomValue < _currentFence.EnhanceProbability;
        Debug.Log($"뽑힌 랜덤 값 : {randomValue}");

        if (success)
        {
            // 강화 성공
            _currentFence.ExecuteEnhanceSuccess();
            EventManager.Instance.TriggerEvent(EEventType.FenceStateChanged);
            return true;
        }
        else
        {
            // 강화 실패
            _currentFence.ExecuteEnhanceFailed();
            EventManager.Instance.TriggerEvent(EEventType.FenceStateChanged);
            return false;
        }
        
    }

    #endregion

    #region 전투

    /// <summary>
    /// 펜스가 데미지를 받음
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (_currentFence == null)
            return;

        int actualDamage = _currentFence.TakeDamage(damage);

        EventManager.Instance.TriggerEvent(EEventType.FenceDamaged);

        // 펜스 파괴 체크
        if (_currentFence.IsDestroyed())
        {
            OnFenceDestroyed();
        }
    }

    /// <summary>
    /// 펜스 파괴 처리
    /// </summary>
    private void OnFenceDestroyed()
    {
        //EventManager.Instance.TriggerEvent(EEventType.FenceDestroyed);
        Debug.LogWarning("Fence destroyed!");
        // TODO: 게임 오버 또는 엔딩 로직
    }

    /// <summary>
    /// 펜스 공격력 가져오기
    /// </summary>
    public int GetFenceAttackDamage()
    {
        if (_currentFence == null)
            return 0;

        return _currentFence.Damage;
    }

    #endregion

    #region 저장/로드

    /// <summary>
    /// 저장 데이터 가져오기
    /// </summary>
    public FenceSaveData GetSaveData()
    {
        if (_currentFence == null)
            return new FenceSaveData();

        return _currentFence.GetSaveData();
    }

    /// <summary>
    /// 저장 데이터로부터 로드
    /// </summary>
    public void LoadFromSaveData(FenceSaveData saveData)
    {
        InitForScene();

        if (_currentFence == null)
        {
            Debug.LogWarning("Fence component not found in scene!");
            return;
        }

        if (saveData == null)
        {
            Debug.LogWarning("FenceSaveData is null! Initializing with default values.");
            _currentFence.Initialize(1);
            EventManager.Instance.TriggerEvent(EEventType.FenceStateChanged);
            return;
        }

        _currentFence.LoadFromSaveData(saveData);

        EventManager.Instance.TriggerEvent(EEventType.FenceStateChanged);
    }

    #endregion
}
