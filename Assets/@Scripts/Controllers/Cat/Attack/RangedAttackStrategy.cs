using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 원거리 공격 전략 (발사체 공격용)
/// </summary>
public class RangedAttackStrategy : IAttackStrategy
{
    private readonly Member _owner;
    private readonly float _attackSpeed;
    private readonly float _detectionRange;
    private readonly string _projectilePrefabName;
    private readonly float _projectileLaunchDelay = 0.5f; // 발사 딜레이 (애니메이션 타이밍)
    
    private float _lastAttackTime;
    private bool _isAttacking;
    private NormalCat _currentTarget;
    private Coroutine _attackCoroutine;

    public bool IsAttacking => _isAttacking;

    /// <summary>
    /// 현재 공격 데미지 (Member.Power 기반)
    /// </summary>
    private int CurrentDamage => _owner?.AttackDamage ?? 0;

    public RangedAttackStrategy(Member owner, float attackSpeed, float detectionRange, string projectilePrefabName)
    {
        _owner = owner;
        _attackSpeed = attackSpeed;
        _detectionRange = detectionRange;
        _projectilePrefabName = projectilePrefabName;
    }

    public bool CanAttack()
    {
        return Time.time >= _lastAttackTime + (1f / _attackSpeed);
    }

    public void StartAttack()
    {
        _isAttacking = true;
    }

    public void StopAttack()
    {
        _isAttacking = false;
        _currentTarget = null;
        
        // 진행 중인 공격 코루틴 중지
        if (_attackCoroutine != null)
        {
            _owner.StopCoroutine(_attackCoroutine);
            _attackCoroutine = null;
        }
        
        _owner.SetStateIdle();
        _owner.SetAnimationSpeed(1f);
    }

    public void Update()
    {
        if (!_isAttacking)
            return;

        // 타겟 감지
        if (_currentTarget == null || !_currentTarget.IsAlive)
        {
            _currentTarget = FindNearestTarget();
        }

        // 타겟이 있고 공격 가능하면 공격 시작 (코루틴으로)
        if (_currentTarget != null && CanAttack() && _attackCoroutine == null)
        {
            _attackCoroutine = _owner.StartCoroutine(CoPerformAttack(_currentTarget));
            _lastAttackTime = Time.time;
        }
        // 타겟이 없으면 Idle 상태로
        else if (_currentTarget == null && _attackCoroutine == null)
        {
            _owner.SetStateIdle();
        }
    }

    /// <summary>
    /// 공격 수행 코루틴 (애니메이션 타이밍 제어)
    /// </summary>
    private IEnumerator CoPerformAttack(NormalCat target)
    {
        // Attack 애니메이션 시작
        _owner.SetStateAttack();
        _owner.SetAnimationSpeed(1f); // 기본 속도로 재생
        
        // 팔을 뻗는 타이밍까지 대기
        yield return new WaitForSeconds(_projectileLaunchDelay);

        // 대기 후 owner 유효성 확인
        if (_owner == null || _owner.gameObject == null || !_owner.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("[RangedAttackStrategy] Owner destroyed during attack delay");
            _attackCoroutine = null;
            yield break;
        }

        // 타겟 유효성 확인
        if (target == null || !target.IsAlive)
        {
            Debug.LogWarning("[RangedAttackStrategy] Target destroyed during attack delay");
            _owner.SetStateIdle();
            _attackCoroutine = null;
            yield break;
        }
        LaunchProjectile(target);

        // 애니메이션이 완료될 때까지 대기
        float animDuration = _owner.GetCurrentAnimationDuration();
        float remainingTime = animDuration - _projectileLaunchDelay;
        
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }

        // Idle 복귀 전에도 유효성 확인
        if (_owner != null && _owner.gameObject != null && _owner.gameObject.activeInHierarchy)
        {
            _owner.SetStateIdle();
        }
        _attackCoroutine = null;
    }

    private NormalCat FindNearestTarget()
    {
        List<NormalCat> cats = DefenseManager.Instance.GetAliveCats();
        NormalCat nearest = null;
        float minDistance = float.MaxValue;

        // 2D 좌표로 변환
        Vector2 ownerPos2D = new Vector2(_owner.transform.position.x, _owner.transform.position.y);

        foreach (var cat in cats)
        {
            if (!cat.IsAlive)
                continue;

            // 2D 거리 계산 (z축 무시)
            Vector2 catPos2D = new Vector2(cat.transform.position.x, cat.transform.position.y);
            float distance = Vector2.Distance(ownerPos2D, catPos2D);
            
            if (distance <= _detectionRange && distance < minDistance)
            {
                minDistance = distance;
                nearest = cat;
            }
        }

        return nearest;
    }

    private void LaunchProjectile(NormalCat target)
    {
        // 발사체 생성 전 최종 안전 체크
        if (_owner == null || _owner.gameObject == null || !_owner.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("[RangedAttackStrategy] Cannot launch projectile: owner is destroyed");
            return;
        }

        if (target == null || !target.IsAlive)
        {
            Debug.LogWarning("[RangedAttackStrategy] Cannot launch projectile: target is destroyed");
            return;
        }

        // ObjectManager를 통해 발사체 생성 (풀링 사용)
        Projectile projectile = ObjectManager.Instance.SpawnProjectile(_projectilePrefabName, pooling: true);

        // 발사체 생성 실패 체크
        if (projectile == null)
        {
            Debug.LogError("[RangedAttackStrategy] Failed to spawn projectile");
            return;
        }
        projectile.transform.position = _owner.transform.position;
        
        // 현재 시점의 데미지를 동적으로 가져옴
        int currentDamage = CurrentDamage;
        projectile.Initialize(target, currentDamage, null);
        
        Debug.Log($"[RangedAttackStrategy] {_owner.CurrentMemberData?.Name} launched projectile with {currentDamage} damage (Power: {_owner.CurrentMemberData?.Power})");
    }
}
