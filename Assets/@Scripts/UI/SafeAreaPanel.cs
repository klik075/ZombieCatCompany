using UnityEngine;

/// <summary>
/// Safe Area를 적용하는 패널 컴포넌트
/// Canvas의 최상위 패널 RectTransform에 추가하여 사용
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SafeAreaPanel : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Rect _lastSafeArea = Rect.zero;
    private Vector2Int _lastScreenSize = Vector2Int.zero;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    private void Update()
    {
        // 화면 크기나 Safe Area가 변경되었을 때만 업데이트 (회전 등)
        if (_lastSafeArea != Screen.safeArea || 
            _lastScreenSize.x != Screen.width || 
            _lastScreenSize.y != Screen.height)
        {
            ApplySafeArea();
        }
    }

    /// <summary>
    /// Safe Area를 RectTransform에 적용
    /// </summary>
    private void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        // 화면 크기 기준으로 정규화 (0~1)
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        _rectTransform.anchorMin = anchorMin;
        _rectTransform.anchorMax = anchorMax;

        _lastSafeArea = safeArea;
        _lastScreenSize = new Vector2Int(Screen.width, Screen.height);

#if UNITY_EDITOR
        Debug.Log($"[SafeAreaPanel] {gameObject.name} Applied - Min: {anchorMin}, Max: {anchorMax}");
#endif
    }
}
