using System;
using System.Collections.Generic;
using UnityEngine;

public interface IResourceLoader
{
    void LoadAll(Action<float> onProgress = null, Action onComplete = null);
    T Get<T>(string key) where T : UnityEngine.Object;
    GameObject Instantiate(string key, Transform parent = null);
    void Destroy(GameObject go);
    void ReleaseAll();
}

public class ResourceManager : Singleton<ResourceManager>
{
    IResourceLoader _loader = new ResourcesLoader();

    public void LoadAll(Action<float> onProgress = null, Action onComplete = null)
    {
        _loader.LoadAll(onProgress, onComplete);
	}

    public T Get<T>(string key) where T : UnityEngine.Object
    {
        return _loader.Get<T>(key);
    }

    public GameObject Instantiate(string key, Transform parent = null)
    {
        return _loader.Instantiate(key, parent);
    }

    public void Destroy(GameObject go)
    {
        _loader.Destroy(go);
    }

    public void ReleaseAll()
    {
        _loader.ReleaseAll();
	}
}

public class ResourcesLoader : IResourceLoader
{
    private Dictionary<string, UnityEngine.Object> _resources = new Dictionary<string, UnityEngine.Object>();

	public void LoadAll(Action<float> onProgress = null, Action onComplete = null)
	{
        List<string> paths = new List<string> { "PreLoad" };
        int totalPaths = paths.Count;
        int loadedPaths = 0;

        foreach (string path in paths)
        {
			UnityEngine.Object[] resources = Resources.LoadAll(path);
            foreach (UnityEngine.Object resource in resources)
            {
                string fullKey = $"{resource.name}_{resource.GetType().Name}";
                if (_resources.ContainsKey(fullKey) == false)
                    _resources.Add(fullKey, resource);
            }

			loadedPaths++;
			float progress = (float)loadedPaths / totalPaths;
			onProgress?.Invoke(progress);

            if (loadedPaths >= totalPaths)
                onComplete?.Invoke();
		}
	}

	public T Get<T>(string key) where T : UnityEngine.Object
    {
		string fullKey = $"{key}_{typeof(T).Name}";

        if (_resources.TryGetValue(fullKey, out UnityEngine.Object resource))
            return resource as T;
            
        return null;
	}

    public GameObject Instantiate(string key, Transform parent = null)
    {
        GameObject prefab = Get<GameObject>(key);
        if (prefab == null)
            return null;

        GameObject instance = UnityEngine.Object.Instantiate(prefab, parent);
        instance.name = prefab.name;
        return instance;
	}

    public void Destroy(GameObject go)
    {
        if (go == null)
            return;
  
        UnityEngine.Object.Destroy(go);
    }

    public void ReleaseAll()
    {
        foreach (UnityEngine.Object resource in _resources.Values)
            Resources.UnloadAsset(resource);

        _resources.Clear();
        Resources.UnloadUnusedAssets();
    }
}