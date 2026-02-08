using System.Collections;
using UnityEngine;
using static Define;    
public class PurchaseManager : Singleton<PurchaseManager>
{
    Merchant _merchant;
    private void Awake()
    {
        EventManager.Instance.AddEvent(EEventType.MerchantStateChanged, OnChangedMerchantState);
    }
    private void OnChangedMerchantState()
    {
        if (_merchant == null) 
            return;

        switch (_merchant.MyMerchantState)
        {
            case Merchant.MerchantState.End:
                EndPurchase();
                break;
            default:
                break;
        }
    }
    public void StartPurchase()
    {
        if (GameManager.Instance.GameState != EGameState.FoodPurchase)
            return;

        _merchant = ObjectManager.Instance.SpawnMerchant("Merchant");
        Vector2Int spawnPos = MapManager.Instance.FindNearPosition(MemberManager.Instance.DoorWay);
        _merchant.transform.position = MapManager.Instance.CellToWorld(spawnPos);
        _merchant.CellPosition = spawnPos;
        MapManager.Instance.RegisterCat(_merchant, spawnPos);
        _merchant.MoveToBossNearPosition();
    }
    private void EndPurchase()
    {
        _merchant.MoveToDoorWay();
    }
    public void MemberLeave()
    {   
        Vector2Int door = MemberManager.Instance.DoorWay;
        foreach (Member member in MemberManager.Instance.GetAllMembers())
        {
            var doorMovement = MovementPoolManager.Instance.Get<SeatMovement>()
                .Initialize(door, member.GridManager);
            member.SetMovementStrategy(doorMovement);
        }
        CoroutineManager.Instance.StartCoroutine(CoWaitForMemberLeave());
    }
    IEnumerator CoWaitForMemberLeave()
    {
        WaitForSeconds wait = new WaitForSeconds(1f);
        yield return wait;

        while (!MemberManager.Instance.AreAllMembersDisabled())
        {
            yield return wait;
        }

        GameManager.Instance.GameState = EGameState.Morning;
    }
}
