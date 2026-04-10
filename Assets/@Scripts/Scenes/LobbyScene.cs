using UnityEngine;

public class LobbyScene : BaseScene
{
    protected override void Awake()
    {
        base.Awake();

        SceneType = Define.EScene.LobbyScene;

        ResourceManager.Instance.LoadAll();
        DataManager.Instance.LoadData();

        SaveManager.Instance.LoadUserData();

        SoundManager.Instance.Play2D(Define.ESound.Bgm, "Lobby_BGM");
    }
    void OnApplicationQuit()
    {

    }
}
