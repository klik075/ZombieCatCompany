using System.Collections;
using UnityEngine;

/// <summary>
/// 일반 고양이 (디펜스 적 고양이)
/// </summary>
public class NormalCat : Cat
{
    public int AttackDamage { get; private set; } = 2;
    public float AttackSpeed { get; private set; } = 1f;
    public int MaxHealth { get; private set; } = 4;
    public int CurrentHealth { get; private set; }

    private float _lastAttackTime;
    private Coroutine _attackCoroutine;

    public bool IsAlive => CurrentHealth > 0;
    public bool IsAttacking => _attackCoroutine != null;

    protected override void Awake()
    {
        base.Awake();
        CurrentHealth = MaxHealth;
    }

    protected override void Start()
    {
        base.Start();
    }

    public override void Update()
    {
        base.Update();
    }

    /// <summary>
    /// 데미지 받기
    /// </summary>
    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        CurrentHealth = Mathf.Max(0, CurrentHealth);

        Debug.Log($"[NormalCat] Took {damage} damage. HP: {CurrentHealth}/{MaxHealth}");

        if (CurrentHealth <= 0)
        {
            OnDeath();
        }
    }

    /// <summary>
    /// 공격 가능 여부 확인
    /// </summary>
    public bool CanAttack()
    {
        return Time.time >= _lastAttackTime + (1f / AttackSpeed);
    }

    /// <summary>
    /// 울타리 공격 시작
    /// </summary>
    public void StartAttack()
    {
        if (_attackCoroutine == null)
        {
            _attackCoroutine = StartCoroutine(CoAttackFence());
        }
    }

    /// <summary>
    /// 울타리 공격 중지
    /// </summary>
    public void StopAttack()
    {
        if (_attackCoroutine != null)
        {
            StopCoroutine(_attackCoroutine);
            _attackCoroutine = null;
        }

        // 공격 중지 시 Idle 상태로 복귀
        State = ECatState.Idle;
    }

    /// <summary>
    /// 울타리 공격 코루틴
    /// </summary>
    private IEnumerator CoAttackFence()
    {
        Fence fence = FenceManager.Instance.CurrentFence;

        if (fence == null)
        {
            Debug.LogError("[NormalCat] No fence found!");
            yield break;
        }

        Debug.Log($"[NormalCat] {name} starts attacking fence!");

        // 공격 상태로 전환
        State = ECatState.Attack;

        // 공격 속도에 맞춰 애니메이션 속도 조정
        // AttackSpeed = 1이면 기본 속도, 2면 2배 빠르게
        SetAnimationSpeed(AttackSpeed);

        while (IsAlive && fence != null && !fence.IsDestroyed())
        {
            if (CanAttack())
            {
                _lastAttackTime = Time.time;

                // 울타리 공격
                FenceManager.Instance.TakeDamage(AttackDamage);
                Debug.Log($"[NormalCat] Dealt {AttackDamage} damage to fence");

                // 울타리 반격
                int counterDamage = fence.Damage;
                TakeDamage(counterDamage);

                if (!IsAlive)
                {
                    break;
                }
            }

            yield return null;
        }

        _attackCoroutine = null;

        // 공격 종료 시 Idle 상태로 복귀
        State = ECatState.Idle;
        SetAnimationSpeed(1f); // 애니메이션 속도 원래대로

        Debug.Log($"[NormalCat] {name} stopped attacking");
    }

    /// <summary>
    /// 사망 처리
    /// </summary>
    private void OnDeath()
    {
        Debug.Log($"[NormalCat] Died at {CellPosition}");

        StopAttack();
        DefenseManager.Instance.RemoveCat(this);
    }

    /// <summary>
    /// 능력치 설정
    /// </summary>
    public void SetStats(int health, int damage, float attackSpeed)
    {
        MaxHealth = health;
        CurrentHealth = health;
        AttackDamage = damage;
        AttackSpeed = attackSpeed;
    }
}
