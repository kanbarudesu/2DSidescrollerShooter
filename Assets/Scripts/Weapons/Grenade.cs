using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

public class Grenade : MonoBehaviour
{
    [Header("Grenade Settings")]
    [SerializeField] private float explosionDelay = 2f;
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private float damage = 20f;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private LayerMask hitMask;

    [Header("Effects")]
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private UnityEvent onExplode;

    private void Start()
    {
        Invoke(nameof(Explode), explosionDelay);
    }

    private void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, hitMask);
        foreach (Collider2D hit in hits)
        {
            Health health = hit.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }

            Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dir = (rb.transform.position - transform.position).normalized;
                rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
            }
        }

        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        onExplode.Invoke();
        Addressables.ReleaseInstance(gameObject);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
