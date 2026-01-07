using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameData
{
    public int Gold;
    public int Level;
    public List<int> UpgradeCount;
}

public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    private GameData _gameData = new GameData();
    public GameData GameData
    {
        get { return _gameData; }
        set
        {
            _gameData = value;
        }
    }

    public int Gold
    {
        get { return _gameData.Gold; }
        set
        {
            _gameData.Gold = value;
            EventManager.Instance.TriggerEvent(Define.EEventType.GoldChanged);
        }
    }
}
