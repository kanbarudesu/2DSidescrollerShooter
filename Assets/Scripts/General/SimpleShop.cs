using UnityEngine;
using UnityEngine.AddressableAssets;

public class SimpleShop : MonoBehaviour
{
    [SerializeField] private ShopItemData[] items;
    [SerializeField] private GameObject shopItemPrefab;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Transform content;
    [SerializeField] private WeaponHandler playerWeaponHandler;

    private void InitializeShopItems()
    {
        foreach (var item in items)
        {
            var shopItem = Instantiate(shopItemPrefab, content).GetComponent<ShopItem>();
            shopItem.SetItem(item, () => OnBuyItem(item));
        }
    }

    private void Start() => InitializeShopItems();

    public void OpenShop()
    {
        Time.timeScale = 0;
        shopPanel.SetActive(true);
    }

    public void CloseShop()
    {
        Time.timeScale = 1;
        shopPanel.SetActive(false);
    }

    private void OnBuyItem(ShopItemData data)
    {
        if (GameManager.Instance.CanSpendCoins(data.Price) == false) return;
        
        if (data.isAmmo)
            playerWeaponHandler.GetCurrentWeapon().AddAmmo(data.AmmoAmount);
        else
            playerWeaponHandler.EquipWeapon(data.Prefab);
    }

}

[System.Serializable]
public class ShopItemData
{
    public AssetReferenceGameObject Prefab;
    public Sprite Icon;
    public int Price;
    public string Description;
    public bool isAmmo;
    public int AmmoAmount;
}
