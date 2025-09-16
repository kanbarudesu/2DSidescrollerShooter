using UnityEngine;

public class ShootWeapon : WeaponBase
{
    [Header("Shoot Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float bulletDamage = 10;

    protected override void PerformAttack(Transform playerTransform)
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = new Vector2(playerTransform.localScale.x * bulletSpeed, 0f);

        Projectile projectile = bullet.GetComponent<Projectile>();
        if (projectile != null)
            projectile.SetDamage(bulletDamage);
    }
}