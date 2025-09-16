using UnityEngine;

public class Shotgun : WeaponBase
{
    [Header("Shotgun Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private int bulletCount = 6;
    [SerializeField] private float spreadAngle = 15f;
    [SerializeField] private float bulletSpeed = 15f;
    [SerializeField] private float bulletDamage = 8f;

    protected override void PerformAttack(Transform playerTransform)
    {
        if (bulletPrefab == null || firePoint == null) return;

        float angleStep = bulletCount > 1 ? spreadAngle / (bulletCount - 1) : 0f;
        float startAngle = -spreadAngle / 2f;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.right * playerTransform.localScale.x;

            GameObject projectileObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Rigidbody2D rb = projectileObj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = dir.normalized * bulletSpeed;
            }

            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.SetDamage(bulletDamage);
            }
        }
    }
}
