using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PickupDrop : MonoBehaviour
{
    [SerializeField] private DropData[] pickupPrefabs;
    [SerializeField, Range(0f, 1f)] private float dropChance = 0.5f;

    public void DropPickup()
    {
        if (pickupPrefabs == null || pickupPrefabs.Length == 0) return;

        if (Random.value > dropChance) return;

        float totalWeight = 0f;
        foreach (var data in pickupPrefabs)
            totalWeight += Mathf.Max(0f, data.dropChance);

        if (totalWeight <= 0f) return;

        float randomValue = Random.value * totalWeight;
        float cumulative = 0f;

        foreach (var data in pickupPrefabs)
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
