using UnityEngine;
using UnityEngine.AddressableAssets;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private LayerMask hitMask;

    private void Start()
    {
        Invoke(nameof(DelayDestroy), lifeTime);
    }

    private void DelayDestroy()
    {
        Addressables.ReleaseInstance(gameObject);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & hitMask) != 0)
        {
            Health targetHealth = collision.GetComponent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);
            }

            Addressables.ReleaseInstance(gameObject);
            Destroy(gameObject);
        }
    }

    public void SetDamage(float damage)
    {
        this.damage = damage;
    }
}
