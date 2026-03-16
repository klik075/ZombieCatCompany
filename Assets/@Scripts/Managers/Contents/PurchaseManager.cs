using System.Collections;
using UnityEngine;
using static Define;
    
/// <summary>
/// 구매 시스템 총괄 (Orchestrator)
/// </summary>
public class PurchaseManager : Singleton<PurchaseManager>
{
    private Merchant _merchant;
    private IPurchaseFlow _currentFlow;
    private MemberExitController _exitController;
    
    public void StartPurchase()
    {
        if (GameManager.Instance.GameState != EGameState.FoodPurchase)
            return;
        
        _merchant = SpawnMerchant();
        
        _currentFlow = new PurchaseFlowController(_merchant);
        _currentFlow.Start(onComplete: OnPurchaseFlowCompleted);
    }
    
    private Merchant SpawnMerchant()
    {
        Merchant merchant = ObjectManager.Instance.SpawnMerchant("Merchant");
        Vector2Int spawnPos = MapManager.Instance.FindNearPosition(MemberManager.Instance.DoorWay);
        merchant.transform.position = MapManager.Instance.CellToWorld(spawnPos);
        merchant.CellPosition = spawnPos;
        MapManager.Instance.RegisterCat(merchant, spawnPos);
        
        return merchant;
    }
    
    /// <summary>
    /// 구매 프로세스 완료
    /// </summary>
    private void OnPurchaseFlowCompleted()
    {
        // Merchant 제거
        ObjectManager.Instance.Despawn(_merchant);
        _merchant = null;
        _currentFlow = null;

        ExitAllMembersAndTransition();
    }
    public void ExitAllMembersAndTransition()
    {
        // Member 퇴장 관리자 생성
        _exitController = new MemberExitController(
            MemberManager.Instance.GetAllMembers(),
            MemberManager.Instance.DoorWay
        );

        // 퇴장 시작 (모두 퇴장하면 OnAllMembersExited 호출)
        _exitController.StartExit(onAllMembersExited: OnAllMembersExited);
    }
    
    /// <summary>
    /// 모든 Member 퇴장 완료
    /// </summary>
    private void OnAllMembersExited()
    {
        Debug.Log("All members exited. Transitioning to Morning.");
        
        _exitController.Cancel();
        _exitController = null;
        
        // 아침으로 전환
        GameManager.Instance.GameState = EGameState.Morning;
    }
}
