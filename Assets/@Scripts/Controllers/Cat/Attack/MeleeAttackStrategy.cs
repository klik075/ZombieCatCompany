using System.Collections;
using UnityEngine;

/// <summary>
/// 근접 공격 전략 (울타리 공격용)
/// </summary>
public class MeleeAttackStrategy : IAttackStrategy
{
    private readonly NormalCat _owner;
    private readonly int _damage;
    private readonly float _attackSpeed;
    
    private float _lastAttackTime;
    private Coroutine _attackCoroutine;

    public bool IsAttacking => _attackCoroutine != null;

    public MeleeAttackStrategy(NormalCat owner, int damage, float attackSpeed)
    {
        _owner = owner;
        _damage = damage;
        _attackSpeed = attackSpeed;
    }

    public bool CanAttack()
    {
        return Time.time >= _lastAttackTime + (1f / _attackSpeed);
    }

    public void StartAttack()
    {
        if (_attackCoroutine == null)
        {
            _attackCoroutine = _owner.StartCoroutine(CoAttackFence());
        }
        else
        {
            Debug.LogWarning($"[MeleeAttack] Attack already in progress for {_owner.name}");
        }
    }

    public void StopAttack()
    {
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
        // 근접 공격은 명시적으로 StartAttack 호출 필요
    }

    private IEnumerator CoAttackFence()
    {
        Fence fence = FenceManager.Instance.CurrentFence;

        if (fence == null)
        {
            Debug.LogError("[MeleeAttack] No fence found!");
            _attackCoroutine = null;
            yield break;
        }

        Debug.Log($"[MeleeAttack] {_owner.name} starts attacking fence!");

        while (_owner.IsAlive && fence != null && !fence.IsDestroyed())
        {
            if (CanAttack())
            {
                // 각 공격마다 애니메이션 재생
                yield return _owner.StartCoroutine(CoPerformSingleAttack(fence));
                
                _lastAttackTime = Time.time;
            }

            yield return null;
        }

        _attackCoroutine = null;
        _owner.SetStateIdle();
        _owner.SetAnimationSpeed(1f);

        Debug.Log($"[MeleeAttack] {_owner.name} stopped attacking");
    }

    /// <summary>
    /// 단일 공격 수행 (애니메이션 동기화)
    /// </summary>
    private IEnumerator CoPerformSingleAttack(Fence fence)
    {
        // Attack 상태로 전환 (애니메이션 재시작)
        _owner.SetStateAttack();
        _owner.SetAnimationSpeed(_attackSpeed);
        
        Debug.Log($"[MeleeAttack] Attack animation started");

        // 애니메이션 길이 가져오기
        float animDuration = _owner.GetCurrentAnimationDuration();
        
        // 애니메이션 속도 보정
        float adjustedDuration = animDuration / _attackSpeed;
        
        // 애니메이션 중간 지점에서 데미지 (약 50% 지점)
        float damageDelay = adjustedDuration * 0.5f;
        
        yield return new WaitForSeconds(damageDelay);
        
        // 데미지 적용
        if (_owner.IsAlive && fence != null && !fence.IsDestroyed())
        {
            FenceManager.Instance.TakeDamage(_damage);
            Debug.Log($"[MeleeAttack] Dealt {_damage} damage to fence");

            // 울타리 반격
            int counterDamage = fence.Damage;
            _owner.TakeDamage(counterDamage);
        }
        
        // 애니메이션 나머지 부분 대기
        float remainingTime = adjustedDuration - damageDelay;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }
        
        // 짧은 Idle 상태 (다음 공격 전 대기)
        _owner.SetStateIdle();
        yield return new WaitForSeconds(0.1f);
    }
}
