using UnityEngine;

public class Utils
{
    public static T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
            component = go.AddComponent<T>();

        return component;
    }

    public static GameObject FindChildGameObject(GameObject go, string name, bool recursive = false)
    {
        if (go == null)
            return null;

        if (recursive == false)
        {
            // 직계 자식만 검색
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform child = go.transform.GetChild(i);
                if (child.name == name)
                    return child.gameObject;
            }
        }
        else
        {
            // 모든 하위 계층 검색 (비활성화된 오브젝트 포함)
            Transform[] children = go.GetComponentsInChildren<Transform>(includeInactive: true);
            foreach (Transform child in children)
            {
                if (child.name == name)
                    return child.gameObject;
            }
        }

        return null;
    }

    public static T FindChildComponent<T>(GameObject go, string name = null, bool recursive = false) where T : Object
    {
        if (go == null)
            return null;

        if (recursive == false)
        {
            // 직계 자식만 검색
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform child = go.transform.GetChild(i);

                if (string.IsNullOrEmpty(name) || child.name == name)
                {
                    T component = child.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }
        }
        else
        {
            // 모든 하위 계층 검색 (비활성화된 오브젝트 포함)
            T[] children = go.GetComponentsInChildren<T>(includeInactive: true);
            foreach (T child in children)
            {
                if (string.IsNullOrEmpty(name) || child.name == name)
                    return child;
            }
        }

        return null;
    }

    public static Transform GetRootTransform(ref Transform root, string name, Transform parent = null)
    {
        if (root == null)
        {
            GameObject go = GameObject.Find(name);
            if (go == null)
                go = new GameObject(name);

            root = go.transform;
            root.SetParent(parent);
        }

        return root;
    }
}
