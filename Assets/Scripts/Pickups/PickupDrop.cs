using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PickupDrop : MonoBehaviour
{
    [SerializeField] private string dropConfigLabel = "DropConfigs";
    [SerializeField, Range(0f, 1f)] private float dropChance = 0.5f;

    private readonly List<DropConfig> allConfigs = new List<DropConfig>();
    private List<DropData> mergedDropData = new List<DropData>();

    private IEnumerator Start()
    {
        var handle = Addressables.LoadAssetsAsync<DropConfig>(dropConfigLabel, config =>
        {
            allConfigs.Add(config);
        });
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            foreach (var config in allConfigs)
                mergedDropData.AddRange(config.DropsData);
        }
        else
        {
            Debug.LogError("Failed to load DropConfig!");
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
