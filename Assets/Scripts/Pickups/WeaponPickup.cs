using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class WeaponPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private AssetReferenceGameObject weaponPrefab;
    [SerializeField] private bool autoPickup = false;
    [SerializeField] private KeyCode pickupButton = KeyCode.E;

    [Header("Icon Floating")]
    [SerializeField] private Transform iconTransform;
    [SerializeField] private float floatAmplitude = 0.25f;
    [SerializeField] private float floatFrequency = 2f;

    [Header("Event")]
    [SerializeField] private UnityEvent onPickup;

    private Vector3 startIconPos;
    private bool playerInRange = false;
    private WeaponHandler weaponHandler;

    private void Awake()
    {
        if (iconTransform == null)
            iconTransform = transform;
        startIconPos = iconTransform.localPosition;
    }

    private void Update()
    {
        if (iconTransform != null)
        {
            float offset = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
            iconTransform.localPosition = startIconPos + new Vector3(0f, offset, 0f);
        }

        if (!autoPickup && playerInRange && Input.GetKeyDown(pickupButton))
        {
            PickupWeapon();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            weaponHandler = other.GetComponent<WeaponHandler>();
            playerInRange = true;

            if (autoPickup)
                PickupWeapon();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            weaponHandler = null;
        }
    }

    private void PickupWeapon()
    {
        if (weaponHandler == null || weaponPrefab == null)
            return;

        onPickup.Invoke();
        weaponHandler.EquipWeapon(weaponPrefab);
        Destroy(gameObject);
    }
}
