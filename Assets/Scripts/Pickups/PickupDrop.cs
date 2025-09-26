using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PickupDrop : MonoBehaviour
{
    [SerializeField] private string dropConfigLabel = "DropConfigs";
    [SerializeField, Range(0f, 1f)] private float dropChance = 0.5f;

    private List<DropData> mergedDropData = new List<DropData>();
    private List<string> dlcLabels = new List<string>();

    private IEnumerator Start()
    {
        DiscoverDlcLabels();
        var handle = Addressables.LoadAssetsAsync<DropConfig>(dropConfigLabel, config =>
        {
            mergedDropData.AddRange(config.DropsData);
        });
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Failed)
        {
            Debug.LogError("Failed to load DropConfig!");
        }

        yield return LoadDlcConfigsRoutine();
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
                    if (obj is DropConfig config)
                        mergedDropData.AddRange(config.DropsData);
                });
                yield return handle;
                Debug.Log($"Loaded DropConfig DLC: {dlcLabel}");
            }
            else
            {
                Debug.Log($"DropConfig DLC : {dlcLabel} not cached.");
            }
            Addressables.Release(sizeHandle);
        }
    }

    public void DropPickup()
    {
        if (mergedDropData.Count == 0) return;

        if (Random.value > dropChance) return;

        float totalWeight = 0f;
        foreach (var data in mergedDropData)
            totalWeight += Mathf.Max(0f, data.dropChance);

        if (totalWeight <= 0f) return;

        float randomValue = Random.value * totalWeight;
        float cumulative = 0f;

        foreach (var data in mergedDropData)
        {
            cumulative += Mathf.Max(0f, data.dropChance);
            if (randomValue <= cumulative)
            {
                StartCoroutine(TryDropPickupRoutine(data.pickupPrefab));
                break;
            }
        }
    }

    private IEnumerator TryDropPickupRoutine(AssetReferenceGameObject pickupPrefab)
    {
        AsyncOperationHandle<GameObject> handle = pickupPrefab.InstantiateAsync(transform.position, Quaternion.identity);
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Failed)
        {
            Debug.LogError($"Failed to load pickup: {pickupPrefab.RuntimeKey}");
        }
    }
}

[System.Serializable]
public struct DropData
{
    public AssetReferenceGameObject pickupPrefab;
    [Range(0f, 1f)] public float dropChance;
}
