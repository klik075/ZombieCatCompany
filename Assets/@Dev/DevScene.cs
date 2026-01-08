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

		//UIManager.Instance.ShowSceneUI<UI_GameHUD>();

		List<Vector2Int> walkableCells = MapManager.Instance.GetWalkableCells();
        for (int i = 0; i < 5; i++)
        {
            bool placed = false;
            int attempts = 0;
            while (!placed && attempts < 100 && walkableCells.Count > 0)
            {
                int randomIndex = Random.Range(0, walkableCells.Count);
                Vector2Int spawnPos = walkableCells[randomIndex];
                walkableCells.RemoveAt(randomIndex);

                Player cat = ObjectManager.Instance.SpawnPlayer("Cat");
                if (MapManager.Instance.MoveTo(cat, spawnPos, true))
                {
                    placed = true;
                }
                attempts++;
            }
        }
    }

}
