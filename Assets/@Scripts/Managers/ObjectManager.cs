using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

public class ObjectManager : Singleton<ObjectManager>
{
    #region Roots
    private Transform _memberRoot;
    public Transform MemberRoot
    {
        get { return Utils.GetRootTransform(ref _memberRoot, "@Members"); }
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
    private HashSet<Member> _members = new HashSet<Member>();
    private HashSet<Merchant> _merchants = new HashSet<Merchant>();
    private HashSet<NormalCat> _normalCats = new HashSet<NormalCat>(); // 추가

    public Member SpawnMember(string prefab = "Member", bool pooling = false)
    {
        GameObject go = null;
        if (pooling)
            go = PoolManager.Instance.Pop(prefab);
        else
            go = ResourceManager.Instance.Instantiate(prefab);

        go.name = prefab;
        go.transform.parent = MemberRoot;

        Member member = go.GetOrAddComponent<Member>();
        _objects.Add(member);
        _members.Add(member);

        member.Pooling = pooling;

        return member;
    }

    // 추가: 일반 고양이 소환
    public NormalCat SpawnNormalCat(string prefab = "NormalCat", bool pooling = false)
    {
        GameObject go = null;
        if (pooling)
            go = PoolManager.Instance.Pop(prefab);
        else
            go = ResourceManager.Instance.Instantiate(prefab);

        go.name = prefab;
        go.transform.parent = MonsterRoot;

        NormalCat normalCat = go.GetOrAddComponent<NormalCat>();
        _objects.Add(normalCat);
        _normalCats.Add(normalCat);

        normalCat.Pooling = pooling;

        return normalCat;
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

        if (obj is Member member)
            _members.Remove(member);

        if (obj is Merchant merchant)
            _merchants.Remove(merchant);

        if (obj is NormalCat normalCat) // 추가
            _normalCats.Remove(normalCat);

        if (obj.Pooling)
        {
            obj.Init();
            PoolManager.Instance.Push(obj.gameObject);
        }            
        else
            ResourceManager.Instance.Destroy(obj.gameObject);
    }
}
