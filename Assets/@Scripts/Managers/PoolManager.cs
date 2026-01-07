using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;


public class Pool
{
    private GameObject _prefab;
    private ObjectPool<GameObject> _objectPool;

    private Transform _root;
    private Transform Root
    {
        get
        {
            return Utils.GetRootTransform(ref _root, $"@{_prefab.name}Pool");
        }
    }

    public Pool(GameObject prefab)
    {
        _prefab = prefab;
        _objectPool = new ObjectPool<GameObject>(OnCreate, OnGet, OnRelease, OnDestroy);
    }

    public void Reserve(int count)
    {
        List<GameObject> objects = new List<GameObject>();

        for (int i = 0; i < count; i++)
            objects.Add(Pop());

        for (int i = 0; i < count; i++)
            Push(objects[i]);
    }

    public void Push(GameObject go)
    {
        _objectPool.Release(go);
    }

    public GameObject Pop()
    {
        return _objectPool.Get();
    }

    #region Funcs
    private GameObject OnCreate()
    {
        GameObject go = ResourceManager.Instance.Instantiate(_prefab.name);
        go.name = _prefab.name;
        return go;
    }

    private void OnGet(GameObject go)
    {
        go.transform.SetParent(null);
        go.SetActive(true);
    }

    private void OnRelease(GameObject go)
    {
        go.transform.SetParent(Root);
        go.SetActive(false);
    }

    private void OnDestroy(GameObject go)
    {
        ResourceManager.Instance.Destroy(go);
    }
    #endregion
}

public class PoolManager : Singleton<PoolManager>
{
    private Dictionary<string, Pool> _pools = new Dictionary<string, Pool>();

    public void Reserve(string prefabName, int count)
    {
        GameObject prefab = ResourceManager.Instance.Get<GameObject>(prefabName);
        Reserve(prefab, count);
    }

    public void Reserve(GameObject prefab, int count)
    {
        if (_pools.ContainsKey(prefab.name) == false)
            CreatePool(prefab);

        _pools[prefab.name].Reserve(count);
    }

    public GameObject Pop(string prefabName)
    {
        GameObject prefab = ResourceManager.Instance.Get<GameObject>(prefabName);
        return Pop(prefab);
    }

    public GameObject Pop(GameObject prefab)
    {
        if (_pools.ContainsKey(prefab.name) == false)
            CreatePool(prefab);

        return _pools[prefab.name].Pop();
    }

    public bool Push(GameObject go)
    {
        if (_pools.ContainsKey(go.name) == false)
            return false;

        _pools[go.name].Push(go);
        return true;
    }

    public void Clear()
    {
        _pools.Clear();
    }

    private void CreatePool(GameObject original)
    {
        Pool pool = new Pool(original);
        _pools.Add(original.name, pool);
    }
}
