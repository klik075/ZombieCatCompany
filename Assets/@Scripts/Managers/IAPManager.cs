using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : Singleton<IAPManager>
{
    StoreController _storeController;
    Action _onPurchaseCallback;

    #region 아이템 구매
    public void Purchase(string productId, Action onPurchaseCallback)
    {
        Product product = _storeController.GetProducts().FirstOrDefault(product => product.definition.id == productId);

        if (product != null)
        {
            _onPurchaseCallback = onPurchaseCallback;
            _storeController.PurchaseProduct(product);
        }
        else
        {
            Debug.Log($"The product service has no product with the ID {productId}");
        }
    }

    public void RestorePurchases(Action<bool, string> onRestoreCallback)
    {
        _storeController.RestoreTransactions(onRestoreCallback);
    }
    #endregion

    public void Init()
    {
        // 초기화는 Start()에서 비동기로 처리.
    }

    async void Start()
    {
        try
        {
            await InitializeIAP();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    async Task InitializeIAP()
    {
        _storeController = UnityIAPServices.StoreController();

        // 모든 이벤트 핸들러 연결
        _storeController.OnStoreDisconnected += OnStoreDisconnected;
        _storeController.OnPurchasePending += OnPurchasePending;
        _storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;
        _storeController.OnPurchaseFailed += OnPurchaseFailed;
        _storeController.OnPurchaseDeferred += OnPurchaseDeferred;
        _storeController.OnCheckEntitlement += OnCheckEntitlement;
        _storeController.OnProductsFetched += OnProductsFetched;
        _storeController.OnProductsFetchFailed += OnProductsFetchedFailed;

        await _storeController.Connect();

        FetchProducts();
    }

    private void FetchProducts()
    {
        IAPConfig iapConfig = DataManager.Instance.IAPConfig;
        if (iapConfig == null)
        {
            Debug.LogError("IAPConfig is not loaded in DataManager!");
            return;
        }

        List<ProductDefinition> products = iapConfig.GetProductDefinitions();
        _storeController.FetchProducts(products);
    }

    #region Event Handlers
    private void OnStoreDisconnected(StoreConnectionFailureDescription obj)
    {
        Debug.LogWarning($"Store disconnected: {obj.Message}");
    }

    void OnPurchasePending(PendingOrder order)
    {
        _onPurchaseCallback?.Invoke();
        _storeController.ConfirmPurchase(order);
    }

    void OnPurchaseConfirmed(Order order)
    {
        switch (order)
        {
            case FailedOrder failedOrder:
                Debug.Log($"Purchase confirmation failed: {failedOrder.CartOrdered.Items().First().Product.definition.id}, {failedOrder.FailureReason.ToString()}, {failedOrder.Details}");
                break;
            case ConfirmedOrder:
                Debug.Log($"Purchase completed:  {order.CartOrdered.Items().First().Product.definition.id}");
                break;
        }
    }

    private void OnPurchaseFailed(FailedOrder order)
    {
        Debug.LogWarning($"Purchase failed: {order.CartOrdered.Items().First().Product.definition.id}, {order.FailureReason}, {order.Details}");
    }

    private void OnPurchaseDeferred(DeferredOrder order)
    {
        Debug.LogWarning($"Purchase deferred: {order.CartOrdered.Items().First().Product.definition.id}");
    }

    private void OnCheckEntitlement(Entitlement entitlement)
    {
        Product product = entitlement.Product;
        EntitlementStatus status = entitlement.Status;

        Debug.Log($"Entitlement checked: {product.definition.id}, Status: {status}");

        if (status == EntitlementStatus.FullyEntitled)
        {
            // TODO : 광고 제거 아이템이라면 관련 코드 추가.
        }
    }

    private void OnProductsFetched(List<Product> products)
    {
        foreach (Product product in products)
        {
            _storeController.CheckEntitlement(product);
        }
    }

    private void OnProductsFetchedFailed(ProductFetchFailed obj)
    {
        Debug.LogWarning($"Product fetch failed: {obj.FailureReason}");
    }
    #endregion
}