using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MovementPoolManager : Singleton<MovementPoolManager>
{
    private Dictionary<Type, Stack<IMovementStrategy>> _pools = new Dictionary<Type, Stack<IMovementStrategy>>();

    private const int MAX_POOL_SIZE = 5;

    /// <summary>
    /// 제네릭 Get 메서드 (타입 자동 판별)
    /// </summary>
    public T Get<T>() where T : IMovementStrategy, new()
    {
        Type type = typeof(T);

        // 풀이 없으면 생성
        if (!_pools.ContainsKey(type))
        {
            _pools[type] = new Stack<IMovementStrategy>();
        }

        // 풀에서 가져오기
        if (_pools[type].Count > 0)
        {
            return (T)_pools[type].Pop();
        }

        // 없으면 새로 생성
        return new T();
    }

    /// <summary>
    /// 풀에 반환
    /// </summary>
    public void Return(IMovementStrategy movement)
    {
        if (movement == null)
            return;

        Type type = movement.GetType();

        // 풀이 없으면 생성
        if (!_pools.ContainsKey(type))
        {
            _pools[type] = new Stack<IMovementStrategy>();
        }

        // 최대 크기 제한
        if (_pools[type].Count < MAX_POOL_SIZE)
        {
            movement.Reset();
            _pools[type].Push(movement);
        }
    }

    /// <summary>
    /// 모든 풀 초기화
    /// </summary>
    public void Clear()
    {
        _pools.Clear();
    }
}
