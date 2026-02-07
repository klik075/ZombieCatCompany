using System;
using System.Collections.Generic;
using UnityEngine;
using static Define;
using static Merchant;

public class Merchant : MovableCat
{
    public enum MerchantState
    {
        Start,
        End,
    }
    private Vector2Int _doorWay;          // 등장,퇴장 위치
    private Vector2Int _bossPosition; // 보스 위치
    private Vector2Int _targetNull;
    private bool _isTalked = false;
    private MerchantState _merchantState;
    public MerchantState MyMerchantState
    {
        get 
        { 
            return _merchantState; 
        }
        set
        {
            _merchantState = value;
            EventManager.Instance.TriggerEvent(EEventType.MerchantStateChanged);
        }
    }

    public string MerchantName = "암상인";
    public override void Init()
    {
        base.Init();

        _doorWay = MemberManager.Instance.DoorWay;
        _bossPosition = MemberManager.Instance.GetMemberSeat(0);
        _isTalked = false;
        _merchantState = MerchantState.Start;
        _targetNull = new Vector2Int(int.MinValue, int.MinValue);
        _aiTargetPosition = _targetNull;
    }

    protected override void OnAIIdle()
    {
        if (_aiTargetPosition != _targetNull)
        {
            if (CellPosition != _aiTargetPosition)
                return;

            _aiTargetPosition = _targetNull;

            if (_isTalked == false)
            {
                Hello();
            }
            else
            {
                PurchaseManager.Instance.MemberLeave();
                ObjectManager.Instance.Despawn(this);
            }
        }
    }
    public void MoveToBossNearPosition()
    {
        MoveToPosition(MapManager.Instance.FindNearPosition(_bossPosition));
    }
    public void MoveToDoorWay()
    {
        MoveToPosition(MapManager.Instance.FindNearPosition(_doorWay));
    }
    private void Hello()
    {
        _isTalked = true;

        UI_ChatPopup chatPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
        chatPopup.SetInfo(MemberManager.MAIN_CHARACTER_ID, MessageManager.Instance.GetMessageScript(EMessageType.MerchantHello).Contents, action: OnClickChatPopup);
    }
    private void Bye()
    {
        UI_ChatPopup chatPopup = UIManager.Instance.ShowPopupUI<UI_ChatPopup>();
        chatPopup.SetInfo(MemberManager.MAIN_CHARACTER_ID, MessageManager.Instance.GetMessageScript(EMessageType.MerchantBye).Contents, action: SwapState);
    }
    private void OnClickChatPopup()
    {
        if (_merchantState == MerchantState.Start)
        {
            UI_MessagePopup messagePopup = UIManager.Instance.ShowPopupUI<UI_MessagePopup>();
            messagePopup.SetInfo(MessageManager.Instance.GetMessageScript(EMessageType.PurchaseMessage).Contents, okAction: OnClickPurchaseOkButton, noAction: OnClickPurchaseNoButton);
        }
        else
        {

        }
    }
    private void OnClickPurchaseOkButton()
    {
        UI_PurchaseFoodPopup purchaseFoodPopup = UIManager.Instance.ShowPopupUI<UI_PurchaseFoodPopup>();
        purchaseFoodPopup.SetInfo();
    }
    private void OnClickPurchaseNoButton()
    {
        Bye();
    }
    private void SwapState()
    {
        MyMerchantState = _merchantState == MerchantState.Start ? MerchantState.End : MerchantState.Start;
    }
}
