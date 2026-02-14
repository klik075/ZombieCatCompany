using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Random = UnityEngine.Random;
using System.Collections;


public class NightScene : BaseScene
{
	protected override void Awake()
	{
		base.Awake();

		SceneType = Define.EScene.NightScene;

        ResourceManager.Instance.LoadAll();
        DataManager.Instance.LoadData();

#if UNITY_EDITOR
        // UserData가 없으면 먼저 로드/생성
        if (GameManager.Instance.UserData == null)
        {
            SaveManager.Instance.LoadUserData();
        }

        // GameData가 없으면 에디터 테스트용으로 새 게임 시작
        if (!SaveManager.Instance.HasGameData())
        {
            Debug.Log("[Editor] NightScene: No GameData found, creating test GameData");
            SaveManager.Instance.NewGame();
        }
#endif

        // MapManager 씬 초기화 (Tilemap 재설정)
        MapManager.Instance.InitForScene();

        // GameData 로드 및 멤버 소환
        SaveManager.Instance.LoadGameData();

        UIManager.Instance.ShowSceneUI<UI_NightGame>();
    }
    void OnApplicationQuit()
    {
        
    }
}

