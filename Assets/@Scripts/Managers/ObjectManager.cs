using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

public class ObjectManager : Singleton<ObjectManager>
{
    #region Roots
    private Transform _playerRoot;
    public Transform PlayerRoot
    {
        get { return Utils.GetRootTransform(ref _playerRoot, "@Players"); }
    }

    private Transform _monsterRoot;
    public Transform MonsterRoot
    {
        get { return Utils.GetRootTransform(ref _monsterRoot, "@Monsters"); }
    }

    private Transform _npcRoot;
    public Transform NpcRoot
    {
        get { return Utils.GetRootTransform(ref _npcRoot, "@Npcs"); }
    }
    #endregion

    private HashSet<ObjectBase> _objects = new HashSet<ObjectBase>();
    private HashSet<Player> _players = new HashSet<Player>();

    public Player SpawnPlayer(string prefab = "Player", bool pooling = false)
    {
        GameObject go = null;
        if (pooling)
            go = PoolManager.Instance.Pop(prefab);
        else
            go = ResourceManager.Instance.Instantiate(prefab);

        go.name = prefab;
        go.transform.parent = PlayerRoot;

        Player player = go.GetOrAddComponent<Player>();
        _objects.Add(player);
        _players.Add(player);

        player.Pooling = pooling;

        return player;
    }

    public void Despawn(ObjectBase obj)
    {
        if (obj == null)
            return;

        _objects.Remove(obj);

        if (obj is Player player)
            _players.Remove(player);

        if (obj.Pooling)
        {
            obj.Init();
            PoolManager.Instance.Push(obj.gameObject);
        }            
        else
            ResourceManager.Instance.Destroy(obj.gameObject);
    }
}
