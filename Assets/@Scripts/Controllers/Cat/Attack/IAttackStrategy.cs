using UnityEngine;

public interface IAttackStrategy
{
    /// <summary>
    /// 공격 가능 여부
    /// </summary>
    bool CanAttack();

    /// <summary>
    /// 공격 시작
    /// </summary>
    void StartAttack();

    /// <summary>
    /// 공격 중지
    /// </summary>
    void StopAttack();

    /// <summary>
    /// 매 프레임 업데이트 (타겟 감지, 쿨다운 체크 등)
    /// </summary>
    void Update();

    /// <summary>
    /// 현재 공격 중인지
    /// </summary>
    bool IsAttacking { get; }
}
