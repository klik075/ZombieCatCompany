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
    private HashSet<Member> _players = new HashSet<Member>();
    private HashSet<Merchant> _merchants = new HashSet<Merchant>();

    public Member SpawnPlayer(string prefab = "Player", bool pooling = false)
    {
        GameObject go = null;
        if (pooling)
            go = PoolManager.Instance.Pop(prefab);
        else
            go = ResourceManager.Instance.Instantiate(prefab);

        go.name = prefab;
        go.transform.parent = PlayerRoot;

        Member player = go.GetOrAddComponent<Member>();
        _objects.Add(player);
        _players.Add(player);

        player.Pooling = pooling;

        return player;
    }
    public Merchant SpawnMerchant(string prefab = "Merchant", bool pooling = false)
    {
        GameObject go = null;
        if (pooling)
            go = PoolManager.Instance.Pop(prefab);
        else
            go = ResourceManager.Instance.Instantiate(prefab);

        go.name = prefab;
        go.transform.parent = NpcRoot;

        Merchant merchant = go.GetOrAddComponent<Merchant>();

        _objects.Add(merchant);
        _merchants.Add(merchant);

        merchant.Pooling = pooling;

        return merchant;
    }
    public Fence SpawnFence(string prefab = "Fence", bool pooling = false)
    {
        GameObject go = null;
        if (pooling)
            go = PoolManager.Instance.Pop(prefab);
        else
            go = ResourceManager.Instance.Instantiate(prefab);
        go.name = prefab;
        go.transform.parent = NpcRoot;

        Fence fence = go.GetOrAddComponent<Fence>();
        _objects.Add(fence);

        fence.Pooling = pooling;
        return fence;
    }

    public void Despawn(ObjectBase obj)
    {
        if (obj == null)
            return;

        _objects.Remove(obj);

        if (obj is Member player)
            _players.Remove(player);

        if (obj is Merchant merchant)
            _merchants.Remove(merchant);

        if (obj.Pooling)
        {
            obj.Init();
            PoolManager.Instance.Push(obj.gameObject);
        }            
        else
            ResourceManager.Instance.Destroy(obj.gameObject);
    }
}
