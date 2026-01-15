using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScene : BaseScene
{
    protected override void Awake()
    {
        base.Awake();

        SceneType = Define.EScene.LoadingScene;

        ResourceManager.Instance.LoadAll(OnProgress, OnComplete);		
	} 

    void OnProgress(float value)
    {
		Debug.Log($"Loading Progress: {value * 100}%");
	}

    void OnComplete()
    {
		Debug.Log($"Loading Complete");
        DataManager.Instance.LoadData();
        SceneManager.Instance.LoadScene(Define.EScene.DevScene);
	}
}
