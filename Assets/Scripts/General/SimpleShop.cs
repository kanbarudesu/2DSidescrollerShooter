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
    private List<string> dlcLabels = new List<string>();

    private IEnumerator Start()
    {
        DiscoverDlcLabels();
        var handle = Addressables.LoadAssetsAsync<ShopConfig>(shopConfigLabel, config =>
        {
            mergedShopItems.AddRange(config.Items);
        });
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Failed)
        {
            Debug.LogError("Failed to load ShopConfig!");
        }

        yield return LoadDlcConfigsRoutine();
        yield return null;
        InitializeShopItems();
    }

    private void DiscoverDlcLabels()
    {
        foreach (var locator in Addressables.ResourceLocators)
        {
            foreach (var key in locator.Keys)
            {
                string keyStr = key.ToString();
                if (keyStr.StartsWith("DLC"))
                {
                    dlcLabels.Add(keyStr);
                }
            }
        }
    }

    private IEnumerator LoadDlcConfigsRoutine()
    {
        foreach (var dlcLabel in dlcLabels)
        {
            var sizeHandle = Addressables.GetDownloadSizeAsync(dlcLabel);
            yield return sizeHandle;

            if (sizeHandle.Status == AsyncOperationStatus.Succeeded && sizeHandle.Result == 0)
            {
                var handle = Addressables.LoadAssetsAsync<object>(dlcLabel, obj =>
                {
                    if (obj is ShopConfig config)
                        mergedShopItems.AddRange(config.Items);
                });
                yield return handle;
                Debug.Log($"Loaded ShopConfig DLC: {dlcLabel}");
            }
            else
            {
                Debug.Log($"ShopConfig DLC : {dlcLabel} not cached.");
            }
            Addressables.Release(sizeHandle);
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
