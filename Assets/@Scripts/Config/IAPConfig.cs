using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

[System.Serializable]
public class ProductData
{
    public string productId;
    public ProductType productType;
    // TODO
}

[CreateAssetMenu(fileName = "IAPConfig", menuName = "Config/IAPConfig")]
public class IAPConfig : ScriptableObject
{
    [Header("Product Definitions")]
    [SerializeField]
    private List<ProductData> products = new List<ProductData>
    {
        new ProductData { productId = "com.rookiss.s2.100gold", productType = ProductType.Consumable },
        new ProductData { productId = "com.rookiss.s2.500gold", productType = ProductType.Consumable },
        new ProductData { productId = "com.rookiss.s2.noads", productType = ProductType.NonConsumable }
    };

    public List<ProductDefinition> GetProductDefinitions()
    {
        var productDefinitions = new List<ProductDefinition>();
        foreach (var product in products)
        {
            productDefinitions.Add(new ProductDefinition(product.productId, product.productType));
        }
        return productDefinitions;
    }
}

