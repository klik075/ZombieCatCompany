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
        SaveManager.Instance.Load();

        UIManager.Instance.ShowSceneUI<UI_MorningGame>();
    }
    void OnApplicationQuit()
    {

    }
}
