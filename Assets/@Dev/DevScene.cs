using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Random = UnityEngine.Random;
using System.Collections;


public class DevScene : BaseScene
{
	protected override void Awake()
	{
		base.Awake();

		SceneType = Define.EScene.DevScene;

        ResourceManager.Instance.LoadAll();
        DataManager.Instance.LoadData();
        //IAPManager.Instance.Init();

        SaveManager.Instance.Load();
	}
    void OnApplicationQuit()
    {
        
    }
}

