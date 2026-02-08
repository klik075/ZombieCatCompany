using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 이동 전략 인터페이스 (다른 프로젝트에서도 재사용 가능)
/// </summary>
public interface IMovementStrategy
{
    /// <summary>
    /// 이동 전략 실행
    /// </summary>
    void Execute(Cat cat);
    
    /// <summary>
    /// 이동이 완료되었는지 여부
    /// </summary>
    bool IsComplete { get; }
    
    /// <summary>
    /// 전략 초기화
    /// </summary>
    void Reset();
    
    /// <summary>
    /// 디버그용 경로 정보 가져오기 (에디터 시각화)
    /// </summary>
    List<Vector2Int> GetDebugPath();
}
