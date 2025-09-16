using NUnit.Framework;
using UnityEngine;

public class LaserRifleWeapon : WeaponBase
{
    [Header("Laser Settings")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float laserLength = 10f;
    [SerializeField] private float damagePerSecond = 5f;
    [SerializeField] private float ammoUsePerSecond = 2f;
    [SerializeField] private LayerMask hitMask;

    private bool isFiring = false;
    private Transform playerTransform;

    private void Update()
    {
        if (isFiring && HasAmmo)
        {
            ConsumeAmmo(ammoUsePerSecond * Time.deltaTime);
            UpdateLaser();

            if (CanFire())
            {
                nextFireTime = Time.time + fireRate;
                onFire.Invoke();
            }
        }
        else if (lineRenderer.enabled == true)
        {
            lineRenderer.enabled = false;
        }

        if (!HasAmmo && autoReload)
        {
            Reload();
        }
    }

    private void UpdateLaser()
    {
        if (lineRenderer == null || firePoint == null) return;

        if (lineRenderer.enabled == false)
        {
            lineRenderer.enabled = true;
        }

        Vector3 direction = Vector3.right * playerTransform.localScale.x;
        RaycastHit2D[] hits = Physics2D.RaycastAll(firePoint.position, direction, laserLength, hitMask);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                if (hit.collider.TryGetComponent(out Health health))
                {
                    health.TakeDamage(damagePerSecond * Time.deltaTime);
                }
            }
        }

        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, Vector3.right * laserLength);
    }

    protected override void PerformAttack(Transform playerTransform) { }

    public override void Fire(Transform playerTransform)
    {
        if (!HasAmmo) return;

        this.playerTransform = playerTransform;
        isFiring = true;
    }

    public override void StopFire()
    {
        isFiring = false;

        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }
}
