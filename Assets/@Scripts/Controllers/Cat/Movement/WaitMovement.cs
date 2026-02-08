using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 현재 위치에서 일정 시간 대기하는 전략
/// </summary>
public class WaitMovement : IMovementStrategy
{
    private float _waitDuration;
    private float _elapsedTime;
    
    public bool IsComplete { get; private set; }
    
    public WaitMovement(float waitDuration)
    {
        _waitDuration = waitDuration;
        _elapsedTime = 0f;
        IsComplete = false;
    }
    
    public void Execute(Cat cat)
    {
        // 시간만 카운트 (이동하지 않음)
        _elapsedTime += Time.deltaTime;
        
        if (_elapsedTime >= _waitDuration)
        {
            IsComplete = true;
        }
    }
    
    public void Reset()
    {
        _elapsedTime = 0f;
        IsComplete = false;
    }
    
    /// <summary>
    /// 대기 중에는 경로가 없음 (현재 위치만 반환)
    /// </summary>
    public List<Vector2Int> GetDebugPath()
    {
        return new List<Vector2Int>();
    }
}
