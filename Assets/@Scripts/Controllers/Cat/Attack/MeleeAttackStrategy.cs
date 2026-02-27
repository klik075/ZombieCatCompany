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
    }

    public void StopAttack()
    {
        if (_attackCoroutine != null)
        {
            _owner.StopCoroutine(_attackCoroutine);
            _attackCoroutine = null;
        }

        _owner.SetStateIdle(); // State 대신 내부 메서드 사용
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
            yield break;
        }

        Debug.Log($"[MeleeAttack] {_owner.name} starts attacking fence!");

        _owner.SetStateAttack(); // State 대신 내부 메서드 사용
        _owner.SetAnimationSpeed(_attackSpeed);

        while (_owner.IsAlive && fence != null && !fence.IsDestroyed())
        {
            if (CanAttack())
            {
                _lastAttackTime = Time.time;

                FenceManager.Instance.TakeDamage(_damage);
                Debug.Log($"[MeleeAttack] Dealt {_damage} damage to fence");

                int counterDamage = fence.Damage;
                _owner.TakeDamage(counterDamage);

                if (!_owner.IsAlive)
                {
                    break;
                }
            }

            yield return null;
        }

        _attackCoroutine = null;
        _owner.SetStateIdle(); // State 대신 내부 메서드 사용
        _owner.SetAnimationSpeed(1f);

        Debug.Log($"[MeleeAttack] {_owner.name} stopped attacking");
    }
}
