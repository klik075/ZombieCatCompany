using UnityEngine;
using static Define;

public class Merchant : Cat
{
    public enum MerchantState
    {
        Start,
        End,
    }
    private Vector2Int _doorWay;          // 등장,퇴장 위치
    private Vector2Int _bossNearPosition; // 보스 근처 위치
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
    
    protected override void Awake()
    {
        base.Awake();
        
        _doorWay = MemberManager.Instance.DoorWay;
        _bossNearPosition = MapManager.Instance.FindNearPosition(MemberManager.Instance.GetMemberSeat(0));
        _isTalked = false;
        _merchantState = MerchantState.Start;
        _targetNull = new Vector2Int(int.MinValue, int.MinValue);
    }

    protected override void Start()
    {
        base.Start();
        
        _mover.OnMoveCompleted += OnMoveCompleted;
    }
    
    public override void Update()
    {
        base.Update();
        
        // Merchant는 AI가 필요 없으므로 Update에서 특별한 처리 없음
    }
    
    private void OnMoveCompleted()
    {
        // 목표 위치에 도착했는지 확인
        if (CellPosition == _bossNearPosition)
        {
            if (!_isTalked)
            {
                Hello();
            }
        }
        else if (CellPosition == _doorWay)
        {
            if (_isTalked)
            {
                PurchaseManager.Instance.MemberLeave();
                ObjectManager.Instance.Despawn(this);
            }
        }
    }
    
    public void MoveToBossNearPosition()
    {
        var movement = MovementPoolManager.Instance.Get<PathMovement>()
            .Initialize(_gridManager.FindPath(CellPosition, _bossNearPosition), _gridManager);
        SetMovementStrategy(movement);
    }

    public void MoveToDoorWay()
    {
        var movement = MovementPoolManager.Instance.Get<PathMovement>()
            .Initialize(_gridManager.FindPath(CellPosition, _doorWay), _gridManager);
        SetMovementStrategy(movement);
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

