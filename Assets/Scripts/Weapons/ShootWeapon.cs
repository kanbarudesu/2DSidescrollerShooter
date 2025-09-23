using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ShootWeapon : WeaponBase
{
    [Header("Shoot Settings")]
    [SerializeField] private AssetReferenceGameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float bulletDamage = 10;

    protected override void PerformAttack(Transform playerTransform)
    {
        if (bulletPrefab == null || firePoint == null) return;
        StartCoroutine(TrySpawnBulletRoutine(playerTransform));
    }

    private IEnumerator TrySpawnBulletRoutine(Transform playerTransform)
    {
        AsyncOperationHandle<GameObject> handle = bulletPrefab.InstantiateAsync(firePoint.position, firePoint.rotation);
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Failed)
        {
            Debug.Log($"Failed to load bullet: {bulletPrefab.RuntimeKey}");
            yield break;
        }

        GameObject bullet = handle.Result;
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = new Vector2(playerTransform.localScale.x * bulletSpeed, 0f);

        Projectile projectile = bullet.GetComponent<Projectile>();
        if (projectile != null)
            projectile.SetDamage(bulletDamage);
    }
}