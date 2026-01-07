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
        string sceneName = sceneType.ToString();
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        _currentScene = null;
	}

}
