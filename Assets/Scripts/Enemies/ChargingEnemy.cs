using System.Collections;
using UnityEngine;

public class ChargingEnemy : EnemyBase
{
    [Header("Charging Settings")]
    [SerializeField] private float chargeDelay = 1.5f;
    [SerializeField] private float chargeSpeed = 10f;
    [SerializeField] private float chargeDistance = 8f;
    [SerializeField] private float contactDamage = 20f;
    [SerializeField] private LineRenderer chargeIndicator;
    [SerializeField] private Health health;

    private bool isCharging;
    private Vector2 chargeDirection;
    private Vector2 startPosition;
    private Vector2 endPosition;

    protected void Start()
    {
        if (player == null) return;
        rb.linearVelocity = Vector2.zero;
        startPosition = transform.position;
        FacePlayer();
        chargeDirection = player.position.x > transform.position.x ? Vector2.right : Vector2.left;
        endPosition = startPosition + chargeDirection * chargeDistance;

        if (chargeIndicator != null)
        {
            chargeIndicator.enabled = true;
            chargeIndicator.positionCount = 2;
            StartCoroutine(DrawIndicatorRoutine());
        }
    }

    private IEnumerator DrawIndicatorRoutine()
    {
        float t = 0f;
        while (t < chargeDelay)
        {
            t += Time.deltaTime;
            float progress = Mathf.Clamp01(t / chargeDelay);
            Vector2 currentEnd = Vector2.Lerp(startPosition, endPosition, progress);
            chargeIndicator.SetPosition(0, startPosition);
            chargeIndicator.SetPosition(1, currentEnd);
            yield return null;
        }

        StartCharge();
    }

    private void StartCharge()
    {
        if (chargeIndicator != null)
            chargeIndicator.enabled = false;

        isCharging = true;
        rb.linearVelocity = chargeDirection * chargeSpeed;
    }

    protected override void HandleMovement()
    {
        if (!isCharging) return;

        float traveled = Vector2.Distance(startPosition, transform.position);
        if (traveled >= chargeDistance)
        {
            rb.linearVelocity = Vector2.zero;
            health.Die();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isCharging) return;

        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent(out Health playerHealth))
                playerHealth.TakeDamage(contactDamage);
        }
    }
}
