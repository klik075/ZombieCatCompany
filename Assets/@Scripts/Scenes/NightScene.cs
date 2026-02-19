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

        // MapManager 씬 초기화 (Tilemap 재설정)
        //MapManager.Instance.InitForScene();

        // GameData 로드 및 멤버 소환
        SaveManager.Instance.LoadNightSceneData();

        UIManager.Instance.ShowSceneUI<UI_NightGame>();
    }
    void OnApplicationQuit()
    {
        
    }
}

