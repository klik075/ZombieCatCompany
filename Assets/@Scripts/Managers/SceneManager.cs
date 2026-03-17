using UnityEngine;

public class SceneManager : Singleton<SceneManager>
{
    private BaseScene _currentScene;
    public BaseScene CurrentScene
    {
        get
        {
            if (_currentScene == null)
                _currentScene = FindFirstObjectByType<BaseScene>();

			return _currentScene;
        }
    }

    public Define.EScene CurrentSceneType
    {
        get
        {
            if (CurrentScene == null)
                return Define.EScene.Unknown;

            return CurrentScene.SceneType;
        }
	}

    public void LoadScene(Define.EScene sceneType)
    {
        UIManager.Instance.CloseAllPopupUI();

        string sceneName = sceneType.ToString();
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        _currentScene = null;
	}
    public void LoadScene(Define.EScene sceneType, float fadeDuration)
    {
        CoroutineManager.Instance.StartCoroutine(CoLoadSceneWithFade(sceneType, fadeDuration));
    }
    private System.Collections.IEnumerator CoLoadSceneWithFade(Define.EScene sceneType, float fadeDuration)
    {
        // 1. Fade Panel 가져오기 (UIManager에서 관리)
        UI_FadePanel fadePanel = UIManager.Instance.GetFadePanel();

        // 2. 화면을 검게 덮음 (Fade In)
        bool fadeInComplete = false;
        fadePanel.FadeIn(fadeDuration, () => fadeInComplete = true);

        while (!fadeInComplete)
        {
            yield return null;
        }

        // 3. 팝업 모두 닫기 (화면이 검은 상태에서 닫아서 깜빡임 없음)
        UIManager.Instance.CloseAllPopupUI();

        PoolManager.Instance.ClearAllPools();
        // 4. 씬 비동기 로드
        string sceneName = sceneType.ToString();
        var asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);

        // 씬 로드가 완료될 때까지 대기
        while (!asyncOperation.isDone)
        {
            yield return null;
        }

        _currentScene = null;

        // 5. 새 씬이 로드된 후 한 프레임 대기
        yield return null;

        // 6. 화면을 밝게 함 (Fade Out)
        // DontDestroyOnLoad이므로 동일한 fadePanel 사용
        bool fadeOutComplete = false;
        fadePanel.FadeOut(fadeDuration, () => fadeOutComplete = true);

        while (!fadeOutComplete)
        {
            yield return null;
        }

        Debug.Log($"[SceneManager] Scene loaded: {sceneType}");
    }
}
