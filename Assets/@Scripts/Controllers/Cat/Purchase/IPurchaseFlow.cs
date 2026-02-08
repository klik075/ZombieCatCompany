using System;

/// <summary>
/// 구매 흐름 인터페이스 (확장 가능)
/// </summary>
public interface IPurchaseFlow
{
    void Start(Action onComplete);
    void Cancel();
}