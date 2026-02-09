using UnityEngine;
using static Define;
public class MorningScene : BaseScene
{
    protected override void Awake()
    {
        base.Awake();

        SceneType = EScene.MorningScene;
        ResourceManager.Instance.LoadAll();
        DataManager.Instance.LoadData();

        UIManager.Instance.ShowSceneUI<UI_MorningGame>();
    }
    private void Start()
    {
        GameManager.Instance.GameState = EGameState.Morning;
        SaveManager.Instance.Load();
    }
    void OnApplicationQuit()
    {

    }
}
