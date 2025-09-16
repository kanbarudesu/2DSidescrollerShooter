using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] protected float minMoveSpeed = 2f;
    [SerializeField] protected float maxMoveSpeed = 4f;

    protected Transform player;
    protected Rigidbody2D rb;
    protected float moveSpeed;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        moveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);
    }

    protected virtual void Update()
    {
        if (player == null) return;
        HandleMovement();
    }

    protected abstract void HandleMovement();

    protected void FacePlayer()
    {
        if (player == null) return;
        Vector3 scale = transform.localScale;
        scale.x = player.position.x > transform.position.x ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }
}
