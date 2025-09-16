using UnityEngine;

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
                Instantiate(data.pickupPrefab, transform.position, Quaternion.identity);
                break;
            }
        }
    }
}

[System.Serializable]
public struct DropData
{
    public GameObject pickupPrefab;
    [Range(0f, 1f)] public float dropChance;
}
