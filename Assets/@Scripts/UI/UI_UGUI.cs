using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class UI_UGUI : UI_Base
{
    protected Dictionary<Type, Object[]> _objects = new Dictionary<Type, Object[]>();
    protected Action onClosedCallback;
    protected bool isTransitioning;

    [Header("Safe Area Settings")]
    [SerializeField] protected bool applySafeArea = true; // Inspector에서 토글 가능
    [SerializeField] protected string safeAreaPanelName = "SafeAreaPanel"; // 기본 이름
    protected override void Awake()
    {
        if (Object.FindAnyObjectByType<EventSystem>() == null)
            ResourceManager.Instance.Instantiate("EventSystem");

        // Safe Area 자동 적용
        if (applySafeArea)
        {
            ApplySafeAreaAutomatically();
        }
    }
    protected virtual void ApplySafeAreaAutomatically()
    {
        // 1. 지정된 이름의 GameObject 찾기
        GameObject safeAreaPanel = Utils.FindChildGameObject(gameObject, safeAreaPanelName, true);

        if (safeAreaPanel != null)
        {
            // SafeAreaPanel 컴포넌트 추가/확인
            SafeAreaPanel safeArea = Utils.GetOrAddComponent<SafeAreaPanel>(safeAreaPanel);
#if UNITY_EDITOR
            Debug.Log($"[UI_UGUI] Safe Area applied to {gameObject.name} > {safeAreaPanelName}");
#endif
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning($"[UI_UGUI] {gameObject.name}: '{safeAreaPanelName}' not found. Safe Area not applied.");
#endif
        }
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        isTransitioning = false;
    }
    protected override void OnDisable()
    {
        base.OnDisable();

        if (!isTransitioning)
        {
            onClosedCallback?.Invoke();
        }
    }

    protected void BindObjects(Type type) { Bind<GameObject>(type); }
    protected void BindImages(Type type) { Bind<Image>(type); }
    protected void BindTexts(Type type) { Bind<TMP_Text>(type); }
    protected void BindButtons(Type type) { Bind<Button>(type); }

    protected GameObject GetObject(int idx) { return Get<GameObject>(idx); }
    protected TMP_Text GetText(int idx) { return Get<TMP_Text>(idx); }
    protected Button GetButton(int idx) { return Get<Button>(idx); }
    protected Image GetImage(int idx) { return Get<Image>(idx); }

    protected void Bind<T>(Type type) where T : Object
    {
        string[] names = Enum.GetNames(type);
        Object[] objects = new Object[names.Length];

        for (int i = 0; i < names.Length; i++)
        {
            if (typeof(T) == typeof(GameObject))
                objects[i] = Utils.FindChildGameObject(gameObject, names[i], true);
            else
                objects[i] = Utils.FindChildComponent<T>(gameObject, names[i], true);

            if (objects[i] == null)
                Debug.Log($"Failed to bind({names[i]})");
        }

        _objects.Add(typeof(T), objects);
    }

    protected T Get<T>(int idx) where T : Object
    {
        if (_objects.TryGetValue(typeof(T), out Object[] objects) == false)
            return null;

        return objects[idx] as T;
    }
    public UI_UGUI OnClosed(Action callback)
    {
        onClosedCallback = callback;
        return this;
    }
}
