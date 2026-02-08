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
        GameManager.Instance.GameState = EGameState.Morning;
        SaveManager.Instance.Load();
    }
    private void Start()
    {
      
    }
    void OnApplicationQuit()
    {

    }
}
