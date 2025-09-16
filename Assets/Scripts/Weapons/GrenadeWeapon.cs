using UnityEngine;

public class GrenadeWeapon : WeaponBase
{
    [Header("Grenade Settings")]
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float throwForce = 7f;

    protected override void PerformAttack(Transform playerTransform)
    {
        if (grenadePrefab == null || throwPoint == null) return;

        GameObject grenade = Instantiate(grenadePrefab, throwPoint.position, throwPoint.rotation);
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