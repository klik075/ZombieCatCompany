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

		// 구성원 시스템 초기화 (주인공 생성)
		MemberManager.Instance.InitializeMainCharacter();
		
		// 테스트: 구성원 2명 추가 고용
		int memberIndex;
		if (MemberManager.Instance.HireMember(out memberIndex))
		{
			Debug.Log($"Hired member at index {memberIndex}");
		}
		if (MemberManager.Instance.HireMember(out memberIndex))
		{
			Debug.Log($"Hired member at index {memberIndex}");
		}
    }
}

