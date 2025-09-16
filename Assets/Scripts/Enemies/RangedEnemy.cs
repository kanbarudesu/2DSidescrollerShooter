using UnityEngine;

public class RangedEnemy : EnemyBase
{
    [Header("Ranged Settings")]
    [SerializeField] private float preferredDistance = 5f;
    [SerializeField] private float repositionInterval = 3f;
    [SerializeField] private float arriveThreshold = 0.2f;
    [SerializeField] private float shootInterval = 1f;
    [SerializeField] private GameObject[] weaponPrefabs;
    [SerializeField] private Transform weaponHolder;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(1f, 0.5f);
    [SerializeField] private int maxPositionAttempts = 8;

    private WeaponBase weapon;
    private Vector2 targetPosition;
    private float repositionTimer;
    private float shootTimer;
    private bool isMoving;

    protected override void Awake()
    {
        base.Awake();
        TryChooseNewPosition();
        InitializeWeapon();
        shootTimer = shootInterval;
    }

    private void InitializeWeapon()
    {
        if (weaponPrefabs == null || weaponHolder == null) return;
        var randomWeapon = weaponPrefabs[Random.Range(0, weaponPrefabs.Length)];
        weapon = Instantiate(randomWeapon, weaponHolder.position, weaponHolder.rotation, weaponHolder).GetComponent<WeaponBase>();
    }

    protected override void HandleMovement()
    {
        if (player == null) return;

        if (!isMoving)
            repositionTimer -= Time.deltaTime;

        if (isMoving)
        {
            Vector2 pos = transform.position;
            float dist = Vector2.Distance(pos, targetPosition);

            if (dist > arriveThreshold)
            {
                Vector2 dir = (targetPosition - pos).normalized;
                rb.linearVelocity = dir * moveSpeed;
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
                isMoving = false;
                shootTimer = shootInterval;
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;

            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0f)
            {
                Shoot();
                shootTimer = shootInterval;
            }

            if (repositionTimer <= 0f)
            {
                TryChooseNewPosition();
            }
        }

        FacePlayer();
    }

    private void TryChooseNewPosition()
    {
        if (player == null) return;

        for (int i = 0; i < maxPositionAttempts; i++)
        {
            Vector2 pos = Vector2.Distance(transform.position, player.position) < preferredDistance ? transform.position : (Vector2)player.position;
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            Vector2 candidate = pos + randomDir * preferredDistance;

            if (IsOnGround(candidate))
            {
                targetPosition = candidate;
                repositionTimer = repositionInterval;
                isMoving = true;
                return;
            }
        }

        targetPosition = transform.position;
        repositionTimer = repositionInterval;
        isMoving = false;
    }

    private bool IsOnGround(Vector2 position)
    {
        Collider2D hit = Physics2D.OverlapBox(position, groundCheckSize, 0f, groundLayer);
        return hit != null;
    }

    private void Shoot()
    {
        if (weaponPrefabs == null) return;
        weapon.Fire(transform);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(targetPosition, groundCheckSize);
    }
#endif
}
