using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private Transform healthBar;
    [SerializeField] private GameObject visual;
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private UnityEvent onTakeDamage;
    public UnityEvent<GameObject> OnDie;

    [Header("Hit Indicator Settings")]
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float blinkDuration = 0.1f;
    [SerializeField] private int blinkCount = 3;

    private float currentHealth;
    private bool isDead;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine indicatorRoutine;

    private void Awake()
    {
        currentHealth = maxHealth;
        if (visual != null)
        {
            spriteRenderer = visual.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
        }
    }

    private void Start()
    {
        if (healthBar != null)
        {
            SetHealthBar(currentHealth / maxHealth);
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        onTakeDamage.Invoke();
        Debug.Log($"{gameObject.name} took {amount} damage. Remaining: {currentHealth}");

        if (spriteRenderer != null)
        {
            if (indicatorRoutine != null) StopCoroutine(indicatorRoutine);
            indicatorRoutine = StartCoroutine(IndicatorRoutine());
        }

        if (healthBar != null)
        {
            SetHealthBar(currentHealth / maxHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator IndicatorRoutine()
    {
        for (int i = 0; i < blinkCount; i++)
        {
            spriteRenderer.color = hitColor;
            yield return new WaitForSeconds(blinkDuration);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(blinkDuration);
        }
    }

    private void SetHealthBar(float health)
    {
        healthBar.localScale = new Vector3(health, 1f, 1f);
    }

    public void Die()
    {
        Debug.Log($"{gameObject.name} died!");
        isDead = true;
        Instantiate(deathEffect, transform.position, Quaternion.identity);
        OnDie.Invoke(this.gameObject);
        Destroy(gameObject);
    }
}
