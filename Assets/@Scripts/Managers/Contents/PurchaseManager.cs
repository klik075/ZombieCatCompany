using UnityEngine;
using static Define;    
public class PurchaseManager : Singleton<PurchaseManager>
{
    private void Awake()
    {
        EventManager.Instance.AddEvent(EEventType.GameStateChanged, StartPurchase);
    }
    private void StartPurchase()
    {
        if (GameManager.Instance.GameState != EGameState.FoodPurchase)
            return;

        Player mainCharacter = ObjectManager.Instance.SpawnPlayer("CatBlackZombie");
        mainCharacter.SetMemberData(MemberManager.MAIN_CHARACTER_ID);        

        //입구에 암상인 캐릭터 소환
        //사장 옆자리로 이동,
        //구매 UI 오픈
    }
}
