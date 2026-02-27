using UnityEngine;

/// <summary>
/// 일반 고양이 (디펜스 적 고양이)
/// </summary>
public class NormalCat : Cat
{
    public int AttackDamage { get; private set; } = 2;
    public float AttackSpeed { get; private set; } = 1f;
    public int MaxHealth { get; private set; } = 10;
    public int CurrentHealth { get; private set; }

    public bool IsAlive => CurrentHealth > 0;

    protected override void Awake()
    {
        base.Awake();
        CurrentHealth = MaxHealth;

        // 근접 공격 전략 설정
        SetAttackStrategy(new MeleeAttackStrategy(this, AttackDamage, AttackSpeed));
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

        // 공격 전략 재설정
        SetAttackStrategy(new MeleeAttackStrategy(this, AttackDamage, AttackSpeed));
    }
}
