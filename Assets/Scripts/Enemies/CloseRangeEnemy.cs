using System;
using UnityEngine;

public class CloseRangeEnemy : EnemyBase
{
    [Header("Close Range Settings")]
    [SerializeField] private float stopDistance = 0.5f;
    [SerializeField] private float damage = 1f;
    [SerializeField] private float damageInterval = 1f;

    private float damageTimer;
    private bool playerInContact;
    private Health playerHealth;

    private void Start()
    {
        damageTimer = damageInterval;
    }

    protected override void Update()
    {
        base.Update();
        HandleDamage();
    }

    private void HandleDamage()
    {
        if (playerInContact)
        {
            damageTimer -= Time.deltaTime;
            if (damageTimer <= 0f)
            {
                DealDamage();
                damageTimer = damageInterval;
            }
        }
    }

    protected override void HandleMovement()
    {
        if (player == null) return;
        
        if (Vector2.Distance(transform.position, player.position) > stopDistance)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            rb.linearVelocity = dir * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }

        FacePlayer();
    }

    private void DealDamage()
    {
        if (playerHealth == null) return;
        playerHealth.TakeDamage(damage);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        if (playerHealth == null)
        {
            playerHealth = other.gameObject.GetComponent<Health>();
        }

        playerInContact = true;
        damageTimer = 0f;
        DealDamage();
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        playerInContact = false;
    }
}
