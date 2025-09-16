using UnityEngine;

public class FlamethrowerWeapon : WeaponBase
{
    [Header("Flamethrower Settings")]
    [SerializeField] private ParticleSystem flameParticles;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float ammoUsePerSecond = 3f;

    [Header("Damage Settings")]
    [SerializeField] private float damagePerSecond = 2f;
    [SerializeField] private float flameRadius = 3f;
    [SerializeField] private float coneAngle = 45f;
    [SerializeField] private LayerMask hitMask;

    private bool isFiring = false;
    private Transform playerTransform;

    private void Update()
    {
        if (isFiring && HasAmmo)
        {
            ConsumeAmmo(ammoUsePerSecond * Time.deltaTime);
            ApplyConeDamage();

            if (CanFire())
            {
                nextFireTime = Time.time + fireRate;
                onFire.Invoke();
            }
        }
        else if (flameParticles.isPlaying)
        {
            flameParticles.Stop();
        }

        if (!HasAmmo && autoReload)
        {
            Reload();
        }
    }

    protected override void PerformAttack(Transform playerTransform) { }

    public override void Fire(Transform playerTransform)
    {
        this.playerTransform = playerTransform;
        isFiring = true;

        if (flameParticles != null && !flameParticles.isPlaying)
        {
            flameParticles.Play();
        }
    }

    public override void StopFire()
    {
        isFiring = false;

        if (flameParticles != null && flameParticles.isPlaying)
        {
            flameParticles.Stop();
        }
    }

    private void ApplyConeDamage()
    {
        if (flameParticles == null || firePoint == null) return;

        if (!flameParticles.isPlaying)
        {
            flameParticles.Play();
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(firePoint.position, flameRadius, hitMask);
        Vector2 forward = Vector2.right * playerTransform.localScale.x;

        foreach (var hit in hits)
        {
            Vector2 toTarget = ((Vector2)hit.transform.position - (Vector2)firePoint.position).normalized;
            float angle = Vector2.Angle(forward, toTarget);

            Debug.DrawLine(firePoint.position, hit.transform.position, Color.red, 0.05f);

            if (angle <= coneAngle * 0.5f)
            {
                if (hit.TryGetComponent(out Health health))
                {
                    health.TakeDamage(damagePerSecond * Time.deltaTime);
                }
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (firePoint == null || playerTransform == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(firePoint.position, flameRadius);

        Vector3 forward = Vector3.right * playerTransform.localScale.x;
        float halfAngle = coneAngle * 0.5f;

        Quaternion leftRot = Quaternion.Euler(0, 0, -halfAngle * playerTransform.localScale.x);
        Quaternion rightRot = Quaternion.Euler(0, 0, halfAngle * playerTransform.localScale.x);

        Vector3 leftDir = leftRot * forward;
        Vector3 rightDir = rightRot * forward;

        Gizmos.DrawLine(firePoint.position, firePoint.position + leftDir * flameRadius);
        Gizmos.DrawLine(firePoint.position, firePoint.position + rightDir * flameRadius);
    }
#endif
}
