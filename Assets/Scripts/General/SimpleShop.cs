using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SimpleShop : MonoBehaviour
{
    [SerializeField] private string shopConfigLabel = "ShopConfigs";
    [SerializeField] private GameObject shopItemPrefab;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Transform content;
    [SerializeField] private WeaponHandler playerWeaponHandler;

    private readonly List<ShopConfig> allConfigs = new List<ShopConfig>();
    private List<ShopItemData> mergedShopItems = new List<ShopItemData>();

    private IEnumerator Start()
    {
        var handle = Addressables.LoadAssetsAsync<ShopConfig>(shopConfigLabel, config =>
        {
            Debug.Log(config.Items.Length);
            allConfigs.Add(config);
        });
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            foreach (var config in allConfigs)
                mergedShopItems.AddRange(config.Items);

            yield return null;
            InitializeShopItems();
        }
        else
        {
            Debug.LogError("Failed to load ShopConfig!");
        }
    }

    private void InitializeShopItems()
    {
        foreach (var item in mergedShopItems)
        {
            var shopItem = Instantiate(shopItemPrefab, content).GetComponent<ShopItem>();
            shopItem.SetItem(item, () => OnBuyItem(item));
        }
    }

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
