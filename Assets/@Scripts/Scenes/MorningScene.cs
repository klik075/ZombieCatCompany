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

        SaveManager.Instance.LoadMorningSceneData();

        if (!SoundManager.Instance.IsCurrentBgm("Night_BGM"))
        {
            SoundManager.Instance.Play2D(ESound.Bgm, "Night_BGM");
        }
    }
    void OnApplicationQuit()
    {

    }
}
