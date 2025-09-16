using UnityEngine;
using UnityEngine.Events;

public class AmmoDrop : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private int[] ammoAmounts;
    [SerializeField] private UnityEvent onPickup;

    [Header("Icon Floating")]
    [SerializeField] private Transform iconTransform;
    [SerializeField] private float floatAmplitude = 0.25f;
    [SerializeField] private float floatFrequency = 2f;

    private Vector3 startIconPos;

    private void Update()
    {
        if (iconTransform != null)
        {
            float offset = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
            iconTransform.localPosition = startIconPos + new Vector3(0f, offset, 0f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent(out WeaponHandler weaponHandler))
            {
                int randomIndex = Random.Range(0, ammoAmounts.Length);
                weaponHandler.GetCurrentWeapon().AddAmmo(ammoAmounts[randomIndex]);
            }

            onPickup.Invoke();
            Destroy(gameObject);
        }
    }
}