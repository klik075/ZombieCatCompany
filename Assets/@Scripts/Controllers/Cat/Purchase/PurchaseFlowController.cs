using System;
using UnityEngine;
using static Define;

/// <summary>
/// 구매 프로세스 흐름 제어 (UI 관리 책임)
/// </summary>
public class PurchaseFlowController : IPurchaseFlow
{
    private enum FlowState
    {
        WaitingForArrival,
        Greeting,
        OfferingPurchase,
        Purchasing,
        Farewell,
        Completed
    }

    private FlowState _currentState;
    private Merchant _merchant;
    private Action _onComplete;

    public PurchaseFlowController(Merchant merchant)
    {
        _merchant = merchant;
    }

    public void Start(Action onComplete)
    {
        _onComplete = onComplete;
        _currentState = FlowState.WaitingForArrival;

        // Merchant 이벤트 구독 (느슨한 결합)
        _merchant.OnArrivedAtBoss += OnMerchantArrived;
        _merchant.OnArrivedAtDoor += OnMerchantLeft;

        // Merchant에게 이동만 지시
        _merchant.MoveToBossNearPosition();
    }

    public void Cancel()
    {
        _merchant.OnArrivedAtBoss -= OnMerchantArrived;
        _merchant.OnArrivedAtDoor -= OnMerchantLeft;
    }

    /// <summary>
    /// Merchant가 보스 근처 도착 시
    /// </summary>
    private void OnMerchantArrived()
    {
        _currentState = FlowState.Greeting;
        ShowGreeting();
    }

    /// <summary>
    /// 1단계: 인사 팝업
    /// </summary>
    private void ShowGreeting()
    {
        UI_ChatPopup chatPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
        chatPopup.SetInfo(
            MemberManager.MAIN_CHARACTER_ID,
            MessageManager.Instance.GetMessageScript(EMessageType.MerchantHello).Contents,
            action: OnGreetingCompleted
        );
    }

    private void OnGreetingCompleted()
    {
        _currentState = FlowState.OfferingPurchase;
        ShowPurchaseOffer();
    }

    /// <summary>
    /// 2단계: 구매 제안
    /// </summary>
    private void ShowPurchaseOffer()
    {
        UI_MessagePopup messagePopup = UIManager.Instance.ShowPopupUI<UI_MessagePopup>();
        messagePopup.SetInfo(
            MessageManager.Instance.GetMessageScript(EMessageType.PurchaseMessage).Contents,
            okAction: OnPurchaseAccepted,
            noAction: OnPurchaseDeclined
        );
    }

    private void OnPurchaseAccepted()
    {
        _currentState = FlowState.Purchasing;
        ShowPurchasePopup();
    }

    private void OnPurchaseDeclined()
    {
        _currentState = FlowState.Farewell;
        ShowFarewell();
    }

    /// <summary>
    /// 3단계: 구매 팝업
    /// </summary>
    private void ShowPurchasePopup()
    {
        UI_PurchaseFoodPopup purchasePopup = UIManager.Instance.ShowPopupUI<UI_PurchaseFoodPopup>();
        purchasePopup.SetInfo();

        purchasePopup.OnClosed(OnPurchaseCompleted);
    }

    private void OnPurchaseCompleted()
    {
        _currentState = FlowState.Farewell;
        ShowFarewell();
    }

    /// <summary>
    /// 4단계: 작별 인사
    /// </summary>
    private void ShowFarewell()
    {
        UI_ChatPopup chatPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
        chatPopup.SetInfo(
            MemberManager.MAIN_CHARACTER_ID,
            MessageManager.Instance.GetMessageScript(EMessageType.MerchantBye).Contents,
            action: OnFarewellCompleted
        );
    }

    private void OnFarewellCompleted()
    {
        _currentState = FlowState.Completed;
        _merchant.MoveToDoorWay();
    }

    /// <summary>
    /// Merchant가 문에 도착하여 퇴장
    /// </summary>
    private void OnMerchantLeft()
    {
        Cancel(); // 이벤트 구독 해제
        _onComplete?.Invoke();
    }
}
