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
        //IAPManager.Instance.Init();

        SaveManager.Instance.Load();
        UIManager.Instance.ShowSceneUI<UI_NightGame>();
	}
    void OnApplicationQuit()
    {
        
    }
}

