using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

[System.Serializable]
public struct DefenseSpawnInfo
{
    public Vector2Int SpawnPosition;
    public float SpawnWeight;

    public DefenseSpawnInfo(Vector2Int position, float weight = 1f)
    {
        SpawnPosition = position;
        SpawnWeight = weight;
    }
}

[System.Serializable]
public struct DefenseTargetInfo
{
    public Vector2Int TargetPosition;

    public DefenseTargetInfo(Vector2Int position)
    {
        TargetPosition = position;
    }
}

public class DefenseManager : Singleton<DefenseManager>
{
    private DefenseSpawnInfo[] _spawnPositions = new DefenseSpawnInfo[]
    {
        new DefenseSpawnInfo(new Vector2Int(-1, -17), 1f),
        new DefenseSpawnInfo(new Vector2Int(-2, -17), 1f),
        new DefenseSpawnInfo(new Vector2Int(-3, -17), 1f),
        new DefenseSpawnInfo(new Vector2Int(-4, -17), 1f),
    };

    private DefenseTargetInfo[] _targetPositions = new DefenseTargetInfo[]
    {
        new DefenseTargetInfo(new Vector2Int(0, -4)),
        new DefenseTargetInfo(new Vector2Int(-1, -4)),
        new DefenseTargetInfo(new Vector2Int(-2, -4)),
        new DefenseTargetInfo(new Vector2Int(-3, -4)),
        new DefenseTargetInfo(new Vector2Int(-4, -4)),
        new DefenseTargetInfo(new Vector2Int(-5, -4)),
    };

    [Header("Randomization Settings")]
    [SerializeField] private float _spawnIntervalMin = 0.5f;
    [SerializeField] private float _spawnIntervalMax = 1f;
    [SerializeField] private bool _randomTargetSelection = true;
    [SerializeField] private bool _randomizeCatStats = true;
    [SerializeField] private int _minHealth = 10;
    [SerializeField] private int _maxHealth = 15;
    [SerializeField] private int _minDamage = 1;
    [SerializeField] private int _maxDamage = 3;
    [SerializeField] private float _minAttackSpeed = 0.8f;
    [SerializeField] private float _maxAttackSpeed = 1.2f;

    [Header("Reward Settings")]
    [SerializeField] private int _rewardPerCat = 100; // 고양이당 보상
    [SerializeField] private int _foodPerCat = 1; // 고양이당 강탈 식량

    private int _currentWave = 0;
    private int _totalCatsToSpawn = 10;
    private int _spawnedCatsCount = 0;
    private float _targetReassignInterval = 1f;

    private List<NormalCat> _spawnedCats = new List<NormalCat>();
    private HashSet<NormalCat> _waitingCats = new HashSet<NormalCat>();
    private HashSet<NormalCat> _attackingCats = new HashSet<NormalCat>();
    private Dictionary<NormalCat, Vector2Int> _assignedTargets = new Dictionary<NormalCat, Vector2Int>();

    private bool _isDefenseActive = false;
    private Coroutine _spawnCoroutine;
    private Coroutine _reassignCoroutine;

    #region 디펜스 시작/종료

    public void StartDefense()
    {
        if (_isDefenseActive)
        {
            Debug.LogWarning("Defense is already active!");
            return;
        }

        _isDefenseActive = true;
        _currentWave = 1;
        _spawnedCatsCount = 0;

        Debug.Log($"Defense started! Wave {_currentWave}");

        EventManager.Instance.TriggerEvent(EEventType.DefenseStarted);

        EnableAllMembersAttack();

        if (_spawnCoroutine != null)
            StopCoroutine(_spawnCoroutine);
        
        _spawnCoroutine = StartCoroutine(CoSpawnCats());

        // 대기 중인 고양이 재할당 체크 시작
        if (_reassignCoroutine != null)
            StopCoroutine(_reassignCoroutine);
        
        _reassignCoroutine = StartCoroutine(CoReassignWaitingCats());
    }

    public void StopDefense()
    {
        _isDefenseActive = false;

        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }

        if (_reassignCoroutine != null)
        {
            StopCoroutine(_reassignCoroutine);
            _reassignCoroutine = null;
        }

        DisableAllMembersAttack();
        ClearAllCats();

        Debug.Log("Defense stopped");

        EventManager.Instance.TriggerEvent(EEventType.DefenseStopped);
    }

    public void StartNextWave()
    {
        _currentWave++;
        _spawnedCatsCount = 0;
        _totalCatsToSpawn = CalculateWaveSpawnCount(_currentWave);

        Debug.Log($"Starting Wave {_currentWave} with {_totalCatsToSpawn} cats");

        if (_spawnCoroutine != null)
            StopCoroutine(_spawnCoroutine);
        
        _spawnCoroutine = StartCoroutine(CoSpawnCats());
    }

    private void EnableAllMembersAttack()
    {
        List<Member> members = MemberManager.Instance.GetAllMembers();
        
        foreach (var member in members)
        {
            if (member != null)
            {
                member.StartAttack();
                Debug.Log($"[DefenseManager] {member.name} attack enabled");
            }
        }
    }

    private void DisableAllMembersAttack()
    {
        List<Member> members = MemberManager.Instance.GetAllMembers();
        
        foreach (var member in members)
        {
            if (member != null)
            {
                member.StopAttack();
                Debug.Log($"[DefenseManager] {member.name} attack disabled");
            }
        }
    }

    #endregion

    #region 고양이 소환

    private IEnumerator CoSpawnCats()
    {
        while (_isDefenseActive && _spawnedCatsCount < _totalCatsToSpawn)
        {
            DefenseSpawnInfo spawnInfo = SelectSpawnPosition();

            if (CanSpawnAt(spawnInfo.SpawnPosition))
            {
                SpawnCat(spawnInfo.SpawnPosition);
                _spawnedCatsCount++;
                
                // UI 갱신 이벤트
                EventManager.Instance.TriggerEvent(EEventType.DefenseProgressChanged);
            }
            else
            {
                Debug.Log($"Cannot spawn at {spawnInfo.SpawnPosition}, trying again next interval");
            }

            // 랜덤 스폰 간격
            float spawnInterval = Random.Range(_spawnIntervalMin, _spawnIntervalMax);
            yield return new WaitForSeconds(spawnInterval);
        }

        Debug.Log($"Wave {_currentWave} spawn completed. {_spawnedCatsCount}/{_totalCatsToSpawn} cats spawned");
    }

    private DefenseSpawnInfo SelectSpawnPosition()
    {
        float totalWeight = 0f;
        foreach (var info in _spawnPositions)
        {
            totalWeight += info.SpawnWeight;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var info in _spawnPositions)
        {
            currentWeight += info.SpawnWeight;
            if (randomValue <= currentWeight)
            {
                return info;
            }
        }

        return _spawnPositions[0];
    }

    private bool CanSpawnAt(Vector2Int position)
    {
        return MapManager.Instance.CanMove(position);
    }

    private void SpawnCat(Vector2Int spawnPosition)
    {
        NormalCat cat = ObjectManager.Instance.SpawnNormalCat("CatBlack");

        if (cat == null)
        {
            Debug.LogError("Failed to spawn cat!");
            return;
        }

        cat.CellPosition = spawnPosition;
        cat.transform.position = MapManager.Instance.CellToWorld(spawnPosition);
        MapManager.Instance.RegisterCat(cat, spawnPosition);

        // 능력치 랜덤화
        if (_randomizeCatStats)
        {
            int health = Random.Range(_minHealth, _maxHealth + 1);
            int damage = Random.Range(_minDamage, _maxDamage + 1);
            float attackSpeed = Random.Range(_minAttackSpeed, _maxAttackSpeed);
            cat.SetStats(health, damage, attackSpeed);
            Debug.Log($"[DefenseManager] Cat spawned at {spawnPosition} - HP:{health} DMG:{damage} AtkSpd:{attackSpeed:F2}");
        }

        _spawnedCats.Add(cat);
        AssignTargetAndStartMovement(cat);
    }

    private void AssignTargetAndStartMovement(NormalCat cat)
    {
        if (cat == null || !cat.IsAlive)
            return;

        Vector2Int? targetPosition = SelectAvailableTargetPosition();
        
        if (targetPosition.HasValue)
        {
            // 타겟 할당 및 이동
            _assignedTargets[cat] = targetPosition.Value;
            
            // 대기 목록에서 제거
            _waitingCats.Remove(cat);
            
            var strategy = new FenceTargetMovementStrategy(cat, cat.GridManager, targetPosition.Value)
                .SetOnArrived(() => OnCatReachedTarget(cat));
            
            cat.SetMovementStrategy(strategy);
            
            Debug.Log($"[DefenseManager] Cat at {cat.CellPosition} assigned to fence target {targetPosition.Value}");
        }
        else
        {
            // 대기 상태로 설정
            if (!_waitingCats.Contains(cat))
            {
                _waitingCats.Add(cat);
            }
            
            // 울타리 근처 대기 위치로 이동
            Vector2Int waitingPos = FindNearestWaitingPositionNearFence(cat.CellPosition);
            
            var strategy = new FenceTargetMovementStrategy(cat, cat.GridManager, waitingPos)
                .SetOnArrived(() => OnCatReachedWaitingPosition(cat));
            
            cat.SetMovementStrategy(strategy);
            
            Debug.Log($"[DefenseManager] Cat at {cat.CellPosition} assigned to waiting position {waitingPos} near fence (no fence targets available)");
        }
    }

    /// <summary>
    /// 대기 중인 고양이들에게 타겟 재할당 시도 (주기적)
    /// </summary>
    private IEnumerator CoReassignWaitingCats()
    {
        WaitForSeconds wait = new WaitForSeconds(_targetReassignInterval);

        while (_isDefenseActive)
        {
            yield return wait;

            if (_waitingCats.Count == 0)
                continue;

            // 대기 중인 고양이들을 리스트로 복사
            List<NormalCat> waitingList = new List<NormalCat>(_waitingCats);

            foreach (var cat in waitingList)
            {
                if (cat == null || !cat.IsAlive)
                {
                    _waitingCats.Remove(cat);
                    continue;
                }

                // 이동 중이거나 공격 중이면 스킵
                if (cat.IsMoving || cat.IsAttacking)
                    continue;

                // 빈 타겟 찾기
                Vector2Int? newTarget = SelectAvailableTargetPosition();

                if (newTarget.HasValue)
                {
                    // 타겟 발견! 재할당
                    _waitingCats.Remove(cat);
                    _assignedTargets[cat] = newTarget.Value;

                    // 기존 전략 제거 후 새 전략 설정
                    cat.ClearMovementStrategy();

                    var strategy = new FenceTargetMovementStrategy(cat, cat.GridManager, newTarget.Value)
                        .SetOnArrived(() => OnCatReachedTarget(cat));

                    cat.SetMovementStrategy(strategy);

                    Debug.Log($"[DefenseManager] Waiting cat at {cat.CellPosition} reassigned to fence target {newTarget.Value}");
                }
            }
        }
    }

    /// <summary>
    /// 고양이가 타겟(울타리 앞)에 도착했을 때
    /// </summary>
    private void OnCatReachedTarget(NormalCat cat)
    {
        if (cat == null || !cat.IsAlive)
            return;

        Debug.Log($"[DefenseManager] Cat reached fence target at {cat.CellPosition}! Starting attack.");
        
        // 전략 제거 (더 이상 이동하지 않도록)
        cat.ClearMovementStrategy();
        
        // 공격 목록에 추가
        if (!_attackingCats.Contains(cat))
        {
            _attackingCats.Add(cat);
        }
        
        // 대기 목록에서 제거
        _waitingCats.Remove(cat);
        
        // 공격 시작
        cat.StartAttack();
    }

    /// <summary>
    /// 고양이가 대기 위치에 도착했을 때
    /// </summary>
    private void OnCatReachedWaitingPosition(NormalCat cat)
    {
        if (cat == null || !cat.IsAlive)
            return;

        Debug.Log($"[DefenseManager] Cat reached waiting position at {cat.CellPosition}");
        
        // 전략 제거 (대기 위치에서 멈춤)
        cat.ClearMovementStrategy();
        
        // 대기 목록에 확실히 추가 (중복 방지)
        if (!_waitingCats.Contains(cat))
        {
            _waitingCats.Add(cat);
            Debug.Log($"[DefenseManager] Cat added to waiting list");
        }
    }

    #endregion

    #region 이동 제어

    private Vector2Int? SelectAvailableTargetPosition()
    {
        HashSet<Vector2Int> assignedTargets = new HashSet<Vector2Int>(_assignedTargets.Values);

        List<Vector2Int> availableTargets = new List<Vector2Int>();

        foreach (var target in _targetPositions)
        {
            Vector2Int targetPos = target.TargetPosition;
            
            // 이미 할당되지 않았고, 이동 가능한 위치인지 확인
            if (!assignedTargets.Contains(targetPos) && MapManager.Instance.CanMove(targetPos))
            {
                availableTargets.Add(targetPos);
            }
        }

        if (availableTargets.Count == 0)
        {
            Debug.Log($"[DefenseManager] No available fence targets (assigned: {assignedTargets.Count})");
            return null;
        }

        if (_randomTargetSelection)
        {
            int randomIndex = Random.Range(0, availableTargets.Count);
            return availableTargets[randomIndex];
        }
        else
        {
            return availableTargets[0];
        }
    }

    /// <summary>
    /// 울타리 근처에서 대기 위치 찾기 (타겟 뒤쪽 우선)
    /// </summary>
    private Vector2Int FindNearestWaitingPositionNearFence(Vector2Int startPosition)
    {
        // 타겟 포지션들을 기준으로 후보 위치 생성
        List<Vector2Int> candidatePositions = new List<Vector2Int>();

        // 각 타겟 위치 뒤쪽(y 좌표가 더 큰 쪽) 1~3칸 위치 추가
        foreach (var target in _targetPositions)
        {
            Vector2Int targetPos = target.TargetPosition;
            
            // 바로 뒤쪽 (y+1, y+2, y+3)
            for (int offset = 1; offset <= 3; offset++)
            {
                candidatePositions.Add(new Vector2Int(targetPos.x, targetPos.y + offset));
                // 좌우로도 조금씩 퍼지도록
                candidatePositions.Add(new Vector2Int(targetPos.x - 1, targetPos.y + offset));
                candidatePositions.Add(new Vector2Int(targetPos.x + 1, targetPos.y + offset));
            }
        }

        // 후보 위치를 울타리와의 거리순으로 정렬
        candidatePositions = candidatePositions
            .OrderBy(pos => GetMinDistanceToFenceTargets(pos))
            .ToList();

        // 후보 위치 중 이동 가능한 첫 번째 위치 찾기
        foreach (var candidate in candidatePositions)
        {
            if (MapManager.Instance.CanMove(candidate) && !IsTargetPosition(candidate))
            {
                Debug.Log($"[DefenseManager] Found waiting position near fence: {candidate}");
                return candidate;
            }
        }

        // 후보에서 못 찾으면 BFS로 타겟 근처 검색
        Vector2Int? bfsResult = FindWaitingPositionByBFS();
        if (bfsResult.HasValue)
        {
            Debug.Log($"[DefenseManager] Found waiting position via BFS: {bfsResult.Value}");
            return bfsResult.Value;
        }

        // 최후의 수단: 타겟 중심에서 뒤쪽으로 오프셋
        Vector2 center = Vector2.zero;
        foreach (var target in _targetPositions)
        {
            center += new Vector2(target.TargetPosition.x, target.TargetPosition.y);
        }
        center /= _targetPositions.Length;

        Vector2Int fallbackPos = new Vector2Int(
            Mathf.RoundToInt(center.x), 
            Mathf.RoundToInt(center.y) + 3 // 뒤쪽으로
        );
        
        Debug.LogWarning($"[DefenseManager] Could not find waiting position, using fallback {fallbackPos}");
        return fallbackPos;
    }

    /// <summary>
    /// 위치에서 가장 가까운 울타리 타겟까지의 거리
    /// </summary>
    private float GetMinDistanceToFenceTargets(Vector2Int position)
    {
        float minDistance = float.MaxValue;
        
        foreach (var target in _targetPositions)
        {
            float distance = Vector2Int.Distance(position, target.TargetPosition);
            if (distance < minDistance)
            {
                minDistance = distance;
            }
        }
        
        return minDistance;
    }

    /// <summary>
    /// BFS로 타겟 근처에서 대기 위치 찾기
    /// </summary>
    private Vector2Int? FindWaitingPositionByBFS()
    {
        // 모든 타겟 위치를 시작점으로 BFS 시작
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        foreach (var target in _targetPositions)
        {
            queue.Enqueue(target.TargetPosition);
            visited.Add(target.TargetPosition);
        }

        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
            new Vector2Int(1, 1), new Vector2Int(1, -1),
            new Vector2Int(-1, 1), new Vector2Int(-1, -1)
        };

        int maxSearchDepth = 15;
        int currentDepth = 0;

        while (queue.Count > 0 && currentDepth < maxSearchDepth)
        {
            int levelSize = queue.Count;
            
            for (int i = 0; i < levelSize; i++)
            {
                Vector2Int current = queue.Dequeue();

                // 타겟 위치가 아니고 이동 가능하면 반환
                if (!IsTargetPosition(current) && MapManager.Instance.CanMove(current))
                {
                    return current;
                }

                foreach (Vector2Int dir in directions)
                {
                    Vector2Int neighbor = current + dir;

                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
            
            currentDepth++;
        }

        return null;
    }

    private bool IsTargetPosition(Vector2Int position)
    {
        foreach (var target in _targetPositions)
        {
            if (target.TargetPosition == position)
                return true;
        }
        return false;
    }

    #endregion

    #region 웨이브 관리

    private int CalculateWaveSpawnCount(int wave)
    {
        return 10 + (wave - 1) * 5;
    }

    public bool IsWaveCompleted()
    {
        return _spawnedCatsCount >= _totalCatsToSpawn && _spawnedCats.Count == 0;
    }

    public int GetTotalCatsCount()
    {
        return _totalCatsToSpawn;
    }

    public int GetRemainingCatsCount()
    {
        int notSpawnedYet = _totalCatsToSpawn - _spawnedCatsCount;
        int aliveCount = 0;
        
        foreach (var cat in _spawnedCats)
        {
            if (cat != null && cat.IsAlive)
            {
                aliveCount++;
            }
        }
        
        return notSpawnedYet + aliveCount;
    }

    public float GetProgressPercentage()
    {
        if (_totalCatsToSpawn == 0)
            return 0f;
        
        int remaining = GetRemainingCatsCount();
        int eliminated = _totalCatsToSpawn - remaining;
        
        return (float)eliminated / _totalCatsToSpawn * 100f;
    }

    public float GetRemainingPercentage()
    {
        return 100f - GetProgressPercentage();
    }

    public int GetCurrentWave()
    {
        return _currentWave;
    }

    public bool IsDefenseActive()
    {
        return _isDefenseActive;
    }

    #endregion

    #region 유틸리티

    private void ClearAllCats()
    {
        foreach (var cat in _spawnedCats)
        {
            if (cat != null)
            {
                cat.StopAttack();
                cat.ClearMovementStrategy();
                MapManager.Instance.UnregisterCat(cat.CellPosition);
                ObjectManager.Instance.Despawn(cat);
            }
        }

        _spawnedCats.Clear();
        _assignedTargets.Clear();
        _waitingCats.Clear();
        _attackingCats.Clear();
        Debug.Log("All defense cats cleared");
    }

    /// <summary>
    /// 살아있는 NormalCat 목록 가져오기
    /// </summary>
    public List<NormalCat> GetAliveCats()
    {
        List<NormalCat> aliveCats = new List<NormalCat>();
        
        foreach (var cat in _spawnedCats)
        {
            if (cat != null && cat.IsAlive)
            {
                aliveCats.Add(cat);
            }
        }

        return aliveCats;
    }

    public void RemoveCat(NormalCat cat)
    {
        if (_spawnedCats.Contains(cat))
        {
            _spawnedCats.Remove(cat);
            _waitingCats.Remove(cat);
            _attackingCats.Remove(cat);

            // 할당된 타겟 해제
            if (_assignedTargets.ContainsKey(cat))
            {
                Vector2Int freedTarget = _assignedTargets[cat];
                _assignedTargets.Remove(cat);
                Debug.Log($"[DefenseManager] Fence target {freedTarget} freed by cat death");
            }

            MapManager.Instance.UnregisterCat(cat.CellPosition);
            ObjectManager.Instance.Despawn(cat);

            Debug.Log($"[DefenseManager] Cat removed, remaining: {_spawnedCats.Count}");

            // UI 갱신 이벤트 발생
            EventManager.Instance.TriggerEvent(EEventType.DefenseProgressChanged);

            // 웨이브 완료 체크
            CheckWaveCompletion();
        }
    }

    /// <summary>
    /// 웨이브 완료 여부 체크 및 결과 팝업 표시
    /// </summary>
    private void CheckWaveCompletion()
    {
        if (!_isDefenseActive)
            return;

        // 모든 고양이가 스폰되었고, 살아있는 고양이가 없으면 완료 (= 모두 처치)
        if (_spawnedCatsCount >= _totalCatsToSpawn && _spawnedCats.Count == 0)
        {
            Debug.Log($"[DefenseManager] Wave {_currentWave} completed! All cats eliminated.");
            
            // 보상 계산 (웨이브 물량 기반)
            int defenseReward = CalculateDefenseReward();
            int defeatedFoods = CalculateDefeatedFoods();

            // 디펜스 결과 데이터 생성
            var defenseData = new DefenseResultData(
                defeatedCats: _totalCatsToSpawn, // 모든 물량 처치
                defenseReward: defenseReward,
                defeatedFoods: defeatedFoods
            );

            // 결과 팝업 표시
            var resultPopup = UIManager.Instance.ShowPopupUI<UI_ResultsReportPopup>();
            resultPopup.SetInfo(defenseData, () =>
            {
                // 결과 확인 후 디펜스 종료
                StopDefense();
                Debug.Log("[DefenseManager] Defense ended after result confirmation");
                
                // 보상 지급 (골드는 이미 UI_ResultsReportPopup에서 추가됨)
                // 식량 추가 지급
                if (defeatedFoods > 0)
                {
                    GameManager.Instance.Food += defeatedFoods;
                    Debug.Log($"[DefenseManager] Added {defeatedFoods} food");
                }
                
                // 모든 데이터 저장 및 NightScene 로드
                SaveAllDataAndLoadNightScene();
            });
        }
    }

    /// <summary>
    /// 모든 게임 데이터 저장 및 NightScene 로드
    /// </summary>
    private void SaveAllDataAndLoadNightScene()
    {
        Debug.Log("[DefenseManager] Saving all game data before loading NightScene...");

        try
        {
            // NightScene용 멤버 위치 초기화 (자리로 복귀)
            ResetMembersToSeatsForNightScene();
            
            // SaveManager를 통해 모든 게임 데이터 저장
            SaveManager.Instance.SaveGameData();
            
            Debug.Log("[DefenseManager] All game data saved successfully");
            
            // 데이터 확인 로그
            LogSavedData();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DefenseManager] Failed to save game data: {e.Message}\n{e.StackTrace}");
        }

        // NightScene으로 전환
        LoadNightScene();
    }

    /// <summary>
    /// NightScene 진입 전 멤버들을 자리 위치로 초기화
    /// </summary>
    private void ResetMembersToSeatsForNightScene()
    {
        List<Member> members = MemberManager.Instance.GetAllMembers();
        
        Debug.Log("[DefenseManager] Resetting member positions for NightScene...");
        
        foreach (var member in members)
        {
            if (member == null)
                continue;
            
            // 멤버의 인덱스 가져오기
            int memberIndex = MemberManager.Instance.GetIndex(member);
            
            if (memberIndex == -1)
            {
                Debug.LogWarning($"[DefenseManager] Member {member.name} not found in MemberManager");
                continue;
            }
            
            // 멤버의 NightScene 자리 정보 가져오기 (기존 메서드 사용)
            MemberSeatInfo seatInfo = MemberManager.Instance.GetMemberSeatInfo(memberIndex, isNight: true);
            
            // CellPosition을 자리로 설정
            member.CellPosition = seatInfo.SeatPosition;
            
            // 이동 전략 초기화
            member.ClearMovementStrategy();
            
            // 공격 중지
            member.StopAttack();
            
            // 상태를 Idle로 변경
            member.SetStateForced(Cat.ECatState.Idle);
        }
        
        Debug.Log($"[DefenseManager] Reset {members.Count} members to their NightScene seats");
    }

    /// <summary>
    /// 저장된 데이터 확인 로그
    /// </summary>
    private void LogSavedData()
    {
        var gameData = GameManager.Instance.MyGameData;
        if (gameData == null)
        {
            Debug.LogWarning("[DefenseManager] GameData is null!");
            return;
        }

        Debug.Log($"[DefenseManager] Saved Data Summary:");
        Debug.Log($"  - Company: {gameData.CompanyData.CompanyName}");
        Debug.Log($"  - Year: {gameData.CompanyData.Year}");
        Debug.Log($"  - Gold: {gameData.CompanyData.Gold}");
        Debug.Log($"  - Food: {gameData.CompanyData.Food}");
        Debug.Log($"  - Members: {gameData.CompanyData.MemberSaveDatas?.Count ?? 0}");
        
        if (gameData.MorningData.FenceSaveData != null)
        {
            Debug.Log($"  - Fence HP: {gameData.MorningData.FenceSaveData.CurrentHp}");
            Debug.Log($"  - Fence Level: {gameData.MorningData.FenceSaveData.EnhanceLevel}");
            Debug.Log($"  - Fence Durability: {gameData.MorningData.FenceSaveData.CurrentDurability}");
        }
    }

    /// <summary>
    /// NightScene으로 전환
    /// </summary>
    private void LoadNightScene()
    {
        Debug.Log("[DefenseManager] Loading NightScene...");
        
        // SceneManager를 통해 NightScene 로드
        SceneManager.Instance.LoadScene(EScene.NightScene);
    }

    /// <summary>
    /// 방어 보상 계산 (웨이브 물량 * 고양이당 보상)
    /// </summary>
    private int CalculateDefenseReward()
    {
        // 웨이브 물량 * 고양이당 보상
        int baseReward = _totalCatsToSpawn * _rewardPerCat;
        
        int totalReward = Mathf.RoundToInt(baseReward);

        Debug.Log($"[DefenseManager] Reward calculated: {totalReward}");

        return totalReward;
    }

    /// <summary>
    /// 강탈한 식량 계산 (웨이브 물량 * 고양이당 식량)
    /// </summary>
    private int CalculateDefeatedFoods()
    {
        if (GameManager.Instance.GameMode == EGameMode.Purchase)
            return 0;

        // 웨이브 물량 * 고양이당 식량
        int totalFoods = _totalCatsToSpawn * _foodPerCat;
        
        Debug.Log($"[DefenseManager] Foods calculated: {totalFoods}");
        
        return totalFoods;
    }

    public void SetSpawnPositions(DefenseSpawnInfo[] positions)
    {
        _spawnPositions = positions;
    }

    public void SetTargetPositions(DefenseTargetInfo[] positions)
    {
        _targetPositions = positions;
    }

    #endregion

    #region 디버그

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || MapManager.Instance == null)
            return;

        // 스폰 위치 (녹색)
        Gizmos.color = Color.green;
        foreach (var spawnInfo in _spawnPositions)
        {
            Vector3 worldPos = MapManager.Instance.CellToWorld(spawnInfo.SpawnPosition);
            Gizmos.DrawWireSphere(worldPos, 0.3f);
        }

        // 타겟 위치 (빨간색)
        Gizmos.color = Color.red;
        foreach (var targetInfo in _targetPositions)
        {
            Vector3 worldPos = MapManager.Instance.CellToWorld(targetInfo.TargetPosition);
            Gizmos.DrawWireCube(worldPos, Vector3.one * 0.5f);
        }

        // 대기 중인 고양이 (노란색)
        Gizmos.color = Color.yellow;
        foreach (var waitingCat in _waitingCats)
        {
            if (waitingCat != null)
            {
                Vector3 worldPos = waitingCat.transform.position;
                Gizmos.DrawWireSphere(worldPos, 0.3f);
            }
        }

        // 공격 중인 고양이 (시안색)
        Gizmos.color = Color.cyan;
        foreach (var attackingCat in _attackingCats)
        {
            if (attackingCat != null)
            {
                Vector3 worldPos = attackingCat.transform.position;
                Gizmos.DrawWireSphere(worldPos, 0.4f);
            }
        }
    }

    #endregion
}
