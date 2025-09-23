using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GrenadeWeapon : WeaponBase
{
    [Header("Grenade Settings")]
    [SerializeField] private AssetReferenceGameObject grenadePrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float throwForce = 7f;

    protected override void PerformAttack(Transform playerTransform)
    {
        if (grenadePrefab == null || throwPoint == null) return;
        StartCoroutine(TrySpawnGrenadeRoutine(playerTransform));
    }

    private IEnumerator TrySpawnGrenadeRoutine(Transform playerTransform)
    {
        AsyncOperationHandle<GameObject> handle = grenadePrefab.InstantiateAsync(throwPoint.position, throwPoint.rotation);
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Failed)
        {
            Debug.Log($"Failed to load grenade prefab: {grenadePrefab.RuntimeKey}");
            yield break;
        }

        GameObject grenade = handle.Result;
        Rigidbody2D rb = grenade.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForceX(playerTransform.localScale.x * throwForce, ForceMode2D.Impulse);
        }

        if (grenade.TryGetComponent(out Rigidbody2D grenadeRb))
        {
            grenadeRb.AddTorque(Random.Range(15f, 30f) * playerTransform.localScale.x, ForceMode2D.Impulse);
        }
    }
}