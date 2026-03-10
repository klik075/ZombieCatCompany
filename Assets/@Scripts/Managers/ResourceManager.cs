using System;
using System.Collections.Generic;
using UnityEngine;

public interface IResourceLoader
{
    void LoadAll(Action<float> onProgress = null, Action onComplete = null);
    void LoadResourcesFromPath(string path);
    T Get<T>(string key) where T : UnityEngine.Object;
    T[] GetAllFromPath<T>(string path) where T : UnityEngine.Object;
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
    public void LoadResourcesFromPath(string path)
    {
        _loader.LoadResourcesFromPath(path);
    }

    public T Get<T>(string key) where T : UnityEngine.Object
    {
        return _loader.Get<T>(key);
    }
    public T[] GetAllFromPath<T>(string path) where T : UnityEngine.Object
    {
        return _loader.GetAllFromPath<T>(path);
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
    private Dictionary<string, UnityEngine.Object> _resources = new Dictionary<string, UnityEngine.Object>();//개별 리소스
    private Dictionary<string, UnityEngine.Object[]> _pathResources = new Dictionary<string, UnityEngine.Object[]>();//경로별 리소스
    private HashSet<string> _loadedPaths = new HashSet<string>();

    public void LoadAll(Action<float> onProgress = null, Action onComplete = null)
	{
        List<string> paths = new List<string> { "PreLoad", "Cat", "QualityImage" };
        int totalPaths = paths.Count;
        int loadedPaths = 0;

        foreach (string path in paths)
        {
            LoadResourcesFromPath(path);

            loadedPaths++;
			float progress = (float)loadedPaths / totalPaths;
			onProgress?.Invoke(progress);

            if (loadedPaths >= totalPaths)
                onComplete?.Invoke();
		}
	}
    public void LoadResourcesFromPath(string path)
    {
        if (_loadedPaths.Contains(path))
        {
            Debug.Log($"[ResourcesLoader] Path already loaded: {path}");
            return;
        }

        UnityEngine.Object[] resources = Resources.LoadAll(path);
        foreach (UnityEngine.Object resource in resources)
        {
            string fullKey = $"{resource.name}_{resource.GetType().Name}";
            if (_resources.ContainsKey(fullKey) == false)
                _resources.Add(fullKey, resource);
        }

        _pathResources[path] = resources;
        _loadedPaths.Add(path);
    }
    public T[] GetAllFromPath<T>(string path) where T : UnityEngine.Object
    {
        // 이미 로드된 경로인지 확인
        if (_pathResources.TryGetValue(path, out UnityEngine.Object[] cachedResources))
        {
            Debug.Log($"[ResourcesLoader] Returning {cachedResources.Length} cached resources from path: {path}");

            // T 타입만 필터링하여 반환
            List<T> result = new List<T>();
            foreach (var resource in cachedResources)
            {
                if (resource is T typedResource)
                    result.Add(typedResource);
            }

            return result.ToArray();
        }

        // 새로 로드
        LoadResourcesFromPath(path);

        // 재귀 호출로 캐시에서 반환 (무한 루프 방지를 위해 다시 확인)
        if (_pathResources.TryGetValue(path, out UnityEngine.Object[] loadedResources))
        {
            List<T> result = new List<T>();
            foreach (var resource in loadedResources)
            {
                if (resource is T typedResource)
                    result.Add(typedResource);
            }

            return result.ToArray();
        }

        Debug.LogWarning($"[ResourcesLoader] No resources found at path: {path}");
        return new T[0];
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
