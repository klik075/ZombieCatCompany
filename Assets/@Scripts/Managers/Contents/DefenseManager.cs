using System.Collections;
using System.Collections.Generic;
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

    private int _currentWave = 0;
    private int _totalCatsToSpawn = 10;
    private int _spawnedCatsCount = 0;
    private float _spawnInterval = 2f;
    private float _pathRetryDelay = 1f;

    private List<NormalCat> _spawnedCats = new List<NormalCat>();
    private Dictionary<NormalCat, Vector2Int> _assignedTargets = new Dictionary<NormalCat, Vector2Int>();
    private Dictionary<NormalCat, Vector2Int> _waitingPositions = new Dictionary<NormalCat, Vector2Int>();

    private bool _isDefenseActive = false;
    private Coroutine _spawnCoroutine;

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

        if (_spawnCoroutine != null)
            StopCoroutine(_spawnCoroutine);
        
        _spawnCoroutine = StartCoroutine(CoSpawnCats());
    }

    public void StopDefense()
    {
        _isDefenseActive = false;

        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }

        ClearAllCats();

        Debug.Log("Defense stopped");
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

    #endregion

    #region 고양이 소환

    private IEnumerator CoSpawnCats()
    {
        WaitForSeconds spawnWait = new WaitForSeconds(_spawnInterval);

        while (_isDefenseActive && _spawnedCatsCount < _totalCatsToSpawn)
        {
            DefenseSpawnInfo spawnInfo = SelectSpawnPosition();

            if (CanSpawnAt(spawnInfo.SpawnPosition))
            {
                SpawnCat(spawnInfo.SpawnPosition);
                _spawnedCatsCount++;
            }
            else
            {
                Debug.Log($"Cannot spawn at {spawnInfo.SpawnPosition}, trying again next interval");
            }

            yield return spawnWait;
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

        _spawnedCats.Add(cat);

        AssignTargetAndStartMovement(cat);
    }

    private void AssignTargetAndStartMovement(NormalCat cat)
    {
        Vector2Int? targetPosition = SelectAvailableTargetPosition();
        
        if (targetPosition.HasValue)
        {
            _assignedTargets[cat] = targetPosition.Value;
            StartCatMovement(cat);
            Debug.Log($"[DefenseManager] Cat assigned to target {targetPosition.Value}");
        }
        else
        {
            // 타겟이 없으면 타겟 근처 대기 위치로 이동
            Vector2Int waitingPos = FindNearestWaitingPosition();
            _waitingPositions[cat] = waitingPos;
            StartCatMovement(cat);
            Debug.Log($"[DefenseManager] Cat assigned to waiting position {waitingPos}");
        }
    }

    #endregion

    #region 이동 제어

    /// <summary>
    /// 비어있고 아직 할당되지 않은 타겟 위치 선택
    /// </summary>
    private Vector2Int? SelectAvailableTargetPosition()
    {
        HashSet<Vector2Int> assignedTargets = new HashSet<Vector2Int>(_assignedTargets.Values);

        foreach (var target in _targetPositions)
        {
            if (MapManager.Instance.CanMove(target.TargetPosition) && 
                !assignedTargets.Contains(target.TargetPosition))
            {
                return target.TargetPosition;
            }
        }

        return null;
    }

    /// <summary>
    /// 타겟 근처의 대기 위치 찾기
    /// </summary>
    private Vector2Int FindNearestWaitingPosition()
    {
        // 모든 타겟 위치의 중심 계산
        Vector2 center = Vector2.zero;
        foreach (var target in _targetPositions)
        {
            center += new Vector2(target.TargetPosition.x, target.TargetPosition.y);
        }
        center /= _targetPositions.Length;

        Vector2Int centerCell = new Vector2Int(Mathf.RoundToInt(center.x), Mathf.RoundToInt(center.y));

        // BFS로 타겟 근처의 빈 공간 찾기
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        
        queue.Enqueue(centerCell);
        visited.Add(centerCell);

        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
            new Vector2Int(1, 1), new Vector2Int(1, -1),
            new Vector2Int(-1, 1), new Vector2Int(-1, -1)
        };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            // 이미 대기 중인 고양이가 있는지 확인
            bool isOccupiedByWaiting = false;
            foreach (var waitingPos in _waitingPositions.Values)
            {
                if (waitingPos == current)
                {
                    isOccupiedByWaiting = true;
                    break;
                }
            }

            if (MapManager.Instance.CanMove(current) && !isOccupiedByWaiting)
            {
                return current;
            }

            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighbor = current + dir;

                if (!visited.Contains(neighbor) && MapManager.Instance.IsWalkable(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        // 찾지 못하면 중심 위치 반환
        return centerCell;
    }

    private void StartCatMovement(NormalCat cat)
    {
        StartCoroutine(CoCatMovement(cat));
    }

    private IEnumerator CoCatMovement(NormalCat cat)
    {
        WaitForSeconds retryWait = new WaitForSeconds(_pathRetryDelay);

        while (cat != null && cat.IsAlive)
        {
            Vector2Int? targetPosition = null;

            // 타겟이 할당되었는지 확인
            if (_assignedTargets.ContainsKey(cat))
            {
                targetPosition = _assignedTargets[cat];
            }
            else
            {
                // 타겟 재할당 시도
                Vector2Int? newTarget = SelectAvailableTargetPosition();
                
                if (newTarget.HasValue)
                {
                    // 타겟 할당
                    _assignedTargets[cat] = newTarget.Value;
                    targetPosition = newTarget.Value;
                    
                    // 대기 위치에서 제거
                    if (_waitingPositions.ContainsKey(cat))
                    {
                        _waitingPositions.Remove(cat);
                    }
                    
                    Debug.Log($"[DefenseManager] Waiting cat assigned to target {newTarget.Value}");
                }
                else
                {
                    // 여전히 타겟이 없으면 대기 위치로 이동
                    if (_waitingPositions.ContainsKey(cat))
                    {
                        targetPosition = _waitingPositions[cat];
                    }
                    else
                    {
                        // 대기 위치 재할당
                        Vector2Int waitingPos = FindNearestWaitingPosition();
                        _waitingPositions[cat] = waitingPos;
                        targetPosition = waitingPos;
                    }
                }
            }

            if (!targetPosition.HasValue)
            {
                yield return retryWait;
                continue;
            }

            // 목표 위치에 도착했는지 확인
            if (cat.CellPosition == targetPosition.Value)
            {
                // 타겟 위치에 도착했으면 공격 시작
                if (_assignedTargets.ContainsKey(cat) && _assignedTargets[cat] == targetPosition.Value)
                {
                    Debug.Log($"[DefenseManager] Cat reached target {targetPosition.Value}!");
                    OnCatReachedTarget(cat);
                    yield break;
                }
                else
                {
                    // 대기 위치에 도착했으면 타겟이 비기를 기다림
                    Debug.Log($"[DefenseManager] Cat waiting at {targetPosition.Value}");
                    yield return retryWait;
                    continue;
                }
            }

            // 경로 찾기
            List<Vector2Int> path = MapManager.Instance.FindPath(cat.CellPosition, targetPosition.Value);

            if (path != null && path.Count > 0)
            {
                bool moveSuccess = false;
                yield return StartCoroutine(CoFollowPath(cat, path, success => moveSuccess = success));

                if (!moveSuccess)
                {
                    yield return retryWait;
                }
            }
            else
            {
                yield return retryWait;
            }
        }
    }

    private IEnumerator CoFollowPath(NormalCat cat, List<Vector2Int> path, System.Action<bool> onComplete)
    {
        foreach (Vector2Int nextPosition in path)
        {
            if (!cat.IsAlive)
            {
                onComplete?.Invoke(false);
                yield break;
            }

            if (!MapManager.Instance.CanMove(nextPosition))
            {
                onComplete?.Invoke(false);
                yield break;
            }

            cat.MoveTo(nextPosition);

            while (cat.IsMoving)
            {
                yield return null;
            }

            MapManager.Instance.MoveTo(cat, nextPosition, true);
        }

        onComplete?.Invoke(true);
    }

    private void OnCatReachedTarget(NormalCat cat)
    {
        Debug.Log($"[DefenseManager] Cat reached fence at {cat.CellPosition}!");

        // NormalCat의 공격 시작
        cat.StartAttack();
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

    #endregion

    #region 유틸리티

    private void ClearAllCats()
    {
        foreach (var cat in _spawnedCats)
        {
            if (cat != null)
            {
                cat.StopAttack();
                MapManager.Instance.UnregisterCat(cat.CellPosition);
                ObjectManager.Instance.Despawn(cat);
            }
        }

        _spawnedCats.Clear();
        _assignedTargets.Clear();
        _waitingPositions.Clear();
        Debug.Log("All defense cats cleared");
    }

    public void RemoveCat(NormalCat cat)
    {
        if (_spawnedCats.Contains(cat))
        {
            _spawnedCats.Remove(cat);

            // 할당된 타겟 해제
            if (_assignedTargets.ContainsKey(cat))
            {
                Vector2Int freedTarget = _assignedTargets[cat];
                _assignedTargets.Remove(cat);
                Debug.Log($"[DefenseManager] Target {freedTarget} freed");
            }

            // 대기 위치 해제
            if (_waitingPositions.ContainsKey(cat))
            {
                _waitingPositions.Remove(cat);
            }

            MapManager.Instance.UnregisterCat(cat.CellPosition);
            ObjectManager.Instance.Despawn(cat);

            Debug.Log($"[DefenseManager] Cat removed, remaining: {_spawnedCats.Count}");
        }
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

        Gizmos.color = Color.green;
        foreach (var spawnInfo in _spawnPositions)
        {
            Vector3 worldPos = MapManager.Instance.CellToWorld(spawnInfo.SpawnPosition);
            Gizmos.DrawWireSphere(worldPos, 0.3f);
        }

        Gizmos.color = Color.red;
        foreach (var targetInfo in _targetPositions)
        {
            Vector3 worldPos = MapManager.Instance.CellToWorld(targetInfo.TargetPosition);
            Gizmos.DrawWireCube(worldPos, Vector3.one * 0.5f);
        }

        // 대기 위치 표시 (노란색)
        Gizmos.color = Color.yellow;
        foreach (var waitingPos in _waitingPositions.Values)
        {
            Vector3 worldPos = MapManager.Instance.CellToWorld(waitingPos);
            Gizmos.DrawWireSphere(worldPos, 0.2f);
        }
    }

    #endregion
}
